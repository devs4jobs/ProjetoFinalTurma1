 using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Model
{
    /// <summary>
    /// Classe Usuario essa classe é referênciada em todos momentos no projeto.
    /// </summary>
    public class Usuario : Base
    {
        /*
         * Todos os atributos listados abaixo serve para cadastro do Usuário na nossa base de dados,
         * declaramos algumas obrigatoriedades também.
         */

        public string Nome { get; set; } 
        public string Email { get; set; }
        public string Senha { get; set; }
        [NotMapped]
        public string ConfirmaSenha { get; set; }
        public string Tipo { get; set; }
    }
}
