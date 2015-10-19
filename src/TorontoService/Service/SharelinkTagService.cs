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

        public async Task<IList<SharelinkTag>> GetUserSharelinkTags(string userId)
        {
            var userOId = new ObjectId(userId);
            var collectionTag = Client.GetDatabase("Sharelink").GetCollection<SharelinkTag>("SharelinkTag");

            //Cache this data
            return await collectionTag.Find(t => t.UserId == userOId).ToListAsync();
        }

        public async Task<IDictionary<ObjectId, IEnumerable<SharelinkTag>>> GetLinkersTags(IEnumerable<ObjectId> linkerIds)
        {
            var result = new Dictionary<ObjectId, IEnumerable<SharelinkTag>>();
            var tagCollection = Client.GetDatabase("Sharelink").GetCollection<SharelinkTag>("SharelinkTag");

            var filter = new FilterDefinitionBuilder<SharelinkTag>().In(t=>t.UserId, linkerIds);
            var tags = await tagCollection.Find(filter).ToListAsync();
            var tagsGrouped = from t in tags group t by t.UserId;
            foreach (var item in tagsGrouped)
            {
                result.Add(item.Key, item.ToList());
            }
            return result;
        }

        public async Task<IList<SharelinkTag>> GetUserFocusTags(string userId)
        {
            var userOId = new ObjectId(userId);
            var collectionTag = Client.GetDatabase("Sharelink").GetCollection<SharelinkTag>("SharelinkTag");

            //Cache this data
            return await collectionTag.Find(t => t.UserId == userOId && t.IsFocus).ToListAsync();
        }

        public IEnumerable<Tuple<string, string>> MatchTags(IEnumerable<string> tagCollection1, IEnumerable<string> tagColleciton2)
        {
            var result = new List<Tuple<string, string>>();

            foreach (var tag1 in tagCollection1)
            {
                foreach (var tag2 in tagColleciton2)
                {
                    if (isTagMatch(tag1, tag2))
                    {
                        var t = Tuple.Create(tag1, tag2);
                        result.Add(t);
                    }
                }
            }

            return result;
        }

        private bool isTagMatch(string tag1, string tag2)
        {
            return tag1 == tag2;
        }

        public async Task<SharelinkTag> CreateNewSharelinkTag(string userId,string tagName, string tagColor, string data,string isFocus)
        {
            var uId = new ObjectId(userId);
            var newTag = new SharelinkTag()
            {
                TagColor = tagColor,
                TagName = tagName,
                Data = data,
                IsFocus = isFocus == null ? false : bool.Parse(isFocus),
                UserId = uId,
                LastActiveTime = DateTime.UtcNow
            };
            var collection = Client.GetDatabase("Sharelink").GetCollection<SharelinkUser>("SharelinkUser");
            var collectionTag = Client.GetDatabase("Sharelink").GetCollection<SharelinkTag>("SharelinkTag");
            await collectionTag.InsertOneAsync(newTag);
            var res = await collection.UpdateOneAsync(u => u.Id == uId, 
                new UpdateDefinitionBuilder<SharelinkUser>().Push(su => su.SharelinkTags, newTag.Id));
            return newTag;
        }

        public async Task<bool> UpdateSharelinkTag(string userId, string tagId ,string newTagName,string newColor,string data,string isFocus)
        {
            var collection = Client.GetDatabase("Sharelink").GetCollection<SharelinkUser>("SharelinkUser");
            var collectionTag = Client.GetDatabase("Sharelink").GetCollection<SharelinkTag>("SharelinkTag");
            var me = await collection.Find(u => u.Id == new ObjectId(userId)).FirstAsync();
            var tagOId = new ObjectId(tagId);
            var update = new UpdateDefinitionBuilder<SharelinkTag>();
            var uList = new List<UpdateDefinition<SharelinkTag>>();
            if (!string.IsNullOrWhiteSpace(newTagName))
            {
                var u = update.Set(tt => tt.TagName, newTagName);
                uList.Add(u);
            }

            if (!string.IsNullOrWhiteSpace(newColor))
            {
                var u = update.Set(tt => tt.TagColor, newColor);
                uList.Add(u);
            }

            if (data != null)
            {
                var u = update.Set(t => t.Data, data);
                uList.Add(u);
            }

            if (isFocus != null)
            {
                var u = update.Set(t => t.IsFocus, bool.Parse(isFocus));
                uList.Add(u);
                
            }

            if (isFocus != null || !string.IsNullOrWhiteSpace(newTagName))
            {
                var u = update.Set(tt => tt.LastActiveTime, DateTime.UtcNow);
                uList.Add(u);
            }

            if (me.SharelinkTags.Contains(tagOId))
            {
                var result = await collectionTag.UpdateOneAsync(t => t.Id == tagOId, update.Combine(uList));
                return result.ModifiedCount > 0;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> DeleteSharelinkTags(string userId, string[] tagIds)
        {
            var collection = Client.GetDatabase("Sharelink").GetCollection<SharelinkUser>("SharelinkUser");
            var collectionLink = Client.GetDatabase("Sharelink").GetCollection<SharelinkTag>("SharelinkTag");
            var ids = from id in tagIds select new ObjectId(id);
            var update = new UpdateDefinitionBuilder<SharelinkUser>().PullAll(u => u.SharelinkTags, ids);  
            var res = await collection.UpdateOneAsync(u => u.Id == new ObjectId(userId),update);
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
