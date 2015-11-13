using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TorontoModel.MongodbModel
{
    public class SharelinkerConstants
    {
        public const string SharelinkCenterNickName = "#<SharelinkCenter>#";
    }
    public class Sharelinker
    {
        public Sharelinker()
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
        public int Point { get; set; }
        public ObjectId[] LinkedUsers { get; set; }
        public ObjectId[] SharelinkTags { get; set; }
    }

    public class ShareThingConstants
    {
        public const string SHARE_TYPE_FILM = "share:film";

        public const string SHARE_TYPE_MESSAGE_CUSTOM = "message:custom";
        public const string SHARE_TYPE_MESSAGE_TEXT = "message:text";
        public const string SHARE_TYPE_MESSAGE_FOCUS_TAG = "message:focus_tag";
        public const string SHARE_TYPE_MESSAGE_ADD_TAG = "message:add_tag";
    }

    public static class ShareThingShareTypeExtension
    {
        public static bool IsUserShareType(this ShareThing share)
        {
            return string.IsNullOrWhiteSpace(share.ShareType) == false && share.ShareType.StartsWith("share:");
        }
        public static bool IsFilmType(this ShareThing share)
        {
            return ShareThingConstants.SHARE_TYPE_FILM == share.ShareType;
        }

        public static bool IsMessageType(this ShareThing share)
        {
            return string.IsNullOrWhiteSpace(share.ShareType) == false && share.ShareType.StartsWith("message:");
        }

        public static bool IsCustomMessageType(this ShareThing share)
        {
            return ShareThingConstants.SHARE_TYPE_MESSAGE_CUSTOM == share.ShareType;
        }

        public static bool IsAddTagMessageType(this ShareThing share)
        {
            return ShareThingConstants.SHARE_TYPE_MESSAGE_ADD_TAG == share.ShareType;
        }

        public static bool IsFocusTagMessageType(this ShareThing share)
        {
            return ShareThingConstants.SHARE_TYPE_MESSAGE_FOCUS_TAG == share.ShareType;
        }
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
        public string Message{ get; set; }
        public string ShareContent { get; set; }
        public string ShareType { get; set; }
        public Vote[] Votes { get; set; }
        public string[] Tags { get; set; }
        public bool Reshareable { get; set; }
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
        public string ExtraData { get; set; }
    }

    public class LinkMessageConstants
    {
        public const string LINK_MESSAGE_TYPE_ASKING_LINK = "asklink";
        public const string LINK_MESSAGE_TYPE_ACCEPT_LINK = "acceptlink";
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

    public class ChatMessage
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
        public string TagDomain { get; set; }
        public string TagType { get; set; }
        public string Data { get; set; }
        public bool IsFocus { get; set; }
        public bool ShowToLinkers { get; set; }
        public DateTime CreateTime { get; set; }
    }

    public class Vote
    {
        public ObjectId UserId { get; set; }
        public DateTime VoteTime { get; set; }
    }

    public class SharelinkerLink
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
