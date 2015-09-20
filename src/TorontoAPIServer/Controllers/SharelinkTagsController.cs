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
                return sharelinkTagService.GetMyAllSharelinkTags(UserSessionData.UserId);
            });
            var result = from t in taskRes.Result
                         select new
                         {
                             tagId = t.Id.ToString(),
                             tagName = t.TagName,
                             tagColor = t.TagColor,
                             data = t.Data,
                             isFocus = t.IsFocus
                         };
            return result;
        }

        // POST /SharelinkTags (tagName,tagColor,data,isFocus):add a new user tag to my tags collection
        [HttpPost]
        public object Post(string tagName, string tagColor,string data,string isFocus)
        {
            var sharelinkTagService = this.UseSharelinkTagService().GetSharelinkTagService();
            var taskResult = Task.Run(() =>
            {
                return sharelinkTagService.CreateNewSharelinkTag(UserSessionData.UserId, tagName, tagColor, data, isFocus);
            });
            return new
            {
                tagId = taskResult.Result.Id.ToString(),
                tagName = taskResult.Result.TagName,
                tagColor = taskResult.Result.TagColor,
                data = taskResult.Result.Data,
                isFocus = taskResult.Result.IsFocus
            };
        }

        // PUT /SharelinkTags/{tagId}: update tag property
        [HttpPut("{tagId}")]
        public void PutTag(string tagId, string tagName, string tagColor, string data, string isFocus)
        {
            var sharelinkTagService = this.UseSharelinkTagService().GetSharelinkTagService();
            var isSuc = Task.Run(async () =>
            {
                return await sharelinkTagService.UpdateSharelinkTag(UserSessionData.UserId, tagId, tagName, tagColor, data, isFocus);
            }).Result;
            if (!isSuc)
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }

        // DELETE /SharelinkTags (tagId) : delete the tag,and all the user has link to this tag will be dislink
        [HttpDelete("{tagId}")]
        public void Delete(string tagId)
        {
            var sharelinkTagService = this.UseSharelinkTagService().GetSharelinkTagService();
            var isSuc = Task.Run(async () =>
            {
                return await sharelinkTagService.DeleteSharelinkTag(UserSessionData.UserId, tagId);
            }).Result;
            if (!isSuc)
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }
    }
}
