using Model;
using FluentValidation;
using System.Linq;
using Core.Util;
using System;
using System.Collections.Generic;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

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
            _ticket = _mapper.Map<TicketView, Ticket>(ticket);
            _serviceContext = serviceContext;

            RuleFor(t => t.Titulo).NotNull().MinimumLength(5)
                .WithMessage("O título do ticket não pode ser nulo  minimo de caracteres é 5");

            RuleFor(t => t.Mensagem).NotNull().MinimumLength(10)
                .WithMessage("A Mensagem do ticket não pode ser nula , deve haver uma descrição, e o minimo de caracteres é 10");

            RuleFor(t => t.Status).IsInEnum();

            RuleFor(t => t.Avaliacao).IsInEnum();
        }
        #endregion

        public async Task<Retorno> CadastrarTicket(string Usertoken)
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
            var cliente = await _serviceContext.Usuarios.SingleOrDefaultAsync(u => u.Id == _ticket.ClienteId);

            if (cliente.Tipo != "CLIENTE") return new Retorno { Status = false, Resultado = new List<string> { "Usuario não é do tipo cliente" } };

            //add o ticket e salvo alterações.
            await _serviceContext.Tickets.AddAsync(_ticket);

            await _serviceContext.SaveChangesAsync();

            return new Retorno { Status = true, Resultado = new List<string> { $"{cliente.Nome} seu Ticket foi cadastrado com Sucesso!" } };
        }

        public async Task<Retorno> AtualizarTicket(string Usertoken, string TicketID, TicketView ticketView)
        {
            //verifico login.
            if (!Autorizacao.ValidarUsuario(Usertoken, _serviceContext))
                return new Retorno { Status = false, Resultado = new List<string> { "Autorização Negada!" } };

            //verifico se o Ticket ID é valido.
            if (!Guid.TryParse(TicketID, out Guid tId))
                return new Retorno { Status = false, Resultado = new List<string> { "Ticket não identificado!" } };

            var ticketSelecionado = await _serviceContext.Tickets.SingleOrDefaultAsync(t => t.Id == tId);

            if (ticketSelecionado.LstRespostas == null || ticketSelecionado.LstRespostas.Count() == 0)
                return new Retorno { Status = false, Resultado = new List<string> { "Como o ticket já contem respostas. não é mais possivel atualiza-lo" } };

            //vejo se o cliente que ta longado é o mesmo que está Atualizando o ticket.
            if (ticketSelecionado.ClienteId != Guid.Parse(Usertoken)) return new Retorno { Status = false, Resultado = new List<string> { "Usuario não é o mesmo que postou o ticket!" } };

            if (ticketSelecionado.Status == Status.FECHADO) return new Retorno { Status = false, Resultado = new List<string> { "Não se pode atualizar Tickets já encerrados." } };

            _mapper.Map(ticketView, ticketSelecionado);
            await _serviceContext.SaveChangesAsync();

            return new Retorno { Status = true, Resultado = _mapper.Map<TicketRetorno>(ticketSelecionado) };
        }
        public async Task<Retorno> DeletarTicket(string Usertoken, string TicketID)
        {
            //verifico login.
            if (!Autorizacao.ValidarUsuario(Usertoken, _serviceContext))
                return new Retorno { Status = false, Resultado = new List<string> { "Autorização Negada!" } };

            //verifico se o Ticket ID é valido.
            if (!Guid.TryParse(TicketID, out Guid tId))
                return new Retorno { Status = false, Resultado = new List<string> { "Ticket não identificado!" } };

            _ticket = await _serviceContext.Tickets.Include(c => c.LstRespostas).SingleOrDefaultAsync(t => t.Id == tId);


            //vejo se o cliente que ta logado é o mesmo que está públicou o ticket.
            if (Guid.Parse(Usertoken) != _ticket.ClienteId) return new Retorno { Status = false, Resultado = new List<string> { "Usuario não pode deletar esse ticket, pois não é quem postou o mesmo!" } };

            //tento excluir o ticket e salvar as  alterações.

            if (_ticket.LstRespostas.Count() > 0)
                return new Retorno { Status = false, Resultado = new List<string> { "Não é possivel remover este ticket, pois ele ja tem respostas!" } };

            _serviceContext.Tickets.Remove(_ticket);
            await _serviceContext.SaveChangesAsync();


            return new Retorno { Status = true, Resultado = new List<string> { " seu Ticket foi Deletado com Sucesso!" } };
        }
        public async Task<Retorno> BuscarTicketporNumeroDoTicket(string Usertoken, string NumeroTicketQueVem)
        {
            //verifico login.
            if (!Autorizacao.ValidarUsuario(Usertoken, _serviceContext))
                return new Retorno { Status = false, Resultado = new List<string> { "Autorização Negada!" } };

            if (!long.TryParse(NumeroTicketQueVem, out long numeroticket))
                return new Retorno { Status = false, Resultado = new List<string> { "Número do ticket incorreto!" } };

            var cliente = await _serviceContext.Usuarios.SingleOrDefaultAsync(u => u.Id == Guid.Parse(Usertoken));
            if (cliente == null) return new Retorno { Status = false, Resultado = new List<string> { "Cliente não identificado!" } };

            //vejo se o cliente que ta longado é o mesmo que está públicando o ticket .

            var TicketSolicitado = await _serviceContext.Tickets.Include(c=>c.LstRespostas).SingleOrDefaultAsync(t => t.NumeroTicket == numeroticket && t.ClienteId == cliente.Id || t.NumeroTicket == numeroticket && t.AtendenteId == cliente.Id);

            TicketSolicitado.LstRespostas = await _serviceContext.Respostas.Where(c => c.TicketId == TicketSolicitado.Id).ToListAsync();

            var TicketRetorno = _mapper.Map<TicketRetorno>(TicketSolicitado);

            return new Retorno { Status = true, Resultado = _mapper.Map<TicketRetorno>(TicketSolicitado) };
        }
        public async Task<Retorno> BuscarTodosTickets(string Usertoken, int NumeroPagina, int QuantidadeRegistro, string StatusAtual)
        {
            //verifico login.
            if (!Guid.TryParse(Usertoken, out Guid token))
                return new Retorno { Status = false, Resultado = new List<string> { "Token inválido" } };

            //busco pelo usuario e vejo se ele existe.

            var usuario = await _serviceContext.Usuarios.SingleOrDefaultAsync(u => u.Id == Guid.Parse(Usertoken));
            if (usuario == null)
                return new Retorno { Status = false, Resultado = new List<string> { "Cliente não identificado!" } };

            // nova instancia da paganicação
            var Paginacao = new Paginacao();

            //Confiro o tipo do usuario e exibo os resultados paginados de acordo com o tipo do usuario
            if (usuario.Tipo.ToUpper() == "ATENDENTE")
            {
                List<Ticket> ticketsAtendente;

                // busco pelos tickets daquele especifico usuario 
                switch (StatusAtual.ToUpper())
                {
                    case "CONCLUIDO":
                        ticketsAtendente = _serviceContext.Tickets.Where(t => t.Status == Status.FECHADO && t.AtendenteId == usuario.Id).ToList();
                        break;

                    case "ANDAMENTO":
                        ticketsAtendente = await _serviceContext.Tickets.Where(t => ( t.Status == Status.AGUARDANDO_RESPOSTA_DO_CLIENTE || t.Status == Status.AGUARDANDO_RESPOSTA_DO_ATENDENTE)
                  && t.AtendenteId == usuario.Id).ToListAsync();
                        break;

                    default:
                        ticketsAtendente = await _serviceContext.Tickets.Where(c => c.AtendenteId == null && c.Status == Status.ABERTO).ToListAsync();
                        break;
                }

                // caso for possivel realizar a paginação se nao for exibo a quantidade padrão = 10, e ordeno pelo mais antigo
                if (NumeroPagina > 0 && QuantidadeRegistro > 0)
                {
                    Paginacao.Paginar(NumeroPagina, QuantidadeRegistro, ticketsAtendente.Count());
                    var listaPaginada = ticketsAtendente.OrderByDescending(d => d.DataCadastro).Skip((NumeroPagina - 1) * QuantidadeRegistro).Take(QuantidadeRegistro).ToList();

                    return listaPaginada.Count() == 0 ? new Retorno { Status = false, Resultado = new List<string>
                    { "Não foi possivel realizar a paginação" } } : new Retorno
                    { Status = true, Paginacao = Paginacao, Resultado =_mapper.Map<List<TicketRetorno>>(ticketsAtendente) };

                }

                Paginacao.Paginar(1, 10, ticketsAtendente.Count());

                return _mapper.Map<List<TicketRetorno>>(ticketsAtendente.Take(10)).Count() == 0 ? new Retorno { Status = false, Resultado = new List<string> { $"VocÊ nao tem tickets {StatusAtual} momento!" } } : new Retorno { Status = true, Paginacao = Paginacao, Resultado = _mapper.Map<List<TicketRetorno>>(ticketsAtendente.Take(10)) };
            }

            // busco pelos tickets daquele especifico usuario 
            var ticketsCliente = _serviceContext.Tickets.Where(c => c.Status == Enum.Parse<Status>("ABERTO") || c.Status == Enum.Parse<Status>(" AGUARDANDO_RESPOSTA_DO_ATENDENTE") && c.ClienteId == Guid.Parse(Usertoken)).ToList();


              ticketsCliente = StatusAtual.ToUpper() == "CONCLUIDO" ?   _serviceContext.Tickets.Where(c => c.Status == Status.FECHADO && c.ClienteId == usuario.Id).ToList() : 
               _serviceContext.Tickets.Where(c => c.Status != Status.FECHADO && c.ClienteId == usuario.Id).ToList();

            _serviceContext.SaveChanges();
            // caso for possivel realizar a paginação se nao for exibo a quantidade padrão = 10


            if (NumeroPagina > 0 && QuantidadeRegistro > 0)
            {
                Paginacao.Paginar(NumeroPagina, QuantidadeRegistro, ticketsCliente.Count());
                var listaPaginada = ticketsCliente.OrderByDescending(d => d.DataCadastro).Skip((NumeroPagina - 1) * QuantidadeRegistro).Take(QuantidadeRegistro).ToList();
                return listaPaginada.Count() == 0 ? new Retorno
                { Status = false, Resultado = new List<string> { "Não foi possivel realizar a paginação" } } : 
                new Retorno { Status = true, Paginacao = Paginacao, Resultado = _mapper.Map<List<TicketRetorno>>(ticketsCliente) };
            }
            Paginacao.Paginar(1, 10, ticketsCliente.Count());

            //assasaassasasa q
            return _mapper.Map<List<TicketRetorno>>(ticketsCliente.Take(10)).Count() == 0 ? new Retorno { Status = false,  Resultado = new List<string> { $"VocÊ nao tem tickets {StatusAtual} momento!" } }: new Retorno { Status = true, Paginacao = Paginacao, Resultado = _mapper.Map<List<TicketRetorno>>(ticketsCliente.Take(10)) };
        }
        public async Task<Retorno> TomarPosseTicket(string Usertoken, string numeroTicket)
        {
            //verifico login.
            if (!Autorizacao.ValidarUsuario(Usertoken, _serviceContext))
                return new Retorno { Status = false, Resultado = new List<string> { "Autorização Negada!" } };

            //verifico se o Ticket ID é valido.
            if (!long.TryParse(numeroTicket, out long numeroDoTicket))
                return new Retorno { Status = false, Resultado = new List<string> { "Ticket não identificado!" } };

            //verifico se tem um usuário na base com ID informado e o tipo dele é atendente.
            var atendente = await _serviceContext.Usuarios.SingleOrDefaultAsync(u => u.Id == Guid.Parse(Usertoken) && u.Tipo == "ATENDENTE");
            if (atendente == null) return new Retorno { Status = false, Resultado = new List<string> { "Atendente não identificado!" } };

            //verifico se o ticket solicitado existe na base de dados.

            var TicketSolicitado = await _serviceContext.Tickets.SingleOrDefaultAsync(t => t.NumeroTicket == numeroDoTicket);
            if (TicketSolicitado.AtendenteId != null) return new Retorno { Status = false, Resultado = new List<string> { "Ticket já tem um atendente." } };

            //passo o valor para o ticket
            TicketSolicitado.AtendenteId = atendente.Id;
            TicketSolicitado.Status = Status.AGUARDANDO_RESPOSTA_DO_ATENDENTE;

            ///asdassasaassasa
            await _serviceContext.SaveChangesAsync();
            return new Retorno { Status = true, Resultado = new List<string> { $"{atendente.Nome} você atribuiu esse Ticket a sua base." } };
        }



        // Método para buscar os tickets disponiveis para o atendente
        public Retorno BuscarTicketSemAtendente(string Usertoken, int NumeroPagina, int QuantidadeRegistro)
        {
            //verifico login.
            if (!Autorizacao.ValidarUsuario(Usertoken, _serviceContext))
                return new Retorno { Status = false, Resultado = new List<string> { "Autorização Negada!" } };

            var todosTickets = _serviceContext.Tickets.Where(c => c.AtendenteId == null && c.Status != Enum.Parse<Status>("FECHADO")).ToList();
            
            // nova instancia da paganicação
            var Paginacao = new Paginacao();
  
            // caso for possivel realizar a paginação se nao for exibo a quantidade padrão = 10
            if (NumeroPagina > 0 && QuantidadeRegistro > 0)
            {
                Paginacao.Paginar(NumeroPagina, QuantidadeRegistro, todosTickets.Count());
                return new Retorno { Status = true, Paginacao = Paginacao, Resultado = todosTickets.OrderByDescending(d => d.DataCadastro).Skip((NumeroPagina - 1) * QuantidadeRegistro).Take(QuantidadeRegistro) };
            }

           // /asasaassaasassa
            Paginacao.Paginar(1, 10, todosTickets.Count());

            return new Retorno { Status = true, Paginacao = Paginacao, Resultado = _mapper.Map<List<TicketRetorno>>(todosTickets.Take(10)) };
        }
        public async Task<Retorno> AvaliarTicket(string tokenAutor, string ticketId, string avaliacao)
        {
            //verifico login.
            if (!Autorizacao.ValidarUsuario(tokenAutor, _serviceContext))
                return new Retorno { Status = false, Resultado = new List<string> { "Autorização Negada!" } };

            if (int.TryParse(avaliacao, out int num) || num < 0 || num > 4)
                return new Retorno { Status = false, Resultado = new List<string> { "Avaliação não válida!" } };

            //verifico se o Ticket ID é valido.
            if (!Guid.TryParse(ticketId, out Guid tId))
                return new Retorno { Status = false, Resultado = new List<string> { "Ticket não identificado!" } };

            // vejo se a avaliacao é valida
            if (!Enum.TryParse(avaliacao, out Avaliacao result))
                return new Retorno { Status = false, Resultado = new List<string> { "Avaliação não válida!" } };

            // busco pelo ticket e faço a validação de o ticket precisar estar fechado
            var Oticket = await _serviceContext.Tickets.SingleOrDefaultAsync(c => c.Id == Guid.Parse(ticketId) && c.ClienteId == Guid.Parse(tokenAutor));

            if (Oticket.AtendenteId == null)
                return new Retorno { Status = false, Resultado = new List<string> { "Não é possivel avaliar um ticket que nao foi atendido" } };

            if (Oticket.Status != Status.FECHADO)
                return new Retorno { Status = false, Resultado = new List<string> { "O ticket precisa estar fechado para ocorrer a avaliação" } };

            Oticket.Avaliacao = result;

            await _serviceContext.SaveChangesAsync();

            return new Retorno { Status = true, Resultado = new List<string> { "Avaliação registrada com sucesso!" } };
        }
        // metódo para realizar o fechamento do ticket
        public async Task<Retorno> FecharTicket(string tokenAutor, string ticketId)
        {
            //verifico login.
            if (!Autorizacao.ValidarUsuario(tokenAutor, _serviceContext))
                return new Retorno { Status = false, Resultado = new List<string> { "Autorização Negada!" } };

            // verifico se o guid o ticket é valido
            if (!Guid.TryParse(ticketId, out Guid result))
                return new Retorno { Status = false, Resultado = new List<string> { "Ticket inválido" } };

            // busco e valido se este ticket em especifico é valido.
            var oTicket = await _serviceContext.Tickets.SingleOrDefaultAsync(c => c.Id == result && c.ClienteId == Guid.Parse(tokenAutor));

            if (oTicket == null)
                return new Retorno { Status = false, Resultado = new List<string> { "Ticket inválido" } };

            if (oTicket.ClienteId != Guid.Parse(tokenAutor))
                return new Retorno { Status = false, Resultado = new List<string> { "Somente clientes podem fechar seus tickets!" } };

            if (oTicket.Status == Status.FECHADO)
                return new Retorno { Status = false, Resultado = new List<string> { "Este ticket já foi fechado!" } };

            if (oTicket.AtendenteId == null)
                return new Retorno { Status = false, Resultado = new List<string> { "Não é possivel fechar um ticket que nao foi atendido" } };

            // atribuo e fecho o ticket
            oTicket.Status = Status.FECHADO;

            await _serviceContext.SaveChangesAsync();

            return new Retorno { Status = true, Resultado = new List<string> { "Ticket fechado com sucesso!" } };
        }
        public async Task<Retorno> TrocarAtendente(string Numero, string tokenAutor)
        {
            if (!Autorizacao.ValidarUsuario(tokenAutor, _serviceContext))
                return new Retorno { Status = false, Resultado = new List<string> { "Autorização Negada!" } };

            if (!long.TryParse(Numero, out long Result) || !_serviceContext.Tickets.Any(c => c.NumeroTicket == Result))
                return new Retorno { Status = false, Resultado = new List<string> { "Numero não existe na base de dados" } };

            var UltimaResposta = await _serviceContext.Respostas.Include(c => c.Usuario).Where(c => c.TicketId == _serviceContext.Tickets.FirstOrDefault(d => d.NumeroTicket == Result).Id).OrderBy(c => c.DataCadastro).LastAsync();

            if (UltimaResposta.Usuario.Tipo == "ATENDENTE" || (UltimaResposta.Usuario.Tipo == "CLIENTE" && UltimaResposta.DataCadastro.AddDays(14) < DateTime.Now))
                return new Retorno { Status = false, Resultado = new List<string> { "Ultima Mensagem é do atendente ou Ultima Mensagem do cliente foi em menos de 14 dias" } };

            var Ticket = await _serviceContext.Tickets.SingleOrDefaultAsync(c => c.Id == UltimaResposta.TicketId);

            Ticket.AtendenteId = Guid.Parse(tokenAutor);

          await  _serviceContext.SaveChangesAsync();

            return new Retorno { Status = true, Resultado = new List<string> { $"Ticket Numero: {Ticket.NumeroTicket} atualizado, você agora é o atendente deste ticket" } };
        }

        //metodo para fazer uma identificação unica de cada usuário.
        public long ConvertNumeroTickets()
        {
            var dataString = DateTime.Now.ToString("yyyyMM");
            try
            {

                var number = _serviceContext.Tickets.OrderBy(c => c.NumeroTicket).Last().NumeroTicket + 1;

                //aqui retornamos o dia e o ano junto com o resultado dos calculos.
                return long.Parse(dataString + number.ToString().Substring(6));
            }
            catch (Exception) { return long.Parse(dataString + (1).ToString("D6")); }
        }
    }
}