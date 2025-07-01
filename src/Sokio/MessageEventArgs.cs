

namespace Sokio
{
    public class MessageEventArgs : EventArgs
    {
        public string? Text { get; }
        public byte[]? RawData { get; }
        public bool IsText { get; }

        public Message? Message { get; }

        public MessageEventArgs(Message msg)
        {
            IsText = msg.MessageType == "text";

            if (msg is TextMessage)
            {
                Text = ((TextMessage)msg).Content;
            }
            else
            {
                RawData = ((BinaryMessage)msg).RawData;
            }
            Message = msg;
        }

        public MessageEventArgs(string message)
        {
            Text = message;
            IsText = true;

        }

        public MessageEventArgs(byte[] data)
        {
            RawData = data;
            IsText = false;
        }
    }
}