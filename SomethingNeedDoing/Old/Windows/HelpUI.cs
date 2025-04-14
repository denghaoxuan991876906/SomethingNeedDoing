//using Dalamud.Game.ClientState.Keys;
//using Dalamud.Game.ClientState.Objects.Enums;
//using Dalamud.Game.Text;
//using Dalamud.Interface;
//using Dalamud.Interface.Colors;
//using Dalamud.Interface.Utility.Raii;
//using Dalamud.Interface.Windowing;
//using ECommons.Automation.UIInput;
//using ECommons.UIHelpers.AddonMasterImplementations;
//using FFXIVClientStructs.FFXIV.Client.Game;
//using SomethingNeedDoing.Old.Macros.Commands;
//using SomethingNeedDoing.Old.Macros.Commands.Modifiers;
//using SomethingNeedDoing.Old.Macros.LuaFunctions;
//using SomethingNeedDoing.Old.Misc;
//using System.Reflection;

//namespace SomethingNeedDoing.Old.Windows;

//internal class HelpUI : Window
//{
//    public static new readonly string WindowName = "Something Need Doing Help";

//    private readonly (string Name, string Description, string? Example)[] cliData =
//    [
//        ("help", "Show this window.", null),
//        ("run", "Run a macro, the name must be unique.", $"{P.Aliases[0]} run MyMacro"),
//        ("run loop #", "Run a macro and then loop N times, the name must be unique. Only the last /loop in the macro is replaced", $"{P.Aliases[0]} run loop 5 MyMacro"),
//        ("pause", "Pause the currently executing macro.", null),
//        ("pause loop", "Pause the currently executing macro at the next /loop.", null),
//        ("resume", "Resume the currently paused macro.", null),
//        ("stop", "Clear the currently executing macro list.", null),
//        ("stop loop", "Clear the currently executing macro list at the next /loop.", null),
//    ];

//    private readonly List<string> clickNames;

//    private List<string> luaRequirePathsBuffer = [];

//    public HelpUI() : base(WindowName)
//    {
//        Flags |= ImGuiWindowFlags.NoScrollbar;

//        Size = new Vector2(400, 600);
//        SizeCondition = ImGuiCond.FirstUseEver;
//        RespectCloseHotkey = false;

//        clickNames = [.. ClickHelper.GetAvailableClicks()];

//        luaRequirePathsBuffer = new(C.LuaRequirePaths);
//    }

//    /// <inheritdoc/>
//    public override void Draw()
//    {
//        using var tabs = ImRaii.TabBar("GameDataTab");
//        if (tabs)
//        {
//            using (var tab = ImRaii.TabItem("Changelog"))
//                if (tab)
//                    Changelog.Draw();
//            using (var tab = ImRaii.TabItem("Options"))
//                if (tab)
//                    DrawOptions();
//            using (var tab = ImRaii.TabItem("Commands"))
//                if (tab)
//                    DrawCommands();
//            using (var tab = ImRaii.TabItem("Modifiers"))
//                if (tab)
//                    DrawModifiers();
//            using (var tab = ImRaii.TabItem("Lua"))
//                if (tab)
//                    DrawLua();
//            using (var tab = ImRaii.TabItem("CLI"))
//                if (tab)
//                    DrawCli();
//            using (var tab = ImRaii.TabItem("Clicks"))
//                if (tab)
//                    DrawClicks();
//            using (var tab = ImRaii.TabItem("Sends"))
//                if (tab)
//                    DrawVirtualKeys();
//            using (var tab = ImRaii.TabItem("Conditions"))
//                if (tab)
//                    DrawAllConditions();
//            using (var tab = ImRaii.TabItem("Game Data"))
//                if (tab)
//                    DrawGameData();
//            using (var tab = ImRaii.TabItem("Debug"))
//                if (tab)
//                    DrawDebug();
//        }
//    }

//    private void DrawOptions()
//    {
//        using var font = ImRaii.PushFont(UiBuilder.MonoFont);

//        static void DisplayOption(params string[] lines)
//        {
//            using var colour = ImRaii.PushColor(ImGuiCol.Text, ImGuiUtils.ShadedColor);

//            foreach (var line in lines)
//                ImGui.TextWrapped(line);
//        }

//        if (ImGui.CollapsingHeader("Crafting skips"))
//        {
//            var craftSkip = C.CraftSkip;
//            if (ImGui.Checkbox("Craft Skip", ref craftSkip))
//            {
//                C.CraftSkip = craftSkip;
//                C.Save();
//            }

//            DisplayOption("- Skip craft actions when not crafting.");

//            ImGui.Separator();

//            var smartWait = C.SmartWait;
//            if (ImGui.Checkbox("Smart Wait", ref smartWait))
//            {
//                C.SmartWait = smartWait;
//                C.Save();
//            }

//            DisplayOption("- Intelligently wait for crafting actions to complete instead of using the <wait> or <unsafe> modifiers.");

//            ImGui.Separator();

//            var qualitySkip = C.QualitySkip;
//            if (ImGui.Checkbox("Quality Skip", ref qualitySkip))
//            {
//                C.QualitySkip = qualitySkip;
//                C.Save();
//            }

//            DisplayOption("- Skip quality increasing actions when the HQ chance is at 100%%. If you depend on durability increases from Manipulation towards the end of your macro, you will likely want to disable this.");
//        }

//        if (ImGui.CollapsingHeader("Loop echo"))
//        {
//            var loopEcho = C.LoopEcho;
//            if (ImGui.Checkbox("Craft and Loop Echo", ref loopEcho))
//            {
//                C.LoopEcho = loopEcho;
//                C.Save();
//            }

//            DisplayOption("- /loop and /craft commands will always have an <echo> tag applied.");
//        }

//        if (ImGui.CollapsingHeader("Action retry"))
//        {
//            ImGui.SetNextItemWidth(50);
//            var maxTimeoutRetries = C.MaxTimeoutRetries;
//            if (ImGui.InputInt("Action max timeout retries", ref maxTimeoutRetries, 0))
//            {
//                if (maxTimeoutRetries < 0)
//                    maxTimeoutRetries = 0;
//                if (maxTimeoutRetries > 10)
//                    maxTimeoutRetries = 10;

//                C.MaxTimeoutRetries = maxTimeoutRetries;
//                C.Save();
//            }

//            DisplayOption("- The number of times to re-attempt an action command when a timely response is not received.");
//        }

//        if (ImGui.CollapsingHeader("Font"))
//        {
//            var disableMonospaced = C.DisableMonospaced;
//            if (ImGui.Checkbox("Disable Monospaced fonts", ref disableMonospaced))
//            {
//                C.DisableMonospaced = disableMonospaced;
//                C.Save();
//            }

//            DisplayOption("- Use the regular font instead of monospaced in the macro window. This may be handy for JP users so as to prevent missing unicode errors.");
//        }

//        if (ImGui.CollapsingHeader("Craft loop"))
//        {
//            var useCraftLoopTemplate = C.UseCraftLoopTemplate;
//            if (ImGui.Checkbox("Enable CraftLoop templating", ref useCraftLoopTemplate))
//            {
//                C.UseCraftLoopTemplate = useCraftLoopTemplate;
//                C.Save();
//            }

//            DisplayOption($"- When enabled the CraftLoop template will replace various placeholders with values.");

//            if (useCraftLoopTemplate)
//            {
//                var craftLoopTemplate = C.CraftLoopTemplate;

//                const string macroKeyword = "{{macro}}";
//                const string countKeyword = "{{count}}";

//                if (!craftLoopTemplate.Contains(macroKeyword))
//                    ImGui.TextColored(ImGuiColors.DPSRed, $"{macroKeyword} must be present in the template");

//                DisplayOption($"- {macroKeyword} inserts the current macro content.");
//                DisplayOption($"- {countKeyword} inserts the loop count for various commands.");

//                if (ImGui.InputTextMultiline("CraftLoopTemplate", ref craftLoopTemplate, 100_000, new Vector2(-1, 200)))
//                {
//                    C.CraftLoopTemplate = craftLoopTemplate;
//                    C.Save();
//                }
//            }
//            else
//            {
//                var craftLoopFromRecipeNote = C.CraftLoopFromRecipeNote;
//                if (ImGui.Checkbox("CraftLoop starts in the Crafting Log", ref craftLoopFromRecipeNote))
//                {
//                    C.CraftLoopFromRecipeNote = craftLoopFromRecipeNote;
//                    C.Save();
//                }

//                DisplayOption("- When enabled the CraftLoop option will expect the Crafting Log to be visible, otherwise the Synthesis window must be visible.");

//                var craftLoopEcho = C.CraftLoopEcho;
//                if (ImGui.Checkbox("CraftLoop Craft and Loop echo", ref craftLoopEcho))
//                {
//                    C.CraftLoopEcho = craftLoopEcho;
//                    C.Save();
//                }

//                DisplayOption("- When enabled the /craft or /gate commands supplied by the CraftLoop option will have an echo modifier.");

//                ImGui.SetNextItemWidth(50);
//                var craftLoopMaxWait = C.CraftLoopMaxWait;
//                if (ImGui.InputInt("CraftLoop maxwait", ref craftLoopMaxWait, 0))
//                {
//                    if (craftLoopMaxWait < 0)
//                        craftLoopMaxWait = 0;

//                    if (craftLoopMaxWait != C.CraftLoopMaxWait)
//                    {
//                        C.CraftLoopMaxWait = craftLoopMaxWait;
//                        C.Save();
//                    }
//                }

//                DisplayOption("- The CraftLoop /waitaddon \"...\" <maxwait> modifiers have their maximum wait set to this value.");
//            }
//        }

//        if (ImGui.CollapsingHeader("Chat"))
//        {
//            var names = Enum.GetNames<XivChatType>();
//            var chatTypes = Enum.GetValues<XivChatType>();

//            var current = Array.IndexOf(chatTypes, C.ChatType);
//            if (current == -1)
//            {
//                current = Array.IndexOf(chatTypes, C.ChatType = XivChatType.Echo);
//                C.Save();
//            }

//            ImGui.SetNextItemWidth(200f);
//            if (ImGui.Combo("Normal chat channel", ref current, names, names.Length))
//            {
//                C.ChatType = chatTypes[current];
//                C.Save();
//            }

//            var currentError = Array.IndexOf(chatTypes, C.ErrorChatType);
//            if (currentError == -1)
//            {
//                currentError = Array.IndexOf(chatTypes, C.ErrorChatType = XivChatType.Urgent);
//                C.Save();
//            }

//            ImGui.SetNextItemWidth(200f);
//            if (ImGui.Combo("Error chat channel", ref currentError, names, names.Length))
//            {
//                C.ChatType = chatTypes[currentError];
//                C.Save();
//            }
//        }

//        if (ImGui.CollapsingHeader("Error beeps"))
//        {
//            var noisyErrors = C.NoisyErrors;
//            if (ImGui.Checkbox("Noisy errors", ref noisyErrors))
//            {
//                C.NoisyErrors = noisyErrors;
//                C.Save();
//            }

//            DisplayOption("- When a check fails or error happens, some helpful beeps will play to get your attention.");

//            ImGui.SetNextItemWidth(50f);
//            var beepFrequency = C.BeepFrequency;
//            if (ImGui.InputInt("Beep frequency", ref beepFrequency, 0))
//            {
//                C.BeepFrequency = beepFrequency;
//                C.Save();
//            }

//            ImGui.SetNextItemWidth(50f);
//            var beepDuration = C.BeepDuration;
//            if (ImGui.InputInt("Beep duration", ref beepDuration, 0))
//            {
//                C.BeepDuration = beepDuration;
//                C.Save();
//            }

//            ImGui.SetNextItemWidth(50f);
//            var beepCount = C.BeepCount;
//            if (ImGui.InputInt("Beep count", ref beepCount, 0))
//            {
//                C.BeepCount = beepCount;
//                C.Save();
//            }

//            if (ImGui.Button("Beep test"))
//            {
//                System.Threading.Tasks.Task.Run(() =>
//                {
//                    for (var i = 0; i < beepCount; i++)
//                        Console.Beep(beepFrequency, beepDuration);
//                });
//            }
//        }

//        if (ImGui.CollapsingHeader("/action"))
//        {
//            var stopMacro = C.StopMacroIfActionTimeout;
//            if (ImGui.Checkbox("Stop macro if /action times out", ref stopMacro))
//            {
//                C.StopMacroIfActionTimeout = stopMacro;
//                C.Save();
//            }
//        }

//        if (ImGui.CollapsingHeader("/item"))
//        {
//            var stopMacroNotFound = C.StopMacroIfItemNotFound;
//            if (ImGui.Checkbox("Stop macro if the item to use is not found", ref stopMacroNotFound))
//            {
//                C.StopMacroIfItemNotFound = stopMacroNotFound;
//                C.Save();
//            }

//            var stopMacro = C.StopMacroIfCantUseItem;
//            if (ImGui.Checkbox("Stop macro if you cannot use an item", ref stopMacro))
//            {
//                C.StopMacroIfCantUseItem = stopMacro;
//                C.Save();
//            }
//        }

//        if (ImGui.CollapsingHeader("/target"))
//        {
//            var defaultTarget = C.UseSNDTargeting;
//            if (ImGui.Checkbox("Use SND's targeting system.", ref defaultTarget))
//            {
//                C.UseSNDTargeting = defaultTarget;
//                C.Save();
//            }

//            DisplayOption("- Override the behaviour of /target with SND's system.");

//            var stopMacro = C.StopMacroIfTargetNotFound;
//            if (ImGui.Checkbox("Stop macro if target not found (only applies to SND's targeting system).", ref stopMacro))
//            {
//                C.StopMacroIfTargetNotFound = stopMacro;
//                C.Save();
//            }
//        }

//        if (ImGui.CollapsingHeader("/waitaddon"))
//        {
//            var stopMacro = C.StopMacroIfAddonNotFound;
//            if (ImGui.Checkbox("Stop macro if the requested addon is not found", ref stopMacro))
//            {
//                C.StopMacroIfAddonNotFound = stopMacro;
//                C.Save();
//            }

//            var stopMacroVisible = C.StopMacroIfAddonNotVisible;
//            if (ImGui.Checkbox("Stop macro if the requested addon is not visible", ref stopMacroVisible))
//            {
//                C.StopMacroIfAddonNotVisible = stopMacroVisible;
//                C.Save();
//            }
//        }

//        if (ImGui.CollapsingHeader("AutoRetainer"))
//        {
//            var selected = string.Empty;
//            ImGui.TextUnformatted("Script to run on AutoRetainer CharacterPostProcess");
//            ImGui.SetNextItemWidth(300);
//            using (var combo = ImRaii.Combo("##CharacterPostProcessMacroSelection", C.ARCharacterPostProcessMacro?.Name ?? string.Empty))
//            {
//                if (combo)
//                {
//                    if (ImGui.Selectable("##EmptySelection"))
//                    {
//                        selected = string.Empty;
//                        C.ARCharacterPostProcessMacro = null;
//                        C.Save();
//                    }
//                    foreach (var node in C.GetAllNodes().OfType<MacroNode>())
//                    {
//                        if (ImGui.Selectable(node.Name))
//                        {
//                            selected = node.Name;
//                            C.ARCharacterPostProcessMacro = C.GetAllNodes().OfType<MacroNode>().First(m => m.Name == selected);
//                            C.Save();
//                        }
//                    }
//                }
//            }

//            if (C.ARCharacterPostProcessExcludedCharacters.Any(x => x == Svc.ClientState.LocalContentId))
//            {
//                if (ImGui.Button("Remove current character from exclusion list"))
//                {
//                    C.ARCharacterPostProcessExcludedCharacters.RemoveAll(x => x == Svc.ClientState.LocalContentId);
//                    C.Save();
//                }
//            }
//            else
//            {
//                if (ImGui.Button("Exclude current character"))
//                {
//                    C.ARCharacterPostProcessExcludedCharacters.Add(Svc.ClientState.LocalContentId);
//                    C.Save();
//                }
//            }
//        }

//        if (ImGui.CollapsingHeader("Lua"))
//        {
//            ImGui.TextUnformatted("Lua Required Paths:");

//            // We need to pass Imgui a reference and we can't do that with lists, so temporarily demote our list to an array
//            string[] paths = [.. luaRequirePathsBuffer];
//            for (var index = 0; index < luaRequirePathsBuffer.Count; index++)
//            {
//                if (ImGuiX.IconButton(FontAwesomeIcon.Trash, "Delete Path " + index))
//                {
//                    luaRequirePathsBuffer.RemoveAt(index);

//                    C.LuaRequirePaths = [.. luaRequirePathsBuffer];
//                    C.Save();
//                }

//                ImGui.SameLine();
//                if (ImGui.InputText("Path " + index, ref paths[index], 100_000))
//                {
//                    luaRequirePathsBuffer = new(paths);
//                    // Remove blank lines from the list
//                    luaRequirePathsBuffer = luaRequirePathsBuffer.Where(str => !string.IsNullOrEmpty(str)).ToList();

//                    C.LuaRequirePaths = [.. luaRequirePathsBuffer];
//                    C.Save();
//                }
//            }

//            if (ImGuiX.IconButton(FontAwesomeIcon.Plus, "Add Path"))
//            {
//                luaRequirePathsBuffer.Add(string.Empty);
//            }
//        }
//    }

//    private void DrawCommands()
//    {
//        using var font = ImRaii.PushFont(UiBuilder.MonoFont);
//        var macroCommandTypes = typeof(MacroCommand).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(MacroCommand)) && t != typeof(NativeCommand));
//        foreach (var type in macroCommandTypes)
//        {
//            var commandsProp = type.GetProperty("Commands", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
//            var descriptionProp = type.GetProperty("Description", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
//            var examplesProp = type.GetProperty("Examples", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
//            //var mods = type.GetConstructors()
//            //    .SelectMany(c => c.GetParameters())
//            //    .Where(p => typeof(MacroModifier).IsAssignableFrom(p.ParameterType))
//            //    .Select(p => p.Name)
//            //    .ToArray();

//            ImGui.TextUnformatted($"/{type.Name.ToLower().Replace("command", "")}");

//            using var colour = ImRaii.PushColor(ImGuiCol.Text, ImGuiUtils.ShadedColor);

//            ImGui.TextUnformatted($"- Commands: {string.Join(", ", commandsProp != null ? (string[])commandsProp.GetValue(null)! : [])}");
//            ImGui.TextUnformatted($"- Description: {(descriptionProp != null ? (string)descriptionProp.GetValue(null)! : string.Empty)}");
//            //if (mods.Length != 0)
//            //    ImGui.TextUnformatted($"- Mods: {string.Join(", ", mods)}");

//            ImGui.TextUnformatted($"- Examples:");
//            foreach (var example in (string[])examplesProp?.GetValue(null)! ?? [])
//            {
//                ImGui.TextUnformatted("  - ");
//                ImGui.SameLine();
//                ImGuiUtils.ClickToCopyText(example);
//            }
//            ImGui.Separator();
//        }
//    }

//    private void DrawModifiers()
//    {
//        using var font = ImRaii.PushFont(UiBuilder.MonoFont);
//        var modifierTypes = typeof(MacroModifier).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(MacroModifier)));
//        foreach (var type in modifierTypes)
//        {
//            var modifierProp = type.GetProperty("Modifier", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
//            var descriptionProp = type.GetProperty("Description", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
//            var examplesProp = type.GetProperty("Examples", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

//            ImGui.TextUnformatted($"{(modifierProp != null ? (string)modifierProp.GetValue(null)! : string.Empty)}");

//            using var colour = ImRaii.PushColor(ImGuiCol.Text, ImGuiUtils.ShadedColor);

//            ImGui.TextUnformatted($"- Description: {(descriptionProp != null ? (string)descriptionProp.GetValue(null)! : string.Empty)}");
//            ImGui.TextUnformatted($"- Examples:");
//            foreach (var example in (string[])examplesProp?.GetValue(null)! ?? [])
//            {
//                ImGui.TextUnformatted("  - ");
//                ImGui.SameLine();
//                ImGuiUtils.ClickToCopyText(example);
//            }
//            ImGui.Separator();
//        }
//    }

//    private void DrawCli()
//    {
//        using var font = ImRaii.PushFont(UiBuilder.MonoFont);

//        foreach (var (name, desc, example) in cliData)
//        {
//            ImGui.TextUnformatted($"{P.Aliases[0]} {name}");
//            using var colour = ImRaii.PushColor(ImGuiCol.Text, ImGuiUtils.ShadedColor);
//            ImGui.TextWrapped($"- Description: {desc}");
//            if (example != null)
//                ImGui.TextUnformatted($"- Example: {example}");
//            ImGui.Separator();
//        }
//    }

//    private void DrawLua()
//    {
//        using var font = ImRaii.PushFont(UiBuilder.MonoFont);

//        var text = @$"
//Lua scripts work by yielding commands back to the macro engine.

//For example:

//yield(""/ac Muscle memory <wait.3>"")
//yield(""/ac Precise touch <wait.2>"")
//yield(""/echo done!"")
//...and so on.

//Every script has access to these global variables:
//{string.Join(", ", typeof(Svc).GetProperties().Select(p => p.Name))}

//They are Dalamud services, whose code is available here
//https://github.com/goatcorp/Dalamud/tree/master/Dalamud/Plugin/Services".Trim();

//        //ActionManager, AgentMap, EnvManager, EventFramework, FateManager, Framework, InventoryManager, LimitBreakController, PlayerState, QuestManager, RouletteController, UIState

//        ImGui.TextWrapped(text);
//        ImGui.Separator();

//        var commands = new List<(string, dynamic)>
//        {
//            (nameof(Actions), Actions.Instance),
//            (nameof(Addons), Addons.Instance),
//            (nameof(CharacterState), CharacterState.Instance),
//            (nameof(CraftingState), CraftingState.Instance),
//            (nameof(EntityState), EntityState.Instance),
//            (nameof(Internal), Internal.Instance),
//            (nameof(Inventory), Inventory.Instance),
//            (nameof(Ipc), Ipc.Instance),
//            (nameof(Quests), Quests.Instance),
//            (nameof(UserEnv), UserEnv.Instance),
//            (nameof(WorldState), WorldState.Instance),
//        };

//        foreach (var (commandName, commandInstance) in commands)
//        {
//            ImGui.TextUnformatted($"{commandName}");
//            using var colour = ImRaii.PushColor(ImGuiCol.Text, ImGuiUtils.ShadedColor);
//            ECommons.ImGuiMethods.ImGuiEx.TextWrapped(string.Join("\n", commandInstance.ListAllFunctions()));
//            ImGui.Separator();
//        }
//    }

//    private unsafe void DrawClicks()
//    {
//        using var font = ImRaii.PushFont(UiBuilder.MonoFont);

//        ImGui.TextWrapped("Refer to https://github.com/NightmareXIV/ECommons/tree/master/ECommons/UIHelpers/AddonMasterImplementations for any details.\nClicks in red are not callable as-is stated. They are click properties that themselves have methods.");
//        ImGui.Separator();

//        //var clicks = typeof(AddonMaster).Assembly.GetTypes()
//        //    .Where(type => type.FullName!.StartsWith($"{typeof(AddonMaster).FullName}+") && type.DeclaringType == typeof(AddonMaster))
//        //    .SelectMany(type => type.GetMethods()
//        //        .Where(m => m.DeclaringType != typeof(object) && !m.IsSpecialName)
//        //        .Select(method => $"{type.Name} {method.Name}"));
//        var clicks = typeof(AddonMaster).Assembly.GetTypes()
//            .Where(type => type.FullName!.StartsWith($"{typeof(AddonMaster).FullName}+") && type.DeclaringType == typeof(AddonMaster))
//            .SelectMany(type => type.GetMembers()
//                .Where(m => m is MethodInfo info && !info.IsSpecialName && info.DeclaringType != typeof(object) || m is PropertyInfo prop && prop.GetAccessors().Length > 0 && prop.PropertyType.IsClass && prop.PropertyType.Namespace == type.Namespace)
//                .Select(member => $"{(member is MethodInfo ? "m" : "p")}{type.Name} {member.Name}"));

//        foreach (var name in clicks)
//        {
//            var colour = name.StartsWith('p') ? ImGuiColors.DalamudRed : *ImGui.GetStyleColorVec4(ImGuiCol.Text);
//            ImGuiUtils.ClickToCopyText($"/click {name[1..]}", colour);
//        }
//    }

//    private void DrawVirtualKeys()
//    {
//        using var font = ImRaii.PushFont(UiBuilder.MonoFont);

//        ImGui.TextWrapped("Active keys will highlight green.");
//        ImGui.Separator();

//        var validKeys = Svc.KeyState.GetValidVirtualKeys().ToHashSet();

//        var names = Enum.GetNames<VirtualKey>();
//        var values = Enum.GetValues<VirtualKey>();

//        for (var i = 0; i < names.Length; i++)
//        {
//            var name = names[i];
//            var vkCode = values[i];

//            if (!validKeys.Contains(vkCode))
//                continue;

//            var isActive = Svc.KeyState[vkCode];

//            using var colour = ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.HealerGreen, isActive);
//            ImGui.TextUnformatted($"/send {name}");
//        }
//    }

//    private void DrawAllConditions()
//    {
//        using var font = ImRaii.PushFont(UiBuilder.MonoFont);

//        ImGui.TextWrapped("Active conditions will highlight green.");
//        ImGui.Separator();
//        foreach (var (flag, isActive) in from ConditionFlag flag in Enum.GetValues(typeof(ConditionFlag))
//                                         let isActive = Svc.Condition[flag]
//                                         select (flag, isActive))
//        {
//            using var colour = ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.HealerGreen, isActive);
//            ImGui.TextUnformatted($"ID: {(int)flag} Enum: {flag}");
//        }
//    }

//    private void DrawGameData()
//    {
//        using var tabs = ImRaii.TabBar("GameDataTab");
//        if (tabs)
//        {
//            using (var tab = ImRaii.TabItem("ObjectKinds"))
//                if (tab)
//                    DrawEnum<ObjectKind>();
//            using (var tab = ImRaii.TabItem("InventoryTypes"))
//                if (tab)
//                    DrawEnum<InventoryType>();
//        }
//    }

//    private void DrawDebug()
//    {
//        var bronzes = WorldState.Instance.GetBronzeChestLocations();
//        foreach (var l in bronzes)
//            ImGui.TextUnformatted($"bronze @ {new Vector3(l.Item1, l.Item2, l.Item3)}");
//        var silvers = WorldState.Instance.GetSilverChestLocations();
//        foreach (var l in silvers)
//            ImGui.TextUnformatted($"silver @ {new Vector3(l.Item1, l.Item2, l.Item3)}");
//        var golds = WorldState.Instance.GetGoldChestLocations();
//        foreach (var l in golds)
//            ImGui.TextUnformatted($"gold @ {new Vector3(l.Item1, l.Item2, l.Item3)}");
//    }

//    private void DrawEnum<T>()
//    {
//        using var font = ImRaii.PushFont(UiBuilder.MonoFont);
//        using var colour = ImRaii.PushColor(ImGuiCol.Text, ImGuiUtils.ShadedColor);
//        foreach (var value in Enum.GetValues(typeof(T)))
//            ImGui.TextUnformatted($"{Enum.GetName(typeof(T), value)}: {Convert.ChangeType(value, Enum.GetUnderlyingType(typeof(T)))}");
//    }
//}
