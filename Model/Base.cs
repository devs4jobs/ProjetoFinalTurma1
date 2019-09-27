using System;
using System.ComponentModel.DataAnnotations;

namespace Model
{
    public abstract class Base
    {
        [Key]
        public Guid Id { get; private set; } = Guid.NewGuid();
        public DateTime DataCadastro { get; set; } = DateTime.Now;
    }
}
