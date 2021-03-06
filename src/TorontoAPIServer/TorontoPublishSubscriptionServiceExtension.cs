﻿using BahamutCommon;
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

    public class PublishConstants
    {
        public const string NotifyId = "Toronto";
    }
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
                NotifyType = "Toronto",
                ToUser = userId,
                CustomCmd = "UsrNewSTMsg",
                NotifyInfo = JsonConvert.SerializeObject(new { LocKey = "NEW_SHARE_NOTIFICATION" })
            };
            Startup.ServicesProvider.GetBahamutPubSubService().PublishBahamutUserNotifyMessage(PublishConstants.NotifyId, pbModel);
            
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
                        NotifyType = "Toronto",
                        ToUser = idstr,
                        CustomCmd = "UsrNewMsg",
                        NotifyInfo = JsonConvert.SerializeObject(new { LocKey = "NEW_MSG_NOTIFICATION" })
                    };
                    Startup.ServicesProvider.GetBahamutPubSubService().PublishBahamutUserNotifyMessage(PublishConstants.NotifyId, pbModel);
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
                NotifyType = "Toronto",
                ToUser = toSharelinkerId,
                CustomCmd = "UsrNewLinkMsg",
                NotifyInfo = JsonConvert.SerializeObject(new { LocKey = "NEW_FRI_MSG_NOTIFICATION" })
            };
            Startup.ServicesProvider.GetBahamutPubSubService().PublishBahamutUserNotifyMessage(PublishConstants.NotifyId, pbModel);
        }
    }
}
