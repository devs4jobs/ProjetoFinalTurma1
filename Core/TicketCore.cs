using Model;
using FluentValidation;
using System.Linq;
using Core.Util;
using System;
using System.Collections.Generic;
using AutoMapper;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.CSharp.RuntimeBinder;

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

            RuleFor(t => t.Titulo).NotNull().MinimumLength(5)
                .WithMessage("O título do ticket não pode ser nulo  minimo de caracteres é 5");

            RuleFor(t => t.Mensagem).NotNull().MinimumLength(10)
                .WithMessage("A Mensagem do ticket não pode ser nula , deve haver uma descrição, e o minimo de caracteres é 10");

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

            //Atribuição do numero do ticket e do cliente ID
            _ticket.NumeroTicket = ConvertNumeroTickets();
            _ticket.ClienteId = Guid.Parse(Usertoken);
            //busco o cliente na base e verifico.
            var cliente = _serviceContext.Usuarios.FirstOrDefault(u => u.Id == _ticket.ClienteId);

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
            if (ticketSelecionado.ClienteId != Guid.Parse(Usertoken)) return new Retorno { Status = false, Resultado = new List<string> { "Usuario não é o mesmo que postou o ticket!" } };

            _mapper.Map(ticketView, ticketSelecionado);
            _serviceContext.SaveChanges();

            return new Retorno { Status = true, Resultado = _mapper.Map<TicketRetorno>(ticketSelecionado) };
        }
        public Retorno DeletarTicket(string Usertoken, string TicketID)
        {
            //verifico login.
            if (!Autorizacao.ValidarUsuario(Usertoken, _serviceContext))
                return new Retorno { Status = false, Resultado = new List<string> { "Autorização Negada!" } };

            //verifico se o Ticket ID é valido.
            if (!Guid.TryParse(TicketID, out Guid tId))
                return new Retorno { Status = false, Resultado = new List<string> { "Ticket não identificado!" } };

            _ticket = _serviceContext.Tickets.Include(c =>c.LstRespostas).FirstOrDefault(t => t.Id == tId);


            //vejo se o cliente que ta logado é o mesmo que está públicou o ticket.
            if (Guid.Parse(Usertoken) != _ticket.ClienteId) return new Retorno { Status = false, Resultado = new List<string> { "Usuario não pode deletar esse ticket, pois não é quem postou o mesmo!" } };

            //tento excluir o ticket e salvar as  alterações.
            
            if(_ticket.LstRespostas.Count() >0)
                return new Retorno { Status = false, Resultado = new List<string> { "Não é possivel remover este ticket, pois ele ja tem respostas!" } };

            _serviceContext.Tickets.Remove(_ticket);
                _serviceContext.SaveChanges();
           
      
            return new Retorno { Status = true, Resultado = new List<string> { " seu Ticket foi Deletado com Sucesso!" } };
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
            var TicketSolicitado = _serviceContext.Tickets.FirstOrDefault(t => t.Id == tId && t.ClienteId == cliente.Id || t.Id == tId && t.AtendenteId == cliente.Id);

            TicketSolicitado.LstRespostas = _serviceContext.Respostas.Include(r => r.Usuario).Where(c => c.TicketId == TicketSolicitado.Id).OrderBy(c => c.DataCadastro).ToList(); 

            return TicketSolicitado != null ? new Retorno { Status = true, Resultado = _mapper.Map<TicketRetorno>(TicketSolicitado) } : new Retorno { Status = false, Resultado = new List<string> { "Ticket não identificado!" } };
        }

        public Retorno BuscarTodosTickets(string Usertoken, int NumeroPagina, int QuantidadeRegistro,string StatusAtual)
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
                List<Ticket> ticketsAtendente;
                // busco pelos tickets daquele especifico usuario 
                if (StatusAtual == "FECHADO")
                    ticketsAtendente = _serviceContext.Tickets.Where(t => t.Status == Enum.Parse<Status>("FECHADO") && t.AtendenteId == Guid.Parse(Usertoken)).ToList();

                else if (StatusAtual == "ANDAMENTO")
                    ticketsAtendente = _serviceContext.Tickets.Where(t => t.Status == Enum.Parse<Status>("ABERTO") || t.Status == Enum.Parse<Status>(" AGUARDANDO_RESPOSTA_DO_CLIENTE")
                    && t.AtendenteId == Guid.Parse(Usertoken)).ToList();

                else 
                    ticketsAtendente = _serviceContext.Tickets.Where(c => c.AtendenteId == null && c.Status != Enum.Parse<Status>("FECHADO")).ToList();

                // caso for possivel realizar a paginação se nao for exibo a quantidade padrão = 10, e ordeno pelo mais antigo
                if (NumeroPagina > 0 && QuantidadeRegistro > 0)
                {
                    Paginacao.Paginar(NumeroPagina, QuantidadeRegistro, ticketsAtendente.Count());
                    var listaPaginada = ticketsAtendente.OrderByDescending(d => d.DataCadastro).Skip((NumeroPagina - 1) * QuantidadeRegistro).Take(QuantidadeRegistro).ToList();
                    return listaPaginada.Count() == 0 ? new Retorno { Status = false, Resultado = new List<string> { "Não foi possivel realizar a paginação" } } : new Retorno { Status = true, Paginacao = Paginacao, Resultado =_mapper.Map<List<TicketRetorno>>(ticketsAtendente) };
                }

                Paginacao.Paginar(1, 10, ticketsAtendente.Count());
                var retorno1 = _mapper.Map<List<TicketRetorno>>(ticketsAtendente.Take(10));

                return retorno1.Count() == 0 ? new Retorno { Status = false, Resultado = new List<string> { "VocÊ nao tem tickets no momento!" } } : new Retorno { Status = true, Paginacao = Paginacao, Resultado = retorno1 };
            }

            // busco pelos tickets daquele especifico usuario 
            List<Ticket> ticketsCliente;

            if (StatusAtual.ToUpper()=="CONCLUIDO")
                ticketsCliente = _serviceContext.Tickets.Where(c => (c.Status == Enum.Parse<Status>("ABERTO") ||  c.Status == Enum.Parse<Status>(" AGUARDANDO_RESPOSTA_DO_ATENDENTE")) && c.ClienteId == Guid.Parse(Usertoken) ).ToList();
            else
                ticketsCliente = _serviceContext.Tickets.Where(c => c.Status == Enum.Parse<Status>("FECHADO") && c.ClienteId == Guid.Parse(Usertoken)).ToList();


            // caso for possivel realizar a paginação se nao for exibo a quantidade padrão = 10
            if (NumeroPagina > 0 && QuantidadeRegistro > 0)
            {
                Paginacao.Paginar(NumeroPagina, QuantidadeRegistro, ticketsCliente.Count());
                var listaPaginada = ticketsCliente.OrderByDescending(d => d.DataCadastro).Skip((NumeroPagina - 1) * QuantidadeRegistro).Take(QuantidadeRegistro).ToList();
                return listaPaginada.Count() == 0 ? new Retorno { Status = false, Resultado = new List<string> { "Não foi possivel realizar a paginação" } } : new Retorno { Status = true, Paginacao = Paginacao, Resultado = _mapper.Map<List<TicketRetorno>>(ticketsCliente) };
            }
            Paginacao.Paginar(1, 10, ticketsCliente.Count());
            var retorno2 = _mapper.Map<List<TicketRetorno>>(ticketsCliente.Take(10));
            

            return new Retorno { Status = true, Paginacao = Paginacao, Resultado =  retorno2};
        }
        public Retorno TomarPosseTicket(string Usertoken, string numeroTicket)
        {
            //verifico login.
            if (!Autorizacao.ValidarUsuario(Usertoken, _serviceContext))
                return new Retorno { Status = false, Resultado = new List<string> { "Autorização Negada!" } };

            //verifico se o Ticket ID é valido.
            if (!long.TryParse(numeroTicket, out long numeroDoTicket))
                return new Retorno { Status = false, Resultado = new List<string> { "Ticket não identificado!" } };

            //verifico se tem um usuário na base com ID informado e o tipo dele é atendente.
            var atendente = _serviceContext.Usuarios.FirstOrDefault(u => u.Id == Guid.Parse(Usertoken) && u.Tipo == "ATENDENTE");
            if (atendente == null) return new Retorno { Status = false, Resultado = new List<string> { "Atendente não identificado!" } };

            //verifico se o ticket solicitado existe na base de dados.

            var TicketSolicitado = _serviceContext.Tickets.FirstOrDefault(t => t.NumeroTicket == numeroDoTicket);
            if (TicketSolicitado.AtendenteId != null) return new Retorno { Status = false, Resultado = new List<string> { "Ticket já tem um atendente." } };

            //passo o valor para o ticket
            TicketSolicitado.AtendenteId = atendente.Id;

            _serviceContext.SaveChanges();
            return new Retorno { Status = true, Resultado = new List<string> { $"{atendente.Nome} você atribuiu esse Ticket a sua base." } };
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
            var oTicket = _serviceContext.Tickets.FirstOrDefault(c => c.Id == result && c.AtendenteId == Guid.Parse(tokenAutor));

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

            if (UltimaResposta.Usuario.Tipo == "ATENDENTE" || (UltimaResposta.Usuario.Tipo=="CLIENTE" && UltimaResposta.DataCadastro.AddDays(14) < DateTime.Now))
                return new Retorno { Status = false, Resultado = new List<string> { "Ultima Mensagem é do atendente ou Ultima Mensagem do cliente foi em menos de 14 dias" } };

            var Ticket = _serviceContext.Tickets.FirstOrDefault(c => c.Id == UltimaResposta.TicketId);

            Ticket.AtendenteId = Guid.Parse(tokenAutor);

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
    }
}
