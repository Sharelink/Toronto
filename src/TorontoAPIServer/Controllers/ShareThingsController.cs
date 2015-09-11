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

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace TorontoAPIServer.Controllers
{
    [Route("api/[controller]")]
    public class ShareThingsController : TorontoAPIController
    {
        /// <summary>
        /// GET /ShareThings : get user shares, get my share default,if set the userId property, get the share of userId user, with shareIds parameter, will only get the shares which in the shareIds
        /// </summary>
        /// <param name="newerThanThisTime">return the newest modified sharethings after this time, except this time, the first one of return values is close to this time, conflict with olderThanThisTime property，last set effective</param>
        /// <param name="olderThanThisTime">return the sharethings before this time, except this time, the first one of return values is close to this time, conflict with newerThanThisTime property，last set effective the</param>
        /// <param name="page">page of the results,0 means return all record</param>
        /// <param name="pageCount">result num of one page</param>
        /// <param name="shareIds">filter, defalut nil</param>
        /// <returns></returns>
        [HttpGet]
        public object Get(string newerThanThisTime,string olderThanThisTime,int page, int pageCount,string shareIds = null)
        {

            DateTime newerTime = DateTimeUtil.ToDate(newerThanThisTime);
            DateTime olderTime = DateTimeUtil.ToDate(olderThanThisTime);
            var service = this.UseShareService().GetShareService();
            var usrService = this.UseSharelinkUserService().GetSharelinkUserService();
            var things = Task.Run(() => {
                return service.GetUserShareThings(UserSessionData.UserId, newerTime, olderTime, page, pageCount);
            }).Result;
            var userIds = from t in things select t.UserId.ToString();
            var users = Task.Run(() => 
            {
                return usrService.GetUserLinkedUsers(UserSessionData.UserId, userIds, true);
            }).Result;
            var result = new object[things.Count];
            for (int i = 0; i < result.Length; i++)
            {
                var uId = things[i].UserId.ToString();
                var user = users[uId];
                var thing = things[i];
                result[i] = new
                {
                    shareId = thing.Id.ToString(),
                    pShareId = thing.PShareId.ToString(),
                    userId = uId,
                    userNick = user.NickName,
                    headIconImageId = user.HeadIcon,
                    shareTime = DateTimeUtil.ToString(thing.ShareTime),
                    shareType = thing.ShareType.ToString(),
                    title = thing.Title,
                    shareContent = thing.ShareContent,
                    //reShares:[ShareThing]!
                    voteUsers = (from v in thing.Votes select v.UserId.ToString()).ToArray(),
                    lastActiveTime = DateTimeUtil.ToString(thing.LastActiveTime),
                    forTags = thing.Tags
                };
            }
            return result;
        }

        /// <summary>
        /// GET /ShareThings/1234/Reshares : get the reshares of 1234, with shareIds parameter,will only get the shares which in the shareIds
        /// </summary>
        /// <param name="shareId"></param>
        /// <returns></returns>
        [HttpGet("{shareId}/Reshares")]
        public object Get(string shareId)
        {
            var service = this.UseShareService().GetShareService();
            return service.GetShareThingReshares(shareId);
        }

        // POST /ShareThings (pshareid,sharetitle,sharetypeid,shareContent) : post a new share,if pshareid equals 0, means not a reshare action
        [HttpPost]
        public object Post(string pShareId, string shareTitle, string shareTypeId,string tagIds, string shareContent)
        {
            var service = this.UseShareService().GetShareService();
            var newShare = new ShareThing()
            {
                PShareId = new ObjectId(pShareId),
                ShareTime = DateTime.Now,
                Title = shareTitle,
                UserId = new ObjectId(UserSessionData.UserId),
                
                ShareContent = new ShareContent()
                {
                    ContentDocument = shareContent,
                    Type = new ShareType()
                    {
                        Id = new ObjectId(shareTypeId)
                    }
                }
            };
            var itemWithId = service.PostNewShareThing(newShare);
            return new { ShareId = itemWithId.Id.ToString() };
        }

    }
}
