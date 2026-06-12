// -----------------------------------------------------------------------------
//  Glass.Message — the public entry point. These static overloads mirror the
//  shape of System.Windows.Forms.MessageBox so existing code can switch over by
//  changing nothing more than the type name, while ShowAsync/Create open the door
//  to the richer feature set.
//
//  File        : GlassMessage.cs
//  Developer   ::> Gehan Fernando
// -----------------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Glass;

/// <summary>
/// Drop-in modern replacement for <see cref="MessageBox"/>. The <c>Show</c>
/// overloads are signature-compatible with the framework's message box; use
/// <see cref="Create"/> for the fluent builder and <see cref="ShowAsync(string, string, MessageBoxIcon, MessageBoxButtons, CancellationToken)"/>
/// for non-blocking dialogs.
/// </summary>
/// <remarks>
/// All methods that create or show a dialog must be called on the application's
/// UI (STA) thread. Calling from a background or thread-pool thread will throw
/// <see cref="System.InvalidOperationException"/>.
/// </remarks>
public static class GlassMessage
{
    // Volatile backing fields guarantee that writes from any thread are immediately
    // visible to all readers — including on weakly-ordered architectures such as
    // Windows on ARM (Surface Pro X, Copilot+ PCs).
    private static volatile GlassTheme _defaultTheme = GlassTheme.Default;
    private static volatile bool _useRoundedCorners;
    private static volatile bool _playSystemSounds;

    /// <summary>
    /// Theme applied when a caller doesn't specify one. Defaults to the built-in
    /// dark palette; assign <see cref="GlassTheme.AutoDetect"/> to follow Windows.
    /// </summary>
    public static GlassTheme DefaultTheme
    {
        get => _defaultTheme;
        set => _defaultTheme = value;
    }

    /// <summary>
    /// Global default for rounded corners. Individual dialogs can override this
    /// through the builder; left off by default to match classic square chrome.
    /// </summary>
    public static bool UseRoundedCorners
    {
        get => _useRoundedCorners;
        set => _useRoundedCorners = value;
    }

    /// <summary>
    /// Global default for playing the icon's Windows system sound when a dialog
    /// opens (as the classic <see cref="MessageBox"/> does). Off by default;
    /// individual dialogs can override this through the builder's <c>Sound</c> method.
    /// </summary>
    public static bool PlaySystemSounds
    {
        get => _playSystemSounds;
        set => _playSystemSounds = value;
    }

    // --- MessageBox-compatible synchronous overloads --------------------------

    public static DialogResult Show(string message)
        => Core(null, message, string.Empty, MessageBoxIcon.None,
                MessageBoxButtons.OK, MessageBoxDefaultButton.Button1, null);

    public static DialogResult Show(string message, string title)
        => Core(null, message, title, MessageBoxIcon.None,
                MessageBoxButtons.OK, MessageBoxDefaultButton.Button1, null);

    public static DialogResult Show(string message, string title, MessageBoxIcon icon)
        => Core(null, message, title, icon,
                MessageBoxButtons.OK, MessageBoxDefaultButton.Button1, null);

    public static DialogResult Show(string message, string title,
                                    MessageBoxIcon icon, MessageBoxButtons buttons)
        => Core(null, message, title, icon, buttons,
                MessageBoxDefaultButton.Button1, null);

    public static DialogResult Show(string message, string title,
                                    MessageBoxIcon icon, MessageBoxButtons buttons,
                                    MessageBoxDefaultButton defaultButton)
        => Core(null, message, title, icon, buttons, defaultButton, null);

    public static DialogResult Show(IWin32Window owner, string message, string title,
                                    MessageBoxIcon icon, MessageBoxButtons buttons,
                                    MessageBoxDefaultButton defaultButton)
        => Core(owner, message, title, icon, buttons, defaultButton, null);

    public static DialogResult Show(IWin32Window owner, string message, string title,
                                    MessageBoxIcon icon, MessageBoxButtons buttons,
                                    MessageBoxDefaultButton defaultButton, GlassTheme theme)
        => Core(owner, message, title, icon, buttons, defaultButton, theme);

    /// <summary>
    /// Shows the dialog without blocking the UI thread. The returned task completes
    /// with the chosen button once the dialog closes. The optional
    /// <paramref name="cancellationToken"/> closes the dialog (yielding
    /// <see cref="DialogResult.Cancel"/>) if it is cancelled while open.
    /// </summary>
    /// <remarks>Must be called on the UI (STA) thread.</remarks>
    public static Task<DialogResult> ShowAsync(
        string message,
        string title = "",
        MessageBoxIcon icon = MessageBoxIcon.None,
        MessageBoxButtons buttons = MessageBoxButtons.OK,
        CancellationToken cancellationToken = default)
        => CoreAsync(null, message, title, icon, buttons,
                     MessageBoxDefaultButton.Button1, null, cancellationToken);

    /// <summary>
    /// Async overload that also accepts an explicit <paramref name="theme"/>.
    /// </summary>
    /// <remarks>Must be called on the UI (STA) thread.</remarks>
    public static Task<DialogResult> ShowAsync(
        string message,
        string title,
        MessageBoxIcon icon,
        MessageBoxButtons buttons,
        GlassTheme theme,
        CancellationToken cancellationToken = default)
        => CoreAsync(null, message, title, icon, buttons,
                     MessageBoxDefaultButton.Button1, theme, cancellationToken);

    /// <summary>
    /// Starts a fluent <see cref="GlassBuilder"/> for dialogs that need more than
    /// a message and buttons — input fields, checkboxes, progress, detail, etc.
    /// </summary>
    public static GlassBuilder Create(string message) => new(message);

    private static GlassDialogConfig BasicConfig(
        string message, string title, MessageBoxIcon icon, MessageBoxButtons buttons,
        MessageBoxDefaultButton defaultButton, GlassTheme theme, string[] customLabels)
        => new()
        {
            Message = message ?? string.Empty,
            Title = title ?? string.Empty,
            Icon = icon,
            Buttons = buttons,
            DefaultButton = defaultButton,
            Theme = theme ?? _defaultTheme ?? GlassTheme.Default,
            CustomLabels = customLabels,
        };

    internal static DialogResult Core(
        IWin32Window owner,
        string message,
        string title,
        MessageBoxIcon icon,
        MessageBoxButtons buttons,
        MessageBoxDefaultButton defaultButton,
        GlassTheme theme,
        string[] customLabels = null)
    {
        using var dlg = new GlassDialog(
            BasicConfig(message, title, icon, buttons, defaultButton, theme, customLabels));
        return owner == null ? dlg.ShowDialog() : dlg.ShowDialog(owner);
    }

    internal static GlassResult CoreEx(IWin32Window owner, GlassDialogConfig config)
    {
        config.Theme ??= _defaultTheme ?? GlassTheme.Default;
        config.UseRoundedCorners ??= _useRoundedCorners;
        using var dlg = new GlassDialog(config);
        var result = owner == null ? dlg.ShowDialog() : dlg.ShowDialog(owner);
        return new GlassResult(result, dlg.CheckBoxChecked, dlg.InputText);
    }

    private static async Task<DialogResult> CoreAsync(
        IWin32Window owner,
        string message,
        string title,
        MessageBoxIcon icon,
        MessageBoxButtons buttons,
        MessageBoxDefaultButton defaultButton,
        GlassTheme theme,
        CancellationToken ct)
    {
        var dlg = new GlassDialog(
            BasicConfig(message, title, icon, buttons, defaultButton, theme, null));
        var result = await ShowModeless(owner, dlg, ct).ConfigureAwait(true);
        return result.Button;
    }

    internal static Task<GlassResult> CoreExAsync(
        IWin32Window owner, GlassDialogConfig config, CancellationToken ct)
    {
        config.Theme ??= _defaultTheme ?? GlassTheme.Default;
        config.UseRoundedCorners ??= _useRoundedCorners;
        return ShowModeless(owner, new GlassDialog(config), ct);
    }

    // Shows a dialog modeless and bridges its close to a Task<GlassResult>. Shared by
    // every non-modal entry point so cancellation and result handling stay identical.
    private static Task<GlassResult> ShowModeless(
        IWin32Window owner, GlassDialog dlg, CancellationToken ct)
    {
        var tcs = new TaskCompletionSource<GlassResult>(
                      TaskCreationOptions.RunContinuationsAsynchronously);

        // Cancellation marshals back onto the UI thread before touching the form,
        // since the token may be cancelled from any thread.
        CancellationTokenRegistration reg = default;
        if (ct.CanBeCanceled)
        {
            reg = ct.Register(() =>
            {
                if (!dlg.IsDisposed && dlg.IsHandleCreated)
                {
                    // BeginInvoke can throw InvalidOperationException or
                    // ObjectDisposedException if the handle is destroyed in the
                    // narrow window between the IsHandleCreated check above and the
                    // post.  Either exception means the dialog is closing anyway, so
                    // swallow both — the FormClosed / Disposed handlers will set the
                    // TCS.
                    try
                    {
                        _ = dlg.BeginInvoke(new System.Action(() =>
                        {
                            if (!dlg.IsDisposed)
                            {
                                dlg.RequestClose(DialogResult.Cancel);
                            }
                        }));
                    }
                    catch (ObjectDisposedException) { }
                    catch (InvalidOperationException) { }
                }
            });
        }

        // Primary completion path: the dialog closes normally.
        dlg.FormClosed += (s, e) =>
        {
            reg.Dispose();
            var button = dlg.DialogResult == DialogResult.None
                ? DialogResult.Cancel
                : dlg.DialogResult;
            _ = tcs.TrySetResult(new GlassResult(button, dlg.CheckBoxChecked, dlg.InputText));
            dlg.Dispose();
        };

        // Safety-net path: if the form is disposed (e.g. Application.Exit, parent
        // form closed) without FormClosed firing, the TCS must still be completed
        // so every await ShowAsync / await Completion returns instead of hanging.
        // TrySetResult is idempotent — the first call wins; the second is a no-op.
        dlg.Disposed += (s, e) =>
        {
            reg.Dispose();
            _ = tcs.TrySetResult(new GlassResult(DialogResult.Cancel, false, string.Empty));
        };

        if (owner != null)
        {
            dlg.Show(owner);
        }
        else
        {
            dlg.Show();
        }

        // If the token was already cancelled, the registration above ran before the
        // handle existed and was skipped — close now so the dialog never gets stuck.
        if (ct.IsCancellationRequested && !dlg.IsDisposed)
        {
            dlg.RequestClose(DialogResult.Cancel);
        }

        return tcs.Task;
    }

    internal static GlassProgressController CoreProgress(IWin32Window owner, GlassDialogConfig config)
    {
        config.Theme ??= _defaultTheme ?? GlassTheme.Default;
        config.UseRoundedCorners ??= _useRoundedCorners;
        var dlg = new GlassDialog(config);
        var task = ShowModeless(owner, dlg, default);
        return new GlassProgressController(dlg, task);
    }
}
