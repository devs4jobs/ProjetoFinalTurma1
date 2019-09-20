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
        private IMapper _Mapper { get; set; }

        // construtor para a utilização do automapper por meio de injeçao de dependecia
        public TicketsController(ServiceContext contexto) => _contexto = contexto;

        //Chamando o metodo de cadastar usurario da core 
        [HttpPost]
        public IActionResult CadastrarTicket([FromBody] Ticket ticket, [FromHeader] string autorToken)
        {
            var Core = new TicketCore(ticket, _contexto).CadastrarTicket(autorToken).Result;
            return Core.Status ? Created($"{HttpContext.Request.Host}{HttpContext.Request.Path}", Core) : (IActionResult)Ok(Core);
        }

        [HttpPost]
        public IActionResult TomarPosseDoTicket([FromBody]string TicketID, [FromHeader] string autorToken)
        {
            var Core = new TicketCore(_contexto).TomarPosseTicket(autorToken, TicketID);
            return Core.Status ? Ok(Core) : Ok(Core);
        }

        [HttpGet("{TicketID}")]
      
        public IActionResult GetIdTicket([FromHeader]string autorToken, string TicketID)
        {
            var Core = new TicketCore(_contexto).BuscarTicketporID(autorToken, TicketID);
            return Core.Status ? Ok(Core) : Ok(Core);
        }

        //Chamando o metodo de listar todos da core 
        [HttpGet]
        public IActionResult GetTodosTickets([FromHeader]string autorToken,[FromQuery] int numeroPagina,[FromQuery]int quantidadePagina)
        {
            var Core = new TicketCore(_contexto).BuscarTodosTickets(autorToken, numeroPagina, quantidadePagina);
            return Core.Status ? Ok(Core) : Ok(Core);
        }

        //Chamando o metodo de listar todos da core 
        [HttpGet("Tickets")]
        public IActionResult GetTicketsDisponiveis([FromHeader]string autorToken, [FromQuery] int numeroPagina, [FromQuery]int quantidadePagina)
        {
            var Core = new TicketCore(_contexto).BuscarTicketSemAtendente(autorToken, numeroPagina, quantidadePagina);
            return Core.Status ? Ok(Core) : Ok(Core);
        }

        [HttpPut("{TicketID}")]
        public IActionResult AtualizarTicketId([FromHeader]string autorToken, string TicketID,[FromBody] Ticket ticket)
        {
            var Core = new TicketCore(_Mapper,_contexto).AtualizarTicket(autorToken, TicketID, ticket);
            return Core.Status ? Accepted(Core) : (IActionResult)Ok(Core);
        }

        [HttpDelete("{TicketID}")]
        public IActionResult DeletarTicketId([FromHeader]string autorToken, string TicketID)
        {
            var Core = new TicketCore(_contexto).DeletarTicket(autorToken, TicketID);
            return Core.Status ? Ok(Core) : Ok(Core);
        }
    }
}