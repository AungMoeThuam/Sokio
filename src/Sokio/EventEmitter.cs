



// namespace Sokio
// {
// 	public class EventEmitter : IEmitable
// 	{
// 		private IEmitable? _target;

// 		public EventEmitter(IEmitable socket)
// 		{
// 			_target = socket;
// 		}

// 		public void Emit(string eventName, object data)
// 		{
// 			if (_target != null)
// 				_target.Emit(eventName, data);
// 		}
// 	}


// 	public class EventDispatcher
// 	{
// 		private SocketStore _socketStore;
// 		public EventDispatcher(SocketStore socketStore)
// 		{
// 			_socketStore = socketStore;
// 		}

// 		public EventEmitter ToSocket(string _targetSocketID)
// 		{
// 			SokioSocket targetSocket = _socketStore.Sockets[_targetSocketID];
// 			return new EventEmitter(targetSocket);

// 		}

// 		public EventEmitter ToRoom(string targetRoomID)
// 		{
// 			Room targetRoom = _socketStore.GetRoom(targetRoomID);


// 			return new EventEmitter(targetRoom);


// 		}


// 	}
// }
