using BahamutCommon;
using BahamutService.Service;
using MongoDB.Bson;
using Newtonsoft.Json;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TorontoModel.MongodbModel;

namespace TorontoAPIServer
{


    public static class TorontoPublishSubscriptionServiceExtension
    {

        public static void PublishShareMessages(this BahamutPubSubService service, List<ShareThingMail> mails)
        {
            foreach (var m in mails)
            {
                var smsg = new ShareThingUpdatedMessage()
                {
                    ShareId = m.ShareId.ToString(),
                    Time = DateTime.UtcNow
                };
                var msg = new BahamutUserAppNotifyMessage()
                {
                    DeserializableMessage = JsonConvert.SerializeObject(smsg),
                    NotificationType = ShareThingUpdatedMessage.NotifyType
                };
                var sharelinker = m.ToSharelinker.ToString();
                service.PublishBahamutUserNotifyMessage(Startup.Appname, sharelinker, msg);
            }
        }

        public static void PublishShareUpdatedMessages(this BahamutPubSubService service, string userId, ShareThingUpdatedMessage updateMsg)
        {
            var msg = new BahamutUserAppNotifyMessage()
            {
                DeserializableMessage = JsonConvert.SerializeObject(updateMsg),
                NotificationType = ShareThingUpdatedMessage.NotifyType
            };
            service.PublishBahamutUserNotifyMessage(Startup.Appname, userId, msg);
        }

        public static void PublishChatMessages(this BahamutPubSubService service, ObjectId senderId, ShareChat chat, ChatMessage msg)
        {
            foreach (var user in chat.UserIds)
            {
                if (user != senderId)
                {
                    var nmsg = new BahamutUserAppNotifyMessage()
                    {
                        ExtraInfo = chat.Id.ToString(),
                        DeserializableMessage = msg.Id.ToString(),
                        NotificationType = ChatMessage.NotifyType
                    };
                    var idstr = user.ToString();
                    service.PublishBahamutUserNotifyMessage(Startup.Appname, idstr, nmsg);
                }
            }
        }

        public static void PublishLinkMessages(this BahamutPubSubService service, string toSharelinkerId, LinkMessage linkMessage)
        {
            var msg = new BahamutUserAppNotifyMessage()
            {
                DeserializableMessage = JsonConvert.SerializeObject(linkMessage),
                NotificationType = LinkMessage.NotifyType
            };
            service.PublishBahamutUserNotifyMessage(Startup.Appname, toSharelinkerId, msg);
        }
    }
}
