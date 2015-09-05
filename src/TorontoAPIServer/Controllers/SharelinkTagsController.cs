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
            var taskRes = Task.Run(() =>
            {
                return sharelinkTagService.GetMyAllSharelinkTags();
            });
            var result = from t in taskRes.Result
                         select new
                         {
                             tagId = t.Id.ToString(),
                             tagName = t.TagName,
                             tagColor = t.TagColor
                         };
            return result;
        }

        // POST /SharelinkTags (tagName,tagColor):add a new user tag to my tags collection
        [HttpPost]
        public object Post(string tagName, string tagColor)
        {
            var sharelinkTagService = this.UseSharelinkTagService().GetSharelinkTagService();
            var taskResult = Task.Run(() =>
            {
                return sharelinkTagService.CreateNewSharelinkTag(tagName, tagColor);
            });
            return new
            {
                tagId = taskResult.Result.Id.ToString(),
                tagName = taskResult.Result.TagName,
                tagColor = taskResult.Result.TagColor
            };
        }

        // PUT /SharelinkTags/{tagId}/{tagColor}: update tag color property
        [HttpPut("{tagId}/{tagColor}")]
        public async void PutTagColor(string tagId,string tagColor)
        {
            var sharelinkTagService = this.UseSharelinkTagService().GetSharelinkTagService();
            if(! await sharelinkTagService.UpdateSharelinkTagColor(tagId, tagColor))
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }

        // PUT /SharelinkTags/{tagId}/{tagName}: update tag name property
        [HttpPut("{tagId}/{tagName}")]
        public async void PutTagName(string tagId, string tagName)
        {
            var sharelinkTagService = this.UseSharelinkTagService().GetSharelinkTagService();
            if (!await sharelinkTagService.UpdateSharelinkTagName(tagId, tagName))
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
