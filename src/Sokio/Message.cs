using System.Text.Json.Serialization;

namespace Sokio
{
    /// <summary>
    /// Abstract base class for all message types in the WebSocket library
    /// </summary>
    public abstract class Message
    {
        private string _id;
        private DateTime _timestamp;
        private string _senderId;
        private string _receiverId;

        /// <summary>
        /// Unique identifier for the message
        /// </summary>
        public string Id { get { return _id; } }

        /// <summary>
        /// When the message was created
        /// </summary>
        public DateTime Timestamp { get { return _timestamp; } }

        /// <summary>
        /// ID of the sender WebSocket connection
        /// </summary>
        public string SenderId
        {
            get { return _senderId; }
            set { _senderId = value; }
        }

        /// <summary>
        /// ID of the target receiver (null for broadcast)
        /// </summary>
        public string ReceiverId
        {
            get { return _receiverId; }
            set { _receiverId = value; }
        }

        /// <summary>
        /// The type of message (text or binary)
        /// </summary>
        [JsonPropertyName("type")]
        public abstract string MessageType { get; }

        /// <summary>
        /// Serializes the message to JSON string for transmission
        /// </summary>
        public abstract string ToJson();

        /// <summary>
        /// Converts the message to bytes for WebSocket transmission
        /// </summary>
        public abstract byte[] ToBytes();

        protected Message(string senderId = null, string receiverId = null)
        {
            _id = Guid.NewGuid().ToString();
            _timestamp = DateTime.UtcNow;
            _senderId = senderId;
            _receiverId = receiverId;
        }
    }
}