﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using TorontoService;
using System.Net;
using BahamutService;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace TorontoAPIServer.Controllers
{

    [Route("[controller]")]
    public class TokensController : TorontoAPIController
    {
        [HttpGet]
        public object Get(string appkey, string accountId, string accessToken)
        {
            var tokenService = Startup.ServicesProvider.GetTokenService();
            var userService = this.UseSharelinkUserService().GetSharelinkUserService();
            var result = Task.Run(() => {
                return userService.GetUserOfAccountId(accountId);
            });
            try
            {
                var userId = result.Result.Id.ToString();
                var tokenResult = tokenService.ValidateAccessToken(appkey, accountId, accessToken, userId);
                if (tokenResult.Succeed)
                {
                    return new
                    {
                        AppToken = tokenResult.UserSessionData.AppToken,
                        UserId = userId,
                        APIServer = Startup.APIUrl,
                        FileAPIServer = "http://192.168.1.67:8089/Files"
                    };
                }
                else
                {
                    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return tokenResult.Message;
                }
            }catch(NullReferenceException)
            {
                return new
                {
                    RegistAPIServer = Startup.Server
                };
            }
            catch (Exception)
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return "Server Error";
            }
            
        }

        // DELETE api/values/5
        [HttpDelete]
        public bool Delete(string appkey, string userId, string appToken)
        {
            var tokenService = Startup.ServicesProvider.GetTokenService();
            return tokenService.ReleaseAppToken(appkey, userId, appToken);
        }
    }
}
