using AutoMapper;
using Core;
using Microsoft.AspNetCore.Mvc;
using Model;
using System.Threading.Tasks;
using Model.Views.Receber;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using Core.Util;
using System.Collections.Generic;

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
        public async Task<IActionResult> CadastrarTicket([FromBody] JObject ticket, [FromHeader] string autorToken)
        {
            try
            {
                var Core = new TicketCore(JsonConvert.DeserializeObject<Ticket>(JsonConvert.SerializeObject(ticket)), _contexto, _Mapper);
                var result = await Core.CadastrarTicket(autorToken);
                return result.Status ? Created($"{HttpContext.Request.Host}{HttpContext.Request.Path}", result) : (IActionResult)Ok(result);
            }
            catch (Exception)
            {
                return Ok(new Retorno { Status = false, Resultado = new List<string> { $"As Informações foram passadas de forma errada, por favor siga o exemplo do Swagger" } });

            }
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
        public async Task<IActionResult> AtualizarTicketId([FromHeader]string autorToken, string TicketID, [FromBody] JObject ticket)
        {
            try
            {
                var Core = new TicketCore(_Mapper, _contexto);
                var result = await Core.AtualizarTicket(autorToken, TicketID, JsonConvert.DeserializeObject<Ticket>(JsonConvert.SerializeObject(ticket)));
                return result.Status ? Accepted(result) : (IActionResult)Ok(result);
            }
            catch (Exception)
            {
                return Ok(new Retorno { Status = false, Resultado = new List<string> { $"As Informações foram passadas de forma errada, por favor siga o exemplo do Swagger" } });

            }
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
        /// Cliente fecha o Ticket.
        /// </summary>
        /// <param name="autorToken"></param>
        /// <param name="Avaliacao"></param>
        /// <returns>Retorno uma mensagem de Status para o Cliente.</returns>
        [HttpPost("Fechar")]
        public async Task<IActionResult> FecharTicket([FromHeader]string autorToken, [FromBody] AvaliacaoView Avaliacao)
        {

            var Core = new TicketCore(_contexto);
            var result = await Core.FecharTicket(autorToken, Avaliacao);
            return result.Status ? Ok(result) : Ok(result);
        }
    }
}