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
    private const string DefaultToken = "";
    private const string DefaultLibs = "";
    private const string ApiUrl = "http://localhost/ThouSands/api/gateway.php";

    public override void _EnterTree()
    {
        RemoveAnyExistingDockByName(SandBridgesDock.DockNodeName);

        _dock = new SandBridgesDock();

        string token = ProjectSettings.GetSetting(TokenKey, DefaultToken).AsString();
        string libsRefTokens = ProjectSettings.GetSetting(LibsKey, DefaultLibs).AsString();
        _dock.LoadSettings(libsRefTokens, token);

        _dock.SettingsChanged += OnSettingsChanged;

        AddControlToDock(DockSlot.RightUl, _dock);

        GD.Print("[SandBridges] Plugin enabled");
    }

    public override void _ExitTree()
    {
        if (_dock != null)
        {
            try { RemoveControlFromDocks(_dock); } catch { }
            _dock.QueueFree();
            _dock = null;
        }
        GD.Print("[SandBridges] Plugin disabled");
    }

    private void OnSettingsChanged(string libsRefTokens, string token)
    {
        ProjectSettings.SetSetting(LibsKey, libsRefTokens);
        ProjectSettings.SetSetting(TokenKey, token);
        ProjectSettings.Save();
        GD.Print($"[SandBridges] Settings saved: libs={libsRefTokens}, token={token}");
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
    private static void CGcall(Node calls, string libsRefTokens)
    {
        if (calls == null)
        {
            GD.PrintErr("[SandBridges] CGcall: undefined caller");
            return;
        }
        var cgProtocol = "https://www.thousands.org/api/cgc.php?skey=";
        var localdir = "./tgcg/udb.o";
        string libsRefTokens = ProjectSettings.GetSetting(LibsKey, DefaultLibs).AsString();
        if (string.IsNullOrEmpty(libsRefTokens))
        {
            GD.PrintErr("[SandBridges] undefined keys: your software reftokens is not set or saved in the settings.");
            return;
        }

        var reqst = new HttpRequest();
        calls.AddChild(reqst);
        string body =
            "libsIds=" + Uri.EscapeDataString(libsRefTokens);
        string fullUrl = cgProtocol + Uri.EscapeDataString(libsRefTokens);
        var headers = new string[] { "Content-Type: application/x-www-form-urlencoded" };
        Error err = reqst.Request(fullUrl, headers, HttpClient.Method.Post, body);
        if (err != Error.Ok)
        {
            GD.PrintErr("[SandBridges] HTTP request failed to initiate: " + err);
            reqst.QueueFree(); 
            return;
        }

        reqst.RequestCompleted += (result, code, hdrs, bytes) =>
        {
            string text = "";
            try { text = Encoding.UTF8.GetString(bytes); } catch { }
            GD.Print("[SandBridges] Response: " + code + " | Body: " + text);
            reqst.QueueFree();
        };
    }

// take profileTags from user saved data
// , string profileTags
    public static void InitiateAchievement(Node caller, string achievementId)
    {
        if (caller == null)
        {
            GD.PrintErr("[SandBridges] InitiateAchievement: caller is not set / set to null.");
            return;
        }

        string token = ProjectSettings.GetSetting(TokenKey, DefaultToken).AsString();
        if (string.IsNullOrEmpty(libsRefTokens))
        {
            GD.PrintErr("[SandBridges] undefined keys: your software reftokens is not set or saved in the settings.");
            return;
        }

        var request = new HttpRequest();
        caller.AddChild(request);
        string body =
            "libsIds=" + Uri.EscapeDataString(libsRefTokens) +
            "&achievementIds=" + Uri.EscapeDataString(achievementId) +
            "&profileTags=" + Uri.EscapeDataString(profileTag) +
            "&tokens=" + Uri.EscapeDataString(token);
        string fullUrl = ApiUrl + "?tokens=" + Uri.EscapeDataString(token);
        var headers = new string[] { "Content-Type: application/x-www-form-urlencoded" };
        Error err = request.Request(fullUrl, headers, HttpClient.Method.Post, body);
        if (err != Error.Ok)
        {
            GD.PrintErr("[SandBridges] HTTP request failed to initiate: " + err);
            request.QueueFree();
            return;
        }

        request.RequestCompleted += (result, code, hdrs, bytes) =>
        {
            string text = "";
            try { text = Encoding.UTF8.GetString(bytes); } catch { }
            GD.Print("[SandBridges] Response: " + code + " | Body: " + text);
            request.QueueFree();
        };
    }
}
#endif
