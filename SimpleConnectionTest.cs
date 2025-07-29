using Godot;
using Riptide.Transports.EOS;
using EOSPluign.addons.eosplugin;
using Epic.OnlineServices;
using Epic.OnlineServices.Auth;

public partial class SimpleConnectionTest : Node
{
    private EOSServer server;
    private EOSClient client1;
    private EOSClient client2;
    
    private VBoxContainer ui;
    private Label statusLabel;
    
    private bool isLoggedIn = false;
    private bool loginInProgress = false;
    private bool serverStarted = false;
    
    public override void _Ready()
    {
        CreateSimpleUI();
        SetupComponents();
        
        GD.Print("=== Simple EOS P2P Connection Test ===");
        GD.Print("This test will automatically attempt to:");
        GD.Print("1. Log into EOS");
        GD.Print("2. Start an EOS server");
        GD.Print("3. Connect two clients to it");
        GD.Print("4. Test basic connectivity");
        GD.Print("=====================================");
        
        // Setup login event handlers
        EOSInterfaceManager.Instance.AuthService.LoginSucceeded += OnLoginSucceeded;
        EOSInterfaceManager.Instance.AuthService.LoginFailed += OnLoginFailed;
        
        // Auto-start the test
        CallDeferred(nameof(StartTest));
    }
    
    private void CreateSimpleUI()
    {
        ui = new VBoxContainer();
        AddChild(ui);
        
        statusLabel = new Label();
        statusLabel.Text = "Starting connection test...";
        ui.AddChild(statusLabel);
    }
    
    private void SetupComponents()
    {
        // Initialize server
        server = new EOSServer();
        
        // Initialize clients with reference to local server
        client1 = new EOSClient(server);
        client2 = new EOSClient(server);
    }
    
    private void StartTest()
    {
        UpdateStatus("Checking EOS login status...");
        
        // Check if user is already logged in
        var userId = EOSInterfaceManager.Instance?.ConnectService?.GetProductUserId();
        if (userId != null)
        {
            UpdateStatus($"Already logged in: {userId}");
            isLoggedIn = true;
            StartConnectionTest();
            return;
        }
        
        // Attempt automatic login
        if (!loginInProgress)
        {
            UpdateStatus("Attempting automatic EOS login...");
            loginInProgress = true;
            
            try
            {
                EOSInterfaceManager.Instance.AuthService.SmartLogin();
                EOSInterfaceManager.Instance.ConnectService.Login();
                UpdateStatus("Login request sent, waiting for response...");
            }
            catch (System.Exception ex)
            {
                UpdateStatus($"ERROR during login attempt: {ex.Message}");
                GD.PushError($"Login failed: {ex.Message}");
                loginInProgress = false;
            }
        }
    }
    
    private void OnLoginSucceeded(string userId)
    {
        UpdateStatus($"Login successful! User ID: {userId}");
        isLoggedIn = true;
        loginInProgress = false;
        
        // Start the connection test
        CallDeferred(nameof(StartConnectionTest));
    }
    
    private void OnLoginFailed(string errorCode, string errorMessage)
    {
        UpdateStatus($"Login failed: {errorCode} - {errorMessage}");
        GD.PushError($"EOS login failed: {errorCode} - {errorMessage}");
        loginInProgress = false;
    }
    
    private void StartConnectionTest()
    {
        UpdateStatus("Starting EOS server...");
        try
        {
            server.Start(7777);
            serverStarted = true;
            UpdateStatus("Server started successfully");
        }
        catch (System.Exception ex)
        {
            UpdateStatus($"ERROR starting server: {ex.Message}");
            GD.PushError($"Server start failed: {ex.Message}");
            return;
        }
        
        // Wait a frame then connect clients
        CallDeferred(nameof(ConnectClients));
    }
    
    private void ConnectClients()
    {
        UpdateStatus("Connecting Client 1...");
        
        try
        {
            bool success1 = client1.Connect("localhost", out var connection1, out string error1);
            if (success1)
            {
                UpdateStatus("Client 1 connected successfully");
            }
            else
            {
                UpdateStatus($"Client 1 failed to connect: {error1}");
                GD.PushError($"Client 1 connection failed: {error1}");
                return;
            }
        }
        catch (System.Exception ex)
        {
            UpdateStatus($"ERROR connecting Client 1: {ex.Message}");
            GD.PushError($"Client 1 exception: {ex.Message}");
            return;
        }
        
        // Wait a moment then connect second client
        CallDeferred(nameof(ConnectClient2));
    }
    
    private void ConnectClient2()
    {
        UpdateStatus("Connecting Client 2...");
        
        try
        {
            bool success2 = client2.Connect("localhost", out var connection2, out string error2);
            if (success2)
            {
                UpdateStatus("Client 2 connected successfully");
                UpdateStatus("SUCCESS: Both clients connected!");
                CallDeferred(nameof(TestComplete));
            }
            else
            {
                UpdateStatus($"Client 2 failed to connect: {error2}");
                GD.PushError($"Client 2 connection failed: {error2}");
            }
        }
        catch (System.Exception ex)
        {
            UpdateStatus($"ERROR connecting Client 2: {ex.Message}");
            GD.PushError($"Client 2 exception: {ex.Message}");
        }
    }
    
    private void TestComplete()
    {
        UpdateStatus("=== CONNECTION TEST COMPLETE ===");
        GD.Print("Connection test completed successfully!");
        GD.Print("Both clients are now connected through EOS P2P transport");
    }
    
    private void UpdateStatus(string message)
    {
        statusLabel.Text = message;
        GD.Print($"[ConnectionTest] {message}");
    }
    
    public override void _Process(double delta)
    {
        // Only poll if we're logged in and server is started
        if (!isLoggedIn || !serverStarted) return;
        
        // Poll server and clients for network updates
        server?.Poll();
        client1?.Poll();
        client2?.Poll();
    }
    
    public override void _ExitTree()
    {
        // Clean up event handlers
        if (EOSInterfaceManager.Instance?.AuthService != null)
        {
            EOSInterfaceManager.Instance.AuthService.LoginSucceeded -= OnLoginSucceeded;
            EOSInterfaceManager.Instance.AuthService.LoginFailed -= OnLoginFailed;
        }
        
        // Clean up connections
        client1?.Disconnect();
        client2?.Disconnect();
        server?.Stop();
    }
}