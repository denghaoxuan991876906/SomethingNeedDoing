using Microsoft.Extensions.DependencyInjection;
using SomethingNeedDoing.Gui;
using SomethingNeedDoing.External;
using Dalamud.Interface.Windowing;

namespace SomethingNeedDoing.Utils;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSomethingNeedDoingServices(this IServiceCollection services)
    {
        // Core Services
        services.AddSingleton<MacroParser>();
        services.AddSingleton<NativeMacroEngine>();
        services.AddSingleton<LuaMacroEngine>();
        services.AddSingleton<TriggerEventManager>();
        services.AddSingleton<IMacroScheduler, MacroScheduler>();
        services.AddSingleton<IMacroEngine, NativeMacroEngine>();
        services.AddSingleton<IMacroEngine, LuaMacroEngine>();
        services.AddSingleton<LuaModuleManager>();
        services.AddSingleton<GitMacroManager>();

        // UI Services
        services.AddSingleton<RunningMacrosPanel>();
        services.AddSingleton<MacroUI>();
        services.AddSingleton<WindowSystem>();

        // External Services
        services.AddSingleton<Tippy>();

        // Framework Services
        services.AddTransient<MacroContext>();

        return services;
    }
}
