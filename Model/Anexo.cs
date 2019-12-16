using System;
using System.ComponentModel.DataAnnotations;

namespace Model
{
    public class Anexo
    {
        public DateTime DataCadastro { get; set; } = DateTime.Now;
        [Key]
        public string NomeArquivo { get; set; }
        public byte[] Arquivo { get; set; }
        public Guid? RespostaId { get; set; }
    }
}
