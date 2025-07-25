using System;
using System.Threading.Tasks;
using EOSPluign.addons.eosplugin.EOS_Service_Layer.Authentication;
using Godot;
using Epic.OnlineServices;
using Epic.OnlineServices.Platform;
using Epic;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;
namespace EOSPluign.addons.eosplugin;

public partial class EOSInterfaceManager : Node
{
    public static EOSInterfaceManager Instance { get; private set; }
    public bool IsInitialized { get; private set; }
    public PlatformInterface Platform {private set; get;}
    public AuthService AuthService { private set; get; }
    public ConnectService ConnectService { private set; get; }
    public EOSConfiguration Configuration { private set; get; }
    private double  PlatformTickTimer { get; set; }
    private double PlatformTickInterval { get; set; } = 0.1f;
    public override void _EnterTree()
    {
        Instance = this;
        SetupNativeLibraryResolver();
        base._EnterTree();
    }

    public override void _Ready()
    {
        GD.Print("Initializing EOS");
        
        // Set up native library search paths before initializing EOS
        SetupNativeLibraryPaths();
        
        EOSConfiguration.LoadConfig();
        var options = new InitializeOptions()
        {
            ProductName = EOSConfiguration.ConfigFields[ EOSConfiguration.RequiredConfigFields.ProductName],
            ProductVersion =EOSConfiguration.ConfigFields[ EOSConfiguration.RequiredConfigFields.ProductVersion],
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
            ProductId = EOSConfiguration.ConfigFields[ EOSConfiguration.RequiredConfigFields.EosProductId],
            SandboxId = EOSConfiguration.ConfigFields[ EOSConfiguration.RequiredConfigFields.EosSandboxId],
            DeploymentId = EOSConfiguration.ConfigFields[ EOSConfiguration.RequiredConfigFields.EosDeploymentId],
            ClientCredentials = 
                new ClientCredentials()
                {
                    ClientId =EOSConfiguration.ConfigFields[ EOSConfiguration.RequiredConfigFields.EosClientId],
                    ClientSecret = EOSConfiguration.ConfigFields[ EOSConfiguration.RequiredConfigFields.EosClientSecret],
                }
        };
        Platform = PlatformInterface.Create(ref platformOptions);
        if (Platform == null)
        {
            GD.PushError("Failed to create EOS platform");
            OnServiceError("EOS", "Failed to create EOS platform");
        }
        AuthService = new AuthService();
        AuthService.Initialize(this);
        ConnectService = new ConnectService();
        ConnectService.Initialize(this);
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
    
    private void SetupNativeLibraryPaths()
    {
        try
        {
            // Add the plugin's thirdparty directory to the DLL search path
            string pluginPath = Path.Combine(System.Environment.CurrentDirectory, "addons", "eosplugin", "thirdparty");
            string outputPath = Path.Combine(System.Environment.CurrentDirectory, ".godot", "mono", "temp", "bin", "Debug");
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Add directories to DLL search path for Windows
                AddDllDirectory(pluginPath);
                AddDllDirectory(outputPath);
                
                GD.Print($"Added DLL search paths: {pluginPath}, {outputPath}");
                
                // Set the DLL directory to include dependencies
                SetDllDirectory(pluginPath);
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to setup native library paths: {ex.Message}");
        }
    }
    
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AddDllDirectory(string lpPathName);
    
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetDllDirectory(string lpPathName);
    
    private void SetupNativeLibraryResolver()
    {
        NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), DllImportResolver);
    }
    
    private static IntPtr DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        // Define search paths for native libraries
        string[] searchPaths = {
            Path.Combine(System.Environment.CurrentDirectory, "addons", "eosplugin", "thirdparty"),
            Path.Combine(System.Environment.CurrentDirectory, ".godot", "mono", "temp", "bin", "Debug"),
            System.Environment.CurrentDirectory
        };
        
        foreach (string searchDir in searchPaths)
        {
            string libraryPath = null;
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                libraryPath = Path.Combine(searchDir, $"{libraryName}.dll");
                if (!File.Exists(libraryPath))
                    libraryPath = Path.Combine(searchDir, libraryName);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                libraryPath = Path.Combine(searchDir, $"lib{libraryName}.so");
                if (!File.Exists(libraryPath))
                    libraryPath = Path.Combine(searchDir, libraryName);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                libraryPath = Path.Combine(searchDir, $"lib{libraryName}.dylib");
                if (!File.Exists(libraryPath))
                    libraryPath = Path.Combine(searchDir, libraryName);
            }
            
            if (libraryPath != null && File.Exists(libraryPath))
            {
                GD.Print($"Loading native library: {libraryPath}");
                if (NativeLibrary.TryLoad(libraryPath, out IntPtr handle))
                {
                    return handle;
                }
            }
        }
        
        // Fallback to default behavior
        return IntPtr.Zero;
    }
}