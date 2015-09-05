using TorontoModel.MongodbModel;
using MongoDB.Driver;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BahamutService.Model;
using BahamutCommon;
namespace TorontoService
{
    public class SharelinkUserService : IAccountSessionData
    {
        public IMongoClient Client { get; set; }

        public AccountSessionData UserSessionData { get; set; }

        public SharelinkUserService(IMongoClient client)
        {
            Client = client;
        }

        public async Task<string> GetUserIdOfAccountId(string accountId)
        {
            try
            {
                var collection = Client.GetDatabase("Sharelink").GetCollection<SharelinkUser>("SharelinkUser");
                var lst = collection.Find(u => u.AccountId == accountId);
                var me = await lst.SingleAsync();
                return me.Id.ToString();
                
            }
            catch (Exception)
            {
                throw new NullReferenceException("No User Of accountId");
            }
            
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
            var links = from u in (await GetUserlinksOfUserId(userId)) select u;
            var linkedUserIds = from u in links select u.SlaveUserObjectId;
            var linkedUsers = await (await collection.FindAsync(usr => linkedUserIds.Contains(usr.Id))).ToListAsync();
            if (useNoteName)
            {
                var linkMap = links.ToDictionary(lu => lu.ToString());
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
                result.Add(item, usr);
            }
            return result;
        }

        public SharelinkUser GetUser(string otherUserAccountId)
        {
            //TODO
            return null;
        }

        public async Task<IList<SharelinkUser>> GetUsersOfNickName(string nickName)
        {
            var collection = Client.GetDatabase("Sharelink").GetCollection<SharelinkUser>("SharelinkUser");
            var users = await collection.FindAsync(usr => usr.NickName == nickName);
            var result = await users.ToListAsync();
            return result;
        }


        public async Task<IList<SharelinkUserLink>> GetUserlinksOfUserId(string UserId)
        {
            var uOId = new ObjectId(UserId);
            var collection = Client.GetDatabase("Sharelink").GetCollection<SharelinkUser>("SharelinkUser");
            var me = await collection.Find(u => u.Id == uOId).SingleAsync();
            //TODO:Cache this
            return me.LinkedUsers;
        }

        public SharelinkUserLink CreateNewLinkWithOtherUser(string masterUserId,string otherUserId,string status)
        {
            var collection = Client.GetDatabase("Sharelink").GetCollection<SharelinkUserLink>("SharelinkUserLink");
            //TODO:
            var newLink = new SharelinkUserLink()
            {
                CreateTime = DateTime.Now,

            };
            return newLink;
        }

        //////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IList<SharelinkUserLink>> GetAllMyUserlinks()
        {
            return await GetUserlinksOfUserId(UserSessionData.UserId);
        }

        public async Task<SharelinkUser> GetMyLinkedUser(string userId)
        {
            var myLinkedUsers = await GetLinkedUsersOfUserId(userId);
            var userOId = new ObjectId(userId);
            var otherUser = (from ul in myLinkedUsers where ul.Id == userOId select ul).First();
            return otherUser;
        }

        public async Task<IList<SharelinkUser>> GetAllMyLinkedUsers()
        {
            return await GetLinkedUsersOfUserId(UserSessionData.UserId);
        }

        public async Task<IDictionary<string, SharelinkUser>> GetMyLinkedUsers(IEnumerable<string> linkedUserIds, bool useNoteNameReplaceNickName = false)
        {
            return await GetUserLinkedUsers(UserSessionData.UserId, linkedUserIds, useNoteNameReplaceNickName);
        }

        public async Task<bool> UpdateMyUserProfileNickName(string newNick)
        {
            var collection = Client.GetDatabase("Sharelink").GetCollection<SharelinkUser>("SharelinkUser");
            var result = await collection.FindOneAndUpdateAsync(usr => usr.Id == new ObjectId(UserSessionData.UserId), new UpdateDefinitionBuilder<SharelinkUser>().Set(u => u.NickName, newNick));
            return result.NickName == newNick;
        }

        public async Task<bool> UpdateMyUserProfileSignText(string newSign)
        {
            var collection = Client.GetDatabase("Sharelink").GetCollection<SharelinkUser>("SharelinkUser");
            var result = await collection.FindOneAndUpdateAsync(usr => usr.Id == new ObjectId(UserSessionData.UserId), new UpdateDefinitionBuilder<SharelinkUser>().Set(u => u.SignText, newSign));
            return result.SignText == newSign;
        }

        public async Task<IEnumerable<ObjectId>> GetMyLinkedUserIds()
        {
            var userLinks = await GetUserlinksOfUserId(UserSessionData.UserId);
            return from lu in userLinks select lu.SlaveUserObjectId;
        }

        public async Task<IEnumerable<string>> GetMyLinkedUserNoteNames()
        {
            var userLinks = await GetUserlinksOfUserId(UserSessionData.UserId);
            return from lu in userLinks select lu.SlaveUserNoteName;
        }

        public async Task<bool> UpdateMyUserlinkStateWithUser(string otherUserId, string state)
        {
            var collection = Client.GetDatabase("Sharelink").GetCollection<SharelinkUser>("SharelinkUser");
            var result = await collection.UpdateOneAsync(u => u.Id == new ObjectId(UserSessionData.UserId), new UpdateDefinitionBuilder<SharelinkUser>().
                Set(usr => (from ul in usr.LinkedUsers where ul.SlaveUserUserId == otherUserId select ul).First().StateDocument, state));
            return result.ModifiedCount > 0;
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
