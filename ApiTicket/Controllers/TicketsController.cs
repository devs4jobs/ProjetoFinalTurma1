using AutoMapper;
using Core;
using Microsoft.AspNetCore.Mvc;
using Model;
namespace ApiForum.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        //Construtor contendo o contexto.
        private ServiceContext _contexto { get; set; }
        private readonly IMapper _Mapper;

        // construtor para a utilização do automapper por meio de injeçao de dependecia
        public TicketsController(ServiceContext contexto, IMapper mapper) { _contexto = contexto; _Mapper = mapper; }


        /// <summary>
        /// Criar Ticket
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///	    {
        ///  		"titulo": "Meu Computador se encontra com  problemas.",
        ///  		"mensagem": "Oi meu comprei meu computador recentemente e está com defeitos.",
        ///	    }
        ///
        /// </remarks>
        /// <param name="ticket"></param>
        /// <param name="autorToken"></param>
        /// <returns>Retorna Status de cadastro do ticket</returns>
        [HttpPost]
        public IActionResult CadastrarTicket([FromBody] TicketView ticket, [FromHeader] string autorToken)
        {
            var Core = new TicketCore(ticket, _contexto, _Mapper).CadastrarTicket(autorToken);
            return Core.Status ? Created($"{HttpContext.Request.Host}{HttpContext.Request.Path}", Core) : (IActionResult)Ok(Core);
        }

        /// <summary>
        /// TomarPosseDoTicket
        /// </summary>
        /// <param name="numeroTicket"></param>
        /// <param name="autorToken"></param>
        [HttpPost("PegarTicket/{numeroTicket}")]
        public IActionResult TomarPosseDoTicket(string numeroTicket, [FromHeader] string autorToken)
        {
            var Core = new TicketCore(_contexto).TomarPosseTicket(autorToken, numeroTicket);
            return Core.Status ? Ok(Core) : Ok(Core);
        }

        /// <summary>
        /// ProcurarTicketPorId
        /// </summary>
        /// <param name="autorToken"></param>
        /// <param name="TicketID"></param>
        /// <returns>Retorna ticket que possui o Id inserido.</returns>
        [HttpGet("{TicketID}")]
        public IActionResult ProcurarTicketPorId([FromHeader]string autorToken, string TicketID)
        {
            var Core = new TicketCore(_Mapper, _contexto).BuscarTicketporID(autorToken, TicketID);
            return Core.Status ? Ok(Core) : Ok(Core);
        }

        /// <summary>
        /// Busco todos Tickets
        /// </summary>
        /// <param name="autorToken"></param>
        /// <param name="numeroPagina"></param>
        /// <param name="quantidadePagina"></param>
        /// <param name="StatusAtual"></param>
        /// <returns>Retorna Tickets com filtros dos parametros inseridos e a autorização do usuário.</returns>
        [HttpGet("Todos/{StatusAtual}")]
        public IActionResult BuscarTodosTickets([FromHeader]string autorToken, [FromQuery] int numeroPagina, [FromQuery]int quantidadePagina, string StatusAtual)
        {
            var Core = new TicketCore(_contexto).BuscarTodosTickets(autorToken, numeroPagina, quantidadePagina, StatusAtual);
            return Core.Status ? Ok(Core) : Ok(Core);
        }

        /// <summary>
        /// atualizar Ticket
        /// </summary>
        /// <remarks>
        ///     Sample request:
        ///       
        ///         {
        ///             "titulo": "Meu Computador se encontra com problemas há 2 Dias.",
        ///             "mensagem": "Oi meu comprei meu computador recentemente e está com defeitos na inicialização.",
        ///             "avaliacao": 5,
        ///             "status": 1
        ///         }
        ///         
        /// </remarks>
        /// <param name="ticket"></param>
        /// <param name="TicketID"></param>
        /// <param name="autorToken"></param>
        [HttpPut("{TicketID}")]
        public IActionResult AtualizarTicketId([FromHeader]string autorToken, string TicketID, [FromBody] TicketUpadateView ticket)
        {
            var Core = new TicketCore(_Mapper, _contexto).AtualizarTicket(autorToken, TicketID, ticket);
            return Core.Status ? Accepted(Core) : (IActionResult)Ok(Core);
        }

        /// <summary>
        /// Deletar Ticket por Id
        /// </summary>
        /// <param name="autorToken"></param>
        /// <param name="TicketID"></param>
        /// <returns>Retorno uma mensagem se foi ou não apagado o Ticket.</returns>
        [HttpDelete("{TicketID}")]
        public IActionResult DeletarTicketId([FromHeader]string autorToken, string TicketID)
        {
            var Core = new TicketCore(_contexto).DeletarTicket(autorToken, TicketID);
            return Core.Status ? Ok(Core) : Ok(Core);
        }

        /// <summary>
        /// O Cliente Avalia o Atendente.
        /// </summary>
        /// <param name="autorToken"></param>
        /// <param name="TicketID"></param>
        /// <param name="avaliacao"></param>
        /// <returns>Retorna mensagem de Status da Avaliação.</returns>
        [HttpPost("Avaliar/{TicketID}/{avaliacao}")]
        public IActionResult AvaliarTicket([FromHeader]string autorToken, string TicketID, string avaliacao)
        {
            var Core = new TicketCore(_contexto).AvaliarTicket(autorToken, TicketID, avaliacao);
            return Core.Status ? Ok(Core) : Ok(Core);
        }

        /// <summary>
        /// Cliente fecha o Ticket.
        /// </summary>
        /// <param name="autorToken"></param>
        /// <param name="TicketID"></param>
        /// <returns>Retorno uma mensagem de Status para o Cliente.</returns>
        [HttpPost("Fechar")]
        public IActionResult FecharTicket([FromHeader]string autorToken, string TicketID)
        {
            var Core = new TicketCore(_contexto).FecharTicket(autorToken, TicketID);
            return Core.Status ? Ok(Core) : Ok(Core);
        }

        /// <summary>
        /// Troco o Atendente que está atendendo o Ticket.
        /// </summary>
        /// <param name="numeroTicket"></param>
        /// <param name="autorToken"></param>
        /// <returns>Retorno o Status da troca se foi ou não bem sucedida.</returns>
        [HttpPost("TrocarAtendente/{numeroTicket}")]
        public IActionResult TrocarAtendente( string numeroTicket, [FromHeader]string autorToken)
        {
            var Core = new TicketCore(_contexto).TrocarAtendente(numeroTicket, autorToken);
            return Core.Status ? Ok(Core) : Ok(Core);
        }

    }
}