﻿using Core;
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

        // construtor para a utilização do automapper por meio de injeçao de dependecia
        public TicketsController(ServiceContext contexto) => _contexto = contexto;

        //Chamando o metodo de cadastar usurario da core 
        [HttpPost]
        public IActionResult CadastrarTicket([FromBody] Ticket ticket, [FromHeader] string Usertoken)
        {
            var Core = new TicketCore(ticket, _contexto).CadastrarTicket(Usertoken);
            return Core.Status ? Ok(Core) : (IActionResult)BadRequest(Core);
        }

        [HttpGet("{id}")]
      
        public IActionResult GetIdTicket([FromHeader]string Usertoken, string TicketID)
        {
            var Core = new TicketCore(_contexto).BuscarTicketporID(Usertoken, TicketID);
            return Core.Status ? Ok(Core) : (IActionResult)BadRequest(Core);
        }

        //Chamando o metodo de listar todos da core 
        [HttpGet]
        public IActionResult GetTodosTickets([FromHeader]string Usertoken)
        {
            var Core = new TicketCore(_contexto).BuscarTodosTickets(Usertoken);
            return Core.Status ? Ok(Core) : (IActionResult)BadRequest(Core);
        }

        [HttpPut("{TicketID}")]
        public IActionResult AtualizarTicketId([FromHeader]string Usertoken, string TicketID,[FromBody] Ticket ticket)
        {
            var Core = new TicketCore(_contexto).AtualizarTicket(Usertoken, TicketID, ticket);
            return Core.Status ? Ok(Core) : (IActionResult)BadRequest(Core);
        }

        [HttpDelete("{TicketID}")]
        public IActionResult DeletarTicketId([FromHeader]string Usertoken, string TicketID)
        {
            var Core = new TicketCore(_contexto).DeletarTicket(Usertoken, TicketID);
            return Core.Status ? Ok(Core) : (IActionResult)BadRequest(Core);
        }
    }
}