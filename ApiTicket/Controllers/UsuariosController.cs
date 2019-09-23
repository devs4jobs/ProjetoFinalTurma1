using AutoMapper;
using Core;
using Microsoft.AspNetCore.Mvc;
using Model;
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
        ///     Post/Usuarios 
        ///     {
        ///       "nome": "string",
        ///       "email": "string",
        ///       "senha": "string",
        ///       "confirmaSenha": "string",
        ///       "tipo": "string"  
        ///     }
        ///
        /// </remarks>
        /// <param name="usuarioView"></param>
        [HttpPost]
        public IActionResult Cadastro([FromBody] UsuarioView usuarioView  )
        {
            var Core = new UsuarioCore(usuarioView, _contexto,_mapper).CadastrarUsuario();
            return Core.Status ? Created($"{HttpContext.Request.Host}{HttpContext.Request.Path}/Autenticar", Core) : (IActionResult)Ok(Core);
        }

        /// <summary>
        /// Criar Usuário.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        /// Post/Usuarios/Autenticar
        ///
        ///      {
        ///        "email": "string",
        ///        "senha": "string"
        ///      }
        /// </remarks>
        /// <param name="loginView"></param>
        [HttpPost("Autenticar")]
        public IActionResult Logar([FromBody] LoginView loginView)
        {
            var Core = new UsuarioCore(_contexto).LogarUsuario(loginView);
            return Core.Status ? Ok(Core) : Ok(Core);
        }
    }
}