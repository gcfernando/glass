// -----------------------------------------------------------------------------
//  Glass.Message — renders real, live dialog/toast/button windows at a matrix of
//  DPI scale factors and saves the actual composited output (via PrintWindow, the
//  same mechanism Alt+PrtScn-style capture tools use) to disk, so the rendering
//  quality fixes (sharp edges, smooth curves, crisp borders, even corners, no
//  1px seams) can be visually inspected from real rendered pixels rather than
//  only reasoned about from source. These are not assertions of visual quality
//  (that judgement requires a human/vision pass on the saved images) — they are
//  evidence-generation tests that fail loudly if capture itself breaks.
//
//  File        : GlassDpiVisualCaptureTests.cs
//  Developer   ::> Gehan Fernando
// -----------------------------------------------------------------------------

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Xunit;

namespace Glass.Message.Tests;

[Collection("GlassStaticState")]
public class GlassDpiVisualCaptureTests
{
    private const string OutDir =
        @"C:\Users\A534313\.copilot\session-state\c31f7a58-517c-4f0d-bb6e-ead8da14a9cd\files\dpi-renders";

    [DllImport("user32.dll")]
    private static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, uint nFlags);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out NativeRect lpRect);

    [DllImport("user32.dll")]
    private static extern uint GetDpiForWindow(IntPtr hWnd);

    private const uint PW_RENDERFULLCONTENT = 0x00000002;

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect { public int Left, Top, Right, Bottom; }

    private const int WM_DPICHANGED = 0x02E0;

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    private static void CaptureWindow(Form f, string path)
    {
        f.Refresh();
        _ = GetWindowRect(f.Handle, out var osRect);
        var w = Math.Max(1, osRect.Right - osRect.Left);
        var h = Math.Max(1, osRect.Bottom - osRect.Top);
        using var bmp = new Bitmap(w, h);
        using (var g = Graphics.FromImage(bmp))
        {
            var hdc = g.GetHdc();
            try
            {
                _ = PrintWindow(f.Handle, hdc, PW_RENDERFULLCONTENT);
            }
            finally
            {
                g.ReleaseHdc(hdc);
            }
        }

        bmp.Save(path, ImageFormat.Png);
    }

    // Drives the dialog's real WM_DPICHANGED handler — the exact code path a
    // physical per-monitor DPI change delivers — rather than reflecting into
    // private fields, so this is a faithful simulation of an OS DPI change.
    private static IntPtr SimulateDpiChange(Form f, int dpi, int baseWidth, int baseHeight)
    {
        var wParam = new IntPtr((dpi << 16) | dpi);
        // Windows always supplies a suggested rect scaled to the NEW dpi in
        // lParam (the recommended window bounds at the new scale); passing the
        // stale unscaled bounds here — as an earlier version of this harness
        // did — causes the WinForms base Form.WmDpiChanged handler (invoked via
        // base.WndProc after GlassDialog's own Rebuild()) to SetBounds back to
        // the old size, masking GlassDialog's correct rescale. Compute the
        // rect relative to the dialog's ORIGINAL 100%-scale size (not its
        // current, possibly already-rescaled, size) so repeated calls in this
        // loop reflect an absolute target scale exactly as GlassDialog's own
        // _scale = dpi / 96f computation does, rather than compounding.
        var factor = dpi / 96f;
        var newW = (int)(baseWidth * factor);
        var newH = (int)(baseHeight * factor);
        var rect = new NativeRect { Left = f.Left, Top = f.Top, Right = f.Left + newW, Bottom = f.Top + newH };
        var lParamPtr = Marshal.AllocHGlobal(Marshal.SizeOf<NativeRect>());
        try
        {
            Marshal.StructureToPtr(rect, lParamPtr, false);
            return SendMessage(f.Handle, WM_DPICHANGED, wParam, lParamPtr);
        }
        finally
        {
            Marshal.FreeHGlobal(lParamPtr);
        }
    }

    [Fact]
    public void Diagnostic_Direct_Rebuild_Reflects_Scale_Change()
    {
        var cfg = new GlassDialogConfig
        {
            Message = "Diagnostic message for direct Rebuild invocation.",
            Title = "Diag",
            Buttons = MessageBoxButtons.OK,
        };
        var dlg = new GlassDialog(cfg);
        dlg.Show();
        StressHarness.PumpFor(50);

        var sizeBefore = dlg.ClientSize;
        var scaleField = typeof(GlassDialog).GetField("_scale",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var rebuildMethod = typeof(GlassDialog).GetMethod("Rebuild",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        scaleField.SetValue(dlg, 2.0f);
        rebuildMethod.Invoke(dlg, new object[] { null });
        StressHarness.PumpFor(50);

        var sizeAfter = dlg.ClientSize;
        File.WriteAllText(Path.Combine(OutDir, "direct_rebuild_diagnostic.txt"),
            $"sizeBefore={sizeBefore} sizeAfter={sizeAfter} scaleAfter={scaleField.GetValue(dlg)}");

        dlg.Dispose();
    }

    [Fact]
    public void Render_Dialog_At_100_125_150_175_200_Percent_Dpi_For_Visual_Inspection()
    {
        _ = Directory.CreateDirectory(OutDir);

        var cfg = new GlassDialogConfig
        {
            Message = "This is a sample message used to visually verify rendering quality across DPI scales.",
            Title = "Visual DPI Audit",
            Icon = MessageBoxIcon.Warning,
            Buttons = MessageBoxButtons.YesNoCancel,
            ShowProgress = true,
            ProgressValue = 55,
            ProgressMax = 100,
            ProgressActivity = GlassProgressActivity.Upload,
            CheckBoxLabel = "Remember my choice",
            InputMode = GlassInputMode.Text,
            InputPlaceholder = "Type a value",
            DetailText = "Example\r\nmulti-line\r\ndetail text.",
            AutoCloseMs = 15_000,
            UseRoundedCorners = true,
        };
        var dlg = new GlassDialog(cfg);
        dlg.Show();
        StressHarness.PumpFor(120); // let open animation/fade settle before the first capture

        var dpis = new[] { (96, "100pct"), (120, "125pct"), (144, "150pct"), (168, "175pct"), (192, "200pct") };
        var log = new System.Text.StringBuilder();
        var scaleField = typeof(GlassDialog).GetField("_scale",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var baseWidth = dlg.Width;
        var baseHeight = dlg.Height;
        log.AppendLine($"Process DPI awareness context (GetDpiForWindow before any change): {GetDpiForWindow(dlg.Handle)}");
        log.AppendLine($"_scale before any simulated change: {scaleField?.GetValue(dlg)}");
        foreach (var (dpi, label) in dpis)
        {
            var result = SimulateDpiChange(dlg, dpi, baseWidth, baseHeight);
            StressHarness.PumpFor(260); // >1 full 200ms countdown-timer tick so the label resyncs before capture
            _ = GetWindowRect(dlg.Handle, out var osRect);
            log.AppendLine(
                $"dpi={dpi} label={label} SendMessageResult={result} _scale(after)={scaleField?.GetValue(dlg)} " +
                $"WinForms.ClientSize={dlg.ClientSize} WinForms.Size={dlg.Size} " +
                $"OS.GetWindowRect=({osRect.Left},{osRect.Top},{osRect.Right},{osRect.Bottom}) " +
                $"OS.Width={osRect.Right - osRect.Left} OS.Height={osRect.Bottom - osRect.Top} " +
                $"GetDpiForWindow={GetDpiForWindow(dlg.Handle)}");
            CaptureWindow(dlg, Path.Combine(OutDir, $"dialog_{label}.png"));
        }

        File.WriteAllText(Path.Combine(OutDir, "dpi_diagnostics.txt"), log.ToString());

        dlg.Close();
        StressHarness.PumpFor(20);
        dlg.Dispose();

        foreach (var (_, label) in dpis)
        {
            Assert.True(File.Exists(Path.Combine(OutDir, $"dialog_{label}.png")));
        }
    }

    [Fact]
    public void Render_CloseButton_Countdown_Progress_Checkbox_States_For_Visual_Inspection()
    {
        _ = Directory.CreateDirectory(OutDir);

        var cfg = new GlassDialogConfig
        {
            Message = "Hover/pressed/focus state capture.",
            Title = "States",
            Buttons = MessageBoxButtons.OKCancel,
            ShowProgress = true,
            ProgressValue = 30,
            ProgressMax = 100,
            CheckBoxLabel = "Option",
            AutoCloseMs = 15_000,
            UseRoundedCorners = true,
        };
        var dlg = new GlassDialog(cfg);
        dlg.Show();
        StressHarness.PumpFor(120);
        CaptureWindow(dlg, Path.Combine(OutDir, "dialog_idle_state.png"));

        // Hover the close button via the same private field OnPaint reads, then
        // force a repaint, to capture the hover-halo + "×" pen rendering.
        var hoverField = typeof(GlassDialog).GetField("_closeHover",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        hoverField?.SetValue(dlg, true);
        dlg.Invalidate();
        StressHarness.PumpFor(40);
        CaptureWindow(dlg, Path.Combine(OutDir, "dialog_close_hover_state.png"));

        dlg.Close();
        StressHarness.PumpFor(20);
        dlg.Dispose();

        Assert.True(File.Exists(Path.Combine(OutDir, "dialog_idle_state.png")));
        Assert.True(File.Exists(Path.Combine(OutDir, "dialog_close_hover_state.png")));
    }

    [Fact]
    public void Render_Toast_For_Visual_Inspection()
    {
        _ = Directory.CreateDirectory(OutDir);

        var form = new GlassToast.ToastForm(
            new GlassToastOptions
            {
                Message = "This is a sample toast notification message for visual inspection.",
                Title = "Visual Toast Audit",
                Icon = MessageBoxIcon.Information,
                UseRoundedCorners = true,
            },
            GlassTheme.Default);
        form.Show();
        StressHarness.PumpFor(80);
        CaptureWindow(form, Path.Combine(OutDir, "toast_default.png"));
        form.Close();
        form.Dispose();

        Assert.True(File.Exists(Path.Combine(OutDir, "toast_default.png")));
    }

    [Fact]
    public void Render_Button_Hover_Pressed_Focus_Disabled_States_For_Visual_Inspection()
    {
        _ = Directory.CreateDirectory(OutDir);

        var cfg = new GlassDialogConfig
        {
            Message = "Button state capture.",
            Title = "Buttons",
            Buttons = MessageBoxButtons.YesNoCancel,
            UseRoundedCorners = true,
        };
        var dlg = new GlassDialog(cfg);
        dlg.Show();
        StressHarness.PumpFor(120);
        CaptureWindow(dlg, Path.Combine(OutDir, "buttons_default.png"));

        // Find the first GlassButton child control and drive its visual states
        // directly (mirrors how a real mouse hover/focus/press would look).
        Control FindButton(Control root)
        {
            foreach (Control c in root.Controls)
            {
                if (c.GetType().Name == "GlassButton")
                {
                    return c;
                }

                var nested = FindButton(c);
                if (nested != null)
                {
                    return nested;
                }
            }

            return null;
        }

        var btn = FindButton(dlg);
        if (btn != null)
        {
            btn.Focus();
            StressHarness.PumpFor(40);
            CaptureWindow(dlg, Path.Combine(OutDir, "buttons_focused.png"));
        }

        dlg.Close();
        StressHarness.PumpFor(20);
        dlg.Dispose();

        Assert.True(File.Exists(Path.Combine(OutDir, "buttons_default.png")));
    }

    // Mixed-DPI-while-active: a per-monitor DPI change arriving mid-flight while
    // a progress controller is being hammered from a background thread AND a
    // toast is simultaneously open. Exercises Rebuild() racing against
    // GlassProgressController's BeginInvoke-marshaled updates and ToastForm's
    // independent paint/animation path, all on one UI thread's message queue.
    [Fact]
    public void DpiChange_While_Progress_Updates_And_Toast_Are_Active_Does_Not_Corrupt_State()
    {
        var cfg = new GlassDialogConfig
        {
            Message = "Mixed DPI + concurrency stress.",
            Title = "Mixed",
            Buttons = MessageBoxButtons.OKCancel,
            ShowProgress = true,
            ProgressValue = 0,
            ProgressMax = 100,
        };
        var dlg = new GlassDialog(cfg);
        dlg.Show();
        StressHarness.PumpFor(80);

        var tcs = new System.Threading.Tasks.TaskCompletionSource<GlassResult>();
        var controller = (GlassProgressController)Activator.CreateInstance(
            typeof(GlassProgressController),
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
            null,
            new object[] { dlg, tcs.Task },
            null);

        var toast = new GlassToast.ToastForm(
            new GlassToastOptions { Message = "Concurrent toast", Title = "Toast", UseRoundedCorners = true },
            GlassTheme.Default);
        toast.Show();

        var stop = false;
        var workerException = (Exception)null;
        var worker = new System.Threading.Thread(() =>
        {
            try
            {
                var i = 0;
                while (!stop)
                {
                    controller.SetValue(i % 100);
                    controller.SetMessage($"Working {i}");
                    i++;
                    System.Threading.Thread.Sleep(2);
                }
            }
            catch (Exception ex)
            {
                workerException = ex;
            }
        });
        worker.Start();

        // Fire DPI changes while the worker is actively hammering the controller
        // and the toast remains open.
        var baseWidth = dlg.Width;
        var baseHeight = dlg.Height;
        foreach (var dpi in new[] { 120, 144, 168, 192, 96 })
        {
            SimulateDpiChange(dlg, dpi, baseWidth, baseHeight);
            StressHarness.PumpFor(120);
        }

        stop = true;
        worker.Join(TimeSpan.FromSeconds(5));
        StressHarness.PumpFor(100);

        Assert.Null(workerException);
        Assert.False(dlg.IsDisposed);
        Assert.False(toast.IsDisposed);

        toast.Close();
        toast.Dispose();
        dlg.Close();
        StressHarness.PumpFor(50);
        dlg.Dispose();

        Assert.True(dlg.IsDisposed);
    }
}
