
// EventParser.cs
using System.Text;
using System.Text.Json;

namespace Sokio
{
    /// <summary>
    /// Parses events from raw data
    /// </summary>
    public static class EventParser
    {
        /// <summary>
        /// Parses an event from text data (JSON)
        /// </summary>
        public static Event? ParseTextEvent(string data)
        {
            try
            {
                using (JsonDocument doc = JsonDocument.Parse(data))
                {
                    var root = doc.RootElement;

                    if (!root.TryGetProperty("eventName", out var eventNameElement))
                        return null;

                    var eventName = eventNameElement.GetString();
                    if (string.IsNullOrEmpty(eventName))
                        return null;

                    if (!root.TryGetProperty("message", out var messageElement))
                        return null;

                    // Parse message based on type
                    if (messageElement.TryGetProperty("type", out var typeElement))
                    {
                        var type = typeElement.GetString();
                        var messageJson = messageElement.GetRawText();

                        Message? message = type switch
                        {
                            "text" => TextMessage.FromJson(messageJson),
                            "binary" => throw new InvalidOperationException("Binary message in text event"),
                            _ => null
                        };

                        if (message != null)
                        {
                            // Set additional properties if present
                            if (messageElement.TryGetProperty("roomId", out var roomIdElement))
                            {
                                message.RoomId = roomIdElement.GetString();
                            }

                            return new Event(eventName, message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Failed to parse text event: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Parses an event from binary data
        /// Format: [metadata_length][metadata][raw_data]
        /// </summary>
        public static Event? ParseBinaryEvent(byte[] data)
        {
            try
            {
                if (data.Length < 4)
                    return null;

                // Read metadata length
                var metadataLength = BitConverter.ToInt32(data, 0);
                if (metadataLength <= 0 || metadataLength > data.Length - 4)
                    return null;

                // Read metadata
                var metadataJson = Encoding.UTF8.GetString(data, 4, metadataLength);

                // Read raw data
                var rawDataStart = 4 + metadataLength;
                var rawDataLength = data.Length - rawDataStart;
                byte[]? rawData = null;

                if (rawDataLength > 0)
                {
                    rawData = new byte[rawDataLength];
                    Array.Copy(data, rawDataStart, rawData, 0, rawDataLength);
                }

                // Parse metadata
                using (JsonDocument doc = JsonDocument.Parse(metadataJson))
                {
                    var root = doc.RootElement;

                    if (!root.TryGetProperty("eventName", out var eventNameElement))
                        return null;

                    var eventName = eventNameElement.GetString();
                    if (string.IsNullOrEmpty(eventName))
                        return null;

                    if (!root.TryGetProperty("message", out var messageElement))
                        return null;

                    // Extract binary message properties
                    var fileName = messageElement.GetProperty("fileName").GetString() ?? "unknown";
                    var senderId = messageElement.TryGetProperty("senderId", out var s) ? s.GetString() : null;
                    var receiverId = messageElement.TryGetProperty("receiverId", out var r) ? r.GetString() : null;
                    var roomId = messageElement.TryGetProperty("roomId", out var rm) ? rm.GetString() : null;

                    var binaryMessage = new BinaryMessage(fileName, rawData ?? Array.Empty<byte>(), receiverId, senderId, roomId);
                    return new Event(eventName, binaryMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to parse binary event: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Attempts to parse an event from any data type
        /// </summary>
        public static Event? Parse(byte[] data)
        {
            // First try to parse as text
            try
            {
                var text = Encoding.UTF8.GetString(data);
                var textEvent = ParseTextEvent(text);
                if (textEvent != null)
                    return textEvent;
            }
            catch { }

            // If text parsing fails, try binary format
            return ParseBinaryEvent(data);
        }
    }
}
