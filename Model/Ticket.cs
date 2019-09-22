using Newtonsoft.Json;
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
        public long NumeroTicket { get; set; }
        public Status? Status { get; set; } = Model.Status.ABERTO;
        public string Titulo { get; set; }
        public string Mensagem { get; set; }
        [NotMapped]
        public UsuarioRetorno Cliente { get; set; }
        public Usuario Client { get; set; }
        [JsonIgnore]
        [ForeignKey("Usuarios")]
        public Guid? ClientId { get; set; }
        public Usuario Atendent { get; set; }
        [NotMapped]
        public UsuarioRetorno Atendente { get; set; } 
        [JsonIgnore]
        [ForeignKey("Usuarios")]
        public Guid? AtendentId { get; set; }
        public List<Resposta> LstRespostas { get; set; }
        public Avaliacao? Avaliacao { get; set; }
    }
}
 