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

        private Anexo _Anexo { get; set; }
        public AnexoCore(ServiceContext serviceContext) => _ServiceContext = serviceContext;
        
        public async Task<(string extensao,byte[] Arquivo)> BuscarArquivo(string id)
        {
           _Anexo = await _ServiceContext.Anexos.FirstAsync(x => x.Id == Guid.Parse(id));
            return (_Anexo.Extensão, _Anexo.Arquivo);
        }
    }
}
