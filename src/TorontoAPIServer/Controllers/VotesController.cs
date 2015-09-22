using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using TorontoModel.MongodbModel;
using TorontoService;
using System.Net;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace TorontoAPIServer.Controllers
{
    [Route("api/[controller]")]
    public class VotesController : TorontoAPIController
    {

        //POST /Votes/{shareId} : vote sharething of shareId
        [HttpPost("{shareId}")]
        public void Post(string shareId)
        {
            var isSuc = Task.Run(async () =>
            {
                var shareService = this.UseShareService().GetShareService();
                shareService.MarkARecordForShareThing(new MongoDB.Bson.ObjectId(shareId), new MongoDB.Bson.ObjectId(UserSessionData.UserId), "vote");
                return await shareService.VoteShare(UserSessionData.UserId, shareId);
            }).Result;
            if(!isSuc)
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }

        //DELETE /Votes/{shareId} : vote sharething of shareId
        [HttpDelete("{shareId}")]
        public void Delete(string shareId)
        {
            var shareService = this.UseShareService().GetShareService();
            var isSuc = Task.Run(async () =>
            {
                return await shareService.UnvoteShare(UserSessionData.UserId, shareId);
            }).Result;
            if (!isSuc)
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }
    }
}
