using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Streamer.API.Startup.Filters;
using Streamer.API.Startup.Middleware;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.IO;
using System.Reflection;

namespace Streamer.API.Startup
{
    public class ApiStartup
    {
        private readonly string corsAllowedOriginsKey = "AllowMyOrigin";

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(corsAllowedOriginsKey, builder =>
                {
                    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                    if (environment == "Local" || environment == "Development")
                    {
                        builder
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                    }
                });
            });

            Domain.Setup.Types.Register(services);

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

                c.OperationFilter<SessionHeaderFilter>();
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment() || env.IsEnvironment("Local"))
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

            app.UseMiddleware<RequestResponseLoggingMiddleware>();
            app.UseMiddleware<RequestTimerStartMiddleware>();
            app.UseMiddleware<SessionValidationMiddleware>();
            app.UseMvc();
        }
    }
}
