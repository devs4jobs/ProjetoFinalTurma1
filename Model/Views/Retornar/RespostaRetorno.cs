using System;

namespace Model
{
    public class RespostaRetorno
    {
        public string Mensagem { get; set; }
        public UsuarioRetorno Usuario { get; set; }
        public Guid Id { get; set; }
        public DateTime DataCadastro { get; set; }
    }
}
