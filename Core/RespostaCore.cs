using Core.Util;
using FluentValidation;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core
{
    public class RespostaCore : AbstractValidator<Resposta>
    {
        private Resposta _resposta;
        public ServiceContext _serviceContext { get; set; }

        public RespostaCore(ServiceContext ServiceContext) => _serviceContext = ServiceContext;

        public RespostaCore(Resposta Resposta, ServiceContext ServiceContext)
        {
            _serviceContext = ServiceContext;
            _resposta = Resposta;

            RuleFor(e => e.Mensagem).MinimumLength(10).WithMessage("O tamanho da mensagem deve ser de no minimo 10 caracteres");
            RuleFor(e => e.TicketId).NotNull().WithMessage("O ticketId nao pode ser nulo!");
            RuleFor(e => e.UsuarioId).NotNull().WithMessage("o Usuario Id nao pode ser nulo!");
        }

        public Retorno CadastrarResposta(string tokenAutor)
        {
            if (!Autorizacao.ValidarUsuario(tokenAutor, _serviceContext))
                return new Retorno { Status = false, Resultado = new List<string> { "Autorização negada!" } };

            var valida = Validate(_resposta);

            if (!valida.IsValid)
                return new Retorno { Status = false, Resultado = valida.Errors.Select(a => a.ErrorMessage).ToList() };

            return new Retorno { Status = true, Resultado = new List<string> { "Resposta enviada!" } };
        }

        public Retorno BuscarRespostas(string tokenAutor, string id)
        {
            if (!Autorizacao.ValidarUsuario(tokenAutor, _serviceContext))
                return new Retorno { Status = false, Resultado = new List<string> { "Autorização negada!" } };

            if (!Guid.TryParse(id, out Guid ident))
                return new Retorno { Status = false, Resultado = new List<string> { "Id do ticket inválido" } };

            var todasRespostas = _serviceContext.Respostas.Where(r => r.Id == ident).ToList();

            return todasRespostas.Count() == 0 ? new Retorno { Status = false, Resultado = new List<string> { "Não há respostas nesse ticket" } } : new Retorno { Status = true, Resultado = todasRespostas };
        }

        public Retorno EditarResposta(string tokenAutor, string ticketId, Resposta resposta)
        {
            if (!Autorizacao.ValidarUsuario(tokenAutor, _serviceContext))
                return new Retorno { Status = false, Resultado = new List<string> { "Autorização negada!" } };

            var umaResposta = _serviceContext.Respostas.FirstOrDefault(c => c.TicketId == Guid.Parse(ticketId));

            if (umaResposta.UsuarioId != Guid.Parse(tokenAutor))
                return new Retorno { Status = false, Resultado = new List<string> { "Autorização para editar negada, só o autor da resposta pode edita-la" } };

            try
            {
                if (resposta.Mensagem.Length < 10)
                    return new Retorno { Status = false, Resultado = new List<string> { "A mensagem deve ter no mínimo 10 caracteres para ser editada" } };
            }
            catch (Exception) { return new Retorno { Status = false, Resultado = new List<string> { "A resposta não pode ser nula!" } }; }
           
            if (resposta.Mensagem != null)
                umaResposta.Mensagem = resposta.Mensagem;

            return new Retorno { Status = true, Resultado = new List<string> { "Resposta editada com sucesso!" } };
        }

        public Retorno DeletarResposta(string tokenAutor, string ticketId)
        {
            if (!Autorizacao.ValidarUsuario(tokenAutor, _serviceContext))
                return new Retorno { Status = false, Resultado = new List<string> { "Autorização negada!" } };

            var umaResposta = _serviceContext.Respostas.FirstOrDefault(c => c.TicketId == Guid.Parse(ticketId));

            if (umaResposta.UsuarioId != Guid.Parse(tokenAutor))
                return new Retorno { Status = false, Resultado = new List<string> { "Autorização para deletar negada, só o autor da resposta pode deleta-la" } };

            _serviceContext.Respostas.Remove(umaResposta);
            _serviceContext.SaveChanges();

            return new Retorno { Status = true, Resultado = new List<string> { "Resposta deletada com sucesso!" } };
        }
    }
}
