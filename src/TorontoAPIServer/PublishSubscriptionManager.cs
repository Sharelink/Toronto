using MongoDB.Bson;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TorontoModel.MongodbModel;

namespace TorontoAPIServer
{
    public class PublishSubscriptionManager
    {
        private IRedisClientsManager mcClientManager;
        private IRedisClientsManager psClientManager { get; set; }
        public IRedisClientsManager MessageCacheClientManager { get { return mcClientManager; } }

        public PublishSubscriptionManager(IRedisClientsManager psClientManager, IRedisClientsManager mcClientManager)
        {
            this.mcClientManager = mcClientManager;
            this.psClientManager = psClientManager;
        }

        public void PublishShareMessages(List<ShareThingMail> mails)
        {
            using (var psClient = psClientManager.GetClient())
            {
                using (var msgClient = mcClientManager.GetClient())
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
                        psClient.PublishMessage(sharelinker, string.Format("ShareThingMessage:{0}", m.ShareId.ToString()));
                    }

                }
            }
        }

        public void PublishShareUpdatedMessages(string userId, ShareThingUpdatedMessage updateMsg)
        {
            using (var psClient = psClientManager.GetClient())
            {
                using (var msgClient = mcClientManager.GetClient())
                {
                    msgClient.As<ShareThingUpdatedMessage>().Lists[userId].Add(
                        updateMsg);
                }
                psClient.PublishMessage(userId, string.Format("ShareThingMessage:{0}", updateMsg.ShareId.ToString()));
            }
        }

        public void PublishChatMessages(ObjectId senderId, ShareChat chat, ChatMessage msg)
        {
            using (var psClient = psClientManager.GetClient())
            {
                using (var msc = mcClientManager.GetClient())
                {
                    foreach (var user in chat.UserIds)
                    {
                        if (user != senderId)
                        {
                            var idstr = user.ToString();
                            msc.As<ChatMessage>().Lists[idstr].Add(msg);
                            psClient.PublishMessage(idstr, "ChatMessage:" + chat.ChatId);
                        }
                    }
                }

            }
        }

        public void PublishLinkMessages(string toSharelinkerId, LinkMessage linkMessage)
        {
            using (var psClient = psClientManager.GetClient())
            {
                using (var messageCache = mcClientManager.GetClient())
                {
                    messageCache.As<LinkMessage>().Lists[toSharelinkerId].Add(linkMessage);
                }
                psClient.PublishMessage(toSharelinkerId, "LinkMessage:" + "new");
            }
        }
    }
}
