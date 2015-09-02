using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using TorontoService;
using BahamutCommon;
using BahamutService.Model;

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
            get { return Context.Items["AccountSessionData"] as AccountSessionData; }
        }

        public IBahamutServiceProvider ServiceProvider
        {
            get
            {
                if(_torontoServiceProvider == null)
                {
                    _torontoServiceProvider = TorontoServiceProvider.GetInstance(UserSessionData);
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
}
