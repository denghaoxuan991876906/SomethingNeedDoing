using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using ECommons;
using ECommons.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SomethingNeedDoing.Framework.Interfaces;
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
    private readonly MainWindow _mainWindow;
    private readonly MacroStatusWindow _macroStatusWindow;
    private readonly IMacroScheduler _macroScheduler;
    private bool _isFirstDraw = true;

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
        _macroScheduler = _serviceProvider.GetRequiredService<IMacroScheduler>();
        _mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        _macroStatusWindow = _serviceProvider.GetRequiredService<MacroStatusWindow>();

        // Initialize UI
        _windowSystem.AddWindow(_mainWindow);
        _windowSystem.AddWindow(_macroStatusWindow);

        // Set up commands and UI
        Svc.Framework.RunOnFrameworkThread(() =>
        {
            Svc.PluginInterface.UiBuilder.Draw += CheckFontsOnFirstDraw;
            Svc.PluginInterface.UiBuilder.Draw += _windowSystem.Draw;
            Svc.PluginInterface.UiBuilder.OpenConfigUi += ToggleMainWindow;
            EzCmd.Add(Command, OnChatCommand, "Open a window to edit various settings.", displayOrder: int.MaxValue);
            Aliases.ToList().ForEach(a => EzCmd.Add(a, OnChatCommand, $"{Command} Alias"));
        });

        EzCmd.Add("/sndstatus", ToggleStatusWindow, "Toggle the macro status window");
    }

    private void CheckFontsOnFirstDraw()
    {
        if (!_isFirstDraw) return;
        _isFirstDraw = false;

        // Unregister this handler after first draw
        Svc.PluginInterface.UiBuilder.Draw -= CheckFontsOnFirstDraw;

        // Check if IconFont is properly loaded
        var isFontValid = UiBuilder.IconFont.IsLoaded();

        if (!isFontValid)
        {
            Svc.Chat.PrintError("[SomethingNeedDoing] WARNING: FontAwesome icon font is not loaded properly.");
            Svc.Chat.PrintError("[SomethingNeedDoing] This may cause toolbar icons to display incorrectly.");
            Svc.Chat.Print("[SomethingNeedDoing] Try reloading the plugin if icons appear as '=' characters.");
        }
        else
        {
            Svc.Chat.Print("[SomethingNeedDoing] FontAwesome icon font loaded successfully.");

            // Check first time run and show tutorial
            if (!C.HasCompletedTutorial)
            {
                Svc.Chat.Print("[SomethingNeedDoing] Welcome! Type /snd to open the main window.");
                C.HasCompletedTutorial = true;
                C.Save();
            }
        }
    }

    private void ToggleMainWindow()
    {
        _mainWindow.IsOpen = !_mainWindow.IsOpen;
    }

    public void Dispose()
    {
        Svc.PluginInterface.UiBuilder.Draw -= CheckFontsOnFirstDraw;
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
            _mainWindow.IsOpen = !_mainWindow.IsOpen;
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

    private void ToggleStatusWindow(string command, string args)
    {
        // Toggle status window visibility
        _macroStatusWindow.IsOpen = !_macroStatusWindow.IsOpen;

        // If we're opening it, bring it to front
        if (_macroStatusWindow.IsOpen)
            _macroStatusWindow.BringToFront();
    }
}
