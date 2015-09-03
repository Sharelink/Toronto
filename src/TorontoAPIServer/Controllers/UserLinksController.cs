using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using TorontoService;
using System.Net;

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
            return service.GetAllMyUserlinks();
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
            service.CreateNewLinkWithOtherUser(otherUserId);
        }
    }
}
