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
            LinkedUsers = new ObjectId[0];
            SharelinkTags = new ObjectId[0];
        }
        public ObjectId Id { get; set; }
        public string AccountId { get; set; }
        public string NickName { get; set; }
        public string NoteName { get; set; }
        public DateTime CreateTime { get; set; }
        public string Avatar { get; set; }
        public string PersonalVideo { get; set; }
        public string Motto { get; set; }
        public ObjectId[] LinkedUsers { get; set; }
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
        public string ShareContent { get; set; }
        public string ShareType { get; set; }
        public Vote[] Votes { get; set; }
        public string[] Tags { get; set; }
    }

    public class ShareThingUpdatedMessage
    {
        public ObjectId ShareId { get; set; }
        public DateTime Time { get; set; }
    }

    public class ShareThingMail
    {
        public ObjectId Id { get; set; }
        public ObjectId ShareId { get; set; }
        public ObjectId ToSharelinker { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public DateTime Time { get; set; }
    }

    public class LinkMessage
    {
        public string Id { get; set; }
        public string SharelinkerId { get; set; }
        public string SharelinkerNick { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
        public string Avatar { get; set; }
        public DateTime Time { get; set; }
    }

    public enum MessageType
    {
        Text = 1,
        Picture = 2,
        Sound = 3,
        Film = 4,
        Html = 5,
        Other = 6
    }

    public class SharelinkerLastGetMessageInfo
    {
        public ObjectId UserId { get; set; }
        public DateTime LastTime { get; set; }
        public DateTime NewMessageTime { get; set; }
    }

    public class ShareChat
    {
        public ObjectId Id { get; set; }
        public ObjectId ShareId { get; set; }
        public string ChatId { get; set; }
        public ObjectId[] UserIds { get; set; }
        public DateTime Time { get; set; }
    }

    public class SharelinkMessage
    {
        public ObjectId Id { get; set; }
        public ObjectId SenderId { get; set; }
        public string ChatId { get; set; }
        public string ShareId { get; set; }
        public string Message { get; set; }
        public string MessageData { get; set; }
        public string MessageType { get; set; }
        public DateTime Time { get; set; }
    }

    public class SharelinkTag
    {
        public ObjectId Id { get; set; }
        public ObjectId UserId { set; get; }
        public string TagName { get; set; }
        public string TagColor { get; set; }
        public string Data { get; set; }
        public bool IsFocus { get; set; }
        public bool ShowToLinkers { get; set; }
        public DateTime LastActiveTime { get; set; }
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
            WaitToAccept = 4,
            Rejected = 5
        }

        public class State
        {
            public int LinkState { get; set; }

            public static State FromJson(string json)
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<State>(json);
            }
        }
        public ObjectId Id { get; set; }
        public ObjectId MasterUserObjectId { get; set; }
        public ObjectId SlaveUserObjectId { get; set; }
        public string SlaveUserNoteName { get; set; } //default = nickname
        public string StateDocument { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
