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
    public class ShareService
    {
        IMongoClient Client = new MongoClient();

        public ShareService(IMongoClient client)
        {
            Client = client;
        }

        public async Task<ShareThing> PostNewShareThing(ShareThing newShareThing)
        {
            var shareThingCollection = Client.GetDatabase("Sharelink").GetCollection<ShareThing>("ShareThing");
            await shareThingCollection.InsertOneAsync(newShareThing);
            return newShareThing;
        }

        public async void InsertMails(IEnumerable<ShareThingMail> mails)
        {
            var shareThingMailCollection = Client.GetDatabase("Sharelink").GetCollection<ShareThingMail>("ShareThingMail");
            await shareThingMailCollection.InsertManyAsync(mails);
        }

        public async Task<IList<ShareThingMail>> GetUserShareMails(string userId, DateTime beginTime, DateTime endTime, int page, int pageCount)
        {
            var shareThingMailCollection = Client.GetDatabase("Sharelink").GetCollection<ShareThingMail>("ShareThingMail");
            var uOId = new ObjectId(userId);
            var shareUserService = new SharelinkUserService(Client);
            var shareTagService = new SharelinkTagService(Client);
            var filterBuilder = new FilterDefinitionBuilder<ShareThingMail>();
            var shareMails = await shareThingMailCollection.FindAsync(m =>m.ToSharelinker == uOId && m.Time >= beginTime && m.Time < endTime);
            var mails = await shareMails.ToListAsync();
            if (page == -1)
            {
                return mails.ToArray();
            }
            var result = from m in mails orderby m.Time descending select m;
            return result.Skip(page * pageCount).Take(pageCount).ToArray();
        }

        public async Task<IList<ShareThing>> GetShares(IEnumerable<ObjectId> shareIds)
        {
            var shareThingCollection = Client.GetDatabase("Sharelink").GetCollection<ShareThing>("ShareThing");
            var fileter = new FilterDefinitionBuilder<ShareThing>().In(s => s.Id, shareIds);
            var shareThings = shareThingCollection.Find(fileter);
            return await shareThings.ToListAsync();
        }

        public async Task<IList<Vote>> GetVoteOfShare(string userId, string shareId)
        {
            var sId = new ObjectId(shareId);
            var shareThingCollection = Client.GetDatabase("Sharelink").GetCollection<ShareThing>("ShareThing");
            var share = await shareThingCollection.Find(s => s.Id == sId).FirstAsync();
            var uOId = new ObjectId(userId);
            var userCollection = Client.GetDatabase("Sharelink").GetCollection<SharelinkUser>("SharelinkUser");
            var me = await userCollection.Find(u => u.Id == uOId).FirstAsync();

            //TODO:Cache this
            var linkedUserIds = me.LinkedUsers;

            var result = from v in share.Votes where linkedUserIds.Contains(v.UserId) select v;
            return result.ToList();
        }

        public async Task<ShareThing> VoteShare(string userId, string shareId)
        {
            var sId = new ObjectId(shareId);
            var shareThingCollection = Client.GetDatabase("Sharelink").GetCollection<ShareThing>("ShareThing");
            var newVote = new Vote()
            {
                UserId = new ObjectId(userId),
                VoteTime = DateTime.UtcNow
            };
            var update = new UpdateDefinitionBuilder<ShareThing>().Push(ts => ts.Votes, newVote);
            var result = await shareThingCollection.FindOneAndUpdateAsync(s => s.Id == sId, update);

            return result;
        }

        public async Task<ShareThing> UnvoteShare(string userId, string shareId)
        {
            var sId = new ObjectId(shareId);
            var shareThingCollection = Client.GetDatabase("Sharelink").GetCollection<ShareThing>("ShareThing");
            var UserId = new ObjectId(userId);
            var op = new UpdateDefinitionBuilder<ShareThing>().PullFilter(t => t.Votes, t => t.UserId == UserId);
            var result = await shareThingCollection.FindOneAndUpdateAsync(s => s.Id == sId, op);
            return result;
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
