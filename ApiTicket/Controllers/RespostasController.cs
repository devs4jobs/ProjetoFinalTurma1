﻿using Core;
using Microsoft.AspNetCore.Mvc;
using Model;
using Microsoft.EntityFrameworkCore;


namespace ApiForum.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RespostasController : ControllerBase
    {
        //Construtor contendo o contexto.
        private ServiceContext _contexto { get; set; }

        // construtor para a utilização do automapper por meio de injeçao de dependecia
        public RespostasController(ServiceContext contexto) => _contexto = contexto;

        //Chamando o metodo de cadastar usurario da core 
        [HttpPost]
        public IActionResult CadastrarTicket([FromBody] Resposta resposta, [FromHeader] string autorToken)
        {
            var Core = new RespostaCore(resposta, _contexto).CadastrarResposta(autorToken);
            return Core.Status ? Created("https://localhost:44362//api/Respostas/", Core) : (IActionResult)Ok(Core);
        }

        [HttpGet("{TicketId}")]
        public IActionResult GetIdTicket(string TokenAutor, string TicketId)
        {
            var Core = new RespostaCore(_contexto).BuscarRespostas(TokenAutor, TicketId);
            return Core.Status ? Ok(Core) : Ok(Core);
        }

        [HttpPut("{TicketId}")]
        public IActionResult AtualizarTicketId(string TokenAutor, string TicketId, Resposta resposta)
        {
            var Core = new RespostaCore(_contexto).EditarResposta(TokenAutor, TicketId, resposta);
            return Core.Status ? Ok(Core) : Ok(Core);
        }

        [HttpDelete("{TicketId}")]
        public IActionResult DeletarTicketId([FromHeader]string TokenAutor, string TicketId)
        {
            var Core = new RespostaCore(_contexto).DeletarResposta(TokenAutor, TicketId);
            return Core.Status ? Ok(Core) : Ok(Core);
        }
    }
}