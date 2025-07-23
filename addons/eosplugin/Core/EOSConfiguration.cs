using Godot;

namespace EOSPluign.addons.eosplugin;

public class EOSConfiguration
{
    public string ConfigVersion { get; private set; } = "1.0";
    public string ProductName { get; private set; }
    public string ProductVersion { get; private set; }
    public string ProductId { get; private set; }
    public string SandboxId { get; private set; }
    public string DeploymentId { get; private set; }
    public string ClientId { get; private set; }
    public string ClientSecret { get; private set; }


    public EOSConfiguration()
    {
        //var err =  LoadConfig();
    }
    
    private const string CONFIG_PATH = "res://EOSconfig.cfg";
    private ConfigFile _configFile;
    public Error LoadConfig()
    {
        _configFile = new ConfigFile();
        Error err = _configFile.Load(CONFIG_PATH);
        if (err != Error.Ok)
        {
            GD.PushWarning("Error loading config file: " + err.ToString());
            return err;
        }

        if ((string)_configFile.GetValue("Config", "CONFIG_VERSION", "") != ConfigVersion)
        {
            GD.PushError("Config version mismatch.");
            return Error.ParseError;
        }
        
        
        bool missing = false;
        foreach (var section in _configFile.GetSections())
        {
            foreach (var field in _configFile.GetSectionKeys(section))
            {
                if ((string)_configFile.GetValue(section, field, "") == "")
                {
                    GD.PushWarning("Missing value for " + field + " in " + section + " section. " +
                                   "Make sure to fill out EOSConfig.cfg file");
                    missing = true;
                }
            }
        }

        if (!missing)
        {
            ProductName = (string)_configFile.GetValue("ProductSettings", "PRODUCT_NAME", "");
            ProductVersion = (string)_configFile.GetValue("ProductSettings", "PRODUCT_VERSION", "");
            
            ProductId = (string)_configFile.GetValue("Secrets", "EOS_PRODUCT_ID", "");
            SandboxId = (string)_configFile.GetValue("Secrets", "EOS_SANDBOX_ID", "");
            DeploymentId = (string)_configFile.GetValue("Secrets", "EOS_DEPLOYMENT_ID", "");
            ClientId = (string)_configFile.GetValue("Secrets", "EOS_CLIENT_ID", "");
            ClientSecret = (string)_configFile.GetValue("Secrets", "EOS_CLIENT_SECRET", "");
        }
        else
        {
            return Error.FileNotFound;
        }

        return Error.Ok;
    }
    
   
}