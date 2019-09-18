using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Model
{
    public class Ticket : Base
    {
        
        public int NumeroTicket { get; set; }
        public Usuario Cliente { get; set; }
        public Guid? ClienteId { get; set; }
        public List<Resposta> LstRespostas { get; set; }
        public Status Status { get; set; } 
        public string Titulo { get; set; } 
        public string Tipo { get; set; } 
        public Avaliacao Avaliacao { get; set; } 
    }
}
