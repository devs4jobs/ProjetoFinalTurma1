using System;
using System.Collections.Generic;

namespace Model
{
    public class Ticket : Base
    {
        public Guid ClienteId { get; set; }
        public List<Resposta> lstRespostas { get; set; } 
        public Status Status { get; set; } 
        public string Titulo { get; set; } 
        public string Tipo { get; set; } 
        public Avaliacao Avaliacao { get; set; } 
    }
}
