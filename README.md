# Sokio
web socket implementation in c#
A C# WebSocket implementation built on top of TcpListener, created for a university assignment.

## Overview

This project implements the WebSocket protocol (RFC 6455) from scratch using .NET's TcpListener for low-level TCP socket handling. It provides both server and client functionality for WebSocket communication.

## Features

- Custom WebSocket protocol implementation
- Built on TcpListener for educational purposes
- WebSocket handshake handling (HTTP upgrade)
- Frame parsing and construction
- Text and binary message support
- Connection management

## Usage

Code examples and documentation will be added as development progresses.

## Architecture

This implementation follows the WebSocket protocol specification (RFC 6455) and includes:

- HTTP to WebSocket upgrade handshake
- WebSocket frame parsing and generation
- Opcode handling (text, binary, close, ping, pong)
- Masking/unmasking for client-server communication

## Development Status

This project is actively being developed as part of a university assignment. Features and API may change as development progresses.

## Dependencies

- System.Net.Sockets (TcpListener)
- System.Text (for UTF-8 encoding)
- System.Security.Cryptography (for handshake hashing)

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Created as part of a university assignment
- Built from scratch using TcpListener to understand WebSocket protocol internals
- Implements WebSocket specification RFC 6455

**Note**: This is an educational implementation of the WebSocket protocol built for learning purposes as part of a university assignment.
