using AutoMapper;
using Core;
using Microsoft.AspNetCore.Mvc;
using Model;
using System.Threading.Tasks;

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
        ///  		"mensagem": "Oi meu comprei meu computador recentemente e está com defeitos."
        ///	    }
        ///
        /// </remarks>
        /// <param name="ticket"></param>
        /// <param name="autorToken"></param>
        /// <returns>Retorna Status de cadastro do ticket</returns>
        [HttpPost]
        public async Task<IActionResult> CadastrarTicket([FromBody] TicketView ticket, [FromHeader] string autorToken)
        {
            var Core = new TicketCore(ticket, _contexto, _Mapper);
            var result = await Core.CadastrarTicket(autorToken);
            return result.Status ? Created($"{HttpContext.Request.Host}{HttpContext.Request.Path}", result) : (IActionResult)Ok(result);
        }

        /// <summary>
        /// TomarPosseDoTicket
        /// </summary>
        /// <param name="numeroTicket"></param>
        /// <param name="autorToken"></param>
        [HttpPost("PegarTicket/{numeroTicket}")]
        public async Task<IActionResult> TomarPosseDoTicket(string numeroTicket, [FromHeader] string autorToken)
        {
            var Core = new TicketCore(_contexto);
            var result = await Core.TomarPosseTicket(autorToken, numeroTicket);
            return result.Status ? Ok(result) : Ok(result);
        }

        /// <summary>
        /// Procurar Ticket pelo numero dele.
        /// </summary>
        /// <param name="autorToken"></param>
        /// <param name="NumeroTicket"></param>
        /// <returns>Retorna ticket que possui o Id inserido.</returns>
        [HttpGet("{NumeroTicket}")]
        public async Task<IActionResult> ProcurarTicketPorId([FromHeader]string autorToken, string NumeroTicket)

        {
            var Core = new TicketCore(_Mapper, _contexto);
            var result = await Core.BuscarTicketporNumeroDoTicket(autorToken, NumeroTicket);
            return result.Status ? Ok(result) : Ok(result);
        }

        /// <summary>
        /// Buscar todos Tickets
        /// </summary>
        /// <param name="autorToken"></param>
        /// <param name="numeroPagina"></param>
        /// <param name="quantidadePagina"></param>
        /// <param name="StatusAtual"></param>
        /// <returns>Retorna Tickets com filtros dos parametros inseridos e a autorização do usuário.</returns>
        [HttpGet("Todos/{StatusAtual}")]
        public async Task<IActionResult> BuscarTodosTickets([FromHeader]string autorToken, [FromQuery] int numeroPagina, [FromQuery]int quantidadePagina, string StatusAtual)
        {
            var Core = new TicketCore(_Mapper, _contexto);
            var result = await Core.BuscarTodosTickets(autorToken, numeroPagina, quantidadePagina, StatusAtual);
            return result.Status ? Ok(result) : Ok(result);
        }

        /// <summary>
        /// atualizar Ticket
        /// </summary>
        /// <remarks>
        ///     Sample request:
        ///       
        ///         {
        ///             "titulo": "Meu Computador se encontra com problemas há 2 Dias.",
        ///             "mensagem": "Oi meu comprei meu computador recentemente e está com defeitos na inicialização."
        ///         }
        ///         
        /// </remarks>
        /// <param name="ticket"></param>
        /// <param name="TicketID"></param>
        /// <param name="autorToken"></param>
        [HttpPut("{TicketID}")]
        public async Task<IActionResult> AtualizarTicketId([FromHeader]string autorToken, string TicketID, [FromBody] TicketView ticket)
        {
            var Core = new TicketCore(_Mapper, _contexto);
            var result = await Core.AtualizarTicket(autorToken, TicketID, ticket);
            return result.Status ? Accepted(result) : (IActionResult)Ok(result);
        }

        /// <summary>
        /// Deletar Ticket por Id
        /// </summary>
        /// <param name="autorToken"></param>
        /// <param name="TicketID"></param>
        /// <returns>Retorno uma mensagem se foi ou não apagado o Ticket.</returns>
        [HttpDelete("{TicketID}")]
        public async Task<IActionResult> DeletarTicketId([FromHeader]string autorToken, string TicketID)
        {
            var Core = new TicketCore(_contexto);
            var result = await Core.DeletarTicket(autorToken, TicketID);
            return result.Status ? Ok(result) : Ok(result);
        }

        /// <summary>
        /// O Cliente Avalia o Atendente.
        /// </summary>
        /// <param name="autorToken"></param>
        /// <param name="TicketID"></param>
        /// <param name="avaliacao"></param>
        /// <returns>Retorna mensagem de Status da Avaliação.</returns>
        [HttpPost("Avaliar/{TicketID}/{avaliacao}")]
        public async Task<IActionResult> Avaliar([FromHeader]string autorToken, string TicketID, string avaliacao)
        {
            var Core = new TicketCore(_contexto);
            var result = await  Core.AvaliarTicket(autorToken, TicketID, avaliacao);
            return result.Status ? Ok(result) : Ok(result);
        }

        /// <summary>
        /// Cliente fecha o Ticket.
        /// </summary>
        /// <param name="autorToken"></param>
        /// <param name="TicketID"></param>
        /// <returns>Retorno uma mensagem de Status para o Cliente.</returns>
        [HttpPost("Fechar")]
        public async Task<IActionResult> FecharTicket(string autorToken, string TicketID)
        {
            var Core = new TicketCore(_contexto);
            var result = await Core.FecharTicket(autorToken, TicketID);
            return result.Status ? Ok(result) : Ok(result);
        }

        /// <summary>
        /// Trocar o Atendente que está atendendo o Ticket.
        /// </summary>
        /// <param name="numeroTicket"></param>
        /// <param name="autorToken"></param>
        /// <returns>Retorno o Status da troca se foi ou não bem sucedida.</returns>
        [HttpPost("TrocarAtendente/{numeroTicket}")]
        public async Task<IActionResult> TrocarAtendente(string numeroTicket, [FromHeader]string autorToken)
        {
            var Core = new TicketCore(_contexto);
            var result = await Core.TrocarAtendente(numeroTicket, autorToken);
            return result.Status ? Ok(result) : Ok(result);
        }
    }
}