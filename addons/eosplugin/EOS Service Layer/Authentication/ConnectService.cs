using System;
using System.Threading.Tasks;
using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Connect;
using Godot;
using Steamworks;
using Credentials = Epic.OnlineServices.Connect.Credentials;
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
    private AccountChoicePanel accountChoicePanel;
    public enum AccountChoice { Create, Link, Cancel}
    public TaskCompletionSource<AccountChoice> ActiveChoiceRequest { get; private set; }

    [Signal]
    public delegate void AccountChoiceRequiredEventHandler();
    protected override void OnInitialize()
    {
        _connectInterface = Manager.Platform.GetConnectInterface();
        if (_connectInterface == null)
        {
            throw new InvalidOperationException("Failed to get Connect interface from EOS Platform");
        }
    }

    public override void OnShutdown()
    {
        throw new System.NotImplementedException();
    }

    private async Task<string> GetSteamSessionTicket()
    {
        try
        {
            SteamClient.Init(uint.Parse(SteamAppId), true);
        }
        catch (Exception e)
        {
            GD.PushError($"Failed to initialize SteamClient: {e.Message}");
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

        AuthTicket ticket = await SteamUser.GetAuthTicketForWebApiAsync("epiconlineservices");
        if (ticket == null || ticket.Data == null || ticket.Data.Length == 0)
        {
            GD.PushError("Failed to get Steam session ticket");
            return "";
        }

        string hexToken = Convert.ToHexString(ticket.Data);
        return hexToken;
    }

    public async void Login()
    {
        Utf8String steamSessionTicket = await GetSteamSessionTicket();
        if (string.IsNullOrEmpty(steamSessionTicket))
        {
            GD.PushError("Could not get Steam Session Ticket. Aborting login.");
            return;
        }
        var loginOptions = new LoginOptions()
        {
            Credentials = new Credentials()
            {
                Type = ExternalCredentialType.SteamSessionTicket, Token = steamSessionTicket,
            },
        };
        _connectInterface.Login(ref loginOptions, null, m_LoginComplete);
    }

    private void CreateNewUser()
    {
        var createUserOptions = new CreateUserOptions()
        {
            ContinuanceToken = _continuanceToken,
        };
        _connectInterface.CreateUser(ref createUserOptions,null, m_OnCreateUserComplete);
    }

    private void m_OnCreateUserComplete(ref CreateUserCallbackInfo data)
    {
        if (data.ResultCode != Result.Success)
        {
           GD.PrintErr("Failed to create new user: " + data.ResultCode); 
        }
        else
        {
            Login();
        }
    }

    private void LinkAccount()
    {
        var authInterface = Manager.Platform.GetAuthInterface();
        var authLoginOptions = new Epic.OnlineServices.Auth.LoginOptions()
        {
            ScopeFlags = AuthScopeFlags.BasicProfile | AuthScopeFlags.FriendsList | AuthScopeFlags.Presence,
            Credentials = new Epic.OnlineServices.Auth.Credentials()
            {
                Type = LoginCredentialType.AccountPortal,
            }
        };
        authInterface.Login(ref authLoginOptions, null, m_OnEpicLoginCompleteForLinking);
    }
    
    private void m_LoginComplete(ref LoginCallbackInfo data)
    {
        if (data.ResultCode == Result.Success)
        {
            // ... success logic ...
            _productUserId = data.LocalUserId;
            return;
        }

        if (data.ResultCode == Result.InvalidUser)
        {

            HandleAccountLinking(data.ContinuanceToken);
        }

        EmitError($"Login failed: {data.ResultCode}");
    }

    private async void HandleAccountLinking(ContinuanceToken token)
    {
        _continuanceToken = token;
        GD.Print("Waiting for user to choose 'Create' or 'Link' account...");
        AccountChoice choice = await PromptUserForAccountChoice();

        switch (choice)
        {
            case AccountChoice.Create:
                CreateNewUser();
                break;
            case AccountChoice.Link:
                LinkAccount();
                break;
            case AccountChoice.Cancel:
                GD.Print("User cancelled the login process.");
                // Clean up the token if necessary
                _continuanceToken = null;
                break;
        }
    } 
    private async Task<AccountChoice> PromptUserForAccountChoice()
    {
        var tcs = new TaskCompletionSource<AccountChoice>();
        this.ActiveChoiceRequest = tcs;
        EmitSignal(SignalName.AccountChoiceRequired);
        
        return await tcs.Task;
    }
    
    private void m_OnEpicLoginCompleteForLinking(ref Epic.OnlineServices.Auth.LoginCallbackInfo data)
    {
        if (data.ResultCode != Result.Success)
        {
            GD.PrintErr("Epic Account login failed: " + data.ResultCode);
            return;
        }

        var linkOptions = new Epic.OnlineServices.Connect.LinkAccountOptions()
        {
            ContinuanceToken = _continuanceToken,
        };
        _connectInterface.LinkAccount(ref linkOptions, null, m_OnLinkAccountComplete);
    }

    private void m_OnLinkAccountComplete(ref LinkAccountCallbackInfo data)
    {
        _continuanceToken = null;
        if (data.ResultCode != Result.Success)
        {
            GD.PrintErr("Linking account failed: " + data.ResultCode);
            return;
        }

        Login();
    }
}