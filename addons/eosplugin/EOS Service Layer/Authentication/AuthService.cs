using System;
using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using Godot;

namespace EOSPluign.addons.eosplugin.EOS_Service_Layer.Authentication;

public partial class AuthService : BaseEOSService
{
    
    private AuthInterface _authInterface;
    private ulong _loginStatusChangeNotificationId;
    
    // Current authentication state
    private EpicAccountId _currentUserId;
    private LoginStatus _currentLoginStatus = LoginStatus.NotLoggedIn;
    
    private LoginCredentialType defaultCredentialType = LoginCredentialType.PersistentAuth;
    
    protected override void OnInitialize()
    {
        _authInterface = Manager.Platform.GetAuthInterface();
        
        if (_authInterface == null)
        {
            throw new InvalidOperationException("Failed to get Auth interface from EOS Platform");
        }
    }
    
    public override void OnShutdown()
    {
        // Clean up notifications
        if (_loginStatusChangeNotificationId != 0 && _authInterface != null)
        {
            _authInterface.RemoveNotifyLoginStatusChanged(_loginStatusChangeNotificationId);
            _loginStatusChangeNotificationId = 0;
        }
        
        // Logout if currently logged in
        if (_currentLoginStatus == LoginStatus.LoggedIn && _currentUserId != null)
        {
            LogoutUser();
        }
        
        _authInterface = null;
        _currentUserId = null;
        _currentLoginStatus = LoginStatus.NotLoggedIn;
    }
    
    private void SetupLoginStatusNotifications()
    {
        var options = new AddNotifyLoginStatusChangedOptions();
        
        _loginStatusChangeNotificationId = _authInterface.AddNotifyLoginStatusChanged(
            ref options, 
            null, 
            OnLoginStatusChanged
        );
        
        if (_loginStatusChangeNotificationId == 0)
        {
            EmitWarning("Failed to register for login status change notifications");
        }
    }
    // <summary>
    /// Attempts to login using persistent authentication (saved credentials)
    /// </summary>
/*public void LoginWithPersistentAuth()
    {
        if (!EnsureInitialized("login with persistent auth"))
            return;
            
        var credentials = new Credentials
        {
            Type = LoginCredentialType.PersistentAuth,
            Id = null,
            Token = null
        };
        
        var loginOptions = new LoginOptions
        {
            Credentials = credentials,
            ScopeFlags = AuthScopeFlags.BasicProfile | AuthScopeFlags.FriendsList | AuthScopeFlags.Presence
        };
        
        GD.Print("Attempting login with persistent authentication...");
        
        _authInterface.Login(ref loginOptions, null, OnLoginComplete);
    }
    */
    /// <summary>
    /// Attempts to login using account portal (opens browser for user login)
    /// </summary>
    public void LoginWithAccountPortal()
    {
        if (!EnsureInitialized("login with account portal"))
            return;
            
        var credentials = new Credentials
        {
            Type = LoginCredentialType.AccountPortal,
            Id = null,
            Token = null
        };
        
        var loginOptions = new LoginOptions
        {
            Credentials = credentials,
            ScopeFlags = AuthScopeFlags.BasicProfile | AuthScopeFlags.FriendsList | AuthScopeFlags.Presence
        };
        
        GD.Print("Opening account portal for login...");
        
        _authInterface.Login(ref loginOptions, null, OnLoginComplete);
    }

    // <summary>
    /// Attempts to login using external authentication (e.g., Steam, Xbox, PlayStation)
    /// </summary>
    public void LoginWithExternalAuth(ExternalCredentialType externalType, string token, string id = null)
    {
        if (!EnsureInitialized("login with external auth"))
            return;
            
        if (string.IsNullOrEmpty(token))
        {
            EmitError("External auth token cannot be null or empty");
            return;
        }
        
        var credentials = new Credentials
        {
            Type = LoginCredentialType.ExternalAuth,
            Id = id,
            Token = token,
            ExternalType = externalType
        };
        
        var loginOptions = new LoginOptions
        {
            Credentials = credentials,
            ScopeFlags = AuthScopeFlags.BasicProfile | AuthScopeFlags.FriendsList | AuthScopeFlags.Presence
        };
        
        GD.Print($"Attempting login with external auth: {externalType}");
        
        _authInterface.Login(ref loginOptions, null, OnLoginComplete);
    }
    
    /// <summary>
    /// Attempts to login using an exchange code (typically from Epic Games Launcher)
    /// </summary>
    public void LoginWithExchangeCode(string exchangeCode)
    {
        if (!EnsureInitialized("login with exchange code"))
            return;
            
        if (string.IsNullOrEmpty(exchangeCode))
        {
            EmitError("Exchange code cannot be null or empty");
            return;
        }
        
        var credentials = new Credentials
        {
            Type = LoginCredentialType.ExchangeCode,
            Id = null,
            Token = exchangeCode
        };
        
        var loginOptions = new LoginOptions
        {
            Credentials = credentials,
            ScopeFlags = AuthScopeFlags.BasicProfile | AuthScopeFlags.FriendsList | AuthScopeFlags.Presence
        };
        
        GD.Print("Attempting login with exchange code...");
        
        _authInterface.Login(ref loginOptions, null, OnLoginComplete);
    }
    
    
    /// <summary>
    /// Logs out the current user
    /// </summary>
    public void LogoutUser()
    {
        if (!EnsureInitialized("logout"))
            return;
            
        if (_currentUserId == null || _currentLoginStatus != LoginStatus.LoggedIn)
        {
            EmitWarning("No user is currently logged in");
            return;
        }
        
        var logoutOptions = new LogoutOptions
        {
            LocalUserId = _currentUserId
        };
        
        GD.Print("Logging out current user...");
        
        _authInterface.Logout(ref logoutOptions, null, OnLogoutComplete);
    }
    
    /// <summary>
    /// Deletes persistent authentication data for automatic login
    /// </summary>
    public void DeletePersistentAuth()
    {
        if (!EnsureInitialized("delete persistent auth"))
            return;
            
        var options = new DeletePersistentAuthOptions();
        _authInterface.DeletePersistentAuth(ref options,null,OnDeletePersistentAuthComplete);
        
        
    }

    /// <summary>
    /// Gets the current login status for a user
    /// </summary>
    public LoginStatus GetLoginStatus(EpicAccountId userId = null)
    {
        if (!EnsureInitialized("get login status"))
            return LoginStatus.NotLoggedIn;
            
        var targetUserId = userId ?? _currentUserId;
        if (targetUserId == null)
            return LoginStatus.NotLoggedIn;
            
        return _authInterface.GetLoginStatus(targetUserId);
    }
    /// <summary>
    /// Gets the current logged in user's Epic Account ID
    /// </summary>
    public EpicAccountId GetCurrentUserId()
    {
        return _currentUserId;
    }
    
    /// <summary>
    /// Checks if a user is currently logged in
    /// </summary>
    public bool IsLoggedIn()
    {
        return _currentLoginStatus == LoginStatus.LoggedIn && _currentUserId != null;
    }
    
    // <summary>
    /// Attempts automatic login flow - tries exchange code first, then persistent auth
    /// </summary>
    public void AttemptAutomaticLogin()
    {
        if (!EnsureInitialized("automatic login"))
            return;
            
        if (IsLoggedIn())
        {
            GD.Print("User is already logged in");
            EmitSignal(SignalName.LoginSucceeded, _currentUserId.ToString());
            return;
        }
        
        // Check for Epic Games Launcher exchange code first
        var args = OS.GetCmdlineArgs();
        string exchangeCode = ExtractExchangeCodeFromArgs(args);
        
        if (!string.IsNullOrEmpty(exchangeCode))
        {
            GD.Print("Exchange code found, attempting launcher login...");
            LoginWithExchangeCode(exchangeCode);
            return;
        }
        
        // Fallback to persistent auth
        GD.Print("No exchange code found, attempting AccountPortal login...");
        LoginWithAccountPortal();
    }
    /// <summary>
    /// Attempts to login with Steam if Steam is available
    /// </summary>
    public void LoginWithSteamIfAvailable()
    {
        if (!EnsureInitialized("Steam login"))
            return;
            
        // This would need to be implemented based on your Steam integration
        // For now, just show that the structure is in place
        EmitWarning("Steam integration not yet implemented");
        
        // Example of what this might look like:
        // var steamTicket = GetSteamSessionTicket();
        // if (!string.IsNullOrEmpty(steamTicket))
        // {
        //     LoginWithExternalAuth(ExternalCredentialType.SteamSessionTicket, steamTicket);
        // }
        // else
        // {
        //     EmitError("Failed to get Steam session ticket");
        // }
    }
    private string ExtractExchangeCodeFromArgs(string[] args)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "-AUTH_PASSWORD")
            {
                return args[i + 1];
            }
        }
        return null;
    }
    /// <summary>
    /// Comprehensive login attempt that tries multiple methods in order of preference
    /// </summary>
    public void SmartLogin()
    {
        if (!EnsureInitialized("smart login"))
            return;
            
        GD.Print("Starting smart login process...");
        
        // First try automatic methods
        AttemptAutomaticLogin();
        
        // The login callbacks will handle fallbacks if automatic methods fail
    }
    private void OnDeletePersistentAuthComplete(ref DeletePersistentAuthCallbackInfo data)
    {
        if (HandleEOSResult(data.ResultCode, "delete persistent auth"))
        {
            GD.Print("Persistent authentication data deleted successfully");
            EmitSignal(SignalName.PersistentAuthDeleted);
        }
    }
    
    private void OnLoginStatusChanged(ref LoginStatusChangedCallbackInfo data)
    {
        _currentLoginStatus = data.CurrentStatus;
        
        if (data.CurrentStatus != LoginStatus.LoggedIn)
        {
            _currentUserId = null;
        }
        
        GD.Print($"Login status changed for user {data.LocalUserId}: {data.PrevStatus} -> {data.CurrentStatus}");
        
        EmitSignal(SignalName.LoginStatusChanged, 
            data.LocalUserId.ToString(), 
            data.PrevStatus.ToString(), 
            data.CurrentStatus.ToString());
    }
    
    private bool ShouldFallbackToManualLogin(Result result)
    {
        return result switch
        {
            Result.AuthInvalidRefreshToken => true,
            Result.NotFound => true,
            Result.AuthInvalidToken => true,
            Result.AuthScopeConsentRequired => true,
            Result.InvalidAuth => true,
            Result.InvalidCredentials => true,
            Result.AuthExpired => true,
            Result.AuthExternalAuthNotLinked => true,
            _ => false
        };
    }
    
    private void OnLoginComplete(ref LoginCallbackInfo data)
    {
        if (data.ResultCode == Result.Success)
        {
            _currentUserId = data.LocalUserId;
            _currentLoginStatus = LoginStatus.LoggedIn;
            
            GD.Print($"Login successful for user: {data.LocalUserId}");
            EmitSignal(SignalName.LoginSucceeded, data.LocalUserId.ToString());
        }
        else if (data.ResultCode == Result.InvalidUser && data.ContinuanceToken != null)
        {
            // External account not linked - need to link account
            GD.Print("External account not linked. Continuance token received for account linking.");
            EmitSignal(SignalName.AccountLinkingRequired, data.ContinuanceToken.ToString());
        }
        else
        {
            var errorMessage = GetUserFriendlyErrorMessage(data.ResultCode, "login");
            EmitError($"Login failed: {errorMessage}");
            
            // Check if we should suggest manual login
            if (ShouldFallbackToManualLogin(data.ResultCode))
            {
                EmitSignal(SignalName.LoginFailedWithFallback, data.ResultCode.ToString(), errorMessage);
            }
            else
            {
                EmitSignal(SignalName.LoginFailed, data.ResultCode.ToString(), errorMessage);
            }
        }
    }
    
    private void OnLogoutComplete(ref LogoutCallbackInfo data)
    {
        if (data.ResultCode == Result.Success)
        {
            var loggedOutUserId = _currentUserId?.ToString() ?? "Unknown";
            
            _currentUserId = null;
            _currentLoginStatus = LoginStatus.NotLoggedIn;
            
            GD.Print($"Logout successful for user: {loggedOutUserId}");
            EmitSignal(SignalName.LogoutSucceeded, loggedOutUserId);
        }
        else
        {
            HandleEOSResult(data.ResultCode, "logout");
            EmitSignal(SignalName.LogoutFailed, data.ResultCode.ToString());
        }
    }
    
    protected override string GetUserFriendlyErrorMessage(Result result, string operation)
    {
        return result switch
        {
            Result.InvalidAuth => "Authentication failed - please check your credentials",
            Result.InvalidCredentials => "Invalid login credentials provided",
            Result.InvalidUser => "User account not found or external account not linked",
            Result.AuthAccountLocked => "Account is locked - please contact support",
            Result.AuthAccountLockedForUpdate => "Account is temporarily locked for updates",
            Result.AuthInvalidRefreshToken => "Login session expired - please login again",
            Result.AuthInvalidToken => "Invalid authentication token",
            Result.AuthAuthenticationFailure => "Authentication failed - invalid bearer token",
            Result.AuthInvalidPlatformToken => "Invalid platform authentication token",
            Result.AuthWrongAccount => "Authentication parameters not associated with this account",
            Result.AuthWrongClient => "Authentication parameters not associated with this client",
            Result.AuthFullAccountRequired => "Full Epic account required for this operation",
            Result.AuthHeadlessAccountRequired => "Headless account required for this operation",
            Result.AuthPasswordResetRequired => "Password reset required - please reset your password",
            Result.AuthExpired => "Authorization code has expired - please login again",
            Result.AuthScopeConsentRequired => "Additional permissions required - please login through account portal",
            Result.AuthApplicationNotFound => "Application not found - check your product configuration",
            Result.AuthScopeNotFound => "Requested permissions not found",
            Result.AuthAccountFeatureRestricted => "Account access has been restricted",
            Result.AuthAccountPortalLoadError => "Failed to load account portal - please try again",
            Result.AuthCorrectiveActionRequired => "Account requires corrective action - please visit Epic Games website",
            Result.AuthPinGrantCode => "PIN grant code initiated",
            Result.AuthPinGrantExpired => "PIN grant code has expired",
            Result.AuthPinGrantPending => "PIN grant code is pending",
            Result.AuthExternalAuthNotLinked => "External account not linked to Epic account",
            Result.AuthExternalAuthRevoked => "External authentication access has been revoked",
            Result.AuthExternalAuthInvalid => "External authentication token is invalid",
            Result.AuthExternalAuthRestricted => "External account cannot be linked due to restrictions",
            Result.AuthExternalAuthCannotLogin => "External account cannot be used for login",
            Result.AuthExternalAuthExpired => "External authentication has expired",
            Result.AuthExternalAuthIsLastLoginType => "Cannot remove external auth - it's the only way to login",
            Result.AuthExchangeCodeNotFound => "Exchange code not found or expired",
            Result.AuthOriginatingExchangeCodeSessionExpired => "Original exchange code session has expired",
            Result.AuthAccountNotActive => "Account is disabled and cannot be used",
            Result.AuthMFARequired => "Multi-factor authentication required",
            Result.AuthParentalControls => "Parental controls prevent login",
            Result.AuthNoRealId => "Real ID association required but missing",
            Result.AuthUserInterfaceRequired => "User interaction required for login",
            _ => base.GetUserFriendlyErrorMessage(result, operation)
        };

    }
    
    [Signal] public delegate void LoginSucceededEventHandler(string userId);
    [Signal] public delegate void LoginFailedEventHandler(string errorCode, string errorMessage);
    [Signal] public delegate void LoginFailedWithFallbackEventHandler(string errorCode, string errorMessage);
    [Signal] public delegate void LogoutSucceededEventHandler(string userId);
    [Signal] public delegate void LogoutFailedEventHandler(string errorCode);
    [Signal] public delegate void LoginStatusChangedEventHandler(string userId, string previousStatus, string currentStatus);
    [Signal] public delegate void AccountLinkingRequiredEventHandler(string continuanceToken);
    [Signal] public delegate void PersistentAuthDeletedEventHandler();
    
}