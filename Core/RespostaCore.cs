using AutoMapper;
using Core.Util;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace Core
{
    public class RespostaCore : AbstractValidator<Resposta>
    {
        private Resposta _resposta { get; set; }
        private IMapper _mapper { get; set; }
        private ServiceContext _serviceContext { get; set; }

        private List<string> Extensão { get; set; } = new List<string> { "jpg", "png", "gif", "pdf", "txt", "doc", "docx" };

        #region Construtores
        public RespostaCore(ServiceContext ServiceContext, IMapper mapper) { _serviceContext = ServiceContext; _mapper = mapper; }
        public RespostaCore(ServiceContext ServiceContext) => _serviceContext = ServiceContext;

        public RespostaCore(Resposta RespostaQueVem, ServiceContext ServiceContext)
        {
            _serviceContext = ServiceContext;
            _resposta = RespostaQueVem;

            RuleSet("Cadastro", () =>
            {
                RuleFor(e => e.Mensagem).NotEmpty().WithMessage("A mensagem não pode ser enviada sem conteúdo.");
                RuleFor(e => e.Mensagem).NotNull().MinimumLength(2).WithMessage("O tamanho da mensagem deve ser de no minimo 2 caracteres");
                RuleFor(e => e.TicketId).NotNull().WithMessage("O ticketId nao pode ser nulo!");
            });

            RuleSet("CadastroAnexo", () =>
            {
                if (_resposta.Mensagem != null)
                {
                    RuleFor(e => e.Mensagem).NotEmpty().WithMessage("A mensagem não pode ser enviada sem conteúdo.");
                    RuleFor(e => e.Mensagem).MinimumLength(2).WithMessage("O tamanho da mensagem deve ser de no minimo 2 caracteres");
                }
                RuleFor(e => e.TicketId).NotNull().WithMessage("O ticketId nao pode ser nulo!");
                RuleFor(e => e.Anexo.NomeArquivo).NotNull().NotEmpty().WithMessage("Nome não pode ser nulo ou vazio");
                RuleFor(e => e.Anexo.NomeArquivo).Must(x => !Extensão.Any(c => c == x.Substring(x.IndexOf('.')))).WithMessage("Extensao não aceita");
                RuleFor(e => e.Anexo.Arquivo).NotNull().NotEmpty().WithMessage("Anexo não pode vir vazio");
                RuleFor(e => e.Anexo.Arquivo).Must(x => x.Length < 1024 * 1024 * 100).WithMessage("Arquivo muito grande tamanho maximo de arquivo 100MB");
            });

        }

        #endregion

        #region Regras de Negocio

        /// <summary>
        /// Método para o cadastro de respostas
        /// </summary>
        /// <param name="tokenAutor"></param>
        public async Task<Retorno> CadastrarResposta(string tokenAutor)
        {
            // o teste para a validacao do usuario
            if (!Autorizacao.ValidarUsuario(tokenAutor, _serviceContext))
                return new Retorno { Resultado = new List<string> { "Autorização negada!" } };

            try
            {
                _resposta.UsuarioId = Guid.Parse(tokenAutor);
                
                var validar = this.Validate(_resposta, ruleSet: _resposta.Anexo != null ? "CadastroAnexo" : "Cadastro");
                if (!validar.IsValid) return new Retorno { Resultado = validar.Errors.Select(a => a.ErrorMessage).ToList() };

                // vejo se o ticket é valido
                var Ticket = await _serviceContext.Tickets.SingleOrDefaultAsync(x => x.Id == _resposta.TicketId);

                if (Ticket.Status == Status.FECHADO) return new Retorno { Resultado = new List<string> { "Não é possível responder um ticket fechado!" } };

                if (Ticket.ClienteId != _resposta.UsuarioId && Ticket.AtendenteId != _resposta.UsuarioId) return new Retorno { Resultado = new List<string> { "Usuário não está vinculado a esse ticket" } };

                // defino o status da resposta baseando se na pessoa que esta enviando 
                _resposta.Usuario = await _serviceContext.Usuarios.SingleOrDefaultAsync(x => x.Id == _resposta.UsuarioId);
                Ticket.Status = _resposta.Usuario.Tipo == "CLIENTE" ? Status.AGUARDANDO_RESPOSTA_DO_ATENDENTE : Status.AGUARDANDO_RESPOSTA_DO_CLIENTE;

                //salvo e adciono no banco de dados
                await _serviceContext.AddAsync(_resposta);
                await _serviceContext.SaveChangesAsync();

                return new Retorno { Status = true, Resultado = new List<string> { "Reposta enviada com sucesso!" } };
            }
            catch (NullReferenceException)
            {
                return new Retorno { Resultado = new List<string> { "Ticket não existe" } };
            }
        }
    
        /// <summary>
        ///  Método para realizar a edição das respostas
        /// </summary>
        /// <param name="tokenAutor"></param>
        /// <param name="RespostaId"></param>
        /// <param name="respostaQueVem"></param>
        public async Task<Retorno> EditarResposta(string tokenAutor, string RespostaId, Resposta respostaQueVem)
        {
            // realizo as validacoes  do usuario e em seguida do ticket
            if (!Autorizacao.ValidarUsuario(tokenAutor, _serviceContext))
                return new Retorno { Resultado = new List<string> { "Autorização negada!" } };

            try
            {
                // busco pela resposta e realiza as validações
                _resposta = await _serviceContext.Respostas.SingleOrDefaultAsync(c => c.Id == Guid.Parse(RespostaId));

                if (_resposta.UsuarioId != Guid.Parse(tokenAutor))  return new Retorno { Resultado = new List<string> { "Autorização para editar negada, só o autor da resposta pode edita-la" } };

                if (respostaQueVem.Mensagem.Length < 10)  return new Retorno { Resultado = new List<string> { "A mensagem deve ter no mínimo 10 caracteres para ser editada" } };

                _mapper.Map(respostaQueVem, _resposta);
                await _serviceContext.SaveChangesAsync();

                return new Retorno { Status = true, Resultado = _mapper.Map<RespostaRetorno>(_resposta) };
            }
            catch (FormatException)
            {
                return new Retorno { Resultado = new List<string> { "RespostaId Formato incorreta" } };
            }
            catch (NullReferenceException)
            {
                return new Retorno { Resultado = new List<string> { "Resposta não existe" } };
            }
        }

        /// <summary>
        /// Método para deletar uma resposta
        /// </summary>
        /// <param name="tokenAutor"></param>
        /// <param name="RespostaId"></param>
        public async Task<Retorno> DeletarResposta(string tokenAutor, string RespostaId)
        {
            // realizo as validacoes  do usuario e em seguida do ticket
            if (!Autorizacao.ValidarUsuario(tokenAutor, _serviceContext))
                return new Retorno { Resultado = new List<string> { "Autorização negada!" } };

            try
            {
                //procuro pela resposta em questao e aplico as validacoes
                _resposta = await _serviceContext.Respostas.SingleOrDefaultAsync(c => c.Id == Guid.Parse(RespostaId));

                if (_resposta.UsuarioId != Guid.Parse(tokenAutor))  return new Retorno { Resultado = new List<string> { "Autorização para deletar negada, só o autor da resposta pode deletá-la" } };

                //salvo a remoção 
                _resposta.VisualizarMensagem = false;
                await _serviceContext.SaveChangesAsync();

                return new Retorno { Status = true, Resultado = new List<string> { "Resposta deletada com sucesso!" } };
            }
            catch (FormatException)
            {
                return new Retorno { Resultado = new List<string> { "RespostaId Formato incorreto" } };
            }
            catch (NullReferenceException)
            {
                return new Retorno { Resultado = new List<string> { "Resposta não existe " } };
            }
        }
        #endregion
    }
}
