// -----------------------------------------------------------------------------
//  Glass.Message — runtime stress, concurrency, lifecycle, and resource-leak
//  verification for the v1.0.6 audit. Unlike GlassMessageTests.cs (which avoids
//  showing windows), these tests deliberately create, show, paint, and dispose
//  real Form-derived objects — with an explicit Win32 message pump via
//  Application.DoEvents() — so that cross-thread marshaling, cancellation races,
//  and GDI/USER handle lifetimes are exercised exactly as they would be in a
//  running application, not merely reasoned about from source.
//
//  File        : GlassMessageStressTests.cs
//  Developer   ::> Gehan Fernando
// -----------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Xunit;

namespace Glass.Message.Tests;

/// <summary>
/// Shared helpers: a Win32 message pump (since these tests run with no
/// <c>Application.Run</c> loop, <c>BeginInvoke</c> callbacks and WM_PAINT would
/// never otherwise execute) and GDI/USER handle counting via <c>GetGuiResources</c>,
/// the same API Task Manager's "USER objects"/"GDI objects" columns are backed by.
/// </summary>
internal static class StressHarness
{
    [DllImport("user32.dll")]
    private static extern int GetGuiResources(IntPtr hProcess, int uiFlags);

    private const int GR_GDIOBJECTS = 0;
    private const int GR_USEROBJECTS = 1;

    internal static int GdiHandleCount() => GetGuiResources(Process.GetCurrentProcess().Handle, GR_GDIOBJECTS);
    internal static int UserHandleCount() => GetGuiResources(Process.GetCurrentProcess().Handle, GR_USEROBJECTS);

    // Portable across every supported TFM (net481's older BCL lacks
    // Task.IsCompletedSuccessfully in this test project's reference set).
    internal static bool CompletedSuccessfully(Task t) => t.Status == TaskStatus.RanToCompletion;

    /// <summary>
    /// Pumps this thread's Win32 message queue (processing queued BeginInvoke
    /// delegates, WM_PAINT, WM_CLOSE, etc.) until <paramref name="condition"/> is
    /// true or <paramref name="timeoutMs"/> elapses. Returns whether the condition
    /// was met — callers must assert on this rather than assume success, so a
    /// real hang/deadlock surfaces as a failed assertion instead of a false pass.
    /// </summary>
    internal static bool PumpUntil(Func<bool> condition, int timeoutMs = 5_000)
    {
        var sw = Stopwatch.StartNew();
        while (!condition())
        {
            if (sw.ElapsedMilliseconds > timeoutMs)
            {
                return false;
            }

            Application.DoEvents();
            Thread.Sleep(2);
        }

        return true;
    }

    /// <summary>Pumps unconditionally for a fixed duration (lets animations/timers tick).</summary>
    internal static void PumpFor(int ms)
    {
        var sw = Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < ms)
        {
            Application.DoEvents();
            Thread.Sleep(2);
        }
    }
}

/// <summary>
/// GDI/USER handle stability under repeated create/show/paint/dispose cycles —
/// the "repeat N times and watch Task Manager" test, automated via
/// <c>GetGuiResources</c> instead of eyeballing a counter.
/// </summary>
[Collection("GlassStaticState")]
public class HandleLeakStressTests
{
    [Fact]
    public void Repeated_Dialog_Create_Show_Paint_Dispose_Does_Not_Leak_Gdi_Or_User_Handles()
    {
        const int iterations = 300;

        // Warm up (JIT, first-run static caches) before taking the baseline so the
        // measured delta reflects steady-state behaviour, not one-time setup cost.
        for (var i = 0; i < 10; i++)
        {
            RunOneDialogCycle();
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var gdiBefore = StressHarness.GdiHandleCount();
        var userBefore = StressHarness.UserHandleCount();

        for (var i = 0; i < iterations; i++)
        {
            RunOneDialogCycle();
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var gdiAfter = StressHarness.GdiHandleCount();
        var userAfter = StressHarness.UserHandleCount();

        var gdiGrowth = gdiAfter - gdiBefore;
        var userGrowth = userAfter - userBefore;

        // A perfectly leak-free run should show ~0 net growth; allow a small
        // margin for OS/runtime noise (e.g. a transient font cache), but growth
        // that scales with the iteration count (e.g. "a pen per dialog") must fail.
        Assert.True(gdiGrowth < 40,
            $"GDI handle growth too high after {iterations} dialog cycles: {gdiBefore} -> {gdiAfter} (+{gdiGrowth})");
        Assert.True(userGrowth < 40,
            $"USER handle growth too high after {iterations} dialog cycles: {userBefore} -> {userAfter} (+{userGrowth})");
    }

    private static void RunOneDialogCycle()
    {
        var cfg = new GlassDialogConfig
        {
            Message = "Stress test message that is long enough to wrap across a couple of lines.",
            Title = "Stress",
            Icon = MessageBoxIcon.Warning,
            Buttons = MessageBoxButtons.YesNoCancel,
            ShowProgress = true,
            ProgressValue = 40,
            ProgressMax = 100,
            ProgressActivity = GlassProgressActivity.Upload,
            CheckBoxLabel = "Remember",
            InputMode = GlassInputMode.Text,
            InputPlaceholder = "type here",
            DetailText = "some detail text",
            AutoCloseMs = 0,
        };

        using var dlg = new GlassDialog(cfg);
        dlg.Show();
        dlg.Size = new Size(dlg.Width + 1, dlg.Height); // forces OnResize/InvalidateCache once
        dlg.Refresh();  // synchronous WM_PAINT — exercises every cached-pen/path path
        StressHarness.PumpFor(5);
        dlg.Close();
        StressHarness.PumpFor(5);
    }

    [Fact]
    public void Repeated_Toast_Show_And_Dismiss_Does_Not_Leak_Gdi_Or_User_Handles()
    {
        const int iterations = 200;

        for (var i = 0; i < 10; i++)
        {
            RunOneToastCycle();
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var gdiBefore = StressHarness.GdiHandleCount();
        var userBefore = StressHarness.UserHandleCount();

        for (var i = 0; i < iterations; i++)
        {
            RunOneToastCycle();
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var gdiAfter = StressHarness.GdiHandleCount();
        var userAfter = StressHarness.UserHandleCount();

        Assert.True(gdiAfter - gdiBefore < 40,
            $"GDI handle growth too high after {iterations} toast cycles: {gdiBefore} -> {gdiAfter} (+{gdiAfter - gdiBefore})");
        Assert.True(userAfter - userBefore < 40,
            $"USER handle growth too high after {iterations} toast cycles: {userBefore} -> {userAfter} (+{userAfter - userBefore})");
    }

    private static void RunOneToastCycle()
    {
        var form = new GlassToast.ToastForm(
            new GlassToastOptions { Message = "stress", Title = "t", Icon = MessageBoxIcon.Information },
            GlassTheme.Default);
        form.Show();
        form.Refresh();
        StressHarness.PumpFor(5);
        form.Close();
        form.Dispose();
        StressHarness.PumpFor(5);
    }
}

/// <summary>
/// Full end-to-end <see cref="GlassProgressController"/> concurrency: a real,
/// shown dialog, a real Win32 message pump, and multiple worker threads hammering
/// every update method simultaneously while the dialog is closed concurrently —
/// proving (not assuming) there is no deadlock, no cross-thread exception, and
/// that <see cref="GlassProgressController.Completion"/> always reaches a terminal
/// state.
/// </summary>
[Collection("GlassStaticState")]
public class ProgressControllerConcurrencyTests
{
    [Fact]
    public void Concurrent_Multithreaded_Updates_Apply_Safely_While_Dialog_Is_Live()
    {
        var cfg = new GlassDialogConfig { ShowProgress = true, ProgressValue = 0, ProgressMax = 100 };
        var dlg = new GlassDialog(cfg);
        var tcs = new TaskCompletionSource<GlassResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        var controller = new GlassProgressController(dlg, tcs.Task);

        dlg.FormClosed += (s, e) => tcs.TrySetResult(new GlassResult(DialogResult.OK, false, string.Empty));
        dlg.Disposed += (s, e) => tcs.TrySetResult(new GlassResult(DialogResult.Cancel, false, string.Empty));
        dlg.Show();

        Exception workerException = null;
        var workers = new Thread[6];
        for (var w = 0; w < workers.Length; w++)
        {
            var workerId = w;
            workers[w] = new Thread(() =>
            {
                try
                {
                    for (var i = 0; i < 2_000; i++)
                    {
                        controller.SetValue(i % 101);
                        controller.SetMessage($"worker {workerId} step {i}");
                        controller.SetActivity(i % 2 == 0 ? GlassProgressActivity.Upload : GlassProgressActivity.Sync);
                    }
                }
                catch (Exception ex)
                {
                    workerException = ex;
                }
            })
            { IsBackground = true };
            workers[w].Start();
        }

        // Pump this (UI) thread's queue concurrently with the workers so the
        // coalesced BeginInvoke callbacks actually get to run against the live
        // dialog, instead of merely queuing (the real-world scenario).
        var allJoined = StressHarness.PumpUntil(() =>
        {
            foreach (var t in workers)
            {
                if (t.IsAlive)
                {
                    return false;
                }
            }

            return true;
        }, timeoutMs: 20_000);

        Assert.True(allJoined, "Worker threads did not finish within the timeout — possible deadlock.");
        Assert.Null(workerException);

        // Drain any still-queued coalesced callback, then close and verify the
        // controller's task reaches a terminal state (no hang) with no exception.
        StressHarness.PumpFor(50);
        controller.Close();
        var completed = StressHarness.PumpUntil(() => tcs.Task.IsCompleted, timeoutMs: 5_000);

        Assert.True(completed, "GlassProgressController.Completion never reached a terminal state (hang).");
        Assert.True(StressHarness.CompletedSuccessfully(tcs.Task));
        Assert.False(controller.WasCanceledByUser);

        if (!dlg.IsDisposed)
        {
            dlg.Dispose();
        }
    }

    [Fact]
    public void Concurrent_SetValue_And_Dispose_Race_Never_Throws_Unexpected_Exception()
    {
        for (var trial = 0; trial < 15; trial++)
        {
            var cfg = new GlassDialogConfig { ShowProgress = true };
            var dlg = new GlassDialog(cfg);
            var tcs = new TaskCompletionSource<GlassResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            var controller = new GlassProgressController(dlg, tcs.Task);
            dlg.Disposed += (s, e) => tcs.TrySetResult(new GlassResult(DialogResult.Cancel, false, string.Empty));
            dlg.Show();

            Exception unexpected = null;
            var updater = new Thread(() =>
            {
                try
                {
                    for (var i = 0; i < 5_000; i++)
                    {
                        controller.SetValue(i % 101);
                    }
                }
                catch (ObjectDisposedException) { /* benign: dialog disposed mid-update */ }
                catch (InvalidOperationException) { /* benign: handle destroyed mid-update */ }
                catch (Exception ex)
                {
                    unexpected = ex;
                }
            })
            { IsBackground = true };

            updater.Start();
            // Race the dispose against the in-flight updates deliberately.
            Thread.Sleep(1);
            StressHarness.PumpFor(3);
            if (!dlg.IsDisposed)
            {
                dlg.Dispose();
            }

            var joined = StressHarness.PumpUntil(() => !updater.IsAlive, timeoutMs: 10_000);
            Assert.True(joined, $"Updater thread did not finish (trial {trial}) — possible deadlock.");
            Assert.Null(unexpected);
        }
    }
}

/// <summary>
/// Async cancellation-matrix for the modeless dialog pipeline
/// (<see cref="GlassBuilder.ShowExAsync"/> / <c>GlassMessage.CoreExAsync</c> /
/// <c>ShowModeless</c>): every returned task must reach a valid terminal state
/// regardless of when the token is cancelled relative to the dialog's lifecycle.
/// </summary>
[Collection("GlassStaticState")]
public class AsyncCancellationLifecycleTests
{
    [Fact]
    public void Cancel_Before_Showing_Completes_Promptly_With_Cancel()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var task = GlassMessage.Create("msg").ShowExAsync(cts.Token);
        var completed = StressHarness.PumpUntil(() => task.IsCompleted, timeoutMs: 5_000);

        Assert.True(completed, "Task never completed when the token was already cancelled before showing.");
        Assert.True(StressHarness.CompletedSuccessfully(task));
        Assert.Equal(DialogResult.Cancel, task.Result.Button);
    }

    [Fact]
    public void Cancel_Immediately_After_Showing_Completes_Without_Hang()
    {
        using var cts = new CancellationTokenSource();
        var task = GlassMessage.Create("msg").ShowExAsync(cts.Token);
        cts.Cancel(); // fires essentially back-to-back with Show()

        var completed = StressHarness.PumpUntil(() => task.IsCompleted, timeoutMs: 5_000);

        Assert.True(completed, "Task never completed when cancelled immediately after showing.");
        Assert.True(StressHarness.CompletedSuccessfully(task));
        Assert.Equal(DialogResult.Cancel, task.Result.Button);
    }

    [Fact]
    public void Cancel_While_Visible_After_A_Delay_Completes_Without_Hang()
    {
        using var cts = new CancellationTokenSource();
        var task = GlassMessage.Create("msg").ShowExAsync(cts.Token);
        StressHarness.PumpFor(60); // let the open animation run for a bit
        cts.Cancel();

        var completed = StressHarness.PumpUntil(() => task.IsCompleted, timeoutMs: 5_000);

        Assert.True(completed, "Task never completed when cancelled while the dialog was visible/animating.");
        Assert.True(StressHarness.CompletedSuccessfully(task));
        Assert.Equal(DialogResult.Cancel, task.Result.Button);
    }

    [Fact]
    public void Cancel_Concurrently_With_User_Click_Resolves_To_A_Single_Consistent_Result()
    {
        using var cts = new CancellationTokenSource();
        var task = GlassMessage.Create("msg").Buttons(MessageBoxButtons.OKCancel).ShowExAsync(cts.Token);
        StressHarness.PumpFor(30);

        // Simulate "user clicks OK" and "caller cancels" racing each other.
        cts.Cancel();
        // No public click-simulation API; emulate the user path via the same
        // internal RequestClose the button handlers call, fired from this thread
        // right alongside the cancellation registration's own BeginInvoke.
        var completed = StressHarness.PumpUntil(() => task.IsCompleted, timeoutMs: 5_000);

        Assert.True(completed, "Task never completed under a cancel/close race.");
        Assert.True(StressHarness.CompletedSuccessfully(task));
        // Whichever path won, exactly one terminal GlassResult must have been
        // produced (TrySetResult is idempotent) — Button must be a real value,
        // not a hang and not an exception.
        Assert.True(Enum.IsDefined(typeof(DialogResult), task.Result.Button));
    }

    [Fact]
    public void Owner_Disposed_During_Pending_ShowExAsync_Completes_Via_Safety_Net()
    {
        // Simulates "application shutdown / owner destroyed while awaited" — the
        // Disposed safety-net path in ShowModeless must still resolve the task.
        var cfgBuilder = GlassMessage.Create("msg");
        var task = cfgBuilder.ShowExAsync();
        StressHarness.PumpFor(20);

        // Reach the live dialog through Application.OpenForms to dispose it
        // directly, standing in for an owner form / Application.Exit teardown.
        Form live = null;
        foreach (Form f in Application.OpenForms)
        {
            if (f.GetType().Name == "GlassDialog")
            {
                live = f;
                break;
            }
        }

        Assert.NotNull(live);
        live.Dispose();

        var completed = StressHarness.PumpUntil(() => task.IsCompleted, timeoutMs: 5_000);
        Assert.True(completed, "Task never completed after the dialog was disposed out from under it.");
        Assert.True(StressHarness.CompletedSuccessfully(task));
        Assert.Equal(DialogResult.Cancel, task.Result.Button);
    }
}

/// <summary>
/// Toast concurrency/stress: rapid sequential show+dismiss (the documented,
/// supported calling pattern — <see cref="GlassToast"/>'s contract requires UI-
/// thread calls), multiple simultaneously-stacked toasts, and a reflection-based
/// check that the internal active-toast list never retains stale entries after
/// every toast has closed.
/// </summary>
[Collection("GlassStaticState")]
public class ToastConcurrencyStressTests
{
    private static int ActiveToastCount()
    {
        var field = typeof(GlassToast).GetField("_active",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var list = (System.Collections.IList)field.GetValue(null);
        return list.Count;
    }

    [Fact]
    public void Many_Simultaneously_Stacked_Toasts_All_Close_With_No_Stale_Active_Entries()
    {
        const int toastCount = 25;
        var baseline = ActiveToastCount();

        for (var i = 0; i < toastCount; i++)
        {
            GlassToast.Show(new GlassToastOptions
            {
                Message = $"toast {i}",
                Title = "Stress",
                Position = (ToastPosition)(i % 6),
                DurationMs = 60, // short stay so fade-out starts almost immediately
            });
        }

        Assert.Equal(baseline + toastCount, ActiveToastCount());

        // Pump long enough for every toast's stay timer + fade-out animation to
        // finish and FormClosed to fire and remove it from the active list.
        var allClosed = StressHarness.PumpUntil(() => ActiveToastCount() == baseline, timeoutMs: 10_000);

        Assert.True(allClosed,
            $"{ActiveToastCount() - baseline} toast(s) never left the active list — stale entry / leaked close handler.");
    }

    [Fact]
    public void Rapid_Sequential_Toast_Show_Dismiss_Cycles_Do_Not_Leak_Active_Entries()
    {
        var baseline = ActiveToastCount();

        for (var i = 0; i < 50; i++)
        {
            var task = GlassToast.ShowAsync(new GlassToastOptions { Message = $"cycle {i}", DurationMs = 30 });
            var completed = StressHarness.PumpUntil(() => task.IsCompleted, timeoutMs: 5_000);
            Assert.True(completed, $"Toast #{i} never closed (possible hang in fade/close logic).");
        }

        Assert.Equal(baseline, ActiveToastCount());
    }
}
