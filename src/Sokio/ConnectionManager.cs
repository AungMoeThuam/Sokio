


namespace Sokio
{
	public class ConnectionManager : IMessageMediator
	{
		private Dictionary<string, Room> _rooms;
		private Dictionary<string, IWebSocket> _sockets;

		public Dictionary<string, Room> Rooms
		{
			get
			{
				return _rooms;
			}
		}

		public Dictionary<string, IWebSocket> Sockets
		{
			get
			{
				return _sockets;
			}
		}
		public ConnectionManager()
		{
			_rooms = new Dictionary<string, Room>();
			_sockets = new Dictionary<string, IWebSocket>();
		}

		public void AddSocket(IWebSocket socket)
		{

			_sockets.Add(socket.Id, socket);


		}

		public void RemoveSocket(IWebSocket socket)
		{
			_sockets.Remove(socket.Id);
		}



		public void JoinRoom(string roomID, IWebSocket socket)
		{

			if (!_rooms.ContainsKey(roomID))
			{
				Room newRoom = new Room(roomID);
				newRoom.AddSocket(socket);
				_rooms.Add(roomID, newRoom);
			}
			else
			{
				_rooms[roomID].AddSocket(socket);
			}

			socket.JoinRoom(roomID);


		}

		public void LeaveRoom(string roomID, IWebSocket socket)
		{
			if (_rooms.ContainsKey(roomID))
			{
				_rooms[roomID].RemoveSocket(socket);
				if (_rooms[roomID].Sockets.Count == 0)
				{
					_rooms.Remove(roomID);
				}
			}
			else
			{
				Console.WriteLine($"Room with ID {roomID} not found.");
			}
		}

		public IWebSocket? GetSocket(string id)
		{
			if (_sockets.ContainsKey(id))
			{
				return _sockets[id];
			}
			return null;
		}

		public Dictionary<string, IWebSocket> GetRoom(string roomId)
		{
			if (_rooms.ContainsKey(roomId))
			{

				return _rooms[roomId].Sockets;
			}

			return new Dictionary<string, IWebSocket>();
		}
	}
}
