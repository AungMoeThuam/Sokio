# Sokio WebSocket Library

A lightweight, feature-rich C# WebSocket library for building real-time applications with support for text messaging, binary file transfers, and persistent storage.

## 🚀 Quick Start

### Installation

Install the package via NuGet Package Manager:

```bash
dotnet add package Sokio --version 1.0.3
```

Or via Package Manager Console in Visual Studio:
```powershell
Install-Package Sokio -Version 1.0.3
```

Or add it directly to your `.csproj` file:
```xml
<PackageReference Include="Sokio" Version="1.0.3" />
```


## ✨ Features

- 🚀 **High Performance**: Asynchronous WebSocket server and client implementations
- 💬 **Structured Messaging**: Built-in support for text and binary message types with metadata
- 📁 **File Persistence**: Automatic binary file storage and retrieval
- 🎯 **Targeted Messaging**: Send messages to specific clients or broadcast to all
- 🔄 **Event-Driven**: Comprehensive event handling for connections, messages, and errors
- 📦 **Zero Dependencies**: Pure C# implementation using only .NET standard libraries

## 🚀 Quick Start

### Installation

Add the Sokio namespace to your project and include the source files.

### Creating a WebSocket Server

```csharp
using Sokio;

// Create server on port 8080
var server = new WebSocketServer(8080);

// Optional: Enable file persistence
server.SetPersistenceBinaryFile(new BinaryFileStore("./uploads"));

// Handle new connections
server.OnConnection += async (e) =>
{
    var client = e.WebSocket;
    Console.WriteLine($"Client connected: {client.Id}");
    
    // Send welcome message
    await client.SendAsync(new TextMessage("Welcome to the server!"));
    
    // Handle messages from this client
    client.OnMessage += async (msg) =>
    {
        if (msg.IsText)
        {
            Console.WriteLine($"Received: {msg.Text}");
            // Echo back to sender
            await client.SendAsync(new TextMessage($"Echo: {msg.Text}"));
        }
    };
    
    client.OnClose += (e) => Console.WriteLine($"Client {client.Id} disconnected");
};

// Start listening
server.Listen();
Console.WriteLine($"WebSocket server running on port {server.Port}");
```

### Creating a WebSocket Client

```csharp
using Sokio;

// Connect to server
var client = new WebSocket("ws://localhost:8080");

client.OnOpen += (e) => Console.WriteLine("Connected to server");

client.OnMessage += (msg) =>
{
    if (msg.IsText)
        Console.WriteLine($"Server says: {msg.Text}");
};

client.OnClose += (e) => Console.WriteLine("Disconnected from server");

// Connect and send message
await client.ConnectAsync();
await client.SendAsync("Hello Server!");
```

## 📖 Core Concepts

### Message Types

**Text Messages** - Structured text communication with metadata:
```csharp
// Simple text message
var message = new TextMessage("Hello World!");

// Private message to specific client
var privateMsg = new TextMessage("Secret message", receiverId: "client-123");

await socket.SendAsync(message);
```

**Binary Messages** - File transfers with automatic persistence:
```csharp
// Send a file
byte[] fileData = File.ReadAllBytes("document.pdf");
var fileMsg = new BinaryMessage("document.pdf", fileData);

await socket.SendAsync(fileMsg);
```

### Broadcasting

Send messages to all connected clients:
```csharp
// Broadcast text to everyone
await server.BroadcastAsync("Server announcement!");

// Broadcast binary data
byte[] data = File.ReadAllBytes("update.zip");
await server.BroadcastAsync(data);
```

### File Persistence

Automatically save received files to disk:
```csharp
// Set up file storage
var fileStore = new BinaryFileStore("./received_files");
server.SetPersistenceBinaryFile(fileStore);

// Files are automatically saved when binary messages are received
```

## 🛠️ Examples

### Chat Server

```csharp
var server = new WebSocketServer(8080);

server.OnConnection += async (e) =>
{
    var client = e.WebSocket;
    
    // Announce new user
    await server.BroadcastAsync($"User {client.Id} joined the chat");
    
    client.OnMessage += async (msg) =>
    {
        if (msg.IsText && msg.Message is TextMessage textMsg)
        {
            // Broadcast to all other clients
            var chatMsg = $"[{client.Id}]: {textMsg.Content}";
            
            foreach (var otherClient in server.Clients)
            {
                if (otherClient.Id != client.Id)
                    await otherClient.SendAsync(chatMsg);
            }
        }
    };
    
    client.OnClose += async (e) =>
    {
        await server.BroadcastAsync($"User {client.Id} left the chat");
    };
};

server.Listen();
```

### File Transfer Server

```csharp
var server = new WebSocketServer(8080);
server.SetPersistenceBinaryFile(new BinaryFileStore("./uploads"));

server.OnConnection += (e) =>
{
    var client = e.WebSocket;
    
    client.OnMessage += async (msg) =>
    {
        if (msg.Message is BinaryMessage fileMsg)
        {
            Console.WriteLine($"Received file: {fileMsg.FileName} ({fileMsg.RawData.Length} bytes)");
            
            // Confirm receipt
            await client.SendAsync(new TextMessage($"File '{fileMsg.FileName}' uploaded successfully!"));
        }
    };
};

server.Listen();
```

### Private Messaging

```csharp
server.OnConnection += (e) =>
{
    var client = e.WebSocket;
    
    client.OnMessage += async (msg) =>
    {
        if (msg.Message?.ReceiverId != null)
        {
            // Find target client
            var target = server.Clients.FirstOrDefault(c => c.Id == msg.Message.ReceiverId);
            
            if (target != null)
                await target.SendAsync(msg.Message);
            else
                await client.SendAsync(new TextMessage("User not found"));
        }
    };
};

// Client sends private message
var privateMsg = new TextMessage("Hi there!", receiverId: "target-client-id");
await client.SendAsync(privateMsg);
```

## 📚 API Overview

### Server API

```csharp
var server = new WebSocketServer(port);

// Events
server.OnConnection += (ConnectionEventArgs e) => { };
server.OnError += (ErrorEventArgs e) => { };

// Methods
server.Listen();                              // Start listening
server.Stop();                               // Stop server
server.BroadcastAsync(string message);       // Broadcast text
server.BroadcastAsync(byte[] data);          // Broadcast binary
server.SetPersistenceBinaryFile(IPersistence); // Enable file storage

// Properties
server.Clients;                              // Connected clients
server.Port;                                 // Server port
server.Address;                              // Server address
```

### Client API

```csharp
var client = new WebSocket(url);

// Events
client.OnOpen += (EventArgs e) => { };
client.OnMessage += (MessageEventArgs e) => { };
client.OnClose += (EventArgs e) => { };
client.OnError += (ErrorEventArgs e) => { };

// Methods
await client.ConnectAsync();                 // Connect to server
await client.SendAsync(string message);     // Send text
await client.SendAsync(byte[] data);        // Send binary
await client.SendAsync(Message message);    // Send structured message
await client.CloseAsync();                  // Close connection

// Properties
client.Id;                                   // Unique client ID
client.IsConnected;                          // Connection status
```

### Message API

```csharp
// Text Message
var textMsg = new TextMessage(content, receiverId?, senderId?);
textMsg.Content;        // Message text
textMsg.SenderId;       // Sender ID
textMsg.ReceiverId;     // Target receiver (null = broadcast)
textMsg.Timestamp;      // Creation time
textMsg.ToJson();       // Serialize to JSON

// Binary Message
var binaryMsg = new BinaryMessage(fileName, data, receiverId?, senderId?);
binaryMsg.FileName;     // File name
binaryMsg.RawData;      // Binary data
binaryMsg.SenderId;     // Sender ID
binaryMsg.ReceiverId;   // Target receiver (null = broadcast)
binaryMsg.Timestamp;    // Creation time
binaryMsg.ToBytes();    // Serialize to bytes
```

## 🔧 Advanced Usage

### Custom Persistence

Implement your own storage solution:

```csharp
public class DatabasePersistence : IPersistence
{
    public Tuple<string, byte[]> readBinaryFile(string fileName)
    {
        // Read from database
        var data = database.GetFile(fileName);
        return new Tuple<string, byte[]>(fileName, data);
    }
    
    public void writeBinaryFile(string fileName, byte[] binaryFile)
    {
        // Save to database
        database.SaveFile(fileName, binaryFile);
    }
}

server.SetPersistenceBinaryFile(new DatabasePersistence());
```

### Error Handling

```csharp
server.OnError += (e) =>
{
    Console.WriteLine($"Server error: {e.Message}");
    Console.WriteLine($"Exception: {e.Exception}");
};

client.OnError += (e) =>
{
    Console.WriteLine($"Client error: {e.Message}");
    // Attempt reconnection
    await Task.Delay(5000);
    await client.ConnectAsync();
};
```

### Message Filtering

```csharp
client.OnMessage += (msg) =>
{
    if (msg.Message is TextMessage textMsg)
    {
        switch (textMsg.Content)
        {
            case "ping":
                await client.SendAsync(new TextMessage("pong"));
                break;
            case "quit":
                await client.CloseAsync();
                break;
            default:
                Console.WriteLine($"Message: {textMsg.Content}");
                break;
        }
    }
};
```

## 🏗️ Architecture

SokioD follows a layered architecture:

- **Transport Layer**: Raw TCP/WebSocket protocol handling
- **Frame Layer**: WebSocket frame parsing and creation
- **Message Layer**: Structured message types with serialization
- **Application Layer**: High-level server/client APIs
- **Persistence Layer**: Optional file storage capabilities

- ✅ Multiple concurrent connections supported
- ✅ File persistence operations
- ✅ Automatic resource cleanup
- ⚠️ Event handlers may execute on different threads

## 📋 Requirements

- .NET Standard 2.0 or later
- C# 7.0 or later

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.


**Built using C# and WebSocket protocol**
