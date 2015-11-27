using System;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.DependencyInjection;
using TorontoAPIServer.Authentication;
using Microsoft.Extensions.Configuration;
using BahamutService;
using BahamutCommon;
using ServerControlService.Service;
using ServerControlService.Model;
using ServiceStack.Redis;
using NLog;
using System.Collections.Generic;
using Microsoft.Extensions.PlatformAbstractions;

namespace TorontoAPIServer
{
    public class Startup
    {
        public static IConfiguration Configuration { get; set; }
        public static IServiceProvider ServicesProvider { get; private set; }
        public static string Appkey { get; private set; }
        public static string Appname { get; private set; }
        public static string Server { get; set; }
        public static string APIUrl { get; private set; }
        public static string FileApiUrl { get; private set; }
        public static string SharelinkDBUrl { get; private set; }
        public static string BahamutDBConnectionString { get; private set; }
        public static BahamutAppInstance BahamutAppInstance { get; private set; }
        public static string ChicagoServerAddress { get; private set; }
        public static int ChicagoServerPort { get; private set; }
        public static IDictionary<string, string> ValidatedUsers { get; private set; }

        public static PublishSubscriptionManager PublishSubscriptionManager { get; private set; }

        public static IHostingEnvironment HostingEnvironment { get; private set; }
        public static IApplicationEnvironment AppEnvironment { get; private set; }

        public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv)
        {
            // Setup configuration sources.
            HostingEnvironment = env;
            AppEnvironment = appEnv;
            ValidatedUsers = new Dictionary<string, string>();
            var builder = new ConfigurationBuilder()
                .SetBasePath(appEnv.ApplicationBasePath);
            if (env.IsDevelopment())
            {
                builder.AddJsonFile("config_debug.json");
            }
            else
            {
                builder.AddJsonFile("/etc/bahamut/toronto.json");
            }

            builder.AddJsonFile("new_sharelinker_config.json");
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();

            BahamutDBConnectionString = Configuration["Data:BahamutDBConnection:connectionString"];
            FileApiUrl = Configuration["Data:FileServer:url"];
            Server = Configuration["Data:App:url"];
            ChicagoServerAddress = Configuration["Data:ChicagoServer:host"];
            ChicagoServerPort = int.Parse(Configuration["Data:ChicagoServer:port"]);

            Appkey = Configuration["Data:App:appkey"];
            Appname = Configuration["Data:App:appname"];
            APIUrl = Server + "/api";
            SharelinkDBUrl = Configuration["Data:SharelinkDBServer:url"];
        }

        // This method gets called by a runtime.
        // Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            var tokenServerUrl = Configuration["Data:TokenServer:url"].Replace("redis://", "");
            var TokenServerClientManager = new PooledRedisClientManager(tokenServerUrl);

            var serverControlUrl = Configuration["Data:ControlServiceServer:url"].Replace("redis://", "");
            var ControlServerServiceClientManager = new PooledRedisClientManager(serverControlUrl);
            services.AddInstance(new ServerControlManagementService(ControlServerServiceClientManager));
            services.AddInstance(new TokenService(TokenServerClientManager));
            services.AddInstance(new BahamutAccountService(BahamutDBConnectionString));

            var pubsubServerUrl = Configuration["Data:MessagePubSubServer:url"].Replace("redis://", "");
            var pbClientManager = new PooledRedisClientManager(pubsubServerUrl);

            var messageCacheServerUrl = Configuration["Data:MessageCacheServer:url"].Replace("redis://", "");
            var mcClientManager = new PooledRedisClientManager(messageCacheServerUrl);
            PublishSubscriptionManager = new PublishSubscriptionManager(pbClientManager,mcClientManager);
            // Uncomment the following line to add Web API services which makes it easier to port Web API 2 controllers.
            // You will also need to add the Microsoft.AspNet.Mvc.WebApiCompatShim package to the 'dependencies' section of project.json.
            // services.AddWebApiConventions();
        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            ServicesProvider = app.ApplicationServices;

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

            //Regist App Instance
            var serverMgrService = ServicesProvider.GetServerControlManagementService();
            var appInstance = new BahamutAppInstance()
            {
                Appkey = Appkey,
                InstanceServiceUrl = Configuration["Data:App:url"],
                Region = Configuration["Data:App:region"]
            };
            try
            {
                BahamutAppInstance = serverMgrService.RegistAppInstance(appInstance);
                var observer = serverMgrService.StartKeepAlive(BahamutAppInstance);
                observer.OnExpireError += KeepAliveObserver_OnExpireError;
                observer.OnExpireOnce += KeepAliveObserver_OnExpireOnce;
                LogManager.GetLogger("Toronto").Info("Bahamut App Instance:" + BahamutAppInstance.Id.ToString());
                LogManager.GetLogger("KeepAlive").Info("Keep Server Instance Alive To Server Controller Thread Started!");
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex, "Unable To Regist App Instance");
            }

            app.UseMiddleware<BasicAuthentication>(Appkey);
            // Configure the HTTP request pipeline.
            app.UseStaticFiles();

            // Add MVC to the request pipeline.
            app.UseMvc();
            // Add the following route for porting Web API 2 controllers.
            // routes.MapWebApiRoute("DefaultApi", "api/{controller}/{id?}");

            LogManager.GetCurrentClassLogger().Info("Toronto Started!");
        }

        private void KeepAliveObserver_OnExpireOnce(object sender, KeepAliveObserverEventArgs e)
        {
            
        }

        private void KeepAliveObserver_OnExpireError(object sender, KeepAliveObserverEventArgs e)
        {
            LogManager.GetLogger("KeepAlive").Error(string.Format("Expire Server Error.Instance:{0}", e.Instance.Id), e);
            var serverMgrService = ServicesProvider.GetServerControlManagementService();
            BahamutAppInstance.OnlineUsers = ValidatedUsers.Count;
            serverMgrService.ReActiveAppInstance(BahamutAppInstance);
        }

        // Entry point for the application.
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }

    public static class IGetBahamutServiceExtension
    {

        public static BahamutAccountService GetAccountService(this IServiceProvider provider)
        {
            return provider.GetService<BahamutAccountService>();
        }

        public static ServerControlManagementService GetServerControlManagementService(this IServiceProvider provider)
        {
            return provider.GetService<ServerControlManagementService>();
        }

        public static TokenService GetTokenService(this IServiceProvider provider)
        {
            return provider.GetService<TokenService>();
        }
    }
}
