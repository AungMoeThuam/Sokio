
using System.Net.Sockets;
using System.Text;

namespace Sokio
{
    /// <summary>
    /// WebSocket client implementation
    /// </summary>
    public class WebSocket : BaseSocket
    {

        private Uri _uri;
        private WebSocketHandshake _handshake;

        public OnOpenHandler OnOpen;

        public WebSocket(string url) : base()
        {
            _uri = new Uri(url);
            _handshake = new WebSocketHandshake();
            _client = new TcpClient();
        }



        public async Task ConnectAsync()
        {
            try
            {
                string host = _uri.Host;
                int port = _uri.Port == -1 ? (_uri.Scheme == "wss" ? 443 : 80) : _uri.Port;

                await _client.ConnectAsync(host, port);
                _stream = _client.GetStream();

                // Send handshake
                string request = _handshake.GenerateClientRequest(host, _uri.AbsolutePath);
                byte[] requestBytes = Encoding.UTF8.GetBytes(request);

                await _stream.WriteAsync(requestBytes, 0, requestBytes.Length);

                // Read response
                byte[] buffer = new byte[1024];
                int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                if (response.Contains("101 Switching Protocols"))
                {
                    _isConnected = true;
                    OnOpen?.Invoke(EventArgs.Empty);
                    _ = StartReceivingAsync();
                }
                else
                {
                    throw new InvalidOperationException("WebSocket handshake failed");
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        protected override bool IsServerSocket() => false;
    }
}