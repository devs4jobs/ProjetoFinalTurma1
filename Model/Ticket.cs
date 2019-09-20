using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model
{
    /// <summary>
    /// Minha Model Ticket tem todos os atributos necessarios para controle e relacionamento.
    /// </summary>
    public class Ticket : Base
    {
        public string NumeroTicket { get; set; }
        public Usuario Cliente { get; set; }
        [ForeignKey("Usuarios")]
        public Guid? ClienteId { get; set; }
        public Usuario Atendente { get; set; }
        [ForeignKey("Usuarios")]
        public Guid? AtendenteId { get; set; }
        public List<Resposta> LstRespostas { get; set; }
        public Status? Status { get; set; } 
        public string Titulo { get; set; } 
        public string Mensagem { get; set; }
        public Avaliacao? Avaliacao { get; set; } 
    }
}
 