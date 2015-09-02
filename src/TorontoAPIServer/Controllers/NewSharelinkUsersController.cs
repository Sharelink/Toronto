using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using BahamutService.Model;
using TorontoService;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace TorontoAPIServer.Controllers
{
    [Route("api/[controller]")]
    public class NewSharelinkUsersController : Controller
    {
        // POST api/values
        [HttpPost]
        public AccessTokenValidateResult Post([FromBody]string appkey, string accountId, string accessToken)
        {
            var tokenService = Startup.ServicesProvider.GetTokenService();
            var tokenResult = tokenService.ValidateAccessToken(appkey, accountId, accessToken);
            if (tokenResult.Succeed)
            {
                var accountService = new AccountService();
                if (accountService.BindAccountUser(accountId, tokenResult.UserId))
                {
                    return tokenResult;
                }
                return tokenResult;
            }
            else
            {
                return new AccessTokenValidateResult() { Message = "Validate Failed" };  
            }
        }
    }
}
