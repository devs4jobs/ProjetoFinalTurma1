using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model
{
    public abstract class Base
    {
        [Key]
        public Guid Id { get; private set; } = Guid.NewGuid();
        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
    }
}
