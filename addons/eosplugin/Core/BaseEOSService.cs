using System;
using Epic.OnlineServices;
using Godot;

namespace EOSPluign.addons.eosplugin;

public abstract partial class BaseEOSService : Node
{
    protected EOSInterfaceManager Manager { get; private set; }
    protected bool IsInitialized { get; private set; }
    
    
    
    public virtual void Initialize(EOSInterfaceManager manager)
    {
        if (IsInitialized)
        {
            GD.PrintErr($"Service {GetType().Name} is already initialized");
            return;
        }
        
        Manager = manager ?? throw new ArgumentNullException(nameof(manager));
        
        try
        {
            OnInitialize();
            IsInitialized = true;
            GD.Print($"Initialized EOS service: {GetType().Name}");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to initialize {GetType().Name}: {ex.Message}");
            EmitSignal(SignalName.ServiceError, GetType().Name, $"Initialization failed: {ex.Message}");
            throw;
        }
    }

    public virtual void Shutdown()
    {
        if (!IsInitialized)
        {
            return;
        }
        
        try
        {
            OnShutdown();
            IsInitialized = false;
            GD.Print($"Shutdown EOS service: {GetType().Name}");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Error during {GetType().Name} shutdown: {ex.Message}");
        }
    }
    protected abstract void OnInitialize();
    public abstract void OnShutdown();
    
    // Common error handling
    protected void EmitError(string message)
    {
        var fullMessage = $"[{GetType().Name}] {message}";
        GD.PrintErr(fullMessage);
        EmitSignal(SignalName.ServiceError, GetType().Name, message);
        
        // Also notify the manager
        Manager?.OnServiceError(GetType().Name, message);
    }
    
    protected void EmitWarning(string message)
    {
        var fullMessage = $"[{GetType().Name}] WARNING: {message}";
        GD.PushWarning(fullMessage);
        EmitSignal(SignalName.ServiceWarning, GetType().Name, message);
    }
    
    protected bool HandleEOSResult(Result result, string operation)
    {
        if (result == Result.Success)
        {
            return true;
        }
        
        var errorMessage = GetUserFriendlyErrorMessage(result, operation);
        EmitError($"{operation} failed: {errorMessage}");
        return false;
    }
    
    // Convert EOS error codes to user-friendly messages
    protected virtual string GetUserFriendlyErrorMessage(Result result, string operation)
    {
        return result switch
        {
            Result.InvalidAuth => "Authentication failed - please login again",
            Result.InvalidUser => "User account not found or not linked",
            Result.NotFound => $"Resource not found during {operation}",
            Result.NetworkDisconnected => "Network connection error - please check your internet",
            Result.ServiceFailure => "EOS services are temporarily unavailable",
            Result.TooManyRequests => "Too many requests - please wait before trying again",
            Result.AccessDenied => "Access denied - insufficient permissions",
            Result.InvalidRequest => $"Invalid request parameters for {operation}",
            Result.OperationWillRetry => $"Operation {operation} will retry automatically",
            _ => $"Operation failed with error: {result}"
        };
    }
    
    // Helper to check if service is ready for operations
    protected bool EnsureInitialized(string operation = "operation")
    {
        if (!IsInitialized)
        {
            EmitError($"Cannot perform {operation}: service not initialized");
            return false;
        }
        
        if (Manager?.Platform == null)
        {
            EmitError($"Cannot perform {operation}: EOS platform not available");
            return false;
        }
        
        return true;
    }
    
    [Signal] public delegate void ErrorEventHandler(string message);
    [Signal] public delegate void ServiceWarningEventHandler(string serviceName, string message);
    [Signal] public delegate void ServiceErrorEventHandler(string serviceName, string message);
}