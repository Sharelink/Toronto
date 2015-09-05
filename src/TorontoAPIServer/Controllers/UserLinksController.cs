using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using TorontoService;
using System.Net;
using BahamutCommon;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace TorontoAPIServer.Controllers
{
    [Route("api/[controller]")]
    public class UserLinksController : TorontoAPIController
    {

        //GET /UserLinks : get my all userlinks
        [HttpGet]
        public object Get()
        {
            var service = this.UseSharelinkUserService().GetSharelinkUserService();
            var taskRes = Task.Run(() => {
                return service.GetAllMyUserlinks();
            });
            return from ul in taskRes.Result
                   select new
                   {
                       linkId = ul.SlaveUserUserId,
                       slaveUserId = ul.SlaveUserUserId,
                       status = ul.StateDocument,
                       createTime = DateTimeUtil.ToString(ul.CreateTime)
                   };

        }

        //PUT /UserLinks (myUserId,otherUserId,newState) : update my userlink status with other people
        [HttpPut]
        public async void Put(string otherUserId, string newState)
        {
            var service = this.UseSharelinkUserService().GetSharelinkUserService();
            if (!await service.UpdateMyUserlinkStateWithUser(otherUserId, newState))
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }

        //POST /UserLinks (myUserId,otherUserId) : add new link with other user
        [HttpPost]
        public void Post(string otherUserId)
        {
            var service = this.UseSharelinkUserService().GetSharelinkUserService();
            //service.CreateNewLinkWithOtherUser(otherUserId);
        }
    }
}
