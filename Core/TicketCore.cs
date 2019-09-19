﻿using Model;
using FluentValidation;
using System.Linq;
using Core.Util;
using System;
using System.Collections.Generic;
using AutoMapper;
using System.Globalization;

namespace Core
{
    /// <summary>
    /// Classe  Ticket Core aonde se encontra minha regra de negocios referente a tickets.
    /// </summary>
    public class TicketCore : AbstractValidator<Ticket>
    {
        private IMapper _mapper { get; set; }
        private Ticket _ticket { get; set; }
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
            if (!Autorizacao.GuidValidation(Usertoken)) 
                 return new Retorno {Status = false, Resultado = new List<string>{"Autorização Negada!"} } ;
            
            //verifico ticket se é valido.
            var validar = Validate(_ticket);
            if (!validar.IsValid) 
                 return new Retorno {Status = false, Resultado = validar.Errors.Select(e => e.ErrorMessage).ToList() } ;
              _ticket.NumeroTicket = ConvertNumeroTickets();

            _ticket.ClienteId = Guid.Parse(Usertoken);
            //busco o cliente na base e verifico.
            var cliente = _serviceContext.Usuarios.FirstOrDefault(u => u.Id == _ticket.ClienteId);

            if (cliente == null) return new Retorno { Status = false, Resultado = new List<string> { "Cliente não identificado!" } };
            if (cliente.Tipo != "CLIENTE") return new Retorno { Status = false, Resultado = new List<string> { "Usuario não é do tipo cliente" } };

            _ticket.NumeroTicket = ConvertNumeroTickets();
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

        public Retorno TomarPosseTicket(string Usertoken, string TicketID)
        {
            //verifico login.
            if (!Autorizacao.ValidarUsuario(Usertoken, _serviceContext))
                return new Retorno { Status = false, Resultado = new List<string> { "Autorização Negada!" } };

            //verifico se o Ticket ID é valido.
            if (!Autorizacao.GuidValidation(TicketID))
                return new Retorno { Status = false, Resultado = new List<string> { "Ticket não identificado!" } };

            //verifico se tem um usuário na base com ID informado e o tipo dele é atendente.
            var atendente = _serviceContext.Usuarios.FirstOrDefault(u => u.Id == Guid.Parse(Usertoken) && u.Tipo == "ATENDENTE");
            if (atendente == null) return new Retorno { Status = false, Resultado = new List<string> { "Atendente não identificado!" } };

            //verifico se o ticket solicitado existe na base de dados.
            var TicketSolicitado = _serviceContext.Tickets.FirstOrDefault(t => t.Id == Guid.Parse(TicketID));
            if (TicketSolicitado.Atendente == null && TicketSolicitado.AtendenteId == null) return new Retorno {Status = false , Resultado = new List<string> { "Ticket já tem um atendente." } };

            //passo os valores para o ticket
            TicketSolicitado.Atendente = atendente;
            TicketSolicitado.AtendenteId = atendente.Id;

            _serviceContext.SaveChanges();
            return new Retorno { Status = true, Resultado = new List<string> { $"{atendente.Nome} você atribuiu esse Ticket a sua base." } };

        }

         public string ConvertNumeroTickets()
        {
            var random = new Random();
            var dataString = DateTime.Now.ToString("MMyyyy", CultureInfo.CreateSpecificCulture("pt-BR")); ;
            var number = 7 * random.Next(1000, 9999) / 100;

            if (number == 0)
                number = 6 * random.Next(1000, 9999) / 100;


            return dataString + number.ToString("D");
        }
    }
}
