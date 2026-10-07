// -----------------------------------------------------------------------------
//  Glass.Message — handle to a live, modeless progress dialog. Returned by
//  GlassBuilder.ShowProgress / GlassMessage.ShowProgress so the caller can update
//  the bar and message while work runs, then close it — all thread-safe.
//
//  File        : GlassProgressController.cs
//  Developer   ::> Gehan Fernando
// -----------------------------------------------------------------------------

using System;
using System.Threading;
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

    // Coalescing dispatchers: no matter how many times (or from how many threads)
    // SetValue/SetMessage/SetActivity are called between UI-thread pumps, at most
    // one BeginInvoke per kind is ever outstanding, and the UI always ends up
    // applying only the latest value. This keeps a worker that calls SetValue in a
    // tight loop from flooding the message queue with thousands of queued
    // delegates (unbounded backlog / growing memory / an ever-more-stale UI).
    private readonly Coalescer<int> _valueCoalescer;
    private readonly Coalescer<string> _messageCoalescer;
    private readonly Coalescer<GlassProgressActivity> _activityCoalescer;

    internal GlassProgressController(GlassDialog dialog, Task<GlassResult> completion)
    {
        _dialog = dialog;
        Completion = completion;
        _valueCoalescer = new Coalescer<int>(v => _dialog.SetProgressValue(v), Marshal);
        _messageCoalescer = new Coalescer<string>(m => _dialog.SetMessageText(m), Marshal);
        _activityCoalescer = new Coalescer<GlassProgressActivity>(a => _dialog.SetProgressActivity(a), Marshal);
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

    /// <summary>
    /// Updates the determinate progress bar to <paramref name="value"/> (clamped to its
    /// range). Safe to call at very high frequency from one or many worker threads:
    /// calls are coalesced, so only the most recent value is ever applied — the UI
    /// thread is never asked to catch up through a backlog of stale values.
    /// </summary>
    public void SetValue(int value) => _valueCoalescer.Post(value);

    /// <summary>
    /// Replaces the dialog's message text — handy for status lines like "Copying file
    /// 3 of 10". As with <see cref="SetValue"/>, rapid calls are coalesced to the latest message.
    /// </summary>
    public void SetMessage(string message) => _messageCoalescer.Post(message ?? string.Empty);

    /// <summary>
    /// Changes the directional flow animation on the bar — useful when an operation
    /// moves between phases (e.g. <see cref="GlassProgressActivity.FileTransfer"/>
    /// while compressing, then <see cref="GlassProgressActivity.Upload"/> while
    /// sending). No-op if the dialog has no progress bar.
    /// </summary>
    public void SetActivity(GlassProgressActivity activity) => _activityCoalescer.Post(activity);

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

    // Coalesces rapid, same-kind updates from one or many worker threads so that at
    // most one marshaled callback is ever outstanding, regardless of how many times
    // Post() is called in between: a burst of calls collapses to "apply the latest
    // value once". This bounds the amount of queued UI work to O(1) per update kind
    // instead of O(number of calls), without ever blocking the caller.
    //
    // The drain loop + recheck-after-clearing pattern below avoids the classic lost-
    // wakeup race: if a new Post() lands after the loop's last read of _dirty but
    // before _scheduled is cleared, the recheck immediately after clearing it
    // guarantees that value is still picked up by a follow-up dispatch rather than
    // being stranded until some later, unrelated Post() call happens to arrive.
    private sealed class Coalescer<T>
    {
        private readonly Action<T> _apply;
        private readonly Action<Action> _marshal;
        private T _latest;
        private int _dirty;      // 1 while a posted value hasn't been applied yet
        private int _scheduled;  // 1 while a drain callback is queued/running

        public Coalescer(Action<T> apply, Action<Action> marshal)
        {
            _apply = apply;
            _marshal = marshal;
        }

        public void Post(T value)
        {
            _latest = value;
            _ = Interlocked.Exchange(ref _dirty, 1);
            if (Interlocked.CompareExchange(ref _scheduled, 1, 0) == 0)
            {
                _marshal(Drain);
            }
        }

        private void Drain()
        {
            while (Interlocked.Exchange(ref _dirty, 0) == 1)
            {
                _apply(_latest);
            }

            _ = Interlocked.Exchange(ref _scheduled, 0);

            // A Post() may have set _dirty=1 after the loop's final (false) check but
            // before _scheduled was cleared above; if so, nothing else will drain it
            // unless we reschedule here.
            if (Volatile.Read(ref _dirty) == 1 && Interlocked.CompareExchange(ref _scheduled, 1, 0) == 0)
            {
                _marshal(Drain);
            }
        }
    }
}
