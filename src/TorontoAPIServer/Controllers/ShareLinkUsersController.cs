using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using TorontoService;
using TorontoModel.MongodbModel;

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
            return userService.GetAllMyLinkedUsers();
        }

        //GET /ShareLinkUsers/{id} : return the user of id
        [HttpGet("{id}")]
        public SharelinkUser Get(string id)
        {
            var userService = this.UseSharelinkUserService().GetSharelinkUserService();
            return userService.GetAllMyLinkedUsers().Where(u => u.UserId == id).First();
        }

        //PUT /ShareLinkUsers (nickName,signText) : update my user profile property
        [HttpPut]
        public void Put(string nickName, string signText)
        {
            var userService = this.UseSharelinkUserService().GetSharelinkUserService();
            if (nickName != null)
            {
                userService.UpdateMyUserProfileNickName( nickName);
            }
            if(signText != null)
            {
                userService.UpdateMyUserProfileSignText( signText);
            }
        }
    }
}
