using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using System;
using System.Collections.Generic;
using EOSPluign.addons.eosplugin;
using Godot;

namespace Riptide.Transports.EOS
{
    public class EOSServer : EOSPeer, IServer
    {
        public event EventHandler<ConnectedEventArgs> Connected;
        public event EventHandler<DataReceivedEventArgs> DataReceived;
        public event EventHandler<DisconnectedEventArgs> Disconnected;

        public void Shutdown()
        {
            throw new NotImplementedException();
        }

        public ushort Port { get; private set; }

        private Dictionary<string, EOSConnection> connections;
        private ProductUserId localUserId;
        private ulong connectionRequestNotification;
        private ulong connectionClosedNotification;
        private ulong incomingConnectionNotification;

        public void Start(ushort port)
        {
            Port = port;
            connections = new Dictionary<string, EOSConnection>();

            GD.Print($"{LogName}: Starting server on port {port}...");

            if (p2pInterface == null)
            {
                GD.PushError($"{LogName}: P2P interface not available");
                return;
            }

            GD.Print($"{LogName}: P2P interface available, getting local user ID...");

            // Get local user ID from Connect service
            localUserId = EOSInterfaceManager.Instance.ConnectService.GetProductUserId();
            if (localUserId == null)
            {
                GD.PushError($"{LogName}: Local user ID not available. Make sure user is logged in via Connect service.");
                return;
            }

            GD.Print($"{LogName}: Local user ID obtained: {localUserId}");

            // Set up connection request notifications
            GD.Print($"{LogName}: Setting up connection request notifications...");
            var addNotifyOptions = new AddNotifyPeerConnectionRequestOptions()
            {
                LocalUserId = localUserId,
                SocketId = new SocketId() { SocketName = "RiptideSocket" }
            };

            connectionRequestNotification = p2pInterface.AddNotifyPeerConnectionRequest(ref addNotifyOptions, null, OnConnectionRequested);
            GD.Print($"{LogName}: Connection request notification ID: {connectionRequestNotification}");

            // Set up connection closed notifications
            GD.Print($"{LogName}: Setting up connection closed notifications...");
            var addNotifyClosedOptions = new AddNotifyPeerConnectionClosedOptions()
            {
                LocalUserId = localUserId,
                SocketId = new SocketId() { SocketName = "RiptideSocket" }
            };

            connectionClosedNotification = p2pInterface.AddNotifyPeerConnectionClosed(ref addNotifyClosedOptions, null, OnConnectionClosed);
            GD.Print($"{LogName}: Connection closed notification ID: {connectionClosedNotification}");

            // Set up incoming connection notifications
            GD.Print($"{LogName}: Setting up incoming connection notifications...");
            var addNotifyIncomingOptions = new AddNotifyPeerConnectionRequestOptions()
            {
                LocalUserId = localUserId,
                SocketId = new SocketId() { SocketName = "RiptideSocket" }
            };

            incomingConnectionNotification = p2pInterface.AddNotifyPeerConnectionRequest(ref addNotifyIncomingOptions, null, OnIncomingConnectionRequest);
            GD.Print($"{LogName}: Incoming connection notification ID: {incomingConnectionNotification}");

            GD.Print($"{LogName}: Server successfully started on port {port} with {connections.Count} active connections");
        }

        public void Stop()
        {
            if (p2pInterface != null)
            {
                // Clean up notifications
                if (connectionRequestNotification != 0)
                {
                    p2pInterface.RemoveNotifyPeerConnectionRequest(connectionRequestNotification);
                    connectionRequestNotification=0;
                }

                if (connectionClosedNotification != 0)
                {
                    p2pInterface.RemoveNotifyPeerConnectionClosed(connectionClosedNotification);
                    connectionClosedNotification = 0;
                }

                if (incomingConnectionNotification != 0)
                {
                    p2pInterface.RemoveNotifyPeerConnectionRequest(incomingConnectionNotification);
                    incomingConnectionNotification = 0;
                }

                // Close all connections (only if connections was initialized)
                if (connections != null)
                {
                    foreach (var connection in connections.Values)
                    {
                        Close(connection);
                    }
                    connections.Clear();
                }
            }

            GD.Print($"{LogName}: Server stopped");
        }

        private void OnConnectionRequested(ref OnIncomingConnectionRequestInfo data)
        {
            GD.Print($"{LogName}: Connection requested from {data.RemoteUserId}");
            
            // Accept the connection
            var acceptOptions = new AcceptConnectionOptions()
            {
                LocalUserId = localUserId,
                RemoteUserId = data.RemoteUserId,
                SocketId = data.SocketId
            };

            var result = p2pInterface.AcceptConnection(ref acceptOptions);
            if (result != Result.Success)
            {
                GD.PushWarning($"{LogName}: Failed to accept connection from {data.RemoteUserId}: {result}");
                return;
            }

            // Create connection object
            var connection = new EOSConnection(data.RemoteUserId, localUserId, this);
            string userIdString = data.RemoteUserId.ToString();
            
            if (!connections.ContainsKey(userIdString))
            {
                connections.Add(userIdString, connection);
                OnConnected(connection);
            }
            else
            {
                GD.Print($"{LogName}: Connection from {data.RemoteUserId} already exists");
            }
        }

        private void OnIncomingConnectionRequest(ref OnIncomingConnectionRequestInfo data)
        {
            GD.Print($"{LogName}: Incoming connection request from {data.RemoteUserId}");
            // This is automatically handled by OnConnectionRequested
        }

        private void OnConnectionClosed(ref OnRemoteConnectionClosedInfo data)
        {
            GD.Print($"{LogName}: Connection closed with {data.RemoteUserId}, reason: {data.Reason}");
            
            string userIdString = data.RemoteUserId.ToString();
            if (connections.TryGetValue(userIdString, out EOSConnection connection))
            {
                connections.Remove(userIdString);
                
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

                OnDisconnected(connection, reason);
            }
        }

        internal void Add(EOSConnection connection)
        {
            string userIdString = connection.RemoteUserId.ToString();
            if (!connections.ContainsKey(userIdString))
            {
                connections.Add(userIdString, connection);
                OnConnected(connection);
            }
            else
            {
                GD.Print($"{LogName}: Connection from {connection.RemoteUserId} could not be accepted: Already connected");
            }
        }

        public void Close(Connection connection)
        {
            if (connection is EOSConnection eosConnection)
            {
                var closeOptions = new CloseConnectionOptions()
                {
                    LocalUserId = localUserId,
                    RemoteUserId = eosConnection.RemoteUserId,
                    SocketId = new SocketId() { SocketName = "RiptideSocket" }
                };

                p2pInterface.CloseConnection(ref closeOptions);
                connections.Remove(eosConnection.RemoteUserId.ToString());
            }
        }

        public void Poll()
        {
            if (connections == null) return;
            
            foreach (EOSConnection connection in connections.Values)
                Receive(connection);
        }

        protected override void OnDataReceived(byte[] dataBuffer, int amount, EOSConnection fromConnection)
        {
            DataReceived?.Invoke(this, new DataReceivedEventArgs(dataBuffer, amount,fromConnection));
        }

        private void OnConnected(EOSConnection connection)
        {
            GD.Print($"{LogName}: Client {connection.RemoteUserId} connected");
            Connected?.Invoke(this, new ConnectedEventArgs(connection));
        }

        private void OnDisconnected(EOSConnection connection, DisconnectReason reason)
        {
            GD.Print($"{LogName}: Client {connection.RemoteUserId} disconnected: {reason}");
            Disconnected?.Invoke(this, new DisconnectedEventArgs(connection, reason));
        }
    }
}