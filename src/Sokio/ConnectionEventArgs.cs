namespace Sokio
{
    public class ConnectionEventArgs : EventArgs
    {
        public BaseSocket WebSocket { get; }

        public ConnectionEventArgs(BaseSocket webSocket)
        {
            WebSocket = webSocket;
        }
    }

}