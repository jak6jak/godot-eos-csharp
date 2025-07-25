#if TOOLS
using EOSPluign.addons.eosplugin;
using Godot;

[Tool]
public partial class Eosplugin : EditorPlugin
{
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
        _CheckConfig();
    }

    public override void _ExitTree()
    {
    }
}
#endif