#if TOOLS

using System;
using System.Linq;
using System.Reflection;
using Godot;
using Godot.Collections;

namespace EOSPluign.addons.eosplugin.editor;

[Tool]
public partial class EosConfigMenu : Panel
{
    private VBoxContainer _mainContainer;
    private Button _saveButton;
    private Button _loadButton;
    private Button _validateButton;
    private Label _statusLabel;
    private ScrollContainer _scrollContainer;
    
    // Store references to input controls for easy access
    private Dictionary<string, Control> _inputControls = new Dictionary<string, Control>();
    
    [Signal]
    public delegate void ConfigurationSavedEventHandler();
    
    [Signal]
    public delegate void ConfigurationLoadedEventHandler();

    public override void _EnterTree()
    {
        GD.Print("EosConfigMenu::_EnterTree()");
        
        CreateUI();
        LoadConfiguration();
        
        GD.Print($"EosConfigMenu: Panel size: {Size}");
        GD.Print($"EosConfigMenu: Created {_inputControls.Count} input controls");
    }
    
   

    private void CreateUI()
    {
        // Set up panel sizing
        SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        
        // Main container
        _mainContainer = new VBoxContainer();
        _mainContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _mainContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        // Set anchors manually for full rect
        _mainContainer.AnchorLeft = 0;
        _mainContainer.AnchorTop = 0;
        _mainContainer.AnchorRight = 1;
        _mainContainer.AnchorBottom = 1;
        AddChild(_mainContainer);
        
        // Title
        var titleLabel = new Label();
        titleLabel.Text = "EOS Configuration";
        titleLabel.AddThemeStyleboxOverride("normal", new StyleBoxFlat());
        titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        titleLabel.AddThemeStyleboxOverride("normal", CreateHeaderStyleBox());
        _mainContainer.AddChild(titleLabel);
        
        // Add some spacing
        var spacer1 = new Control();
        spacer1.CustomMinimumSize = new Vector2(0, 10);
        _mainContainer.AddChild(spacer1);
        
        // Scroll container for the form
        _scrollContainer = new ScrollContainer();
        _scrollContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _scrollContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _scrollContainer.CustomMinimumSize = new Vector2(300, 400);
        _mainContainer.AddChild(_scrollContainer);
        
        var formContainer = new VBoxContainer();
        formContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        formContainer.AddThemeConstantOverride("separation", 10);
        _scrollContainer.AddChild(formContainer);
        
        // Generate form fields dynamically
        GenerateFormFields(formContainer);
        
        // Add spacer before buttons
        var spacer2 = new Control();
        spacer2.CustomMinimumSize = new Vector2(0, 10);
        _mainContainer.AddChild(spacer2);
        
        // Button container
        var buttonContainer = new HBoxContainer();
        buttonContainer.Alignment = BoxContainer.AlignmentMode.Center;
        buttonContainer.AddThemeConstantOverride("separation", 10);
        _mainContainer.AddChild(buttonContainer);
        
        // Load button
        _loadButton = new Button();
        _loadButton.Text = "Load Config";
        _loadButton.Pressed += LoadConfiguration;
        buttonContainer.AddChild(_loadButton);
        
        // Save button
        _saveButton = new Button();
        _saveButton.Text = "Save Config";
        _saveButton.Pressed += SaveConfiguration;
        buttonContainer.AddChild(_saveButton);
        
        // Validate button
        _validateButton = new Button();
        _validateButton.Text = "Validate";
        _validateButton.Pressed += ValidateConfiguration;
        buttonContainer.AddChild(_validateButton);
        
        // Status label
        _statusLabel = new Label();
        _statusLabel.Text = "Ready";
        _statusLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _statusLabel.AddThemeColorOverride("font_color", Colors.Gray);
        _mainContainer.AddChild(_statusLabel);
    }

    private void GenerateFormFields(VBoxContainer container)
    {
        var propertiesByCategory = EOSConfiguration.GetPropertiesByCategory();
        
        foreach (var category in propertiesByCategory.Keys.OrderBy(k => k))
        {
            GD.Print("Generating fields for category: " + category);
            // Category header
            var categoryLabel = new Label();
            categoryLabel.Text = category;
            categoryLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            categoryLabel.AddThemeStyleboxOverride("normal", CreateCategoryStyleBox());
            categoryLabel.AddThemeColorOverride("font_color", Colors.White);
            categoryLabel.CustomMinimumSize = new Vector2(0, 30);
            container.AddChild(categoryLabel);
            
            // Category container
            var categoryContainer = new VBoxContainer();
            categoryContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            categoryContainer.AddThemeConstantOverride("separation", 8);
            container.AddChild(categoryContainer);
            
            foreach (var property in propertiesByCategory[category])
            {
                var attr = property.GetCustomAttribute<ConfigFieldAttribute>();
                GD.Print($"Creating field control for: {attr.DisplayName} ({property.Name})");
                CreateFieldControl(categoryContainer, property, attr);
            }
            
            // Add spacing between categories
            var spacer = new Control();
            spacer.CustomMinimumSize = new Vector2(0, 10);
            container.AddChild(spacer);
        }
    }

    private void CreateFieldControl(VBoxContainer parent, PropertyInfo property, ConfigFieldAttribute attr)
    {
        var fieldContainer = new VBoxContainer();
        fieldContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        fieldContainer.AddThemeConstantOverride("separation", 3);
        parent.AddChild(fieldContainer);
        
        // Label
        var label = new Label();
        label.Text = attr.DisplayName + (attr.IsRequired ? " *" : "");
        label.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        if (attr.IsRequired)
        {
            label.AddThemeColorOverride("font_color", Colors.LightBlue);
        }
        fieldContainer.AddChild(label);
        
        // Description (if provided)
        if (!string.IsNullOrEmpty(attr.Description))
        {
            var descLabel = new Label();
            descLabel.Text = attr.Description;
            descLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            descLabel.AddThemeColorOverride("font_color", Colors.Gray);
            descLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            fieldContainer.AddChild(descLabel);
        }
        
        // Input control based on property type
        Control inputControl = CreateInputControlForProperty(property, attr);
        if (inputControl != null)
        {
            inputControl.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            inputControl.CustomMinimumSize = new Vector2(250, 0);
            fieldContainer.AddChild(inputControl);
            _inputControls[property.Name] = inputControl;
        }
        
        // Add small spacer after each field
        var fieldSpacer = new Control();
        fieldSpacer.CustomMinimumSize = new Vector2(0, 5);
        fieldContainer.AddChild(fieldSpacer);
    }

    private Control CreateInputControlForProperty(PropertyInfo property, ConfigFieldAttribute attr)
    {
        if (property.PropertyType == typeof(string))
        {
            var lineEdit = new LineEdit();
            lineEdit.PlaceholderText = attr.DisplayName;
            
            if (attr.IsSecret)
            {
                lineEdit.Secret = true;
            }
            
            return lineEdit;
        }
        else if (property.PropertyType == typeof(int))
        {
            var spinBox = new SpinBox();
            spinBox.AllowGreater = true;
            spinBox.AllowLesser = true;
            spinBox.Step = 1;
            return spinBox;
        }
        else if (property.PropertyType == typeof(bool))
        {
            var checkBox = new CheckBox();
            checkBox.Text = attr.DisplayName;
            return checkBox;
        }
        else if (property.PropertyType.IsEnum)
        {
            var optionButton = new OptionButton();
            var enumValues = Enum.GetValues(property.PropertyType);
            
            foreach (var enumValue in enumValues)
            {
                optionButton.AddItem(enumValue.ToString());
            }
            
            return optionButton;
        }
        
        return null;
    }

    private void LoadConfiguration()
    {
        try
        {
            EOSConfiguration.LoadConfig();
            UpdateUIFromConfiguration();
            UpdateStatus("Configuration loaded successfully", Colors.Green);
            EmitSignal(SignalName.ConfigurationLoaded);
        }
        catch (Exception ex)
        {
            UpdateStatus($"Failed to load configuration: {ex.Message}", Colors.Red);
            GD.PrintErr($"Failed to load EOS configuration: {ex}");
        }
    }

    private void SaveConfiguration()
    {
        try
        {
            UpdateConfigurationFromUI();
            EOSConfiguration.SaveConfig();
            UpdateStatus("Configuration saved successfully", Colors.Green);
            EmitSignal(SignalName.ConfigurationSaved);
        }
        catch (Exception ex)
        {
            UpdateStatus($"Failed to save configuration: {ex.Message}", Colors.Red);
            GD.PrintErr($"Failed to save EOS configuration: {ex}");
        }
    }

    // Public method for external saving (e.g., when window closes)
    public void SaveConfigurationOnClose()
    {
        try
        {
            UpdateConfigurationFromUI();
            EOSConfiguration.SaveConfig();
            GD.Print("EOS Configuration auto-saved on window close");
            EmitSignal(SignalName.ConfigurationSaved);
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to auto-save EOS configuration on close: {ex}");
        }
    }

    private void ValidateConfiguration()
    {
        UpdateConfigurationFromUI();
        var errors = EOSConfiguration.ValidateConfiguration();
        
        if (errors.Count == 0)
        {
            UpdateStatus("Configuration is valid", Colors.Green);
        }
        else
        {
            var errorMessage = "Validation errors:\n" + string.Join("\n", errors);
            UpdateStatus(errorMessage, Colors.Red);
        }
    }

    private void UpdateUIFromConfiguration()
    {
        var properties = EOSConfiguration.GetConfigurableProperties();
        
        foreach (var property in properties)
        {
            if (!_inputControls.ContainsKey(property.Name))
                continue;
                
            var control = _inputControls[property.Name];
            var value = property.GetValue(EOSConfiguration.Instance);
            
            SetControlValue(control, value, property.PropertyType);
        }
    }

    private void UpdateConfigurationFromUI()
    {
        var properties = EOSConfiguration.GetConfigurableProperties();
        
        foreach (var property in properties)
        {
            if (!_inputControls.ContainsKey(property.Name))
                continue;
                
            var control = _inputControls[property.Name];
            var value = GetControlValue(control, property.PropertyType);
            
            if (value != null)
            {
                property.SetValue(EOSConfiguration.Instance, value);
            }
        }
    }

    private void SetControlValue(Control control, object value, Type propertyType)
    {
        switch (control)
        {
            case LineEdit lineEdit:
                lineEdit.Text = value?.ToString() ?? "";
                break;
                
            case SpinBox spinBox:
                if (value is int intValue)
                    spinBox.Value = intValue;
                break;
                
            case CheckBox checkBox:
                if (value is bool boolValue)
                    checkBox.ButtonPressed = boolValue;
                break;
                
            case OptionButton optionButton:
                if (value != null && propertyType.IsEnum)
                {
                    var enumName = value.ToString();
                    for (int i = 0; i < optionButton.ItemCount; i++)
                    {
                        if (optionButton.GetItemText(i) == enumName)
                        {
                            optionButton.Selected = i;
                            break;
                        }
                    }
                }
                break;
        }
    }

    private object GetControlValue(Control control, Type propertyType)
    {
        return control switch
        {
            LineEdit lineEdit => lineEdit.Text,
            SpinBox spinBox => (int)spinBox.Value,
            CheckBox checkBox => checkBox.ButtonPressed,
            OptionButton optionButton when propertyType.IsEnum => 
                Enum.Parse(propertyType, optionButton.GetItemText(optionButton.Selected)),
            _ => null
        };
    }

    private void UpdateStatus(string message, Color color)
    {
        _statusLabel.Text = message;
        _statusLabel.AddThemeColorOverride("font_color", color);
        
        // Auto-clear status after 5 seconds
        GetTree().CreateTimer(5.0).Timeout += () => {
            if (_statusLabel.Text == message) // Only clear if it hasn't changed
            {
                _statusLabel.Text = "Ready";
                _statusLabel.AddThemeColorOverride("font_color", Colors.Gray);
            }
        };
    }

    private StyleBoxFlat CreateHeaderStyleBox()
    {
        var styleBox = new StyleBoxFlat();
        styleBox.BgColor = new Color(0.2f, 0.3f, 0.4f);
        styleBox.BorderWidthBottom = 2;
        styleBox.BorderColor = Colors.LightBlue;
        styleBox.SetContentMarginAll(10);
        return styleBox;
    }

    private StyleBoxFlat CreateCategoryStyleBox()
    {
        var styleBox = new StyleBoxFlat();
        styleBox.BgColor = new Color(0.15f, 0.25f, 0.35f);
        styleBox.BorderWidthLeft = 3;
        styleBox.BorderColor = Colors.Orange;
        styleBox.SetContentMarginAll(5);
        return styleBox;
    }
    
}
#endif