using System.Collections.Generic;
using Microsoft.AspNet.Mvc;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860
using TorontoService;
using System.Threading.Tasks;
using System;
using BahamutService.Model;
using BahamutCommon;

namespace TorontoAPIServer.Controllers
{
    [Route("api/[controller]")]
    public class AccountsController : TorontoAPIController
    {
        // GET /Accounts : return my account information
        [HttpGet]
        public object Get()
        {
            var accountService = Startup.ServicesProvider.GetAccountService();
            var account = accountService.GetAccount(UserSessionData.AccountId);
            return new
            {
                accountId = account.AccountID,
                accountName = account.AccountName,
                createTime = DateTimeUtil.ToString(account.CreateTime),
                name = account.Name,
                mobile = account.Mobile,
                email = account.Email
            };
        }

        // PUT /Accounts (name,birthdate) : update my account properties
        [HttpPut]
        public void Put(string name, DateTime birthdate)
        {
            var accountService = Startup.ServicesProvider.GetAccountService();
            if (name != null)
            {
                accountService.ChangeName(UserSessionData.AccountId,name);
            }
            if (birthdate != null)
            {
                accountService.ChangeAccountBirthday(UserSessionData.AccountId,birthdate);
            }
        }

    }
}
