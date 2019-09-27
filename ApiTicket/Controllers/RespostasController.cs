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
        public async Task<IActionResult> CadastrarResposta([FromBody] RespostaView resposta, [FromHeader] string autorToken)
        {
            var Core = new RespostaCore(resposta, _contexto, _mapper);
             var result = await Core.CadastrarResposta(autorToken);
            return result.Status ? Created($"{HttpContext.Request.Host}{HttpContext.Request.Path}", result) : (IActionResult)Ok(result);
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
        public async Task<IActionResult> AtualizarRespostaId([FromHeader]string autorToken, string RespostaID, RespostaUpdateView resposta)
        {
            var Core = new RespostaCore(_contexto, _mapper);
            var result = await Core.EditarResposta(autorToken, RespostaID, resposta);
            return result.Status ? Accepted(result) : (IActionResult)Ok(result);
        }
        /// <summary>
        /// Deletar a Resposta do Id Inserido.
        /// </summary>
        /// <param name="autorToken"></param>
        /// <param name="RespostaID"></param>
        /// <returns>Retorno uma mensagem com Status da Operação.</returns>
        [HttpDelete("{RespostaID}")]
        public async Task<IActionResult> DeletarRespostaId([FromHeader]string autorToken, string RespostaID)
        {
            var Core = new RespostaCore(_contexto);
            var result = await Core.DeletarResposta(autorToken, RespostaID);
            return result.Status ? Ok(result) : Ok(result);
        }
    }
}