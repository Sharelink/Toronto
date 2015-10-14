using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using TorontoService;
using System.Net;
using BahamutCommon;
using TorontoModel.MongodbModel;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace TorontoAPIServer.Controllers
{
    [Route("api/[controller]")]
    public class UserLinksController : TorontoAPIController
    {

        //GET /UserLinks : get my all userlinks
        [HttpGet]
        public object[] Get()
        {
            var service = this.UseSharelinkUserService().GetSharelinkUserService();
            var taskRes = Task.Run(() => {
                return service.GetUserlinksOfUserId(UserSessionData.UserId);
            }).Result;
            var results =  from ul in taskRes
                   select new
                   {
                       linkId = ul.SlaveUserObjectId.ToString(),
                       slaveUserId = ul.SlaveUserObjectId.ToString(),
                       status = SharelinkUserLink.State.FromJson(ul.StateDocument).LinkState.ToString(),
                       createTime = DateTimeUtil.ToString(ul.CreateTime)
                   };
            return results.ToArray();
        }

        //PUT /UserLinks (myUserId,otherUserId,newState) : update my userlink status with other people
        [HttpPut("NoteName")]
        public void PutUpdateNoteName(string userId, string newNoteName)
        {
            var service = this.UseSharelinkUserService().GetSharelinkUserService();
            var suc = Task.Run(async () =>
            {
                return await service.UpdateLinkedUserNoteName(UserSessionData.UserId, userId, newNoteName);
            }).Result;
            if(!suc)
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }

        //PUT /UserLinks (myUserId,otherUserId,newState) : update my userlink status with other people
        [HttpPut]
        public void Put(string userId, string newState)
        {
            var service = this.UseSharelinkUserService().GetSharelinkUserService();
            var suc = Task.Run(async () =>
            {
                var ns = new SharelinkUserLink.State() { LinkState = int.Parse(newState) };
                return await service.UpdateUserlinkStateWithUser(UserSessionData.UserId, userId,JsonConvert.SerializeObject(ns));
            }).Result;
            if (!suc)
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }

        //POST /UserLinks (myUserId,otherUserId) : add new link with other user
        [HttpPost]
        public bool Post(string otherUserId)
        {
            var service = this.UseSharelinkUserService().GetSharelinkUserService();
            var res = Task.Run(async () =>
            {
                return await service.AskForLink(UserSessionData.UserId, otherUserId);
            }).Result;
            return res != null;
        }
    }
}
