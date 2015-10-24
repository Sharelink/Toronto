using System;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Framework.DependencyInjection;
using TorontoAPIServer.Authentication;
using Microsoft.Framework.Configuration;
using BahamutService;
using DataLevelDefines;
using BahamutCommon;
using Microsoft.Dnx.Runtime;
using ServerControlService.Service;
using ServerControlService.Model;
using ServiceStack.Redis;
using System.Net;
using BahamutFireService.Service;

namespace TorontoAPIServer
{
    public class Startup
    {
        public IConfiguration Configuration { get; set; }
        public static IServiceProvider ServicesProvider { get; private set; }
        public static string Appkey { get; private set; }
        public static string Appname { get; private set; }
        public static string Server { get; set; }
        public static string APIUrl { get; private set; }
        public static string FileApiUrl { get; private set; }
        public static IMongoDbServerConfig SharelinkDBConfig { get; private set; }
        public static string BahamutDBConnectionString { get; private set; }
        public static IRedisServerConfig ControlRedisServerConfig { get; private set; }
        public static BahamutAppInstance BahamutAppInstance { get; private set; }
        public static string ChicagoServerAddress { get; private set; }
        public static int ChicagoServerPort { get; private set; }
        public static RedisManagerPool MessagePubSubServerClientManager { get; private set; }
        public static RedisManagerPool MessageCacheServerClientManager { get; private set; }

        public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv)
        {
            // Setup configuration sources.

            var builder = new ConfigurationBuilder()
                .SetBasePath(appEnv.ApplicationBasePath)
                .AddJsonFile("config.json")
                .AddEnvironmentVariables();
            Configuration = builder.Build();
            FileApiUrl = Configuration["Data:FileServer:url"];
            Appkey = Configuration["Data:App:appkey"];
            Appname = Configuration["Data:App:appname"];
            Server = Configuration["Data:App:url"];
            APIUrl = Server + "/api";
            SharelinkDBConfig = new MongoDbServerConfig()
            {
                Url = Configuration["Data:SharelinkDBServer:url"]
            };
            BahamutDBConnectionString = Configuration["Data:BahamutDBConnection:connectionString"];
            ChicagoServerAddress = Configuration["Data:ChicagoServer:host"];
            ChicagoServerPort = int.Parse(Configuration["Data:ChicagoServer:port"]);
        }

        // This method gets called by a runtime.
        // Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            var TokenServerClientManager = new RedisManagerPool(Configuration["Data:TokenServer:url"]);
            var ControlServerServiceClientManager = new RedisManagerPool(Configuration["Data:ControlServiceServer:url"]);
            services.AddInstance(new ServerControlManagementService(ControlServerServiceClientManager));
            services.AddInstance(new TokenService(TokenServerClientManager));
            services.AddInstance(new BahamutAccountService(BahamutDBConnectionString));
            services.AddInstance(new FireAccesskeyService());
            MessagePubSubServerClientManager = new RedisManagerPool(Configuration["Data:MessagePubSubServer:url"]);
            MessageCacheServerClientManager = new RedisManagerPool(Configuration["Data:MessageCacheServer:url"]);
            // Uncomment the following line to add Web API services which makes it easier to port Web API 2 controllers.
            // You will also need to add the Microsoft.AspNet.Mvc.WebApiCompatShim package to the 'dependencies' section of project.json.
            // services.AddWebApiConventions();
        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            ServicesProvider = app.ApplicationServices;

            var serverMgrService = ServicesProvider.GetServerControlManagementService();
            var appInstance = new BahamutAppInstance()
            {
                Appkey = Appkey,
                InstanceServiceUrl = Configuration["server.urls"]
            };
            try
            {
                BahamutAppInstance = serverMgrService.RegistAppInstance(appInstance);
                serverMgrService.StartKeepAlive(BahamutAppInstance.Id);
            }
            catch (Exception)
            {
                Console.WriteLine("Can't connect to app center to regist");
            }

            app.UseMiddleware<BasicAuthentication>(Appkey);
            // Configure the HTTP request pipeline.
            app.UseStaticFiles();

            // Add MVC to the request pipeline.
            app.UseMvc();
            // Add the following route for porting Web API 2 controllers.
            // routes.MapWebApiRoute("DefaultApi", "api/{controller}/{id?}");
        }
    }

    public static class IGetBahamutServiceExtension
    {
        public static FireAccesskeyService GetFireAccesskeyService(this IServiceProvider provider)
        {
            return provider.GetService<FireAccesskeyService>();
        }

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
