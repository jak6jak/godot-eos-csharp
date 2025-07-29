#if TOOLS
using System;
using EOSPluign.addons.eosplugin;
using EOSPluign.addons.eosplugin.editor;
using Godot;

[Tool]
public partial class Eosplugin : EditorPlugin
{
    private Control _configurationDock;
    private Window _configWindow;
    
    private Error _CheckConfig()
    {
        Error err = EOSConfiguration.LoadConfig();
        if (err != Error.Ok)
        {
            GD.PushWarning("Failed to load EOS config file. " + err);
            return err;
        }

        return err;
    }

    public override void _EnterTree()
    {
        GD.Print("EOS Plugin: Entering tree...");
        _CheckConfig();
        
        // Create and add the configuration dock using the scene
        try
        {
            var configScene = GD.Load<PackedScene>("res://addons/eosplugin/editor/EOSconfig.tscn");
            if (configScene != null)
            {
                _configurationDock = configScene.Instantiate<Panel>();
                _configurationDock.Name = "EOS Config";
                
                // Try different dock slots in order of preference
                AddControlToDock(DockSlot.LeftUr, _configurationDock);
                GD.Print("EOS Plugin: Configuration dock added successfully to LeftUr slot");
            }
            else
            {
                GD.PrintErr("EOS Plugin: Could not load EOSconfig.tscn for dock");
            }
            
            // Also add to project menu for easier access
            AddToolMenuItem("EOS Configuration", Callable.From(ShowConfigWindow));
            GD.Print("EOS Plugin: Added EOS Configuration menu item");
        }
        catch (System.Exception ex)
        {
            GD.PrintErr("EOS Plugin: Failed to add configuration dock: " + ex.Message);
        }
    }

    private void ShowConfigWindow()
    {
        GD.Print("EOS Plugin: Opening configuration window");
        
        // Check if window is already open
        if (_configWindow != null && IsInstanceValid(_configWindow))
        {
            GD.Print("EOS Plugin: Configuration window is already open, bringing to front");
            _configWindow.GrabFocus();
            return;
        }
        
        // Load the EOSconfig scene
        var configScene = GD.Load<PackedScene>("res://addons/eosplugin/editor/EOSconfig.tscn");
        if (configScene == null)
        {
            GD.PrintErr("EOS Plugin: Could not load EOSconfig.tscn scene");
            return;
        }
        
        var configControl = configScene.Instantiate<Panel>();
        if (configControl == null)
        {
            GD.PrintErr("EOS Plugin: Could not instantiate EOSconfig scene");
            return;
        }
        
        // Create a responsive popup window with the scene
        _configWindow = new Window();
        _configWindow.Title = "EOS Configuration";
        
        // Set responsive size constraints
        _configWindow.MinSize = new Vector2I(400, 500);
        _configWindow.Size = new Vector2I(600, 800);
        _configWindow.MaxSize = new Vector2I(1200, 1000);
        
        // Configure window properties
        _configWindow.Unresizable = false; // Allow resizing
        
        // Connect to window close event to clean up reference
        _configWindow.CloseRequested += OnConfigWindowClosed;
        
        _configWindow.AddChild(configControl);
        
        GetEditorInterface().GetBaseControl().AddChild(_configWindow);
        _configWindow.PopupCentered();
        
        GD.Print("EOS Plugin: Configuration window opened with scene");
    }
    
    private void OnConfigWindowClosed()
    {
        GD.Print("EOS Plugin: Configuration window closed");
        if (_configWindow != null)
        {
            // Try to save configuration before closing
            try
            {
                var configMenu = _configWindow.GetChild<EosConfigMenu>(0);
                if (configMenu != null)
                {
                    configMenu.SaveConfigurationOnClose();
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"EOS Plugin: Failed to auto-save on window close: {ex.Message}");
            }
            
            _configWindow.QueueFree();
            _configWindow = null;
        }
    }

    public override void _ExitTree()
    {
        GD.Print("EOS Plugin: Exiting tree...");
        
        // Auto-save configuration dock if it exists
        if (_configurationDock != null)
        {
            try
            {
                var configMenu = _configurationDock as EosConfigMenu;
                if (configMenu != null)
                {
                    configMenu.SaveConfigurationOnClose();
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"EOS Plugin: Failed to auto-save dock configuration on exit: {ex.Message}");
            }
            
            RemoveControlFromDocks(_configurationDock);
            _configurationDock?.QueueFree();
        }
        
        // Close and clean up config window if open (auto-save handled in OnConfigWindowClosed)
        if (_configWindow != null && IsInstanceValid(_configWindow))
        {
            _configWindow.QueueFree();
            _configWindow = null;
        }
        
        // Remove menu item
        RemoveToolMenuItem("EOS Configuration");
    }
}
#endif