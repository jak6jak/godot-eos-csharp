using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using EOSPluign.addons.eosplugin;
using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using Godot;

namespace Riptide.Transports.EOS
{
    /// <summary>
    /// Base class for EOS P2P transport layer - provides raw EOS networking for Riptide
    /// </summary>
    public abstract class EOSPeer
    {
        /// <summary>The name to use when logging messages via RiptideLogger.</summary>
        public const string LogName = "EOS";

        protected const byte DefaultSocketId = 0; // Default socket ID for P2P connections
        protected const int MaxPacketSize = 1170; // EOS P2P actual limit

        protected P2PInterface p2pInterface;

        protected EOSPeer()
        {
            // Configure Riptide for EOS P2P packet size limits
            // EOS P2P max packet size is 1170 bytes, reserve some for Riptide headers
            Message.MaxPayloadSize = 1100; // Safe margin under EOS 1170-byte limit
            GD.Print($"{LogName}: Set Riptide MaxPayloadSize to {Message.MaxPayloadSize} bytes for EOS compatibility");
            
            // Get P2P interface from EOS platform
            if (EOSInterfaceManager.Instance?.Platform != null)
            {
                p2pInterface = EOSInterfaceManager.Instance.Platform.GetP2PInterface();
                GD.Print($"{LogName}: EOS P2P interface initialized successfully");
            }
            else
            {
                GD.PushError($"{LogName}: EOS Platform not initialized!");
            }
        }

        /// <summary>
        /// Sends raw data through EOS P2P to the specified user
        /// </summary>
        internal void Send(byte[] dataBuffer, int numBytes, ProductUserId remoteUserId, byte channel = DefaultSocketId)
        {
            if (p2pInterface == null)
            {
                GD.PushError($"{LogName}: P2P interface not available for sending data");
                return;
            }

            if (numBytes <= 0 || dataBuffer == null || numBytes > dataBuffer.Length)
            {
                GD.PushWarning($"{LogName}: Invalid data buffer or size for sending");
                return;
            }

            try
            {
                // Create properly sized data array
                byte[] sendData = new byte[numBytes];
                Array.Copy(dataBuffer, 0, sendData, 0, numBytes);

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
            }
            catch (Exception ex)
            {
                GD.PushError($"{LogName}: Exception while sending packet: {ex.Message}");
            }
        }

        /// <summary>
        /// Receives raw data from EOS P2P for the specified connection
        /// </summary>
        protected void Receive(EOSConnection connection)
        {
            if (p2pInterface == null || connection == null) return;

            var getNextReceivedPacketSizeOptions = new GetNextReceivedPacketSizeOptions()
            {
                LocalUserId = connection.LocalUserId,
                RequestedChannel = DefaultSocketId
            };

            var result = p2pInterface.GetNextReceivedPacketSize(ref getNextReceivedPacketSizeOptions, out uint nextPacketSizeBytes);
            
            while (result == Result.Success && nextPacketSizeBytes > 0 && nextPacketSizeBytes <= MaxPacketSize)
            {
                var buffer = new byte[nextPacketSizeBytes];
                
                var receivePacketOptions = new ReceivePacketOptions()
                {
                    LocalUserId = connection.LocalUserId,
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
                    buffer, 
                    out uint bytesWritten);
                
                if (receiveResult == Result.Success && bytesWritten > 0)
                {
                    // Pass all data directly to Riptide - no custom processing
                    OnDataReceived(buffer, (int)bytesWritten, connection);
                }

                // Check for next packet
                result = p2pInterface.GetNextReceivedPacketSize(ref getNextReceivedPacketSizeOptions, out nextPacketSizeBytes);
            }
        }

        /// <summary>
        /// Abstract method for handling received data - implemented by EOSClient and EOSServer
        /// </summary>
        protected abstract void OnDataReceived(byte[] dataBuffer, int amount, EOSConnection fromConnection);
    }
}

