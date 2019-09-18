using System;

namespace Model
{
    public class Resposta: Base
    {
        public string Mensagem { get; set; }
        public Guid TicketId { get; set; }
        public Guid UsuarioId { get; set; }
    }
}