using AutoMapper;
using Core;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace ApiForum.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        //Construtor contendo o contexto.
        private ServiceContext _contexto { get; set; }
        private readonly IMapper _Mapper;

        // construtor para a utilização do automapper por meio de injeçao de dependecia
        public TicketsController(ServiceContext contexto, IMapper mapper) { _contexto = contexto; _Mapper = mapper; }

        //Chamando o metodo de cadastar usurario da core 
        [HttpPost]
        public IActionResult CadastrarTicket([FromBody] TicketView ticket, [FromHeader] string autorToken)
        {
            var Core = new TicketCore(ticket, _contexto, _Mapper).CadastrarTicket(autorToken);
            return Core.Status ? Created($"{HttpContext.Request.Host}{HttpContext.Request.Path}", Core) : (IActionResult)Ok(Core);
        }

        [HttpPost("PegarTicket/{numeroTicket}")]
        public IActionResult TomarPosseDoTicket(string numeroTicket, [FromHeader] string autorToken)
        {
            var Core = new TicketCore(_contexto).TomarPosseTicket(autorToken, numeroTicket);
            return Core.Status ? Ok(Core) : Ok(Core);
        }

        [HttpGet("{TicketID}")]

        public IActionResult ProcurarTicketPorId([FromHeader]string autorToken, string TicketID)
        {
            var Core = new TicketCore(_Mapper, _contexto).BuscarTicketporID(autorToken, TicketID);
            return Core.Status ? Ok(Core) : Ok(Core);
        }

        //Chamando o metodo de listar todos da core 
        [HttpGet("Todos/{StatusAtual}")]
        public IActionResult BuscarTodosTickets([FromHeader]string autorToken, [FromQuery] int numeroPagina, [FromQuery]int quantidadePagina, string StatusAtual)
        {
            var Core = new TicketCore(_Mapper,_contexto).BuscarTodosTickets(autorToken, 0, 0, StatusAtual);
            return Core.Status ? Ok(Core) : Ok(Core);
        }

        [HttpPut("{TicketID}")]
        public IActionResult AtualizarTicketId([FromHeader]string autorToken, string TicketID, [FromBody] TicketUpadateView ticket)
        {
            var Core = new TicketCore(_Mapper, _contexto).AtualizarTicket(autorToken, TicketID, ticket);
            return Core.Status ? Accepted(Core) : (IActionResult)Ok(Core);
        }

        [HttpDelete("{TicketID}")]
        public IActionResult DeletarTicketId([FromHeader]string autorToken, string TicketID)
        {
            var Core = new TicketCore(_contexto).DeletarTicket(autorToken, TicketID);
            return Core.Status ? Ok(Core) : Ok(Core);
        }

        [HttpPost("Avaliar/{TicketID}/{avaliacao}")]
        public IActionResult AvaliarTicket([FromHeader]string autorToken, string TicketID, string avaliacao)
        {
            var Core = new TicketCore(_contexto).AvaliarTicket(autorToken, TicketID, avaliacao);
            return Core.Status ? Ok(Core) : Ok(Core);
        }

        [HttpPost("Fechar")]
        public IActionResult FecharTicket([FromHeader]string autorToken, string TicketID)
        {
            var Core = new TicketCore(_contexto).FecharTicket(autorToken, TicketID);
            return Core.Status ? Ok(Core) : Ok(Core);
        }

        [HttpPost("TrocarAtendente/{numeroTicket}")]
        public IActionResult TrocarAtendente( string numeroTicket, [FromHeader]string autorToken)
        {
            var Core = new TicketCore(_contexto).TrocarAtendente(numeroTicket, autorToken);
            return Core.Status ? Ok(Core) : Ok(Core);
        }

    }
}