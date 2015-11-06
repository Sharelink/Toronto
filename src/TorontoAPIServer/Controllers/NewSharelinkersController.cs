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
    public class NewSharelinkersController : TorontoAPIController
    {
        // POST api/values
        [HttpPost]
        public async Task<object> Post(string accountId, string accessToken, string nickName, string motto,string region)
        {
            var tokenService = Startup.ServicesProvider.GetTokenService();
            var userSession = await tokenService.ValidateToGetSessionData(Startup.Appkey, accountId, accessToken);
            if (userSession != null)
            {
                var newUser = new Sharelinker()
                {
                    AccountId = accountId,
                    NickName = nickName,
                    CreateTime = DateTime.UtcNow,
                    NoteName = nickName,
                    Motto = motto
                };
                var userService = this.UseSharelinkerService().GetSharelinkerService();
                var user = await userService.CreateNewUser(newUser);
                var newUserId = user.Id.ToString();

                #region New User Default Datas
                //Add user self
                await userService.CreateNewLinkWithOtherUser(newUserId, newUserId, new SharelinkerLink.State() { LinkState = (int)SharelinkerLink.LinkState.Linked },nickName);

                //Add SharelinkerCenter
                var sharelinkCenter = await userService.GetSharelinkCenterOfRegion(region);
                await userService.CreateNewLinkWithOtherUser(newUserId, sharelinkCenter.Id.ToString(), new SharelinkerLink.State() { LinkState = (int)SharelinkerLink.LinkState.Linked }, SharelinkerConstants.SharelinkCenterNickName);

                //Add default share for user
                var shareService = this.UseSharelinkerService().GetShareService();
                var defaultShareThing = shareService.GetNewSharelinkerDefaultShareThings(region);
                var shareMails = from st in defaultShareThing
                                 select new ShareThingMail()
                                 {
                                     ShareId = st.Id,
                                     Tags = new string[] { "Broadcast" },
                                     Time = DateTime.UtcNow,
                                     ToSharelinker = newUser.Id
                                 };
                shareService.InsertMails(shareMails);
                #endregion

                var sessionData = await tokenService.ValidateAccessToken(Startup.Appkey, accountId, accessToken, newUser.Id.ToString());
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
