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
            var service = this.UseSharelinkUserService().GetSharelinkUserService();
            return service.GetAllMyUserTags();
        }

        //PUT /UserTags/{userId} (willAddTagIds,willRemoveTagIds) : update my linked user tag
        [HttpPut("{linkedUserId}")]
        public void Put(string linkedUserId, string willAddTagIds, string willRemoveTagIds)
        {
            string[] willAddTagIdArr = willAddTagIds.Split('#');
            string[] willRemoveTagIdArr = willRemoveTagIds.Split('#');
            var service = this.UseSharelinkUserService().GetSharelinkUserService();
            service.UpdateMyUserTags(linkedUserId, willAddTagIdArr, willRemoveTagIdArr);
        }

    }
}
