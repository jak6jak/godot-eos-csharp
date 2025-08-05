using Godot;
using Riptide.Demos.Steam.PlayerHosted;

namespace EOSPluign.Demo.Player;

public partial class PlayerUIManager : Label3D
{
    // In your UI script
    public override void _Ready()
    {
        NetworkManager.Singleton.ConnectionFailed += OnConnectionFailed;
        NetworkManager.Singleton.Disconnected += OnDisconnected;
    }

    private void OnConnectionFailed()
    {
        // Show connection failed message
        // Return to main menu
    }

    private void OnDisconnected()
    {
        // Show disconnected message  
        // Return to main menu
    }
    
}