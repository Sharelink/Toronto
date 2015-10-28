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

    public class SharelinkTagConstant
    {
        #region tag domain
        public const string TAG_DOMAIN_CUSTOM = "custom:";
        public const string TAG_DOMAIN_SYSTEM = "system:";
        #endregion

        #region tag types
        public const string TAG_TYPE_KEYWORD = "keyword:";
        public const string TAG_TYPE_GEO = "geo:";
        public const string TAG_TYPE_SHARELINKER = "sharelinker:";
        #endregion

        #region system tag types
        public const string TAG_TYPE_FEEDBACK = "feedback:";
        public const string TAG_TYPE_BROADCAST = "broadcast:";
        public const string TAG_TYPE_PRIVATE = "private:";
        #endregion

        public static IEnumerable<SharelinkTag> SystemTags = SharelinkTagUtil.InitSystemTags();
    }

    public static class SharelinkTagUtil
    {
        
        public static IEnumerable<SharelinkTag> InitSystemTags()
        {
            var tags = new List<SharelinkTag>();
            tags.Add(new SharelinkTag()
            {
                TagDomain = SharelinkTagConstant.TAG_DOMAIN_SYSTEM,
                TagType = SharelinkTagConstant.TAG_TYPE_FEEDBACK
            });

            tags.Add(new SharelinkTag()
            {
                TagDomain = SharelinkTagConstant.TAG_DOMAIN_SYSTEM,
                TagType = SharelinkTagConstant.TAG_TYPE_GEO
            });

            tags.Add(new SharelinkTag()
            {
                TagDomain = SharelinkTagConstant.TAG_DOMAIN_SYSTEM,
                TagType = SharelinkTagConstant.TAG_TYPE_BROADCAST
            });

            tags.Add(new SharelinkTag()
            {
                TagDomain = SharelinkTagConstant.TAG_DOMAIN_SYSTEM,
                TagType = SharelinkTagConstant.TAG_TYPE_PRIVATE
            });

            foreach (var tag in tags)
            {
                tag.Id = new ObjectId(tag.TagDomain + tag.TagType);
                tag.IsFocus = true;
                tag.TagColor = "#438ccb";
            }
            return tags;
        }

        public static SharelinkTag GeneratePersonTag(string tagDomain, string userId)
        {
            var tag = new SharelinkTag()
            {
                Data = userId,
                TagDomain = tagDomain,
                TagType = SharelinkTagConstant.TAG_TYPE_SHARELINKER,
                TagColor = "#438ccb",
                IsFocus = true,
                Id = new ObjectId(userId),
                UserId = new ObjectId(userId),
                ShowToLinkers = true
            };
            return tag;
        }

        public static SharelinkTag GenerateKeyworkTag(string keyword)
        {
            return new SharelinkTag()
            {
                Data = keyword,
                TagDomain = SharelinkTagConstant.TAG_DOMAIN_CUSTOM,
                TagType = SharelinkTagConstant.TAG_TYPE_KEYWORD
            };
        }

        public static bool IsTagMatch(this SharelinkTag tag1,SharelinkTag tag2)
        {
            return tag1.Data == tag2.Data;
        }

        public static bool IsSystemTag(this SharelinkTag tag)
        {
            return SharelinkTagConstant.TAG_DOMAIN_SYSTEM == tag.TagDomain;
        }

        public static bool IsCustomTag(this SharelinkTag tag)
        {
            return SharelinkTagConstant.TAG_DOMAIN_CUSTOM == tag.TagDomain;
        }

        public static bool TagIsTypeOf(this SharelinkTag tag,string tagType)
        {
            return tagType == tag.TagType;
        }

        public static bool IsSharelinkerTag(this SharelinkTag tag)
        {
            return TagIsTypeOf(tag, SharelinkTagConstant.TAG_TYPE_SHARELINKER);
        }

        public static bool IsBroadcastTag(this SharelinkTag tag)
        {
            return TagIsTypeOf(tag, SharelinkTagConstant.TAG_TYPE_BROADCAST);
        }

        public static bool IsPrivateTag(this SharelinkTag tag)
        {
            return TagIsTypeOf(tag, SharelinkTagConstant.TAG_TYPE_PRIVATE);
        }

        public static bool IsFeedbackTag(this SharelinkTag tag)
        {
            return TagIsTypeOf(tag, SharelinkTagConstant.TAG_TYPE_FEEDBACK);
        }
    }

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
            var tags = await collectionTag.Find(t => t.UserId == userOId).ToListAsync();
            tags.Add(SharelinkTagUtil.GeneratePersonTag(SharelinkTagConstant.TAG_DOMAIN_SYSTEM, userId));
            tags.AddRange(SharelinkTagConstant.SystemTags);

            return tags;
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

        public IEnumerable<string> MatchTags(IEnumerable<SharelinkTag> shareTagCollection, IEnumerable<SharelinkTag> userTagColleciton)
        {
            var result = new List<string>();

            foreach (var tag1 in shareTagCollection)
            {
                foreach (var tag2 in userTagColleciton)
                {
                    if (tag1.IsTagMatch(tag2))
                    {
                        result.Add(tag2.TagName);
                    }
                }
            }
            return result;
        }

        public async Task<SharelinkTag> CreateNewSharelinkTag(SharelinkTag newTag)
        {
            var collection = Client.GetDatabase("Sharelink").GetCollection<Sharelinker>("Sharelinker");
            var collectionTag = Client.GetDatabase("Sharelink").GetCollection<SharelinkTag>("SharelinkTag");
            await collectionTag.InsertOneAsync(newTag);
            var res = await collection.UpdateOneAsync(u => u.Id == newTag.UserId, 
                new UpdateDefinitionBuilder<Sharelinker>().Push(su => su.SharelinkTags, newTag.Id));
            return newTag;
        }

        public async Task<bool> UpdateSharelinkTag(string userId, string tagId ,string newTagName,string newColor,string data,string isFocus)
        {
            var collection = Client.GetDatabase("Sharelink").GetCollection<Sharelinker>("Sharelinker");
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
            ObjectId userOId = new ObjectId(userId);
            var collection = Client.GetDatabase("Sharelink").GetCollection<Sharelinker>("Sharelinker");
            var collectionTags = Client.GetDatabase("Sharelink").GetCollection<SharelinkTag>("SharelinkTag");
            var ids = from id in tagIds select new ObjectId(id);
            var filter = new FilterDefinitionBuilder<SharelinkTag>().In(t => t.Id, ids);
            var tags = await collectionTags.Find(filter).ToListAsync();
            var removeIds = from t in tags where SharelinkTagUtil.IsCustomTag(t) select t.Id;
            var update = new UpdateDefinitionBuilder<Sharelinker>().PullAll(u => u.SharelinkTags, removeIds);  
            var res = await collection.UpdateOneAsync(u => u.Id == userOId ,update);
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
