using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace TorontoAPIServer.Controllers
{
    [Route("api/[controller]")]
    public class SharelinkThemesController : TorontoAPIController
    {
        // GET: api/values
        [HttpGet("Recommend")]
        public IEnumerable<object> GetRecommendThemes(string region)
        {
            return new string[] { "value1", "value2" };
        }
    }
}
