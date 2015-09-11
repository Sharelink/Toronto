using BahamutCommon;
using BahamutService.Model;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TorontoModel.MongodbModel;

namespace TorontoService
{
    public class SharelinkTagService
    {
        public IMongoClient Client { get; set; }

        public SharelinkTagService(IMongoClient client)
        {
            Client = client;
        }

        public async Task<IList<SharelinkTag>> GetUserSharelinkTags(string UserId)
        {
            var userOId = new ObjectId(UserId);
            var collection = Client.GetDatabase("Sharelink").GetCollection<SharelinkUser>("SharelinkUser");
            var collectionTag = Client.GetDatabase("Sharelink").GetCollection<SharelinkTag>("SharelinkTag");
            var me = await collection.Find(u => u.Id == userOId).FirstAsync();

            //Cache this data
            return await collectionTag.Find(ul => me.SharelinkTags.Contains(ul.Id)).ToListAsync();
        }

        public async Task<IList<SharelinkTag>> GetMyAllSharelinkTags(string userId)
        {
            return await GetUserSharelinkTags(userId);
        }

        public async Task<SharelinkTag> CreateNewSharelinkTag(string userId,string tagName, string tagColor)
        {
            var newTag = new SharelinkTag()
            {
                TagColor = tagColor,
                TagName = tagName
            };
            var collection = Client.GetDatabase("Sharelink").GetCollection<SharelinkUser>("SharelinkUser");
            var collectionTag = Client.GetDatabase("Sharelink").GetCollection<SharelinkTag>("SharelinkTag");
            await collectionTag.InsertOneAsync(newTag);
            var res = await collection.UpdateOneAsync(u => u.Id == new ObjectId(userId),
                new UpdateDefinitionBuilder<SharelinkUser>().AddToSet(su => su.SharelinkTags, newTag.Id));
            return newTag;
        }

        public async Task<bool> UpdateSharelinkTagColor(string userId, string tagId ,string newTagColor)
        {
            var collection = Client.GetDatabase("Sharelink").GetCollection<SharelinkUser>("SharelinkUser");
            var collectionTag = Client.GetDatabase("Sharelink").GetCollection<SharelinkTag>("SharelinkTag");
            var me = await collection.Find(u => u.Id == new ObjectId(userId)).FirstAsync();
            var tagOId = new ObjectId(tagId);
            if (me.SharelinkTags.Contains(tagOId))
            {
                var result = await collectionTag.UpdateOneAsync(t => t.Id == tagOId, new UpdateDefinitionBuilder<SharelinkTag>().Set(tt => tt.TagColor, newTagColor));
                return result.ModifiedCount > 0;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> UpdateSharelinkTagName(string userId, string tagId ,string newTagName)
        {
            var collection = Client.GetDatabase("Sharelink").GetCollection<SharelinkUser>("SharelinkUser");
            var collectionTag = Client.GetDatabase("Sharelink").GetCollection<SharelinkTag>("SharelinkTag");
            var me = await collection.Find(u => u.Id == new ObjectId(userId)).FirstAsync();
            var tagOId = new ObjectId(tagId);
            if (me.SharelinkTags.Contains(tagOId))
            {
                var result = await collectionTag.UpdateOneAsync(t => t.Id == tagOId, new UpdateDefinitionBuilder<SharelinkTag>().Set(tt => tt.TagName, newTagName));
                return result.ModifiedCount > 0;
            }
            else
            {
                return false;
            }
        }

        public async Task<IEnumerable<string>> GetMyTagNames(string userId)
        {
            var userShareLinkTags = await GetUserSharelinkTags(userId);
            return from mt in userShareLinkTags select mt.TagName;
        }

        public async Task<bool> DeleteSharelinkTag(string userId, string tagId)
        {
            var collection = Client.GetDatabase("Sharelink").GetCollection<SharelinkUser>("SharelinkUser");
            var collectionLink = Client.GetDatabase("Sharelink").GetCollection<SharelinkTag>("SharelinkTag");

            var res = await collection.UpdateOneAsync(u => u.Id == new ObjectId(userId),
                new UpdateDefinitionBuilder<SharelinkUser>().Pull(su => su.SharelinkTags, new ObjectId(tagId)));
            return res.ModifiedCount > 0;
        }

    }

    public static class SharelinkTagServiceExtensions
    {
        public static IBahamutServiceBuilder UseSharelinkTagService(this IBahamutServiceBuilder builder, params object[] args)
        {
            return builder.UseService<SharelinkTagService>(args);
        }

        public static SharelinkTagService GetSharelinkTagService(this IBahamutServiceBuilder builder)
        {
            return builder.GetService<SharelinkTagService>();
        }
    }
}
