﻿using BahamutCommon;
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

        public async Task<IList<ISharelinkTag>> GetUserSharelinkTags(string userId)
        {
            var userOId = new ObjectId(userId);
            var collection = Client.GetDatabase("Sharelink").GetCollection<SharelinkUser>("SharelinkUser");
            var collectionTag = Client.GetDatabase("Sharelink").GetCollection<ISharelinkTag>("SharelinkTag");
            var me = await collection.Find(u => u.Id == userOId).FirstAsync();
            var tagIds = me.SharelinkTags;
            //Cache this data
            return await collectionTag.Find(ul => tagIds.Contains(ul.Id)).ToListAsync();
        }

        public async Task<IList<ISharelinkTag>> GetUserFocusTags(string userId)
        {
            var tags = await GetUserSharelinkTags(userId);
            var result = from t in tags where t.IsFocus select t;
            return result.ToList();
        }

        public async Task<IList<ISharelinkTag>> GetMyAllSharelinkTags(string userId)
        {
            return await GetUserSharelinkTags(userId);
        }

        public async Task<ISharelinkTag> CreateNewSharelinkTag(string userId,string tagName, string tagColor, string data,string isFocus)
        {
            var uId = new ObjectId(userId);
            var newTag = new SharelinkTag()
            {
                TagColor = tagColor,
                TagName = tagName,
                Data = data,
                IsFocus = isFocus == null ? false : bool.Parse(isFocus),
                UserId = uId,
                LastActiveTime = DateTime.Now
            };
            var collection = Client.GetDatabase("Sharelink").GetCollection<SharelinkUser>("SharelinkUser");
            var collectionTag = Client.GetDatabase("Sharelink").GetCollection<ISharelinkTag>("SharelinkTag");
            await collectionTag.InsertOneAsync(newTag);
            var res = await collection.UpdateOneAsync(u => u.Id == uId, 
                new UpdateDefinitionBuilder<SharelinkUser>().AddToSet(su => su.SharelinkTags, newTag.Id));
            return newTag;
        }

        public async Task<bool> UpdateSharelinkTag(string userId, string tagId ,string newTagName,string newColor,string data,string isFocus)
        {
            var collection = Client.GetDatabase("Sharelink").GetCollection<SharelinkUser>("SharelinkUser");
            var collectionTag = Client.GetDatabase("Sharelink").GetCollection<ISharelinkTag>("SharelinkTag");
            var me = await collection.Find(u => u.Id == new ObjectId(userId)).FirstAsync();
            var tagOId = new ObjectId(tagId);
            var update = new UpdateDefinitionBuilder<ISharelinkTag>();
            var uList = new List<UpdateDefinition<ISharelinkTag>>();
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
                var u = update.Set(tt => tt.LastActiveTime, DateTime.Now);
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

        public async Task<IEnumerable<string>> GetMyTagNames(string userId)
        {
            var userShareLinkTags = await GetUserSharelinkTags(userId);
            return from mt in userShareLinkTags select mt.TagName;
        }

        public async Task<bool> DeleteSharelinkTags(string userId, string[] tagIds)
        {
            var collection = Client.GetDatabase("Sharelink").GetCollection<SharelinkUser>("SharelinkUser");
            var collectionLink = Client.GetDatabase("Sharelink").GetCollection<ISharelinkTag>("SharelinkTag");
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
