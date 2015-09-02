using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TorontoModel.MongodbModel
{
    public class SharelinkUser
    {
        public string UserId { get; set; }
        public string NickName { get; set; }
        public DateTime CreateTime { get; set; }
        public string HeadIcon { get; set; }
        public string PersonalVideo { get; set; }
    }

    public class ShareThing
    {
        public string ShareId { get; set; }
        public string UserId { get; set; }
        public string PShareId { get; set; }
        public DateTime ShareTime { get; set; }
        public string Title { get; set; }
        public ShareContent ShareContent { get; set; }
        public Vote[] Votes { get; set; }
        public string[] TagIds { get; set; }
    }

    public class SharelinkTag
    {
        public string TagId { get; set; }
        public string TagName { get; set; }
        public string TagColor { get; set; }
    }

    public class UserTag
    {
        public string MasterUserId { get; set; }
        public string SlaveUserId { get; set; }
        public string[] TagIds { get; set; }
    }

    public class ShareContent
    {
        public string ContentId { get; set; }
        public ShareType Type { get; set; }
        public string ContentDocument { get; set; }
    }

    public class ShareType
    {
        public string ShareTypeId { get; set; }
        public string ShareTypeDocument { get; set; }

    }

    public class Vote
    {
        public string VoteId { get; set; }
        public string ShareId { get; set; }
        public string UserId { get; set; }
        public DateTime VoteTime { get; set; }
    }

    public class SharelinkUserLink
    {
        public string LinkId { get; set; }
        public string MasterUserId { get; set; }
        public string SlaveUserId { get; set; }
        public string[] UserTagIds { get; set; }
        public string StateDocument { get; set; }
    }
}
