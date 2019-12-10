using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Model
{
    /// <summary>
    /// Classe Ticket tem todos os atributos para gerenciamento dos chamados, nessa classe que ocorre a relação cliente e atendente.
    /// </summary>
    public class Ticket : Base
    {
       
        public long NumeroTicket { get; set; }
        public Status Status { get; set; } = Status.ABERTO;
        public string Titulo { get; set; }
        public string Mensagem { get; set; }
        public Usuario Cliente { get; set; }
        [ForeignKey("Usuarios")]
        public Guid? ClienteId { get; set; }
        public Usuario Atendente { get; set; }
        [ForeignKey("Usuarios")]
        public Guid? AtendenteId { get; set; }
        public List<Resposta> LstRespostas { get; set; }
        public Avaliacao Avaliacao { get; set; } = Avaliacao.NAO_AVALIADO;
        // public      GrauDeImportancia { get; set; }
        public bool VisualizarTicket { get; set; } = true;
    }
}
