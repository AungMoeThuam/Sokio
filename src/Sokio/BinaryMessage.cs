using System.Text;
using System.Text.Json;

namespace Sokio
{
    /// <summary>
    /// Represents a binary message (e.g., file transfer)
    /// </summary>
    public class BinaryMessage : Message
    {
        private string _fileName;
        private byte[] _rawData;

        /// <summary>
        /// The name of the file being sent
        /// </summary>
        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; }
        }

        /// <summary>
        /// The raw binary data
        /// </summary>
        public byte[] RawData
        {
            get { return _rawData; }
            set { _rawData = value; }
        }

        public override string MessageType
        {
            get { return "binary"; }
        }

        /// <summary>
        /// Creates a new binary message
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="rawData">Binary data</param>
        /// <param name="receiverId">Target receiver ID (null for broadcast)</param>
        /// <param name="senderId">Sender ID (will be set by server if null)</param>
        public BinaryMessage(string fileName, byte[] rawData, string receiverId = null,
                           string senderId = null)
            : base(senderId, receiverId)
        {

            string extension = Path.GetExtension(fileName);
            string uniqueName = Guid.NewGuid().ToString("N") + extension;
            _fileName = uniqueName;
            _rawData = rawData;
        }

        public override string ToJson()
        {
            // For binary messages, we only serialize metadata as JSON
            var metadata = new
            {
                id = Id,
                type = MessageType,
                fileName = FileName,
                fileSize = RawData?.Length ?? 0,
                senderId = SenderId,
                receiverId = ReceiverId,
                timestamp = Timestamp.ToString("O")
            };

            return JsonSerializer.Serialize(metadata);
        }

        public override byte[] ToBytes()
        {
            // Create a format: [metadata_length(4 bytes)][metadata][raw_data]
            var metadataJson = ToJson();
            var metadataBytes = Encoding.UTF8.GetBytes(metadataJson);
            var metadataLength = BitConverter.GetBytes(metadataBytes.Length);

            // Calculate total size
            var totalSize = 4 + metadataBytes.Length + (_rawData?.Length ?? 0);
            var result = new byte[totalSize];

            // Copy metadata length (4 bytes)
            Array.Copy(metadataLength, 0, result, 0, 4);

            // Copy metadata
            Array.Copy(metadataBytes, 0, result, 4, metadataBytes.Length);

            // Copy raw data
            if (_rawData != null && _rawData.Length > 0)
            {
                Array.Copy(_rawData, 0, result, 4 + metadataBytes.Length, _rawData.Length);
            }

            return result;
        }

        /// <summary>
        /// Deserializes a BinaryMessage from bytes
        /// </summary>
        public static BinaryMessage FromBytes(byte[] data)
        {
            // Read metadata length
            var metadataLength = BitConverter.ToInt32(data, 0);

            // Read metadata
            var metadataJson = Encoding.UTF8.GetString(data, 4, metadataLength);

            // Read raw data
            var rawDataLength = data.Length - 4 - metadataLength;
            var rawData = new byte[rawDataLength];
            if (rawDataLength > 0)
            {
                Array.Copy(data, 4 + metadataLength, rawData, 0, rawDataLength);
            }

            // Parse metadata
            using (JsonDocument doc = JsonDocument.Parse(metadataJson))
            {
                var root = doc.RootElement;
                var fileName = root.GetProperty("fileName").GetString();
                var receiverId = root.TryGetProperty("receiverId", out var r) ? r.GetString() : null;
                var senderId = root.TryGetProperty("senderId", out var s) ? s.GetString() : null;

                return new BinaryMessage(fileName, rawData, receiverId, senderId);
            }
        }
    }
}