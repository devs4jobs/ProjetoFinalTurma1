using System.ComponentModel.DataAnnotations;

namespace Model
{

    public class UsuarioView
    {
   
        public string Nome { get;  set; } 
        public string Email { get;  set; } 
        public string Senha { get;  set; }
        public string ConfirmaSenha { get; set; }
        public string Tipo { get; set; } 
    }

    public class LoginView
    {
        public string Email { get; set; } 
        public string Senha { get; set; } 
    }
}
