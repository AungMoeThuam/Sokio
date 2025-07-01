using System.Net.Sockets;

namespace Sokio
{
    /// <summary>
    /// Abstract base class for WebSocket implementations
    /// </summary>

    public abstract class BaseSocket : IWebSocket
    {
        protected TcpClient _client;
        protected NetworkStream _stream;
        protected WebSocketFrameHandler _frameHandler;
        protected bool _isConnected;
        protected string _id;

        public string Id => _id;
        public bool IsConnected => _isConnected;

        public event OnMessageHandler OnMessage;
        public event OnCloseHandler OnClose;
        public event OnErrorHandler OnError;

        protected IPersistence? _persistence;


        protected BaseSocket()
        {
            _id = Guid.NewGuid().ToString();
            _frameHandler = new WebSocketFrameHandler();
            _isConnected = false;
        }

        public void Persist(IPersistence persistence)
        {
            _persistence = persistence;
        }

        public virtual async Task SendAsync(string message)
        {
            if (!_isConnected)
                throw new InvalidOperationException("WebSocket is not connected");

            try
            {
                byte[] frame = _frameHandler.CreateTextFrame(message, IsServerSocket());
                await _stream.WriteAsync(frame, 0, frame.Length);
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        public virtual async Task SendAsync(byte[] data)
        {
            if (!_isConnected)
                throw new InvalidOperationException("WebSocket is not connected");

            try
            {
                byte[] frame = _frameHandler.CreateBinaryFrame(data, IsServerSocket());
                await _stream.WriteAsync(frame, 0, frame.Length);
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        public virtual async Task SendAsync(Message message)
        {
            if (!_isConnected)
                throw new InvalidOperationException("WebSocket is not connected");

            try
            {
                // Set sender ID if not already set
                if (string.IsNullOrEmpty(message.SenderId))
                {
                    message.SenderId = _id;
                }

                byte[] data = message.ToBytes();

                // Determine frame type based on message type
                if (message is TextMessage)
                {
                    byte[] frame = _frameHandler.CreateTextFrame(message.ToJson(), IsServerSocket());
                    await _stream.WriteAsync(frame, 0, frame.Length);
                }
                else if (message is BinaryMessage)
                {
                    byte[] frame = _frameHandler.CreateBinaryFrame(data, IsServerSocket());
                    await _stream.WriteAsync(frame, 0, frame.Length);
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        public virtual async Task CloseAsync()
        {
            if (!_isConnected)
                return;

            try
            {
                byte[] closeFrame = _frameHandler.CreateCloseFrame(IsServerSocket());
                await _stream.WriteAsync(closeFrame, 0, closeFrame.Length);
                _isConnected = false;
                OnClose?.Invoke(EventArgs.Empty);
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
            finally
            {
                Dispose();
            }
        }

        protected async Task StartReceivingAsync()
        {
            byte[] buffer = new byte[4096];


            while (_isConnected)
            {
                try
                {
                    int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        _isConnected = false;
                        OnClose?.Invoke(EventArgs.Empty);
                        break;
                    }

                    WebSocketFrame frame = _frameHandler.ParseFrame(buffer, bytesRead);

                    switch (frame.Opcode)
                    {
                        case WebSocketOpcode.Text:
                            {
                                TextMessage m = MessageParser.ParseMessage(frame.GetTextPayload());
                                if (m == null)
                                {
                                    OnMessage?.Invoke(new MessageEventArgs(frame.GetTextPayload()));
                                }
                                else
                                {

                                    OnMessage?.Invoke(new MessageEventArgs(m));
                                }
                                break;
                            }

                        case WebSocketOpcode.Binary:
                            {
                                BinaryMessage m = MessageParser.ParseBinaryMessage(frame.Payload);
                                if (m == null)
                                {
                                    OnMessage?.Invoke(new MessageEventArgs(frame.Payload));
                                }
                                else
                                {
                                    if (_persistence != null) _persistence.writeBinaryFile(m.FileName, m.RawData);
                                    OnMessage?.Invoke(new MessageEventArgs(m));
                                }
                                break;
                            }
                        case WebSocketOpcode.Close:
                            await HandleCloseFrame();
                            break;
                        case WebSocketOpcode.Ping:
                            await HandlePingFrame(frame.Payload);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    HandleError(ex);
                    break;
                }
            }
        }

        protected virtual async Task HandleCloseFrame()
        {
            if (_isConnected)
            {
                _isConnected = false;
                try
                {
                    byte[] closeFrame = _frameHandler.CreateCloseFrame(IsServerSocket());
                    await _stream.WriteAsync(closeFrame, 0, closeFrame.Length);
                }
                catch { }

                OnClose?.Invoke(EventArgs.Empty);
            }
            Dispose();
        }

        protected virtual async Task HandlePingFrame(byte[] payload)
        {
            byte[] pongFrame = _frameHandler.CreatePongFrame(payload, IsServerSocket());
            await _stream.WriteAsync(pongFrame, 0, pongFrame.Length);
        }

        protected virtual void HandleError(Exception ex)
        {
            OnError?.Invoke(new ErrorEventArgs(ex));
            _isConnected = false;
            Dispose();
        }

        protected virtual void Dispose()
        {
            _stream?.Dispose();
            _client?.Dispose();
        }

        protected abstract bool IsServerSocket();
    }
}