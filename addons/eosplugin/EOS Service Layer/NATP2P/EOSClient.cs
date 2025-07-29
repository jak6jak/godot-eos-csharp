// EOSClient.cs - Client implementation for EOS P2P
using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using System;
using System.Threading.Tasks;
using EOSPluign.addons.eosplugin;
using Godot;

namespace Riptide.Transports.EOS
{
    public class EOSClient : EOSPeer, IClient
    {
        public event EventHandler Connected;
        public event EventHandler ConnectionFailed;
        public event EventHandler<DataReceivedEventArgs> DataReceived;
        public event EventHandler<DisconnectedEventArgs> Disconnected;

        private const string LocalHostName = "localhost";
        private const string LocalHostIP = "127.0.0.1";

        private EOSConnection eosConnection;
        private EOSServer localServer;
        private ProductUserId localUserId;
        private ulong connectionClosedNotification;

        public EOSClient(EOSServer localServer = null)
        {
            this.localServer = localServer;
        }

        public void ChangeLocalServer(EOSServer newLocalServer)
        {
            localServer = newLocalServer;
        }

        public bool Connect(string hostAddress, out Connection connection, out string connectError)
        {
            connection = null;
            connectError = "";

            GD.Print($"{LogName}: Attempting to connect to {hostAddress}...");

            if (p2pInterface == null)
            {
                connectError = "EOS P2P interface not available";
                GD.PushError($"{LogName}: {connectError}");
                return false;
            }

            GD.Print($"{LogName}: P2P interface available, getting local user ID...");

            // Get local user ID
            localUserId = EOSInterfaceManager.Instance.ConnectService.GetProductUserId();
            if (localUserId == null)
            {
                connectError = "Local user ID not available. Make sure user is logged in via Connect service.";
                GD.PushError($"{LogName}: {connectError}");
                return false;
            }

            GD.Print($"{LogName}: Local user ID obtained: {localUserId}");

            ProductUserId targetUserId = null;

            // Handle localhost connections
            if (hostAddress == LocalHostIP || hostAddress == LocalHostName)
            {
                GD.Print($"{LogName}: Connecting to localhost...");
                if (localServer == null)
                {
                    connectError = $"No locally running server specified. Pass an {nameof(EOSServer)} instance to your {nameof(EOSClient)}'s constructor or call {nameof(ChangeLocalServer)} before connecting locally.";
                    GD.PushError($"{LogName}: {connectError}");
                    return false;
                }

                // For local connections, use the same user ID
                targetUserId = localUserId;
                GD.Print($"{LogName}: Using local user ID as target: {targetUserId}");
            }
            else
            {
                GD.Print($"{LogName}: Remote connection requested, but remote connection logic not implemented");
            }

            // Set up connection closed notification
            GD.Print($"{LogName}: Setting up connection closed notification...");
            var addNotifyClosedOptions = new AddNotifyPeerConnectionClosedOptions()
            {
                LocalUserId = localUserId,
                SocketId = new SocketId() { SocketName = "RiptideSocket" }
            };

            connectionClosedNotification = p2pInterface.AddNotifyPeerConnectionClosed(ref addNotifyClosedOptions, null, OnConnectionClosed);
            GD.Print($"{LogName}: Connection closed notification ID: {connectionClosedNotification}");

            // Create connection object
            GD.Print($"{LogName}: Creating EOS connection object...");
            eosConnection = new EOSConnection(targetUserId, localUserId, this);

            // For localhost, directly add to local server
            if (hostAddress == LocalHostIP || hostAddress == LocalHostName)
            {
                GD.Print($"{LogName}: Adding connection to local server...");
                localServer.Add(eosConnection);
                connection = eosConnection;
                GD.Print($"{LogName}: Localhost connection established successfully");
                OnConnected();
                return true;
            }

            // For remote connections, send a connection request
            var sendPacketOptions = new SendPacketOptions()
            {
                LocalUserId = localUserId,
                RemoteUserId = targetUserId,
                SocketId = new SocketId() { SocketName = "RiptideSocket" },
                Channel = DefaultSocketId,
                Data = null,
                AllowDelayedDelivery = false,
                Reliability = PacketReliability.ReliableOrdered
            };

            var result = p2pInterface.SendPacket(ref sendPacketOptions);
            if (result != Result.Success)
            {
                connectError = $"Failed to send connection request: {result}";
                return false;
            }

            connection = eosConnection;
            
            // Connection success will be determined by the server's response
            // For now, we'll assume success and let the polling handle the rest
            Task.Run(async () =>
            {
                await Task.Delay(100); // Small delay to allow connection to establish
                OnConnected();
            });

            return true;
        }

        private void OnConnectionClosed(ref OnRemoteConnectionClosedInfo data)
        {
            GD.Print($"{LogName}: Connection closed with {data.RemoteUserId}, reason: {data.Reason}");
            
            if (eosConnection != null && data.RemoteUserId.ToString() == eosConnection.RemoteUserId.ToString())
            {
                DisconnectReason reason = data.Reason switch
                {
                    ConnectionClosedReason.ClosedByLocalUser => DisconnectReason.Disconnected,
                    ConnectionClosedReason.ClosedByPeer => DisconnectReason.Disconnected,
                    ConnectionClosedReason.TimedOut => DisconnectReason.TimedOut,
                    ConnectionClosedReason.TooManyConnections => DisconnectReason.TransportError,
                    ConnectionClosedReason.InvalidMessage => DisconnectReason.TransportError,
                    ConnectionClosedReason.InvalidData => DisconnectReason.TransportError,
                    ConnectionClosedReason.ConnectionFailed => DisconnectReason.TransportError,
                    ConnectionClosedReason.ConnectionClosed => DisconnectReason.Disconnected,
                    ConnectionClosedReason.NegotiationFailed => DisconnectReason.TransportError,
                    ConnectionClosedReason.UnexpectedError => DisconnectReason.TransportError,
                    _ => DisconnectReason.TransportError
                };

                OnDisconnected(reason);
            }
        }

        public void Disconnect()
        {
            if (eosConnection != null && p2pInterface != null)
            {
                var closeOptions = new CloseConnectionOptions()
                {
                    LocalUserId = localUserId,
                    RemoteUserId = eosConnection.RemoteUserId,
                    SocketId = new SocketId() { SocketName = "RiptideSocket" }
                };

                p2pInterface.CloseConnection(ref closeOptions);

                // Clean up notification
                if (connectionClosedNotification != 0)
                {
                    p2pInterface.RemoveNotifyPeerConnectionClosed(connectionClosedNotification);
                    connectionClosedNotification = 0;
                }

                eosConnection = null;
            }
        }

        public void Poll()
        {
            if (eosConnection != null)
                Receive(eosConnection);
        }

        protected override void OnDataReceived(byte[] dataBuffer, int amount, EOSConnection fromConnection)
        {
            DataReceived?.Invoke(this, new DataReceivedEventArgs(dataBuffer, amount,fromConnection));
        }

        private void OnConnected()
        {
            GD.Print($"{LogName}: Connected to server");
            Connected?.Invoke(this, EventArgs.Empty);
        }

        private void OnConnectionFailed()
        {
            GD.Print($"{LogName}: Connection failed");
            ConnectionFailed?.Invoke(this, EventArgs.Empty);
        }

        private void OnDisconnected(DisconnectReason reason)
        {
            GD.Print($"{LogName}: Disconnected from server: {reason}");
            Disconnected?.Invoke(this, new DisconnectedEventArgs(eosConnection, reason));
        }
    }
}
