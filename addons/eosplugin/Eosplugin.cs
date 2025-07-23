#if TOOLS
using EOSPluign.addons.eosplugin;
using Godot;


[Tool]
public partial class Eosplugin : EditorPlugin
{
	private const string CONFIG_PATH = "res://EOSconfig.cfg";
	private ConfigFile _configFile;
	
	public Error CreateConfig()
	{
		_configFile = new ConfigFile();
		_configFile.SetValue("Config","CONFIG_VERSION","1.0");
		_configFile.SetValue("ProductSettings","PRODUCT_NAME","");
		_configFile.SetValue("ProductSettings", "PRODUCT_VERSION", "");
        
		_configFile.SetValue("Secrets","EOS_PRODUCT_ID","");
		_configFile.SetValue("Secrets", "EOS_SANDBOX_ID", "");
		_configFile.SetValue("Secrets","EOS_DEPLOYMENT_ID","");
		_configFile.SetValue("Secrets", "EOS_CLIENT_ID", "");
		_configFile.SetValue("Secrets", "EOS_CLIENT_SECRET", "");

		var err = _configFile.Save(CONFIG_PATH);
		if (err != Error.Ok)
			GD.Print("Error saving config file: " + err.ToString());
		return err; 
	}
	
	
	private Error _CheckConfig()
	{
		_configFile = new ConfigFile();
		Error err = _configFile.Load(CONFIG_PATH);
		if (err != Error.Ok)
		{
			if (err == Error.FileNotFound)
			{
				GD.Print("Creating new config file.");
				CreateConfig();
			}
			else
			{
				GD.PushWarning("Failed to load EOS config file. " + err);
				return err;
			}
		}

		foreach (var section in _configFile.GetSections())
		{
			foreach (var field in _configFile.GetSectionKeys(section))
			{
				if ((string)_configFile.GetValue(section, field, "") == "")
				{
					GD.PushWarning("Missing value for " + field + " in " + section + " section. " +
					               "Make sure to fill out EOSConfig.cfg file");
				}
			}
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
