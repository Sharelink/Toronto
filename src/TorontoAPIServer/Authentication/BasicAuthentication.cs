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
    [System.AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public sealed class NoBasicAuthenticationAttribute : Attribute
    {
        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        readonly string positionalString;

        // This is a positional argument
        public NoBasicAuthenticationAttribute(string positionalString)
        {
            this.positionalString = positionalString;
            BasicAuthentication.ArrowRoute.Add(positionalString, true);
        }

        public string PositionalString
        {
            get { return positionalString; }
        }

        // This is a named argument
        public int NamedInt { get; set; }
    }

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

        public async Task<Task> Invoke(HttpContext httpContext)
        {
            Console.WriteLine(httpContext.Request.Path);
            if (httpContext.Request.Path == "/Tokens" || httpContext.Request.Path == "/NewSharelinkUsers")
            {
                return _next(httpContext);
            }
            var userId = httpContext.Request.Headers["userId"];
            var token = httpContext.Request.Headers["token"];
            var res = await tokenService.ValidateAppToken(Appkey, userId, token);
            
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
