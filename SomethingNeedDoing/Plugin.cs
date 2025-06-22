using Dalamud.Plugin;
using ECommons;
using ECommons.Configuration;
using ECommons.Singletons;
using Microsoft.Extensions.DependencyInjection;
using SomethingNeedDoing.Services;

namespace SomethingNeedDoing;

public sealed class Plugin : IDalamudPlugin
{
    public string Name => Svc.PluginInterface.InternalName;
    internal string Prefix => "SND";

    internal static Plugin P { get; private set; } = null!;
    internal static Config C { get; private set; } = null!;
    internal string Version => Svc.PluginInterface.Manifest.AssemblyVersion.ToString(2);

    private readonly ServiceProvider _serviceProvider;

    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        P = this;
        ECommonsMain.Init(pluginInterface, this, Module.ObjectFunctions, Module.DalamudReflector);

        EzConfig.DefaultSerializationFactory = new ConfigFactory();
        ReloadConfig();

        _serviceProvider = new ServiceCollection().SetupPluginServices().BuildServiceProvider();
        _ = _serviceProvider.GetRequiredService<WindowService>();
        _ = _serviceProvider.GetRequiredService<CommandService>();
        _ = _serviceProvider.GetRequiredService<StubGeneratorService>();
        SingletonServiceManager.Initialize(typeof(StaticsService)); // rip 100% DI
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
        ECommonsMain.Dispose();
    }

    public void ReloadConfig()
    {
        C = EzConfig.Init<Config>();
        EzConfig.Save();
    }
}
