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
    public override void _EnterTree()
    {
        Instance = this;
        base._EnterTree();
    }

    public override void _Ready()
    {
        
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
    }
    
    public override void _ExitTree()
    {
        base._ExitTree();
        
    }
/*
    private async void InitializeEOSAsync()
    {
        try
        {
            await InitializeEOS();
        }
        catch (Exception e)
        {
            GD.PushError($"Failed to initialize EOS: {e.Message}");
            OnServiceError("EOS", $"Failed to initialize EOS: {e.Message}");
        }
    }

    private async Task InitializeEOS()
    {
        if (IsInitialized)
        {
            GD.PushWarning("Already init");   
        }
        
        
    }

    private void LoadConfiguration()
    {
        try
        {
            Configuration = new EOSConfiguration();
            var err = Configuration.LoadConfig();
            if (err != Error.Ok)
            {
                throw new Exception($"Failed to load EOS configuration: {err}");
            }
        }
        catch (Exception e)
        {
            throw new Exception($"Failed to load EOS configuration: {e.Message}",e);
        }
    }

    private void InitializeEOSSDK()
    {
        var result = Platform.
    }
    */
    public void OnServiceError(string serviceName, string message)
    {
        GD.PrintErr($"Service error from {serviceName}: {message}");
        EmitSignal(SignalName.ServiceError, serviceName, message);
    }
    
    [Signal] public delegate void ServiceErrorEventHandler(string serviceName, string message);
}