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
    public class SharelinkUserService
    {
        public IMongoClient Client { get; set; }

        public SharelinkUserService(IMongoClient client)
        {
            Client = client;
        }

        public async Task<SharelinkUser> CreateNewUser(SharelinkUser newUser)
        {
            var collection = Client.GetDatabase("Sharelink").GetCollection<SharelinkUser>("SharelinkUser");
            await collection.InsertOneAsync(newUser);
            return newUser;
        }

        public async Task<IList<SharelinkUser>> GetLinkedUsersOfUserId(string userId, bool useNoteName = true)
        {
            var collection = Client.GetDatabase("Sharelink").GetCollection<SharelinkUser>("SharelinkUser");
            var links = await GetUserlinksOfUserId(userId);
            var linkedUserIds = from u in links select u.SlaveUserObjectId;
            var linkedUsers = await (await collection.FindAsync(usr => linkedUserIds.Contains(usr.Id))).ToListAsync();
            if (useNoteName)
            {
                var linkMap = links.ToDictionary(lu => lu.SlaveUserObjectId.ToString());
                for (int i = 0; i < linkedUsers.Count; i++)
                {
                    linkedUsers[i].NoteName = linkMap[linkedUsers[i].Id.ToString()].SlaveUserNoteName;
                }
            }
            return linkedUsers;
        }

        public async Task<IDictionary<string, SharelinkUser>> GetUserLinkedUsers(string userId,IEnumerable<string> linkedUserIds,bool useNoteName = true)
        {
            var users = await GetLinkedUsersOfUserId(userId, useNoteName);
            
            var userDict = users.ToDictionary(u => u.Id.ToString());
            var result = new Dictionary<string, SharelinkUser>();
            foreach (var item in linkedUserIds)
            {
                var usr = userDict[item];
                if(!result.ContainsKey(item))
                {
                    result.Add(item, usr);
                }
            }
            return result;
        }

        public async Task<SharelinkUser> GetUserOfUserId(string userId)
        {
            var collection = Client.GetDatabase("Sharelink").GetCollection<SharelinkUser>("SharelinkUser");
            var uOId = new ObjectId(userId);
            var user = await collection.Find(u => u.Id == uOId).SingleAsync();
            return user;
        }

        public async Task<SharelinkUser> GetUserOfAccountId(string accountId)
        {
            try
            {
                var collection = Client.GetDatabase("Sharelink").GetCollection<SharelinkUser>("SharelinkUser");
                var user = await collection.Find(u => u.AccountId == accountId).SingleAsync();
                return user;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<IList<SharelinkUser>> GetUsersOfNickName(string nickName)
        {
            var collection = Client.GetDatabase("Sharelink").GetCollection<SharelinkUser>("SharelinkUser");
            var users = await collection.FindAsync(usr => usr.NickName == nickName);
            var result = await users.ToListAsync();
            return result;
        }


        public async Task<IList<SharelinkUserLink>> GetUserlinksOfUserId(string userId)
        {
            var user = await GetUserOfUserId(userId);
            var linkCollection = Client.GetDatabase("Sharelink").GetCollection<SharelinkUserLink>("SharelinkUserLink");
            var linkedUsers = linkCollection.Find(usr => user.LinkedUsers.Contains(usr.Id));
            //TODO:Cache this
            return await linkedUsers.ToListAsync();
        }

        public SharelinkUserLink AskForLink(string masterUserId, string otherUserId)
        {
            var state = new SharelinkUserLink.State();
            if (masterUserId == otherUserId)
            {
                state.LinkState = (int)SharelinkUserLink.LinkState.Linked;
            }
            else
            {
                state.LinkState = (int)SharelinkUserLink.LinkState.Asking;
                CreateNewLinkWithOtherUser(otherUserId, masterUserId, new SharelinkUserLink.State()
                {
                    LinkState = (int)SharelinkUserLink.LinkState.WaitToAccept
                });
            }
            return CreateNewLinkWithOtherUser(masterUserId, otherUserId, state);
        }

        public SharelinkUserLink CreateNewLinkWithOtherUser(string masterUserId,string otherUserId,SharelinkUserLink.State state)
        {
            var mUOId = new ObjectId(masterUserId);
            var otherUser = Task.Run(() => { return GetUserOfUserId(otherUserId); }).Result;
            var linkCollection = Client.GetDatabase("Sharelink").GetCollection<SharelinkUserLink>("SharelinkUserLink");
            var newLink = new SharelinkUserLink()
            {
                CreateTime = DateTime.Now,
                StateDocument = state.ToJson(),
                MasterUserObjectId = mUOId,
                SlaveUserObjectId = otherUser.Id,
                SlaveUserNoteName = otherUser.NickName
            };

            Task.Run(async () =>
            {
                await linkCollection.InsertOneAsync(newLink);
            });
            
            var update = new UpdateDefinitionBuilder<SharelinkUser>().Push(slu => slu.LinkedUsers, newLink.Id);
            var collection = Client.GetDatabase("Sharelink").GetCollection<SharelinkUser>("SharelinkUser");
            collection.UpdateOneAsync(slu => slu.Id == mUOId, update);
            return newLink;
        }

        public async Task<bool> UpdateLinkedUserNoteName(string masterUserId, string otherUserId, string noteName)
        {
            var collection = Client.GetDatabase("Sharelink").GetCollection<SharelinkUserLink>("SharelinkUserLink");
            var mUOId = new ObjectId(masterUserId);
            var oUId = new ObjectId(otherUserId);
            var update = new UpdateDefinitionBuilder<SharelinkUserLink>();

            var result = await collection.UpdateOneAsync(u => u.MasterUserObjectId == mUOId && u.SlaveUserObjectId == oUId
            , update.Set(u => u.SlaveUserNoteName, noteName));
            return result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateUserlinkStateWithUser(string masterUserId, string otherUserId, string state)
        {
            var collection = Client.GetDatabase("Sharelink").GetCollection<SharelinkUserLink>("SharelinkUserLink");
            var mUOId = new ObjectId(masterUserId);
            var oUId = new ObjectId(otherUserId);
            var update = new UpdateDefinitionBuilder<SharelinkUserLink>();

            var result = await collection.UpdateOneAsync(u => u.MasterUserObjectId == mUOId && u.SlaveUserObjectId == oUId
            , update.Set(u => u.StateDocument, state));
            return result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateUserProfileNickName(string userId,string newNick)
        {
            var collection = Client.GetDatabase("Sharelink").GetCollection<SharelinkUser>("SharelinkUser");
            var userOId = new ObjectId(userId);
            var result = await collection.FindOneAndUpdateAsync(usr => usr.Id == userOId, new UpdateDefinitionBuilder<SharelinkUser>().Set(u => u.NickName, newNick));
            return result.NickName == newNick;
        }

        public async Task<bool> UpdateUserProfileSignText(string userId,string newSign)
        {
            var userOId = new ObjectId(userId);
            var collection = Client.GetDatabase("Sharelink").GetCollection<SharelinkUser>("SharelinkUser");
            var result = await collection.FindOneAndUpdateAsync(usr => usr.Id == userOId, new UpdateDefinitionBuilder<SharelinkUser>().Set(u => u.SignText, newSign));
            return result.SignText == newSign;
        }


        public async Task<SharelinkUser> GetMyLinkedUser(string myUserId, string userId)
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

    public static class SharelinkUserServiceExtensions
    {
        public static IBahamutServiceBuilder UseSharelinkUserService(this IBahamutServiceBuilder builder, params object[] args)
        {
            return builder.UseService<SharelinkUserService>(args);
        }

        public static SharelinkUserService GetSharelinkUserService(this IBahamutServiceBuilder builder)
        {
            return builder.GetService<SharelinkUserService>();
        }
    }
}
