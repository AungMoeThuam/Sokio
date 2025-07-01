
using System.Text.Json;

namespace Sokio
{
    /// <summary>
    /// Parses raw WebSocket data into Message objects
    /// </summary>
    public static class MessageParser
    {
        /// <summary>
        /// Attempts to parse a message from raw string data
        /// </summary>
        public static TextMessage? ParseMessage(string data)
        {
            try
            {
                using (JsonDocument doc = JsonDocument.Parse(data))
                {
                    var root = doc.RootElement;

                    if (root.TryGetProperty("type", out var typeElement))
                    {
                        var type = typeElement.GetString();

                        switch (type)
                        {
                            case "text":
                                return TextMessage.FromJson(data);
                            case "binary":
                                // Binary messages should come through binary frame
                                throw new InvalidOperationException("Binary message received as text frame");
                        }
                    }
                    Console.WriteLine("invalid");
                    // No type specified, assume plain text
                    return null;
                }
            }
            catch (JsonException)
            {
                // Not JSON, treat as plain text message
                return null;
            }
        }

        /// <summary>
        /// Parses a binary message from raw bytes
        /// </summary>
        public static BinaryMessage? ParseBinaryMessage(byte[] data)
        {
            try
            {
                // Check if it's our binary message format
                if (data.Length > 4)
                {
                    var metadataLength = BitConverter.ToInt32(data, 0);
                    if (metadataLength > 0 && metadataLength < data.Length - 4)
                    {
                        return BinaryMessage.FromBytes(data);
                    }
                }

                // Not our format, create a generic binary message
                return null;
            }
            catch
            {
                // Failed to parse, return as generic binary message
                return null;
            }
        }
    }
}
