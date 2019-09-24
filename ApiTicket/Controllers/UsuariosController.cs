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
    public class UsuariosController : ControllerBase
    {
        //Construtor contendo o contexto.
        private ServiceContext _contexto { get; set; }
        private readonly IMapper _mapper;

        // construtor para a utilização do automapper por meio de injeçao de dependecia
        public UsuariosController(ServiceContext service, IMapper mapper) { _contexto = service; _mapper = mapper; }

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
        public async Task<IActionResult> Cadastro([FromBody] UsuarioView usuarioView)
        {
            var Core = new UsuarioCore(usuarioView, _contexto, _mapper);
            var result = await Core.CadastrarUsuario();

            return result.Status ? Created($"{HttpContext.Request.Host}{HttpContext.Request.Path}/Autenticar", result) : (IActionResult)Ok(result);
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
        public async Task<IActionResult> Logar([FromBody] LoginView loginView)
        {
            var Core = new UsuarioCore(_contexto);
            var result = await Core.LogarUsuario(loginView);

            return result.Status ? Ok(result) : Ok(result);
        }
    }
}