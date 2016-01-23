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
        public async Task<object[]> Get(string chatId, string newerThanTime)
        {
            var messageService = this.UseMessageService().GetMessageService();
            var msgList = await messageService.GetMessage(UserSessionData.UserId, chatId, newerThanTime);
            var messages = from m in msgList
                           select new
                           {
                               shareId = m.ShareId,
                               msgId = m.Id.ToString(),
                               senderId = m.SenderId.ToString(),
                               chatId = m.ChatId,
                               data = m.MessageData,
                               msg = m.Message,
                               time = DateTimeUtil.ToAccurateDateTimeString(m.Time),
                               msgType = m.MessageType
                           };
            return messages.ToArray();
        }

        [HttpDelete("New")]
        public async Task DeleteNewMessages()
        {
            await Task.Run(() =>
            {
                using (var msc = Startup.PublishSubscriptionManager.MessageCacheClientManager.GetClient())
                {
                    var client = msc.As<ChatMessage>();
                    var list = client.Lists[UserSessionData.UserId];
                    client.RemoveAllFromList(list);
                }
            });
        }

        [HttpGet("New")]
        public async Task<object[]> GetNewMessage()
        {
            return await Task.Run(() =>
            {
                using (var msc = Startup.PublishSubscriptionManager.MessageCacheClientManager.GetClient())
                {
                    var list = msc.As<ChatMessage>().Lists[UserSessionData.UserId];
                    var msgList = msc.As<ChatMessage>().GetAllItemsFromList(list);
                    var messages = from m in msgList
                                   select new
                                   {
                                       msgId = m.Id.ToString(),
                                       senderId = m.SenderId.ToString(),
                                       shareId = m.ShareId,
                                       chatId = m.ChatId,
                                       data = m.MessageData,
                                       msg = m.Message == null ? "reserved" : m.Message,
                                       time = DateTimeUtil.ToAccurateDateTimeString(m.Time),
                                       msgType = m.MessageType
                                   };
                    return messages.ToArray();
                }
            });
        }

        // POST api/values
        [HttpPost("{chatId}")]
        public async Task<object> Post(string message, string chatId, string type, string messageData, string time, string audienceId, string shareId)
        {
            var messageService = this.UseMessageService().GetMessageService();

            var msg = new ChatMessage()
            {
                Message = message,
                MessageData = messageData,
                MessageType = type,
                SenderId = new ObjectId(UserSessionData.UserId),
                Time = DateTime.UtcNow,
                ChatId = chatId,
                ShareId = shareId
            };
            var sendUserOId = new ObjectId(UserSessionData.UserId);
            var chat = await messageService.GetOrCreateChat(chatId, UserSessionData.UserId, shareId, audienceId);
            msg = await messageService.NewMessage(msg);
            Startup.PublishSubscriptionManager.PublishChatMessages(sendUserOId, chat, msg);
            return new
            {
                msgId = msg.Id.ToString()
            };
        }

    }
}
