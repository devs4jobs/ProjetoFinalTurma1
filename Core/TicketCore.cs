using Model;
using FluentValidation;
using System.Linq;
using Core.Util;
using System;
using System.Collections.Generic;
using AutoMapper;

namespace Core
{
    /// <summary>
    /// Classe  Ticket Core aonde se encontra minha regra de negocios referente a tickets.
    /// </summary>
    public class TicketCore : AbstractValidator<Ticket>
    {
        private IMapper _mapper { get; set; }
        private Ticket _ticket;
        private ServiceContext _serviceContext { get; set; }

        #region Construtores
        public TicketCore(ServiceContext serviceContext) => _serviceContext = serviceContext;
        public TicketCore(IMapper mapper, ServiceContext serviceContext )
        {
            _mapper = mapper;
            _serviceContext = serviceContext;
        }


        public TicketCore(Ticket ticket, ServiceContext serviceContext)
        {
            _ticket = ticket;
            _serviceContext = serviceContext;

            RuleFor(t => t.ClienteId).NotNull()
                .WithMessage("O ID do cliente não pode ser nulo.");

            RuleFor(t => t.Titulo).NotNull()
                .WithMessage("O título do ticket não pode ser nulo.");

            RuleFor(t => t.NumeroTicket).Null()
                .WithMessage("Número do Ticket será nulo quando criamos ele elaboramos uma identificação unica.");

            RuleFor(t => t.Status).IsInEnum();

            RuleFor(t => t.Avaliacao).IsInEnum();
        }
        #endregion

        public Retorno CadastrarTicket(string Usertoken)
        {
            //verifico login.
            if (!Autorizacao.ValidarUsuario(Usertoken, _serviceContext)) 
                 return new Retorno {Status = false, Resultado = new List<string>{"Autorização Negada!"} } ;
            //verifico ticket se é valido.
            var validar = Validate(_ticket);
            if (!validar.IsValid) 
                 return new Retorno {Status = false, Resultado = validar.Errors.Select(e => e.ErrorMessage).ToList() } ;

            //busco o cliente na base e verifico.
            var cliente = _serviceContext.Usuarios.FirstOrDefault(u => u.Id == _ticket.ClienteId);
            if (cliente == null)  return new Retorno {Status = false, Resultado = new List<string>{"Cliente não identificado!"}} ;

            //vejo se o cliente que ta longado é o mesmo que está públicando o ticket.
            if (cliente.Id != Guid.Parse(Usertoken))  return new Retorno {Status = false, Resultado = new List<string>{"Autorização Negada!"} } ;

            //add o ticket e salvo alterações.
            _serviceContext.Tickets.Add(_ticket);
            _serviceContext.SaveChanges();

           return new Retorno { Status = true, Resultado = new List<string>{$"{cliente.Nome} seu Ticket foi cadastrado com Sucesso!"} };

        }
        public Retorno AtualizarTicket(string Usertoken, string TicketID, Ticket ticketView)
        {
            //verifico login.
            if (!Autorizacao.ValidarUsuario(Usertoken, _serviceContext))
                return new Retorno { Status = false, Resultado = new List<string> { "Autorização Negada!" } };

            //verifico se o Ticket ID é valido.
            if (!Autorizacao.GuidValidation(TicketID))
                return new Retorno { Status = false, Resultado = new List<string> { "Ticket não identificado!" } };

            var cliente = _serviceContext.Usuarios.FirstOrDefault(u => u.Id == Guid.Parse(Usertoken));
            if (cliente == null) return new Retorno { Status = false, Resultado = new List<string> { "Cliente não identificado!" } };

            var ticketSelecionado = _serviceContext.Tickets.FirstOrDefault(t => t.Id == Guid.Parse(TicketID));

            //vejo se o cliente que ta longado é o mesmo que está Atualizando o ticket.
            if (ticketSelecionado.Id != Guid.Parse(Usertoken)) return new Retorno { Status = false, Resultado = new List<string> { "Autorização Negada!" } };

            _mapper.Map(ticketView, ticketSelecionado);
            _serviceContext.SaveChanges();
            return new Retorno { Status = true, Resultado = ticketSelecionado };

        }
        public Retorno DeletarTicket(string Usertoken, string TicketID)
        {
            //verifico login.
           if (!Autorizacao.ValidarUsuario(Usertoken, _serviceContext))
               return new Retorno { Status = false, Resultado = new List<string> { "Autorização Negada!" } };

           //verifico se o Ticket ID é valido.
           if(!Autorizacao.GuidValidation(TicketID))
                return new Retorno { Status = false, Resultado = new List<string> { "Ticket não identificado!" } };

            var cliente = _serviceContext.Usuarios.FirstOrDefault(u => u.Id == Guid.Parse(Usertoken));
            if (cliente == null) return new Retorno { Status = false, Resultado = new List<string> { "Cliente não identificado!" } };

            //vejo se o cliente que ta longado é o mesmo que está públicando o ticket.
            if (cliente.Id != Guid.Parse(Usertoken)) return new Retorno { Status = false, Resultado = new List<string> { "Autorização Negada!" } };

            //excluo o ticket e salvo alterações.
            _serviceContext.Tickets.Remove(_serviceContext.Tickets.FirstOrDefault(t => t.Id == Guid.Parse(TicketID)));
            _serviceContext.SaveChanges();

            return new Retorno { Status = true, Resultado = new List<string> { $"{cliente.Nome} seu Ticket foi Deletado com Sucesso!" } };
        }
        public Retorno BuscarTicketporID(string Usertoken, string TicketID)
        {
            //verifico login.
            if (!Autorizacao.ValidarUsuario(Usertoken, _serviceContext))
                return new Retorno { Status = false, Resultado = new List<string> { "Autorização Negada!" } };

            //verifico se o Ticket ID é valido.
            if (!Autorizacao.GuidValidation(TicketID))
                return new Retorno { Status = false, Resultado = new List<string> { "Ticket não identificado!" } };

            var cliente = _serviceContext.Usuarios.FirstOrDefault(u => u.Id == Guid.Parse(Usertoken));
            if (cliente == null) return new Retorno { Status = false, Resultado = new List<string> { "Cliente não identificado!" } };

            var TicketSolicitado = _serviceContext.Tickets.FirstOrDefault(t => t.Id == Guid.Parse(TicketID));
            //vejo se o cliente que ta longado é o mesmo que está públicando o ticket.
            if (cliente.Id != TicketSolicitado.ClienteId) return new Retorno { Status = false, Resultado = new List<string> { "Autorização Negada!" } };

           
            return TicketSolicitado != null? new Retorno {Status = true , Resultado = TicketSolicitado } : new Retorno { Status = false, Resultado = new List<string> { "Ticket não identificado!" } };

        }

        public Retorno BuscarTodosTickets(string Usertoken)
        {
            //verifico login.
            if (!Autorizacao.GuidValidation(Usertoken)) 
            return new Retorno { Status = false, Resultado = new List<string> { "Autorização Negada!" } };

            var cliente = _serviceContext.Usuarios.FirstOrDefault(u => u.Id == Guid.Parse(Usertoken));
            if (cliente == null) 
             return new Retorno { Status = false, Resultado = new List<string> { "Cliente não identificado!" } };

            var todos = _serviceContext.Tickets.ToList();

            //se o cliente for atendente ele tem acesso total
            if (cliente.Tipo.ToUpper() == "ATENDENTE") 
                return todos.Any() ? 
                    new Retorno { Status = true, Resultado = todos } 
                    : new Retorno { Status = false, Resultado = new List<string> {"Não existe nenhum ticket na base de dados."} };
           
            //se for um cliente ele só pode ver os tickets dele.
            return todos.Where(t => t.ClienteId == cliente.Id).ToList().Count() > 0 ?
                new Retorno { Status = true, Resultado = todos.Where(t => t.ClienteId == cliente.Id).ToList() } 
                : new Retorno { Status = false, Resultado = new List<string> { $"{cliente.Nome} você não fez nenhum ticket." } };
        }
    }
}
