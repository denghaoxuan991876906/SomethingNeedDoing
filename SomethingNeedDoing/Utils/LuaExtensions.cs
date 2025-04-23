using NLua;

namespace SomethingNeedDoing.Utils;
public static class LuaExtensions
{
    public static object[] LoadEntryPointWrappedScript(this Lua lua, string script) => lua.DoString(string.Format(LuaCodeSnippets.EntrypointTemplate, script));
    public static void LoadFStrings(this Lua lua) => lua.DoString(LuaCodeSnippets.FStringSnippet);

    /// <summary>
    /// Loads the enhanced error handler for better Lua error reporting.
    /// </summary>
    /// <param name="lua">The Lua state.</param>
    public static void LoadErrorHandler(this Lua lua) => lua.DoString(LuaCodeSnippets.ErrorHandlerSnippet);

    /// <summary>
    /// Gets detailed error information from a Lua error.
    /// </summary>
    /// <param name="lua">The Lua state.</param>
    /// <returns>A string containing detailed error information.</returns>
    public static string GetLuaErrorDetails(this Lua lua)
    {
        try
        {
            // Check if there's an error on the stack
            if (lua.State.Type(-1) == KeraLua.LuaType.String)
            {
                var errorMessage = lua.State.ToString(-1);
                lua.State.Pop(1);
                return errorMessage;
            }

            // Try to get debug information
            lua.DoString(@"
                local error_info = debug.getinfo(2, ""Sln"")
                if error_info then
                    local source = error_info.source
                    if source:sub(1,1) == ""@"" then
                        source = source:sub(2)
                    end
                    return string.format(""Error at %s:%d in function '%s'"",
                        source, error_info.currentline, error_info.name or ""(anonymous)"")
                end
                return ""Unknown error location""
            ");

            var result = lua.State.ToString(-1);
            lua.State.Pop(1);
            return result;
        }
        catch
        {
            return "Failed to get Lua error details";
        }
    }

    public static void SetTriggerEventData(this Lua lua, TriggerEventArgs? args)
    {
        if (args is null) return;
        lua.NewTable("TriggerData");
        var table = lua.GetTable("TriggerData");

        // Set the event type
        table["eventType"] = args.EventType;

        // Set the timestamp
        table["timestamp"] = args.Timestamp;

        // Handle the data based on its type
        if (args.Data is Dictionary<string, object> data)
        {
            // If it's a dictionary, add each key-value pair to the table
            foreach (var kvp in data)
                table[kvp.Key] = kvp.Value;
        }
        else if (args.Data != null)
        {
            // If it's not a dictionary but not null, add it as a single value
            table["data"] = args.Data;
        }
    }
}
