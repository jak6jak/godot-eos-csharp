using Godot;
using Riptide.Demos.Steam.PlayerHosted;
using EOSPluign.addons.eosplugin;

public partial class DemoUI : Control
{
    [Export] private Button startServerButton;
    [Export] private Button connectClientButton;
    [Export] private Button stopServerButton;
    [Export] private Button disconnectClientButton;
    [Export] private LineEdit ipAddressInput;
    [Export] private LineEdit portInput;
    [Export] private Label statusLabel;
    
    private NetworkManager networkManager;
    
    public override void _Ready()
    {
        // Get references if not assigned in editor
        if (startServerButton == null)
            startServerButton = GetNode<Button>("%StartServerButton");
        if (connectClientButton == null)
            connectClientButton = GetNode<Button>("%ConnectClientButton");
        if (stopServerButton == null)
            stopServerButton = GetNode<Button>("%StopServerButton");
        if (disconnectClientButton == null)
            disconnectClientButton = GetNode<Button>("%DisconnectClientButton");
        if (ipAddressInput == null)
            ipAddressInput = GetNode<LineEdit>("%IPAddressInput");
        if (portInput == null)
            portInput = GetNode<LineEdit>("%PortInput");
        if (statusLabel == null)
            statusLabel = GetNode<Label>("%StatusLabel");
        
        // Set default values
        if (ipAddressInput != null)
            ipAddressInput.Text = "127.0.0.1";
        if (portInput != null)
            portInput.Text = "7777";
        
        // Connect button signals
        if (startServerButton != null)
            startServerButton.Pressed += OnStartServerPressed;
        if (connectClientButton != null)
            connectClientButton.Pressed += OnConnectClientPressed;
        if (stopServerButton != null)
            stopServerButton.Pressed += OnStopServerPressed;
        if (disconnectClientButton != null)
            disconnectClientButton.Pressed += OnDisconnectClientPressed;
        
        // Defer NetworkManager initialization to ensure it's ready
        CallDeferred(nameof(InitializeNetworkManager));
    }
    
    private void InitializeNetworkManager()
    {
        networkManager = NetworkManager.Singleton;
        if (networkManager != null)
        {
            GD.Print("DemoUI: NetworkManager found and connected");
            networkManager.ConnectionFailed += OnConnectionFailed;
            networkManager.Disconnected += OnDisconnected;
            networkManager.ServerStarted += OnServerStarted;
            networkManager.ServerStopped += OnServerStopped;
        }
        else
        {
            GD.PrintErr("DemoUI: NetworkManager singleton is null!");
            // Try finding it in the scene tree as fallback
            var networkManagerNode = GetTree().GetFirstNodeInGroup("NetworkManager");
            if (networkManagerNode is NetworkManager nm)
            {
                networkManager = nm;
                GD.Print("DemoUI: Found NetworkManager in scene tree");
                networkManager.ConnectionFailed += OnConnectionFailed;
                networkManager.Disconnected += OnDisconnected;
                networkManager.ServerStarted += OnServerStarted;
                networkManager.ServerStopped += OnServerStopped;
            }
        }
        
        UpdateUI();
        UpdateStatus();
    }
    
    private void OnStartServerPressed()
    {
        if (networkManager == null) return;
        
        ushort port = 7777;
        if (portInput != null && !string.IsNullOrEmpty(portInput.Text))
        {
            if (!ushort.TryParse(portInput.Text, out port))
            {
                SetStatus("Invalid port number!");
                return;
            }
        }
        
        networkManager.StartServer(port);
    }
    
    private void OnConnectClientPressed()
    {
        if (networkManager == null) return;
        
        string address = "127.0.0.1";
        ushort port = 7777;
        
        if (ipAddressInput != null && !string.IsNullOrEmpty(ipAddressInput.Text))
            address = ipAddressInput.Text;
            
        if (portInput != null && !string.IsNullOrEmpty(portInput.Text))
        {
            if (!ushort.TryParse(portInput.Text, out port))
            {
                SetStatus("Invalid port number!");
                return;
            }
        }
        
        networkManager.ConnectToServer(address, port);
    }
    
    private void OnStopServerPressed()
    {
        if (networkManager == null) return;
        networkManager.StopServer();
    }
    
    private void OnDisconnectClientPressed()
    {
        if (networkManager == null) return;
        networkManager.DisconnectClient();
    }
    
    private void OnConnectionFailed()
    {
        SetStatus("Connection failed!");
        UpdateUI();
    }
    
    private void OnDisconnected()
    {
        SetStatus("Disconnected from server");
        UpdateUI();
    }
    
    private void OnServerStarted()
    {
        SetStatus("Server started successfully!");
        UpdateUI();
    }
    
    private void OnServerStopped()
    {
        SetStatus("Server stopped");
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        if (networkManager == null) return;
        
        bool serverRunning = networkManager.Server?.IsRunning ?? false;
        bool clientConnected = networkManager.Client?.IsConnected ?? false;
        
        if (startServerButton != null)
            startServerButton.Disabled = serverRunning;
        if (stopServerButton != null)
            stopServerButton.Disabled = !serverRunning;
        if (connectClientButton != null)
            connectClientButton.Disabled = clientConnected;
        if (disconnectClientButton != null)
            disconnectClientButton.Disabled = !clientConnected;
    }
    
    private void SetStatus(string statusText)
    {
        if (statusLabel != null)
            statusLabel.Text = statusText;
    }
    
    private void UpdateStatus()
    {
        if (statusLabel == null) return;
        
        string status = "Ready";
        
        // Check EOS status
        var eosManager = EOSInterfaceManager.Instance;
        if (eosManager == null || eosManager.Platform == null)
        {
            status = "EOS not initialized";
        }
        else if (eosManager.AuthService?.IsLoggedIn() == true)
        {
            status = "EOS authenticated - Ready for multiplayer";
        }
        else
        {
            status = "EOS initialized - Attempting login...";
        }
        
        if (networkManager?.Server?.IsRunning == true)
            status += " | Server running";
        if (networkManager?.Client?.IsConnected == true)
            status += " | Client connected";
            
        statusLabel.Text = status;
    }
    
    public override void _Process(double delta)
    {
        // Update status periodically
        UpdateStatus();
        UpdateUI();
    }
    
    public override void _ExitTree()
    {
        // Clean up event connections
        if (networkManager != null)
        {
            networkManager.ConnectionFailed -= OnConnectionFailed;
            networkManager.Disconnected -= OnDisconnected;
            networkManager.ServerStarted -= OnServerStarted;
            networkManager.ServerStopped -= OnServerStopped;
        }
    }
}