using Serilog;
using SerilogWeb.Classic.Enrichers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;

namespace WebformsWithLogging45
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            Log.Logger = new LoggerConfiguration()
                        .Enrich.With<HttpRequestIdEnricher>()
                        .WriteTo.Seq("http://localhost:8081")
                        .CreateLogger();
            // Code that runs on application startup
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}