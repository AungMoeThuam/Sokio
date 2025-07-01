




// // Example usage in a chat application
// namespace SokioD.Examples
// {
//     class MessageExample
//     {
//         static async Task ExampleUsage()
//         {
//             var client = new WebSocket("ws://localhost:8080");
//             await client.ConnectAsync();

//             // Method 1: Send raw string (backward compatible)
//             await client.SendAsync("Hello everyone!");

//             // Method 2: Send structured text message
//             var textMsg = new TextMessage(
//                 content: "Hello user123!",
//                 receiverId: "user123"  // Will be routed to specific user (if server supports it)
//             );
//             await client.SendAsync(textMsg);

//             // Method 3: Send binary file
//             byte[] fileData = File.ReadAllBytes("document.pdf");
//             var binaryMsg = new BinaryMessage(
//                 fileName: "document.pdf",
//                 rawData: fileData,
//                 receiverId: "user456"
//             );
//             await client.SendAsync(binaryMsg);

//             // Method 4: Broadcast to room (for future HD feature)
//             var roomMsg = new TextMessage(
//                 content: "Hello room members!"
//             );
//             await client.SendAsync(roomMsg);

//             // Receiving messages
//             client.OnMessage += (e) =>
//             {
//                 if (e.IsText)
//                 {
//                     var message = MessageParser.ParseMessage(e.Message);
//                     if (message is TextMessage textMessage)
//                     {
//                         Console.WriteLine($"From {textMessage.SenderId}: {textMessage.Content}");
//                     }
//                 }
//                 else
//                 {
//                     var message = MessageParser.ParseBinaryMessage(e.RawData);
//                     if (message is BinaryMessage binaryMessage)
//                     {
//                         Console.WriteLine($"Received file: {binaryMessage.FileName} ({binaryMessage.RawData.Length} bytes)");
//                     }
//                 }
//             };
//         }
//     }
// }