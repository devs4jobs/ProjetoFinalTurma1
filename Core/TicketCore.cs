using Model;
using FluentValidation;
using System.Linq;
using Core.Util;
using System;
using System.Collections.Generic;
using AutoMapper;
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


        public TicketCore(TicketView ticket, ServiceContext serviceContext, IMapper mapper)
        {
            _mapper = mapper;
            _ticket = _mapper.Map<TicketView,Ticket>(ticket);
            _serviceContext = serviceContext;

            RuleFor(t => t.Titulo).NotNull()
                .WithMessage("O título do ticket não pode ser nulo.");

            RuleFor(t => t.Mensagem).NotNull()
                .WithMessage("A Mensagem do ticket não pode ser nula , deve haver uma descrição.");


            RuleFor(t => t.Status).IsInEnum();

            RuleFor(t => t.Avaliacao).IsInEnum();
        }
        #endregion

        public Retorno CadastrarTicket(string Usertoken)
        {
            //verifico login.
            if (!Autorizacao.ValidarUsuario(Usertoken, _serviceContext))
                return new Retorno { Status = false, Resultado = new List<string> { "Autorização Negada! ,Usuario não existe" } };

            //verifico ticket se é valido.
            var validar = Validate(_ticket);
            if (!validar.IsValid)
                return new Retorno { Status = false, Resultado = validar.Errors.Select(e => e.ErrorMessage).ToList() };

            _ticket.NumeroTicket = ConvertNumeroTickets();
            _ticket.ClientId = Guid.Parse(Usertoken);
            //busco o cliente na base e verifico.
            var cliente = _serviceContext.Usuarios.FirstOrDefault(u => u.Id == _ticket.ClientId);

            if (cliente.Tipo != "CLIENTE") return new Retorno { Status = false, Resultado = new List<string> { "Usuario não é do tipo cliente" } };

            //add o ticket e salvo alterações.
            _serviceContext.Tickets.Add(_ticket);

            _serviceContext.SaveChanges();

            return new Retorno { Status = true, Resultado = new List<string> { $"{cliente.Nome} seu Ticket foi cadastrado com Sucesso!" } };
        }
        public Retorno AtualizarTicket(string Usertoken, string TicketID, TicketUpadateView ticketView)
        {
            //verifico login.
            if (!Autorizacao.ValidarUsuario(Usertoken, _serviceContext))
                return new Retorno { Status = false, Resultado = new List<string> { "Autorização Negada!" } };

            //verifico se o Ticket ID é valido.
            if (!Guid.TryParse(TicketID, out Guid tId))
                return new Retorno { Status = false, Resultado = new List<string> { "Ticket não identificado!" } };

            var ticketSelecionado = _serviceContext.Tickets.FirstOrDefault(t => t.Id == tId);

            //vejo se o cliente que ta longado é o mesmo que está Atualizando o ticket.
            if (ticketSelecionado.ClientId != Guid.Parse(Usertoken)) return new Retorno { Status = false, Resultado = new List<string> { "Usuario não é o mesmo que postou o ticket!" } };

            _mapper.Map(ticketView, ticketSelecionado);
            AtribuiLista(ticketSelecionado);

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

            _ticket = _serviceContext.Tickets.Include(c => c.Cliente).FirstOrDefault(t => t.Id == tId);

            //vejo se o cliente que ta longado é o mesmo que está públicou o ticket.
            if (Guid.Parse(Usertoken) != _ticket.ClientId) return new Retorno { Status = false, Resultado = new List<string> { "Usuario não pode deletar esse ticket, pois não é quem postou o mesmo!" } };

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

            //vejo se o cliente que ta longado é o mesmo que está públicando o ticket .
            var TicketSolicitado = _serviceContext.Tickets.FirstOrDefault(t => t.Id == tId && t.ClientId == cliente.Id || t.Id == tId && t.AtendentId == cliente.Id);

            AtribuiLista(TicketSolicitado);


            return TicketSolicitado != null ? new Retorno { Status = true, Resultado = TicketSolicitado } : new Retorno { Status = false, Resultado = new List<string> { "Ticket não identificado!" } };
        }
        public Retorno BuscarTodosTickets(string Usertoken, int NumeroPagina, int QuantidadeRegistro)
        {
            //verifico login.
            if (!Autorizacao.ValidarUsuario(Usertoken, _serviceContext))
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

                var ticketsAtendente = _serviceContext.Tickets.Where(t => t.Status == Enum.Parse<Status>("ABERTO") || t.Status == Enum.Parse<Status>(" AGUARDANDO_RESPOSTA_DO_CLIENTE")
                && t.AtendentId == Guid.Parse(Usertoken)).ToList();

                // caso for possivel realizar a paginação se nao for exibo a quantidade padrão = 10, e ordeno pelo mais antigo
                if (NumeroPagina > 0 && QuantidadeRegistro > 0)
                {
                    Paginacao.Paginar(NumeroPagina, QuantidadeRegistro, ticketsAtendente.Count());
                    var listaPaginada = ticketsAtendente.OrderByDescending(d => d.DataCadastro).Skip((NumeroPagina - 1) * QuantidadeRegistro).Take(QuantidadeRegistro).ToList();
                    return listaPaginada.Count() == 0 ? new Retorno { Status = false, Resultado = new List<string> { "Não foi possivel realizar a paginação" } } : new Retorno { Status = true, Paginacao = Paginacao, Resultado = ticketsAtendente };
                }

                Paginacao.Paginar(1, 10, ticketsAtendente.Count());

                return new Retorno { Status = true, Paginacao = Paginacao, Resultado = ticketsAtendente.Take(10) };
            }
            // busco pelos tickets daquele especifico usuario 

            var ticketsCliente = _serviceContext.Tickets.Where(c => c.Status == Enum.Parse<Status>("ABERTO") || c.Status == Enum.Parse<Status>(" AGUARDANDO_RESPOSTA_DO_ATENDENTE") && c.ClientId == Guid.Parse(Usertoken)).ToList();

            // caso for possivel realizar a paginação se nao for exibo a quantidade padrão = 10
            if (NumeroPagina > 0 && QuantidadeRegistro > 0)
            {
                Paginacao.Paginar(NumeroPagina, QuantidadeRegistro, ticketsCliente.Count());
                var listaPaginada = ticketsCliente.OrderByDescending(d => d.DataCadastro).Skip((NumeroPagina - 1) * QuantidadeRegistro).Take(QuantidadeRegistro).ToList();
                return listaPaginada.Count() == 0 ? new Retorno { Status = false, Resultado = new List<string> { "Não foi possivel realizar a paginação" } } : new Retorno { Status = true, Paginacao = Paginacao, Resultado = ticketsCliente };
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
            if (!Guid.TryParse(TicketID, out Guid ValidTicketId))
                return new Retorno { Status = false, Resultado = new List<string> { "Ticket não identificado!" } };

            //verifico se tem um usuário na base com ID informado e o tipo dele é atendente.
            var atendente = _serviceContext.Usuarios.FirstOrDefault(u => u.Id == Guid.Parse(Usertoken) && u.Tipo == "ATENDENTE");
            if (atendente == null) return new Retorno { Status = false, Resultado = new List<string> { "Atendente não identificado!" } };

            //verifico se o ticket solicitado existe na base de dados.

            var TicketSolicitado = _serviceContext.Tickets.FirstOrDefault(t => t.Id == ValidTicketId);
            if (TicketSolicitado.AtendentId != null) return new Retorno { Status = false, Resultado = new List<string> { "Ticket já tem um atendente." } };


            //passo o valor para o ticket
            TicketSolicitado.AtendentId = atendente.Id;

            _serviceContext.SaveChanges();
            return new Retorno { Status = true, Resultado = new List<string> { $"{atendente.Nome} você atribuiu esse Ticket a sua base." } };
        }

        // Método para buscar os tickets disponiveis para o atendente
        public Retorno BuscarTicketSemAtendente(string Usertoken, int NumeroPagina, int QuantidadeRegistro)
        {
            //verifico login.
            if (!Autorizacao.ValidarUsuario(Usertoken, _serviceContext))
                return new Retorno { Status = false, Resultado = new List<string> { "Autorização Negada!" } };

            var todosTickets = _serviceContext.Tickets.Where(c => c.AtendentId == null && c.Status != Enum.Parse<Status>("FECHADO")).ToList();
           // todosTickets.ForEach(c => AtribuiLista(c));
            
              // nova instancia da paganicação
              var Paginacao = new Paginacao();

            // caso for possivel realizar a paginação se nao for exibo a quantidade padrão = 10
            if (NumeroPagina > 0 && QuantidadeRegistro > 0)
            {
                Paginacao.Paginar(NumeroPagina, QuantidadeRegistro, todosTickets.Count());
                return new Retorno { Status = true, Paginacao = Paginacao, Resultado = todosTickets.OrderByDescending(d => d.DataCadastro).Skip((NumeroPagina - 1) * QuantidadeRegistro).Take(QuantidadeRegistro) };
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
            var Oticket = _serviceContext.Tickets.FirstOrDefault(c => c.Id == Guid.Parse(ticketId) && c.ClientId == Guid.Parse(tokenAutor));

            if (Oticket.Status == Enum.Parse<Status>("ABERTO"))
                return new Retorno { Status = false, Resultado = new List<string> { "O ticket precisa estar fechado para ocorrer a avaliação" } };

            Oticket.Avaliacao = result;

            _serviceContext.SaveChanges();

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
            var oTicket = _serviceContext.Tickets.FirstOrDefault(c => c.Id == result && c.AtendentId == Guid.Parse(tokenAutor));

            if (oTicket == null)
                return new Retorno { Status = false, Resultado = new List<string> { "Ticket inválido" } };

            // atribuo e fecho o ticket
            oTicket.Status = Enum.Parse<Status>("FECHADO");

            return new Retorno { Status = true, Resultado = new List<string> { "Ticket fechado com sucesso!" } };
        }

        public Retorno TrocarAtendente(string Numero,string tokenAutor)
        {
            if (!Autorizacao.ValidarUsuario(tokenAutor, _serviceContext))
                return new Retorno { Status = false, Resultado = new List<string> { "Autorização Negada!" } };

            if (!long.TryParse(Numero, out long Result)||_serviceContext.Tickets.Any(c=>c.NumeroTicket==Result))
                return new Retorno { Status = false, Resultado = new List<string> { "Numero não existe na base de dados" } };

            var UltimaResposta =_serviceContext.Respostas.Include(c=>c.Usuario).Where(c=>c.TicketId==_serviceContext.Tickets.FirstOrDefault(d => d.NumeroTicket == Result).Id).OrderBy(c=>c.DataCadastro).Last();

            if (UltimaResposta.Usuario.Tipo == "ATENDENTE"||(UltimaResposta.Usuario.Tipo=="CLIENTE"&&UltimaResposta.DataCadastro.AddDays(14)<DateTime.Now))
                return new Retorno { Status = false, Resultado = new List<string> { "Ultima Mensagem é do atendente ou Ultima Mensagem do cliente foi em menos de 14 dias" } };

            var Ticket = _serviceContext.Tickets.FirstOrDefault(c => c.Id == UltimaResposta.TicketId);

            Ticket.AtendentId = Guid.Parse(tokenAutor);

            _serviceContext.SaveChanges();

            return new Retorno { Status = true, Resultado = new List<string> { $"Ticket Numero: {Ticket.NumeroTicket} atualizado, você agora é o atendente deste ticket" } };
        }

        //metodo para fazer uma identificação unica de cada usuário.
        public long ConvertNumeroTickets()
        {
            var dataString = DateTime.Now.ToString("yyyyMM"); 
            try { 

            var number = _serviceContext.Tickets.OrderBy(c=>c.NumeroTicket).Last().NumeroTicket + 1;

            //aqui retornamos o dia e o ano junto com o resultado dos calculos.
            return long.Parse(dataString + number.ToString().Substring(6));
            }
            catch (Exception) { return long.Parse(dataString + (1).ToString("D6")); }
        }

        //Método para realizar a atribuicao na lista da lista de respostas no tciket
        public void AtribuiLista(Ticket ticket)
        {
            var ListaResposta = _serviceContext.Respostas.Where(c => c.TicketId == ticket.Id).ToList();

            var oAtendente = _serviceContext.Usuarios.FirstOrDefault(c => c.Id == ticket.AtendentId);
            var oUsuario = _serviceContext.Usuarios.FirstOrDefault(c => c.Id == ticket.ClientId);

            if (oAtendente != null)
                ticket.Atendente = new UsuarioRetorno { Nome = oAtendente.Nome, Email = oAtendente.Email };

            if (oUsuario != null)
                ticket.Cliente = new UsuarioRetorno { Nome = oUsuario.Nome, Email = oUsuario.Email };

            foreach (var Resposta in ListaResposta)
            {
                if (Resposta.UsuarioId == oAtendente.Id)
                    Resposta.Usuario = new UsuarioRetorno { Nome = oAtendente.Nome, Email = oAtendente.Email };

                if (Resposta.UsuarioId == oUsuario.Id)
                    Resposta.Usuario = new UsuarioRetorno { Nome = oUsuario.Nome, Email = oUsuario.Email };
            }

            ticket.LstRespostas = ListaResposta.OrderBy(c => c.DataCadastro).ToList();
            ticket.Atendent = null;
            ticket.Client = null;
        }
    }
}
