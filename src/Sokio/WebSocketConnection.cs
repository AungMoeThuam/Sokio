
using System.Net.Sockets;
using System.Text;

namespace Sokio
{
    /// <summary>
    /// Server-side WebSocket connection
    /// </summary>
    public class WebSocketConnection : BaseSocket
    {

        public WebSocketConnection(TcpClient client) : base()
        {
            _client = client;
            _stream = client.GetStream();
        }

        internal async Task AcceptAsync(WebSocketHandshake handshake)
        {
            // Read handshake
            byte[] buffer = new byte[1024];
            int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
            string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            var headers = handshake.ParseHttpRequest(request);

            if (headers.ContainsKey("Sec-WebSocket-Key"))
            {
                string response = handshake.GenerateServerResponse(headers["Sec-WebSocket-Key"]);
                byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                await _stream.WriteAsync(responseBytes, 0, responseBytes.Length);

                _isConnected = true;
                _ = StartReceivingAsync();
            }
            else
            {
                throw new InvalidOperationException("Invalid WebSocket handshake request");
            }
        }

        protected override bool IsServerSocket() { return true; }


    }
}