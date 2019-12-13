using System;

namespace Model
{
    public class Anexo:Base
    {
        public string NomeArquivo { get; set; }
        public string Extensão { get; set; }
        public byte[] Arquivo { get; set; }
        public Guid? RespostaId { get; set; }
    }
}
