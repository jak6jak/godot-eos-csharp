using Godot;
using System;
using EOSPluign.addons.eosplugin;
using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.UserInfo;

public partial class MainMenu : Control
{
    private Button loginButton;
    private Label helloLabel;
    public override void _Ready()
    {
        loginButton = GetNode<Button>("LoginButton");
        helloLabel = GetNode<Label>("HelloLabel");
        loginButton.Pressed += () =>
        {
            GD.Print("Login button pressed");
            EOSInterfaceManager.Instance.AuthService.SmartLogin();
        };
        EOSInterfaceManager.Instance.AuthService.LoginSucceeded += AuthServiceOnLoginSucceeded;
    }

    private void AuthServiceOnLoginSucceeded(string userId)
    {
        QueryUserInfoOptions options = new QueryUserInfoOptions()
        {
            LocalUserId = EpicAccountId.FromString(userId),
            TargetUserId = EpicAccountId.FromString(userId),
        };
        EOSInterfaceManager.Instance.Platform.GetUserInfoInterface().QueryUserInfo(ref options, null,
            (ref QueryUserInfoCallbackInfo data) =>
            {
                CopyUserInfoOptions options = new CopyUserInfoOptions()
                {
                    LocalUserId = EpicAccountId.FromString(userId),
                    TargetUserId = EpicAccountId.FromString(userId),
                };
                var result = EOSInterfaceManager.Instance.Platform.GetUserInfoInterface()
                    .CopyUserInfo(ref options, out var outUserInfo);
               if (result == Result.Success)
               {
                   helloLabel.Text = $"Hello {outUserInfo.GetValueOrDefault().DisplayName}";
               }
                
            });
        
    }
}
