using BahamutCommon;
using MongoDB.Bson;
using MongoDB.Driver;
using ServiceStack.Redis;
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

        public IMongoCollection<ShareChat> ChatCollection
        {
            get
            {
                return Client.GetDatabase("SharelinkMessage").GetCollection<ShareChat>("ShareChat");
            }
        }

        public MessageService(IMongoClient client)
        {
            Client = client;
        }

        public async Task<ShareChat> GetOrCreateChat(string chatId, string userId, string shareId,  string audienceId)
        {
            
            try
            {
                var chat = await ChatCollection.Find(m => m.ChatId == chatId).SingleAsync();
                return chat;
            }
            catch (Exception)
            {
                var newChat = new ShareChat()
                {
                    ChatId = chatId,
                    ShareId = new ObjectId(shareId),
                    Time = DateTime.UtcNow,
                    UserIds = new ObjectId[] { new ObjectId(userId), new ObjectId(audienceId) }
                };
                await ChatCollection.InsertOneAsync(newChat);
                return newChat;
            }
            
        }


        public async Task<IList<SharelinkMessage>> GetMessage(string userId,string chatId,string newerThanTime,bool includeOwnMessage = false)
        {
            var collection = MessageCollection;
            DateTime date = DateTime.Parse(newerThanTime);
            if (includeOwnMessage)
            {
                var result = collection.Find(m => m.ChatId == chatId && m.Time > date).SortBy(m => m.Time);
                return await result.ToListAsync();
            }
            else
            {
                var userOId = new ObjectId(userId);
                var result = collection.Find(m => m.ChatId == chatId && m.Time > date && m.SenderId != userOId).SortBy(m => m.Time);
                return await result.ToListAsync();
            }
            
        }

        public async Task<SharelinkMessage> NewMessage(SharelinkMessage msg)
        {
            var collection = MessageCollection;
            await collection.InsertOneAsync(msg);
            return msg;
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
