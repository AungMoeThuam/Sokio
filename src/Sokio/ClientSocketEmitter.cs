
namespace Sokio
{
    public class ClientSocketEmitter : ISocketEmitter
    {
        private string? _targetRoomId;
        private string? _targetSocketId;
        private MessageFactory _messageFactory;


        private IWebSocket _self;

        public ClientSocketEmitter(IWebSocket self)
        {
            _self = self;
            Console.WriteLine("client socket emitter " + _self.IsConnected);
            _messageFactory = MessageFactory.Instance;
        }



        public ISocketEmitter ToRoom(string roomId)
        {
            _targetRoomId = roomId;
            return this;
        }

        public ISocketEmitter ToSocket(string socketId)
        {
            _targetSocketId = socketId;
            return this;
        }



        public async Task EmitAsync(string eventName, string data)
        {
            Console.WriteLine(" target socket id - " + _targetSocketId);
            Console.WriteLine(" target room id - " + _targetRoomId);
            Console.WriteLine(" self socket id - " + _self.Id);

            Event ev = _messageFactory.CreateEvent(
                eventName: eventName,
                content: data,
                senderId: _self.Id,
                receiverId: _targetSocketId,
                roomId: _targetRoomId
            );



            await _self.SendAsync(ev.ToJson());

            // Reset for reuse
            _targetRoomId = null;
            _targetSocketId = null;
        }


        public async Task EmitAsync(string eventName, byte[] data, string fileName)
        {
            Event ev = _messageFactory.CreateEvent(
                eventName: eventName,
                data: data,
                fileName: fileName,
                senderId: _self.Id,
                receiverId: _targetSocketId,
                roomId: _targetRoomId
                );

            await Send(ev.ToBytes());

            // Reset for reuse
            _targetRoomId = null;
            _targetSocketId = null;
        }

        public async Task EmitAsync(Event ev)
        {



            await Send(ev.ToBytes());

            // Reset for reuse
            _targetRoomId = null;
            _targetSocketId = null;
        }

        private async Task Send(byte[] raw)
        {
            Console.WriteLine("self - " + _self.IsConnected);
            await _self.SendAsync(raw);

        }
    }
}