using System;
using Elasticsearch.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using ShowScraper.TvMazeClient;
using ShowScraper.TvMazeClient.Models;

namespace ShowScraper.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
           services.AddSwaggerGen(options =>
            {
                options.DescribeAllEnumsAsStrings();
                options.DescribeStringEnumsInCamelCase();
                options.SwaggerDoc("v1", new Swashbuckle.AspNetCore.Swagger.Info
                {
                    Title = "Shows Caching API",
                    Version = "v1",
                    Description = "Return shows with their cast members, in order of their birthday"
                });
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddScoped<IElasticClient>(_ =>
            {
                var node = new Uri(Configuration["ELASTICSEARCH_HOST"] ?? "http://localhost:9200");
            
                var connectionPool = new SniffingConnectionPool(new[] {node}) {SniffedOnStartup = true };
            
                var connectionSettingsValues = new ConnectionSettings(connectionPool)
                    .DefaultMappingFor<ShowWithCast>(s => s.IndexName(nameof(ShowWithCast).ToLowerInvariant()));

                return new ElasticClient(connectionSettingsValues);
            });

            services.AddTransient<ElasticShowsProvider>();
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseAuthentication();
            app.UseMvcWithDefaultRoute();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
               {
                   c.SwaggerEndpoint("/swagger/v1/swagger.json", "HTTP API V1");
               });


            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }

}
