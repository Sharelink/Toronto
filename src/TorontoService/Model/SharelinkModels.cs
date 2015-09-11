using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TorontoModel.MongodbModel
{
    public class SharelinkUser
    {
        public SharelinkUser()
        {
            LinkedUsers = new SharelinkUserLink[0];
            SharelinkTags = new ObjectId[0];
        }
        public ObjectId Id { get; set; }
        public string AccountId { get; set; }
        public string NickName { get; set; }
        public string NoteName { get; set; }
        public DateTime CreateTime { get; set; }
        public string HeadIcon { get; set; }
        public string PersonalVideo { get; set; }
        public string SignText { get; set; }
        public SharelinkUserLink[] LinkedUsers { get; set; }
        public ObjectId[] SharelinkTags { get; set; }
    }

    public class ShareThing
    {
        public ShareThing()
        {
            Votes = new Vote[0];
            Tags = new string[0];
        }

        public ObjectId Id { get; set; }
        public ObjectId UserId { get; set; }
        public ObjectId PShareId { get; set; }
        public DateTime ShareTime { get; set; }
        public string Title { get; set; }
        public ShareContent ShareContent { get; set; }
        public ObjectId ShareType { get; set; }
        public Vote[] Votes { get; set; }
        public string[] Tags { get; set; }
        public DateTime LastActiveTime { get; set; }
    }

    public class ShareThingActiveRecord
    {
        public ObjectId Id { get; set; }
        public ObjectId ShareId { get; set; }
        public ObjectId ShareUserId { get; set; }
        public ObjectId OperateUserId { get; set; }
        public DateTime Time { get; set; }
        public string Operate { get; set; }
    }

    public class SharelinkTag
    {
        public ObjectId Id { get; set; }
        public ObjectId UserId { set; get; }
        public string TagName { get; set; }
        public string TagColor { get; set; }
    }

    public class ShareContent
    {
        public ObjectId ContentId { get; set; }
        public ShareType Type { get; set; }
        public string ContentDocument { get; set; }
    }

    public class ShareType
    {
        public ObjectId Id { get; set; }
        public string ShareTypeDocument { get; set; }

    }

    public class Vote
    {
        public ObjectId UserId { get; set; }
        public DateTime VoteTime { get; set; }
    }

    public class SharelinkUserLink
    {
        public enum LinkState
        {
            //Master
            Asking = 1,
            //Common
            Linked = 2,
            Removed = 3,
            //Slave
            WaitToAccept = 4
        }

        public class State
        {
            public int LinkState { get; set; }

            public static State FromJson(string json)
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<State>(json);
            }
        }
        public ObjectId SlaveUserObjectId { get; set; }
        public string SlaveUserUserId { get { return SlaveUserObjectId.ToString(); } }
        public string SlaveUserNoteName { get; set; } //default = nickname
        public string StateDocument { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
