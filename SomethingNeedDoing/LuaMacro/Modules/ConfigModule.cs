using NLua;
using SomethingNeedDoing.Core.Interfaces;

namespace SomethingNeedDoing.LuaMacro.Modules;

/// <summary>
/// Lua module for macro configuration management.
/// </summary>
public class ConfigModule(IMacro macro) : LuaModuleBase
{
    public override string ModuleName => "Config";

    [LuaFunction]
    public object Get(string name, object? defaultValue = null)
    {
        if (string.IsNullOrEmpty(name))
            return defaultValue ?? string.Empty;

        if (macro.Metadata.Configs.TryGetValue(name, out var configItem))
            return configItem.Value;

        return defaultValue ?? string.Empty;
    }

    [LuaFunction]
    public void Set(string name, object value)
    {
        if (string.IsNullOrEmpty(name))
            return;

        if (macro.Metadata.Configs.TryGetValue(name, out var configItem))
        {
            var validatedValue = ValidateValue(configItem, value);
            configItem.Value = validatedValue;

            if (macro is ConfigMacro)
                C.Save();
        }
    }

    [LuaFunction]
    public object GetDefault(string name)
    {
        if (string.IsNullOrEmpty(name))
            return string.Empty;

        if (macro.Metadata.Configs.TryGetValue(name, out var configItem))
            return configItem.DefaultValue;

        return string.Empty;
    }

    [LuaFunction]
    public void Reset(string name)
    {
        if (string.IsNullOrEmpty(name))
            return;

        if (macro.Metadata.Configs.TryGetValue(name, out var configItem))
        {
            configItem.Value = configItem.DefaultValue;
            if (macro is ConfigMacro)
                C.Save();
        }
    }

    [LuaFunction]
    public string GetDescription(string name)
    {
        if (string.IsNullOrEmpty(name))
            return string.Empty;

        if (macro.Metadata.Configs.TryGetValue(name, out var configItem))
            return configItem.Description;

        return string.Empty;
    }

    [LuaFunction]
    public string GetType(string name)
    {
        if (string.IsNullOrEmpty(name))
            return "string";

        if (macro.Metadata.Configs.TryGetValue(name, out var configItem))
            return configItem.Type;

        return "string";
    }

    [LuaFunction]
    public bool Exists(string name) => !string.IsNullOrEmpty(name) && macro.Metadata.Configs.ContainsKey(name);

    [LuaFunction]
    public string[] GetAllNames() => [.. macro.Metadata.Configs.Keys];

    private object ValidateValue(MacroConfigItem configItem, object value)
    {
        try
        {
            switch (configItem.Type.ToLower())
            {
                case "number":
                case "int":
                    if (int.TryParse(value.ToString(), out var intValue))
                    {
                        if (configItem.MinValue != null && int.TryParse(configItem.MinValue.ToString(), out var min) && intValue < min)
                            return min;
                        if (configItem.MaxValue != null && int.TryParse(configItem.MaxValue.ToString(), out var max) && intValue > max)
                            return max;
                        return intValue;
                    }
                    return configItem.DefaultValue;

                case "float":
                case "double":
                    if (double.TryParse(value.ToString(), out var doubleValue))
                    {
                        if (configItem.MinValue != null && double.TryParse(configItem.MinValue.ToString(), out var min) && doubleValue < min)
                            return min;
                        if (configItem.MaxValue != null && double.TryParse(configItem.MaxValue.ToString(), out var max) && doubleValue > max)
                            return max;
                        return doubleValue;
                    }
                    return configItem.DefaultValue;

                case "bool":
                case "boolean":
                    if (bool.TryParse(value.ToString(), out var boolValue))
                        return boolValue;
                    return configItem.DefaultValue;

                case "string":
                default:
                    if (value.ToString() is { } str && !string.IsNullOrEmpty(configItem.ValidationPattern))
                    {
                        try
                        {
                            var regex = new System.Text.RegularExpressions.Regex(configItem.ValidationPattern);
                            if (!regex.IsMatch(str))
                            {
                                Svc.Log.Warning($"Config validation failed for '{configItem.Description}': {configItem.ValidationMessage ?? "Value does not match pattern"}");
                                return configItem.DefaultValue;
                            }
                        }
                        catch (Exception ex)
                        {
                            Svc.Log.Error(ex, $"Invalid validation pattern for config: {configItem.ValidationPattern}");
                        }
                    }

                    return value.ToString() ?? string.Empty;
            }
        }
        catch (Exception ex)
        {
            Svc.Log.Error(ex, $"Failed to validate config value for {configItem.Description}");
            return configItem.DefaultValue;
        }
    }
}
