using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TorontoModel.MongodbModel
{
    public class SharelinkUser
    {
        public ObjectId Id { get; set; }
        public string AccountId { get; set; }
        public string NickName { get; set; }
        public DateTime CreateTime { get; set; }
        public string HeadIcon { get; set; }
        public string PersonalVideo { get; set; }
        public string SignText { get; set; }
        public SharelinkUserLink[] LinkedUsers { get; set; }
        public ObjectId[] SharelinkTags { get; set; }
    }

    public class ShareThing
    {
        public ObjectId Id { get; set; }
        public string UserId { get; set; }
        public string PShareId { get; set; }
        public DateTime ShareTime { get; set; }
        public string Title { get; set; }
        public ShareContent ShareContent { get; set; }
        public Vote[] Votes { get; set; }
        public string[] Tags { get; set; }
    }

    public class ShareThingActiveRecord
    {
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
        public ObjectId SlaveUserObjectId { get; set; }
        public string SlaveUserUserId { get; set; }
        public string StateDocument { get; set; }
    }
}
