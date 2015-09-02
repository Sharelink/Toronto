using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using TorontoService;

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
            var sharelinkUserService = this.UseSharelinkUserService().GetSharelinkUserService();
            return new { items = sharelinkUserService.GetMyAllSharelinkTags() };
        }

        // POST /SharelinkTags (tagName,tagColor):add a new user tag to my tags collection
        [HttpPost]
        public void Post(string tagName, string tagColor)
        {
            var sharelinkUserService = this.UseSharelinkUserService().GetSharelinkUserService();
            sharelinkUserService.CreateNewSharelinkTag(tagName, tagColor);
        }

        // PUT /SharelinkTags/{tagId} (tagName,tagColor): update tag property
        [HttpPut("{tagId}")]
        public void Put(string tagId, string tagName, string tagColor)
        {
            var sharelinkUserService = this.UseSharelinkUserService().GetSharelinkUserService();
            sharelinkUserService.UpdateSharelinkTag(tagId, tagName, tagColor);
        }

        // DELETE /SharelinkTags (tagId) : delete the tag,and all the user has link to this tag will be dislink
        [HttpDelete("{tagId}")]
        public void Delete(string tagId)
        {
            var sharelinkUserService = this.UseSharelinkUserService().GetSharelinkUserService();
            sharelinkUserService.DeleteSharelinkTag(tagId);
        }
    }
}
