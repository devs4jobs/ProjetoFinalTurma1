using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Core;
using Core.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace ApiTicket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnexosController : ControllerBase
    {

        private ServiceContext Service { get; set; }

        public AnexosController(ServiceContext service) => Service = service;

        /// <summary>
        /// Buscar Arquivo
        /// </summary>
        /// <param name="RespostaId"></param>
        /// <param name="NomeArquivo">Identificador do Arquivo</param>
        /// <returns></returns>
        [HttpGet("{NomeArquivo}")]
        public async Task<IActionResult> BuscarArquivo([FromHeader] string RespostaId,string NomeArquivo)
        {
            try
            {
                var Core = new AnexoCore(Service);
                var Anexo = await Core.BuscarArquivo(NomeArquivo,RespostaId);

                if (!Anexo.Status) return Ok(Anexo);

                HttpContext.Response.ContentType = NomeArquivo.Substring(NomeArquivo.IndexOf('.')) == "jpg" ? "application/octet-stream" : "image/jpeg";
                HttpContext.Response.Headers.Add("content-length", Anexo.Resultado.Arquivo.Length.ToString());
                HttpContext.Response.Body.Write(Anexo.Resultado.Arquivo, 0, Anexo.Resultado.Arquivo.Length);


                return new ContentResult();
            }
            catch (NullReferenceException)
            {
                return Ok(new Retorno { Resultado = new List<string> { "Arquivo não encontrado" } });
            }
            catch (Exception)
            {
                return Ok(new Retorno { Resultado = new List<string> { "Erro ao recuperar o Arquivo, Peça para ser enviado novamente" } });
            }
        }
    }
}