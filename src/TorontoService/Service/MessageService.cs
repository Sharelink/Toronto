using BahamutCommon;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TorontoModel.MongodbModel;

namespace TorontoService
{
    public class MessageService
    {
        public IMongoClient Client { get; set; }

        public IMongoCollection<SharelinkMessage> MessageCollection
        {
            get
            {
                return Client.GetDatabase("SharelinkMessage").GetCollection<SharelinkMessage>("SharelinkMessage");
            }
        }

        public MessageService(IMongoClient client)
        {
            Client = client;
        }

        public async Task<IList<SharelinkMessage>> GetMessage(string userId,string shareId,string senderId = null,string olderThanId = null, int count = 20)
        {
            var collection = MessageCollection;
            DateTime date;
            if (olderThanId == null)
            {
                var oId = new ObjectId(olderThanId);
                var olderMessage = await collection.Find(m => m.Id == oId).SingleAsync();
                date = olderMessage.Time;
            }
            else
            {
                date = DateTime.Now;
            }
            var userOId = new ObjectId(userId);
            var shareOId = new ObjectId(shareId);
            if (string.IsNullOrWhiteSpace(senderId))
            {
                var senderOId = new ObjectId(senderId);
                var result = collection.Find(m => m.ShareId == shareOId && m.SenderId == senderOId && m.ToSharelinkerId == userOId && m.Time < date).SortBy(m => m.Time).Limit(count);
                return await result.ToListAsync();
            }
            else
            {
                var result = collection.Find(m => m.ShareId == shareOId && m.ToSharelinkerId == userOId && m.Time < date).SortBy(m => m.Time).Limit(count);
                return await result.ToListAsync();
            }
            
        }

        public async Task<SharelinkMessage> sendMessageTo(string shareId, string userId, string toSharelinker, string message)
        {
            var shareOId = new ObjectId(shareId);
            var userOId = new ObjectId(userId);
            var toLinkerOId = new ObjectId(toSharelinker);
            var collection = MessageCollection;
            var newMessage = new SharelinkMessage()
            {
                MessageContent = message,
                SenderId = userOId,
                ShareId = shareOId,
                Time = DateTime.Now,
                ToSharelinkerId = toLinkerOId
            };
            await collection.InsertOneAsync(newMessage);
            return newMessage;
        }


    }

    public static class MessageServiceExtensions
    {
        public static IBahamutServiceBuilder UseMessageService(this IBahamutServiceBuilder builder, params object[] args)
        {
            return builder.UseService<MessageService>(args);
        }

        public static MessageService GetMessageService(this IBahamutServiceBuilder builder)
        {
            return builder.GetService<MessageService>();
        }
    }
}
