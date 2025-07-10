using System.Net.Sockets;
using System.Text.Json;

namespace Sokio
{
    /// <summary>
    /// Abstract base class for WebSocket implementations
    /// </summary>

    public abstract class BaseSocket : IWebSocket
    {
        protected readonly IMessageFactory _messageFactory;
        protected TcpClient _client;
        protected NetworkStream _stream;
        protected WebSocketFrameHandler _frameHandler;
        protected bool _isConnected;
        protected string _id;

        protected EventCallbackManager _eventCallbackManager;

        protected ISocketEmitter _socketEmitter;

        public string Id => _id;
        public bool IsConnected => _isConnected;

        public event OnMessageHandler OnMessage;
        public event OnCloseHandler OnClose;
        public event OnErrorHandler OnError;

        public event OnJoinRoomHandler OnJoinRoom;
        public event OnLeaveRoomHandler OnLeaveRoom;

        protected IPersistence? _persistence;

        protected List<string> _joinedRooms;




        protected BaseSocket(ISocketEmitter socketEmitter)
        {
            _id = Guid.NewGuid().ToString();
            _frameHandler = new WebSocketFrameHandler();
            _isConnected = false;
            _eventCallbackManager = new EventCallbackManager();
            _socketEmitter = socketEmitter;
            _messageFactory = MessageFactory.Instance;
            _joinedRooms = new List<string>();

        }

        public void Persist(IPersistence persistence)
        {
            _persistence = persistence;
        }

        public List<string> JoinedRooms
        {
            get
            {
                return _joinedRooms;
            }
        }


        public void On(string eventName, Func<MessageEventArgs, Task> callback)
        {
            _eventCallbackManager.register(eventName, new EventCallback(eventName, callback));
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


        public virtual async Task SendAsync(Event ev)
        {
            if (!_isConnected)
                throw new InvalidOperationException("WebSocket is not connected");

            try
            {
                byte[] frame;
                if (ev.Message.MessageType == "text")
                {
                    frame = _frameHandler.CreateTextFrame(ev.ToJson(), IsServerSocket());
                    await _stream.WriteAsync(frame, 0, frame.Length);

                }
                else
                {

                    frame = _frameHandler.CreateBinaryFrame(ev.ToBytes(), IsServerSocket());
                }
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
                                Event ev = EventParser.ParseTextEvent(frame.GetTextPayload());
                                TextMessage msg = (TextMessage)ev.Message;
                                Console.WriteLine("text is " + ev.EventName);
                                if (ev.EventName == "join-room")
                                {
                                    OnJoinRoom?.Invoke(ev);
                                }
                                else if (ev.EventName == "leave-room")
                                {
                                    OnLeaveRoom?.Invoke(ev);
                                }

                                if (ev.Message.ReceiverId != null)
                                {
                                    await _socketEmitter.ToSocket(ev.Message.ReceiverId).EmitAsync(ev);
                                }
                                else if (ev.Message.RoomId != null)
                                {
                                    await _socketEmitter.ToRoom(ev.Message.RoomId).EmitAsync(ev);
                                }
                                else
                                {
                                    _eventCallbackManager.Execute(ev.EventName, new MessageEventArgs(ev.Message));

                                }
                                OnMessage?.Invoke(new MessageEventArgs(msg));
                                break;
                            }

                        case WebSocketOpcode.Binary:
                            {
                                Event ev = EventParser.ParseBinaryEvent(frame.Payload);
                                if (ev == null) break;
                                BinaryMessage msg = (BinaryMessage)ev.Message;
                                if (ev.Message.ReceiverId != null)
                                {
                                    await _socketEmitter.ToSocket(ev.Message.ReceiverId).EmitAsync(ev);
                                }
                                else if (ev.Message.RoomId != null)
                                {
                                    await _socketEmitter.ToRoom(ev.Message.RoomId).EmitAsync(ev);
                                }
                                else
                                {
                                    _eventCallbackManager.Execute(ev.EventName, new MessageEventArgs(ev.Message));

                                }
                                OnMessage?.Invoke(new MessageEventArgs(msg));
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
                    Console.WriteLine("error happended in listening");
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
            Console.WriteLine("error happened - " + ex.Message);
            Dispose();
        }

        protected virtual void Dispose()
        {
            _stream?.Dispose();
            _client?.Dispose();
        }

        protected abstract bool IsServerSocket();

        public abstract Task EmitAsync(string eventName, string data);
        public abstract Task EmitAsync(string eventName, byte[] data, string fileName);

        public abstract Task JoinRoom(string roomId);
        public abstract Task LeaveRoom(string roomId);

    }
}
