using BahamutCommon;
using BahamutService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TorontoService
{
    public class TorontoServiceProvider : IBahamutServiceProvider, IAccountSessionData
    {
        public AccountSessionData UserSessionData { get; set; }

        virtual public object GetService(Type type)
        {
            var service = type.GetConstructor(Type.EmptyTypes).Invoke(null);
            var iAccountSessionData = service as IAccountSessionData;
            if (iAccountSessionData != null)
            {
                iAccountSessionData.UserSessionData = UserSessionData;
            }
            return service;
        }

        virtual public object GetService(Type type, params object[] args)
        {
            var argsTypes = from arg in args select arg.GetType();
            var service = type.GetConstructor(argsTypes.ToArray()).Invoke(args);
            var iAccountSessionData = service as IAccountSessionData;
            if(iAccountSessionData != null)
            {
                iAccountSessionData.UserSessionData = UserSessionData;
            }
            return service;
        }

    }
}
