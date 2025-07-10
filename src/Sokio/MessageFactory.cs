

// MessageFactory.cs
namespace Sokio
{
    /// <summary>
    /// Factory for creating standardized messages and events
    /// </summary>
    public class MessageFactory : IMessageFactory
    {
        private static MessageFactory? _instance;

        /// <summary>
        /// Singleton instance
        /// </summary>
        public static MessageFactory Instance => _instance ??= new MessageFactory();

        private MessageFactory() { }

        /// <summary>
        /// Creates a text message
        /// </summary>
        public Message CreateMessage(string content, string? senderId = null, string? receiverId = null, string? roomId = null)
        {
            return new TextMessage(content, receiverId, senderId)
            {
                RoomId = roomId
            };
        }

        /// <summary>
        /// Creates a binary message
        /// </summary>
        public Message CreateMessage(byte[] data, string fileName, string? senderId = null, string? receiverId = null, string? roomId = null)
        {
            return new BinaryMessage(fileName, data, receiverId, senderId, roomId);
        }

        /// <summary>
        /// Creates an event with existing message
        /// </summary>
        public Event CreateEvent(string eventName, Message message)
        {
            return new Event(eventName, message);
        }

        /// <summary>
        /// Creates an event with text content
        /// </summary>
        public Event CreateEvent(string eventName, string content, string? senderId = null, string? receiverId = null, string? roomId = null)
        {
            var message = CreateMessage(content, senderId, receiverId, roomId);
            return new Event(eventName, message);
        }

        /// <summary>
        /// Creates an event with binary data
        /// </summary>
        public Event CreateEvent(string eventName, byte[] data, string fileName, string? senderId = null, string? receiverId = null, string? roomId = null)
        {
            var message = CreateMessage(data, fileName, senderId, receiverId, roomId);
            return new Event(eventName, message);
        }
    }
}
