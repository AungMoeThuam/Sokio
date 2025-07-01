
using System.Text;
using System.Text.Json;

namespace Sokio
{
    /// <summary>
    /// Represents a text message
    /// </summary>
    public class TextMessage : Message
    {
        private string _content;

        /// <summary>
        /// The text content of the message
        /// </summary>
        public string Content
        {
            get { return _content; }
            set { _content = value; }
        }

        public override string MessageType
        {
            get { return "text"; }
        }

        /// <summary>
        /// Creates a new text message
        /// </summary>
        /// <param name="content">The text content</param>
        /// <param name="receiverId">Target receiver ID (null for broadcast)</param>
        /// <param name="senderId">Sender ID (will be set by server if null)</param>
        public TextMessage(string content, string receiverId = null, string senderId = null)
            : base(senderId, receiverId)
        {
            _content = content;
        }

        public override string ToJson()
        {
            var messageObject = new
            {
                id = Id,
                type = MessageType,
                content = Content,
                senderId = SenderId,
                receiverId = ReceiverId,
                timestamp = Timestamp.ToString("O") // ISO 8601 format
            };

            return JsonSerializer.Serialize(messageObject);
        }

        public override byte[] ToBytes()
        {
            return Encoding.UTF8.GetBytes(ToJson());
        }

        /// <summary>
        /// Deserializes a TextMessage from JSON
        /// </summary>
        public static TextMessage FromJson(string json)
        {
            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                var root = doc.RootElement;
                var content = root.GetProperty("content").GetString();
                var receiverId = root.TryGetProperty("receiverId", out var r) ? r.GetString() : null;
                var senderId = root.TryGetProperty("senderId", out var s) ? s.GetString() : null;

                return new TextMessage(content, receiverId, senderId);
            }
        }
    }
}