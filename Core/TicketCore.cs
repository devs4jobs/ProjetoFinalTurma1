using Model;
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
        private Ticket _ticket;
        private ServiceContext _serviceContext { get; set; }

        #region Construtores
        public TicketCore(ServiceContext serviceContext) => _serviceContext = serviceContext;
        public TicketCore(IMapper mapper, ServiceContext serviceContext)
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
                return new Retorno { Status = false, Resultado = new List<string> { "Autorização Negada!" } };
            //verifico ticket se é valido.
            var validar = Validate(_ticket);
            if (!validar.IsValid)
                return new Retorno { Status = false, Resultado = validar.Errors.Select(e => e.ErrorMessage).ToList() };

            //busco o cliente na base e verifico.
            var cliente = _serviceContext.Usuarios.FirstOrDefault(u => u.Id == _ticket.ClienteId);
            if (cliente == null) return new Retorno { Status = false, Resultado = new List<string> { "Cliente não identificado!" } };

            //vejo se o cliente que ta longado é o mesmo que está públicando o ticket.
            if (cliente.Id != Guid.Parse(Usertoken)) return new Retorno { Status = false, Resultado = new List<string> { "Autorização Negada!" } };

            //add o ticket e salvo alterações.
            _serviceContext.Tickets.Add(_ticket);
            _serviceContext.SaveChanges();

            return new Retorno { Status = true, Resultado = new List<string> { $"{cliente.Nome} seu Ticket foi cadastrado com Sucesso!" } };

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
            if (!Autorizacao.GuidValidation(TicketID))
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

            return TicketSolicitado != null ? new Retorno { Status = true, Resultado = TicketSolicitado } : new Retorno { Status = false, Resultado = new List<string> { "Ticket não identificado!" } };
        }

        public Retorno BuscarTodosTickets(string Usertoken, int NumeroPagina, int QuantidadeRegistro)
        {
            //verifico login.
            if (!Autorizacao.GuidValidation(Usertoken))
                return new Retorno { Status = false, Resultado = new List<string> { "Autorização Negada!" } };

            //busco pelo usuario e vejo se ele existe.
            var usuario = _serviceContext.Usuarios.FirstOrDefault(u => u.Id == Guid.Parse(Usertoken));
            if (usuario == null)
                return new Retorno { Status = false, Resultado = new List<string> { "Cliente não identificado!" } };

            // nova instancia da paganicação
            var Paginacao = new Paginacao();

            //Confiro o tipo do usuario e exibo os resultados paginados de acordo com o tipo do usuario
            if (usuario.Tipo.ToUpper() == "ATENDENTE")
            {
                // busco pelos tickets daquele especifico usuario 
                var ticketsAtendente = _serviceContext.Tickets.Where(t => t.Status == Enum.Parse<Status>("ABERTO") && t.AtendenteId == Guid.Parse(Usertoken)).ToList();

                // caso for possivel realizar a paginação se nao for exibo a quantidade padrão = 10
                if (NumeroPagina > 0 && QuantidadeRegistro > 0)
                {
                    Paginacao.Paginar(NumeroPagina, QuantidadeRegistro, ticketsAtendente.Count());
                    return new Retorno { Status = true, Paginacao = Paginacao, Resultado = ticketsAtendente.OrderBy(d => d.DataCadastro).Skip((NumeroPagina - 1) * QuantidadeRegistro).Take(QuantidadeRegistro) };
                }

                Paginacao.Paginar(1, 10, ticketsAtendente.Count());

                return new Retorno { Status = true, Paginacao = Paginacao, Resultado = ticketsAtendente.Take(10) };
            }
            // busco pelos tickets daquele especifico usuario 
            var ticketsCliente = _serviceContext.Tickets.Where(c => c.ClienteId == Guid.Parse(Usertoken) && c.Status == Enum.Parse<Status>("ABERTO")).ToList();

            // caso for possivel realizar a paginação se nao for exibo a quantidade padrão = 10
            if (NumeroPagina > 0 && QuantidadeRegistro > 0)
            {
                Paginacao.Paginar(NumeroPagina, QuantidadeRegistro, ticketsCliente.Count());
                return new Retorno { Status = true, Paginacao = Paginacao, Resultado = ticketsCliente.OrderBy(d => d.DataCadastro).Skip((NumeroPagina - 1) * QuantidadeRegistro).Take(QuantidadeRegistro) };
            }
            Paginacao.Paginar(1, 10, ticketsCliente.Count());

            return new Retorno { Status = true, Paginacao = Paginacao, Resultado = ticketsCliente.Take(10) };
        }

        //metodo para fazer uma identificação unica de cada usuário.
        public string ConvertNumeroTickets()
        {
            var dataString = DateTime.Now.ToString("MMyyyy", CultureInfo.CreateSpecificCulture("pt-BR")); ;
            //aqui eu faço um calculo com números aleatórios. 

            var number = 7 * new Random().Next(1000, 9999) / 100;
    
            //aqui retornamos o dia e o ano junto com o resultado dos calculos.
            return dataString + number.ToString("D");
        }
    }
}
