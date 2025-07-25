using Godot;
using System;
using System.Threading.Tasks;
using EOSPluign.addons.eosplugin;
using EOSPluign.addons.eosplugin.EOS_Service_Layer.Authentication;

public partial class AccountChoicePanel : Control
{
    private TaskCompletionSource<ConnectService.AccountChoice> _currentTaskCompletionSource;
    
    private Button CreateAccountButton;
    private Button LinkAccountButton;
    
    
    public override void _Ready()
    {
        CreateAccountButton = GetNode<Button>("%CreateAccountButton");
        LinkAccountButton = GetNode<Button>("%LinkAccountButton");
        EOSInterfaceManager.Instance.ConnectService.AccountChoiceRequired += ShowPanel;
        
        CreateAccountButton.Pressed += OnCreateButtonPressed;
        LinkAccountButton.Pressed += OnLinkButtonPressed;
    }

    public void ShowPanel()
    {   
        _currentTaskCompletionSource = EOSInterfaceManager.Instance.ConnectService.ActiveChoiceRequest;
        if (_currentTaskCompletionSource != null && !_currentTaskCompletionSource.Task.IsCompleted)
        {
            Show();
        }
        else
        {
            GD.PrintErr("No active choice request");
        }
    }

    private void OnCreateButtonPressed()
    {
        _currentTaskCompletionSource?.SetResult(ConnectService.AccountChoice.Create);
        Hide();
    }
    private void OnLinkButtonPressed()
    {
        _currentTaskCompletionSource?.SetResult(ConnectService.AccountChoice.Link);
        Hide();
    }
    
    public override void _Notification(int what)
    {
        if (what == NotificationWMCloseRequest)
        { 
            _currentTaskCompletionSource?.SetResult(ConnectService.AccountChoice.Cancel);
        }
    }
}
