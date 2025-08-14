#if TOOLS
using Godot;
using System;

[Tool]
public partial class SandBridgesDock : VBoxContainer
{
    public const string DockNodeName = "SandBridges";
    private LineEdit _libsEdit;
    private LineEdit _tokenEdit;
    private Button _saveButton;
    public Action<string, string> SettingsChanged;

    public SandBridgesDock()
    {
        Name = DockNodeName;
    }

    public override void _Ready()
    {
        BuildUi();
    }

    private void BuildUi()
    {
        ClearChildren();

        AddChild(new Label { Text = "SandBridges Settings", ThemeTypeVariation = "HeaderSmall" });
        _libsEdit = new LineEdit { PlaceholderText = "Enter your software ids..." };
        AddChild(_libsEdit);
        _tokenEdit = new LineEdit { PlaceholderText = "Enter connection tokens..." };
        AddChild(_tokenEdit);
        _saveButton = new Button { Text = "Save Settings" };
        _saveButton.Pressed += OnSavePressed;
        AddChild(_saveButton);

        CustomMinimumSize = new Vector2(0, 150);
    }

    private void OnSavePressed()
    {
        if (SettingsChanged != null)
            SettingsChanged(_libsEdit.Text, _tokenEdit.Text);
    }

    public void LoadSettings(string libsId, string token)
    {
        CallDeferred(nameof(_LoadSettingsDeferred), libsId, token);
    }

    private void _LoadSettingsDeferred(string libsId, string token)
    {
        if (_libsEdit != null) _libsEdit.Text = libsId;
        if (_tokenEdit != null) _tokenEdit.Text = token;
    }

    private void ClearChildren()
    {
        foreach (Node child in GetChildren())
            child.QueueFree();
    }
}
#endif
