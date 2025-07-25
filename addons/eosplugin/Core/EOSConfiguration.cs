using System;
using System.Collections.Generic;
using System.Linq;
using Epic.OnlineServices.Auth;
using Godot;

namespace EOSPluign.addons.eosplugin;

public class EOSConfiguration
{
    
    public enum Configfield{
        ProductName,
        ProductVersion,
        DefaultCredentialType,
        EosProductId,
        EosSandboxId,
        EosDeploymentId,
        EosClientId,
        EosClientSecret,
            
    }
    
    public static Dictionary<Configfield, string> ConfigFields { get; private set; } = new Dictionary<Configfield, string>();
    public EOSConfiguration()
    {
        //var err =  LoadConfig();
    }
    
    private const string CONFIG_PATH = "res://EOSconfig.cfg";
    private static ConfigFile _configFile;
    private static string sectionID = "EOS";
    public static Error LoadConfig()
    {
        _configFile = new ConfigFile();
        Error err = _configFile.Load(CONFIG_PATH);
        if (err != Error.Ok)
        {
            if (err == Error.FileNotFound)
            {
                CreateConfig();
                GD.PushWarning("Config file not found. Creating new config file.");
                return err;
            }
            GD.PushWarning("Error loading config file: " + err.ToString());
            return err;
        }
        
        foreach (var fieldsKey in Enum.GetValues(typeof(Configfield)).Cast<Configfield>())
        {
            
            string val = (string)_configFile.GetValue(sectionID, fieldsKey.ToString(), "bazinga");
            if ( val == "")
            {
                GD.PushWarning("Missing value for " + fieldsKey.ToString() + " in EOS section. " +
                               "Make sure to fill out EOSConfig.cfg file");
            } else if (val == "bazinga")
            {
                GD.PushWarning("Missing entire Field for " + fieldsKey.ToString() + "in EOS config file." +
                               "Creating Field with empty value.");
                _configFile.SetValue(sectionID, fieldsKey.ToString(), "");
            }
            else
            {
                ConfigFields[fieldsKey] = val;
            }

            _configFile.Save(CONFIG_PATH);
        }
        
        return Error.Ok;
    }

    public static void CreateConfig()
    {
        _configFile = new ConfigFile();
        foreach (var configField in Enum.GetValues(typeof(Configfield)).Cast<Configfield>())
        {
           _configFile.SetValue(sectionID, configField.ToString(), ""); 
        }
        _configFile.Save(CONFIG_PATH);
    }
   
}