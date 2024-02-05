import asyncio
import websockets

async def handle_message(websocket, path):
    # This function will be called whenever a message is received
    message = await websocket.recv()
    print(f"Received message: {message}")

async def start_websocket_server():
    # Start a WebSocket server on port 7072
    server = await websockets.serve(handle_message, "127.0.0.1", 7070)
    print(f"WebSocket server started on port {server.sockets[0].getsockname()[1]}")

    # Keep the server running
    await server.wait_closed()

if __name__ == "__main__":
    # Run the WebSocket server
    asyncio.run(start_websocket_server())
