using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Model;
using Swashbuckle.AspNetCore.Swagger;
using System.Reflection;
using System.IO;
using System;
using Microsoft.Extensions.Options;

namespace ApiTicket
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddMvc().AddJsonOptions(options =>
            {
                options.SerializerSettings.DateFormatString = "dd/MM/yyyy HH:mm:ss";
                options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            });

            services.AddDbContext<ServiceContext>(options => options.UseSqlServer(Configuration.GetConnectionString("StringConexao")), ServiceLifetime.Scoped);

            services.AddSwaggerGen(opt =>
            {

                opt.SwaggerDoc("v2", new Info
                {
                    Version = "v2",
                    Title = "Ticket API",
                    Description = "Aplicação ASP.NET CORE feita para HelpDesk.",
                    TermsOfService = "https://github.com/devs4jobs/ProjetoFinalTurma1",
                    Contact = new Contact
                    {
                        Name = "1ª Turma Dev4Jobs",
                        Url = "https://github.com/devs4jobs/ProjetoFinalTurma1/blob/master/Fluxograma%20API%20TicketOk.png"
                    }
                });
                
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                opt.IncludeXmlComments(xmlPath);

            });

            var config = new MapperConfiguration(cfg =>
            {
                //mapeamento dos usuarios.
                cfg.CreateMap<Usuario, UsuarioRetorno>();

                //mapeamento dos tickets
                cfg.CreateMap<Ticket, Ticket>()
                    .ForMember(dest => dest.Atendente, opt => opt.Ignore())
                    .ForMember(dest => dest.AtendenteId, opt => opt.Ignore())
                    .ForMember(dest => dest.ClienteId, opt => opt.Ignore())
                    .ForMember(dest => dest.Cliente, opt => opt.Ignore())
                    .ForMember(dest => dest.DataCadastro, opt => opt.Ignore())
                    .ForMember(dest => dest.Id, opt => opt.Ignore())
                    .ForMember(dest => dest.LstRespostas, opt => opt.Ignore())
                    .ForMember(dest => dest.NumeroTicket, opt => opt.Ignore())
                    .ForMember(dest => dest.Status, opt => opt.Ignore())
                    .ForMember(dest => dest.Titulo, opt => opt.Condition(ori => ori.Titulo != null))
                    .ForMember(dest => dest.Mensagem, opt => opt.Condition(ori => ori.Mensagem != null));

                cfg.CreateMap<Ticket, TicketRetorno>()
                    .ForMember(dest => dest.Atendente, opt => opt.Condition(ori => ori.Atendente != null))
                    .ForMember(dest => dest.Cliente, opt => opt.Condition(ori => ori.Cliente != null));

                //mapeamento das respostas.
                cfg.CreateMap<Resposta, RespostaRetorno>();
                cfg.CreateMap<Resposta, Resposta>()
                    .ForMember(d => d.DataCadastro, opt => opt.Ignore())
                    .ForMember(d => d.Id, opt => opt.Ignore())
                    .ForMember(d=>d.TicketId,opt=>opt.Ignore())
                    .ForMember(d=>d.UsuarioId,opt=>opt.Ignore())
                    .ForMember(dest => dest.Mensagem, opt => opt.Condition(ori => ori.Mensagem != null));
            });
            IMapper mapper = config.CreateMapper();
            services.AddSingleton(mapper);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseHsts();

            app.UseStaticFiles();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v2/swagger.json", "Ticket API");
                c.RoutePrefix = string.Empty;
            });
            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}