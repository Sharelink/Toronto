using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using TorontoService;
using BahamutCommon;
using BahamutService.Model;
using MongoDB.Driver;

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

    }

    public class TorontoServiceProviderUseMongoDb:TorontoServiceProvider
    {
        IMongoClient Client = new MongoClient(MongoUrl.Create(Startup.SharelinkDBConfig.Url));
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
