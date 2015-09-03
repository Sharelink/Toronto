using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using BahamutService.Model;
using TorontoService;
using TorontoModel.MongodbModel;
using System.Net;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace TorontoAPIServer.Controllers
{
    [Route("api/[controller]")]
    public class NewSharelinkUsersController : TorontoAPIController
    {
        // POST api/values
        [HttpPost]
        public async Task<object> Post([FromBody]string appkey, string accountId, string accessToken)
        {
            var tokenService = Startup.ServicesProvider.GetTokenService();
            if(appkey != Startup.Appkey)
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "Validate Fail,Can't Not Regist New User";
            }
            else if (tokenService.ValidateToGetSessionData(Startup.Appkey,accountId,accessToken) != null)
            {
                var newUser = new SharelinkUser()
                {
                    AccountId = accountId
                };
                var userService = this.UseSharelinkUserService().GetSharelinkUserService();
                newUser = await userService.CreateNewUser(newUser);
                var sessionData = tokenService.ValidateAccessToken(Startup.Appkey, accountId, newUser.Id.ToString(), accessToken);
                return new
                {
                    Succeed = true,
                    AppToken = sessionData.UserSessionData.AppToken,
                    UserId = sessionData.UserSessionData.UserId,
                    APIServer = Startup.APIUrl,
                    FileAPIServer = "http://192.168.0.168:8089/api"
                };
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "Validate Fail,Can't Not Regist New User";
            }
        }
    }
}
