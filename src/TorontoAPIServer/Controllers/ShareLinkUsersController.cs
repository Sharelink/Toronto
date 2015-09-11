﻿using System;
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
        public object Get()
        {
            var userService = this.UseSharelinkUserService().GetSharelinkUserService();
            var taskRes = Task.Run(() => { return userService.GetLinkedUsersOfUserId(UserSessionData.UserId); }).Result;
            var result = from u in taskRes select new
            {
                userId = u.Id.ToString(),
                nickName = u.NickName,
                noteName = u.NoteName,
                headIconId = u.HeadIcon,
                personalVideoId = u.PersonalVideo,
                createTime = DateTimeUtil.ToString(u.CreateTime),
                signText = u.SignText
            };
            var users = result.ToArray();
            return users;
        }

        //GET /ShareLinkUsers/{id} : return the user of id
        [HttpGet("{userId}")]
        public SharelinkUser Get(string userId)
        {
            var userService = this.UseSharelinkUserService().GetSharelinkUserService();
            var taskResult = Task.Run(() => 
            {
                return userService.GetMyLinkedUser(UserSessionData.UserId, userId);
            });
            return taskResult.Result;
        }

        //PUT /ShareLinkUsers/NickName : update my user nick profile property
        [HttpPut("NickName")]
        public bool Put(string nickName)
        {
            var userService = this.UseSharelinkUserService().GetSharelinkUserService();
            var taskResult = Task.Run(() =>
            {
                return userService.UpdateUserProfileNickName(UserSessionData.UserId,nickName);
            });
            return taskResult.Result;
            
        }

        //PUT /ShareLinkUsers/SignText : update my user signtext profile property
        [HttpPut("SignText")]
        public bool PutSignText(string signText)
        {
            var userService = this.UseSharelinkUserService().GetSharelinkUserService();
            var taskResult = Task.Run(() =>
            {
                return userService.UpdateUserProfileSignText(UserSessionData.UserId, signText);
            });
            return taskResult.Result;

        }
    }
}