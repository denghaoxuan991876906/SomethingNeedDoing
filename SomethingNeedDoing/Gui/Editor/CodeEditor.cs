using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using DalamudCodeEditor;
using SomethingNeedDoing.Core.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using TextEditor = DalamudCodeEditor.TextEditor.Editor;

namespace SomethingNeedDoing.Gui.Editor;

/// <summary>
/// DalamudCodeEditor TextEditor wrapper.
/// </summary>
public class CodeEditor : IDisposable
{
    private readonly LuaLanguageDefinition _lua;
    private readonly TextEditor _editor = new();
    private readonly MetadataParser _metadataParser;
    private readonly Dictionary<MacroType, LanguageDefinition> _languages;

    private IMacro? macro = null;
    private CancellationTokenSource? _debounceCts;
    private bool _lastIsDirty = false;

    public CodeEditor(LuaLanguageDefinition lua, MetadataParser metadataParser)
    {
        _lua = lua;
        _metadataParser = metadataParser;
        _languages = new() { { MacroType.Lua, _lua }, { MacroType.Native, new NativeMacroLanguageDefinition() } };
        Config.ConfigFileChanged += RefreshContent;
    }

    public int Lines => _editor.Buffer.LineCount;
    public bool ReadOnly
    {
        get => _editor.IsReadOnly;
        set => _editor.SetReadOnly(value);
    }

    public bool IsHighlightingSyntax
    {
        get => _editor.Colorizer.Enabled;
        set => _editor.Colorizer.SetEnabled(value);
    }

    public bool IsShowingWhitespace
    {
        get => _editor.Style.ShowWhitespace;
        set => _editor.Style.SetShowWhitespace(value);
    }

    public bool IsShowingLineNumbers
    {
        get => _editor.Style.ShowLineNumbers;
        set => _editor.Style.SetShowLineNumbers(value);
    }

    public int Column => _editor.Cursor.GetPosition().Column;

    public void SetMacro(IMacro macro)
    {
        if (this.macro?.Id == macro.Id)
            return;

        this.macro = macro;
        _editor.Buffer.SetText(macro.Content);
        _editor.UndoManager.Clear();

        if (_languages.TryGetValue(macro.Type, out var language))
            _editor.Language = language;
    }

    public void RefreshContent()
    {
        if (macro != null)
            _editor.Buffer.SetText(macro.Content);
    }

    public string GetContent() => _editor.Buffer.GetText();

    public bool Draw()
    {
        if (macro == null)
            return false;

        using var font = ImRaii.PushFont(UiBuilder.MonoFont);
        _editor.Draw(macro.Name);

        var isDirty = _editor.Buffer.IsDirty;
        if (isDirty && !_lastIsDirty && macro is ConfigMacro configMacro)
        {
            _debounceCts?.Cancel();
            _debounceCts = new CancellationTokenSource();

            _ = Task.Delay(500, _debounceCts.Token).ContinueWith(task =>
            {
                if (task.IsCompletedSuccessfully && !_debounceCts.Token.IsCancellationRequested)
                {
                    try
                    {
                        if (macro?.Id == configMacro.Id)
                        {
                            var content = GetContent();
                            var newMetadata = _metadataParser.ParseMetadata(content);
                            if (!MetadataEquals(configMacro.Metadata, newMetadata))
                            {
                                configMacro.Metadata = newMetadata;
                                C.Save();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        FrameworkLogger.Error(ex, "Failed to auto-parse metadata");
                    }
                }

                return Task.CompletedTask;
            }, _debounceCts.Token);
        }

        _lastIsDirty = isDirty;
        return isDirty;
    }

    public void Dispose()
    {
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();
        Config.ConfigFileChanged -= RefreshContent;
    }

    private static bool MetadataEquals(MacroMetadata m1, MacroMetadata m2)
    {
        if (m1 == null && m2 == null) return true;
        if (m1 == null || m2 == null) return false;

        return m1.Author == m2.Author &&
               m1.Version == m2.Version &&
               m1.Description == m2.Description &&
               m1.CraftingLoop == m2.CraftingLoop &&
               m1.CraftLoopCount == m2.CraftLoopCount &&
               m1.TriggerEvents.SequenceEqual(m2.TriggerEvents) &&
               m1.PluginDependecies.SequenceEqual(m2.PluginDependecies) &&
               m1.PluginsToDisable.SequenceEqual(m2.PluginsToDisable) &&
               ConfigsEqual(m1.Configs, m2.Configs);
    }

    private static bool ConfigsEqual(Dictionary<string, MacroConfigItem> c1, Dictionary<string, MacroConfigItem> c2)
    {
        if (c1.Count != c2.Count) return false;

        foreach (var kvp in c1)
        {
            if (!c2.TryGetValue(kvp.Key, out var v2)) return false;

            var v1 = kvp.Value;
            if (v1.DefaultValue?.ToString() != v2.DefaultValue?.ToString() ||
                v1.Description != v2.Description ||
                v1.Type != v2.Type ||
                v1.MinValue?.ToString() != v2.MinValue?.ToString() ||
                v1.MaxValue?.ToString() != v2.MaxValue?.ToString() ||
                v1.Required != v2.Required ||
                v1.ValidationPattern != v2.ValidationPattern ||
                v1.ValidationMessage != v2.ValidationMessage)
            {
                return false;
            }
        }

        return true;
    }
}
