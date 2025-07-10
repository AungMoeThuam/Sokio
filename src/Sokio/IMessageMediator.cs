namespace Sokio
{
    public interface IMessageMediator
    {
        IWebSocket? GetSocket(string id);
        Dictionary<string, IWebSocket> GetRoom(string room);

        public void AddSocket(IWebSocket socket)
       ;

        public void RemoveSocket(IWebSocket socket);



        public void JoinRoom(string roomID, IWebSocket socket);

        public void LeaveRoom(string roomID, IWebSocket socket);
    }
}