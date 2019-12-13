using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model
{
    public class Resposta: Base
    {
        public string Mensagem { get; set; }
        [ForeignKey("Tickets")]
        public Guid? TicketId { get; set; }
        public Usuario Usuario { get; set; }
        [ForeignKey("Usuarios")]
        public Guid? UsuarioId { get; set; }
        public Anexo Anexo { get; set; }
        public bool VisualizarMensagem { get; set; } = true;
    }
}