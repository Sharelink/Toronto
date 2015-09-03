using System.Collections.Generic;
using Microsoft.AspNet.Mvc;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860
using TorontoService;
using System.Threading.Tasks;
using System;
using BahamutService.Model;

namespace TorontoAPIServer.Controllers
{
    [Route("api/[controller]")]
    public class AccountsController : TorontoAPIController
    {
        // GET /Accounts : return my account information
        [HttpGet]
        public Account Get()
        {
            //var accountService = this.UseAccountService().GetAccountService();
            //return accountService.GetAccount();
            return null;
        }

        // PUT /Accounts (name,birthdate) : update my account properties
        [HttpPut]
        public void Put(string name, DateTime birthdate)
        {
            //var accountService = this.UseAccountService().GetAccountService();
            //if (name != null)
            //{
            //    accountService.ChangeName(name);
            //}
            //if (birthdate != null)
            //{
            //    accountService.ChangeAccountBirthday(birthdate);
            //}
        }

    }
}
