using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using TorontoService;
using TorontoModel.MongodbModel;
using System.Net;
using MongoDB.Bson;
using BahamutCommon;

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
            var shareService = this.UseSharelinkerService().GetSharelinkerService();
            try
            {
                var tags = await sharelinkTagService.GetUserSharelinkTags(UserSessionData.UserId);
                var result = from t in tags
                             select SharelinkTagToResultObject(
                                 t.IsSystemTag() ? PasswordHash.Encrypt.MD5(string.Format("{0}{1}{2}", t.TagDomain, t.TagType, t.Data)) : t.Id.ToString(),
                                 t);
                return result.ToArray();
            }
            catch (Exception)
            {
                return new object[0];
            }
        }

        // POST /SharelinkTags (tagName,tagColor,data,isFocus):add a new user tag to my tags collection
        [HttpPost]
        public async Task<object> Post(string tagName, string tagColor,string type, string data, string isFocus,string isShowToLinkers)
        {
            var sharelinkTagService = this.UseSharelinkTagService().GetSharelinkTagService();
            var shareService = this.UseShareService().GetShareService();
            var userService = this.UseSharelinkerService().GetSharelinkerService();
            var userId = new ObjectId(UserSessionData.UserId);
            bool focus = false;
            bool showToLinker = false;
            try { focus = bool.Parse(isFocus); } catch (Exception) { }
            try { showToLinker = bool.Parse(isShowToLinkers); } catch (Exception) { }
            var newTag = new SharelinkTag()
            {
                TagColor = tagColor,
                TagName = tagName,
                TagDomain = SharelinkTagConstant.TAG_DOMAIN_CUSTOM,
                TagType = type,
                Data = data,
                IsFocus = focus,
                UserId = userId,
                CreateTime = DateTime.UtcNow,
                ShowToLinkers = showToLinker
            };

            var r = await sharelinkTagService.CreateNewSharelinkTag(newTag);
            if (showToLinker && newTag.IsSharelinkerTag() == false)
            {
                await SendShowToLinkerTagMessage(shareService, userService, userId, r);
            }
            return SharelinkTagToResultObject(r.Id.ToString(),r);
        }

        private static object SharelinkTagToResultObject(string tagId,SharelinkTag r)
        {
            return new
            {
                tagId = tagId,
                tagName = r.TagName,
                tagColor = r.TagColor,
                data = r.Data,
                isFocus = r.IsFocus.ToString().ToLower(),
                domain = r.TagDomain,
                type = r.TagType,
                showToLinkers = r.ShowToLinkers.ToString().ToLower(),
                time = DateTimeUtil.ToAccurateDateTimeString(r.CreateTime)
            };
        }

        private async Task SendShowToLinkerTagMessage(ShareService shareService, SharelinkerService userService, ObjectId userId, SharelinkTag newTag)
        {
            var newShare = new ShareThing()
            {
                ShareTime = DateTime.UtcNow,
                ShareType = newTag.IsFocus ? ShareThingConstants.SHARE_TYPE_MESSAGE_FOCUS_TAG : ShareThingConstants.SHARE_TYPE_MESSAGE_ADD_TAG,
                UserId = userId,
                ShareContent = SharelinkTagToResultObject(newTag.Id.ToString(),newTag).ToJson()
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
            Startup.PublishSubscriptionManager.PublishShareMessages(newMails);
        }

        // PUT /SharelinkTags/{tagId}: update tag property
        [HttpPut("{tagId}")]
        public async Task PutTag(string tagId, string tagName, string tagColor, string type, string data, string isFocus,string isShowToLinkers)
        {
            var sharelinkTagService = this.UseSharelinkTagService().GetSharelinkTagService();
            bool focus = false;
            bool showToLinker = false;
            try {focus = bool.Parse(isFocus);}catch (Exception){}
            try { showToLinker = bool.Parse(isShowToLinkers); } catch (Exception) { }
            var isSuc = await sharelinkTagService.UpdateSharelinkTag(UserSessionData.UserId, tagId, tagName, tagColor, data, focus, type, showToLinker);
            if (!isSuc)
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }

        // DELETE /SharelinkTags (tagIds) : delete the tag,and all the user has link to this tag will be dislink
        [HttpDelete]
        public async Task Delete(string tagIds)
        {
            try
            {
                var sharelinkTagService = this.UseSharelinkTagService().GetSharelinkTagService();
                var ids = tagIds.Split('#');
                var isSuc = await sharelinkTagService.DeleteSharelinkTags(UserSessionData.UserId, ids);
                if (!isSuc)
                {
                    Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                }
            }
            catch (Exception ex)
            {
                LogWarning(ex.Message, ex);
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
            
        }
    }
}
