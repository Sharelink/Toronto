using TorontoModel.MongodbModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BahamutCommon;
using BahamutService.Model;
using MongoDB.Bson;
using MongoDB.Driver;

namespace TorontoService
{
    public class ShareService : IAccountSessionData
    {
        public AccountSessionData UserSessionData { get; set; }
        IMongoClient Client = new MongoClient();

        public ShareThing PostNewShareThing(ShareThing newShareThing)
        {
            return null;
        }

        public async Task<IList<ShareThing>> GetUserShareThings(DateTime newerThanThisTime, DateTime olderThanThisTime, int page, int pageCount)
        {

            var shareThingCollection = Client.GetDatabase("Sharelink").GetCollection<ShareThing>("ShareThing");
            var activeRecordCollection = Client.GetDatabase("Sharelink").GetCollection<ShareThingActiveRecord>("ShareThingActiveRecord");
            var userCollection = Client.GetDatabase("Sharelink").GetCollection<SharelinkUser>("SharelinkUser");
            var userTagCollection = Client.GetDatabase("Sharelink").GetCollection<SharelinkTag>("SharelinkTag");
            var me = await userCollection.Find(u => u.Id == new ObjectId(UserSessionData.UserId)).FirstAsync();

            var shareUserService = new SharelinkUserService();
            var shareTagService = new SharelinkTagService();
            var linkedUserIds = await shareUserService.GetMyLinkedUserIds();
            var myTags =await shareTagService.GetMyTagNames();

            var inFilter = new FilterDefinitionBuilder<ShareThingActiveRecord>().In("ShareUserId", linkedUserIds);
            var timeNewFilter = new FilterDefinitionBuilder<ShareThingActiveRecord>().Gt("Time", newerThanThisTime);
            var timeOldFilter = new FilterDefinitionBuilder<ShareThingActiveRecord>().Lt("Time", olderThanThisTime);
            IList<ShareThingActiveRecord> records = null;
            if (newerThanThisTime != null)
            {
                records = await activeRecordCollection.Find(inFilter & timeNewFilter).ToListAsync();
            }
            else if (olderThanThisTime != null)
            {
                records = await activeRecordCollection.Find(inFilter & timeOldFilter).ToListAsync();
            }

            var sInFilter = new FilterDefinitionBuilder<ShareThing>().In("Id", records) & new FilterDefinitionBuilder<ShareThing>().AnyIn("Tags", myTags);
            var result = await shareThingCollection.Find(sInFilter).ToListAsync();
            foreach (var item in result)
            {
                item.Votes = (from v in item.Votes where linkedUserIds.Contains(v.UserId) select v).ToArray();
            }
            return result;
        }


        public ShareThing[] GetShareThingReshares(string shareId)
        {
            return null;
        }

        public async Task<IList<Vote>> GetVoteOfShare(string shareId)
        {
            var sId = new ObjectId(shareId);
            var shareThingCollection = Client.GetDatabase("Sharelink").GetCollection<ShareThing>("ShareThing");
            var share = await shareThingCollection.Find(s => s.Id == sId).FirstAsync();

            var userCollection = Client.GetDatabase("Sharelink").GetCollection<SharelinkUser>("SharelinkUser");
            var me = await userCollection.Find(u => u.Id == new ObjectId(UserSessionData.UserId)).FirstAsync();

            //TODO:Cache this
            var linkedUserIds = from lu in me.LinkedUsers select lu.SlaveUserObjectId;

            var result = from v in share.Votes where linkedUserIds.Contains(v.UserId) select v;
            return result.ToList();
        }

        public async Task<bool> VoteShare(string shareId)
        {
            var sId = new ObjectId(shareId);
            var shareThingCollection = Client.GetDatabase("Sharelink").GetCollection<ShareThing>("ShareThing");
            var newVote = new Vote()
            {
                UserId = new ObjectId(UserSessionData.UserId),
                VoteTime = DateTime.Now
            };
            var result = await shareThingCollection.UpdateOneAsync(s => s.Id == sId, new UpdateDefinitionBuilder<ShareThing>().AddToSet(ts => ts.Votes, newVote));

            return result.ModifiedCount > 0;
        }

        public async Task<bool> UnvoteShare(string shareId)
        {
            var sId = new ObjectId(shareId);
            var shareThingCollection = Client.GetDatabase("Sharelink").GetCollection<ShareThing>("ShareThing");
            var newVote = new Vote()
            {
                UserId = new ObjectId(UserSessionData.UserId),
                VoteTime = DateTime.Now
            };
            var result = await shareThingCollection.UpdateOneAsync(s => s.Id == sId, new UpdateDefinitionBuilder<ShareThing>().Pull(ts => ts.Votes, newVote));
            return result.ModifiedCount > 0;
        }
    }

    public static class ShareServiceExtensions
    {
        public static IBahamutServiceBuilder UseShareService(this IBahamutServiceBuilder builder,params object[] args)
        {
            return builder.UseService<ShareService>(args);
        }

        public static ShareService GetShareService(this IBahamutServiceBuilder builder)
        {
            return builder.GetService<ShareService>();
        }
    }
}
