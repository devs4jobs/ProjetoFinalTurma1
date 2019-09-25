using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Model
{
    /// <summary>
    /// Classe Usuario essa classe é referênciada em todos momentos no projeto.
    /// </summary>
    public class Usuario: Base
    {
        /*
         * Todos os atributos listados abaixo serve para cadastro do Usuário na nossa base de dados,
         * declaramos algumas obrigatoriedades também.
         */
        [Required]
        [MinLength(3)]
        public string Nome { get;  set; }
        [Required]
        [EmailAddress]
        public string Email { get;  set; } 
        [Required]
        [StringLength(8,ErrorMessage = "A senha deve conter no mínimo 8 caracteres.")]
        public string Senha {  get;  set; } 
        [NotMapped]
        public string ConfirmaSenha { get; set; }
        [Required]
        public string Tipo { get; set; }
    }
}
