namespace Sokio
{
    public interface ISocketEmitter
    {
        ISocketEmitter ToRoom(string room);
        ISocketEmitter ToSocket(string socketId);
        Task EmitAsync(string eventName, string data);
        Task EmitAsync(string eventName, byte[] data, string fileName);
        Task EmitAsync(Event ev);


    }

}