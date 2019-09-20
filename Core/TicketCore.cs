using Model;
using FluentValidation;
using System.Linq;
using Core.Util;
using System;
using System.Collections.Generic;
using AutoMapper;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

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
        public TicketCore(IMapper mapper, ServiceContext serviceContext)
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

        public async Task<Retorno> CadastrarTicket(string Usertoken)
        {
            //verifico login.
            if (!Autorizacao.ValidarUsuario(Usertoken,_serviceContext))
                return new Retorno { Status = false, Resultado = new List<string> { "Autorização Negada! ,Usuario não existe" } };

            //verifico ticket se é valido.
            var validar = Validate(_ticket);
            if (!validar.IsValid)
                return new Retorno { Status = false, Resultado = validar.Errors.Select(e => e.ErrorMessage).ToList() };

            _ticket.NumeroTicket = ConvertNumeroTickets();

            _ticket.ClienteId = Guid.Parse(Usertoken);
            //busco o cliente na base e verifico.
            var cliente = _serviceContext.Usuarios.FirstOrDefault(u => u.Id == _ticket.ClienteId);

            if (cliente.Tipo != "CLIENTE") return new Retorno { Status = false, Resultado = new List<string> { "Usuario não é do tipo cliente" } };
            
            //add o ticket e salvo alterações.
            _serviceContext.Tickets.Add(_ticket);
            await _serviceContext.SaveChangesAsync();

            return new Retorno { Status = true, Resultado = new List<string> { $"{cliente.Nome} seu Ticket foi cadastrado com Sucesso!" } };
        }
        public Retorno AtualizarTicket(string Usertoken, string TicketID, Ticket ticketView)
        {
            //verifico login.
            if (!Autorizacao.ValidarUsuario(Usertoken, _serviceContext))
                return new Retorno { Status = false, Resultado = new List<string> { "Autorização Negada!" } };

            //verifico se o Ticket ID é valido.
            if (!Guid.TryParse(TicketID ,out Guid tId))
                return new Retorno { Status = false, Resultado = new List<string> { "Ticket não identificado!" } };
         
            var ticketSelecionado = _serviceContext.Tickets.FirstOrDefault(t => t.Id == tId);

            //vejo se o cliente que ta longado é o mesmo que está Atualizando o ticket.
            if (ticketSelecionado.ClienteId != Guid.Parse(Usertoken)) return new Retorno { Status = false, Resultado = new List<string> { "Usuario não é o mesmo que postou o ticket!" } };

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
            if (!Guid.TryParse(TicketID, out Guid tId))
                return new Retorno { Status = false, Resultado = new List<string> { "Ticket não identificado!" } };

            _ticket = _serviceContext.Tickets.Include(c=>c.Cliente).FirstOrDefault(t => t.Id == tId);

            //vejo se o cliente que ta longado é o mesmo que está públicou o ticket.
            if (Guid.Parse(Usertoken) != _ticket.ClienteId) return new Retorno { Status = false, Resultado = new List<string> { "Usuario não pode deletar esse ticket, pois não é quem postou o mesmo!" } };

            //excluo o ticket e salvo alterações.
            _serviceContext.Tickets.Remove(_ticket);
            _serviceContext.SaveChanges();

            return new Retorno { Status = true, Resultado = new List<string> { $"{_ticket.Cliente.Nome} seu Ticket foi Deletado com Sucesso!" } };
        }
        public Retorno BuscarTicketporID(string Usertoken, string TicketID)
        {
            //verifico login.
            if (!Autorizacao.ValidarUsuario(Usertoken, _serviceContext))
                return new Retorno { Status = false, Resultado = new List<string> { "Autorização Negada!" } };

            //verifico se o Ticket ID é valido.
            if (!Guid.TryParse(TicketID, out Guid tId))
                return new Retorno { Status = false, Resultado = new List<string> { "Ticket não identificado!" } };

            var cliente = _serviceContext.Usuarios.FirstOrDefault(u => u.Id == Guid.Parse(Usertoken));
            if (cliente == null) return new Retorno { Status = false, Resultado = new List<string> { "Cliente não identificado!" } };

            //vejo se o cliente que ta longado é o mesmo que está públicando o ticket.
            var TicketSolicitado = _serviceContext.Tickets.FirstOrDefault(t => t.Id == Guid.Parse(TicketID) && t.ClienteId == cliente.Id );
            
            return TicketSolicitado != null ? new Retorno { Status = true, Resultado = TicketSolicitado } : new Retorno { Status = false, Resultado = new List<string> { "Ticket não identificado!" } };
        }
        public Retorno BuscarTodosTickets(string Usertoken, int NumeroPagina, int QuantidadeRegistro)
        {
            //verifico login.
            if (!Autorizacao.ValidarUsuario(Usertoken,_serviceContext))
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

                var ticketsAtendente = _serviceContext.Tickets.Where(t => t.Status != Enum.Parse<Status>("FECHADO") && t.AtendenteId == Guid.Parse(Usertoken)).ToList();

                ticketsAtendente.ForEach(t => VerificaData(t));
                _serviceContext.SaveChanges();


                // caso for possivel realizar a paginação se nao for exibo a quantidade padrão = 10, e ordeno pelo mais antigo
                if (NumeroPagina > 0 && QuantidadeRegistro > 0)
                {
                    Paginacao.Paginar(NumeroPagina, QuantidadeRegistro, ticketsAtendente.Count());
                    var listaPaginada = ticketsAtendente.OrderByDescending(d => d.DataCadastro).Skip((NumeroPagina - 1) * QuantidadeRegistro).Take(QuantidadeRegistro).ToList();
                    return listaPaginada.Count() == 0 ? new Retorno { Status = true, Paginacao = Paginacao, Resultado = ticketsAtendente.Take(10) } : new Retorno { Status = true, Paginacao = Paginacao, Resultado = ticketsAtendente };
                }

                Paginacao.Paginar(1, 10, ticketsAtendente.Count());

                return new Retorno { Status = true, Paginacao = Paginacao, Resultado = ticketsAtendente.Take(10) };
            }
            // busco pelos tickets daquele especifico usuario 

            var ticketsCliente = _serviceContext.Tickets.Where(c => c.ClienteId == Guid.Parse(Usertoken) && c.Status == Enum.Parse<Status>("ABERTO")).ToList();
            ticketsCliente.ForEach(r => VerificaData(r));
            _serviceContext.SaveChanges();

            // caso for possivel realizar a paginação se nao for exibo a quantidade padrão = 10
            if (NumeroPagina > 0 && QuantidadeRegistro > 0)
            {
                Paginacao.Paginar(NumeroPagina, QuantidadeRegistro, ticketsCliente.Count());
                var listaPaginada = ticketsCliente.OrderByDescending(d => d.DataCadastro).Skip((NumeroPagina - 1) * QuantidadeRegistro).Take(QuantidadeRegistro).ToList();
                return listaPaginada.Count() == 0 ? new Retorno { Status = true, Paginacao = Paginacao, Resultado = ticketsCliente.Take(10) } : new Retorno { Status = true, Paginacao = Paginacao, Resultado = ticketsCliente };
            }
            Paginacao.Paginar(1, 10, ticketsCliente.Count());

            return new Retorno { Status = true, Paginacao = Paginacao, Resultado = ticketsCliente.Take(10) };
        }
        public Retorno TomarPosseTicket(string Usertoken, string TicketID)
        {
            //verifico login.
            if (!Autorizacao.ValidarUsuario(Usertoken, _serviceContext))
                return new Retorno { Status = false, Resultado = new List<string> { "Autorização Negada!" } };

            //verifico se o Ticket ID é valido.
            if (!Guid.TryParse(TicketID, out Guid tId))
                return new Retorno { Status = false, Resultado = new List<string> { "Ticket não identificado!" } };

            //verifico se tem um usuário na base com ID informado e o tipo dele é atendente.
            var atendente = _serviceContext.Usuarios.FirstOrDefault(u => u.Id == Guid.Parse(Usertoken) && u.Tipo == "ATENDENTE");
            if (atendente == null) return new Retorno { Status = false, Resultado = new List<string> { "Atendente não identificado!" } };

            //verifico se o ticket solicitado existe na base de dados.
            var TicketSolicitado = _serviceContext.Tickets.FirstOrDefault(t => t.Id == tId);
            if (TicketSolicitado.AtendenteId != null) return new Retorno { Status = false, Resultado = new List<string> { "Ticket já tem um atendente." } };

            //passo o valor para o ticket
            TicketSolicitado.AtendenteId = atendente.Id;

            _serviceContext.SaveChanges();
            return new Retorno { Status = true, Resultado = new List<string> { $"{atendente.Nome} você atribuiu esse Ticket a sua base." } };
        }

        // Método para buscar os tickets disponiveis para o atendente
        public Retorno BuscarTicketSemAtendente(string Usertoken, int NumeroPagina, int QuantidadeRegistro)
        {
            //verifico login.
            if (!Autorizacao.ValidarUsuario(Usertoken,_serviceContext))
                return new Retorno { Status = false, Resultado = new List<string> { "Autorização Negada!" } };

            var todosTickets = _serviceContext.Tickets.Where(c => c.AtendenteId == null && c.Status  != Enum.Parse<Status>("FECHADO"));

            // nova instancia da paganicação
            var Paginacao = new Paginacao();

            // caso for possivel realizar a paginação se nao for exibo a quantidade padrão = 10
            if (NumeroPagina > 0 && QuantidadeRegistro > 0)
            {
                Paginacao.Paginar(NumeroPagina, QuantidadeRegistro, todosTickets.Count());
                return new Retorno { Status = true, Paginacao = Paginacao, Resultado = todosTickets.OrderBy(d => d.DataCadastro).Skip((NumeroPagina - 1) * QuantidadeRegistro).Take(QuantidadeRegistro) };
            }

            Paginacao.Paginar(1, 10, todosTickets.Count());

            return new Retorno { Status = true, Paginacao = Paginacao, Resultado = todosTickets.Take(10) };
        }

        public Retorno AvaliarTicket(string tokenAutor, string ticketId, string avaliacao)
        {
            //verifico login.
            if (!Autorizacao.ValidarUsuario(tokenAutor, _serviceContext))
                return new Retorno { Status = false, Resultado = new List<string> { "Autorização Negada!" } };

            //verifico se o Ticket ID é valido.
            if (!Guid.TryParse(ticketId, out Guid tId))
                return new Retorno { Status = false, Resultado = new List<string> { "Ticket não identificado!" } };

            // vejo se a avaliacao é valida
            if (!Enum.TryParse(avaliacao, out Avaliacao result))
                return new Retorno { Status = false, Resultado = new List<string> { "Avaliação nao válida!" } };

            // busco pelo ticket e faço a validação de o ticket precisar estar fechado
            var Oticket = _serviceContext.Tickets.FirstOrDefault(c => c.Id == Guid.Parse(ticketId) && c.ClienteId == Guid.Parse(tokenAutor));

            if (Oticket.Status == Enum.Parse<Status>("ABERTO"))
                return new Retorno { Status = false, Resultado = new List<string> { "O ticket precisa estar fechado para ocorrer a avaliação" } };

            Oticket.Avaliacao = result;

            return new Retorno { Status = true, Resultado = new List<string> { "Avaliação registrada com sucesso!" } };
        }

        // metódo para realizar o fechamento do ticket
        public Retorno FecharTicket(string tokenAutor, string ticketId)
        {
            //verifico login.
            if (!Autorizacao.ValidarUsuario(tokenAutor, _serviceContext))
                return new Retorno { Status = false, Resultado = new List<string> { "Autorização Negada!" } };

            // verifico se o guid o ticket é valido
            if (!Guid.TryParse(ticketId, out Guid result))
                return new Retorno { Status = false, Resultado = new List<string> { "Ticket inválido" } };

            // busco e valido se este ticket em especifico é valido.
            var oTicket = _serviceContext.Tickets.FirstOrDefault(c => c.Id == result && c.AtendenteId == Guid.Parse(tokenAutor));

            if (oTicket == null)
                return new Retorno { Status = false, Resultado = new List<string> { "Ticket inválido" } };

            // atribuo e fecho o ticket
            oTicket.Status = Enum.Parse<Status>("FECHADO");

            return new Retorno { Status = true, Resultado = new List<string> { "Ticket fechado com sucesso!" } };
        }

        //metodo para fazer uma identificação unica de cada usuário.
        public string ConvertNumeroTickets()
        {
            var dataString = DateTime.Now.ToString("MMyyyy", CultureInfo.CreateSpecificCulture("pt-BR")); ;

            //aqui eu faço um calculo com números aleatórios. 
            var number = 7 * new Random().Next(1000000, 9999999) / 100;

            //aqui retornamos o dia e o ano junto com o resultado dos calculos.
            return dataString + number.ToString("D");
        }

        public static void VerificaData(Ticket ticket)
        {
            var ultimaResposta = ticket.LstRespostas.FindLast(c => ticket.AtendenteId != null);
            if (ultimaResposta.DataCadastro.AddDays(14) < DateTime.Now)
                ticket.Status = Enum.Parse<Status>("FECHADO");

            if (ticket.DataCadastro.AddMonths(1) < DateTime.Now && ticket.AtendenteId != null)
                ticket.Status = Enum.Parse<Status>("FECHADO");
        }
    }
}
