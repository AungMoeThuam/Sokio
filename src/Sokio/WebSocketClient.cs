
using System.Net.Sockets;
using System.Text;

namespace Sokio
{
    /// <summary>
    /// WebSocket client implementation
    /// </summary>
    public class WebSocketClient : BaseSocket
    {

        private Uri _uri;
        private WebSocketHandshake _handshake;

        public OnOpenHandler OnOpen;

        public WebSocketClient(string url) : base(null)
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
                    Console.WriteLine("connected");
                    _isConnected = true;
                    OnOpen?.Invoke(EventArgs.Empty);
                    _socketEmitter = new ClientSocketEmitter(this);
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
        public ISocketEmitter ToRoom(string roomId)
        {
            return _socketEmitter.ToRoom(roomId);
        }

        public ISocketEmitter ToSocket(string socketId)
        {
            return _socketEmitter.ToSocket(socketId);
        }

        public override async Task EmitAsync(string eventName, string data)
        {
            await _socketEmitter.EmitAsync(eventName, data);
        }

        public override async Task EmitAsync(string eventName, byte[] data, string fileName)
        {
            await _socketEmitter.EmitAsync(eventName, data, fileName);

        }

        public override async Task JoinRoom(string roomId)
        {
            Event ev = _messageFactory.CreateEvent(
                  eventName: "join-room",
                  content: null,
                  senderId: Id,
                  receiverId: null,
                  roomId: roomId
              );
            await SendAsync(ev);
        }

        public override async Task LeaveRoom(string roomId)
        {
            Event ev = _messageFactory.CreateEvent(
                 eventName: "leave-room",
                 content: null,
                 senderId: Id,
                 receiverId: null,
                 roomId: roomId
             );
            await SendAsync(ev);
        }

        protected override void Dispose()
        {
            _stream?.Write(_frameHandler.CreateCloseFrame(false));
            base.Dispose();
        }
    }
}