using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Util
{
    public class Retorno
    {
        public bool Status { get; set; }
        public Paginacao Paginacao { get; set; }
        public dynamic Resultado { get; set; }
    }
}
