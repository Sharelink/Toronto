using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using BahamutService.Model;
using TorontoService;
using TorontoModel.MongodbModel;
using System.Net;
using MongoDB.Bson;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace TorontoAPIServer.Controllers
{
    [Route("[controller]")]
    public class NewSharelinkersController : TorontoAPIController
    {
        // POST api/values
        [HttpPost]
        public async Task<object> Post(string accountId, string accessToken, string nickName, string motto,string region="us")
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
                var centerId = "";
                try
                {
                    centerId = Startup.SharelinkCenters[region];
                }
                catch (Exception)
                {
                    centerId = Startup.SharelinkCenters["us"];
                }
                
                var centerOId = new ObjectId(centerId);
                await userService.CreateNewLinkWithOtherUser(newUserId, centerId, new SharelinkerLink.State() { LinkState = (int)SharelinkerLink.LinkState.Linked }, SharelinkerConstants.SharelinkCenterNickName);

                //Add default share for user
                var shareService = this.UseShareService().GetShareService();
                var initShares = Startup.Configuration.GetSection(string.Format("InitShareThing:{0}:shares",region)).GetChildren();
                var now = 0;
                var shares = from share in initShares
                             select new ShareThing()
                             {
                                 Message = share["message"],
                                 ShareContent = share["content"],
                                 ShareType = share["contentType"],
                                 UserId = centerOId,
                                 Reshareable = true,
                                 ShareTime = DateTime.UtcNow.AddSeconds(now -= 7) //sort by time
                             };
                var defaultShareThings = await shareService.CreateNewSharelinkerDefaultShareThings(shares);
                var shareMails = from s in defaultShareThings
                                 select new ShareThingMail()
                                 {
                                     ShareId = s.Id,
                                     Tags = new string[] { "Broadcast" },
                                     ToSharelinker = newUser.Id,
                                     Time = s.ShareTime
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
