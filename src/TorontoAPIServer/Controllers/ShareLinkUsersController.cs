using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using TorontoService;
using TorontoModel.MongodbModel;
using BahamutCommon;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace TorontoAPIServer.Controllers
{
    [Route("api/[controller]")]
    public class ShareLinkUsersController : TorontoAPIController
    {

        //GET /ShareLinkUsers : if not set the property userIds,return all my connnected users,the 1st is myself; set the userIds will return the user info of userIds
        [HttpGet]
        public async Task<object[]> Get(string userIds)
        {
            
            string[] ids = userIds != null ? userIds.Split('#') : null;
            var userService = this.UseSharelinkUserService().GetSharelinkUserService();
            var fireAccessKeyService = Startup.ServicesProvider.GetFireAccesskeyService();
            var users = await userService.GetLinkedUsersOfUserId(UserSessionData.UserId, ids);
            var result = from u in users
                         select new
                         {
                             userId = u.Id.ToString(),
                             nickName = u.NickName,
                             noteName = u.NoteName,
                             avatarId = fireAccessKeyService.GetAccessKeyUseDefaultConverter(UserSessionData.AccountId, u.Avatar),
                             personalVideoId = fireAccessKeyService.GetAccessKeyUseDefaultConverter(UserSessionData.AccountId, u.PersonalVideo),
                             createTime = DateTimeUtil.ToString(u.CreateTime),
                             motto = u.Motto
                         };
            return result.ToArray();
        }

        //GET /ShareLinkUsers/{id} : return the user of id
        [HttpGet("{userId}")]
        public async Task<object> GetLinkedUser(string userId)
        {
            var userService = this.UseSharelinkUserService().GetSharelinkUserService();
            var u = await userService.GetMyLinkedUser(UserSessionData.UserId, userId);
            return new
            {
                userId = u.Id.ToString(),
                nickName = u.NickName,
                noteName = u.NoteName,
                avatarId = u.Avatar,
                personalVideoId = u.PersonalVideo,
                createTime = DateTimeUtil.ToString(u.CreateTime),
                motto = u.Motto
            };
        }

        //PUT /ShareLinkUsers/NickName : update my user nick profile property
        [HttpPut("NickName")]
        public async Task<bool> Put(string nickName)
        {
            var userService = this.UseSharelinkUserService().GetSharelinkUserService();
            return await userService.UpdateUserProfileNickName(UserSessionData.UserId, nickName);

        }

        //PUT /ShareLinkUsers/motto : update my user motto profile property
        [HttpPut("Motto")]
        public async Task<bool> PutMotto(string motto)
        {
            var userService = this.UseSharelinkUserService().GetSharelinkUserService();
            return await userService.UpdateUserProfileMotto(UserSessionData.UserId, motto);
        }

        //PUT /ShareLinkUsers/Avatar : update my user motto profile property
        [HttpPut("Avatar")]
        public async Task<bool> PutAvatar(string newAvatarId)
        {
            var userService = this.UseSharelinkUserService().GetSharelinkUserService();
            return await userService.UpdateUserAvatar(UserSessionData.UserId, newAvatarId);

        }

        //PUT /ShareLinkUsers/ProfileVideo : update my user motto profile property
        [HttpPut("ProfileVideo")]
        public async Task<bool> PutProfileVideo(string newProfileVideoId)
        {
            var userService = this.UseSharelinkUserService().GetSharelinkUserService();
            return await userService.UpdateUserProfileVideo(UserSessionData.UserId, newProfileVideoId);

        }
    }
}
