

// Enhanced BaseSocket emit method
// namespace Sokio
// {
//     public abstract partial class BaseSocket
//     {


//         // Properties for targeting
//         public string? ReceiverId { get; set; }
//         public string? RoomId { get; set; }
//     }
// }

// Usage Examples
// namespace Sokio.Examples
// {
//     class MessageFactoryExample
//     {
//         static async Task Example()
//         {
//             var factory = MessageFactory.Instance;
//             var socket = new WebSocket("ws://localhost:8080");

//             // Example 1: Simple text event
//             await socket.EmitAsync("chat", "Hello world!");

//             // Example 2: Binary file event
//             byte[] fileData = File.ReadAllBytes("document.pdf");
//             await socket.EmitAsync("file-upload", fileData, "document.pdf");

//             // Example 3: Typed data event
//             var userData = new { name = "John", age = 30 };
//             await socket.EmitAsync("user-update", userData);

//             // Example 4: Targeted emit
//             socket.ReceiverId = "user123";
//             await socket.EmitAsync("private-message", "Hello user!");

//             // Example 5: Room emit
//             socket.RoomId = "general";
//             socket.ReceiverId = null;
//             await socket.EmitAsync("room-message", "Hello room!");

//             // Server-side handling
//             socket.OnMessage += (e) =>
//             {
//                 if (e.Message is Event evt)
//                 {
//                     Console.WriteLine($"Event: {evt.EventName}");

//                     switch (evt.Message)
//                     {
//                         case TextMessage text:
//                             Console.WriteLine($"Text: {text.Content}");
//                             break;
//                         case BinaryMessage binary:
//                             Console.WriteLine($"File: {binary.FileName} ({binary.RawData.Length} bytes)");
//                             File.WriteAllBytes($"received_{binary.FileName}", binary.RawData);
//                             break;
//                     }
//                 }
//             };
//         }
//     }
// }