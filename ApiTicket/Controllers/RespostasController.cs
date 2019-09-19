using Core;
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
        public IActionResult CadastrarResposta([FromBody] Resposta resposta, [FromHeader] string autorToken)
        {
            var Core = new RespostaCore(resposta, _contexto).CadastrarResposta(autorToken);
            return Core.Status ? Created($"{HttpContext.Request.Host}{HttpContext.Request.Path}", Core): (IActionResult)Ok(Core);
        }

        [HttpGet("{TicketId}")]
        public IActionResult GetIdResposta([FromHeader]string autorToken, string TicketId)
        {
            var Core = new RespostaCore(_contexto).BuscarRespostas(autorToken, TicketId);
            return Core.Status ? Ok(Core) : Ok(Core);
        }

        [HttpPut("{TicketId}")]
        public IActionResult AtualizarRespostaId([FromHeader]string autorToken, string TicketId, Resposta resposta)
        {
            var Core = new RespostaCore(_contexto).EditarResposta(autorToken, TicketId, resposta);
            return Core.Status ? Accepted(Core) : (IActionResult)Ok(Core);
        }

        [HttpDelete("{TicketId}")]
        public IActionResult DeletarRespostaId([FromHeader]string autorToken, string TicketId)
        {
            var Core = new RespostaCore(_contexto).DeletarResposta(autorToken, TicketId);
            return Core.Status ? Ok(Core) : Ok(Core);
        }
    }
}