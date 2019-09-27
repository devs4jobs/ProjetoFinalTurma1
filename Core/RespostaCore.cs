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

        public RespostaCore(ServiceContext ServiceContext, IMapper mapper) { _serviceContext = ServiceContext; _mapper = mapper; }

        public RespostaCore(ServiceContext ServiceContext) => _serviceContext = ServiceContext;

        public RespostaCore(Resposta RespostaQueVem, ServiceContext ServiceContext)
        {
            _serviceContext = ServiceContext;
            _resposta = RespostaQueVem;

            RuleFor(e => e.Mensagem).NotNull().MinimumLength(2).WithMessage("O tamanho da mensagem deve ser de no minimo 2 caracteres");
            RuleFor(e => e.TicketId).NotNull().WithMessage("O ticket Id não pode ser nulo!");
        }

        /// <summary>
        /// Método para o cadastro de respostas
        /// </summary>
        /// <param name="tokenAutor"></param>
        public async Task<Retorno> CadastrarResposta(string tokenAutor)
        {
            // o teste para a validacao do usuario
            if (!Autorizacao.ValidarUsuario(tokenAutor, _serviceContext))
                return new Retorno { Status = false, Resultado = new List<string> { "Autorização negada!" } };

            var validar = Validate(_resposta);
            if (!validar.IsValid)
                return new Retorno { Status = false, Resultado = validar.Errors.Select(a => a.ErrorMessage).ToList() };

            // vejo se o ticket é valido

            var Ticket = await _serviceContext.Tickets.SingleOrDefaultAsync(x => x.Id == _resposta.TicketId);
            if (Ticket == null)
                return new Retorno { Status = false, Resultado = new List<string> { "ticket não existe" } };

            if(Ticket.Status == Status.FECHADO)
                return new Retorno { Status = false, Resultado = new List<string> { "Não é possível responder um ticket fechado!" } };

            _resposta.UsuarioId = Guid.Parse(tokenAutor);

            if (Ticket.ClienteId != _resposta.UsuarioId && Ticket.AtendenteId != _resposta.UsuarioId)
                return new Retorno { Status = false, Resultado = new List<string> { "Usuário não está vinculado a esse ticket" } };

            // defino o status da resposta baseando se na pessoa que esta enviando 
            _resposta.Usuario = await _serviceContext.Usuarios.SingleOrDefaultAsync(x => x.Id == _resposta.UsuarioId);
            if (_resposta.Usuario.Tipo == "CLIENTE") Ticket.Status = Enum.Parse<Status>("AGUARDANDO_RESPOSTA_DO_ATENDENTE");
            else Ticket.Status = Enum.Parse<Status>("AGUARDANDO_RESPOSTA_DO_CLIENTE");

            //salvo e adciono no banco de dados
            await _serviceContext.AddAsync(_resposta);
            await _serviceContext.SaveChangesAsync();

            return new Retorno { Status = true, Resultado = new List<string> { "Reposta enviada com sucesso!" } };
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
                return new Retorno { Status = false, Resultado = new List<string> { "Autorização negada!" } };

            // verifico se o guid o ticket é valido
            if (!Guid.TryParse(RespostaId, out Guid result))
                return new Retorno { Status = false, Resultado = new List<string> { "Resposta inválida" } };

            // busco pela resposta e realiza as validações
            var umaResposta =  await _serviceContext.Respostas.SingleOrDefaultAsync(c => c.Id == result);

            if (umaResposta == null)
                return new Retorno { Status = false, Resultado = new List<string> { "Resposta inválida" } };

            if (umaResposta.UsuarioId != Guid.Parse(tokenAutor))
                return new Retorno { Status = false, Resultado = new List<string> { "Autorização para editar negada, só o autor da resposta pode edita-la" } };

            if (umaResposta.Mensagem.Length < 10)
                return new Retorno { Status = false, Resultado = new List<string> { "A mensagem deve ter no mínimo 10 caracteres para ser editada" } };

            _mapper.Map(respostaQueVem, umaResposta);

            await _serviceContext.SaveChangesAsync();

            return new Retorno { Status = true, Resultado = _mapper.Map<RespostaRetorno>(umaResposta) };
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
                return new Retorno { Status = false, Resultado = new List<string> { "Autorização negada!" } };

            // verifico se o guid o ticket é valido
            if (!Guid.TryParse(RespostaId, out Guid result))
                return new Retorno { Status = false, Resultado = new List<string> { "Resposta inválida" } };

            //procuro pela resposta em questao e aplico as validacoes
            var umaResposta = await _serviceContext.Respostas.SingleOrDefaultAsync(c => c.Id == result);
            if (umaResposta == null)
                return new Retorno { Status = false, Resultado = new List<string> { "Resposta inválida" } };

            if (umaResposta.UsuarioId != Guid.Parse(tokenAutor))
                return new Retorno { Status = false, Resultado = new List<string> { "Autorização para deletar negada, só o autor da resposta pode deletá-la" } };

            //salvo a remoção 
            _serviceContext.Respostas.Remove(umaResposta);
           await _serviceContext.SaveChangesAsync();

            return new Retorno { Status = true, Resultado = new List<string> { "Resposta deletada com sucesso!" } };
        }
    }
}
