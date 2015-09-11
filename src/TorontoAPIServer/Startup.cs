using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;
using TorontoAPIServer.Authentication;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.Runtime;
using BahamutService;
using DataLevelDefines;
using BahamutCommon;

namespace TorontoAPIServer
{
    public class Startup
    {
        public IConfiguration Configuration { get; set; }
        public static IServiceProvider ServicesProvider { get; private set; }
        public static string Appkey { get; private set; }
        public static string Appname { get; private set; }
        public static string APIUrl { get; private set; }
        public static IRedisServerConfig TokenServerConfig { get; private set; }
        public static IMongoDbServerConfig SharelinkDBConfig { get; private set; }
        public static string BahamutDBConnectionString { get; private set; }
        public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv)
        {
            // Setup configuration sources.

            var builder = new ConfigurationBuilder(appEnv.ApplicationBasePath)
                .AddJsonFile("config.json")
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            Appkey = Configuration["Data:App:appkey"];
            Appname = Configuration["Data:App:appname"];
            APIUrl = Configuration["Data:ServiceUrl"] + "/api";
            TokenServerConfig = new RedisServerConfig()
            {
                Db = long.Parse(Configuration["Data:TokenServer:Db"]),
                Host = Configuration["Data:TokenServer:Host"],
                Password = Configuration["Data:TokenServer:Password"],
                Port = int.Parse(Configuration["Data:TokenServer:Port"])
            };
            SharelinkDBConfig = new MongoDbServerConfig()
            {
                Url = Configuration["Data:SharelinkDBServer:Url"]
            };
            BahamutDBConnectionString = Configuration["Data:BahamutDBConnection:connectionString"];
        }

        // This method gets called by a runtime.
        // Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddInstance(new TokenService(TokenServerConfig));
            services.AddInstance(new BahamutAccountService(BahamutDBConnectionString));
            // Uncomment the following line to add Web API services which makes it easier to port Web API 2 controllers.
            // You will also need to add the Microsoft.AspNet.Mvc.WebApiCompatShim package to the 'dependencies' section of project.json.
            // services.AddWebApiConventions();
        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            ServicesProvider = app.ApplicationServices;
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
        public static BahamutAccountService GetAccountService(this IServiceProvider provider)
        {
            return provider.GetService<BahamutAccountService>();
        }

        public static TokenService GetTokenService(this IServiceProvider provider)
        {
            return provider.GetService<TokenService>();
        }
    }
}
