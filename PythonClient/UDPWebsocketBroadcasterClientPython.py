import asyncio
import websockets
import datetime
import time

import socket



class UdpSender:
    def __init__(self, ip, port):
        self.ip = ip
        self.port = port
        self.socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

    def send_message(self, message):
        message_bytes = message.encode('utf-8')
        self.socket.sendto(message_bytes, (self.ip, self.port))

    def close(self):
        self.socket.close()

udp_sender = UdpSender("localhost", 12345)


async def connect_and_run():
    while True:
        uri = "ws://localhost:8080/"
        async with websockets.connect(uri) as websocket:
            print(f"Connected to server: {uri}")

            # Start a background task to receive messages from the server
            asyncio.create_task(receive_messages(websocket))

            # Send a message to the server every 3 seconds
            while websocket.open:
                message = f"Client message at {datetime.datetime.now()}"
                await websocket.send(message)

                # Wait for 3 seconds before sending the next message
                await asyncio.sleep(3)

        # Handle reconnection logic
        print("Reconnecting in 5 seconds...")
        await asyncio.sleep(5)

async def receive_messages(websocket):
    try:
        while websocket.open:
            received_message = await websocket.recv()
            print(f"Received message from server: {received_message}")
    except websockets.exceptions.ConnectionClosedError:
        # Handle reconnection logic
        print("Reconnecting in 5 seconds...")

if __name__ == "__main__":
    asyncio.run(connect_and_run())
