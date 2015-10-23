using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using TorontoModel.MongodbModel;
using TorontoService;
using System.Net;
using MongoDB.Bson;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace TorontoAPIServer.Controllers
{
    [Route("api/[controller]")]
    public class VotesController : TorontoAPIController
    {

        //POST /Votes/{shareId} : vote sharething of shareId
        [HttpPost("{shareId}")]
        public async void Post(string shareId)
        {
            var shareService = this.UseShareService().GetShareService();
            var share = await shareService.VoteShare(UserSessionData.UserId, shareId);
            using (var psClient = Startup.MessagePubSubServerClientManager.GetClient())
            {
                using (var msgClient = Startup.MessageCacheServerClientManager.GetClient())
                {
                    msgClient.As<ShareThingUpdatedMessage>().Lists[share.UserId.ToString()].Add(
                        new ShareThingUpdatedMessage()
                        {
                            ShareId = share.Id,
                            Time = DateTime.UtcNow
                        });
                }
                psClient.PublishMessage(share.UserId.ToString(), string.Format("ShareThingMessage:{0}", shareId));
                
            }
        }

        //DELETE /Votes/{shareId} : vote sharething of shareId
        [HttpDelete("{shareId}")]
        public async void Delete(string shareId)
        {
            var shareService = this.UseShareService().GetShareService();
            await shareService.UnvoteShare(UserSessionData.UserId, shareId);
        }
    }
}
