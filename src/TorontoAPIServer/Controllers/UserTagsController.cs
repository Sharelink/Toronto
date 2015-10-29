using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using TorontoService;
using BahamutService;
using BahamutCommon;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace TorontoAPIServer.Controllers
{
    [Route("api/[controller]")]
    public class UserTagsController : TorontoAPIController
    {

        //GET /UserTags/{linkedUserId} : return all usertag of my usertag collection
        [HttpGet("{linkedUserId}")]
        public async Task<object> Get(string linkedUserId)
        {
            var service = this.UseSharelinkTagService().GetSharelinkTagService();
            var taskResult = await service.GetUserFocusTags(linkedUserId);
            var tags = from t in taskResult
                       where t.ShowToLinkers
                       select new
                       {
                           tagId = t.Id.ToString(),
                           tagName = t.TagName,
                           tagColor = t.TagColor,
                           data = t.Data,
                           isFocus = t.IsFocus,
                           type = t.TagType,
                           domain = SharelinkTagConstant.TAG_DOMAIN_CUSTOM,
                           time = DateTimeUtil.ToAccurateDateTimeString(t.CreateTime)
                       };
            return new
            {
                userId = linkedUserId,
                tags = tags
            };
        }

    }
}
