using Model;
using FluentValidation;
using System.Linq;
using Core.Util;
using System;
using System.Collections.Generic;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Model.Views.Receber;
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

        public TicketCore(ServiceContext serviceContext) => _serviceContext = serviceContext;
        public TicketCore(IMapper mapper, ServiceContext serviceContext)
        {
            _mapper = mapper;
            _serviceContext = serviceContext;
        }
        public TicketCore(Ticket ticket, ServiceContext serviceContext, IMapper mapper)
        {
            _mapper = mapper;
            _ticket = ticket;
            _serviceContext = serviceContext;

            RuleFor(w => w.Mensagem).NotEmpty().WithMessage("A mensagem não pode ser enviada sem conteúdo.");
            RuleFor(w => w.Titulo).NotEmpty().WithMessage("O título não pode ser enviado sem conteúdo.");
            RuleFor(t => t.Titulo).NotNull().MinimumLength(5).WithMessage("O título do ticket não pode ser nulo  minimo de caracteres é 5");
            RuleFor(t => t.Mensagem).NotNull().MinimumLength(10).WithMessage("A Mensagem do ticket não pode ser nula , deve haver uma descrição, e o minimo de caracteres é 10");
          
        }

        /// <summary>
        /// Método para realizar o cadastro de um ticket
        /// </summary>
        /// <param name="Usertoken"></param>
        public async Task<Retorno> CadastrarTicket(string Usertoken)
        {
            //verifico login.
            if (!Autorizacao.ValidarUsuario(Usertoken, _serviceContext))
                return new Retorno { Status = false, Resultado = new List<string> { "Autorização Negada!" } };

            //verifico ticket se é valido.
            var validar = Validate(_ticket);
            if (!validar.IsValid)
                return new Retorno { Status = false, Resultado = validar.Errors.Select(e => e.ErrorMessage).ToList() };

            //Atribuição do numero do ticket e do cliente ID
            _ticket.NumeroTicket = ConvertNumeroTickets();
            _ticket.ClienteId = Guid.Parse(Usertoken);
            //busco o cliente na base e verifico.
            var cliente = await _serviceContext.Usuarios.SingleOrDefaultAsync(u => u.Id == _ticket.ClienteId);

            if (cliente.Tipo != "CLIENTE") return new Retorno { Status = false, Resultado = new List<string> { "Usuário não é do tipo cliente" } };

            //add o ticket e salvo alterações.
            await _serviceContext.Tickets.AddAsync(_ticket);
            await _serviceContext.SaveChangesAsync();

            return new Retorno { Status = true, Resultado = new List<string> { $"{cliente.Nome} seu ticket foi cadastrado com Sucesso!" } };
        }

        /// <summary>
        /// Método para atualizar o ticket
        /// </summary>
        /// <param name="Usertoken"></param>
        /// <param name="TicketID"></param>
        /// <param name="ticketView"></param>
        public async Task<Retorno> AtualizarTicket(string Usertoken, string TicketID, Ticket ticketView)
        {
            //verifico login.
            if (!Autorizacao.ValidarUsuario(Usertoken, _serviceContext))
                return new Retorno { Status = false, Resultado = new List<string> { "Autorização Negada!" } };

            //verifico se o Ticket ID é valido.
            if (!Guid.TryParse(TicketID, out Guid tId))
                return new Retorno { Status = false, Resultado = new List<string> { "ticket não identificado!" } };

            //busco pelo ticket e efetuo as validacoes
            _ticket = await _serviceContext.Tickets.Include(o => o.LstRespostas).SingleOrDefaultAsync(t => t.Id == tId);

            if (_ticket == null)
                return new Retorno { Status = false, Resultado = new List<string> { "ticket inválido!" } };

            if (_ticket.LstRespostas != null)
                return new Retorno { Status = false, Resultado = new List<string> { "Como o ticket já contem respostas. não é mais possível atualiza-lo" } };

            //vejo se o cliente que ta longado é o mesmo que está Atualizando o ticket.
            if (_ticket.ClienteId != Guid.Parse(Usertoken)) return new Retorno { Status = false, Resultado = new List<string> { "Usuário não é o mesmo que postou o ticket!" } };

            if (_ticket.Status == Status.FECHADO) return new Retorno { Status = false, Resultado = new List<string> { "Não se pode atualizar tickets já encerrados." } };

            // mapeando o retorno
            _mapper.Map(ticketView, _ticket);
            await _serviceContext.SaveChangesAsync();

            return new Retorno { Status = true, Resultado = _mapper.Map<TicketRetorno>(_ticket) };
        }

        /// <summary>
        /// /Método para deletar um ticket da base
        /// </summary>
        /// <param name="Usertoken"></param>
        /// <param name="TicketID"></param>
        public async Task<Retorno> DeletarTicket(string Usertoken, string TicketID)
        {
            //verifico login.
            if (!Autorizacao.ValidarUsuario(Usertoken, _serviceContext))
                return new Retorno { Status = false, Resultado = new List<string> { "Autorização Negada!" } };

            //verifico se o Ticket ID é valido.
            if (!Guid.TryParse(TicketID, out Guid tId))
                return new Retorno { Status = false, Resultado = new List<string> { "ticket não identificado!" } };

            _ticket = await _serviceContext.Tickets.Include(c => c.LstRespostas).SingleOrDefaultAsync(t => t.Id == tId);

            //vejo se o cliente que ta logado é o mesmo que está públicou o ticket.
            if (Guid.Parse(Usertoken) != _ticket.ClienteId) return new Retorno { Status = false, Resultado = new List<string> { "Usuário não pode deletar esse ticket, pois não é quem postou o mesmo!" } };

            //tento excluir o ticket e salvar as  alterações.

            if (_ticket.LstRespostas != null)
                return new Retorno { Status = false, Resultado = new List<string> { "Não é possível remover este ticket, pois ele ja tem respostas!" } };

            // Remoção do ticket
            _serviceContext.Tickets.Remove(_ticket);
            await _serviceContext.SaveChangesAsync();

            return new Retorno { Status = true, Resultado = new List<string> { $"{_ticket.Cliente.Nome.ToLower()} seu ticket foi deletado com Sucesso!" } };
        }

        /// <summary>
        /// Método para realizar a busca de um ticket pelo numero
        /// </summary>
        /// <param name="Usertoken"></param>
        /// <param name="NumeroTicketQueVem"></param>
        public async Task<Retorno> BuscarTicketporNumeroDoTicket(string Usertoken, string NumeroTicketQueVem)
        {

            if (!Autorizacao.ValidarUsuario(Usertoken, _serviceContext))
                return new Retorno { Status = false, Resultado = new List<string> { "Autorização Negada!" } };

            if (!long.TryParse(NumeroTicketQueVem, out long numeroticket))
                return new Retorno { Status = false, Resultado = new List<string> { "Número do ticket incorreto!" } };

           _ticket = await _serviceContext.Tickets.Include(c => c.LstRespostas).SingleOrDefaultAsync(t => t.NumeroTicket == numeroticket && t.ClienteId == Guid.Parse(Usertoken) || t.NumeroTicket == numeroticket && t.AtendenteId == Guid.Parse(Usertoken));

            if (_ticket == null)
                return new Retorno { Status = false, Resultado = new List<string> { "Numero do Ticket passado, não esta Vinculado a você" } };

            _ticket.LstRespostas = await _serviceContext.Respostas.Include(c => c.Usuario).Where(c => c.TicketId == _ticket.Id).OrderBy(e => e.DataCadastro).ToListAsync();

            return new Retorno { Status = true, Resultado = _mapper.Map<TicketRetorno>(_ticket) };
        }

        /// <summary>
        /// Método para realizar a busca de todos os tickets com parâmetros de paginação e de status
        /// </summary>
        /// <param name="Usertoken"></param>
        /// <param name="NumeroPagina"></param>
        /// <param name="QuantidadeRegistro"></param>
        /// <param name="StatusAtual"></param>
        /// <returns></returns>
        public async Task<Retorno> BuscarTodosTickets(string Usertoken, int NumeroPagina, int QuantidadeRegistro, string StatusAtual)
        {
            //verifico login.
            if (!Guid.TryParse(Usertoken, out Guid token))
                return new Retorno { Status = false, Resultado = new List<string> { "Autorização Negada!" } };

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

                // busca uma lsita de tickets baseando no status 
                switch (StatusAtual.ToUpper())
                {
                    case "CONCLUIDO":
                        ticketsAtendente = _serviceContext.Tickets.Where(t => t.Status == Status.FECHADO && t.AtendenteId == usuario.Id).ToList();
                        break;

                    case "ANDAMENTO":
                        ticketsAtendente = await _serviceContext.Tickets.Where(t => (t.Status == Status.AGUARDANDO_RESPOSTA_DO_CLIENTE || t.Status == Status.AGUARDANDO_RESPOSTA_DO_ATENDENTE)
                  && t.AtendenteId == usuario.Id).ToListAsync();
                        break;

                    case "ABERTO":
                        ticketsAtendente = await _serviceContext.Tickets.Where(c => c.AtendenteId == null && c.Status == Status.ABERTO).ToListAsync();
                        break;

                    default:
                        return new Retorno { Status = false, Resultado = new List<string> { "Temos três status entre as opções; Aberto, andamento e concluido." } };
                }

                // caso for possivel realizar a paginação se nao for exibo a quantidade padrão = 10, e ordeno pelo mais antigo
                if (NumeroPagina > 0 && QuantidadeRegistro > 0)
                {
                    Paginacao.Paginar(NumeroPagina, QuantidadeRegistro, ticketsAtendente.Count());
                    var listaPaginada = ticketsAtendente.OrderByDescending(d => d.DataCadastro).Skip((NumeroPagina - 1) * QuantidadeRegistro).Take(QuantidadeRegistro).ToList();

                    return listaPaginada.Count() == 0 ? new Retorno
                    {
                        Status = false,
                        Resultado = new List<string>
                    { "Não foi possível realizar a paginação" }
                    } : new Retorno
                    { Status = true, Paginacao = Paginacao, Resultado = _mapper.Map<List<TicketRetorno>>(listaPaginada) };

                }

                Paginacao.Paginar(1, 10, ticketsAtendente.Count());

                return _mapper.Map<List<TicketRetorno>>(ticketsAtendente.Take(10)).Count() == 0 ? new Retorno { Status = false, Resultado = new List<string> { $"Você não tem tickets {StatusAtual} no momento!" } } : new Retorno { Status = true, Paginacao = Paginacao, Resultado = _mapper.Map<List<TicketRetorno>>(ticketsAtendente.Take(10)) };
            }

            // busco pelos tickets daquele especifico usuario 

            List<Ticket> ticketsCliente;

            // busca uma lsita de tickets baseando no status
            switch (StatusAtual.ToUpper())
            {
                case "ABERTO":
                    ticketsCliente = await _serviceContext.Tickets.Where(c => c.Status == Status.ABERTO && c.ClienteId == usuario.Id).ToListAsync();
                    break;

                case "ANDAMENTO":
                    ticketsCliente = await _serviceContext.Tickets.Where(t => (t.Status == Status.AGUARDANDO_RESPOSTA_DO_CLIENTE || t.Status == Status.AGUARDANDO_RESPOSTA_DO_ATENDENTE)
                 && t.ClienteId == usuario.Id).ToListAsync();
                    break;

                case "CONCLUIDO":
                    ticketsCliente = await _serviceContext.Tickets.Where(c => c.Status == Status.FECHADO && c.ClienteId == usuario.Id).ToListAsync();
                    break;

                default:
                    return new Retorno { Status = false, Resultado = new List<string> { "Temos três status entre as opções; Aberto, andamento e concluido." } };
            }

            // caso for possivel realizar a paginação se nao for exibo a quantidade padrão = 10

            if (NumeroPagina > 0 && QuantidadeRegistro > 0)
            {
                Paginacao.Paginar(NumeroPagina, QuantidadeRegistro, ticketsCliente.Count());
                var listaPaginada = ticketsCliente.OrderByDescending(d => d.DataCadastro).Skip((NumeroPagina - 1) * QuantidadeRegistro).Take(QuantidadeRegistro).ToList();
                return listaPaginada.Count() == 0 ? new Retorno
                { Status = false, Resultado = new List<string> { "Não foi possível realizar a paginação." } } :
                new Retorno { Status = true, Paginacao = Paginacao, Resultado = _mapper.Map<List<TicketRetorno>>(listaPaginada) };
            }
            Paginacao.Paginar(1, 10, ticketsCliente.Count());

            return _mapper.Map<List<TicketRetorno>>(ticketsCliente.Take(10)).Count() == 0 ? new Retorno { Status = false, Resultado = new List<string> { $"Você não tem tickets {StatusAtual} no momento!" } } : new Retorno { Status = true, Paginacao = Paginacao, Resultado = _mapper.Map<List<TicketRetorno>>(ticketsCliente.Take(10)) };
        }

        /// <summary>
        /// Método para realizar a posse de um ticket com status aberto
        /// </summary>
        /// <param name="Usertoken"></param>
        /// <param name="numeroTicket"></param>
        public async Task<Retorno> TomarPosseTicket(string Usertoken, string numeroTicket)
        {
            //verifico login.
            if (!Autorizacao.ValidarUsuario(Usertoken, _serviceContext))
                return new Retorno { Status = false, Resultado = new List<string> { "Autorização Negada!" } };

            //verifico se o Ticket ID é valido.
            if (!long.TryParse(numeroTicket, out long numeroDoTicket))
                return new Retorno { Status = false, Resultado = new List<string> { "ticket não identificado!" } };

            //verifico se tem um usuário na base com ID informado e o tipo dele é atendente.
            var atendente = await _serviceContext.Usuarios.SingleOrDefaultAsync(u => u.Id == Guid.Parse(Usertoken) && u.Tipo == "ATENDENTE");
            if (atendente == null) return new Retorno { Status = false, Resultado = new List<string> { "Atendente não identificado!" } };

            //busco e verifico se o ticket solicitado existe na base de dados.

            _ticket = await _serviceContext.Tickets.SingleOrDefaultAsync(t => t.NumeroTicket == numeroDoTicket);

            if (_ticket == null)
                return new Retorno { Status = false, Resultado = new List<string> { "ticket inválido" } };

            if (_ticket.AtendenteId != null) return new Retorno { Status = false, Resultado = new List<string> { "ticket já tem um atendente." } };

            //passo o valor para o ticket

            _ticket.AtendenteId = atendente.Id;
            _ticket.Status = Status.AGUARDANDO_RESPOSTA_DO_ATENDENTE;

            //Salvando 
            await _serviceContext.SaveChangesAsync();
            return new Retorno { Status = true, Resultado = new List<string> { $"{atendente.Nome.ToLower()} você atribuiu esse ticket a sua base." } };
        }

        /// <summary>
        /// metódo para realizar o fechamento do ticket
        /// </summary>
        /// <param name="tokenAutor"></param>
        /// <param name="Fechamento"></param>
        public async Task<Retorno> FecharTicket(string tokenAutor, AvaliacaoView Fechamento)
        {
            //verifico login.
            if (!Autorizacao.ValidarUsuario(tokenAutor, _serviceContext))
                return new Retorno { Status = false, Resultado = new List<string> { "Autorização Negada!" } };

            // verifico se o guid o ticket é valido
            if (!Guid.TryParse(Fechamento.TicketId, out Guid result))
                return new Retorno { Status = false, Resultado = new List<string> { "ticket inválido." } };

            int.TryParse(Fechamento.Avaliacao, out int avaliacao1);
            if (!Enum.IsDefined(typeof(Avaliacao), avaliacao1))
                return new Retorno { Status = false, Resultado = new List<string> { "Avaliação não válida!" } };

            // busco e valido se este ticket em especifico é valido.
             _ticket = await _serviceContext.Tickets.SingleOrDefaultAsync(c => c.Id == result && c.ClienteId == Guid.Parse(tokenAutor));

            // validações para a reliazação do fechamento
            if (_ticket == null)
                return new Retorno { Status = false, Resultado = new List<string> { "ticket inválido." } };

            if (_ticket.Status == Status.FECHADO)
                return new Retorno { Status = false, Resultado = new List<string> { "Este ticket já foi fechado!" } };

            if (_ticket.AtendenteId == null)
                return new Retorno { Status = false, Resultado = new List<string> { "Não é possível fechar um ticket que não foi atendido." } };

            // atribuo e fecho o ticket
            _ticket.Status = Status.FECHADO;
            _ticket.Avaliacao = Enum.Parse<Avaliacao>(Fechamento.Avaliacao);


            await _serviceContext.SaveChangesAsync();

            return new Retorno { Status = true, Resultado = new List<string> { "ticket fechado com sucesso!" } };
        }
        /// <summary>
        /// metodo para criar uma identificação unica de cada usuário.
        /// </summary>
        /// <returns></returns>
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