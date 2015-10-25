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
            var user = await userService.GetUserOfAccountId(accountId);
            try
            {
                string userId = user == null ? null : user.Id.ToString();
                var tokenResult = tokenService.ValidateAccessToken(appkey, accountId, accessToken, userId);
                if (tokenResult.Succeed)
                {
                    if (userId == null)
                    {
                        return new
                        {
                            RegistAPIServer = Startup.Server
                        };
                    }
                    return new
                    {
                        AppToken = tokenResult.UserSessionData.AppToken,
                        UserId = userId,
                        APIServer = Startup.APIUrl,
                        FileAPIServer = Startup.FileApiUrl,
                        ChicagoServer = string.Format("{0}:{1}", Startup.ChicagoServerAddress, Startup.ChicagoServerPort)
                    };
                }
                else
                {
                    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return tokenResult.Message;
                }
            }
            catch (Exception)
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return "Server Error";
            }

        }

        // DELETE api/values/5
        [HttpDelete]
        public async Task<bool> Delete(string appkey, string userId, string appToken)
        {
            return await Task.Run(() =>
            {
                var tokenService = Startup.ServicesProvider.GetTokenService();
                return tokenService.ReleaseAppToken(appkey, userId, appToken);
            });
        }
    }
}
