using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using TorontoService;
using BahamutCommon;
using TorontoModel.MongodbModel;
using MongoDB.Bson;
using ServiceStack.Redis;
using ServiceStack.Redis.Generic;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace TorontoAPIServer.Controllers
{
    [Route("api/[controller]")]
    public class MessagesController : TorontoAPIController
    {
        // GET: api/values
        [HttpGet("{chatId}")]
        public object Get(string chatId,string newerThanTime)
        {
            var messageService = this.UseMessageService().GetMessageService();
            var msgList = Task.Run(async () =>
            {
                return await messageService.GetMessage(UserSessionData.UserId, chatId, newerThanTime);
            }).Result;
            var messages = from m in msgList
                           select new
                       {
                           msgId = m.Id.ToString(),
                           senderId = m.SenderId.ToString(),
                           chatId = m.ChatId,
                           data = m.MessageData,
                           msg = m.Message,
                           time = DateTimeUtil.ToAccurateDateTimeString(m.Time),
                           msgType = m.MessageType
                       };
            return messages;
        }

        [HttpDelete("New")]
        public void DeleteNewMessages()
        {
            using (var msc = Startup.MessageCacheServerClientManager.GetClient())
            {
                var list = msc.As<SharelinkMessage>().Lists[UserSessionData.UserId];
                msc.As<SharelinkMessage>().RemoveAllFromList(list);
            }
        }

        [HttpGet("New")]
        public object GetNewMessage()
        {
            using (var msc = Startup.MessageCacheServerClientManager.GetClient())
            {
                var list = msc.As<SharelinkMessage>().Lists[UserSessionData.UserId];
                var msgList = msc.As<SharelinkMessage>().GetAllItemsFromList(list);
                var messages = from m in msgList
                           select new
                           {
                               msgId = m.Id.ToString(),
                               senderId = m.SenderId.ToString(),
                               chatId = m.ChatId,
                               data = m.MessageData,
                               msg = m.Message,
                               time = DateTimeUtil.ToAccurateDateTimeString(m.Time),
                               msgType = m.MessageType
                           };
                msc.As<SharelinkMessage>().RemoveAllFromList(list);
                return messages;
            }
        }

        // POST api/values
        [HttpPost("{chatId}")]
        public object Post(string message,string chatId,string type, string messageData,string time,string audienceId,string shareId)
        {
            var messageService = this.UseMessageService().GetMessageService();
            
            var msg = new SharelinkMessage()
            {
                Message = message,
                MessageData = messageData,
                MessageType = type,
                SenderId = new ObjectId(UserSessionData.UserId),
                Time = DateTimeUtil.ToDate(time),
                ChatId = chatId
            };
            var sendUserOId = new ObjectId(UserSessionData.UserId);
            var newmsgId = Task.Run(async () =>
            {
                var chat = await messageService.GetOrCreateChat(chatId, UserSessionData.UserId, shareId, audienceId);
                msg = await messageService.NewMessage(msg);
                using (var psClient = Startup.MessagePubSubServerClientManager.GetClient())
                {
                    using (var msc = Startup.MessageCacheServerClientManager.GetClient())
                    {
                        foreach (var user in chat.UserIds)
                        {
                            if (user != sendUserOId)
                            {
                                msc.As<SharelinkMessage>().Lists[UserSessionData.UserId].Add(msg);
                                psClient.PublishMessage(UserSessionData.UserId, chatId);
                            }
                        }
                    }
                    
                }
                
                return msg.Id.ToString();
            }).Result;
            return new
            {
                msgId = newmsgId
            };
        }

    }
}
