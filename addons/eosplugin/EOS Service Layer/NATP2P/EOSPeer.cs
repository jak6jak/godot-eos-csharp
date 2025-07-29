using System;
using System.Runtime.InteropServices;
using EOSPluign.addons.eosplugin;
using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using Godot;

namespace Riptide.Transports.EOS
{
    public abstract class EOSPeer
    {
        /// <summary>The name to use when logging messages via RiptideLogger.</summary>
        public const string LogName = "EOS";

        protected const int MaxMessages = 256;
        protected const byte DefaultSocketId = 0; // Default socket ID for P2P connections
        protected const int MaxPacketSize = 1200; // Standard MTU size to avoid fragmentation

        private readonly byte[] receiveBuffer;
        private int processedMessages = 0;
        protected P2PInterface p2pInterface;

        protected EOSPeer()
        {
            receiveBuffer = new byte[MaxPacketSize];
            
            // Get P2P interface from EOS platform
            if (EOSInterfaceManager.Instance?.Platform != null)
            {
                p2pInterface = EOSInterfaceManager.Instance.Platform.GetP2PInterface();
                GD.Print($"{LogName}: P2P interface initialized successfully");
            }
            else
            {
                GD.PushError($"{LogName}: EOS Platform not initialized!");
            }
        }

    protected void Receive(EOSConnection fromConnection)
        {
            if (p2pInterface == null) return;

            processedMessages = 0; // Reset message counter for this frame

            // Check for incoming packets with rate limiting
            var getNextReceivedPacketSizeOptions = new GetNextReceivedPacketSizeOptions()
            {
                LocalUserId = fromConnection.LocalUserId,
                RequestedChannel = DefaultSocketId
            };

            var result = p2pInterface.GetNextReceivedPacketSize(ref getNextReceivedPacketSizeOptions, out uint nextPacketSizeBytes);
            
            while (result == Result.Success && nextPacketSizeBytes > 0)
            {
                // Rate limiting to prevent flooding
                if (processedMessages >= MaxMessages)
                {
                    GD.PushWarning($"{LogName}: Message limit ({MaxMessages}) exceeded this frame. Dropping remaining packets.");
                    break;
                }

                // Validate packet size
                if (nextPacketSizeBytes > MaxPacketSize)
                {
                    GD.PushWarning($"{LogName}: Packet size {nextPacketSizeBytes} exceeds maximum {MaxPacketSize}. Dropping packet.");
                    
                    // Still need to receive the packet to clear it from the queue
                    var dummyReceiveOptions = new ReceivePacketOptions()
                    {
                        LocalUserId = fromConnection.LocalUserId,
                        MaxDataSizeBytes = (uint)receiveBuffer.Length,
                        RequestedChannel = DefaultSocketId,
                    };
                    
                    ProductUserId dummyRemoteUserId = new ProductUserId();
                    Epic.OnlineServices.P2P.SocketId dummySocketId = new Epic.OnlineServices.P2P.SocketId();
                    p2pInterface.ReceivePacket(ref dummyReceiveOptions, ref dummyRemoteUserId, ref dummySocketId, out var _, receiveBuffer, out uint _);
                    
                    // Check for next packet
                    result = p2pInterface.GetNextReceivedPacketSize(ref getNextReceivedPacketSizeOptions, out nextPacketSizeBytes);
                    continue;
                }

                var receivePacketOptions = new ReceivePacketOptions()
                {
                    LocalUserId = fromConnection.LocalUserId,
                    MaxDataSizeBytes = nextPacketSizeBytes,
                    RequestedChannel = DefaultSocketId,
                };
                
                ProductUserId remoteUserId = new ProductUserId();
                Epic.OnlineServices.P2P.SocketId receivedSocketId = new Epic.OnlineServices.P2P.SocketId();
                
                var receiveResult = p2pInterface.ReceivePacket(
                    ref receivePacketOptions, 
                    ref remoteUserId, 
                    ref receivedSocketId, 
                    out var bytesReceived, 
                    receiveBuffer, 
                    out uint bytesWritten);
                
                if (receiveResult == Result.Success && bytesWritten > 0)
                {
                    OnDataReceived(receiveBuffer, (int)bytesWritten, fromConnection);
                    processedMessages++;
                }
                else if (receiveResult != Result.Success)
                {
                    GD.PushWarning($"{LogName}: Failed to receive packet: {receiveResult}");
                }

                // Check for next packet
                result = p2pInterface.GetNextReceivedPacketSize(ref getNextReceivedPacketSizeOptions, out nextPacketSizeBytes);
            }
        }

        internal void Send(byte[] dataBuffer, int numBytes, ProductUserId remoteUserId, byte channel = DefaultSocketId)
        {
            if (p2pInterface == null)
            {
                GD.PushError($"{LogName}: P2P interface not available for sending data");
                return;
            }

            // Validate data size
            if (numBytes > MaxPacketSize)
            {
                GD.PushError($"{LogName}: Packet size {numBytes} exceeds maximum {MaxPacketSize}. Cannot send.");
                return;
            }

            if (numBytes <= 0 || dataBuffer == null)
            {
                GD.PushWarning($"{LogName}: Invalid data buffer or size for sending");
                return;
            }

            try
            {
                // Create a properly sized data array
                byte[] sendData = new byte[numBytes];
                Array.Copy(dataBuffer, sendData, numBytes);

                var sendPacketOptions = new SendPacketOptions()
                {
                    LocalUserId = EOSInterfaceManager.Instance.ConnectService.GetProductUserId(),
                    RemoteUserId = remoteUserId,
                    SocketId = new Epic.OnlineServices.P2P.SocketId() { SocketName = "RiptideSocket" },
                    Channel = channel,
                    AllowDelayedDelivery = true,
                    Data = sendData,
                    Reliability = PacketReliability.ReliableOrdered,
                };

                var result = p2pInterface.SendPacket(ref sendPacketOptions);
                if (result != Result.Success)
                {
                    GD.PushWarning($"{LogName}: Failed to send {numBytes} bytes - {result}");
                }
                else
                {
                    GD.Print($"{LogName}: Successfully sent {numBytes} bytes to {remoteUserId}");
                }
            }
            catch (Exception ex)
            {
                GD.PushError($"{LogName}: Exception while sending data: {ex.Message}");
            }
        }

        protected abstract void OnDataReceived(byte[] dataBuffer, int amount, EOSConnection fromConnection);
    }
}

