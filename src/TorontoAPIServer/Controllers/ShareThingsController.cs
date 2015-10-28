﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using TorontoService;
using BahamutCommon;
using BahamutService;
using TorontoModel.MongodbModel;
using MongoDB.Bson;

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
        public async Task<object> Get(string beginTime, string endTime, int page, int pageCount)
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
            var fireAccessService = Startup.ServicesProvider.GetFireAccesskeyService();
            var usrService = this.UseSharelinkerService().GetSharelinkerService();
            var users = await usrService.GetUserLinkedUsers(UserSessionData.UserId);
            var result = new List<object>();
            foreach (var share in shares)
            {
                var uId = share.UserId.ToString();
                var user = users[uId];
                result.Add(new
                {
                    shareId = share.Id.ToString(),
                    pShareId = share.PShareId.ToString(),
                    userId = uId,
                    userNick = user.NoteName,
                    avatarId = fireAccessService.GetAccessKeyUseDefaultConverter(UserSessionData.AccountId, user.Avatar),
                    shareTime = DateTimeUtil.ToString(share.ShareTime),
                    shareType = share.ShareType,
                    title = share.Title,
                    shareContent = share.ShareType == "film" ? fireAccessService.GetAccessKeyUseDefaultConverter(UserSessionData.AccountId, share.ShareContent) : share.ShareContent,
                    voteUsers = (from v in share.Votes select v.UserId.ToString()).ToArray(),
                    forTags = share.Tags
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
            using (var msgClient = Startup.MessageCacheServerClientManager.GetClient())
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
            using (var msgClient = Startup.MessageCacheServerClientManager.GetClient())
            {
                var msgList = msgClient.As<ShareThingUpdatedMessage>().Lists[UserSessionData.UserId];
                msgList.RemoveAll();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pShareId"></param>
        /// <param name="title"></param>
        /// <param name="shareType"></param>
        /// <param name="tags">sample: base64("{type:"tagType",data:"tagData"}")#base64("{type:"tagType",data:"tagData"}")... </param>
        /// <param name="shareContent"></param>
        /// <returns></returns>
        // POST /ShareThings (pShareId, title, shareType,tags, shareContent) : post a new share,if pshareid equals 0, means not a reshare action
        [HttpPost]
        public async Task<object> Post(string pShareId, string title, string shareType, string tags, string shareContent)
        {
            var service = this.UseShareService().GetShareService();
            var b64 = new DBTek.Crypto.Base64();
            var tagb64s = tags.Split('#');
            var tagJsons = from tagB64 in tagb64s select b64.DecodeString(tagB64);
            var newShare = new ShareThing()
            {
                ShareTime = DateTime.UtcNow,
                Title = title,
                UserId = new ObjectId(UserSessionData.UserId),
                ShareType = shareType,
                ShareContent = shareContent,
                Tags = tagJsons.ToArray()
            };

            if (pShareId != null)
            {
                newShare.PShareId = new ObjectId(pShareId);
            }
            newShare = await service.PostNewShareThing(newShare);
            return new { shareId = newShare.Id.ToString() };
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
            var newShare = (await service.GetShares(new ObjectId[] { new ObjectId(shareId) })).First();

            var mails = new List<ShareThingMail>();
            IEnumerable<dynamic> dynamicTags = from tagJson in newShare.Tags select Newtonsoft.Json.JsonConvert.DeserializeObject(tagJson);
            var newShareTags = from dt in dynamicTags
                               select new SharelinkTag()
                               {
                                   Data = dt.data,
                                   TagType = dt.type
                               };
            var isPrivateShare = newShareTags.Count(st => st.IsPrivateTag()) > 0;
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

            PublishShareMessages(shareId, mails);

            return new { message = "ok" };
        }

        private static void PublishShareMessages(string shareId, List<ShareThingMail> mails)
        {
            using (var psClient = Startup.MessagePubSubServerClientManager.GetClient())
            {
                using (var msgClient = Startup.MessageCacheServerClientManager.GetClient())
                {
                    foreach (var m in mails)
                    {
                        var sharelinker = m.ToSharelinker.ToString();
                        msgClient.As<ShareThingUpdatedMessage>().Lists[sharelinker].Add(
                        new ShareThingUpdatedMessage()
                        {
                            ShareId = m.ShareId,
                            Time = DateTime.UtcNow
                        });
                        psClient.PublishMessage(sharelinker, string.Format("ShareThingMessage:{0}", shareId));
                    }

                }
            }
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
                if (linkerTags.Key == newShare.UserId)
                {
                    sendMailFlag = false;
                }
                else
                {
                    var linkTagDatas = from lt in linkerTags.Value select lt;
                    var matchTags = tagService.MatchTags(newShareTags, linkTagDatas);
                    if (matchTags.Count() > 0)
                    {
                        mail.Tags = matchTags;
                        sendMailFlag = true;
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
