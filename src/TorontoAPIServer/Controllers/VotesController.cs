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
        public async Task Post(string shareId)
        {
            var shareService = this.UseShareService().GetShareService();
            var share = await shareService.VoteShare(UserSessionData.UserId, shareId);
            var updateMsg = new ShareThingUpdatedMessage()
            {
                ShareId = share.Id.ToString(),
                Time = DateTime.UtcNow
            };
            Startup.ServicesProvider.GetBahamutPubSubService().PublishShareUpdatedMessages(share.UserId.ToString(), updateMsg);
        }

        //DELETE /Votes/{shareId} : vote sharething of shareId
        [HttpDelete("{shareId}")]
        public async Task Delete(string shareId)
        {
            var shareService = this.UseShareService().GetShareService();
            await shareService.UnvoteShare(UserSessionData.UserId, shareId);
        }
    }
}
