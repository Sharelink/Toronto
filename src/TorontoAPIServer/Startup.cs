﻿using System;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Framework.DependencyInjection;
using TorontoAPIServer.Authentication;
using Microsoft.Framework.Configuration;
using BahamutService;
using BahamutCommon;
using Microsoft.Dnx.Runtime;
using ServerControlService.Service;
using ServerControlService.Model;
using ServiceStack.Redis;
using BahamutFireService.Service;
using Microsoft.Framework.Logging;

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
        public static string SharelinkDBUrl { get; private set; }
        public static string BahamutDBConnectionString { get; private set; }
        public static BahamutAppInstance BahamutAppInstance { get; private set; }
        public static string ChicagoServerAddress { get; private set; }
        public static int ChicagoServerPort { get; private set; }

        public static PublishSubscriptionManager PublishSubscriptionManager { get; private set; }

        public static IHostingEnvironment HostingEnvironment { get; private set; }
        public static IApplicationEnvironment AppEnvironment { get; private set; }

        public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv)
        {
            // Setup configuration sources.
            HostingEnvironment = env;
            AppEnvironment = appEnv;
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

            var TokenServerClientManager = new RedisManagerPool(Configuration["Data:TokenServer:url"]);
            var ControlServerServiceClientManager = new RedisManagerPool(Configuration["Data:ControlServiceServer:url"]);
            services.AddInstance(new ServerControlManagementService(ControlServerServiceClientManager));
            services.AddInstance(new TokenService(TokenServerClientManager));
            services.AddInstance(new BahamutAccountService(BahamutDBConnectionString));
            services.AddInstance(new FireAccesskeyService());

            var pbClientManager = new RedisManagerPool(Configuration["Data:MessagePubSubServer:url"]);
            var mcClientManager = new RedisManagerPool(Configuration["Data:MessageCacheServer:url"]);
            PublishSubscriptionManager = new PublishSubscriptionManager(pbClientManager,mcClientManager);
            // Uncomment the following line to add Web API services which makes it easier to port Web API 2 controllers.
            // You will also need to add the Microsoft.AspNet.Mvc.WebApiCompatShim package to the 'dependencies' section of project.json.
            // services.AddWebApiConventions();
        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            ServicesProvider = app.ApplicationServices;

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
                serverMgrService.StartKeepAlive(BahamutAppInstance.Id);
            }
            catch (Exception)
            {
                Console.WriteLine("Can't connect to app center to regist");
            }
            loggerFactory.MinimumLevel = LogLevel.Information;
            loggerFactory.AddConsole();
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
