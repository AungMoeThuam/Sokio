
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Sokio
{
    /// <summary>
    /// Server-side WebSocket connection
    /// </summary>
    public class WebSocketConnection : BaseSocket
    {


        public WebSocketConnection(TcpClient client) : base(null)
        {
            _client = client;
            _stream = client.GetStream();
        }



        public void SetSocketEmitter(IMessageMediator m)
        {
            _socketEmitter = new ServerSocketEmitter(this, m);
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

        public override async Task EmitAsync(string eventName, string data)
        {
            if (!_isConnected)
                throw new InvalidOperationException("Socket is not connected");
            try
            {

                await _socketEmitter.EmitAsync(eventName, data);


            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        public ISocketEmitter ToRoom(string roomId)
        {
            return _socketEmitter.ToRoom(roomId);
        }

        public ISocketEmitter ToSocket(string socketId)
        {
            return _socketEmitter.ToSocket(socketId);
        }
        public override async Task EmitAsync(string eventName, byte[] data, string fileName)
        {
            if (!_isConnected)
                throw new InvalidOperationException("Socket is not connected");

            try
            {
                await _socketEmitter.EmitAsync(eventName, data, fileName);
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        // public async Task EmitAsync<T>(string eventName, T data) where T : class
        // {
        //     var json = JsonSerializer.Serialize(data);
        //     await EmitAsync(eventName, json);
        // }



        protected override bool IsServerSocket() { return true; }

        public override async Task JoinRoom(string roomId)
        {
            if (!_joinedRooms.Contains(roomId))
            {
                _joinedRooms.Add(roomId);
            }

        }

        public override async Task LeaveRoom(string roomId)
        {
            _joinedRooms.Remove(roomId);
        }

        protected override void Dispose()
        {
            _stream?.Write(_frameHandler.CreateCloseFrame(true));
            base.Dispose();
        }


    }
}