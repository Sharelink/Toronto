﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace TorontoWebsite.Controllers
{
    public class ExeSharelinkController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index(string cmd)
        {
            string ua = Request.Headers["User-Agent"];
            ua = ua.ToLower();
            
            if (ua.Contains("ios") || ua.Contains("iphone") || ua.Contains("ipad") || ua.Contains("ipod"))
            {
                return Redirect("~/IOSExeSharelink.html?cmd=" + cmd);
            }
            else if (ua.Contains("android"))
            {
                return Redirect("~/AndroidExeSharelink.html?cmd=" + cmd);
            }
            else if(ua.Contains("windows mobile") || ua.Contains("windows phone"))
            {
                return Redirect("~/WMExeSharelink.html?cmd=" + cmd);
            }
            else
            {
                return Redirect("~/");
            }
        }
    }
}
