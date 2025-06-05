using Dalamud.Interface.Windowing;
using Microsoft.Extensions.DependencyInjection;
using SomethingNeedDoing.Core.Interfaces;
using System.Net.Http;

namespace SomethingNeedDoing.Utils;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSomethingNeedDoingServices(this IServiceCollection services)
    {
        services.AddSingleton<WindowSystem>();
        services.AddSingleton<HttpClient>();

        services.Scan(scan => scan
            .FromAssemblyOf<Plugin>()
            .AddClasses()
            .AsSelf()
            .WithSingletonLifetime()
            .AddClasses(classes => classes
                .AssignableTo<IMacroEngine>())
            .AsImplementedInterfaces()
            .WithSingletonLifetime()
            .AddClasses(classes => classes
                .AssignableTo<IMacroScheduler>())
            .AsImplementedInterfaces()
            .WithSingletonLifetime()
            .AddClasses(classes => classes
                .AssignableTo<IGitService>())
            .AsImplementedInterfaces()
            .WithSingletonLifetime());

        services.Scan(scan => scan
            .FromAssemblyOf<Plugin>()
            .AddClasses(classes => classes
                .AssignableTo<Window>())
            .AsSelf()
            .WithSingletonLifetime());

        services.AddTransient<MacroContext>();

        return services;
    }
}
