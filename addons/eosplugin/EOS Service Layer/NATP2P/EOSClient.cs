using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using System;
using System.Threading.Tasks;
using EOSPluign.addons.eosplugin;
using Godot;

namespace Riptide.Transports.EOS
{
    /// <summary>
    /// EOS P2P client transport for Riptide networking
    /// </summary>
    public class EOSClient : EOSPeer, IClient
    {
        #region Events (Required by IClient)
        public event EventHandler Connected;
        public event EventHandler ConnectionFailed;
        public event EventHandler<DataReceivedEventArgs> DataReceived;
        public event EventHandler<DisconnectedEventArgs> Disconnected;
        #endregion

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

        /// <summary>
        /// Attempts to connect to an EOS P2P host
        /// </summary>
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

            // Get local user ID
            localUserId = EOSInterfaceManager.Instance.ConnectService.GetProductUserId();
            if (localUserId == null)
            {
                connectError = "Local user ID not available. Make sure user is logged in via Connect service.";
                GD.PushError($"{LogName}: {connectError}");
                return false;
            }

            // Start async connection process
            _ = ConnectAsync(hostAddress);
            
            // Create connection object for Riptide (will be updated when connection succeeds)
            var placeholderUserId = ProductUserId.FromString("connecting");
            eosConnection = new EOSConnection(placeholderUserId, localUserId, this);
            connection = eosConnection;
            
            return true;
        }

        private async Task<bool> ConnectAsync(string hostAddress)
        {
            try
            {
                ProductUserId targetUserId = await ResolveTargetUserId(hostAddress);
                if (targetUserId == null)
                {
                    OnConnectionFailed();
                    return false;
                }

                // Set up connection closed notification
                var addNotifyClosedOptions = new AddNotifyPeerConnectionClosedOptions()
                {
                    LocalUserId = localUserId,
                    SocketId = new SocketId() { SocketName = "RiptideSocket" }
                };

                connectionClosedNotification = p2pInterface.AddNotifyPeerConnectionClosed(ref addNotifyClosedOptions, null, OnConnectionClosed);

                // Update connection with actual target user ID
                eosConnection = new EOSConnection(targetUserId, localUserId, this);

                // Handle localhost connections
                if (hostAddress == LocalHostIP || hostAddress == LocalHostName)
                {
                    if (localServer == null)
                    {
                        GD.PushError($"{LogName}: No local server available for localhost connection");
                        OnConnectionFailed();
                        return false;
                    }
                    
                    localServer.AddConnection(eosConnection);
                    GD.Print($"{LogName}: Localhost connection established");
                    OnConnected();
                    return true;
                }

                // For remote connections, just signal success - let Riptide handle the rest
                GD.Print($"{LogName}: EOS P2P connection established to {targetUserId}");
                OnConnected();
                return true;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"{LogName}: Exception during connection: {ex.Message}");
                OnConnectionFailed();
                return false;
            }
        }

        private async Task<ProductUserId> ResolveTargetUserId(string hostAddress)
        {
            // Handle localhost connections
            if (hostAddress == LocalHostIP || hostAddress == LocalHostName)
            {
                GD.Print($"{LogName}: Connecting to localhost...");
                return localUserId; // For local connections, use the same user ID
            }

            // Handle remote connections - TODO: Implement proper ProductUserId resolution
            // This requires EOS Friends API or lobby system integration
            GD.PrintErr($"{LogName}: Remote ProductUserId resolution not implemented for: {hostAddress}");
            GD.PrintErr($"{LogName}: Remote connections require EOS Friends API or lobby system to discover ProductUserId");
            return null;
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

        /// <summary>
        /// Disconnects from the server
        /// </summary>
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

        /// <summary>
        /// Polls for incoming messages - required by IPeer interface
        /// </summary>
        public void Poll()
        {
            if (eosConnection != null)
                Receive(eosConnection);
        }

        /// <summary>
        /// Handles received data from EOS P2P - passes to Riptide
        /// </summary>
        protected override void OnDataReceived(byte[] dataBuffer, int amount, EOSConnection fromConnection)
        {
            DataReceived?.Invoke(this, new DataReceivedEventArgs(dataBuffer, amount, fromConnection));
        }

        #region Event Helpers
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
        #endregion
    }
}
