using TorontoModel.MongodbModel;
using MongoDB.Driver;
using MongoDB.Bson;
using System;
using System.Linq;
using System.Threading.Tasks;
using BahamutCommon;
using System.Collections.Generic;

namespace TorontoService
{
    public class SharelinkerService
    {
        public IMongoClient Client { get; set; }

        public SharelinkerService(IMongoClient client)
        {
            Client = client;
        }

        public async Task<Sharelinker> CreateNewUser(Sharelinker newUser)
        {
            var collection = Client.GetDatabase("Sharelink").GetCollection<Sharelinker>("Sharelinker");
            await collection.InsertOneAsync(newUser);
            return newUser;
        }

        public async Task<bool> IsUsersLinked(string userIdA,string userIdB)
        {
            var userOIdA = new ObjectId(userIdA);
            var userOIdB = new ObjectId(userIdB);
            var userLinks = await GetUserlinksOfUserId(userIdB);
            var links = from ul in userLinks where ul.SlaveUserObjectId == userOIdA select ul;
            return links.Count() > 0;
        }

        public async Task<IList<Sharelinker>> GetLinkedUsersOfUserId(string userId, IEnumerable<string> ids = null, bool useNoteName = true)
        {
            var links = await GetUserlinksOfUserId(userId);
            var oids = ids == null ? null : from id in ids where !string.IsNullOrWhiteSpace(id) select new ObjectId(id);
            IEnumerable<ObjectId> linkedUserIds = null;
            if (oids == null || oids.Count() == 0)
            {
                linkedUserIds = from l in links select l.SlaveUserObjectId;
            }
            else
            {
                linkedUserIds = from l in links where oids.Contains(l.SlaveUserObjectId) select l.SlaveUserObjectId;
            }
            var filter = new FilterDefinitionBuilder<Sharelinker>().In(s => s.Id, linkedUserIds.ToArray());
            var collection = Client.GetDatabase("Sharelink").GetCollection<Sharelinker>("Sharelinker");
            var linkedUsers =await (await collection.FindAsync(filter)).ToListAsync();
            if (useNoteName)
            {
                var linkMap = links.ToDictionary(lu => lu.SlaveUserObjectId.ToString());
                foreach (var item in linkedUsers)
                {
                    item.NoteName = linkMap[item.Id.ToString()].SlaveUserNoteName;
                }
            }
            return linkedUsers;
        }

        public async Task<IDictionary<string, Sharelinker>> GetUserLinkedUsers(string userId,IEnumerable<string> linkedUserIds = null,bool useNoteName = true)
        {
            var users = await GetLinkedUsersOfUserId(userId,linkedUserIds, useNoteName);
            var userDict = users.ToDictionary(u => u.Id.ToString());
            return userDict;
        }

        public async Task<Sharelinker> GetUserOfUserId(string userId)
        {
            var collection = Client.GetDatabase("Sharelink").GetCollection<Sharelinker>("Sharelinker");
            var uOId = new ObjectId(userId);
            var user = await collection.Find(u => u.Id == uOId).SingleAsync();
            return user;
        }

        public async Task<Sharelinker> GetUserOfAccountId(string accountId)
        {
            try
            {
                var collection = Client.GetDatabase("Sharelink").GetCollection<Sharelinker>("Sharelinker");
                var user = await collection.Find(u => u.AccountId == accountId).SingleAsync();
                return user;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<IList<Sharelinker>> GetUsersOfNickName(string nickName)
        {
            var collection = Client.GetDatabase("Sharelink").GetCollection<Sharelinker>("Sharelinker");
            var users = await collection.FindAsync(usr => usr.NickName == nickName);
            var result = await users.ToListAsync();
            return result;
        }


        public async Task<IList<SharelinkerLink>> GetUserlinksOfUserId(string userId)
        {
            var userOId = new ObjectId(userId);
            var linkCollection = Client.GetDatabase("Sharelink").GetCollection<SharelinkerLink>("SharelinkerLink");
            
            var linkedUsers = linkCollection.Find(l => l.MasterUserObjectId == userOId);
            //TODO:Cache this
            return await linkedUsers.ToListAsync();
        }

        public async Task<Sharelinker> GetSharelinkCenterOfRegion(string region)
        {
            var collection = Client.GetDatabase("Sharelink").GetCollection<Sharelinker>("Sharelinker");
            if (region == "cn")
            {
                return await collection.Find(sl => sl.AccountId == "10000").FirstAsync();
            }
            else
            {
                return await collection.Find(sl => sl.AccountId == "10001").FirstAsync();
            }
        }

        public async Task<SharelinkerLink> CreateNewLinkWithOtherUser(string masterUserId, string otherUserId, SharelinkerLink.State state,string noteName = null)
        {
            var mUOId = new ObjectId(masterUserId);
            var otherUser = await GetUserOfUserId(otherUserId);
            var linkCollection = Client.GetDatabase("Sharelink").GetCollection<SharelinkerLink>("SharelinkerLink");
            var newLink = new SharelinkerLink()
            {
                CreateTime = DateTime.UtcNow,
                StateDocument = state.ToJson(),
                MasterUserObjectId = mUOId,
                SlaveUserObjectId = otherUser.Id,
                SlaveUserNoteName = noteName == null ? otherUser.NickName : noteName
            };

            await linkCollection.InsertOneAsync(newLink);
            var update = new UpdateDefinitionBuilder<Sharelinker>().Push(slu => slu.LinkedUsers, newLink.Id);
            var collection = Client.GetDatabase("Sharelink").GetCollection<Sharelinker>("Sharelinker");
            await collection.UpdateOneAsync(slu => slu.Id == mUOId, update);
            return newLink;
        }

        public async Task<bool> UpdateLinkedUserNoteName(string masterUserId, string otherUserId, string noteName)
        {
            var collection = Client.GetDatabase("Sharelink").GetCollection<SharelinkerLink>("SharelinkerLink");
            var mUOId = new ObjectId(masterUserId);
            var oUId = new ObjectId(otherUserId);
            var update = new UpdateDefinitionBuilder<SharelinkerLink>();

            var result = await collection.UpdateOneAsync(u => u.MasterUserObjectId == mUOId && u.SlaveUserObjectId == oUId
            , update.Set(u => u.SlaveUserNoteName, noteName));
            try
            {
                return result.ModifiedCount > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateUserlinkStateWithUser(string masterUserId, string otherUserId, string state)
        {
            var collection = Client.GetDatabase("Sharelink").GetCollection<SharelinkerLink>("SharelinkerLink");
            var mUOId = new ObjectId(masterUserId);
            var oUId = new ObjectId(otherUserId);
            var update = new UpdateDefinitionBuilder<SharelinkerLink>();

            var result = await collection.UpdateOneAsync(u => u.MasterUserObjectId == mUOId && u.SlaveUserObjectId == oUId
            , update.Set(u => u.StateDocument, state));
            try
            {
                return result.ModifiedCount > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateUserProfileNickName(string userId,string newNick)
        {
            var collection = Client.GetDatabase("Sharelink").GetCollection<Sharelinker>("Sharelinker");
            var userOId = new ObjectId(userId);
            var result = await collection.FindOneAndUpdateAsync(usr => usr.Id == userOId, new UpdateDefinitionBuilder<Sharelinker>().Set(u => u.NickName, newNick));
            return result.NickName == newNick;
        }

        public async Task<bool> UpdateUserProfileMotto(string userId,string newMotto)
        {
            var userOId = new ObjectId(userId);
            var collection = Client.GetDatabase("Sharelink").GetCollection<Sharelinker>("Sharelinker");
            var result = await collection.FindOneAndUpdateAsync(usr => usr.Id == userOId, new UpdateDefinitionBuilder<Sharelinker>().Set(u => u.Motto, newMotto));
            return result.Motto == newMotto;
        }

        public async Task<bool> UpdateUserAvatar(string userId, string newAvatarId)
        {
            var userOId = new ObjectId(userId);
            var collection = Client.GetDatabase("Sharelink").GetCollection<Sharelinker>("Sharelinker");
            var result = await collection.FindOneAndUpdateAsync(usr => usr.Id == userOId, new UpdateDefinitionBuilder<Sharelinker>().Set(u => u.Avatar, newAvatarId));
            return result.Avatar == newAvatarId;
        }

        public async Task<bool> UpdateUserProfileVideo(string userId, string newPersionVideoId)
        {
            var userOId = new ObjectId(userId);
            var collection = Client.GetDatabase("Sharelink").GetCollection<Sharelinker>("Sharelinker");
            var result = await collection.FindOneAndUpdateAsync(usr => usr.Id == userOId, new UpdateDefinitionBuilder<Sharelinker>().Set(u => u.PersonalVideo, newPersionVideoId));
            return result.PersonalVideo == newPersionVideoId;
        }

        public async Task<Sharelinker> GetMyLinkedUser(string myUserId, string userId)
        {
            var myLinkedUsers = await GetLinkedUsersOfUserId(myUserId);
            var userOId = new ObjectId(userId);
            var otherUser = (from ul in myLinkedUsers where ul.Id == userOId select ul).Single();
            return otherUser;
        }

        public async Task<IEnumerable<string>> GetLinkedUserNoteNames(string userId)
        {
            var userLinks = await GetUserlinksOfUserId(userId);
            return from lu in userLinks select lu.SlaveUserNoteName;
        }

    }

    public static class SharelinkerServiceExtensions
    {
        public static IBahamutServiceBuilder UseSharelinkerService(this IBahamutServiceBuilder builder, params object[] args)
        {
            return builder.UseService<SharelinkerService>(args);
        }

        public static SharelinkerService GetSharelinkerService(this IBahamutServiceBuilder builder)
        {
            return builder.GetService<SharelinkerService>();
        }
    }
}
