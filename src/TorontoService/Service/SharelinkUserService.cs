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

        public async Task<string> GetUserIdOfAccountId(string accountId)
        {
            var collection = Client.GetDatabase("Sharelink").GetCollection<SharelinkUser>("SharelinkUser");
            var me = await collection.Find(u => u.AccountId == accountId).FirstAsync();
            if(me != null)
            {
                return me.Id.ToString();
            }
            return null;
        }

        public async Task<SharelinkUser> CreateNewUser(SharelinkUser newUser)
        {
            var collection = Client.GetDatabase("Sharelink").GetCollection<SharelinkUser>("SharelinkUser");
            await collection.InsertOneAsync(newUser);
            return newUser;
        }

        public async Task<IList<SharelinkUser>> GetAllMyLinkedUsers()
        {
            var collection = Client.GetDatabase("Sharelink").GetCollection<SharelinkUser>("SharelinkUser");
            var me = await collection.Find(u => u.Id == new ObjectId(UserSessionData.UserId)).FirstAsync();
            var linkedUserIds = from userLink in me.LinkedUsers select userLink.SlaveUserObjectId;
            var linkedUsers = await (await collection.FindAsync(usr => linkedUserIds.Contains(usr.Id))).ToListAsync();
            linkedUsers.Insert(0, me);
            return linkedUsers;
        }

        public async Task<SharelinkUser> GetMyLinkedUser(string userId)
        {
            var collection = Client.GetDatabase("Sharelink").GetCollection<SharelinkUser>("SharelinkUser");
            var me = await collection.Find(u => u.Id == new ObjectId(UserSessionData.UserId)).FirstAsync();
            var otherUser = (from ul in me.LinkedUsers where ul.SlaveUserUserId == userId select ul).First();
            if(otherUser != null)
            {
                return await collection.Find(u => u.Id == otherUser.SlaveUserObjectId).FirstAsync();
            }
            return null;
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

        public async Task<SharelinkUserLink[]> GetAllMyUserlinks()
        {
            var collection = Client.GetDatabase("Sharelink").GetCollection<SharelinkUser>("SharelinkUser");
            var me = await collection.Find(u => u.Id == new ObjectId(UserSessionData.UserId)).FirstAsync();
            //TODO:Cache this
            return me.LinkedUsers;
        }

        public async Task<IEnumerable<ObjectId>> GetMyLinkedUserIds()
        {
            var userLinks = await GetAllMyUserlinks();
            //TODO:Cache this
            return from lu in userLinks select lu.SlaveUserObjectId;
        }

        public async Task<bool> UpdateMyUserlinkStateWithUser(string otherUserId, string state)
        {
            var collection = Client.GetDatabase("Sharelink").GetCollection<SharelinkUser>("SharelinkUser");
            var result = await collection.UpdateOneAsync(u => u.Id == new ObjectId(UserSessionData.UserId), new UpdateDefinitionBuilder<SharelinkUser>().
                Set(usr => (from ul in usr.LinkedUsers where ul.SlaveUserUserId == otherUserId select ul).First().StateDocument, state));
            return result.ModifiedCount > 0;
        }

        public SharelinkUserLink CreateNewLinkWithOtherUser(string otherUserId)
        {
            var collection = Client.GetDatabase("Sharelink").GetCollection<SharelinkUser>("SharelinkUser");
            return null;
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
