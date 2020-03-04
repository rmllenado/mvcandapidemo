using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Mvc.HealthChecks;
using Newtonsoft.Json.Linq;

namespace Mvc
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
            services.AddControllersWithViews();

            services.AddHealthChecks()
                .AddSqlServer(Configuration["ConnectionStrings:DefaultConnectionString"],
                    name: "SQL Server",
                    tags: new[] { "ready" },
                    failureStatus: HealthStatus.Unhealthy
                    )
                .AddUrlGroup(new Uri($"{Configuration["Apis:WeatherApi:Url"]}/weatherforecast"),
                    name: "Weather Forecast Api Health Check",
                    tags: new[] { "ready" },
                    failureStatus: HealthStatus.Degraded,
                    timeout: new TimeSpan(0, 0, 5)
                    )
                .AddFilePathWrite(Configuration["SecurityLogFilePath"]
                    , HealthStatus.Unhealthy, new[] { "ready" });
            //.AddCheck(name: "File Path Health Check",
            //    instance: new FilePathWriteHealthCheck(Configuration["SecurityLogFilePath"]),
            //    failureStatus: HealthStatus.Unhealthy,
            //    tags: new[] { "ready" }
            //    );

            services.AddHealthChecksUI();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions()
                {
                    ResultStatusCodes = {
                        [HealthStatus.Healthy] = StatusCodes.Status200OK,
                        [HealthStatus.Degraded] = StatusCodes.Status500InternalServerError,
                        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable,
                    },
                    ResponseWriter = WriteHealthCheckReadyResponse,
                    Predicate = (check) => check.Tags.Contains("ready"),
                    AllowCachingResponses = false
                });
                endpoints.MapHealthChecks("/health/live", new HealthCheckOptions()
                {
                    ResultStatusCodes = {
                        [HealthStatus.Healthy] = StatusCodes.Status200OK,
                        [HealthStatus.Degraded] = StatusCodes.Status500InternalServerError,
                        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable,
                    },
                    ResponseWriter = WriteHealthCheckLiveResponse,
                    Predicate = (check) => !check.Tags.Contains("ready"),
                    AllowCachingResponses = false
                });
                endpoints.MapHealthChecks("/healthui", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                    AllowCachingResponses = false
                });
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseHealthChecksUI();
        }

        private Task WriteHealthCheckLiveResponse(HttpContext httpContext, HealthReport result)
        {
            httpContext.Response.ContentType = "application/json";
            var json = new JObject(
                new JProperty("OverallStatus", result.Status.ToString()),
                new JProperty("TotalChecksDuration", result.TotalDuration.TotalSeconds.ToString("0:0.00"))
                );
            return httpContext.Response.WriteAsync(json.ToString(Newtonsoft.Json.Formatting.Indented));
        }

        private Task WriteHealthCheckReadyResponse(HttpContext httpContext, HealthReport result)
        {
            httpContext.Response.ContentType = "application/json";
            var json = new JObject(
                new JProperty("OverallStatus", result.Status.ToString()),
                new JProperty("TotalChecksDuration", result.TotalDuration.TotalSeconds.ToString("0:0.00")),
                new JProperty("DependencyHealthChecks", new JObject(result.Entries.Select(dicItem =>
                    new JProperty(dicItem.Key, new JObject(
                        new JProperty("Status", dicItem.Value.Status.ToString()),
                        new JProperty("Duration", dicItem.Value.Duration.TotalSeconds.ToString("0:0.00")),
                        new JProperty("Exception", dicItem.Value.Exception?.Message),
                        new JProperty("Data", new JObject(dicItem.Value.Data.Select(dicData =>
                            new JProperty(dicData.Key, dicData.Value))))
                        ))
                    )))
                );
            return httpContext.Response.WriteAsync(json.ToString(Newtonsoft.Json.Formatting.Indented));
        }
    }
}
