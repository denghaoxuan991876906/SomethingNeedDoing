using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using ECommons;
using ECommons.Configuration;
using ECommons.SimpleGui;
using ECommons.Singletons;

namespace SomethingNeedDoing;

public sealed class Plugin : IDalamudPlugin
{
    internal string Name => "Something Need Doing (Expanded Edition)";
    internal string Prefix => "SND";
    internal string[] Aliases => ["/snd", "/pcraft"];

    internal static Plugin P { get; private set; } = null!;
    internal static Config C => P.Config;

    private readonly Config Config = null!;
    private const string Command = "/somethingneeddoing";
    internal WindowSystem _ws = new("SomethingNeedDoing");
    private Gui.MacroUI _macroui = null!;

    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        P = this;
        pluginInterface.Create<Service>();
        ECommonsMain.Init(pluginInterface, this, Module.ObjectFunctions, Module.DalamudReflector);

        EzConfig.DefaultSerializationFactory = new ConfigFactory();
        EzConfig.Migrate<Config>();
        Config = EzConfig.Init<Config>();
        Config.Migrate(Config);
        Config.ValidateMigration();
        EzConfig.Save();

        SingletonServiceManager.Initialize(typeof(Service));
        Svc.Framework.RunOnFrameworkThread(() =>
        {
            _macroui = new();
            _ws.AddWindow(_macroui);
            //EzConfigGui.Init(new Gui.MacroUI().Draw);
            // EzConfigGui.Init(new Windows.MacrosUI().Draw);
            // EzConfigGui.WindowSystem.AddWindow(new HelpUI());
            // EzConfigGui.WindowSystem.AddWindow(new ExcelWindow());
            Svc.PluginInterface.UiBuilder.Draw += DrawDevBarEntry;
            Svc.PluginInterface.UiBuilder.Draw += _ws.Draw;
            EzCmd.Add(Command, OnChatCommand, "Open a window to edit various settings.", displayOrder: int.MaxValue);
            Aliases.ToList().ForEach(a => EzCmd.Add(a, OnChatCommand, $"{Command} Alias"));
        });
    }

    private void DrawDevBarEntry()
    {
        if (Svc.PluginInterface.IsDevMenuOpen && ImGui.BeginMainMenuBar())
        {
            if (ImGui.MenuItem("SND Excel"))
            {
                if (ImGui.GetIO().KeyShift)
                    EzConfigGui.Toggle();
                // else
                //     EzConfigGui.GetWindow<ExcelWindow>()!.Toggle();
            }
            ImGui.EndMainMenuBar();
        }
    }

    public void Dispose()
    {
        Svc.PluginInterface.UiBuilder.Draw -= _ws.Draw;
        _ws.RemoveAllWindows();
        Svc.PluginInterface.UiBuilder.Draw -= DrawDevBarEntry;

        ECommonsMain.Dispose();
    }

    private void OnChatCommand(string command, string arguments)
    {
        arguments = arguments.Trim();

        if (arguments == string.Empty)
        {
            _macroui.Toggle();
            //EzConfigGui.Window.IsOpen ^= true;
            return;
        }
        else if (arguments.StartsWith("run "))
        {
            arguments = arguments[4..].Trim();

            var loopCount = 0u;
            if (arguments.StartsWith("loop "))
            {
                arguments = arguments[5..].Trim();
                var nextSpace = arguments.IndexOf(' ');
                if (nextSpace == -1)
                {
                    Svc.Chat.PrintError("Could not determine loop count");
                    return;
                }

                if (!uint.TryParse(arguments[..nextSpace], out loopCount))
                {
                    Svc.Chat.PrintError("Could not parse loop count");
                    return;
                }

                arguments = arguments[(nextSpace + 1)..].Trim();
            }

            var macroName = arguments.Trim('"');
            if (C.GetMacroByName(macroName) is { } macro)
                _ = Service.MacroScheduler.StartMacro(macro);
            // TODO: start a macro with a given loopcount
            return;
        }
        else if (arguments.StartsWith("cfg"))
        {
            var args = arguments[4..].Trim().Split(" ");
            C.SetProperty(args[0], args[1]);
            return;
        }
    }
}
