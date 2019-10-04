using Core.Util;
using Microsoft.EntityFrameworkCore;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class AdmCore
    {
        private ServiceContext _serviceContext { get; set; }

        public AdmCore(){}
   
   
        public async Task<Retorno> TrocarAtendenteTicket(string tokenAutor, string numeroTicket, string AtendenteToken)
        {
            if (!long.TryParse(numeroTicket, out long numeroDoTicket) || !await _serviceContext.Tickets.AnyAsync(c => c.NumeroTicket == numeroDoTicket))
                return new Retorno { Status = false, Resultado = new List<string> { "Número não existe na base de dados" } };

            var oAdm = await _serviceContext.Usuarios.SingleOrDefaultAsync(c => c.Id == Guid.Parse(tokenAutor) && c.Tipo.ToUpper() == "ADMINISTRADOR");

            if (oAdm == null)
                return new Retorno { Status = false, Resultado = new List<string> { "Usuario inválido" } };

            var oTicket = await _serviceContext.Tickets.SingleOrDefaultAsync(c => c.NumeroTicket == numeroDoTicket);

            if (!Guid.TryParse(AtendenteToken, out Guid tokendoAtendente))
                return new Retorno { Status = false, Resultado = new List<string> { "token inválido" } };

            var oAtendente = await _serviceContext.Usuarios.SingleOrDefaultAsync(c => c.Id == tokendoAtendente && c.Tipo == "ATENDENTE");

            if (oAtendente == null)
                return new Retorno { Status = false, Resultado = new List<string> { "Atendente inválido" } };

            oTicket.Atendente = oAtendente;

            await _serviceContext.SaveChangesAsync();

            return new Retorno { Status = true, Resultado = new List<string> { "Atendente trocado com sucesso!" } }; ;
        }

        public async Task<Retorno> BuscarAtendentes(string tokenAutor, int NumeroPagina, int QuantidadeRegistro, string statusAtendente)
        {
            if (!Guid.TryParse(tokenAutor, out Guid usuarioToken))
                return new Retorno { Status = false, Resultado = new List<string> { "Token inválido" } };

            var oAdm = _serviceContext.Usuarios.SingleOrDefaultAsync(c => c.Id == Guid.Parse(tokenAutor) && c.Tipo.ToUpper() == "ADMINISTRADOR");

            if (oAdm == null)
                return new Retorno { Status = false, Resultado = new List<string> { "Usuario inválido" } };

            var listaIds = _serviceContext.Tickets.Select(c => c.AtendenteId).ToString();

            List<Usuario> Atendentes;

            if (!String.IsNullOrEmpty(statusAtendente))
            {
                switch (statusAtendente.ToUpper())
                {

                    case "LIVRES":

                        Atendentes = await _serviceContext.Usuarios.Where(e => e.Tipo == "ATENDENTES").ToListAsync();

                        Atendentes.RemoveAll(c => c.)
             
                 
                            break;

                    



                    default:
                        break;
                }

            }





            Atendentes = await _serviceContext.Usuarios.Where(c => c.Tipo == "ATENDENTES").Take(10).ToListAsync();

            return Atendentes.Count() == 0 ? new Retorno { Status = false, Resultado = new List<string> { "Não há atendentes no momento!" } } : new Retorno { Status = true, Resultado = Atendentes };
        }




    }
}
