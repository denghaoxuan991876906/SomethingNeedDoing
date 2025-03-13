using ECommons.EzIpcManager;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SomethingNeedDoing.IPC;
#nullable disable
public class Questionable
{
    public const string Name = "Questionable";
    public Questionable() => EzIPC.Init(this, Name, SafeWrapper.AnyException);

    [EzIPC] public Func<bool> IsRunning;
    [EzIPC] public Func<string> GetCurrentQuestId;
    [EzIPC] public Func<StepData> GetCurrentStepData;
    [EzIPC] public Func<string, bool> IsQuestLocked;
    [EzIPC] public Func<string, bool> ImportQuestPriority;
    [EzIPC] public Func<bool> ClearQuestPriority;
    [EzIPC] public Func<string, bool> AddQuestPriority;
    [EzIPC] public Func<int, string, bool> InsertQuestPriority;
    [EzIPC] public Func<string> ExportQuestPriority;

    public sealed class StepData
    {
        public required string QuestId { get; init; }
        public required byte Sequence { get; init; }
        public required int Step { get; init; }
        public required string InteractionType { get; init; }
        public required Vector3? Position { get; init; }
        public required ushort TerritoryId { get; init; }
    }
}
