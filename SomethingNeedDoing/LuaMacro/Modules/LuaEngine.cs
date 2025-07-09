using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.LuaMacro.Modules;

/// <summary>
/// Engine for executing Lua code.
/// </summary>
public class LuaEngine(NLuaMacroEngine luaEngine) : IEngine
{
    public string Name => "NLua";

    public async Task ExecuteAsync(string content, CancellationToken cancellationToken = default)
    {
        try
        {
            var tempMacro = new TemporaryMacro(content);
            await luaEngine.StartMacro(tempMacro, cancellationToken);
        }
        catch (Exception ex)
        {
            Svc.Log.Error($"Error executing Lua code '{content}': {ex}");
            throw;
        }
    }

    public bool CanExecute(string content) => !string.IsNullOrWhiteSpace(content) && !content.StartsWith('/');
}
