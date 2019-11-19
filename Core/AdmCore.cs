using AutoMapper;
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
        private IMapper _mapper { get; set; }

        public AdmCore(IMapper Mapper) { _mapper = Mapper; }


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

        public async Task<Retorno> BuscarAtendentes(string tokenAutor, int NumeroPagina, int QuantidadeRegistro)
        {
            if (!Guid.TryParse(tokenAutor, out Guid usuarioToken))
                return new Retorno { Status = false, Resultado = new List<string> { "Token inválido" } };

            var oAdm = _serviceContext.Usuarios.SingleOrDefaultAsync(c => c.Id == Guid.Parse(tokenAutor) && c.Tipo.ToUpper() == "ADMINISTRADOR");

            if (oAdm == null)
                return new Retorno { Status = false, Resultado = new List<string> { "Usuario inválido" } };

            List<Usuario> Atendentes;
            // nova instancia da paganicação
            var Paginacao = new Paginacao();

            Atendentes = await _serviceContext.Usuarios.Where(e => e.Tipo == "ATENDENTES").ToListAsync();

            if (NumeroPagina > 0 && QuantidadeRegistro > 0)
            {
                Paginacao.Paginar(NumeroPagina, QuantidadeRegistro, Atendentes.Count());
                var listaPaginada = Atendentes.OrderByDescending(d => d.DataCadastro).Skip((NumeroPagina - 1) * QuantidadeRegistro).Take(QuantidadeRegistro).ToList();

                return listaPaginada.Count() == 0 ? new Retorno
                {
                    Status = false,
                    Resultado = new List<string> { "Não foi possível realizar a paginação" }
                } : new Retorno
                { Status = true, Paginacao = Paginacao, Resultado = _mapper.Map<List<UsuarioRetorno>>(listaPaginada) };

            }
            Atendentes = await _serviceContext.Usuarios.Where(c => c.Tipo == "ATENDENTES").Take(10).ToListAsync();

            return Atendentes.Count() == 0 ? new Retorno { Status = false, Resultado = new List<string> { "Não há atendentes no momento!" } } : new Retorno { Status = true, Resultado = Atendentes };
        }












    }
}
