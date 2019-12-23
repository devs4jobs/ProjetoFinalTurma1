using Core.Util;
using Microsoft.EntityFrameworkCore;
using Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class AnexoCore
    {
        private ServiceContext _ServiceContext { get; set; }

        public AnexoCore(ServiceContext serviceContext) => _ServiceContext = serviceContext;

        public async Task<Retorno> BuscarArquivo(string id, string respostaId)
        {
            try 
            {
                return new Retorno { Status = true, Resultado = await _ServiceContext.Anexos.FirstAsync(x => x.NomeArquivo == id && x.RespostaId == Guid.Parse(respostaId)) };
            }
            catch (FormatException)
            {
                return new Retorno { Resultado = new List<string> { "RespostaId esta incorreto" } };
            }
        }
    }
}
