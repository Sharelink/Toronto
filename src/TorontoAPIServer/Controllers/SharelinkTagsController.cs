using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using TorontoService;
using TorontoModel.MongodbModel;
using System.Net;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace TorontoAPIServer.Controllers
{
    [Route("api/[controller]")]
    public class SharelinkTagsController : TorontoAPIController
    {
        //GET: /SharelinkTags/ : Get user's all tags from server,return SharelinkTags
        [HttpGet]
        public object Get()
        {
            var sharelinkTagService = this.UseSharelinkTagService().GetSharelinkTagService();
            return new { items = sharelinkTagService.GetMyAllSharelinkTags() };
        }

        // POST /SharelinkTags (tagName,tagColor):add a new user tag to my tags collection
        [HttpPost]
        public async Task<SharelinkTag> Post(string tagName, string tagColor)
        {
            var sharelinkTagService = this.UseSharelinkTagService().GetSharelinkTagService();
            return await sharelinkTagService.CreateNewSharelinkTag(tagName, tagColor);
        }

        // PUT /SharelinkTags/{tagId} (tagName,tagColor): update tag property
        [HttpPut("{tagId}")]
        public async void Put(string tagId, string tagName, string tagColor)
        {
            var sharelinkTagService = this.UseSharelinkTagService().GetSharelinkTagService();
            if(! await sharelinkTagService.UpdateSharelinkTag(tagId, tagName, tagColor))
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }

        // DELETE /SharelinkTags (tagId) : delete the tag,and all the user has link to this tag will be dislink
        [HttpDelete("{tagId}")]
        public async void Delete(string tagId)
        {
            var sharelinkTagService = this.UseSharelinkTagService().GetSharelinkTagService();
            if(! await sharelinkTagService.DeleteSharelinkTag(tagId))
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }
    }
}
