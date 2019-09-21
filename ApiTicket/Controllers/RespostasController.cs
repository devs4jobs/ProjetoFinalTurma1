using AutoMapper;
using Core;
using Microsoft.AspNetCore.Mvc;
using Model;
namespace ApiForum.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RespostasController : ControllerBase
    {
        //Construtor contendo o contexto.
        private ServiceContext _contexto { get; set; }
        private readonly IMapper _mapper;

        // construtor para a utilização do automapper por meio de injeçao de dependecia
        public RespostasController(ServiceContext contexto, IMapper mapper) { _contexto = contexto; _mapper = mapper; }

        //Chamando o metodo de cadastar usurario da core 
        [HttpPost]
        public IActionResult CadastrarResposta([FromBody] RespostaView resposta, [FromHeader] string autorToken)
        {
            var Core = new RespostaCore(resposta, _contexto,_mapper).CadastrarResposta(autorToken);
            return Core.Status ? Created($"{HttpContext.Request.Host}{HttpContext.Request.Path}", Core): (IActionResult)Ok(Core);
        }

        [HttpGet("{RespostaID}")]
        public IActionResult GetIdResposta([FromHeader]string autorToken, string RespostaID)
        {
            var Core = new RespostaCore(_contexto).BuscarRespostas(autorToken, RespostaID);
            return Core.Status ? Ok(Core) : Ok(Core);
        }

        [HttpPut("{RespostaID}")]
        public IActionResult AtualizarRespostaId([FromHeader]string autorToken, string RespostaID, RespostaUpdateView resposta)
        {
            var Core = new RespostaCore(_contexto,_mapper).EditarResposta(autorToken, RespostaID, resposta);
            return Core.Status ? Accepted(Core) : (IActionResult)Ok(Core);
        }

        [HttpDelete("{RespostaID}")]
        public IActionResult DeletarRespostaId([FromHeader]string autorToken, string RespostaID)
        {
            var Core = new RespostaCore(_contexto).DeletarResposta(autorToken, RespostaID);
            return Core.Status ? Ok(Core) : Ok(Core);
        }
    }
}