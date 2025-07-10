
namespace Sokio
{
    public class ServerSocketEmitter : ISocketEmitter
    {
        private readonly IWebSocket _self;
        private readonly IMessageMediator _mediator;
        private string? _targetRoomId;
        private string? _targetSocketId;

        private MessageFactory _messageFactory;

        public ServerSocketEmitter(IWebSocket self, IMessageMediator mediator)
        {
            _self = self;
            _mediator = mediator;
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



        public async Task EmitAsync(Event ev)
        {


            await Send(ev);

            // Reset for reuse
            _targetRoomId = null;
            _targetSocketId = null;
        }

        public async Task EmitAsync(string eventName, string data)
        {

            Event ev = _messageFactory.CreateEvent(
                eventName: eventName,
                content: data,
                senderId: _self.Id,
                receiverId: _targetSocketId,
                roomId: _targetRoomId
            );


            await Send(ev);

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

            await Send(ev);

            // Reset for reuse
            _targetRoomId = null;
            _targetSocketId = null;
        }

        private async Task Send(Event ev)
        {

            Console.WriteLine(" target socket id - " + _targetSocketId);
            Console.WriteLine(" target room id - " + _targetRoomId);
            Console.WriteLine(" self socket id - " + _self.Id);
            if (_targetSocketId != null)
            {
                var target = _mediator.GetSocket(_targetSocketId);
                if (target != null)
                    await target.SendAsync(ev);
            }
            else if (_targetRoomId != null)
            {

                foreach (var s in _mediator.GetRoom(_targetRoomId))
                {
                    var socket = s.Value;
                    if (socket.Id != _self.Id) // to() excludes self
                        await socket.SendAsync(ev);
                }

            }
            else
            {
                await _self.SendAsync(ev);
            }
        }


    }
}