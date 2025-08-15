#if TOOLS
using Godot;
using System;
using System.Text;

[Tool]
public partial class SandBridges : EditorPlugin
{
    private SandBridgesDock _dock;
    private const string TokenKey = "sandbridges/connect_tokens";
    private const string LibsKey = "sandbridges/libs_id";
    private const string DefaultToken = "tokens01";
    private const string DefaultLibs = "0";
    private const string ApiUrl = "http://localhost/ThouSands/api/gateway.php";

    public override void _EnterTree()
    {
        RemoveAnyExistingDockByName(SandBridgesDock.DockNodeName);

        _dock = new SandBridgesDock();

        string token = ProjectSettings.GetSetting(TokenKey, DefaultToken).AsString();
        string libsId = ProjectSettings.GetSetting(LibsKey, DefaultLibs).AsString();
        _dock.LoadSettings(libsId, token);

        _dock.SettingsChanged += OnSettingsChanged;

        AddControlToDock(DockSlot.RightUl, _dock);

        GD.Print("[SandBridges] Plugin loaded.");
    }

    public override void _ExitTree()
    {
        if (_dock != null)
        {
            try { RemoveControlFromDocks(_dock); } catch { }
            _dock.QueueFree();
            _dock = null;
        }
        GD.Print("[SandBridges] Plugin unloaded.");
    }

    private void OnSettingsChanged(string libsId, string token)
    {
        ProjectSettings.SetSetting(LibsKey, libsId);
        ProjectSettings.SetSetting(TokenKey, token);
        ProjectSettings.Save();
        GD.Print($"[SandBridges] Settings saved: libs={libsId}, token={token}");
    }

    private void RemoveAnyExistingDockByName(string name)
    {
        Control root = GetEditorInterface().GetBaseControl();
        if (root == null) return;
        Node found = root.FindChild(name, true, false);
        if (found is Control ctrl)
        {
            try { RemoveControlFromDocks(ctrl); } catch { }
            ctrl.QueueFree();
        }
    }

    public static void InitiateAchievement(Node caller, string achievementId)
    {
        if (caller == null)
        {
            GD.PrintErr("[SandBridges] InitiateAchievement: caller is not set / set to null.");
            return;
        }

        string token = ProjectSettings.GetSetting(TokenKey, DefaultToken).AsString();
        string libsId = ProjectSettings.GetSetting(LibsKey, DefaultLibs).AsString();

        if (string.IsNullOrEmpty(libsId))
        {
            GD.PrintErr("[SandBridges] InitiateAchievement: software id is not set in settings dock.");
            return;
        }

        var request = new HttpRequest();
        caller.AddChild(request);
        string body =
            "libsIds=" + Uri.EscapeDataString(libsId) +
            "&achievementIds=" + Uri.EscapeDataString(achievementId) +
            "&profileTags=" + Uri.EscapeDataString(profileTag) +
            "&tokens=" + Uri.EscapeDataString(token);
        string fullUrl = ApiUrl + "?tokens=" + Uri.EscapeDataString(token);
        var headers = new string[] { "Content-Type: application/x-www-form-urlencoded" };
        Error err = request.Request(fullUrl, headers, HttpClient.Method.Post, body);
        if (err != Error.Ok)
        {
            GD.PrintErr("[SandBridges] HTTP request failed to start: " + err);
            request.QueueFree();
            return;
        }

        request.RequestCompleted += (result, code, hdrs, bytes) =>
        {
            string text = "";
            try { text = Encoding.UTF8.GetString(bytes); } catch { }
            GD.Print("[SandBridges] Response code: " + code + " | Body: " + text);
            request.QueueFree();
        };
    }
}
#endif
