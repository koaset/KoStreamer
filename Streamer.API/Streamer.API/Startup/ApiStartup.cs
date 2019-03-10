using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Streamer.API.Startup.Middleware;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Streamer.API.Startup
{
    public class ApiStartup
    {
        private readonly string environment;

        public ApiStartup(IConfiguration configuration)
        {
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            Configuration = configuration;
            GoogleTokenHelper.Configure(configuration.GetValue<string>("GoogleAppId"));
        }

        public IConfiguration Configuration { get; }

        private readonly string corsAllowedOriginsKey = "AllowMyOrigin";

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(corsAllowedOriginsKey, builder =>
                {
                    var allowedOrigins = new List<string>();

                    if (environment == "Local")
                    {
                        allowedOrigins.Add("http://local.player.koaset.com");
                    }

                    builder.WithOrigins(allowedOrigins.ToArray())
                    .AllowAnyHeader()
                    .AllowAnyMethod();
                });
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info {
                    Title = "My API",
                    Version = "v1"
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            app.UseCors(corsAllowedOriginsKey);
            app.UseMiddleware<RequestTimerStartMiddleware>();
            app.UseMiddleware<RequestResponseLoggingMiddleware>();
            app.UseMvc();
        }
    }
}
