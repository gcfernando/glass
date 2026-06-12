// -----------------------------------------------------------------------------
//  Glass.Message — handle to a live, modeless progress dialog. Returned by
//  GlassBuilder.ShowProgress / GlassMessage.ShowProgress so the caller can update
//  the bar and message while work runs, then close it — all thread-safe.
//
//  File        : GlassProgressController.cs
//  Developer   ::> Gehan Fernando
// -----------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Glass;

/// <summary>
/// Controls a non-blocking progress dialog while a background operation runs. Update
/// the bar with <see cref="SetValue"/> and the caption with <see cref="SetMessage"/>,
/// then call <see cref="Complete"/> (or let the user cancel) to close it. Every
/// method marshals onto the UI thread, so the controller is safe to drive from a
/// worker thread.
/// </summary>
public sealed class GlassProgressController
{
    private readonly GlassDialog _dialog;
    private volatile bool _closedByController;

    internal GlassProgressController(GlassDialog dialog, Task<GlassResult> completion)
    {
        _dialog = dialog;
        Completion = completion;
    }

    /// <summary>
    /// Completes when the dialog closes — whether the caller called
    /// <see cref="Complete"/> or the user dismissed it. The result's
    /// <see cref="GlassResult.Button"/> tells the two apart (e.g. Cancel when the
    /// user dismissed it).
    /// </summary>
    public Task<GlassResult> Completion { get; }

    /// <summary>Whether the dialog has already closed.</summary>
    public bool IsClosed => _dialog.IsDisposed;

    /// <summary>
    /// <c>true</c> once the dialog has closed because the user dismissed it (clicked a
    /// button, pressed Escape, or used the × / Alt+F4) rather than the program calling
    /// <see cref="Complete"/> or <see cref="Close"/>. Use this — not the button label —
    /// to decide whether to abort the work in progress.
    /// </summary>
    public bool WasCanceledByUser => IsClosed && !_closedByController;

    /// <summary>Updates the determinate progress bar to <paramref name="value"/> (clamped to its range).</summary>
    public void SetValue(int value) => Marshal(() => _dialog.SetProgressValue(value));

    /// <summary>Replaces the dialog's message text — handy for status lines like "Copying file 3 of 10".</summary>
    public void SetMessage(string message) => Marshal(() => _dialog.SetMessageText(message));

    /// <summary>Closes the dialog, reporting <see cref="DialogResult.OK"/> to <see cref="Completion"/>.</summary>
    public void Complete()
    {
        _closedByController = true;
        Marshal(() => _dialog.RequestClose(DialogResult.OK));
    }

    /// <summary>Closes the dialog with an explicit <paramref name="result"/>.</summary>
    public void Close(DialogResult result = DialogResult.OK)
    {
        _closedByController = true;
        Marshal(() => _dialog.RequestClose(result));
    }

    // Runs an action on the dialog's UI thread, hopping threads only when needed.
    // Uses BeginInvoke (fire-and-forget) rather than Invoke by design: callers like
    // SetValue / SetMessage are best-effort UI refreshes that must never block the
    // worker thread. The correct way to wait for the dialog to close is to await
    // Completion — not to spin on these update calls.
    // Swallows the benign races where the dialog closed underneath us.
    private void Marshal(Action action)
    {
        if (_dialog.IsDisposed || !_dialog.IsHandleCreated)
        {
            return;
        }

        try
        {
            if (_dialog.InvokeRequired)
            {
                _ = _dialog.BeginInvoke(action);
            }
            else
            {
                action();
            }
        }
        catch (ObjectDisposedException) { /* dialog closed between the check and the call */ }
        catch (InvalidOperationException) { /* handle destroyed mid-marshal */ }
    }
}
