using System.Threading.Tasks;
using SomethingNeedDoing.Macros.Native.Modifiers;

namespace SomethingNeedDoing.Macros.Native.Commands;
/// <summary>
/// Base class for require commands that check conditions.
/// </summary>
public abstract class RequireCommandBase(string text, WaitModifier? waitMod = null) : MacroCommandBase(text, waitMod?.WaitDuration ?? 0)
{
    protected const int DefaultCheckInterval = 250;
    protected const int DefaultTimeout = 5000;

    /// <inheritdoc/>
    public override bool RequiresFrameworkThread => true;

    protected abstract Task<bool> CheckCondition(MacroContext context);
    protected abstract string GetErrorMessage();
}
