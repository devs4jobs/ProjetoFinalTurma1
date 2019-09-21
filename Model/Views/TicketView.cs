using System;
namespace Model
{
    public class TicketView
    {
        public string Titulo { get; set; } 
        public string Mensagem { get; set; }
    }
    // view para update
    public class TicketUpadateView
    {
        public Avaliacao? Avaliacao { get; set; }
        public Status? Status { get; set; }
        public string Titulo { get; set; }
        public string Mensagem { get; set; }
    }
}
 