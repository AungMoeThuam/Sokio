
namespace Sokio
{
    public class Room
    {
        private string _id;
        private Dictionary<string, IWebSocket> _sockets;

        public string Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public Dictionary<string, IWebSocket> Sockets
        {
            get { return _sockets; }
        }

        public Room(string id)
        {
            _id = "room-" + id;
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

    }
}