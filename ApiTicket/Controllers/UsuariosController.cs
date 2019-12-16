using Core;
using Microsoft.AspNetCore.Mvc;
using Model;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;
using Core.Util;
using System.Collections.Generic;

namespace ApiForum.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        //Construtor contendo o contexto.
        private ServiceContext _contexto { get; set; }

        // construtor para a utilização do automapper por meio de injeçao de dependecia
        public UsuariosController(ServiceContext service )=> _contexto = service;

        /// <summary>
        /// Criar Usuário.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///	    {
        ///         "nome": "Rafael",
        ///         "email": "rafaelJunior@dev4jobs.com",
        ///         "senha": "Senha123",
        ///         "confirmaSenha": "Senha123",
        ///         "tipo": "Atendente"
        ///	    }
        ///
        /// </remarks>
        /// <param name="usuarioView"></param>
        ///  /// <returns>Retorna o Status de cadastro</returns>
        [HttpPost]
        public async Task<IActionResult> Cadastro([FromBody] JObject usuarioView)
        {
            try
            {
                var Core = new UsuarioCore(JsonConvert.DeserializeObject<Usuario>(usuarioView.ToString()), _contexto);
                var result = await Core.CadastrarUsuario();

                return result.Status ? Created($"{HttpContext.Request.Host}{HttpContext.Request.Path}/Autenticar", result) : (IActionResult)BadRequest(result);
            }
            catch (Exception) {  return BadRequest(new Retorno { Status = false, Resultado = new List<string> { "As Informações foram passadas de forma errada, por favor siga o exemplo do Swagger" } });}
        }

        /// <summary>
        /// Autenticar Usuário.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///	    {
        ///  		"email": "rafaelJunior@dev4jobs.com",
        ///  		"senha": "Senha123"
        ///	    }
        ///
        /// </remarks>
        /// <param name="loginView"></param>
        /// <returns>Retorna o AutorToken </returns>
        [HttpPost("Autenticar")]
        public async Task<IActionResult> Logar([FromBody] JObject loginView)
        {
            try
            {
                var Core = new UsuarioCore(_contexto);
                var result = await Core.LogarUsuario(JsonConvert.DeserializeObject<Usuario>(loginView.ToString()));

                return result.Status ? Ok(result) : (IActionResult)BadRequest(result);
            }
            catch (Exception) {  return BadRequest(new Retorno { Status = false, Resultado = new List<string> { "As Informações foram passadas de forma errada, por favor siga o exemplo do Swagger" } }); }
        }
    }
}