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
            var Core = new RespostaCore(resposta, _contexto,_mapper).CadastrarResposta(autorToken);
            return Core.Status ? Created($"{HttpContext.Request.Host}{HttpContext.Request.Path}", Core): (IActionResult)Ok(Core);
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
            var Core = new RespostaCore(_contexto,_mapper).BuscarRespostas(autorToken, RespostaID);
            return Core.Status ? Ok(Core) : Ok(Core);
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
            var Core = new RespostaCore(_contexto,_mapper).EditarResposta(autorToken, RespostaID, resposta);
            return Core.Status ? Accepted(Core) : (IActionResult)Ok(Core);
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
            var Core = new RespostaCore(_contexto).DeletarResposta(autorToken, RespostaID);
            return Core.Status ? Ok(Core) : Ok(Core);
        }
    }
}