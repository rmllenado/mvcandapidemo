using System;
using System.Collections.Generic;
using System.Web.Security;
using System.Web.SessionState;

namespace Webforms
{
    public class Global : System.Web.HttpApplication
    {
        private AppConfig _appConfig;

        protected void Application_Start(object sender, EventArgs e)
        {
            _appConfig = AppConfig.ChangeByEnvironmentVar("ASPNET_ENVIRONMENT");
            //_appConfig = AppConfig.ChangeByEnvironmentFile();
        }

        protected void Session_Start(object sender, EventArgs e)
        {
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {
            if (_appConfig != null)
                _appConfig.Dispose();
        }
    }
}