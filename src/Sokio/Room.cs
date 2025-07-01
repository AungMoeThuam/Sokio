

namespace Sokio
{
    public class Room : IEmitable
    {
        private string _id;
        private Dictionary<string, BaseSocket> _sockets;

        public string Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public Dictionary<string, BaseSocket> Sockets
        {
            get { return _sockets; }
            set { _sockets = value; }
        }

        public Room(string id)
        {
            _id = "room-" + id;
            _sockets = new Dictionary<string, BaseSocket>();
        }

        public void AddSocket(BaseSocket socket)
        {
            _sockets.Add(socket.Id, socket);
        }

        public void RemoveSocket(BaseSocket socket)
        {
            _sockets.Remove(socket.Id);
        }

        public void Emit(string eventName, object data)
        {
            foreach (KeyValuePair<string, BaseSocket> s in _sockets)
            {
                _ = s.Value.SendTextAsync(data);
            }
        }


    }
}