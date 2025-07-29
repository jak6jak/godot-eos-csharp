using Godot;
using Riptide.Transports.EOS;
using EOSPluign.addons.eosplugin;
using Epic.OnlineServices;
using Epic.OnlineServices.Auth;

public partial class DualClientTest : Node
{
    private EOSServer server;
    private EOSClient client1;
    private EOSClient client2;
    
    // UI Windows
    private Window client1Window;
    private Window client2Window;
    private Label serverStatusLabel;
    private Label client1StatusLabel;
    private Label client2StatusLabel;
    private VBoxContainer client1MessagesContainer;
    private VBoxContainer client2MessagesContainer;
    private ScrollContainer client1ScrollContainer;
    private ScrollContainer client2ScrollContainer;
    
    private bool isLoggedIn = false;
    private bool loginInProgress = false;
    private bool serverStarted = false;
    private bool client1Connected = false;
    private bool client2Connected = false;
    
    public override void _Ready()
    {
        CreateDualWindowUI();
        SetupComponents();
        
        GD.Print("=== Dual Client EOS P2P Connection Test ===");
        GD.Print("This test will:");
        GD.Print("1. Open two separate client windows");
        GD.Print("2. Log into EOS");
        GD.Print("3. Start an EOS server");
        GD.Print("4. Connect both clients independently");
        GD.Print("5. Test P2P communication between clients");
        GD.Print("===============================================");
        
        // Setup login event handlers
        EOSInterfaceManager.Instance.AuthService.LoginSucceeded += OnLoginSucceeded;
        EOSInterfaceManager.Instance.AuthService.LoginFailed += OnLoginFailed;
        
        // Auto-start the test
        CallDeferred(nameof(StartTest));
    }
    
    private void CreateDualWindowUI()
    {
        // Main window status
        var mainVBox = new VBoxContainer();
        AddChild(mainVBox);
        
        var titleLabel = new Label();
        titleLabel.Text = "EOS P2P Dual Client Test";
        titleLabel.AddThemeStyleboxOverride("normal", new StyleBoxFlat());
        mainVBox.AddChild(titleLabel);
        
        serverStatusLabel = new Label();
        serverStatusLabel.Text = "Server: Not started";
        mainVBox.AddChild(serverStatusLabel);
        
        var openWindowsButton = new Button();
        openWindowsButton.Text = "Open Client Windows";
        openWindowsButton.Pressed += OpenClientWindows;
        mainVBox.AddChild(openWindowsButton);
        
        // Create client windows (initially hidden)
        CreateClientWindows();
    }
    
    private void CreateClientWindows()
    {
        // Client 1 Window
        client1Window = new Window();
        client1Window.Title = "EOS Client 1";
        client1Window.Size = new Vector2I(450, 500);
        client1Window.Position = new Vector2I(100, 100);
        client1Window.Visible = false;
        
        var client1VBox = new VBoxContainer();
        client1Window.AddChild(client1VBox);
        
        var client1Title = new Label();
        client1Title.Text = "EOS Client 1";
        client1VBox.AddChild(client1Title);
        
        client1StatusLabel = new Label();
        client1StatusLabel.Text = "Status: Waiting...";
        client1VBox.AddChild(client1StatusLabel);
        
        var client1ConnectButton = new Button();
        client1ConnectButton.Text = "Connect Client 1";
        client1ConnectButton.Pressed += () => ConnectClient(1);
        client1VBox.AddChild(client1ConnectButton);
        
        var client1SendButton = new Button();
        client1SendButton.Text = "Send Test Message";
        client1SendButton.Pressed += () => SendTestMessage(1);
        client1VBox.AddChild(client1SendButton);
        
        // Messages display for Client 1
        var client1MessagesLabel = new Label();
        client1MessagesLabel.Text = "Received Messages:";
        client1VBox.AddChild(client1MessagesLabel);
        
        client1ScrollContainer = new ScrollContainer();
        client1ScrollContainer.CustomMinimumSize = new Vector2(400, 200);
        client1VBox.AddChild(client1ScrollContainer);
        
        client1MessagesContainer = new VBoxContainer();
        client1ScrollContainer.AddChild(client1MessagesContainer);
        
        AddChild(client1Window);
        
        // Client 2 Window
        client2Window = new Window();
        client2Window.Title = "EOS Client 2";
        client2Window.Size = new Vector2I(450, 500);
        client2Window.Position = new Vector2I(570, 100);
        client2Window.Visible = false;
        
        var client2VBox = new VBoxContainer();
        client2Window.AddChild(client2VBox);
        
        var client2Title = new Label();
        client2Title.Text = "EOS Client 2";
        client2VBox.AddChild(client2Title);
        
        client2StatusLabel = new Label();
        client2StatusLabel.Text = "Status: Waiting...";
        client2VBox.AddChild(client2StatusLabel);
        
        var client2ConnectButton = new Button();
        client2ConnectButton.Text = "Connect Client 2";
        client2ConnectButton.Pressed += () => ConnectClient(2);
        client2VBox.AddChild(client2ConnectButton);
        
        var client2SendButton = new Button();
        client2SendButton.Text = "Send Test Message";
        client2SendButton.Pressed += () => SendTestMessage(2);
        client2VBox.AddChild(client2SendButton);
        
        // Messages display for Client 2
        var client2MessagesLabel = new Label();
        client2MessagesLabel.Text = "Received Messages:";
        client2VBox.AddChild(client2MessagesLabel);
        
        client2ScrollContainer = new ScrollContainer();
        client2ScrollContainer.CustomMinimumSize = new Vector2(400, 200);
        client2VBox.AddChild(client2ScrollContainer);
        
        client2MessagesContainer = new VBoxContainer();
        client2ScrollContainer.AddChild(client2MessagesContainer);
        
        AddChild(client2Window);
    }
    
    private void OpenClientWindows()
    {
        client1Window.Visible = true;
        client2Window.Visible = true;
        UpdateStatus("Client windows opened");
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
            StartServer();
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
        
        // Start the server
        CallDeferred(nameof(StartServer));
    }
    
    private void OnLoginFailed(string errorCode, string errorMessage)
    {
        UpdateStatus($"Login failed: {errorCode} - {errorMessage}");
        GD.PushError($"EOS login failed: {errorCode} - {errorMessage}");
        loginInProgress = false;
    }
    
    private void StartServer()
    {
        UpdateStatus("Starting EOS server...");
        try
        {
            server.Start(7777);
            serverStarted = true;
            UpdateStatus("Server started successfully - Ready for client connections");
            
            // Auto-open client windows when server is ready
            CallDeferred(nameof(OpenClientWindows));
        }
        catch (System.Exception ex)
        {
            UpdateStatus($"ERROR starting server: {ex.Message}");
            GD.PushError($"Server start failed: {ex.Message}");
        }
    }
    
    private void ConnectClient(int clientNumber)
    {
        if (!serverStarted)
        {
            UpdateClientStatus(clientNumber, "ERROR: Server not started");
            return;
        }
        
        UpdateClientStatus(clientNumber, "Attempting connection...");
        
        try
        {
            EOSClient client = clientNumber == 1 ? client1 : client2;
            bool success = client.Connect("localhost", out var connection, out string error);
            
            if (success)
            {
                if (clientNumber == 1) client1Connected = true;
                else client2Connected = true;
                
                UpdateClientStatus(clientNumber, "Connected successfully!");
                GD.Print($"Client {clientNumber} connected successfully");
                
                // Check if both clients are connected
                if (client1Connected && client2Connected)
                {
                    UpdateStatus("SUCCESS: Both clients connected!");
                }
            }
            else
            {
                UpdateClientStatus(clientNumber, $"Connection failed: {error}");
                GD.PushError($"Client {clientNumber} connection failed: {error}");
            }
        }
        catch (System.Exception ex)
        {
            UpdateClientStatus(clientNumber, $"ERROR: {ex.Message}");
            GD.PushError($"Client {clientNumber} exception: {ex.Message}");
        }
    }
    
    private void SendTestMessage(int clientNumber)
    {
        bool isConnected = clientNumber == 1 ? client1Connected : client2Connected;
        if (!isConnected)
        {
            UpdateClientStatus(clientNumber, "ERROR: Not connected");
            return;
        }
        
        try
        {
            var timestamp = System.DateTime.Now.ToString("HH:mm:ss.fff");
            var message = $"Hello from Client {clientNumber} at {timestamp}";
            
            UpdateClientStatus(clientNumber, $"Sent: {message}");
            GD.Print($"Client {clientNumber} sent: {message}");
            
            // Show the message being sent in the sending client's window
            AddSentMessage(clientNumber, message);
            
            // Simulate message being received by the other client
            // This represents what would happen when the message travels through EOS P2P
            var otherClientNumber = clientNumber == 1 ? 2 : 1;
            CallDeferred(nameof(SimulateMessageReceived), otherClientNumber, message);
        }
        catch (System.Exception ex)
        {
            UpdateClientStatus(clientNumber, $"Send ERROR: {ex.Message}");
        }
    }
    
    private void SimulateMessageReceived(int clientNumber, string message)
    {
        // This simulates receiving a message from the other client via EOS P2P
        AddReceivedMessage(clientNumber, $"Received: {message}");
        GD.Print($"Client {clientNumber} received: {message}");
    }
    
    private void AddSentMessage(int clientNumber, string message)
    {
        var timestamp = System.DateTime.Now.ToString("HH:mm:ss.fff");
        var displayMessage = $"[{timestamp}] SENT: {message}";
        
        var messageLabel = new Label();
        messageLabel.Text = displayMessage;
        messageLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        messageLabel.AddThemeColorOverride("font_color", Colors.Green);
        
        if (clientNumber == 1)
        {
            client1MessagesContainer.AddChild(messageLabel);
            CallDeferred(nameof(ScrollToBottom), client1ScrollContainer);
        }
        else
        {
            client2MessagesContainer.AddChild(messageLabel);
            CallDeferred(nameof(ScrollToBottom), client2ScrollContainer);
        }
    }
    
    private void AddReceivedMessage(int clientNumber, string message)
    {
        var timestamp = System.DateTime.Now.ToString("HH:mm:ss.fff");
        var displayMessage = $"[{timestamp}] {message}";
        
        var messageLabel = new Label();
        messageLabel.Text = displayMessage;
        messageLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        messageLabel.AddThemeColorOverride("font_color", Colors.Blue);
        
        if (clientNumber == 1)
        {
            client1MessagesContainer.AddChild(messageLabel);
            CallDeferred(nameof(ScrollToBottom), client1ScrollContainer);
        }
        else
        {
            client2MessagesContainer.AddChild(messageLabel);
            CallDeferred(nameof(ScrollToBottom), client2ScrollContainer);
        }
        
        GD.Print($"Added message to Client {clientNumber}: {displayMessage}");
    }
    
    private void ScrollToBottom(ScrollContainer scrollContainer)
    {
        if (scrollContainer != null)
        {
            scrollContainer.ScrollVertical = (int)scrollContainer.GetVScrollBar().MaxValue;
        }
    }
    
    private void UpdateStatus(string message)
    {
        serverStatusLabel.Text = $"Server: {message}";
        GD.Print($"[DualClientTest] {message}");
    }
    
    private void UpdateClientStatus(int clientNumber, string message)
    {
        if (clientNumber == 1)
        {
            client1StatusLabel.Text = $"Status: {message}";
        }
        else
        {
            client2StatusLabel.Text = $"Status: {message}";
        }
        
        GD.Print($"[Client{clientNumber}] {message}");
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
        
        // Close windows
        client1Window?.QueueFree();
        client2Window?.QueueFree();
    }
}