using TorontoModel.MongodbModel;
using MongoDB.Driver;
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

        public async Task<SharelinkUser> CreateNewUser(SharelinkUser newUser)
        {
            var collection = Client.GetDatabase("Sharelink").GetCollection<SharelinkUser>("SharelinkUser");
            await collection.InsertOneAsync(newUser);
            return newUser;
        }

        public SharelinkUser[] GetAllMyLinkedUsers()
        {
            var collection = Client.GetDatabase("Sharelink").GetCollection<SharelinkUser>("SharelinkUser");
            
            return null;
        }

        public SharelinkUser GetUser(string otherUserAccountId)
        {
            return null;
        }

        public SharelinkUser GetUserOfNickName(string nickName)
        {
            return null;
        }

        public bool UpdateMyUserProfileNickName(string newNick)
        {
            return true;
        }

        public bool UpdateMyUserProfileSignText(string newSign)
        {
            return true;
        }

        public SharelinkUserLink[] GetAllMyUserlinks()
        {
            return null;
        }

        public bool UpdateMyUserlinkStateWithUser(string otherUserId,string state)
        {
            return true;
        }

        public SharelinkUserLink CreateNewLinkWithOtherUser(string otherUserId)
        {
            return null;
        }

        public SharelinkTag[] GetMyAllSharelinkTags()
        {
            return null;
        }

        public SharelinkTag CreateNewSharelinkTag(string tagName, string tagColor)
        {
            return null;
        }

        public bool UpdateSharelinkTag(string tagId,string newTagName,string newTagColor)
        {
            return true;
        }

        public bool DeleteSharelinkTag(string tagId)
        {
            return true;
        }

        public UserTag[] GetAllMyUserTags()
        {
            return null;
        }

        public UserTag UpdateMyUserTags(string linkedUserId,string[] willAddTagIds,string[] willRemoveTagIds)
        {
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
