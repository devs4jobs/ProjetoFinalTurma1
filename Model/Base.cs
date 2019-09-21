using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model
{
    public abstract class Base
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; private set; } = Guid.NewGuid();
        [JsonIgnore]
        public DateTime DataCadastro { get; set; } = DateTime.Now;
    }
}
