using System;

namespace Model
{
    public class RespostaRetorno
    {
        public string Mensagem { get; set; }
        public UsuarioRetorno Usuario { get; set; }
        public AnexoRetorno Anexo { get; set; }
        public DateTime DataCadastro { get; set; }
    }
}
