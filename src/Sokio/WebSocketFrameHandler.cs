
using System.Text;

namespace Sokio
{
    /// <summary>
    /// Handles WebSocket frame parsing and creation
    /// </summary>
    public class WebSocketFrameHandler
    {
        public byte[] CreateTextFrame(string message, bool isServer)
        {
            byte[] payload = Encoding.UTF8.GetBytes(message);
            return CreateFrame(WebSocketOpcode.Text, payload, !isServer);
        }

        public byte[] CreateBinaryFrame(byte[] data, bool isServer)
        {
            return CreateFrame(WebSocketOpcode.Binary, data, !isServer);
        }

        public byte[] CreateCloseFrame(bool isServer)
        {
            return CreateFrame(WebSocketOpcode.Close, new byte[0], !isServer);
        }

        public byte[] CreatePongFrame(byte[] payload, bool isServer)
        {
            return CreateFrame(WebSocketOpcode.Pong, payload, !isServer);
        }

        private byte[] CreateFrame(WebSocketOpcode opcode, byte[] payload, bool mask)
        {
            List<byte> frame = new List<byte>();

            // First byte: FIN + RSV + Opcode
            frame.Add((byte)(0x80 | (byte)opcode));

            // Payload length
            int payloadLength = payload.Length;
            byte[] maskKey = null;

            if (payloadLength < 126)
            {
                frame.Add((byte)((mask ? 0x80 : 0) | payloadLength));
            }
            else if (payloadLength < 65536)
            {
                frame.Add((byte)((mask ? 0x80 : 0) | 126));
                frame.Add((byte)(payloadLength >> 8));
                frame.Add((byte)payloadLength);
            }
            else
            {
                frame.Add((byte)((mask ? 0x80 : 0) | 127));
                for (int i = 7; i >= 0; i--)
                {
                    frame.Add((byte)(payloadLength >> (8 * i)));
                }
            }

            // Masking key
            if (mask)
            {
                maskKey = GenerateMaskKey();
                frame.AddRange(maskKey);

                // Mask payload
                for (int i = 0; i < payload.Length; i++)
                {
                    payload[i] ^= maskKey[i % 4];
                }
            }

            // Add payload
            frame.AddRange(payload);

            return frame.ToArray();
        }

        public WebSocketFrame ParseFrame(byte[] buffer, int length)
        {
            if (length < 2)
                throw new InvalidOperationException("Invalid frame");

            var frame = new WebSocketFrame();
            int index = 0;

            // First byte
            byte firstByte = buffer[index++];
            frame.Fin = (firstByte & 0x80) != 0;
            frame.Opcode = (WebSocketOpcode)(firstByte & 0x0F);

            // Second byte
            byte secondByte = buffer[index++];
            frame.Masked = (secondByte & 0x80) != 0;
            long payloadLength = secondByte & 0x7F;

            // Extended payload length
            if (payloadLength == 126)
            {
                payloadLength = (buffer[index] << 8) | buffer[index + 1];
                index += 2;
            }
            else if (payloadLength == 127)
            {
                payloadLength = 0;
                for (int i = 0; i < 8; i++)
                {
                    payloadLength = (payloadLength << 8) | buffer[index++];
                }
            }

            // Masking key
            byte[] maskKey = null;
            if (frame.Masked)
            {
                maskKey = new byte[4];
                Array.Copy(buffer, index, maskKey, 0, 4);
                index += 4;
            }

            // Payload
            frame.Payload = new byte[payloadLength];
            Array.Copy(buffer, index, frame.Payload, 0, (int)payloadLength);

            // Unmask if needed
            if (frame.Masked && maskKey != null)
            {
                for (int i = 0; i < frame.Payload.Length; i++)
                {
                    frame.Payload[i] ^= maskKey[i % 4];
                }
            }

            return frame;
        }

        private byte[] GenerateMaskKey()
        {
            Random random = new Random();
            byte[] key = new byte[4];
            random.NextBytes(key);
            return key;
        }
    }
}