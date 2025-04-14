using SomethingNeedDoing.Old.Macros.Commands.Modifiers;
using SomethingNeedDoing.Old.Misc;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Old.Macros.Commands;

/// <summary>
/// The base command other commands inherit from.
/// </summary>
internal abstract class MacroCommand
{
    private static readonly Random Rand = new();

    protected MacroCommand(string text) => Text = text;
    protected MacroCommand(string text, WaitModifier waitMod) : this(text, waitMod.Wait, waitMod.Until) { }

    protected MacroCommand(string text, WaitModifier waitMod, IndexModifier indexMod) : this(text, waitMod.Wait, waitMod.Until, indexMod.ObjectId) { }

    protected MacroCommand(string text, int wait, int until)
    {
        Text = text;
        Wait = wait;
        WaitUntil = until;
    }

    protected MacroCommand(string text, int wait, int until, int index)
    {
        Text = text;
        Wait = wait;
        WaitUntil = until;
        ObjectIndex = index;
    }

    /// <summary>
    /// Gets the original line text.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Gets the milliseconds to wait.
    /// </summary>
    public int Wait { get; }

    /// <summary>
    /// Gets the milliseconds to wait until.
    /// </summary>
    public int WaitUntil { get; }

    /// <summary>
    /// Gets the object index.
    /// </summary>
    public int ObjectIndex { get; }

    /// <inheritdoc/>
    public override string ToString() => Text;

    /// <summary>
    /// Execute a macro command.
    /// </summary>
    /// <param name="macro">The macro being run.</param>
    /// <param name="token">Async cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public abstract Task Execute(ActiveMacro macro, CancellationToken token);

    /// <summary>
    /// Extract a match group and unquote if necessary.
    /// </summary>
    /// <param name="match">Match group.</param>
    /// <param name="groupName">Group name.</param>
    /// <returns>Extracted and unquoted group value.</returns>
    protected static string ExtractAndUnquote(Match match, string groupName)
    {
        var group = match.Groups[groupName];
        var groupValue = group.Value;

        if (groupValue.StartsWith('"') && groupValue.EndsWith('"'))
            groupValue = groupValue.Trim('"');

        return groupValue;
    }

    /// <summary>
    /// Perform a wait.
    /// </summary>
    /// <param name="token">Cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    protected async Task PerformWait(CancellationToken token)
    {
        if (Wait == 0 && WaitUntil == 0)
            return;

        TimeSpan sleep;
        if (WaitUntil == 0)
        {
            sleep = TimeSpan.FromMilliseconds(Wait);
            Svc.Log.Debug($"Sleeping for {sleep.TotalMilliseconds} millis");
        }
        else
        {
            var value = Rand.Next(Wait, WaitUntil);
            sleep = TimeSpan.FromMilliseconds(value);
            Svc.Log.Debug($"Sleeping for {sleep.TotalMilliseconds} millis ({Wait} to {WaitUntil})");
        }

        await Task.Delay(sleep, token);
    }

    /// <summary>
    /// Perform an action every <paramref name="interval"/> seconds until either the action succeeds or <paramref name="until"/> seconds elapse.
    /// </summary>
    /// <param name="interval">Action execution interval.</param>
    /// <param name="until">Maximum time to wait.</param>
    /// <param name="action">Action to execute.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>A value indicating whether the action succeeded.</returns>
    protected async Task<bool> LinearWait(int interval, int until, Func<bool> action, CancellationToken token)
    {
        var totalWait = 0;
        while (true)
        {
            var success = action();
            if (success)
                return true;

            totalWait += interval;
            if (totalWait > until)
                return false;

            await Task.Delay(interval, token);
        }
    }

    /// <summary>
    /// Perform an action every <paramref name="interval"/> seconds until either the action succeeds or <paramref name="until"/> seconds elapse.
    /// </summary>
    /// <param name="interval">Action execution interval.</param>
    /// <param name="until">Maximum time to wait.</param>
    /// <param name="action">Action to execute.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>A value indicating whether the action succeeded.</returns>
    /// <typeparam name="T">Result type.</typeparam>
    protected async Task<(T? Result, bool Success)> LinearWait<T>(int interval, int until, Func<(T? Result, bool Success)> action, CancellationToken token)
    {
        var totalWait = 0;
        while (true)
        {
            var (result, success) = action();
            if (success)
                return (result, true);

            totalWait += interval;
            if (totalWait > until)
                return (result, false);

            await Task.Delay(interval, token);
        }
    }
}
