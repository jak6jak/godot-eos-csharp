using Godot;
using System;
using EOSPluign.addons.eosplugin;

public partial class MainMenu : Control
{
    private Button loginButton;
    public override void _Ready()
    {
        loginButton = GetNode<Button>("LoginButton");
        loginButton.Pressed += () =>
        {
            GD.Print("Login button pressed");
            EOSInterfaceManager.Instance.ConnectService.Login();
        };
    }
}
