using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using ECommons;
using ECommons.Configuration;
using ECommons.SimpleGui;
using Microsoft.Extensions.DependencyInjection;
using SomethingNeedDoing.Gui;

namespace SomethingNeedDoing;

public sealed class Plugin : IDalamudPlugin
{
    internal string Name => "Something Need Doing (Expanded Edition)";
    internal string Prefix => "SND";
    internal string[] Aliases => ["/snd", "/pcraft"];

    internal static Plugin P { get; private set; } = null!;
    internal static Config C => P._config;

    private readonly Config _config = null!;
    private const string Command = "/somethingneeddoing";
    private readonly ServiceProvider _serviceProvider;
    private readonly WindowSystem _windowSystem;
    private readonly MacroUI _macroUI;
    private readonly IMacroScheduler _macroScheduler;

    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        P = this;
        ECommonsMain.Init(pluginInterface, this, Module.ObjectFunctions, Module.DalamudReflector);

        EzConfig.DefaultSerializationFactory = new ConfigFactory();
        EzConfig.Migrate<Config>();
        _config = EzConfig.Init<Config>();
        //_config.Migrate(_config);
        //_config.ValidateMigration();
        EzConfig.Save();

        // Set up dependency injection
        var services = new ServiceCollection();
        services.AddSomethingNeedDoingServices();
        _serviceProvider = services.BuildServiceProvider();

        // Get required services
        _windowSystem = _serviceProvider.GetRequiredService<WindowSystem>();
        _macroUI = _serviceProvider.GetRequiredService<MacroUI>();
        _macroScheduler = _serviceProvider.GetRequiredService<IMacroScheduler>();

        // Initialize UI
        _windowSystem.AddWindow(_macroUI);

        // Set up commands and UI
        Svc.Framework.RunOnFrameworkThread(() =>
        {
            Svc.PluginInterface.UiBuilder.Draw += DrawDevBarEntry;
            Svc.PluginInterface.UiBuilder.Draw += _windowSystem.Draw;
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
            }
            ImGui.EndMainMenuBar();
        }
    }

    public void Dispose()
    {
        Svc.PluginInterface.UiBuilder.Draw -= _windowSystem.Draw;
        _windowSystem.RemoveAllWindows();
        Svc.PluginInterface.UiBuilder.Draw -= DrawDevBarEntry;
        _serviceProvider.Dispose();
        ECommonsMain.Dispose();
    }

    private void OnChatCommand(string command, string arguments)
    {
        arguments = arguments.Trim();

        if (arguments == string.Empty)
        {
            _macroUI.Toggle();
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
                _ = _macroScheduler.StartMacro(macro);
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
