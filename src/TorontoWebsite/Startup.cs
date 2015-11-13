using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Framework.DependencyInjection;
using Microsoft.AspNet.Hosting;
using Microsoft.Dnx.Runtime;
using Microsoft.Framework.Configuration;
using NLog;

namespace TorontoWebsite
{
    public class Startup
    {
        public static IConfiguration Configuration { get; set; }
        public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv)
        {
            // Setup configuration sources.
            var builder = new ConfigurationBuilder()
                .SetBasePath(appEnv.ApplicationBasePath);
            if (env.IsDevelopment())
            {
                builder.AddJsonFile("config_debug.json");
            }
            else
            {
                builder.AddJsonFile("config.json");
            }
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            //Log
            var logConfig = new NLog.Config.LoggingConfiguration();
            var fileTarget = new NLog.Targets.FileTarget();
            fileTarget.FileName = Configuration["Data:Log:logFile"];
            fileTarget.Name = "FileLogger";
            fileTarget.Layout = @"${date:format=HH\:mm\:ss} ${logger}:${message};${exception}";
            logConfig.AddTarget(fileTarget);
            logConfig.LoggingRules.Add(new NLog.Config.LoggingRule("*", NLog.LogLevel.Debug, fileTarget));
            if (env.IsDevelopment())
            {
                var consoleLogger = new NLog.Targets.ColoredConsoleTarget();
                consoleLogger.Name = "ConsoleLogger";
                consoleLogger.Layout = @"${date:format=HH\:mm\:ss} ${logger}:${message};${exception}";
                logConfig.AddTarget(consoleLogger);
                logConfig.LoggingRules.Add(new NLog.Config.LoggingRule("*", NLog.LogLevel.Debug, consoleLogger));
            }
            LogManager.Configuration = logConfig;

            // Add the platform handler to the request pipeline.
            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            LogManager.GetLogger("Website").Info("Started!");
        }
    }
}
