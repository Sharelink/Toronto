using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using TorontoService;
using System.Net;
using BahamutCommon;
using TorontoModel.MongodbModel;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace TorontoAPIServer.Controllers
{
    [Route("api/[controller]")]
    public class UserLinksController : TorontoAPIController
    {

        //GET /UserLinks : get my all userlinks
        [HttpGet]
        public async Task<object[]> Get()
        {
            var service = this.UseSharelinkerService().GetSharelinkerService();
            var taskRes = await service.GetUserlinksOfUserId(UserSessionData.UserId);
            var links = from ul in taskRes
                        select new
                        {
                            linkId = ul.SlaveUserObjectId.ToString(),
                            slaveUserId = ul.SlaveUserObjectId.ToString(),
                            status = SharelinkerLink.State.FromJson(ul.StateDocument).LinkState.ToString(),
                            createTime = DateTimeUtil.ToString(ul.CreateTime)
                        };
            return links.ToArray();
        }

        //PUT /UserLinks (myUserId,otherUserId,newState) : update my userlink status with other people
        [HttpPut("NoteName")]
        public async Task PutUpdateNoteName(string userId, string newNoteName)
        {
            var service = this.UseSharelinkerService().GetSharelinkerService();
            var suc = await service.UpdateLinkedUserNoteName(UserSessionData.UserId, userId, newNoteName);
            if (!suc)
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }

        //PUT /UserLinks (myUserId,otherUserId,newState) : update my userlink status with other people
        [HttpPut]
        public async Task Put(string userId, string newState)
        {
            var service = this.UseSharelinkerService().GetSharelinkerService();
            var ns = new SharelinkerLink.State() { LinkState = int.Parse(newState) };
            var suc = await service.UpdateUserlinkStateWithUser(UserSessionData.UserId, userId, JsonConvert.SerializeObject(ns));
            if (!suc)
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }

        [HttpPost("{sharelinkerId}")]
        public async Task<object> AcceptAskingLink(string sharelinkerId,string noteName)
        {
            var service = this.UseSharelinkerService().GetSharelinkerService();
            var newlink = await service.CreateNewLinkWithOtherUser(UserSessionData.UserId, sharelinkerId, new SharelinkerLink.State() { LinkState = (int)SharelinkerLink.LinkState.Linked },noteName);
            await service.CreateNewLinkWithOtherUser(sharelinkerId, UserSessionData.UserId, new SharelinkerLink.State() { LinkState = (int)SharelinkerLink.LinkState.Linked });
            var me = await service.GetUserOfUserId(UserSessionData.UserId);

            var linkreq = new LinkMessage()
            {
                SharelinkerId = UserSessionData.UserId,
                Type = LinkMessageConstants.LINK_MESSAGE_TYPE_ACCEPT_LINK,
                Avatar = me.Avatar,
                Id = DateTime.UtcNow.Ticks.ToString(),
                Message = "accept",
                SharelinkerNick = me.NickName,
                Time = DateTime.UtcNow
            };
            Startup.PublishSubscriptionManager.PublishLinkMessages(sharelinkerId, linkreq);

            var u = await service.GetUserOfUserId(sharelinkerId);
            var newLink = new
            {
                linkId = sharelinkerId,
                masterUserId = UserSessionData.UserId,
                slaveUserId = sharelinkerId,
                status = SharelinkerLink.State.FromJson(newlink.StateDocument).LinkState.ToString(),
                createTime = DateTimeUtil.ToString(newlink.CreateTime)
            };
            var newUser = new
            {
                userId = u.Id.ToString(),
                nickName = u.NickName,
                noteName = noteName,
                avatarId = u.Avatar,
                personalVideoId = u.PersonalVideo,
                createTime = DateTimeUtil.ToString(u.CreateTime),
                motto = u.Motto
            };
            var result = new
            {
                newLink = newLink,
                newUser = newUser  
            };
            return result;
        }

        [HttpDelete("LinkMessages")]
        public async Task<bool> DeleteLinkMessages()
        {
            return await Task.Run(() =>
            {
                using (var messageCache = Startup.PublishSubscriptionManager.MessageCacheClientManager.GetClient())
                {
                    var client = messageCache.As<LinkMessage>();
                    var list = client.Lists[UserSessionData.UserId];
                    client.RemoveAllFromList(list);
                }
                return true;
            });
        }

        [HttpGet("LinkMessages")]
        public async Task<object[]> GetLinkMessages()
        {
            return await Task.Run(() =>
            {
                using (var messageCache = Startup.PublishSubscriptionManager.MessageCacheClientManager.GetClient())
                {
                    var msgs = messageCache.As<LinkMessage>().Lists[UserSessionData.UserId].GetAll().ToArray();
                    var results =  from m in msgs
                           select new
                           {
                               id = m.Id,
                               sharelinkerId = m.SharelinkerId,
                               sharelinkerNick = m.SharelinkerNick,
                               type = m.Type,
                               message = m.Message,
                               avatar = m.Avatar,
                               time = DateTimeUtil.ToString(m.Time)
                           };
                    return results.ToArray();
                }
            });
        }

        //POST /UserLinks (otherUserId,msg) : add new link with other user
        [HttpPost]
        public async Task<bool> Post(string otherUserId,string msg)
        {
            var service = this.UseSharelinkerService().GetSharelinkerService();

            var user = await service.GetUserOfUserId(UserSessionData.UserId);

            var linkmsg = new LinkMessage()
            {
                Id = DateTime.UtcNow.Ticks.ToString(),
                SharelinkerId = UserSessionData.UserId,
                SharelinkerNick = user.NickName,
                Type = LinkMessageConstants.LINK_MESSAGE_TYPE_ASKING_LINK,
                Message = msg,
                Avatar = user.Avatar,
                Time = DateTime.UtcNow
            };
            Startup.PublishSubscriptionManager.PublishLinkMessages(otherUserId, linkmsg);
            return true;
        }
    }
}
