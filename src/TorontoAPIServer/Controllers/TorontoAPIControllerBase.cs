using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TorontoService;
using BahamutCommon;
using BahamutService.Model;
using MongoDB.Driver;
using NLog;

namespace TorontoAPIServer.Controllers
{
    public class TorontoAPIController : Controller, IBahamutServiceBuilder, IGetAccountSessionData
    {
        public TorontoAPIController()
        {
        }
        private IDictionary<string, object> _torontoService = new Dictionary<string,object>();
        private IBahamutServiceProvider _torontoServiceProvider;
        public AccountSessionData UserSessionData
        {
            get { return Request.HttpContext.Items["AccountSessionData"] as AccountSessionData; }
        }

        public IBahamutServiceProvider ServiceProvider
        {
            get
            {
                if(_torontoServiceProvider == null)
                {
                    _torontoServiceProvider = TorontoServiceProviderUseMongoDb.GetInstance(UserSessionData);
                }
                return _torontoServiceProvider;
            }
        }

        public IDictionary<string, object> Properties
        {
            get
            {
                return _torontoService;
            }
        }

        public void LogInfo(string message,params object[] args)
        {
            LogManager.GetLogger("Info").Info(message, args);
        }

        public void LogWarning(string message,Exception exception = null)
        {
            if(exception == null)
            {
                LogManager.GetLogger("Warning").Warn(message);
            }
            else
            {
                LogManager.GetLogger("Warning").Warn(exception, message);
            }
        }
    }

    public class TorontoServiceProviderUseMongoDb:TorontoServiceProvider
    {
        IMongoClient Client = new MongoClient(MongoUrl.Create(Startup.SharelinkDBUrl));
        public override object GetService(Type type)
        {
            return GetService(type, Client);
        }


        public static IBahamutServiceProvider GetInstance(AccountSessionData UserSessionData)
        {
            return new TorontoServiceProviderUseMongoDb()
            {
                UserSessionData = UserSessionData
            };
        }
    }
}
