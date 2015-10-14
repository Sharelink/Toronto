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
    [Route("[controller]")]
    public class NewSharelinkUsersController : TorontoAPIController
    {
        // POST api/values
        [HttpPost]
        public object Post(string accountId, string accessToken,string nickName,string signText)
        {
            var tokenService = Startup.ServicesProvider.GetTokenService();
            if (tokenService.ValidateToGetSessionData(Startup.Appkey,accountId,accessToken) != null)
            {
                var newUser = new SharelinkUser()
                {
                    AccountId = accountId,
                    NickName = nickName,
                    CreateTime = DateTime.Now,
                    NoteName = nickName,
                    SignText = signText
                };
                var userService = this.UseSharelinkUserService().GetSharelinkUserService();
                var taskResult = Task.Run(async () =>
                {
                    var user = await userService.CreateNewUser(newUser);
                    await userService.AskForLink(user.Id.ToString(), user.Id.ToString());
                    return user;
                });
                newUser = taskResult.Result;
                var sessionData = tokenService.ValidateAccessToken(Startup.Appkey, accountId, accessToken, newUser.Id.ToString());
                return new
                {
                    Succeed = true,
                    AppToken = sessionData.UserSessionData.AppToken,
                    UserId = sessionData.UserSessionData.UserId,
                    APIServer = Startup.APIUrl,
                    FileAPIServer = Startup.FileApiUrl,
                    ChicagoServer = string.Format("{0}:{1}", Startup.ChicagoServerAddress, Startup.ChicagoServerPort)
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
