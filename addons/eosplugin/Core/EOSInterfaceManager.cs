using System;
using System.Threading.Tasks;
using Godot;
using Epic.OnlineServices;
using Epic.OnlineServices.Platform;
using Epic;
namespace EOSPluign.addons.eosplugin;

public partial class EOSInterfaceManager : Node
{
    public static EOSInterfaceManager Instance { get; private set; }
    public bool IsInitialized { get; private set; }
    public PlatformInterface Platform {private set; get;}
    public EOSConfiguration Configuration { private set; get; }
    private double  PlatformTickTimer { get; set; }
    private double PlatformTickInterval { get; set; } = 0.1f;
    public override void _EnterTree()
    {
        Instance = this;
        base._EnterTree();
    }

    public override void _Ready()
    {
        GD.Print("Initializing EOS");
        Configuration = new EOSConfiguration();
        Configuration.LoadConfig();
        var options = new InitializeOptions()
        {
            ProductName = Configuration.ProductName,
            ProductVersion = Configuration.ProductVersion,
        };
        
        var result = PlatformInterface.Initialize(ref options);
        if (result != Result.Success)
        {
            GD.PushError($"Failed to initialize EOS: {result}");
            OnServiceError("EOS", $"Failed to initialize EOS: {result}");
        }
        
        Epic.OnlineServices.Logging.LoggingInterface.SetLogLevel(Epic.OnlineServices.Logging.LogCategory.AllCategories,
            Epic.OnlineServices.Logging.LogLevel.VeryVerbose);
        
        
        Epic.OnlineServices.Logging.LoggingInterface.SetCallback((ref Epic.OnlineServices.Logging.LogMessage logMessage) =>
            GD.Print(logMessage.Message));

        var platformOptions = new Epic.OnlineServices.Platform.Options()
        {
            ProductId = Configuration.ProductId,
            SandboxId = Configuration.SandboxId,
            DeploymentId = Configuration.DeploymentId,
            ClientCredentials = 
                new ClientCredentials()
                {
                    ClientId = Configuration.ClientId,
                    ClientSecret = Configuration.ClientSecret,
                }
        };
        Platform = PlatformInterface.Create(ref platformOptions);
        if (Platform == null)
        {
            GD.PushError("Failed to create EOS platform");
            OnServiceError("EOS", "Failed to create EOS platform");
        }
    }
    
    public override void _PhysicsProcess(double delta) {
        if (Platform != null) {
            PlatformTickTimer += delta;

            if (PlatformTickTimer >= PlatformTickInterval) {
                PlatformTickTimer = 0;

                Platform.Tick();
            }
        }
    }
    
    public override void _ExitTree()
    {
        base._ExitTree();
        Platform.Release();
        PlatformInterface.Shutdown();
    }
    public void OnServiceError(string serviceName, string message)
    {
        GD.PrintErr($"Service error from {serviceName}: {message}");
        EmitSignal(SignalName.ServiceError, serviceName, message);
    }
    
    [Signal] public delegate void ServiceErrorEventHandler(string serviceName, string message);
}