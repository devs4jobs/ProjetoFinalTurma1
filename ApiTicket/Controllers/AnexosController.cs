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

        [HttpGet("{id}")]
        public async Task<IActionResult> BuscarArquivo(string id)
        {
            var Core = new AnexoCore(Service);
            var (extensao, Arquivo) = await Core.BuscarArquivo(id);

            if (Arquivo != null)
            {
                HttpContext.Response.ContentType = $"{(extensao=="jpg"||extensao=="jpeg"||extensao=="png"?"image":"application")}/{extensao}";
                HttpContext.Response.Headers.Add("content-length", Arquivo.Length.ToString());
                HttpContext.Response.Body.Write(Arquivo, 0, Arquivo.Length);
            }

            return new ContentResult();
        }
    }
}