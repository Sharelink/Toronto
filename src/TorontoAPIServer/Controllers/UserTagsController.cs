using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using TorontoService;
using BahamutService;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace TorontoAPIServer.Controllers
{
    [Route("api/[controller]")]
    public class UserTagsController : TorontoAPIController
    {

        //GET /UserTags : return all usertag of my usertag collection
        [HttpGet]
        public object Get()
        {
            var service = this.UseSharelinkTagService().GetSharelinkTagService();
            return service.GetMyAllSharelinkTags();
        }

        //PUT /UserTags/{userId} : Get linked user's all tags from server
        [HttpPut("api/[controller]/{linkedUserId}")]
        public void Put(string linkedUserId)
        {
            var service = this.UseSharelinkTagService().GetSharelinkTagService();

        }

    }
}
