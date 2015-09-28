using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using TorontoService;
using BahamutCommon;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace TorontoAPIServer.Controllers
{
    [Route("api/[controller]")]
    public class MessagesController : TorontoAPIController
    {
        // GET: api/values
        [HttpGet("{shareId}")]
        public object Get(string shareId,string olderThanId = null,string senderId = null,int limit = 20)
        {
            var messageService = this.UseMessageService().GetMessageService();
            var messages = Task.Run(async () =>
            {
                return await messageService.GetMessage(UserSessionData.UserId, shareId, senderId, olderThanId, limit);
            }).Result;
            return new
            {
                noMoreMessage = messages.Count > 0 ? false : true,
                shareId = shareId,
                messages = from m in messages select new
                {
                    msgId = m.Id.ToString(),
                    senderId = m.SenderId.ToString(),
                    msg = m.MessageContent,
                    time = DateTimeUtil.ToString(m.Time)
                }
            };
        }

        // POST api/values
        [HttpPost("{shareId}")]
        public object Post(string shareId,string message,string toUserId)
        {
            var messageService = this.UseMessageService().GetMessageService();
            var newmsg = Task.Run(async () =>
            {
                return await messageService.sendMessageTo(shareId, UserSessionData.UserId, toUserId, message);
            }).Result;
            return new
            {
                msgId = newmsg.Id.ToString(),
                time = newmsg.Time
            };
        }

    }
}
