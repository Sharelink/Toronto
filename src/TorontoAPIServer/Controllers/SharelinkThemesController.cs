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
        [HttpGet("HotThemes")]
        public IEnumerable<object> GetRecommendThemes(string region)
        {
            var count = Startup.HotThemes.Count - 100;
            if(count < 0)
            {
                count = 0;
            }
            var hotThemes = Startup.HotThemes.Skip(count);
            yield return new
            {
                themes = hotThemes
            };
        }
    }
}
