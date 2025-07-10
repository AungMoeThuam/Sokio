// Enhanced Event.cs
using System.Text;
using System.Text.Json;

namespace Sokio
{
	/// <summary>
	/// Represents an event containing a message
	/// </summary>
	public class Event
	{
		private string _eventName;
		private Message _message;

		public string EventName => _eventName;
		public Message Message => _message;



		public Event(string eventName, Message message)
		{
			_eventName = eventName ?? throw new ArgumentNullException(nameof(eventName));
			_message = message ?? throw new ArgumentNullException(nameof(message));
		}

		/// <summary>
		/// Converts event to JSON format
		/// </summary>
		public string ToJson()
		{
			var eventObject = new
			{
				eventName = EventName,
				message = JsonSerializer.Deserialize<object>(Message.ToJson())
			};

			return JsonSerializer.Serialize(eventObject);
		}

		/// <summary>
		/// Converts event to bytes for transmission
		/// For text messages: returns JSON bytes
		/// For binary messages: returns [metadata_length][metadata][raw_data]
		/// </summary>
		public byte[] ToBytes()
		{
			if (_message is BinaryMessage binaryMessage)
			{
				// For binary messages, create special format
				return ToBinaryEventBytes(binaryMessage);
			}
			else
			{
				// For text messages, just use JSON
				return Encoding.UTF8.GetBytes(ToJson());
			}
		}

		/// <summary>
		/// Special format for binary events: [metadata_length][metadata][raw_data]
		/// </summary>
		private byte[] ToBinaryEventBytes(BinaryMessage binaryMessage)
		{
			// Create event metadata (without the actual binary data)
			var metadata = new
			{
				eventName = EventName,
				message = new
				{
					id = binaryMessage.Id,
					type = binaryMessage.MessageType,
					fileName = binaryMessage.FileName,
					fileSize = binaryMessage.RawData?.Length ?? 0,
					senderId = binaryMessage.SenderId,
					receiverId = binaryMessage.ReceiverId,
					roomId = binaryMessage.RoomId,
					timestamp = binaryMessage.Timestamp.ToString("O"),
					content = (string?)null // Explicitly null for binary
				}
			};

			var metadataJson = JsonSerializer.Serialize(metadata);
			var metadataBytes = Encoding.UTF8.GetBytes(metadataJson);
			var metadataLength = BitConverter.GetBytes(metadataBytes.Length);

			// Calculate total size
			var rawData = binaryMessage.RawData ?? Array.Empty<byte>();
			var totalSize = 4 + metadataBytes.Length + rawData.Length;
			var result = new byte[totalSize];

			// Assemble: [metadata_length(4)][metadata][raw_data]
			Array.Copy(metadataLength, 0, result, 0, 4);
			Array.Copy(metadataBytes, 0, result, 4, metadataBytes.Length);
			if (rawData.Length > 0)
			{
				Array.Copy(rawData, 0, result, 4 + metadataBytes.Length, rawData.Length);
			}

			return result;
		}
	}
}
