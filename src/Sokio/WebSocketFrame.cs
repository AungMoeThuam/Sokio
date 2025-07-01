using System.Text;

namespace Sokio
{
    /// <summary>
    /// Represents a WebSocket frame
    /// </summary>
    public class WebSocketFrame
    {
        public bool Fin { get; set; }
        public WebSocketOpcode Opcode { get; set; }
        public byte[] Payload { get; set; }
        public bool Masked { get; set; }

        public string GetTextPayload()
        {
            return Encoding.UTF8.GetString(Payload);
        }
    }

    public enum WebSocketOpcode : byte
    {
        Continuation = 0x0,
        Text = 0x1,
        Binary = 0x2,
        Close = 0x8,
        Ping = 0x9,
        Pong = 0xA
    }
}