using System.Threading.Tasks;
using SomethingNeedDoing.MacroFeatures.Native.Modifiers;

namespace SomethingNeedDoing.MacroFeatures.Native.Commands;
/// <summary>
/// Base class for require commands that check conditions.
/// </summary>
public abstract class RequireCommandBase(string text, WaitModifier? waitMod = null) : MacroCommandBase(text, waitMod)
{
    protected const int DefaultCheckInterval = 250;
    protected const int DefaultTimeout = 5000;

    /// <inheritdoc/>
    public override bool RequiresFrameworkThread => true;

    protected abstract Task<bool> CheckCondition(MacroContext context);
    protected abstract string GetErrorMessage();
}
