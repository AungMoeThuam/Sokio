namespace Sokio
{
    public delegate void OnConnectionHandler(ConnectionEventArgs ce);
    public delegate void OnMessageHandler(MessageEventArgs m);
    public delegate void OnCloseHandler(EventArgs m);
    public delegate void OnErrorHandler(ErrorEventArgs m);
    public delegate void OnOpenHandler(EventArgs m);

}