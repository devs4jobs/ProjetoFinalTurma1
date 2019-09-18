using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Model;
using Swashbuckle.AspNetCore.Swagger;

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
            services.AddMvc().AddJsonOptions(options => {
                options.SerializerSettings.DateFormatString = "dd/MM/yyyy";
                options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                    });
            services.AddDbContext<ServiceContext>(options => options.UseSqlServer(Configuration.GetConnectionString("StringConexao")), ServiceLifetime.Scoped);
            services.AddSwaggerGen(s =>
            {
                s.SwaggerDoc("v1", new Info { Title = "Ticket Api", Version = "v1" });
            });

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Ticket, Ticket>()
                    .ForMember(dest => dest.Id, opt => opt.Ignore())
                    .ForMember(dest => dest.DataCadastro, opt => opt.Ignore())
                    .ForMember(dest => dest.ClienteId, opt => opt.Ignore())
                    .ForMember(dest => dest.LstRespostas, opt => opt.Ignore())
                    .ForMember(dest => dest.Avaliacao, opt => opt.Condition(ori => ori.Avaliacao != null))
                    .ForMember(dest => dest.Status, opt => opt.Condition(ori => ori.Status != null))
                    .ForMember(dest => dest.Tipo, opt => opt.Condition(ori => ori.Titulo != null))
                    .ForMember(dest => dest.Titulo, opt => opt.Condition(ori => ori.Titulo != null));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else

                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();

            app.UseSwagger();

            // Ativa o Swagger UI
            app.UseSwaggerUI(opt =>
            {
                opt.SwaggerEndpoint("/swagger/v1/swagger.json", "Ticket Api");
            });
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
