using AutoMapper;
using Core;
using Microsoft.AspNetCore.Mvc;
using Model;
namespace ApiForum.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class RespostasController : ControllerBase
    {
        //Construtor contendo o contexto.
        private ServiceContext _contexto { get; set; }
        private readonly IMapper _mapper;

        // construtor para a utilização do automapper por meio de injeçao de dependecia
        public RespostasController(ServiceContext contexto, IMapper mapper) { _contexto = contexto; _mapper = mapper; }

        /// <summary>
        /// Criar Resposta
        /// </summary>
        /// <remarks>
        /// 
        ///     Sample request:
        /// 
        ///         {
        ///             "mensagem" : "Oi você pode me ajudar estou com problemas, Help!.",	
        ///             "ticketId": "id do ticket em questão."
        ///         }
        ///                  
        /// </remarks>
        /// <param name="resposta"></param>
        /// <param name="autorToken"></param>
        [HttpPost]
        public IActionResult CadastrarResposta([FromBody] RespostaView resposta, [FromHeader] string autorToken)
        {
            var Core = new RespostaCore(resposta, _contexto, _mapper);
            var result = Core.CadastrarResposta(autorToken).Result;
            return result.Status ? Created($"{HttpContext.Request.Host}{HttpContext.Request.Path}", result) : (IActionResult)Ok(result);
        }

        /// <summary>
        ///  Busco a Resposta do Id Inserido.
        /// </summary>
        /// <param name="autorToken"></param>
        /// <param name="RespostaID"></param>
        /// <returns>Retorno a Resposta.</returns>
        [HttpGet("{RespostaID}")]
        public IActionResult GetIdResposta([FromHeader]string autorToken, string RespostaID)
        {
            var Core = new RespostaCore(_contexto, _mapper);
            var result = Core.BuscarRespostas(autorToken, RespostaID).Result;
            return result.Status ? Ok(result) : Ok(result);
        }
        /// <summary>
        /// Atualizar a Resposta do Id inserido.
        /// </summary>
        /// <remarks>
        ///     Sample request:
        /// 
        ///         {
        ///             "mensagem" : "Oi você pode me ajudar estou com problemas, Help!."	
        ///         }
        ///         
        ///</remarks>
        /// <param name="autorToken"></param>
        /// <param name="RespostaID"></param>
        /// <param name="resposta"></param>
        /// <returns>Retorno a Resposta Atualizada.</returns>
        [HttpPut("{RespostaID}")]
        public IActionResult AtualizarRespostaId([FromHeader]string autorToken, string RespostaID, RespostaUpdateView resposta)
        {
            var Core = new RespostaCore(_contexto, _mapper);
            var result = Core.EditarResposta(autorToken, RespostaID, resposta).Result;
            return result.Status ? Accepted(result) : (IActionResult)Ok(result);
        }
        /// <summary>
        /// Deletar a Resposta do Id Inserido.
        /// </summary>
        /// <param name="autorToken"></param>
        /// <param name="RespostaID"></param>
        /// <returns>Retorno uma mensagem com Status da Operação.</returns>
        [HttpDelete("{RespostaID}")]
        public IActionResult DeletarRespostaId([FromHeader]string autorToken, string RespostaID)
        {
            var Core = new RespostaCore(_contexto);
            var result = Core.DeletarResposta(autorToken, RespostaID).Result;
            return result.Status ? Ok(result) : Ok(result);
        }
    }
}