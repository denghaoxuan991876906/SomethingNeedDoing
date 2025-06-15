using Dalamud.Interface.Windowing;
using SomethingNeedDoing.Gui;

namespace SomethingNeedDoing.Services;
public class WindowService : IDisposable
{
    private readonly WindowSystem _ws;
    private readonly MainWindow _mainWindow;
    private readonly StatusWindow _runningMacrosWindow;

    public WindowService(WindowSystem ws, MainWindow mainWindow, StatusWindow runningMacrosWindow)
    {
        _ws = ws;
        _mainWindow = mainWindow;
        _runningMacrosWindow = runningMacrosWindow;
        _ws.AddWindow(_mainWindow);
        _ws.AddWindow(_runningMacrosWindow);
        Svc.PluginInterface.UiBuilder.Draw += _ws.Draw;
        Svc.PluginInterface.UiBuilder.OpenMainUi += _mainWindow.Toggle;
        Svc.PluginInterface.UiBuilder.OpenConfigUi += _mainWindow.Toggle;
    }

    public void Dispose()
    {
        Svc.PluginInterface.UiBuilder.Draw -= _ws.Draw;
        Svc.PluginInterface.UiBuilder.OpenMainUi -= _mainWindow.Toggle;
        Svc.PluginInterface.UiBuilder.OpenConfigUi -= _mainWindow.Toggle;
        _ws.RemoveAllWindows();
    }
}
