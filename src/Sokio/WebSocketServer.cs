using System.Net;
using System.Net.Sockets;

namespace Sokio
{
    /// <summary>
    /// WebSocket server implementation   
    /// </summary>

    public class WebSocketServer
    {
        private TcpListener _listener;
        private WebSocketHandshake _handshake;
        private bool _isListening;
        private List<IWebSocket> _clients;
        private IPersistence? _persistence;

        private IMessageMediator _connectionManager;


        public int Port
        {
            get;
        }
        public string Address
        {
            get;
        }

        public event OnConnectionHandler OnConnection;
        public event OnErrorHandler OnError;



        public IReadOnlyList<IWebSocket> Clients { get { return _clients.AsReadOnly(); } }

        public WebSocketServer(int port) : this(IPAddress.Any, port)
        {
        }

        public WebSocketServer(string host, int port) : this(IPAddress.Parse(host), port)
        {
        }

        public WebSocketServer(IPAddress address, int port)
        {
            Port = port;
            Address = address.ToString();
            _listener = new TcpListener(address, port);
            _handshake = new WebSocketHandshake();
            _clients = new List<IWebSocket>();
            _isListening = false;
            _connectionManager = new ConnectionManager();

        }

        public void SetPersistenceBinaryFile(IPersistence persistence)
        {
            _persistence = persistence;
        }

        public void Listen()
        {
            _listener.Start();
            _isListening = true;
            _ = AcceptConnectionsAsync();
        }

        public void Stop()
        {
            _isListening = false;
            _listener.Stop();

            foreach (var client in _clients)
            {
                client.CloseAsync().Wait();
            }
            _clients.Clear();
        }

        private async Task AcceptConnectionsAsync()
        {
            while (_isListening)
            {
                try
                {
                    TcpClient tcpClient = await _listener.AcceptTcpClientAsync();
                    _ = HandleConnectionAsync(tcpClient);
                }
                catch (Exception ex)
                {
                    if (_isListening)
                    {
                        OnError?.Invoke(new ErrorEventArgs(ex));
                    }
                }
            }
        }

        private async Task HandleConnectionAsync(TcpClient tcpClient)
        {
            WebSocketConnection connection = new WebSocketConnection(tcpClient);
            connection.SetSocketEmitter(_connectionManager);
            try
            {
                await connection.AcceptAsync(_handshake);

                _clients.Add(connection);
                _connectionManager.AddSocket(connection);
                OnConnection?.Invoke(new ConnectionEventArgs(connection));

                connection.OnClose += (e) =>
                {
                    _clients.Remove(connection);
                };
            }
            catch (Exception ex)
            {
                OnError?.Invoke(new ErrorEventArgs(ex));
                tcpClient.Close();
            }
        }

        public async Task BroadcastAsync(string message)
        {
            var tasks = new List<Task>();

            foreach (var client in _clients)
            {
                if (client.IsConnected)
                {
                    tasks.Add(client.SendAsync(message));
                }
            }

            await Task.WhenAll(tasks);
        }

        public async Task BroadcastAsync(byte[] data)
        {
            var tasks = new List<Task>();

            foreach (var client in _clients)
            {
                if (client.IsConnected)
                {
                    tasks.Add(client.SendAsync(data));
                }
            }

            await Task.WhenAll(tasks);
        }
    }
}
