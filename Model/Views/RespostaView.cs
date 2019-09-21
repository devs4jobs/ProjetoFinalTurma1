using System;
namespace Model
{
    public class RespostaView
    {
        public string Mensagem { get; set; }
        public Guid? TicketId { get; set; }
    }

    public class RespostaUpdateView
    {
        public string Mensagem { get; set; }
    }
}