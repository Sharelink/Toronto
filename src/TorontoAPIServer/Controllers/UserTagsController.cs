﻿using System;
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

        //GET /UserTags/{linkedUserId} : return all usertag of my usertag collection
        [HttpGet("{linkedUserId}")]
        public object Get(string linkedUserId)
        {
            var service = this.UseSharelinkTagService().GetSharelinkTagService();
            var taskResult = Task.Run(() => { return service.GetUserSharelinkTags(linkedUserId); });
            return from t in taskResult.Result
                   select new
                   {
                       tagId = t.Id.ToString(),
                       tagName = t.TagName,
                       tagColor = t.TagColor
                   };
        }

    }
}