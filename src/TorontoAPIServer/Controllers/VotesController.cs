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
        // GET /Votes/{shareId} : return the shareId's votes
        [HttpGet("{shareId}")]
        public object Get(string shareId)
        {
            var shareService = this.UseShareService().GetShareService();
            return shareService.GetVoteOfShare(shareId);
        }

        //POST /Votes/{shareId} : vote sharething of shareId
        [HttpPost("{shareId}")]
        public void Post(string shareId)
        {
            var shareService = this.UseShareService().GetShareService();
            shareService.VoteShare(shareId);
        }

        //DELETE /Votes/{shareId} : vote sharething of shareId
        [HttpDelete("{shareId}")]
        public void Delete(string shareId)
        {
            var shareService = this.UseShareService().GetShareService();
            shareService.UnvoteShare(shareId);
        }
    }
}
