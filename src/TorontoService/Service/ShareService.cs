using TorontoModel.MongodbModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BahamutCommon;
using BahamutService.Model;
using MongoDB.Bson;
using MongoDB.Driver;

namespace TorontoService
{
    public class ShareService : IAccountSessionData
    {
        public AccountSessionData UserSessionData { get; set; }

        public ShareThing PostNewShareThing(ShareThing newShareThing)
        {
            return null;
        }

        public ShareThing[] GetUserShareThings(DateTime newerThanThisTime,DateTime olderThanThisTime,int page,int pageCount)
        {
            IMongoClient client = new MongoClient();
            var result = client.GetDatabase("ShareThing").GetCollection<ShareThing>("kkjk");
            return new ShareThing[]
            {
                new ShareThing()
                {
                     ShareId = 111.ToString(),
                      ShareTime = DateTime.Now,
                        Title = "lalalla",
                         UserId = "147258",
                          ShareContent = new ShareContent()
                          {
                               ContentDocument = "aaaaa"
                          }
                },
                new ShareThing()
                {
                     ShareId = 113.ToString(),
                      ShareTime = DateTime.Now,
                        Title = "lalalla",
                         UserId = "147258",
                          ShareContent = new ShareContent()
                          {
                               ContentDocument = "aaaaa"
                          }
                }
            };
        }

        public ShareThing[] GetShareThingReshares(string shareId)
        {
            return null;
        }

        public Vote[] GetVoteOfShare(string shareId)
        {
            return null;
        }

        public bool VoteShare(string shareId)
        {
            return true;
        }
        
        public bool UnvoteShare(string shareId)
        {
            return true;
        }
   
    }

    public static class ShareServiceExtensions
    {
        public static IBahamutServiceBuilder UseShareService(this IBahamutServiceBuilder builder,params object[] args)
        {
            return builder.UseService<ShareService>(args);
        }

        public static ShareService GetShareService(this IBahamutServiceBuilder builder)
        {
            return builder.GetService<ShareService>();
        }
    }
}
