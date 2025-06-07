using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using ECommons;
using ECommons.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SomethingNeedDoing.Core.Interfaces;
using SomethingNeedDoing.Gui;

namespace SomethingNeedDoing;

public sealed class Plugin : IDalamudPlugin
{
    public string Name => Svc.PluginInterface.InternalName;
    internal string Prefix => "SND";
    internal string[] Aliases => ["/snd", "/pcraft"];

    internal static Plugin P { get; private set; } = null!;
    internal static Config C { get; private set; } = null!;

    private const string Command = "/somethingneeddoing";
    private readonly ServiceProvider _serviceProvider;
    private readonly WindowSystem _windowSystem;
    private readonly IMacroScheduler _macroScheduler;

    /*
     * verify that function level triggers work
     * write better docs for all help tabs
     * change the reset gitinfo and version history buttons, and the populate metadata button
     * move the check for updates to right align?
     * fix line numbers not scrolling
     * add checks for plugin dependencies (and macro dependencies?)
     * add pause/resume buttons to editor
     * make sure all config options are in settings
     * see if any of the ui code can be made generic
     * make a command service?
     * get rid of changing cfg by command?
     */

    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        P = this;
        ECommonsMain.Init(pluginInterface, this, Module.ObjectFunctions, Module.DalamudReflector);

        EzConfig.DefaultSerializationFactory = new ConfigFactory();
        C = EzConfig.Init<Config>();
        EzConfig.Save();

        // Set up dependency injection
        var services = new ServiceCollection();
        services.AddSomethingNeedDoingServices();
        _serviceProvider = services.BuildServiceProvider();

        // Get required services
        _windowSystem = _serviceProvider.GetRequiredService<WindowSystem>();
        _windowSystem.AddWindow(_serviceProvider.GetRequiredService<MainWindow>());
        _macroScheduler = _serviceProvider.GetRequiredService<IMacroScheduler>();

        Svc.PluginInterface.UiBuilder.Draw += _windowSystem.Draw;
        Svc.PluginInterface.UiBuilder.OpenConfigUi += ToggleMainWindow;
        EzCmd.Add(Command, OnChatCommand, "Open a window to edit various settings.", displayOrder: int.MinValue);
        Aliases.ToList().ForEach(a => EzCmd.Add(a, OnChatCommand, $"{Command} Alias"));
    }

    private void ToggleMainWindow() => _windowSystem.Toggle<MainWindow>();

    public void Dispose()
    {
        Svc.PluginInterface.UiBuilder.Draw -= _windowSystem.Draw;
        Svc.PluginInterface.UiBuilder.OpenConfigUi -= ToggleMainWindow;
        _windowSystem.RemoveAllWindows();
        _serviceProvider.Dispose();
        ECommonsMain.Dispose();
    }

    private void OnChatCommand(string command, string arguments)
    {
        arguments = arguments.Trim();

        if (arguments == string.Empty)
        {
            ToggleMainWindow();
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
