using System.Collections.Generic;
using EOSPluign.Demo.Player;
using Godot;
using Riptide;
using Riptide.Demos.Steam.PlayerHosted;

namespace EOSPluign.Demo;

public partial class ServerPlayer : CharacterBody3D
{
    public static Dictionary<ushort, ServerPlayer> List { get; private set; } = new Dictionary<ushort, ServerPlayer>();

    public ushort Id { get; private set; }
    public string Username { get; private set; }

    [Export] private PlayerMovement movement;

    public override void _Ready()
    {
        // Initialize movement component if not assigned
        if (movement == null)
            movement = GetNode<PlayerMovement>("PlayerMovement");
    }

    public void SetForwardDirection(Vector3 forward)
    {
        forward.Y = 0; // Keep the player upright
        Transform3D currentTransform = GlobalTransform;
        currentTransform.Basis = Basis.LookingAt(-forward, Vector3.Up);
        GlobalTransform = currentTransform;
    }

    public override void _ExitTree()
    {
        List.Remove(Id);
    }

    public static void Spawn(ushort id, string username)
    {
        // Instantiate the server player prefab
        PackedScene serverPlayerPrefab = NetworkManager.Singleton.ServerPlayerPrefab;
        ServerPlayer player = serverPlayerPrefab.Instantiate<ServerPlayer>();

        // Set position and add to scene
        player.GlobalPosition = new Vector3(0f, 1f, 0f);
        player.GlobalRotation = Vector3.Zero;

        // Add to the scene tree (assuming NetworkManager has a reference to the main scene)
        NetworkManager.Singleton.GetTree().CurrentScene.AddChild(player);

        player.Name = $"Server Player {id} ({(username == "" ? "Guest" : username)})";
        player.Id = id;
        player.Username = username;

        player.SendSpawn();
        List.Add(player.Id, player);
    }

    #region Messages

    /// <summary>Sends a player's info to the given client.</summary>
    /// <param name="toClient">The client to send the message to.</param>
    public void SendSpawn(ushort toClient)
    {
        NetworkManager.Singleton.Server.Send(
            GetSpawnData(Message.Create(MessageSendMode.Reliable, ServerToClientId.SpawnPlayer)), toClient);
    }

    /// <summary>Sends a player's info to all clients.</summary>
    private void SendSpawn()
    {
        NetworkManager.Singleton.Server.SendToAll(GetSpawnData(Message.Create(MessageSendMode.Reliable,
            ServerToClientId.SpawnPlayer)));
    }

    private Message GetSpawnData(Message message)
    {
        message.AddUShort(Id);
        message.AddString(Username);
        message.AddVector3(GlobalPosition);
        return message;
    }

    [MessageHandler((ushort)ClientToServerId.PlayerName, NetworkManager.PlayerHostedDemoMessageHandlerGroupId)]
    private static void PlayerName(ushort fromClientId, Message message)
    {
        Spawn(fromClientId, message.GetString());
    }

    [MessageHandler((ushort)ClientToServerId.PlayerInput, NetworkManager.PlayerHostedDemoMessageHandlerGroupId)]
    private static void PlayerInput(ushort fromClientId, Message message)
    {
        if (List.TryGetValue(fromClientId, out ServerPlayer player))
        {
            message.GetBools(5, player.movement.Inputs);
            player.SetForwardDirection(message.GetVector3());
        }
    }

    #endregion
}