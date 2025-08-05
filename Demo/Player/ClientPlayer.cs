using System.Collections.Generic;
using Godot;
using Riptide;
using Riptide.Demos.Steam.PlayerHosted;

namespace EOSPluign.Demo.Player
{
    public partial class ClientPlayer : CharacterBody3D
    {
        public static Dictionary<ushort, ClientPlayer> list = new Dictionary<ushort, ClientPlayer>();

        [Export] private ushort id;
        [Export] private string username;

        public void Move(Vector3 newPosition, Vector3 forward)
        {
            GlobalPosition = newPosition;

            // Don't overwrite local player's forward direction to avoid noticeable rotational snapping
            if (id != NetworkManager.Singleton.Client.Id) 
            {
                Transform3D currentTransform = GlobalTransform;
                currentTransform.Basis = Basis.LookingAt(-forward, Vector3.Up);
                GlobalTransform = currentTransform;
            }
        }

        public override void _ExitTree()
        {
            list.Remove(id);
        }

        public static void Spawn(ushort id, string username, Vector3 position)
        {
            ClientPlayer player;
            
            if (id == NetworkManager.Singleton.Client.Id)
            {
                // Spawn local player
                player = NetworkManager.Singleton.LocalPlayerPrefab.Instantiate<ClientPlayer>();
            }
            else
            {
                // Spawn remote player
                player = NetworkManager.Singleton.PlayerPrefab.Instantiate<ClientPlayer>();
                
                // Set up UI for remote players
                PlayerUIManager uiManager = player.GetNode<PlayerUIManager>("%PlayerUIManager");
                uiManager.Text = (username);
            }

            // Add to scene
            NetworkManager.Singleton.GetTree().CurrentScene.AddChild(player);
            
            player.GlobalPosition = position;
            player.Name = $"Client Player {id} ({username})";
            player.id = id;
            player.username = username;
            
            list.Add(player.id, player);
        }
        
        #region Messages
        [MessageHandler((ushort)ServerToClientId.SpawnPlayer, NetworkManager.PlayerHostedDemoMessageHandlerGroupId)]
        private static void SpawnPlayer(Message message)
        {
            Spawn(message.GetUShort(), message.GetString(), message.GetVector3());
        }

        [MessageHandler((ushort)ServerToClientId.PlayerMovement, NetworkManager.PlayerHostedDemoMessageHandlerGroupId)]
        private static void PlayerMovement(Message message)
        {
            ushort playerId = message.GetUShort();
            if (list.TryGetValue(playerId, out ClientPlayer player))
                player.Move(message.GetVector3(), message.GetVector3());
        }
        #endregion
    }
}