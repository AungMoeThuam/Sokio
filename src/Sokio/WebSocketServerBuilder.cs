// namespace Sokio
// {
//     public class WebSocketServerBuilder
//     {
//         private IPAddress _address = IPAddress.Any;
//         private int _port = 8080;
//         private readonly List<IMiddleware> _middlewares = new();
//         private readonly Dictionary<string, INamespace> _namespaces = new();
//         private IBroadcastStrategy _defaultBroadcastStrategy;
//         private IPersistence _persistence;
//         private IRoomManager _roomManager;
//         private int _maxConnections = 1000;
//         private TimeSpan _pingInterval = TimeSpan.FromSeconds(30);
//         private TimeSpan _connectionTimeout = TimeSpan.FromMinutes(5);

//         public WebSocketServerBuilder WithAddress(string address)
//         {
//             _address = IPAddress.Parse(address);
//             return this;
//         }

//         public WebSocketServerBuilder WithPort(int port)
//         {
//             _port = port;
//             return this;
//         }

//         public WebSocketServerBuilder WithMaxConnections(int maxConnections)
//         {
//             _maxConnections = maxConnections;
//             return this;
//         }

//         public WebSocketServerBuilder WithPingInterval(TimeSpan interval)
//         {
//             _pingInterval = interval;
//             return this;
//         }

//         public WebSocketServerBuilder WithConnectionTimeout(TimeSpan timeout)
//         {
//             _connectionTimeout = timeout;
//             return this;
//         }

//         public WebSocketServerBuilder WithMiddleware(IMiddleware middleware)
//         {
//             _middlewares.Add(middleware);
//             return this;
//         }

//         public WebSocketServerBuilder WithNamespace(string path, Action<INamespace> configure = null)
//         {
//             var ns = new Namespace(path);
//             configure?.Invoke(ns);
//             _namespaces[path] = ns;
//             return this;
//         }

//         public WebSocketServerBuilder WithDefaultBroadcastStrategy(IBroadcastStrategy strategy)
//         {
//             _defaultBroadcastStrategy = strategy;
//             return this;
//         }

//         public WebSocketServerBuilder WithPersistence(IPersistence persistence)
//         {
//             _persistence = persistence;
//             return this;
//         }

//         public WebSocketServerBuilder WithRoomManager(IRoomManager roomManager)
//         {
//             _roomManager = roomManager;
//             return this;
//         }

//         public EnhancedWebSocketServer Build()
//         {
//             // Create server with all configurations
//             var server = new EnhancedWebSocketServer(_address, _port)
//             {
//                 MaxConnections = _maxConnections,
//                 PingInterval = _pingInterval,
//                 ConnectionTimeout = _connectionTimeout,
//                 DefaultBroadcastStrategy = _defaultBroadcastStrategy ?? new BroadcastToAllStrategy(),
//                 RoomManager = _roomManager ?? new RoomManager()
//             };

//             // Add middlewares
//             foreach (var middleware in _middlewares)
//             {
//                 server.AddMiddleware(middleware);
//             }

//             // Add namespaces
//             foreach (var ns in _namespaces)
//             {
//                 server.AddNamespace(ns.Value);
//             }

//             // Set persistence if provided
//             if (_persistence != null)
//             {
//                 server.SetPersistenceBinaryFile(_persistence);
//             }

//             return server;
//         }
//     }

// }