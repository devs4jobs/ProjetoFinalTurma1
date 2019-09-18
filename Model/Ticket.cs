using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model
{
    public class Ticket : Base
    {
        [ForeignKey("Usuarios")]
        public Guid ClienteId { get; set; }

        [NotMapped]
        public List<Resposta> lstRespostas { get; set; }

        public Status? Status { get; set; } 
        public string Titulo { get; set; } 
        public string Tipo { get; set; } 
        public Avaliacao? Avaliacao { get; set; } 
    }
}
