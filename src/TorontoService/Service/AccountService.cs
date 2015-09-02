using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BahamutService.Model;
using BahamutCommon;

namespace TorontoService
{
    public class AccountService : IAccountSessionData
    {
        public AccountSessionData UserSessionData { get; set; }

        public IMongoClient Client{ get; set; }
        public BahamutDBContext BahamutDBContext { get; set; }
        public AppUserDBContext AppUserDBContext { get; set; }

        public bool BindAccountUser(string accountId,string userId)
        {
            return AppUserDBContext.BindAppUserAccount(accountId, userId);
        }

        public bool ChangePassword(string oldPassword,string newPassword)
        {
            return true;
        }

        public bool ChangeAccountEmail(string newEmail)
        {
            return true;
        }

        public bool ChangeAccountMobile( string newMobile)
        {
            return true;
        }

        public bool ChangeAccountName(string newName)
        {
            return true;
        }

        public bool ChangeAccountBirthday( DateTime newBirth)
        {
            return true;
        }

        public bool ChangeName(string newName)
        {
            return true;
        }

        public Account GetAccount()
        {
            return new Account() { AccountID = long.Parse(UserSessionData.AccountId), AccountName = "test", CreateTime = DateTime.Now, Email = "123@123.com", Mobile = "15800038672", Name = "dfyy" };
        }

    }

    public static class AccountServiceExtensions
    {
        public static IBahamutServiceBuilder UseAccountService(this IBahamutServiceBuilder builder, params object[] args)
        {
            return builder.UseService<AccountService>(args);
        }

        public static AccountService GetAccountService(this IBahamutServiceBuilder builder)
        {
            return builder.GetService<AccountService>();
        }
    }
}
