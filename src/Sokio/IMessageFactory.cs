// IMessageFactory.cs
namespace Sokio
{
    /// <summary>
    /// Factory interface for creating messages
    /// </summary>
    public interface IMessageFactory
    {
        Message CreateMessage(string content, string? senderId = null, string? receiverId = null, string? roomId = null);
        Message CreateMessage(byte[] data, string fileName, string? senderId = null, string? receiverId = null, string? roomId = null);
        Event CreateEvent(string eventName, Message message);
        Event CreateEvent(string eventName, string content, string? senderId = null, string? receiverId = null, string? roomId = null);
        Event CreateEvent(string eventName, byte[] data, string fileName, string? senderId = null, string? receiverId = null, string? roomId = null);
    }
}