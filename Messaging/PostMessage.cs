using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestHubClient.Messaging
{
    public enum PostMessageType
    {
        Created,
        Updated,
        Deleted
    }

    public class PostMessage
    {
        public PostMessageType Type { get; }
        public string PostId { get; }
        public object Payload { get; }

        public PostMessage(PostMessageType type, string postId, object payload = null)
        {
            Type = type;
            PostId = postId;
            Payload = payload;
        }
    }

}
