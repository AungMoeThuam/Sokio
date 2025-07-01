
namespace Sokio
{
    /// <summary>
    /// Interface for WebSocket connections
    /// </summary>

    public interface IWebSocket
    {
        string Id { get; }
        bool IsConnected { get; }
        event OnMessageHandler OnMessage;
        event OnCloseHandler OnClose;
        event OnErrorHandler OnError;
        Task SendAsync(Message message);
        Task SendAsync(string message);
        Task SendAsync(byte[] data);
        Task CloseAsync();
    }
}