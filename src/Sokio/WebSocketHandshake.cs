
using System.Security.Cryptography;
using System.Text;

namespace Sokio
{
    /// <summary>
    /// Handles WebSocket handshake
    /// </summary>
    public class WebSocketHandshake
    {
        private const string WebSocketGuid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        public Dictionary<string, string> ParseHttpRequest(string request)
        {
            var headers = new Dictionary<string, string>();
            var lines = request.Split(new[] { "\r\n" }, StringSplitOptions.None);

            foreach (var line in lines)
            {
                if (line.Contains(":"))
                {
                    var parts = line.Split(new[] { ':' }, 2);
                    if (parts.Length == 2)
                    {
                        headers[parts[0].Trim()] = parts[1].Trim();
                    }
                }
            }

            return headers;
        }

        public string GenerateServerResponse(string clientKey)
        {
            string acceptKey = GenerateAcceptKey(clientKey);

            return "HTTP/1.1 101 Switching Protocols\r\n" +
                   "Upgrade: websocket\r\n" +
                   "Connection: Upgrade\r\n" +
                   $"Sec-WebSocket-Accept: {acceptKey}\r\n\r\n";
        }

        public string GenerateClientRequest(string host, string path = "/")
        {
            string key = GenerateWebSocketKey();

            return $"GET {path} HTTP/1.1\r\n" +
                   $"Host: {host}\r\n" +
                   "Upgrade: websocket\r\n" +
                   "Connection: Upgrade\r\n" +
                   $"Sec-WebSocket-Key: {key}\r\n" +
                   "Sec-WebSocket-Version: 13\r\n\r\n";
        }

        private string GenerateAcceptKey(string clientKey)
        {
            string concatenated = clientKey + WebSocketGuid;
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(concatenated));
                return Convert.ToBase64String(hash);
            }
        }

        private string GenerateWebSocketKey()
        {
            byte[] keyBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(keyBytes);
            }
            return Convert.ToBase64String(keyBytes);
        }
    }
}
