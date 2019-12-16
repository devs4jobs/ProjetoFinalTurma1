using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core;
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
        /// <param name="id">Identificador do Arquivo</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> BuscarArquivo(string id)
        {
            var Core = new AnexoCore(Service);
            var Anexo = await Core.BuscarArquivo(id);

            if (Anexo != null)
            {
                HttpContext.Response.ContentType = "application/octet-stream";
                HttpContext.Response.Headers.Add("content-length", Anexo.Arquivo.Length.ToString());
                HttpContext.Response.Body.Write(Anexo.Arquivo, 0, Anexo.Arquivo.Length);
            }

            return new ContentResult();
        }
    }
}