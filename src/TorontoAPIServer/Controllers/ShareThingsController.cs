using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using TorontoService;
using BahamutCommon;
using BahamutService;
using TorontoModel.MongodbModel;

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
        public object Get(DateTime newerThanThisTime,DateTime olderThanThisTime,int page, int pageCount,string shareIds = null)
        {
            var service = this.UseShareService().GetShareService();
            return service.GetUserShareThings(newerThanThisTime, olderThanThisTime, page, pageCount);
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
        public ShareThing Post(string pShareId, string shareTitle, string shareTypeId,string tagIds, string shareContent)
        {
            var service = this.UseShareService().GetShareService();
            var newShare = new ShareThing()
            {
                PShareId = pShareId,
                ShareTime = DateTime.Now,
                Title = shareTitle,
                UserId = UserSessionData.UserId,
                TagIds = tagIds.Split('#'),
                ShareContent = new ShareContent()
                {
                    ContentDocument = shareContent,
                    Type = new ShareType()
                    {
                        ShareTypeId = shareTypeId
                    }
                }
            };
            var itemWithId = service.PostNewShareThing(newShare);
            return new ShareThing() { ShareId = itemWithId.ShareId };
        }

    }
}
