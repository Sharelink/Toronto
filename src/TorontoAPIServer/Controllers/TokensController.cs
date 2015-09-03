using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using TorontoService;
using System.Net;
using BahamutService;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace TorontoAPIServer.Controllers
{

    [Route("[controller]")]
    public class TokensController : TorontoAPIController
    {
        [HttpGet]
        public async Task<object> Get(string appkey, string accountId, string accessToken)
        {
            var tokenService = Startup.ServicesProvider.GetTokenService();
            var userService = this.UseSharelinkUserService().GetSharelinkUserService();
            var userId = await userService.GetUserIdOfAccountId(accountId);
            if (userId == null)
            {
                return new
                {
                    NewUser = true,
                    RegistAPI = "http://192.168.0.168:8088/api"
                };
            }
            var tokenResult = tokenService.ValidateAccessToken(appkey, accountId, accessToken, userId);
            if (tokenResult.Succeed)
            {
                return new
                {
                    AppToken = tokenResult.UserSessionData.AppToken,
                    UserId = userId,
                    APIServer = Startup.APIUrl,
                    FileAPIServer = "http://192.168.0.168:8089/api"
                };
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return tokenResult.Message;
            }
        }

        // DELETE api/values/5
        [HttpDelete]
        public bool Delete(string appkey, string userId, string appToken)
        {
            var tokenService = Startup.ServicesProvider.GetTokenService();
            return tokenService.ReleaseAppToken(appkey, userId, appToken);
        }
    }
}
