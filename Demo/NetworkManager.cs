using Riptide.Utils;
using System;
using EOSPluign.Demo;
using Godot;
using Riptide.Transports.EOS;

namespace Riptide.Demos.Steam.PlayerHosted
{
    public enum ServerToClientId : ushort
    {
        SpawnPlayer = 1,
        PlayerMovement,
    }
    
    public enum ClientToServerId : ushort
    {
        PlayerName = 1,
        PlayerInput,
    }

    public partial class NetworkManager : Node
    {
        public const byte PlayerHostedDemoMessageHandlerGroupId = 255;

        private static NetworkManager _singleton;
        public static NetworkManager Singleton
        {
            get => _singleton;
            private set
            {
                if (_singleton == null)
                    _singleton = value;
                else if (_singleton != value)
                {
                    GD.Print($"{nameof(NetworkManager)} instance already exists, destroying object!");
                    value?.QueueFree();
                }
            }
        }

        [Export] private PackedScene serverPlayerPrefab;
        [Export] private PackedScene playerPrefab;
        [Export] private PackedScene localPlayerPrefab;

        public PackedScene ServerPlayerPrefab => serverPlayerPrefab;
        public PackedScene PlayerPrefab => playerPrefab;
        public PackedScene LocalPlayerPrefab => localPlayerPrefab;
        
        internal Server Server { get; private set; }
        internal Client Client { get; private set; }

        public override void _Ready()
        {
            Singleton = this;
            
            // Add to group for fallback discovery
            AddToGroup("NetworkManager");
            
            // Make this node persistent across scene changes
            SetProcessMode(ProcessModeEnum.Always);
            
            // Start EOS login process
            StartEOSLogin();
            
            InitializeNetworking();
        }
        
        private void StartEOSLogin()
        {
            // Ensure EOS is ready and attempt login
            var eosManager = EOSPluign.addons.eosplugin.EOSInterfaceManager.Instance;
            if (eosManager != null)
            {
                GD.Print("Starting EOS authentication...");
                eosManager.AuthService?.SmartLogin();
                eosManager.ConnectService?.Login();
            }
            else
            {
                GD.PrintErr("EOS Interface Manager not found!");
            }
        }

        private void InitializeNetworking()
        {
            // Wait for EOS to be initialized
            if (!IsEOSInitialized())
            {
                GD.PrintErr("EOS is not initialized! Waiting...");
                // Retry initialization after a delay
                GetTree().CreateTimer(1.0).Timeout += InitializeNetworking;
                return;
            }

            // Initialize Riptide logger for Godot
            RiptideLogger.Initialize(GD.Print, GD.Print, GD.PrintErr, GD.PrintErr, false);

            EOSServer eosServer = new EOSServer();
            Server = new Server(eosServer);
            Server.ClientConnected += NewPlayerConnected;
            Server.ClientDisconnected += ServerPlayerLeft;

            Client = new Client(new EOSClient(eosServer));
            Client.Connected += DidConnect;
            Client.ConnectionFailed += FailedToConnect;
            Client.ClientDisconnected += ClientPlayerLeft;
            Client.Disconnected += DidDisconnected;
        }

        public override void _PhysicsProcess(double delta)
        {
            // Update networking in physics process for consistency
            if (Server.IsRunning)
                Server.Update();

            Client.Update();
        }

        public override void _ExitTree()
        {
            // Clean up when exiting
            StopServer();
            
            // Unsubscribe from events
            if (Server != null)
            {
                Server.ClientConnected -= NewPlayerConnected;
                Server.ClientDisconnected -= ServerPlayerLeft;
            }

            DisconnectClient();
            
            if (Client != null)
            {
                Client.Connected -= DidConnect;
                Client.ConnectionFailed -= FailedToConnect;
                Client.ClientDisconnected -= ClientPlayerLeft;
                Client.Disconnected -= DidDisconnected;
            }
        }

        public void StopServer()
        {
            if (Server != null && Server.IsRunning)
            {
                GD.Print("NetworkManager: Stopping server...");
                
                // Disconnect all clients gracefully
                Server.Stop();
                
                // Clean up all server players
                foreach (ServerPlayer player in ServerPlayer.List.Values)
                {
                    if (IsInstanceValid(player))
                        player.QueueFree();
                }
                ServerPlayer.List.Clear();
                
                GD.Print("NetworkManager: Server stopped successfully");
                EmitSignal(SignalName.ServerStopped);
            }
            else
            {
                GD.Print("NetworkManager: Server was not running");
            }
        }

        public void DisconnectClient()
        {
            if (Client != null && Client.IsConnected)
            {
                GD.Print("NetworkManager: Disconnecting client...");
                
                Client.Disconnect();
                
                // Clean up all client players
                foreach (EOSPluign.Demo.Player.ClientPlayer player in EOSPluign.Demo.Player.ClientPlayer.list.Values)
                {
                    if (IsInstanceValid(player))
                        player.QueueFree();
                }
                EOSPluign.Demo.Player.ClientPlayer.list.Clear();
                
                GD.Print("NetworkManager: Client disconnected successfully");
            }
            else
            {
                GD.Print("NetworkManager: Client was not connected");
            }
        }

        private void NewPlayerConnected(object sender, ServerConnectedEventArgs e)
        {
            foreach (ServerPlayer player in ServerPlayer.List.Values)
            {
                if (player.Id != e.Client.Id)
                    player.SendSpawn(e.Client.Id);
            }
        }

        private void ServerPlayerLeft(object sender, ServerDisconnectedEventArgs e)
        {
            if (ServerPlayer.List.TryGetValue(e.Client.Id, out ServerPlayer player))
            {
                if (IsInstanceValid(player))
                    player.QueueFree();
                ServerPlayer.List.Remove(e.Client.Id);
            }
        }

        private void DidConnect(object sender, EventArgs e)
        {
            GD.Print("Client: Successfully connected to server!");
            Message message = Message.Create(MessageSendMode.Reliable, ClientToServerId.PlayerName);
            message.AddString(GetPlayerName());
            Client.Send(message);
        }

        private void FailedToConnect(object sender, EventArgs e)
        {
            // Emit signal or call UI manager - adapt to your UI system
            EmitSignal(SignalName.ConnectionFailed);
        }

        private void ClientPlayerLeft(object sender, ClientDisconnectedEventArgs e)
        {
            if (EOSPluign.Demo.Player.ClientPlayer.list.TryGetValue(e.Id, out EOSPluign.Demo.Player.ClientPlayer player))
            {
                if (IsInstanceValid(player))
                    player.QueueFree();
                EOSPluign.Demo.Player.ClientPlayer.list.Remove(e.Id);
            }
        }

        private void DidDisconnected(object sender, DisconnectedEventArgs e)
        {
            GD.Print($"Client: Disconnected from server - Reason: {e.Reason}");
            
            foreach (EOSPluign.Demo.Player.ClientPlayer player in EOSPluign.Demo.Player.ClientPlayer.list.Values)
            {
                if (IsInstanceValid(player))
                    player.QueueFree();
            }

            EOSPluign.Demo.Player.ClientPlayer.list.Clear();

            // Emit signal for UI to handle
            EmitSignal(SignalName.Disconnected);
        }

        // Helper methods for EOS integration
        private bool IsEOSInitialized()
        {
            return EOSPluign.addons.eosplugin.EOSInterfaceManager.Instance != null && 
                   EOSPluign.addons.eosplugin.EOSInterfaceManager.Instance.Platform != null;
        }

        private string GetPlayerName()
        {
            // Try to get EOS user display name, fallback to default
            var authService = EOSPluign.addons.eosplugin.EOSInterfaceManager.Instance?.AuthService;
            if (authService != null && authService.IsLoggedIn())
            {
                // You might want to implement getting display name from EOS
                return "EOS Player";
            }
            return "Guest Player";
        }

        // Godot signals for UI communication
        [Signal]
        public delegate void ConnectionFailedEventHandler();
        
        [Signal]  
        public delegate void DisconnectedEventHandler();
        
        [Signal]
        public delegate void ServerStartedEventHandler();
        
        [Signal]
        public delegate void ServerStoppedEventHandler();
        
        // Public methods for controlling server/client
        public void StartServer(ushort port = 7777, ushort maxClients = 10)
        {
            if (!IsEOSInitialized())
            {
                GD.PrintErr("Cannot start server: EOS not initialized");
                return;
            }
            
            if (!Server.IsRunning)
            {
                Server.Start(port, maxClients);
                GD.Print($"Server started on port {port}");
                EmitSignal(SignalName.ServerStarted);
            }
        }
        
        public void ConnectToServer(string address = "127.0.0.1", ushort port = 7777)
        {
            if (!IsEOSInitialized())
            {
                GD.PrintErr("Cannot connect: EOS not initialized");
                return;
            }
            
            if (!Client.IsConnected)
            {
                Client.Connect(address, port);
                GD.Print($"Attempting to connect to {address}:{port}");
            }
        }
    }
}