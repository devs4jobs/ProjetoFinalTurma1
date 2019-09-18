using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace Model
{
    public abstract class Base
    {
        [Key]
        public Guid Id { get; private set; } = Guid.NewGuid();
        [JsonIgnore]
        public DateTime DataCadastro { get; set; } = DateTime.Now;
    }
}
