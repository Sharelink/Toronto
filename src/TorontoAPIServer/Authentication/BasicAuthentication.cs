using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using TorontoService;
using System.Net;
using BahamutService;

namespace TorontoAPIServer.Authentication
{

    // You may need to install the Microsoft.AspNet.Http.Abstractions package into your project
    public class BasicAuthentication
    {
        private static IDictionary<string, bool> _arrowRouteMap = new Dictionary<string, bool>();
        public static IDictionary<string,bool> ArrowRoute { get { return _arrowRouteMap; } }
        private readonly RequestDelegate _next;
        public string Appkey { get; private set; }
        public TokenService tokenService { get; set; }

        public BasicAuthentication(RequestDelegate next,string appkey)
        {
            Appkey = appkey;
            _next = next;
            tokenService = Startup.ServicesProvider.GetTokenService();
        }

        public Task Invoke(HttpContext httpContext)
        {
            Console.WriteLine(httpContext.Request.Path);
            if (httpContext.Request.Path == "/Tokens" || httpContext.Request.Path == "/NewSharelinkUsers")
            {
                return _next(httpContext);
            }
            var userId = httpContext.Request.Headers["userId"];
            var token = httpContext.Request.Headers["token"];
            var res = tokenService.ValidateAppToken(Appkey, userId, token).Result;
            
            if(res != null)
            {
                httpContext.Items.Add("AccountSessionData", res);
                return _next(httpContext);
            }
            else
            {
                httpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
            
        }
    }
}
