using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Webforms
{
    public abstract class AppConfig : IDisposable
    {
        static string[] VALID_ENVIRONMENTS = new[] { "DEV", "QA", "UAT", "PROD" };
        static string DEFAULT_ENVIRONMENT = "DEV";

        public static AppConfig Change(string path)
        {
            return new ChangeAppConfig(path);
        }

        public static AppConfig ChangeByEnvironmentFile()
        {
            string env = DEFAULT_ENVIRONMENT;

            // look for ENVIRONMENT file
            string location = HttpContext.Current.Server.MapPath(".");
            string environmentFile = $"{location}\\ENVIRONMENT";

            // if not found, look for ENVIRONMENT.txt
            if (!File.Exists(environmentFile))
                environmentFile = environmentFile + ".txt";

            // get the environment setting
            if (File.Exists(environmentFile))
                env = File.ReadLines(environmentFile).FirstOrDefault() ?? DEFAULT_ENVIRONMENT;

            // if it's not a valid setting, fallback to default env
            if (!VALID_ENVIRONMENTS.Any(a => a.Equals(env, StringComparison.OrdinalIgnoreCase)))
                env = DEFAULT_ENVIRONMENT;

            var configFile = $@"{location}\web.{env}.config";

            return Change(configFile);
        }

        public static AppConfig ChangeByEnvironmentVar(string environmentVariable)
        {
            string env = DEFAULT_ENVIRONMENT;

            string location = HttpContext.Current.Server.MapPath(".");

            env = Environment.GetEnvironmentVariable(environmentVariable, EnvironmentVariableTarget.Machine);

            if (!VALID_ENVIRONMENTS.Any(a => a.Equals(env, StringComparison.OrdinalIgnoreCase)))
                env = DEFAULT_ENVIRONMENT;

            var configFile = $@"{location}\web.{env}.config";

            return Change(configFile);
        }

        public abstract void Dispose();

        private class ChangeAppConfig : AppConfig
        {
            private readonly string oldConfig =
                AppDomain.CurrentDomain.GetData("APP_CONFIG_FILE").ToString();

            private bool disposedValue;

            public ChangeAppConfig(string path)
            {
                AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", path);
                ResetConfigMechanism();
            }

            public override void Dispose()
            {
                if (!disposedValue)
                {
                    AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", oldConfig);
                    ResetConfigMechanism();


                    disposedValue = true;
                }
                GC.SuppressFinalize(this);
            }

            private static void ResetConfigMechanism()
            {
                typeof(ConfigurationManager)
                    .GetField("s_initState", BindingFlags.NonPublic |
                                             BindingFlags.Static)
                    .SetValue(null, 0);

                typeof(ConfigurationManager)
                    .GetField("s_configSystem", BindingFlags.NonPublic |
                                                BindingFlags.Static)
                    .SetValue(null, null);

                typeof(ConfigurationManager)
                    .Assembly.GetTypes()
                    .Where(x => x.FullName ==
                                "System.Configuration.ClientConfigPaths")
                    .First()
                    .GetField("s_current", BindingFlags.NonPublic |
                                           BindingFlags.Static)
                    .SetValue(null, null);
            }
        }
    }
}