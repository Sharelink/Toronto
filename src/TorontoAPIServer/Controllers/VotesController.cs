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
        public async void Post(string shareId)
        {
            var shareService = this.UseShareService().GetShareService();
            if (! await shareService.VoteShare(shareId))
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }

        //DELETE /Votes/{shareId} : vote sharething of shareId
        [HttpDelete("{shareId}")]
        public async void Delete(string shareId)
        {
            var shareService = this.UseShareService().GetShareService();
            if (!(await shareService.UnvoteShare(shareId)))
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }
    }
}
