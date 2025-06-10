using Dalamud.Plugin;
using ECommons;
using ECommons.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SomethingNeedDoing.Services;

namespace SomethingNeedDoing;

public sealed class Plugin : IDalamudPlugin
{
    public string Name => Svc.PluginInterface.InternalName;
    internal string Prefix => "SND";

    internal static Plugin P { get; private set; } = null!;
    internal static Config C { get; private set; } = null!;

    private readonly ServiceProvider _serviceProvider;

    /*
     * write better docs for all help tabs
     * move the check for updates to right align?
     * fix line numbers not scrolling
     * get rid of changing cfg by command?
     */

    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        P = this;
        ECommonsMain.Init(pluginInterface, this, Module.ObjectFunctions, Module.DalamudReflector);

        EzConfig.DefaultSerializationFactory = new ConfigFactory();
        C = EzConfig.Init<Config>();
        EzConfig.Save();

        _serviceProvider = new ServiceCollection().SetupPluginServices().BuildServiceProvider();
        _ = _serviceProvider.GetRequiredService<WindowService>();
        _ = _serviceProvider.GetRequiredService<CommandService>();
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
        ECommonsMain.Dispose();
    }
}
