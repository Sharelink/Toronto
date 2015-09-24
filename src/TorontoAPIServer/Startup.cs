﻿using System;
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
using BahamutService;
using DataLevelDefines;
using BahamutCommon;
using Microsoft.Dnx.Runtime;
using ServerControlService.Service;
using ServerControlService.Model;
using ServiceStack.Redis;

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
        public static IRedisServerConfig TokenServerConfig { get; private set; }
        public static IMongoDbServerConfig SharelinkDBConfig { get; private set; }
        public static string BahamutDBConnectionString { get; private set; }
        public static IRedisServerConfig ControlRedisServerConfig { get; private set; }
        public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv)
        {
            // Setup configuration sources.

            var builder = new ConfigurationBuilder(appEnv.ApplicationBasePath)
                .AddJsonFile("config.json")
                .AddEnvironmentVariables();
            builder.AddIniFile("hosting.ini").AddEnvironmentVariables();
            Configuration = builder.Build();
            FileApiUrl = "http://192.168.1.67:8089";
            Appkey = Configuration["Data:App:appkey"];
            Appname = Configuration["Data:App:appname"];
            APIUrl = Configuration["server.urls"] + "/api";
            Server = Configuration["server.urls"];
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

            var TokenServerClientManager = new RedisManagerPool(Configuration["Data:TokenServer:url"]);
            var ControlServerServiceClientManager = new RedisManagerPool(Configuration["Data:ControlServiceServer:url"]);
            services.AddInstance(new ServerControlManagementService(ControlServerServiceClientManager));
            services.AddInstance(new TokenService(TokenServerClientManager));
            services.AddInstance(new BahamutAccountService(BahamutDBConnectionString));
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
            appInstance = serverMgrService.RegistAppInstance(appInstance);
            serverMgrService.StartKeepAlive(appInstance.Id);

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
