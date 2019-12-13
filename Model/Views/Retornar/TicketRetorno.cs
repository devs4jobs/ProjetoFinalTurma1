using System;
using System.Collections.Generic;
namespace Model
{
    public class TicketRetorno
    {
        public Guid Id { get; set; }
        public DateTime DataCadastro { get; set; }
        public long NumeroTicket { get; set; }
        public Status? Status { get; set; }
        public string Titulo { get; set; }
        public string Mensagem { get; set; }
        public UsuarioRetorno Cliente { get; set; }
        public UsuarioRetorno Atendente { get; set; }
        public List<RespostaRetorno> LstRespostas { get; set; }
        public Avaliacao? Avaliacao { get; set; }
    }
}
