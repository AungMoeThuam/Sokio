﻿
using SokioD;
namespace Sokio
{
    class ChatServer
    {
        static async Task Main(string[] args)
        {
            var server = new WebSocketServer(8080);
            server.SetPersistenceBinaryFile(new BinaryFileStore("./hello"));


            server.OnConnection += async (e) =>
            {

                var ws = e.WebSocket;

                Console.WriteLine($"Client connected: {ws.Id}");

                await server.BroadcastAsync("adad");

                // Handle messages from this client
                ws.OnMessage += async (msg) =>
                {
                    if (msg.IsText)
                    {
                        Console.WriteLine($"Received: {msg.Text}");
                        Console.WriteLine($"Message: {msg.Message?.ToJson()}");

                        if (msg.Message?.ReceiverId != null)
                        {
                            foreach (var item in server.Clients)
                            {
                                if (item.Id == msg.Message?.ReceiverId)
                                {
                                    item.SendAsync(new TextMessage(msg.Text));
                                }
                            }
                        }

                        // Broadcast to all clients
                        // await server.BroadcastAsync($"[{ws.Id}]: {msg.Text}");
                    }
                };

                ws.OnClose += (evt) =>
                {
                    Console.WriteLine($"Client disconnected: {ws.Id}");
                };

                ws.OnError += (err) =>
                {
                    Console.WriteLine($"Error from {ws.Id}: {err.Message}");
                };
            };

            server.OnError += (e) =>
            {
                Console.WriteLine($"Server error: {e.Message}");
            };

            server.Listen();



            Console.WriteLine($"WebSocket server listening on {server.Address} port {server.Port}");
            Console.WriteLine("Press any key to stop...");
            Console.ReadKey();

            server.Stop();
        }
    }
}
