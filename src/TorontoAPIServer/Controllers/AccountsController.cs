using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860
using TorontoService;
using System.Threading.Tasks;
using System;
using BahamutService.Model;
using BahamutCommon;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;

namespace TorontoAPIServer.Controllers
{
    [Route("api/[controller]")]
    public class AccountsController : TorontoAPIController
    {
        // GET /Accounts : return my account information
        [HttpGet]
        public async Task<object> Get()
        {
            HttpClient client = new HttpClient();
            string url = string.Format("{0}/BahamutAccounts?appkey={1}&appToken={2}&accountId={3}&userId={4}", Startup.AuthServerUrl, Startup.Appkey, UserSessionData.AppToken, UserSessionData.AccountId, UserSessionData.UserId);
            var msg = await client.GetAsync(url);
            if (msg.IsSuccessStatusCode)
            {
                var result = await msg.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject(result);
            }
            else
            {
                Response.StatusCode = (int)msg.StatusCode;
                return new { msg = msg.StatusCode == HttpStatusCode.Unauthorized ? "TOKEN_UNAUTHORIZED" : "DATA_ERROR" };
            }
        }

        // PUT /Accounts/Name (name) : update my account name properties
        [HttpPut("Name")]
        public async Task PutName(string name)
        {
            HttpClient client = new HttpClient();
            string url = string.Format("{0}/BahamutAccounts/Name", Startup.AuthServerUrl);
            var kvList = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("appkey", Startup.Appkey),
                new KeyValuePair<string, string>("appToken", UserSessionData.AppToken),
                new KeyValuePair<string, string>("accountId", UserSessionData.AccountId),
                new KeyValuePair<string, string>("userId", UserSessionData.UserId),
                new KeyValuePair<string, string>("name", name)
            };
            var result = await client.PutAsync(url, new FormUrlEncodedContent(kvList));
            Response.StatusCode = (int)result.StatusCode;
        }

        // PUT /Accounts/BirthDate (birthdate) : update my account birth properties
        [HttpPut("BirthDate")]
        public async Task PutBirthDate(string birthdate)
        {
            HttpClient client = new HttpClient();
            string url = string.Format("{0}/BahamutAccounts/BirthDate", Startup.AuthServerUrl);
            var kvList = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("appkey", Startup.Appkey),
                new KeyValuePair<string, string>("appToken", UserSessionData.AppToken),
                new KeyValuePair<string, string>("accountId", UserSessionData.AccountId),
                new KeyValuePair<string, string>("userId", UserSessionData.UserId),
                new KeyValuePair<string, string>("birthdate", birthdate)
            };
            var result = await client.PutAsync(url, new FormUrlEncodedContent(kvList));
            Response.StatusCode = (int)result.StatusCode;
        }

        [HttpPut("Password")]
        public async Task<bool> ChangePassword(string oldPassword, string newPassword)
        {
            HttpClient client = new HttpClient();
            string url = string.Format("{0}/BahamutAccounts/Password", Startup.AuthServerUrl);
            var kvList = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("appkey", Startup.Appkey),
                new KeyValuePair<string, string>("appToken", UserSessionData.AppToken),
                new KeyValuePair<string, string>("accountId", UserSessionData.AccountId),
                new KeyValuePair<string, string>("userId", UserSessionData.UserId),
                new KeyValuePair<string, string>("originPassword", oldPassword),
                new KeyValuePair<string, string>("newPassword", newPassword)
            };
            var result = await client.PutAsync(url, new FormUrlEncodedContent(kvList));
            return result.IsSuccessStatusCode;
        }
    }
}
