using System;
using System.Threading.Tasks;
using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Connect;
using Godot;
using Steamworks;
using CopyIdTokenOptions = Epic.OnlineServices.Auth.CopyIdTokenOptions;
using Credentials = Epic.OnlineServices.Connect.Credentials;
using IdToken = Epic.OnlineServices.Auth.IdToken;
using LinkAccountCallbackInfo = Epic.OnlineServices.Connect.LinkAccountCallbackInfo;
using LoginCallbackInfo = Epic.OnlineServices.Connect.LoginCallbackInfo;
using LoginOptions = Epic.OnlineServices.Connect.LoginOptions;
using Result = Epic.OnlineServices.Result;

namespace EOSPluign.addons.eosplugin.EOS_Service_Layer.Authentication;

public partial class ConnectService : BaseEOSService
{
    private ConnectInterface _connectInterface;
    private string SteamAppId = "3136980";
    private ContinuanceToken _continuanceToken;
    private ProductUserId _productUserId;
    private ulong _authExpirationNotificationId;
    
    // State management
    private bool _isLoginInProgress = false;
    private bool _isAccountLinkingInProgress = false;
    public ExternalCredentialType CurrentLoginType;
    public enum AccountChoice { Create, Link, Cancel }
    public TaskCompletionSource<AccountChoice> ActiveChoiceRequest { get; private set; }
    
    [Signal]
    public delegate void AccountChoiceRequiredEventHandler();
    
    [Signal]
    public delegate void ConnectLoginSucceededEventHandler(string productUserId);
    
    [Signal]
    public delegate void ConnectLoginFailedEventHandler(string errorCode, string errorMessage);
    
    [Signal]
    public delegate void AccountLinkingSucceededEventHandler();
    
    [Signal]
    public delegate void AccountLinkingFailedEventHandler(string errorCode, string errorMessage);
    
    [Signal]
    public delegate void AuthTokenExpiringEventHandler();

    protected override void OnInitialize()
    {
        _connectInterface = Manager.Platform.GetConnectInterface();
        if (_connectInterface == null)
        {
            throw new InvalidOperationException("Failed to get Connect interface from EOS Platform");
        }
        
        SetupAuthExpirationNotifications();
    }

    public override void OnShutdown()
    {
        // Clean up notifications
        if (_authExpirationNotificationId != 0 && _connectInterface != null)
        {
            _connectInterface.RemoveNotifyAuthExpiration(_authExpirationNotificationId);
            _authExpirationNotificationId = 0;
        }
        
        // Cancel any pending operations
        ActiveChoiceRequest?.TrySetResult(AccountChoice.Cancel);
        
        _connectInterface = null;
        _productUserId = null;
        _continuanceToken = null;
        _isLoginInProgress = false;
        _isAccountLinkingInProgress = false;
    }

    public void Login()
    {
        if (!EnsureInitialized("login"))
            return;
            
        if (_isLoginInProgress)
        {
            EmitWarning("Login already in progress");
            return;
        }
        
        bool successConvert = Enum.TryParse(EOSConfiguration.ConfigFields[EOSConfiguration.RequiredConfigFields.DefaultExternalCredentialType],true,out CurrentLoginType);
        if (!successConvert)
        {
            GD.PrintErr("Cannot parse default external credential type. Defaulting to EpicIDToken.");
            CurrentLoginType = ExternalCredentialType.SteamSessionTicket;
        }
        
        switch (CurrentLoginType)
        {
           case ExternalCredentialType.SteamSessionTicket:
               LoginWithSteam();
               break;
           case ExternalCredentialType.DeviceidAccessToken:
               LoginWithDeviceId();
               break;
           case ExternalCredentialType.EpicIdToken:
              
               LoginWithEpicAccount();
               break;
           default:
               throw new ArgumentOutOfRangeException();
        }
    }
    
    private void SetupAuthExpirationNotifications()
    {
        var options = new AddNotifyAuthExpirationOptions();
        
        _authExpirationNotificationId = _connectInterface.AddNotifyAuthExpiration(
            ref options,
            null,
            OnAuthExpiration
        );
        
        if (_authExpirationNotificationId == 0)
        {
            EmitWarning("Failed to register for auth expiration notifications");
        }
    }
    
    public bool IsLoggedIn()
    {
        return _productUserId != null && 
               _connectInterface?.GetLoginStatus(_productUserId) == LoginStatus.LoggedIn;
    }
    
    public ProductUserId GetProductUserId()
    {
        return _productUserId;
    }

    /// <summary>
    /// Attempts to login using Steam credentials if available
    /// </summary>
    public async void LoginWithSteam()
    {
        if (!EnsureInitialized("Steam login"))
            return;
            
        if (_isLoginInProgress)
        {
            EmitWarning("Login already in progress");
            return;
        }
        
        if (IsLoggedIn())
        {
            GD.Print("User is already logged in to EOS Connect");
            EmitSignal(SignalName.ConnectLoginSucceeded, _productUserId.ToString());
            return;
        }
        
        _isLoginInProgress = true;
        CurrentLoginType = ExternalCredentialType.SteamSessionTicket;
        try
        {
            string steamSessionTicket = await GetSteamSessionTicketAsync();
            if (string.IsNullOrEmpty(steamSessionTicket))
            {
                EmitError("Could not get Steam Session Ticket");
                _isLoginInProgress = false;
                return;
            }
            
            var loginOptions = new LoginOptions()
            {
                Credentials = new Credentials()
                {
                    Type = ExternalCredentialType.SteamSessionTicket,
                    Token = steamSessionTicket,
                },
            };
            
            GD.Print("Attempting EOS Connect login with Steam...");
            _connectInterface.Login(ref loginOptions, null, OnLoginComplete);
        }
        catch (Exception ex)
        {
            EmitError($"Failed to initiate Steam login: {ex.Message}");
            _isLoginInProgress = false;
        }
    }
    
    /// <summary>
    /// Login using Device ID for mobile/standalone scenarios
    /// </summary>
    public void LoginWithDeviceId()
    {
        if (!EnsureInitialized("Device ID login"))
            return;
            
        if (_isLoginInProgress)
        {
            EmitWarning("Login already in progress");
            return;
        }
        
        if (IsLoggedIn())
        {
            GD.Print("User is already logged in to EOS Connect");
            EmitSignal(SignalName.ConnectLoginSucceeded, _productUserId.ToString());
            return;
        }
        
        _isLoginInProgress = true;
        CurrentLoginType = ExternalCredentialType.DeviceidAccessToken;

        var createDeviceIdOptions = new CreateDeviceIdOptions()
        {
            DeviceModel = Godot.OS.GetName() +" "+ OS.GetProcessorName()
        };
        _connectInterface.CreateDeviceId(ref createDeviceIdOptions, null, m_CreateDeviceCompleted);
    }

    private void m_CreateDeviceCompleted(ref CreateDeviceIdCallbackInfo data)
    {
        // Allow the login to proceed if a new ID was created OR if one already existed.
        if (data.ResultCode == Result.Success || data.ResultCode == Result.DuplicateNotAllowed)
        {
            if (data.ResultCode == Result.DuplicateNotAllowed)
            {
                GD.Print("Device ID already exists. Proceeding to login.");
            }
            else
            {
                GD.Print("Successfully created a new Device ID. Proceeding to login.");
            }

            var loginOptions = new LoginOptions()
            {
                Credentials = new Credentials()
                {
                    Type = ExternalCredentialType.DeviceidAccessToken,
                    Token = null, // Device ID is managed by the SDK
                },
                UserLoginInfo = new UserLoginInfo()
                {
                    DisplayName = "Player" // Provide a default display name
                }
            };
            _connectInterface.Login(ref loginOptions, null, OnLoginComplete); // Assuming OnLoginComplete is your generic login callback
        }
        else
        {
            // Any other result is a genuine error.
            EmitError($"Failed to create or find Device ID: {data.ResultCode}");
            _isLoginInProgress = false;
        }
    }

    /// <summary>
    /// Login using Epic Account ID token from Auth service
    /// </summary>
    public void LoginWithEpicAccount()
    {
        if (!EnsureInitialized("Epic Account login"))
            return;
        
        if (_isLoginInProgress)
        {
            EmitWarning("Login already in progress");
            return;
        }
        
        if (IsLoggedIn())
        {
            GD.Print("User is already logged in to EOS Connect");
            EmitSignal(SignalName.ConnectLoginSucceeded, _productUserId.ToString());
            return;
        }
        
        _isLoginInProgress = true;
        CurrentLoginType = ExternalCredentialType.EpicIdToken;
        EOSInterfaceManager.Instance.AuthService.SmartLogin();
        CopyIdTokenOptions options = new CopyIdTokenOptions()
        {
            AccountId = EOSInterfaceManager.Instance.AuthService.GetCurrentUserId()
        };
        Epic.OnlineServices.Auth.IdToken? idToken;
        var result = EOSInterfaceManager.Instance.Platform.GetAuthInterface().CopyIdToken(ref options, out idToken);
        if (result != Result.Success)
        {
            EmitError($"Failed to copy Epic Account ID token: {result}");
            GD.PrintErr($"Failed to copy Epic Account ID token: {result}");
            _isLoginInProgress = false;
            return;
        }
        
        var loginOptions = new LoginOptions()
        {
            Credentials = new Credentials()
            {
                Type = ExternalCredentialType.EpicIdToken,
                Token = idToken.ToString(),
            },
        };
        
        GD.Print("Attempting EOS Connect login with Epic Account...");
        _connectInterface.Login(ref loginOptions, null, OnLoginComplete);
    }

    /// <summary>
    /// Checks if Steam is available on the system
    /// </summary>
    public bool IsSteamAvailable()
    {
        try
        {
            return InitializeSteam() && SteamClient.IsValid && SteamClient.IsLoggedOn;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Initializes Steam client if not already initialized
    /// </summary>
    public bool InitializeSteam()
    {
        try
        {
            if (!SteamClient.IsValid)
            {
                GD.Print($"Initializing Steam with AppId: {SteamAppId}");
                SteamClient.Init(uint.Parse(SteamAppId), true);
                
                if (SteamClient.IsValid)
                {
                    GD.Print("Steam client initialized successfully");
                    return true;
                }
                else
                {
                    GD.PushError("Steam client initialization failed - Steam may not be running");
                    return false;
                }
            }
            else
            {
                GD.Print("Steam client already initialized");
                return true;
            }
        }
        catch (Exception e)
        {
            GD.PushError($"Failed to initialize SteamClient: {e.Message}");
            return false;
        }
    }
    
    public async Task<string> GetSteamSessionTicketAsync()
    {
        // Initialize Steam if not already initialized
        if (!InitializeSteam())
        {
            return "";
        }

        if (!SteamClient.IsValid)
        {
            GD.PushError("Steam is not running or not initialized");
            return "";
        }

        if (!SteamClient.IsLoggedOn)
        {
            GD.PushError("User is not logged into Steam");
            return "";
        }

        try
        {
            AuthTicket ticket = await SteamUser.GetAuthTicketForWebApiAsync("epiconlineservices");
            if (ticket?.Data == null || ticket.Data.Length == 0)
            {
                GD.PushError("Failed to get Steam session ticket");
                return "";
            }

            string hexToken = Convert.ToHexString(ticket.Data);
            GD.Print($"Retrieved Steam session ticket: {hexToken.Substring(0, Math.Min(hexToken.Length, 16))}...");
            return hexToken;
        }
        catch (Exception ex)
        {
            GD.PushError($"Exception getting Steam session ticket: {ex.Message}");
            return "";
        }
    }

    private void CreateNewUser()
    {
        if (_continuanceToken == null)
        {
            EmitError("Cannot create user: no continuance token available");
            return;
        }
        
        var createUserOptions = new CreateUserOptions()
        {
            ContinuanceToken = _continuanceToken,
        };
        
        GD.Print("Creating new EOS Connect user...");
        _connectInterface.CreateUser(ref createUserOptions, null, OnCreateUserComplete);
    }

    private void OnCreateUserComplete(ref CreateUserCallbackInfo data)
    {
        if (data.ResultCode != Result.Success)
        {
            EmitError($"Failed to create new user: {data.ResultCode}");
            _isAccountLinkingInProgress = false;
        }
        else
        {
            GD.Print("Successfully created new EOS Connect user");
            // After creating user, attempt login again
            _isAccountLinkingInProgress = false;
            // Re-attempt the original login that triggered this flow
            LoginWithSteam(); // Or whatever login method was used
        }
    }

    private void LinkAccountWithEpicAccount()
    {
        if (_continuanceToken == null)
        {
            EmitError("Cannot link account: no continuance token available");
            return;
        }
        
        _isAccountLinkingInProgress = true;
        
        var authInterface = Manager.Platform.GetAuthInterface();
        var authLoginOptions = new Epic.OnlineServices.Auth.LoginOptions()
        {
            ScopeFlags = AuthScopeFlags.BasicProfile | AuthScopeFlags.FriendsList | AuthScopeFlags.Presence,
            Credentials = new Epic.OnlineServices.Auth.Credentials()
            {
                Type = LoginCredentialType.AccountPortal,
            }
        };
        
        GD.Print("Opening Epic Account portal for account linking...");
        authInterface.Login(ref authLoginOptions, null, OnEpicLoginCompleteForLinking);
    }
    
    private void OnLoginComplete(ref LoginCallbackInfo data)
    {
        _isLoginInProgress = false;
        
        if (data.ResultCode == Result.Success)
        {
            _productUserId = data.LocalUserId;
            GD.Print($"EOS Connect login successful. Product User ID: {_productUserId}");
            EmitSignal(SignalName.ConnectLoginSucceeded, _productUserId.ToString());
            return;
        }

        if (data.ResultCode == Result.InvalidUser && data.ContinuanceToken != null)
        {
            _continuanceToken = data.ContinuanceToken;
            GD.Print("External account not linked. Prompting user for account choice...");
            HandleAccountLinking();
            return;
        }

        var errorMessage = GetUserFriendlyErrorMessage(data.ResultCode, "Connect login");
        EmitError($"EOS Connect login failed: {errorMessage}");
        EmitSignal(SignalName.ConnectLoginFailed, data.ResultCode.ToString(), errorMessage);
    }

    private async void HandleAccountLinking()
    {
        if (_isAccountLinkingInProgress)
        {
            EmitWarning("Account linking already in progress");
            return;
        }
        
        _isAccountLinkingInProgress = true;
        
        try
        {
            GD.Print("Waiting for user to choose 'Create' or 'Link' account...");
            AccountChoice choice = await PromptUserForAccountChoice();

            switch (choice)
            {
                case AccountChoice.Create:
                    GD.Print("User chose to create new account");
                    CreateNewUser();
                    break;
                case AccountChoice.Link:
                    GD.Print("User chose to link existing Epic account");
                    LinkAccountWithEpicAccount();
                    break;
                case AccountChoice.Cancel:
                    GD.Print("User cancelled the account linking process");
                    _continuanceToken = null;
                    _isAccountLinkingInProgress = false;
                    EmitSignal(SignalName.ConnectLoginFailed, "UserCancelled", "User cancelled account linking");
                    break;
            }
        }
        catch (Exception ex)
        {
            EmitError($"Error during account linking: {ex.Message}");
            _isAccountLinkingInProgress = false;
        }
    }
    
    private async Task<AccountChoice> PromptUserForAccountChoice()
    {
        var tcs = new TaskCompletionSource<AccountChoice>();
        this.ActiveChoiceRequest = tcs;
        EmitSignal(SignalName.AccountChoiceRequired);
        
        return await tcs.Task;
    }
    
    private void OnEpicLoginCompleteForLinking(ref Epic.OnlineServices.Auth.LoginCallbackInfo data)
    {
        if (data.ResultCode != Result.Success)
        {
            var errorMessage = GetUserFriendlyErrorMessage(data.ResultCode, "Epic Account login for linking");
            EmitError($"Epic Account login failed during linking: {errorMessage}");
            _isAccountLinkingInProgress = false;
            EmitSignal(SignalName.AccountLinkingFailed, data.ResultCode.ToString(), errorMessage);
            return;
        }

        var linkOptions = new Epic.OnlineServices.Connect.LinkAccountOptions()
        {
            ContinuanceToken = _continuanceToken,
        };
        
        GD.Print("Linking Epic Account to EOS Connect...");
        _connectInterface.LinkAccount(ref linkOptions, null, OnLinkAccountComplete);
    }

    private void OnLinkAccountComplete(ref LinkAccountCallbackInfo data)
    {
        _isAccountLinkingInProgress = false;
        _continuanceToken = null;
        
        if (data.ResultCode != Result.Success)
        {
            var errorMessage = GetUserFriendlyErrorMessage(data.ResultCode, "account linking");
            EmitError($"Account linking failed: {errorMessage}");
            EmitSignal(SignalName.AccountLinkingFailed, data.ResultCode.ToString(), errorMessage);
            return;
        }

        GD.Print("Account linking successful! Attempting login again...");
        EmitSignal(SignalName.AccountLinkingSucceeded);
        
        // Re-attempt the original login
        LoginWithSteam(); // Or store the original login method and retry it
    }
    
    private void OnAuthExpiration(ref AuthExpirationCallbackInfo data)
    {
        GD.Print("EOS Connect auth token expiring soon. Attempting refresh...");
        EmitSignal(SignalName.AuthTokenExpiring);
        
        // Attempt to refresh the token by re-logging in
        if (_productUserId != null)
        {
            // Re-attempt login with the same method used originally
            LoginWithSteam(); // This should be dynamic based on original login method
        }
    }
    
    /// <summary>
    /// Logout from EOS Connect
    /// </summary>
    public void Logout()
    {
        if (!EnsureInitialized("logout"))
            return;
            
        if (_productUserId == null)
        {
            EmitWarning("No user is currently logged in to EOS Connect");
            return;
        }
        
        // Note: EOS Connect doesn't have an explicit logout function
        // The connection is maintained until the platform shuts down
        _productUserId = null;
        GD.Print("EOS Connect logout completed");
    }
    
    protected override string GetUserFriendlyErrorMessage(Result result, string operation)
    {
        return result switch
        {
            Result.InvalidUser => "External account not linked to Epic account",
            Result.AuthInvalidToken => "External authentication token is invalid or expired",
            Result.AuthExternalAuthNotLinked => "External account is not linked to an Epic account",
            Result.AuthExternalAuthRevoked => "External authentication access has been revoked",
            Result.AuthExternalAuthInvalid => "External authentication credentials are invalid",
            Result.AuthExternalAuthExpired => "External authentication has expired",
            Result.AuthExternalAuthCannotLogin => "External account cannot be used for login",
            Result.ConnectAuthExpired => "Connect authentication has expired",
            Result.ConnectExternalTokenValidationFailed => "External token validation failed",
            Result.ConnectUserAlreadyExists => "User already exists with different external account",
            Result.DuplicateNotAllowed => "Account linking not allowed - duplicate detected",
            _ => base.GetUserFriendlyErrorMessage(result, operation)
        };
    }
}