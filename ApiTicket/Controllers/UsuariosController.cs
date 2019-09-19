using Core;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace ApiForum.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        //Construtor contendo o contexto.
        private ServiceContext _contexto { get; set; }
    
        // construtor para a utilização do automapper por meio de injeçao de dependecia
        public UsuariosController(ServiceContext contexto) =>  _contexto = contexto;
      
        //Chamando o metodo de cadastar usurario da core 
        [HttpPost]
        public IActionResult Cadastro([FromBody] Usuario Usuario)
        {
            var Core = new UsuarioCore(Usuario, _contexto).CadastrarUsuario();
            return Core.Status ? Created("https://localhost:44362//api/Usuarios/Autenticar", Core) : (IActionResult)Ok(Core);
        }
        //Chamando o metodo de logar usurario da core 
        [HttpPost("Autenticar")]
        public IActionResult Logar([FromBody] Usuario usuario)
        {
            var Core = new UsuarioCore(usuario, _contexto).LogarUsuario();
            return Core.Status ? Ok(Core) : Ok(Core);
        }
    }
}