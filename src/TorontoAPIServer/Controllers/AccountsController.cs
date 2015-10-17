using System.Collections.Generic;
using Microsoft.AspNet.Mvc;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860
using TorontoService;
using System.Threading.Tasks;
using System;
using BahamutService.Model;
using BahamutCommon;
using System.Net;

namespace TorontoAPIServer.Controllers
{
    [Route("api/[controller]")]
    public class AccountsController : TorontoAPIController
    {
        // GET /Accounts : return my account information
        [HttpGet]
        public async Task<object> Get()
        {
            return await Task.Run(() =>
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
            });
        }

        // PUT /Accounts/Name (name) : update my account name properties
        [HttpPut("Name")]
        public async void PutName(string name)
        {
            await Task.Run(() =>
            {
                var accountService = Startup.ServicesProvider.GetAccountService();
                if (name != null)
                {
                    if (!accountService.ChangeName(UserSessionData.AccountId, name))
                    {
                        Response.StatusCode = (int)HttpStatusCode.NotModified;
                    }
                }
                else
                {
                    Response.StatusCode = (int)HttpStatusCode.NotModified;
                }
            });
        }

        // PUT /Accounts/Name (name) : update my account birth properties
        [HttpPut("BirthDate")]
        public async void PutBirthDate(string birthdate)
        {
            await Task.Run(() =>
            {
                var accountService = Startup.ServicesProvider.GetAccountService();
                if (birthdate != null)
                {
                    if (!accountService.ChangeAccountBirthday(UserSessionData.AccountId, DateTimeUtil.ToDate(birthdate)))
                    {
                        Response.StatusCode = (int)HttpStatusCode.NotModified;
                    }
                }
                else
                {
                    Response.StatusCode = (int)HttpStatusCode.NotModified;
                }
            });
        }

    }
}
