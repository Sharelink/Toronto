using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using TorontoService;
using TorontoModel.MongodbModel;
using System.Net;
using MongoDB.Bson;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace TorontoAPIServer.Controllers
{
    [Route("api/[controller]")]
    public class SharelinkTagsController : TorontoAPIController
    {
        //GET: /SharelinkTags/ : Get user's all tags from server,return SharelinkTags
        [HttpGet]
        public async Task<object[]> Get()
        {
            var sharelinkTagService = this.UseSharelinkTagService().GetSharelinkTagService();
            var shareService = this.UseSharelinkUserService().GetSharelinkUserService();
            try
            {
                var tags = await sharelinkTagService.GetUserSharelinkTags(UserSessionData.UserId);
                var result = from t in tags
                             select new
                             {
                                 tagId = t.Id.ToString(),
                                 tagName = t.TagName,
                                 tagColor = t.TagColor,
                                 data = t.Data,
                                 isFocus = t.IsFocus
                             };
                return result.ToArray();
            }
            catch (Exception)
            {
                return new object[0];
            }
            
            
        }

        // POST /SharelinkTags (tagName,tagColor,data,isFocus):add a new user tag to my tags collection
        [HttpPost]
        public async Task<object> Post(string tagName, string tagColor, string data, string isFocus)
        {
            var sharelinkTagService = this.UseSharelinkTagService().GetSharelinkTagService();
            var shareService = this.UseShareService().GetShareService();
            var userService = this.UseSharelinkUserService().GetSharelinkUserService();
            var userId = new ObjectId(UserSessionData.UserId);
            var r = await sharelinkTagService.CreateNewSharelinkTag(UserSessionData.UserId, tagName, tagColor, data, isFocus);
            if (bool.Parse(isFocus))
            {
                var newShare = new ShareThing()
                {
                    ShareTime = DateTime.UtcNow,
                    ShareType = "message",
                    UserId = userId,
                    ShareContent = r.TagName
                };
                await shareService.PostNewShareThing(newShare);

                var linkers = await userService.GetUserlinksOfUserId(UserSessionData.UserId);
                var linkerIds = from l in linkers select l.SlaveUserObjectId;

                var newMails = new List<ShareThingMail>();
                foreach (var linker in linkerIds)
                {
                    var newMail = new ShareThingMail()
                    {
                        ShareId = newShare.Id,
                        Time = DateTime.UtcNow,
                        ToSharelinker = linker
                    };
                    newMails.Add(newMail);
                }
                shareService.InsertMails(newMails);
            }
            var res = new
            {
                tagId = r.Id.ToString(),
                tagName = r.TagName,
                tagColor = r.TagColor,
                data = r.Data,
                isFocus = r.IsFocus.ToString().ToLower()
            };
            return res;
        }

        // PUT /SharelinkTags/{tagId}: update tag property
        [HttpPut("{tagId}")]
        public async void PutTag(string tagId, string tagName, string tagColor, string data, string isFocus)
        {
            var sharelinkTagService = this.UseSharelinkTagService().GetSharelinkTagService();
            var isSuc = await sharelinkTagService.UpdateSharelinkTag(UserSessionData.UserId, tagId, tagName, tagColor, data, isFocus);
            if (!isSuc)
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }

        // DELETE /SharelinkTags (tagIds) : delete the tag,and all the user has link to this tag will be dislink
        [HttpDelete]
        public async void Delete(string tagIds)
        {
            var sharelinkTagService = this.UseSharelinkTagService().GetSharelinkTagService();
            var ids = tagIds.Split('#');
            var isSuc = await sharelinkTagService.DeleteSharelinkTags(UserSessionData.UserId, ids);
            if (!isSuc)
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }
    }
}
