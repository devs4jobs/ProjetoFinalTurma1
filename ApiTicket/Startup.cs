using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.StaticFiles;
using Model;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Reflection;
using System.IO;

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
                options.SerializerSettings.DateFormatString = "HH:mm,dd/MM/yyyy";
                options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            });

            services.AddDbContext<ServiceContext>(options => options.UseSqlServer(Configuration.GetConnectionString("StringConexao")), ServiceLifetime.Scoped);

            services.AddSwaggerGen(s =>
            {
                s.SwaggerDoc("v1", new Info { Title = "Ticket Api", Version = "v1" });
            });

            var config = new MapperConfiguration(cfg =>
            {
                //mapeamento dos usuarios.
                cfg.CreateMap<UsuarioView, Usuario>();
                cfg.CreateMap<Usuario, UsuarioRetorno>();
                cfg.CreateMap<LoginView, Usuario>();

                //mapeamento dos tickets
                cfg.CreateMap<TicketView, Ticket>();
                cfg.CreateMap<TicketUpadateView, Ticket>()
                    .ForMember(dest => dest.Avaliacao, opt => opt.Condition(ori => ori.Avaliacao != null))
                    .ForMember(dest => dest.Status, opt => opt.Condition(ori => ori.Status != null))
                    .ForMember(dest => dest.Titulo, opt => opt.Condition(ori => ori.Titulo != null))
                    .ForMember(dest => dest.Mensagem, opt => opt.Condition(ori => ori.Mensagem != null));

                cfg.CreateMap<Ticket, TicketRetorno>()
                    .ForMember(dest => dest.Atendente, opt => opt.Condition(ori => ori.Atendente != null))
                    .ForMember(dest => dest.Cliente, opt => opt.Condition(ori => ori.Cliente != null));

                //mapeamento das respostas.
                cfg.CreateMap<RespostaView, Resposta>();
                cfg.CreateMap<Resposta, RespostaRetorno>();
                cfg.CreateMap<RespostaUpdateView, Resposta>()
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ticket Api");
                c.RoutePrefix = string.Empty;
            });
            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}