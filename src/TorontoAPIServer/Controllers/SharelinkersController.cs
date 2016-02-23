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
    public class SharelinkersController : TorontoAPIController
    {

        //GET /Sharelinkers : if not set the property userIds,return all my connnected users,the 1st is myself; set the userIds will return the user info of userIds
        [HttpGet]
        public async Task<object[]> Get(string userIds)
        {
            string[] ids = userIds != null ? userIds.Split('#') : null;
            var userService = this.UseSharelinkerService().GetSharelinkerService();
            var users = await userService.GetLinkedUsersOfUserId(UserSessionData.UserId, ids);
            var result = from u in users
                         select new
                         {
                             userId = u.Id.ToString(),
                             nickName = u.NickName,
                             noteName = u.NoteName,
                             avatarId = u.Avatar,
                             personalVideoId = u.PersonalVideo,
                             createTime = DateTimeUtil.ToString(u.CreateTime),
                             motto = u.Motto,
                             levelScore = u.Point,
                             level = CaculateLevel(u.Point)
                         };
            return result.ToArray();
        }

        [HttpGet("{accountId}")]
        public async Task<object> GetUserByAccountId(string accountId)
        {
            var userService = this.UseSharelinkerService().GetSharelinkerService();
            var user = await userService.GetUserOfAccountId(accountId);
            return new
            {
                userId = user.Id.ToString(),
                nickName = user.NickName,
                avatarId = user.Avatar,
                noteName = user.NoteName,
                motto = user.Motto
            };
        }

        private int CaculateLevel(int point)
        {
            if (point < 49)
            {
                return 1;
            }
            return point;
        }

        //GET /Sharelinkers/{id} : return the user of id
        [HttpGet("{userId}")]
        public async Task<object> GetLinkedUser(string userId)
        {
            var userService = this.UseSharelinkerService().GetSharelinkerService();
            var u = await userService.GetMyLinkedUser(UserSessionData.UserId, userId);
            return new
            {
                userId = u.Id.ToString(),
                nickName = u.NickName,
                noteName = u.NoteName,
                avatarId = u.Avatar,
                personalVideoId = u.PersonalVideo,
                createTime = DateTimeUtil.ToString(u.CreateTime),
                motto = u.Motto,
                levelScore = u.Point,
                level = CaculateLevel(u.Point)
            };
        }

        //PUT /Sharelinkers/NickName : update my user nick profile property
        [HttpPut("NickName")]
        public async Task<bool> Put(string nickName)
        {
            var userService = this.UseSharelinkerService().GetSharelinkerService();
            return await userService.UpdateUserProfileNickName(UserSessionData.UserId, nickName);

        }

        //PUT /Sharelinkers/motto : update my user motto profile property
        [HttpPut("Motto")]
        public async Task<bool> PutMotto(string motto)
        {
            var userService = this.UseSharelinkerService().GetSharelinkerService();
            return await userService.UpdateUserProfileMotto(UserSessionData.UserId, motto);
        }

        //PUT /Sharelinkers/Avatar : update my user motto profile property
        [HttpPut("Avatar")]
        public async Task<bool> PutAvatar(string newAvatarId)
        {
            var userService = this.UseSharelinkerService().GetSharelinkerService();
            return await userService.UpdateUserAvatar(UserSessionData.UserId, newAvatarId);

        }

        //PUT /Sharelinkers/ProfileVideo : update my user motto profile property
        [HttpPut("ProfileVideo")]
        public async Task<bool> PutProfileVideo(string newProfileVideoId)
        {
            var userService = this.UseSharelinkerService().GetSharelinkerService();
            return await userService.UpdateUserProfileVideo(UserSessionData.UserId, newProfileVideoId);

        }
    }
}
