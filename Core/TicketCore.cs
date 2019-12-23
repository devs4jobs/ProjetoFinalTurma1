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
       
        #region Contrutores
        public TicketCore(ServiceContext serviceContext) => _serviceContext = serviceContext;
        public TicketCore(IMapper mapper, ServiceContext serviceContext)
        {
            _mapper = mapper;
            _serviceContext = serviceContext;

            RuleSet("Fechar", () =>
            {
                RuleFor(x => x.Status).Must(x => x != Status.FECHADO).WithMessage("Este ticket já foi fechado!");
                RuleFor(x => x.AtendenteId).Must(x => x != null).WithMessage("Não é possível fechar um ticket que não foi atendido.");
            });

            RuleSet("Atualizar", () =>
            {
                RuleFor(x => x.LstRespostas).Must(x => x == null).WithMessage("Como o ticket já contem respostas, não é mais possivel atualiza-lo");
                RuleFor(x => x.Status).Must(x => x != Status.FECHADO).WithMessage("Não se pode atualizar tickets já encerrados.");
            });
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
        #endregion

        #region Regras de Negocio
        /// <summary>
        /// Método para realizar o cadastro de um ticket
        /// </summary>
        /// <param name="Usertoken"></param>
        public async Task<Retorno> CadastrarTicket(string Usertoken)
        {
            //verifico login.
            if (!Autorizacao.ValidarUsuario(Usertoken, _serviceContext))
                return new Retorno {  Resultado = new List<string> { "Autorização Negada!" } };

            _ticket.ClienteId = Guid.Parse(Usertoken);

            //verifico ticket se é valido.
            var validar = Validate(_ticket);
            if (!validar.IsValid) return new Retorno { Resultado = validar.Errors.Select(e => e.ErrorMessage).ToList() };

            //busco o cliente na base e verifico.
            var cliente = await _serviceContext.Usuarios.SingleOrDefaultAsync(u => u.Id == _ticket.ClienteId);

            if (cliente.Tipo != "CLIENTE") return new Retorno {  Resultado = new List<string> { "Usuário não é do tipo cliente" } };

            //Atribuição do numero do ticket
            _ticket.NumeroTicket = ConvertNumeroTickets();

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

            try
            {
                //busco pelo ticket e efetuo as validacoes
                _ticket = await _serviceContext.Tickets.Include(o => o.LstRespostas).SingleAsync(t => t.Id == Guid.Parse(TicketID));

                if (_ticket.ClienteId != Guid.Parse(Usertoken)) return new Retorno { Resultado = new List<string> { "Usuario não é o dono do ticket." } };

                var Validate = this.Validate(ticketView, ruleSet: "Atualizar");
                if (!Validate.IsValid) return new Retorno { Resultado = Validate.Errors.Select(x => x.ErrorMessage) };

                // mapeando o retorno
                _mapper.Map(ticketView, _ticket);
                await _serviceContext.SaveChangesAsync();

                return new Retorno { Status = true, Resultado = _mapper.Map<TicketRetorno>(_ticket) };
            }
            catch (Exception)
            {
                return new Retorno { Resultado = new List<string> { "Ticket não identificado." } };
            }
        }

        /// <summary>
        /// Método para deletar um ticket da base
        /// </summary>
        /// <param name="Usertoken"></param>
        /// <param name="TicketID"></param>
        public async Task<Retorno> DeletarTicket(string Usertoken, string TicketID)
        {
            //verifico login.
            if (!Autorizacao.ValidarUsuario(Usertoken, _serviceContext))
                return new Retorno { Status = false, Resultado = new List<string> { "Autorização Negada!" } };
            try
            {
                _ticket = await _serviceContext.Tickets.Include(c => c.LstRespostas).SingleAsync(t => t.Id == Guid.Parse(TicketID));

                //vejo se o cliente que ta logado é o mesmo que está públicou o ticket.
                if (Guid.Parse(Usertoken) != _ticket.ClienteId) return new Retorno { Resultado = new List<string> { "Usuário não pode deletar esse ticket, pois não é quem postou o mesmo!" } };

                //tento excluir o ticket e salvar as  alterações.
                if (_ticket.LstRespostas.Count() != 0) return new Retorno { Resultado = new List<string> { "Não é possivel remover este ticket, pois ele ja tem respostas!" } };

                // Remoção do ticket
                _ticket.VisualizarTicket = false;
                await _serviceContext.SaveChangesAsync();

                return new Retorno { Status = true, Resultado = new List<string> { $"{_ticket.Cliente.Nome.ToLower()} seu ticket foi deletado com Sucesso!" } };
            }
            catch (Exception)
            {
                return new Retorno { Resultado = new List<string> { "ticket não identificado!" } };
            }
        }

        /// <summary>
        /// Método para realizar a busca de um ticket pelo numero
        /// </summary>
        /// <param name="Usertoken"></param>
        /// <param name="NumeroTicketQueVem"></param>
        public async Task<Retorno> BuscarTicketPorNumeroDoTicket(string Usertoken, string NumeroTicketQueVem)
        {

            if (!Autorizacao.ValidarUsuario(Usertoken, _serviceContext))
                return new Retorno { Resultado = new List<string> { "Autorização Negada!" } };
            
            try
            {
                _ticket = await _serviceContext.Tickets.SingleAsync(t => (t.NumeroTicket == long.Parse(NumeroTicketQueVem) && t.ClienteId == Guid.Parse(Usertoken)) || (t.NumeroTicket == long.Parse(NumeroTicketQueVem) && t.AtendenteId == Guid.Parse(Usertoken))&&t.VisualizarTicket);

                _ticket.LstRespostas = await (from a in _serviceContext.Respostas.Where(x=>x.TicketId==_ticket.Id)
                                              from c in _serviceContext.Usuarios.Where(x=>a.UsuarioId==x.Id)
                                              from b in _serviceContext.Anexos.Where(x=>x.RespostaId==a.Id).DefaultIfEmpty() 
                                              select new Resposta
                                              {
                                                  DataCadastro = a.DataCadastro,
                                                  Mensagem = a.Mensagem,
                                                  Usuario = new Usuario { Email = c.Email, Nome = c.Nome, Tipo = c.Tipo },
                                                  Anexo = new Anexo { DataCadastro=b.DataCadastro,NomeArquivo=b.NomeArquivo,RespostaId=b.RespostaId}
                                              }).ToListAsync();
                    
                    //_serviceContext.Respostas.Include(c => c.Usuario).Where(c => c.TicketId == _ticket.Id).OrderBy(e => e.DataCadastro).ToListAsync();

                return new Retorno { Status = true, Resultado = _mapper.Map<TicketRetorno>(_ticket) };
            }
            catch (FormatException)
            {
                 return new Retorno { Resultado = new List<string> { "Número do ticket incorreto!" } };
            }
            catch (Exception)
            {
                return new Retorno { Resultado = new List<string> { "Ticket não existe" } };
            }
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
            if (!Autorizacao.ValidarUsuario(Usertoken, _serviceContext))
                return new Retorno { Resultado = new List<string> { "Autorização Negada!" } };

            //busco pelo usuario e vejo se ele existe.
            var usuario = await _serviceContext.Usuarios.SingleAsync(u => u.Id == Guid.Parse(Usertoken));
            
            List<Ticket> Tickets;
            //Confiro o tipo do usuario e exibo os resultados paginados de acordo com o tipo do usuario
            if (usuario.Tipo == "ATENDENTE")
            {
                // busca uma lsita de tickets baseando no status 
                switch (StatusAtual.ToUpper())
                {
                    case "CONCLUIDO":
                        Tickets = await _serviceContext.Tickets.Where(t => t.Status == Status.FECHADO && t.AtendenteId == usuario.Id && t.VisualizarTicket).ToListAsync();
                        break;

                    case "ANDAMENTO":
                        Tickets = await _serviceContext.Tickets.Where(t => (t.Status == Status.AGUARDANDO_RESPOSTA_DO_CLIENTE || t.Status == Status.AGUARDANDO_RESPOSTA_DO_ATENDENTE)
                  && t.AtendenteId == usuario.Id && t.VisualizarTicket).ToListAsync();
                        break;

                    case "ABERTO":
                        Tickets = await _serviceContext.Tickets.Where(c => c.AtendenteId == null && c.Status == Status.ABERTO && c.VisualizarTicket).ToListAsync();
                        break;

                    default:
                        return new Retorno { Status = false, Resultado = new List<string> { "Temos três status entre as opções; Aberto, andamento e concluido." } };
                }
            }
            else
            {
                // busca uma lista de tickets baseando no status
                switch (StatusAtual.ToUpper())
                {
                    case "ABERTO":
                        Tickets = await _serviceContext.Tickets.Where(c => c.Status == Status.ABERTO && c.ClienteId == usuario.Id && c.VisualizarTicket).ToListAsync();
                        break;

                    case "ANDAMENTO":
                        Tickets = await _serviceContext.Tickets.Where(t => (t.Status == Status.AGUARDANDO_RESPOSTA_DO_CLIENTE || t.Status == Status.AGUARDANDO_RESPOSTA_DO_ATENDENTE)
                     && t.ClienteId == usuario.Id && t.VisualizarTicket).ToListAsync();
                        break;

                    case "CONCLUIDO":
                        Tickets = await _serviceContext.Tickets.Where(c => c.Status == Status.FECHADO && c.ClienteId == usuario.Id && c.VisualizarTicket).ToListAsync();
                        break;

                    default:
                        return new Retorno { Status = false, Resultado = new List<string> { "Temos três status entre as opções; Aberto, andamento e concluido." } };
                }
            }

            // caso for possivel realizar a paginação se nao for exibo a quantidade padrão = 10
            var Paginacao = new Paginacao();

            if (NumeroPagina > 0 && QuantidadeRegistro > 0)
            {
                Paginacao.Paginar(NumeroPagina, QuantidadeRegistro,Tickets.Count());
                return Paginacao.PaginaAtual > Paginacao.TotalPaginas ? new Retorno { Resultado = new List<string> { $"Você não tem tickets {StatusAtual}s no momento!" } }
                : new Retorno { Status = true, Paginacao = Paginacao, Resultado = _mapper.Map<List<TicketRetorno>>(Tickets.OrderByDescending(d => d.DataCadastro).Skip((NumeroPagina - 1) * QuantidadeRegistro).Take(QuantidadeRegistro)) };
            }
            Paginacao.Paginar(1, 10, Tickets.Count());

            return Paginacao.PaginaAtual > Paginacao.TotalPaginas ? new Retorno { Resultado = new List<string> { $"Você não tem tickets {StatusAtual}s no momento!" } }
            : new Retorno { Status = true, Paginacao = Paginacao, Resultado = _mapper.Map<List<TicketRetorno>>(Tickets.OrderByDescending(d => d.DataCadastro).Take(10)) };
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
                return new Retorno { Resultado = new List<string> { "Autorização Negada!" } };

            try
            {
                //verifico se tem um usuário na base com ID informado e o tipo dele é atendente.
                var atendente = await _serviceContext.Usuarios.SingleOrDefaultAsync(u => u.Id == Guid.Parse(Usertoken) && u.Tipo == "ATENDENTE");
                if (atendente == null) return new Retorno { Resultado = new List<string> { "Atendente não identificado!" } };

                //busco e verifico se o ticket solicitado existe na base de dados.
                _ticket = await _serviceContext.Tickets.SingleOrDefaultAsync(t => t.NumeroTicket == long.Parse(numeroTicket));

                if (_ticket.AtendenteId != null) return new Retorno { Resultado = new List<string> { "ticket já tem um atendente." } };

                //passo o valor para o ticket
                _ticket.AtendenteId = atendente.Id;
                _ticket.Status = Status.AGUARDANDO_RESPOSTA_DO_ATENDENTE;

                //Salvando 
                await _serviceContext.SaveChangesAsync();
                return new Retorno { Status = true, Resultado = new List<string> { $"{atendente.Nome.ToLower()} você atribuiu esse ticket a sua base." } };
            }
            catch (FormatException)
            {
                return new Retorno { Resultado = new List<string> { "Numero do ticket formato incorreto" } };
            }
            catch (NullReferenceException)
            {
                return new Retorno { Resultado = new List<string> { "Ticket não existe!!!" } };
            }
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
                return new Retorno { Resultado = new List<string> { "Autorização Negada!" } };

            if (!Enum.IsDefined(typeof(Avaliacao), Fechamento.Avaliacao))
                return new Retorno { Resultado = new List<string> { "Avaliação não válida!" } };
            try
            {
                // busco e valido se este ticket em especifico é valido.
                _ticket = await _serviceContext.Tickets.SingleOrDefaultAsync(c => c.Id == Guid.Parse(Fechamento.TicketId) && c.ClienteId == Guid.Parse(tokenAutor));

                // validações para a realização do fechamento
                var Validate = this.Validate(_ticket, ruleSet: "Fechar");
                if (!Validate.IsValid) return new Retorno { Resultado = Validate.Errors.Select(c => c.ErrorMessage) };

                // atribuo e fecho o ticket
                _ticket.Status = Status.FECHADO;
                _ticket.Avaliacao = Fechamento.Avaliacao;
                    //Enum.Parse<Avaliacao>(Fechamento.Avaliacao);
                await _serviceContext.SaveChangesAsync();

                return new Retorno { Status = true, Resultado = new List<string> { "ticket fechado com sucesso!" } };
            }
            catch (Exception)
            {
                return new Retorno { Resultado = new List<string> { "Ticket não encontrado" } };
            }
        }
        #endregion

        #region Metodos Internos
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

                //aqui retornamos o mes e o ano junto com o resultado dos calculos.
                return long.Parse(dataString + number.ToString().Substring(6));
            }
            catch (Exception) { return long.Parse(dataString + 1.ToString("D6")); }
        }

        #endregion
    }
}