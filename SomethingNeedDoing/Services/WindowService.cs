using Dalamud.Interface.Windowing;
using SomethingNeedDoing.Gui;

namespace SomethingNeedDoing.Services;
public class WindowService : IDisposable
{
    private readonly WindowSystem _ws;
    private readonly MainWindow _mainWindow;

    public WindowService(WindowSystem ws, MainWindow mainWindow)
    {
        _ws = ws;
        _mainWindow = mainWindow;
        _ws.AddWindow(_mainWindow);
        Svc.PluginInterface.UiBuilder.Draw += _ws.Draw;
    }

    public void Dispose()
    {
        Svc.PluginInterface.UiBuilder.Draw -= _ws.Draw;
        _ws.RemoveAllWindows();
    }
}
