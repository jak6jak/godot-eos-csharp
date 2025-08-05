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
            
            // Make this node persistent across scene changes
            SetProcessMode(ProcessModeEnum.Always);
            
            InitializeNetworking();
        }

        private void InitializeNetworking()
        {
            // Check if Steam is initialized - you'll need to adapt this to your Steam integration
            if (!IsSteamInitialized())
            {
                GD.PrintErr("Steam is not initialized!");
                return;
            }

            // Initialize Riptide logger for Godot
            RiptideLogger.Initialize(GD.Print, GD.Print, GD.PrintErr, GD.PrintErr, false);

            EOSServer steamServer = new EOSServer();
            Server = new Server(steamServer);
            Server.ClientConnected += NewPlayerConnected;
            Server.ClientDisconnected += ServerPlayerLeft;

            Client = new Client(new EOSClient(steamServer));
            Client.Connected += DidConnect;
            Client.ConnectionFailed += FailedToConnect;
            Client.ClientDisconnected += ClientPlayerLeft;
            Client.Disconnected += DidDisconnect;
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
                Client.Disconnected -= DidDisconnect;
            }
        }

        internal void StopServer()
        {
            Server?.Stop();
            
            // Clean up all server players
            foreach (ServerPlayer player in ServerPlayer.List.Values)
            {
                if (IsInstanceValid(player))
                    player.QueueFree();
            }
            ServerPlayer.List.Clear();
        }

        internal void DisconnectClient()
        {
            Client?.Disconnect();
            
            // Clean up all client players
            foreach (EOSPluign.Demo.Player.ClientPlayer player in EOSPluign.Demo.Player.ClientPlayer.list.Values)
            {
                if (IsInstanceValid(player))
                    player.QueueFree();
            }
            EOSPluign.Demo.Player.ClientPlayer.list.Clear();
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
            Message message = Message.Create(MessageSendMode.Reliable, ClientToServerId.PlayerName);
            message.AddString(GetSteamPersonaName()); // You'll need to implement this
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

        private void DidDisconnect(object sender, EventArgs e)
        {
            foreach (EOSPluign.Demo.Player.ClientPlayer player in EOSPluign.Demo.Player.ClientPlayer.list.Values)
            {
                if (IsInstanceValid(player))
                    player.QueueFree();
            }

            EOSPluign.Demo.Player.ClientPlayer.list.Clear();

            // Emit signal for UI to handle
            EmitSignal(SignalName.Disconnected);
        }

        // Helper methods - you'll need to implement these based on your Steam integration
        private bool IsSteamInitialized()
        {
            // Implement your Steam initialization check here
            // For example, if using Steamworks.NET: return SteamManager.Initialized;
            return true; // Placeholder
        }

        private string GetSteamPersonaName()
        {
            // Implement getting Steam persona name here
            // For example: return Steamworks.SteamFriends.GetPersonaName();
            return "Player"; // Placeholder
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
    }
}