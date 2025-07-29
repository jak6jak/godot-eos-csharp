using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Connect;
using Godot;

namespace EOSPluign.addons.eosplugin;

[System.AttributeUsage(System.AttributeTargets.Property)]
public class ConfigFieldAttribute : System.Attribute
{
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public bool IsRequired { get; set; } = true;
    public bool IsSecret { get; set; } = false;
    public string Category { get; set; } = "General";
    
    public ConfigFieldAttribute(string displayName)
    {
        DisplayName = displayName;
    }
}

public class EOSConfiguration
{
    // Configuration Properties with attributes for UI generation
    [ConfigField("Product Name", Description = "The name of your product/game", Category = "Basic")]
    public string ProductName { get; set; } = "";

    [ConfigField("Product Version", Description = "Version of your product", Category = "Basic")]
    public string ProductVersion { get; set; } = "1.0.0";

    [ConfigField("Default Credential Type", Description = "Default authentication method", Category = "Authentication")]
    public LoginCredentialType DefaultCredentialType { get; set; } = LoginCredentialType.AccountPortal;

    [ConfigField("Default External Credential Type", Description = "Default external authentication method", Category = "Authentication")]
    public ExternalCredentialType DefaultExternalCredentialType { get; set; } = ExternalCredentialType.SteamSessionTicket;

    [ConfigField("EOS Product ID", Description = "Your EOS Product ID from Developer Portal", Category = "EOS Settings", IsSecret = true)]
    public string EosProductId { get; set; } = "";

    [ConfigField("EOS Sandbox ID", Description = "Your EOS Sandbox ID from Developer Portal", Category = "EOS Settings", IsSecret = true)]
    public string EosSandboxId { get; set; } = "";

    [ConfigField("EOS Deployment ID", Description = "Your EOS Deployment ID from Developer Portal", Category = "EOS Settings", IsSecret = true)]
    public string EosDeploymentId { get; set; } = "";

    [ConfigField("EOS Client ID", Description = "Your EOS Client ID from Developer Portal", Category = "EOS Settings", IsSecret = true)]
    public string EosClientId { get; set; } = "";

    [ConfigField("EOS Client Secret", Description = "Your EOS Client Secret from Developer Portal", Category = "EOS Settings", IsSecret = true)]
    public string EosClientSecret { get; set; } = "";

    [ConfigField("Dev Auth Port", Description = "Port for development authentication", Category = "Development", IsRequired = false)]
    public int DevAuthPort { get; set; } = 9876;

    [ConfigField("Dev Auth Token", Description = "Token for development authentication", Category = "Development", IsRequired = false)]
    public string DevAuthToken { get; set; } = "DevUser1";

    // Static instance for global access
    public static EOSConfiguration Instance { get; private set; } = new EOSConfiguration();

    // Legacy dictionary for backward compatibility
    //public static Dictionary<RequiredConfigFields, string> ConfigFields { get; private set; } = new Dictionary<RequiredConfigFields, string>();

    // Legacy enums for backward compatibility
    public enum RequiredConfigFields
    {
        ProductName,
        ProductVersion,
        DefaultCredentialType,
        EosProductId,
        EosSandboxId,
        EosDeploymentId,
        EosClientId,
        EosClientSecret,
    }

    public enum OptionalConfigFields
    {
        DevAuthPort,
        DevAuthToken,
    }

    private const string CONFIG_PATH = "res://EOSconfig.cfg";
    private static ConfigFile _configFile;
    private static string sectionID = "EOS";

    // Get all configurable properties using reflection
    public static List<PropertyInfo> GetConfigurableProperties()
    {
        return typeof(EOSConfiguration)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetCustomAttribute<ConfigFieldAttribute>() != null)
            .ToList();
    }

    // Get properties grouped by category
    public static Dictionary<string, List<PropertyInfo>> GetPropertiesByCategory()
    {
        var properties = GetConfigurableProperties();
        var grouped = new Dictionary<string, List<PropertyInfo>>();

        foreach (var prop in properties)
        {
            var attr = prop.GetCustomAttribute<ConfigFieldAttribute>();
            var category = attr.Category ?? "General";
            
            if (!grouped.ContainsKey(category))
                grouped[category] = new List<PropertyInfo>();
                
            grouped[category].Add(prop);
        }

        return grouped;
    }

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

        // Load values into the instance using reflection
        var properties = GetConfigurableProperties();
        
        foreach (var property in properties)
        {
            var attr = property.GetCustomAttribute<ConfigFieldAttribute>();
            string configKey = property.Name;
            
            if (_configFile.HasSection(sectionID) && _configFile.HasSectionKey(sectionID, configKey))
            {
                var value = _configFile.GetValue(sectionID, configKey);
                SetPropertyValue(Instance, property, value);
            }
            else if (attr.IsRequired)
            {
                GD.PushWarning($"Missing required field: {attr.DisplayName} ({configKey})");
                var defaultValue = property.GetValue(Instance);
                var variantValue = ConvertToVariant(defaultValue, property.PropertyType);
                _configFile.SetValue(sectionID, configKey, variantValue);
            }
        }

        // Update legacy ConfigFields dictionary for backward compatibility
        
        _configFile.Save(CONFIG_PATH);
        return Error.Ok;
    }

    public static void SaveConfig()
    {
        if (_configFile == null)
            _configFile = new ConfigFile();

        var properties = GetConfigurableProperties();
        
        foreach (var property in properties)
        {
            var value = property.GetValue(Instance);
            var variantValue = ConvertToVariant(value, property.PropertyType);
            _configFile.SetValue(sectionID, property.Name, variantValue);
        }

        _configFile.Save(CONFIG_PATH);
        GD.Print("EOS Configuration saved successfully.");
    }

    public static void CreateConfig()
    {
        _configFile = new ConfigFile();
        var properties = GetConfigurableProperties();
        
        foreach (var property in properties)
        {
            var defaultValue = property.GetValue(Instance);
            var variantValue = ConvertToVariant(defaultValue, property.PropertyType);
            _configFile.SetValue(sectionID, property.Name, variantValue);
        }
        
        _configFile.Save(CONFIG_PATH);
    }

    private static void SetPropertyValue(EOSConfiguration instance, PropertyInfo property, Variant value)
    {
        try
        {
            if (property.PropertyType == typeof(string))
            {
                property.SetValue(instance, value.AsString());
            }
            else if (property.PropertyType == typeof(int))
            {
                property.SetValue(instance, value.AsInt32());
            }
            else if (property.PropertyType == typeof(bool))
            {
                property.SetValue(instance, value.AsBool());
            }
            else if (property.PropertyType.IsEnum)
            {
                if (Enum.TryParse(property.PropertyType, value.AsString(), out var enumValue))
                {
                    property.SetValue(instance, enumValue);
                }
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to set property {property.Name}: {ex.Message}");
        }
    }

    private static string GetDefaultValueAsString(PropertyInfo property)
    {
        var defaultValue = property.GetValue(Instance);
        return defaultValue?.ToString() ?? "";
    }

    private static Variant ConvertToVariant(object value, Type propertyType)
    {
        if (value == null)
            return Variant.CreateFrom("");

        if (propertyType == typeof(string))
            return Variant.CreateFrom(value.ToString());
        
        if (propertyType == typeof(int))
            return Variant.CreateFrom((int)value);
        
        if (propertyType == typeof(bool))
            return Variant.CreateFrom((bool)value);
        
        if (propertyType.IsEnum)
            return Variant.CreateFrom(value.ToString());
        
        // Fallback to string representation
        return Variant.CreateFrom(value.ToString());
    }

    // Validation methods
    public static List<string> ValidateConfiguration()
    {
        var errors = new List<string>();
        var properties = GetConfigurableProperties();

        foreach (var property in properties)
        {
            var attr = property.GetCustomAttribute<ConfigFieldAttribute>();
            if (!attr.IsRequired) continue;

            var value = property.GetValue(Instance);
            
            if (property.PropertyType == typeof(string))
            {
                if (string.IsNullOrWhiteSpace(value?.ToString()))
                {
                    errors.Add($"{attr.DisplayName} is required but empty");
                }
            }
        }

        return errors;
    }

    public static bool IsValid()
    {
        return ValidateConfiguration().Count == 0;
    }
}