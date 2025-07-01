

namespace Sokio
{
	public class SocketStore
	{
		private Dictionary<string, Room> _rooms;
		private Dictionary<string, SokioSocket> _sockets;

		public Dictionary<string, Room> Rooms
		{
			get
			{
				return _rooms;
			}
		}

		public Dictionary<string, SokioSocket> Sockets
		{
			get
			{
				return _sockets;
			}
		}
		public SocketStore()
		{
			_rooms = new Dictionary<string, Room>();
			_sockets = new Dictionary<string, SokioSocket>();
		}

		public void AddSocket(SokioSocket socket)
		{

			_sockets.Add(socket.Id, socket);


		}

		public void RemoveSocket(SokioSocket socket)
		{
			_sockets.Remove(socket.Id);
		}

		public Room? GetRoom(string roomID)
		{
			if (_rooms.ContainsKey(roomID))
			{
				return _rooms[roomID];
			}

			return null;
		}


		public void JoinRoom(string roomID, SokioSocket socket)
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
		}

		public void LeaveRoom(string roomID, SokioSocket socket)
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
	}
}
