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
                PublishShareUpdatedMessages(service, m.ToSharelinker.ToString(), smsg);
            }
        }

        public static void PublishShareUpdatedMessages(this BahamutPubSubService service, string userId, ShareThingUpdatedMessage updateMsg)
        {
            var cacheModel = new BahamutCacheModel
            {
                AppUniqueId = Startup.Appname,
                CacheDateTime = DateTime.UtcNow,
                UniqueId = userId,
                DeserializableString = JsonConvert.SerializeObject(updateMsg),
                Type = ShareThingUpdatedMessage.NotifyType
            };
            Startup.ServicesProvider.GetBahamutCacheService().PushCacheModelToList(cacheModel);
            var pbModel = new BahamutPublishModel
            {
                NotifyType = ShareThingUpdatedMessage.NotifyType,
                ToUser = userId
            };
            Startup.ServicesProvider.GetBahamutPubSubService().PublishBahamutUserNotifyMessage(Startup.Appname, pbModel);
            
        }

        public static void PublishChatMessages(this BahamutPubSubService service, ObjectId senderId, ShareChat chat, ChatMessage msg)
        {
            foreach (var user in chat.UserIds)
            {
                if (user != senderId)
                {

                    var idstr = user.ToString();
                    var cacheModel = new BahamutCacheModel
                    {
                        AppUniqueId = Startup.Appname,
                        CacheDateTime = DateTime.UtcNow,
                        UniqueId = idstr,
                        DeserializableString = msg.Id.ToString(),
                        Type = ChatMessage.NotifyType,
                        ExtraInfo = chat.Id.ToString()
                    };
                    Startup.ServicesProvider.GetBahamutCacheService().PushCacheModelToList(cacheModel);
                    var pbModel = new BahamutPublishModel
                    {
                        NotifyType = ChatMessage.NotifyType,
                        ToUser = idstr,
                        Info = chat.Id.ToString()
                    };
                    Startup.ServicesProvider.GetBahamutPubSubService().PublishBahamutUserNotifyMessage(Startup.Appname, pbModel);
                }
            }
        }

        public static void PublishLinkMessages(this BahamutPubSubService service, string toSharelinkerId, LinkMessage linkMessage)
        {
            
            var cacheModel = new BahamutCacheModel
            {
                AppUniqueId = Startup.Appname,
                CacheDateTime = DateTime.UtcNow,
                UniqueId = toSharelinkerId,
                DeserializableString = JsonConvert.SerializeObject(linkMessage),
                Type = LinkMessage.NotifyType
            };
            Startup.ServicesProvider.GetBahamutCacheService().PushCacheModelToList(cacheModel);
            var pbModel = new BahamutPublishModel
            {
                NotifyType = LinkMessage.NotifyType,
                ToUser = toSharelinkerId
            };
            Startup.ServicesProvider.GetBahamutPubSubService().PublishBahamutUserNotifyMessage(Startup.Appname, pbModel);
        }
    }
}
