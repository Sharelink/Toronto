using System;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using BahamutService;
using BahamutCommon;
using ServerControlService.Service;
using ServerControlService.Model;
using ServiceStack.Redis;
using NLog;
using System.Collections.Generic;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.AspNet.Mvc.Filters;
using System.Threading.Tasks;
using System.Net;
using System.Linq;
using NLog.Targets;
using NLog.Config;
using BahamutService.Service;

namespace TorontoAPIServer
{
    public class Startup
    {
        public static IHostingEnvironment HostingEnvironment { get; private set; }
        public static IApplicationEnvironment AppEnvironment { get; private set; }
        public static IConfiguration Configuration { get; set; }
        public static IServiceProvider ServicesProvider { get; private set; }

        public static BahamutAppInstance BahamutAppInstance { get; private set; }
        public static string Server { get; private set; }
        public static string Appkey { get; private set; }
        public static string Appname { get; private set; }
        public static string APIUrl { get; private set; }

        public static string AuthServerUrl { get { return Configuration["Data:AuthServer:url"]; } }
        public static string FileApiUrl { get { return Configuration["Data:FileServer:url"]; } }
        public static string SharelinkDBUrl { get { return Configuration["Data:SharelinkDBServer:url"]; } }
        public static string ChicagoServerAddress { get { return Configuration["Data:ChicagoServer:host"]; } }
        public static int ChicagoServerPort { get { return int.Parse(Configuration["Data:ChicagoServer:port"]); } }

        public static IDictionary<string, string> ValidatedUsers { get; private set; }
        public static IList<string> SharelinkCenterList { get; private set; }
        public static IDictionary<string,string> SharelinkCenters { get; private set; }
        public static IList<string> HotThemes { get; set; }

        public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv)
        {
            // Setup configuration sources.
            HostingEnvironment = env;
            AppEnvironment = appEnv;
            ValidatedUsers = new Dictionary<string, string>();
            HotThemes = new List<string>(1000);
            ReadConfig();
            SetServerConfig();
            InitSharelinkCenter();
        }

        private static void SetServerConfig()
        {
            Server = Configuration["Data:App:url"];
            Appkey = Configuration["Data:App:appkey"];
            Appname = Configuration["Data:App:appname"];
            APIUrl = Server + "/api";
        }

        private static void ReadConfig()
        {
            var builder = new ConfigurationBuilder()
                            .SetBasePath(AppEnvironment.ApplicationBasePath);
            if (HostingEnvironment.IsDevelopment())
            {
                builder.AddJsonFile("config_debug.json");
                builder.AddJsonFile("new_sharelinker_config.json");
            }
            else
            {
                builder.AddJsonFile("/etc/bahamut/toronto.json");
                builder.AddJsonFile("/etc/bahamut/new_sharelinker_config.json");
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        private void InitSharelinkCenter()
        {
            SharelinkCenterList = new List<string>();
            SharelinkCenters = new Dictionary<string, string>();
            var centers = Configuration.GetSection("SharelinkCenter:centers").GetChildren();
            foreach (var c in centers)
            {
                var region = c["region"];
                var id = c["id"];
                SharelinkCenterList.Add(id);
                SharelinkCenters[region] = id;
            }
        }

        // This method gets called by a runtime.
        // Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(config => {
                config.Filters.Add(new BahamutAspNetCommon.LogExceptionFilter());
            });

            var tokenServerUrl = Configuration["Data:TokenServer:url"].Replace("redis://", "");
            var TokenServerClientManager = new PooledRedisClientManager(tokenServerUrl);

            var serverControlUrl = Configuration["Data:ControlServiceServer:url"].Replace("redis://", "");
            var ControlServerServiceClientManager = new PooledRedisClientManager(serverControlUrl);
            services.AddInstance(new ServerControlManagementService(ControlServerServiceClientManager));
            services.AddInstance(new TokenService(TokenServerClientManager));

            var pubsubServerUrl = Configuration["Data:MessagePubSubServer:url"].Replace("redis://", "");
            var pbClientManager = new PooledRedisClientManager(pubsubServerUrl);

            var messageCacheServerUrl = Configuration["Data:MessageCacheServer:url"].Replace("redis://", "");
            var mcClientManager = new PooledRedisClientManager(messageCacheServerUrl);

            var pbService = new BahamutPubSubService(pbClientManager, mcClientManager);
            services.AddInstance(pbService);
            
        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            ServicesProvider = app.ApplicationServices;

            //Log
            var logConfig = new LoggingConfiguration();
            LoggerLoaderHelper.LoadLoggerToLoggingConfig(logConfig, Configuration, "Data:Log:fileLoggers");

            if (env.IsDevelopment())
            {
                LoggerLoaderHelper.AddConsoleLoggerToLogginConfig(logConfig);
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
                LogManager.GetLogger("Main").Info("Bahamut App Instance:" + BahamutAppInstance.Id.ToString());
                LogManager.GetLogger("Main").Info("Keep Server Instance Alive To Server Controller Thread Started!");
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("Main").Error(ex, "Unable To Regist App Instance");
            }

            //Authentication
            var openRoutes = new string[]
            {
                "/Tokens",
                "/NewSharelinkers"
            };
            app.UseMiddleware<BahamutAspNetCommon.TokenAuthentication>(Appkey, ServicesProvider.GetTokenService(), openRoutes);

            // Add MVC to the request pipeline.
            app.UseMvc();
            // Add the following route for porting Web API 2 controllers.
            // routes.MapWebApiRoute("DefaultApi", "api/{controller}/{id?}");
            LogManager.GetLogger("Main").Info("Toronto Started!");
        }

        private void KeepAliveObserver_OnExpireOnce(object sender, KeepAliveObserverEventArgs e)
        {
            
        }

        private void KeepAliveObserver_OnExpireError(object sender, KeepAliveObserverEventArgs e)
        {
            LogManager.GetLogger("Main").Error(string.Format("Expire Server Error.Instance:{0}", e.Instance.Id), e);
            var serverMgrService = ServicesProvider.GetServerControlManagementService();
            BahamutAppInstance.OnlineUsers = ValidatedUsers.Count;
            serverMgrService.ReActiveAppInstance(BahamutAppInstance);
        }

        // Entry point for the application.
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }

    public static class IGetBahamutServiceExtension
    {

        public static BahamutPubSubService GetBahamutPubSubService(this IServiceProvider provider)
        {
            return provider.GetService<BahamutPubSubService>();
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
