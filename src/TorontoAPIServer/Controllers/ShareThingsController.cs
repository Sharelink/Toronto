using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using TorontoService;
using BahamutCommon;
using BahamutService;
using TorontoModel.MongodbModel;
using MongoDB.Bson;
using System.Net;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace TorontoAPIServer.Controllers
{
    [Route("api/[controller]")]
    public class ShareThingsController : TorontoAPIController
    {
        /// <summary>
        /// GET /ShareThings : get user shares, get my share default,if set the userId property, get the share of userId user, with shareIds parameter, will only get the shares which in the shareIds
        /// </summary>
        /// <param name="endTime">share time older than endTime</param>
        /// <param name="beginTime">share time newer than beginTime</param>
        /// <param name="page">page of the results,0 means return all record</param>
        /// <param name="pageCount">result num of one page</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<object[]> Get(string beginTime, string endTime, int page, int pageCount)
        {
            DateTime begin = new DateTime(2015, 7, 26, 7, 7, 7);
            DateTime end = DateTime.UtcNow;
            if (beginTime != null)
            {
                begin = DateTimeUtil.ToDate(beginTime);
            }

            if (endTime != null)
            {
                end = DateTimeUtil.ToDate(endTime);
            }

            var service = this.UseShareService().GetShareService();
            var thingMails = await service.GetUserShareMails(UserSessionData.UserId, begin, end, page, pageCount);
            var shareIds = from tm in thingMails select tm.ShareId;
            var things = await service.GetShares(shareIds);
            return await shareThingsToResults(things);
            
        }

        private async Task<object[]> shareThingsToResults(IEnumerable<ShareThing> shares)
        {
            var usrService = this.UseSharelinkerService().GetSharelinkerService();
            var users = await usrService.GetUserLinkedUsers(UserSessionData.UserId);
            var result = new List<object>();
            foreach (var share in shares)
            {
                var uId = share.UserId.ToString();
                var user = users[uId];
                var userAvatar = "";
                var shareContent = share.ShareContent;
                result.Add(new
                {
                    shareId = share.Id.ToString(),
                    pShareId = share.PShareId.ToString(),
                    userId = uId,
                    userNick = user.NoteName,
                    avatarId = userAvatar,
                    shareTime = DateTimeUtil.ToString(share.ShareTime),
                    shareType = share.ShareType,
                    message = share.Message,
                    shareContent = shareContent ,
                    voteUsers = (from v in share.Votes select v.UserId.ToString()).ToArray(),
                    forTags = share.Tags,
                    reshareable = share.Reshareable.ToString().ToLower()
                });
            }
            return result.ToArray();
        }

        [HttpGet("ShareIds")]
        public async Task<object> GetShareOfShareIds(string shareIds)
        {
            var ids = shareIds.Split('#');
            var service = this.UseShareService().GetShareService();
            var things = await service.GetShares(from id in ids select new ObjectId(id));
            return await shareThingsToResults(things);
        }

        [HttpGet("Updated")]
        public object[] GetNewShareThingUpdatedMessages()
        {
            using (var msgClient = Startup.PublishSubscriptionManager.MessageCacheClientManager.GetClient())
            {
                var msgList = msgClient.As<ShareThingUpdatedMessage>().Lists[UserSessionData.UserId];
                var msgs = msgList.GetAll();
                var result = from m in msgs
                             select new
                             {
                                 shareId = m.ShareId.ToString(),
                                 time = DateTimeUtil.ToString(m.Time)
                             };
                return result.ToArray();
            }
        }

        [HttpDelete("Updated")]
        public void DeleteNewShareThingUpdatedMessages()
        {
            using (var msgClient = Startup.PublishSubscriptionManager.MessageCacheClientManager.GetClient())
            {
                var client = msgClient.As<ShareThingUpdatedMessage>();
                var msgList = client.Lists[UserSessionData.UserId];
                client.RemoveAllFromList(msgList);
            }
        }

        [HttpPost("Reshare/{pShareId}")]
        public async Task<object> Reshare(string pShareId, string message, string tags,string reshareable)
        {
            var service = this.UseShareService().GetShareService();
            var tagService = this.UseSharelinkTagService().GetSharelinkTagService();
            var pShare = await service.GetShare(new ObjectId(pShareId));
            if (pShare == null)
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return new { msg = "NO_P_SHARE" };
            }
            var share = (await PostShare(message, pShare.ShareType, tags, pShare.ShareContent, reshareable, pShareId, service)) as ShareThing;
            if (share != null)
            {
                await SetShareFinished(share, tagService, service);
                return new
                {
                    shareId = share.Id.ToString(),
                    pShareId = share.PShareId.ToString(),
                    shareTime = DateTimeUtil.ToString(share.ShareTime),
                    shareType = share.ShareType,
                    message = share.Message,
                    shareContent = share.ShareContent,
                    forTags = share.Tags,
                    reshareable = share.Reshareable.ToString().ToLower()
                };
            }
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return new { };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="shareType"></param>
        /// <param name="tags">sample: base64("{type:"tagType",data:"tagData"}")#base64("{type:"tagType",data:"tagData"}")... </param>
        /// <param name="shareContent">json string</param>
        /// <param name="reshareable">can reshare</param>
        /// <returns></returns>
        // POST /ShareThings (message, shareType,tags, shareContent, string reshareable) : post a new share,if pshareid equals 0, means not a reshare action
        [HttpPost]
        public async Task<object> Post(string message, string shareType, string tags, string shareContent, string reshareable,string pShareId)
        {
            var service = this.UseShareService().GetShareService();
            var share = await PostShare(message, shareType, tags, shareContent, reshareable, pShareId, service);
            return new
            {
                shareId = share.Id.ToString(),
                pShareId = share.PShareId.ToString(),
                shareTime = DateTimeUtil.ToString(share.ShareTime),
                shareType = share.ShareType,
                message = share.Message,
                shareContent = share.ShareContent,
                forTags = share.Tags,
                reshareable = share.Reshareable.ToString().ToLower()
            };
        }

        private async Task<ShareThing> PostShare(string message, string shareType, string tags, string shareContent, string reshareable, string pShareId, ShareService service)
        {
            var b64 = new DBTek.Crypto.Base64();
            var tagb64s = string.IsNullOrWhiteSpace(tags) ? new string[0] : tags.Split('#');
            var tagJsons = (from tagB64 in tagb64s select b64.DecodeString(tagB64)).ToList();
            var ownTag = new
            {
                name = "ME",
                type = SharelinkTagConstant.TAG_TYPE_SHARELINKER,
                data = UserSessionData.UserId
            };
            tagJsons.Add(ownTag.ToJson());
            var reshare = true;
            try { reshare = bool.Parse(reshareable); } catch { };
            var newShare = new ShareThing()
            {
                ShareTime = DateTime.UtcNow,
                Message = message,
                UserId = new ObjectId(UserSessionData.UserId),
                ShareType = shareType,
                ShareContent = shareContent,
                Tags = tagJsons.ToArray(),
                Reshareable = reshare,
            };
            if (string.IsNullOrWhiteSpace(pShareId) == false)
            {
                newShare.PShareId = new ObjectId(pShareId);
            }
            if (newShare.IsUserShareType() == false)
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
            var share = await service.PostNewShareThing(newShare);
            return share;
        }

        // POST /ShareThings/Finished/{shareId} (taskSuccess) : if taskSuccess, post mail to linkers who focus this share's tags
        [HttpPost("Finished/{shareId}")]
        public async Task<object> FinishedPostShare(string shareId,string taskSuccess)
        {
            if (taskSuccess != "true")
            {
                return new { message = "ok" };
            }

            var tagService = this.UseSharelinkTagService().GetSharelinkTagService();
            var service = this.UseShareService().GetShareService();
            var newShare = await service.GetShare(new ObjectId(shareId));
            return await SetShareFinished(newShare, tagService, service);
        }

        private async Task<object> SetShareFinished(ShareThing newShare, SharelinkTagService tagService, ShareService service)
        {
            var mails = new List<ShareThingMail>();
            IEnumerable<dynamic> dynamicTags = from tagJson in newShare.Tags select JsonConvert.DeserializeObject(tagJson);
            var newShareTags = (from dt in dynamicTags
                                select new SharelinkTag()
                                {
                                    Data = dt.data,
                                    TagType = dt.type
                                }).ToList();
            var isPrivateShare = newShareTags.Count(st => st.IsPrivateTag()) > 0;
            newShareTags.Add(new SharelinkTag()
            {
                Data = UserSessionData.UserId,
                TagType = SharelinkTagConstant.TAG_TYPE_SHARELINKER
            });
            mails.Add(new ShareThingMail()
            {
                ShareId = newShare.Id,
                Time = DateTime.UtcNow,
                ToSharelinker = new ObjectId(UserSessionData.UserId)
            });

            if (isPrivateShare == false)
            {
                await MatchLinkerTags(tagService, newShare, mails, newShareTags);
            }
            service.InsertMails(mails);

            Startup.PublishSubscriptionManager.PublishShareMessages(mails);
            return new { message = "ok" };
        }

        private async Task MatchLinkerTags(SharelinkTagService tagService, ShareThing newShare, List<ShareThingMail> mails, IEnumerable<SharelinkTag> newShareTags)
        {
            var userService = this.UseSharelinkerService().GetSharelinkerService();
            var linkers = await userService.GetUserlinksOfUserId(UserSessionData.UserId);
            var linkerIds = from l in linkers select l.SlaveUserObjectId;
            var linkersTags = await tagService.GetLinkersTags(linkerIds);
            foreach (var linkerTags in linkersTags)
            {
                var sendMailFlag = false;
                var mail = new ShareThingMail()
                {
                    ShareId = newShare.Id,
                    Time = DateTime.UtcNow
                };
                if (linkerTags.Key != newShare.UserId)
                {
                    var linkTagDatas = from lt in linkerTags.Value select lt;
                    var matchTags = tagService.MatchTags(newShareTags, linkTagDatas);
                    if (matchTags.Count() > 0)
                    {
                        mail.Tags = matchTags;
                        sendMailFlag = true;
                        mail.ToSharelinker = linkerTags.Key;
                    }
                }
                if (sendMailFlag)
                {
                    mails.Add(mail);
                }
            }
        }
    }
}
