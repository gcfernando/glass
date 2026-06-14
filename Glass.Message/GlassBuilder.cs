// -----------------------------------------------------------------------------
//  Glass.Message — fluent builder for dialogs that go beyond a plain message box.
//  Every method returns the same builder instance so calls can be chained, and
//  Show()/ShowEx() hand the assembled configuration off to GlassMessage.
//
//  File        : GlassBuilder.cs
//  Developer   ::> Gehan Fernando
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Glass;

/// <summary>
/// Chainable builder for composing a dialog from optional parts — title, icon,
/// buttons, animation, input field, checkbox, progress bar, detail panel, and so
/// on. Obtain one from <see cref="GlassMessage.Create"/>.
/// </summary>
public sealed class GlassBuilder
{
    // The message is the one required value, captured at construction; everything
    // else mirrors GlassDialogConfig and starts at the same sensible defaults.
    private readonly string _message;

    private string _title = string.Empty;
    private MessageBoxIcon _icon = MessageBoxIcon.None;
    private Bitmap _customIcon;
    private MessageBoxButtons _buttons = MessageBoxButtons.OK;
    private MessageBoxDefaultButton _defaultButton = MessageBoxDefaultButton.Button1;
    private GlassTheme _theme;
    private IWin32Window _owner;
    private string[] _customLabels;

    private GlassAnimation _animation = GlassAnimation.Fade;

    private int _autoCloseMs;

    private string _checkBoxLabel;
    private bool _checkBoxDefault;

    private GlassInputMode _inputMode = GlassInputMode.None;
    private string _inputPlaceholder;
    private string[] _inputDropdownItems;
    private string _inputDefault;
    private bool _inputShowCapsLockHint = true;

    private string _detailText;

    private bool _showProgress;
    private int _progressValue = -1;
    private int _progressMax = 100;
    private GlassProgressActivity _progressActivity = GlassProgressActivity.None;

    private bool _rightToLeft;

    // Nullable so "never called" stays distinct from an explicit true/false and
    // can fall back to the global GlassMessage.UseRoundedCorners setting.
    private bool? _roundedCorners;

    // Same nullable pattern for the system-sound override (falls back to
    // GlassMessage.PlaySystemSounds when never set).
    private bool? _playSound;

    internal GlassBuilder(string message) => _message = message ?? string.Empty;

    // --- Simple single-value setters -----------------------------------------
    /// <summary>Sets the title-bar caption.</summary>
    public GlassBuilder Title(string title) { _title = title ?? string.Empty; return this; }
    /// <summary>Uses one of the standard system icons.</summary>
    public GlassBuilder Icon(MessageBoxIcon i) { _icon = i; _customIcon = null; return this; }
    /// <summary>Uses a custom bitmap as the dialog icon (e.g. a product logo).</summary>
    public GlassBuilder Icon(Bitmap bitmap) { _customIcon = bitmap; return this; }
    /// <summary>Selects which button is focused (and is the auto-close target) by default.</summary>
    public GlassBuilder Default(MessageBoxDefaultButton d) { _defaultButton = d; return this; }
    /// <summary>Overrides the theme for this dialog only.</summary>
    public GlassBuilder Theme(GlassTheme t) { _theme = t; return this; }
    /// <summary>Sets the owner window so the dialog centres on and stays above it.</summary>
    public GlassBuilder Owner(IWin32Window o) { _owner = o; return this; }
    /// <summary>Chooses the open/close animation.</summary>
    public GlassBuilder Animation(GlassAnimation a) { _animation = a; return this; }

    /// <summary>Uses a standard button set (OK, OKCancel, YesNo, …).</summary>
    public GlassBuilder Buttons(MessageBoxButtons buttons)
    {
        _buttons = buttons;
        _customLabels = null;
        return this;
    }

    /// <summary>
    /// Supplies custom button captions. At most three labels are rendered (mapped to
    /// the Yes/No/Cancel positions). The count picks the layout: 1 → OK, 2 →
    /// OK/Cancel, 3 → Yes/No/Cancel. Labels beyond the third are silently ignored.
    /// </summary>
    public GlassBuilder Buttons(params string[] labels)
    {
        if (labels == null || labels.Length == 0)
        {
            return this;
        }

        _customLabels = labels;
        _buttons = labels.Length switch
        {
            1 => MessageBoxButtons.OK,
            2 => MessageBoxButtons.OKCancel,
            _ => MessageBoxButtons.YesNoCancel,
        };
        return this;
    }

    /// <summary>Enables (or disables) Windows 11 rounded corners for this dialog.</summary>
    public GlassBuilder RoundedCorners(bool enable = true)
    {
        _roundedCorners = enable;
        return this;
    }

    /// <summary>Plays (or suppresses) the Windows system sound matching the icon when the dialog opens.</summary>
    public GlassBuilder Sound(bool enable = true)
    {
        _playSound = enable;
        return this;
    }

    /// <summary>Auto-confirms the default button after the given delay (clamped to ≥ 0).</summary>
    public GlassBuilder AutoClose(int milliseconds) { _autoCloseMs = Math.Max(0, milliseconds); return this; }

    /// <summary>Adds a checkbox (e.g. "Don't show this again") below the message.</summary>
    public GlassBuilder CheckBox(string label, bool defaultChecked = false)
    {
        _checkBoxLabel = label;
        _checkBoxDefault = defaultChecked;
        return this;
    }

    /// <summary>Adds a single-line text input.</summary>
    public GlassBuilder InputText(string placeholder = "", string defaultValue = "")
    {
        _inputMode = GlassInputMode.Text;
        _inputPlaceholder = placeholder;
        _inputDefault = defaultValue;
        return this;
    }

    /// <summary>
    /// Adds a masked password input with a reveal-eye toggle. When
    /// <paramref name="showCapsLockHint"/> is <c>true</c> (the default), a small
    /// "Caps Lock is on" badge appears beneath the field while Caps Lock is active
    /// and the field is focused. Set it to <c>false</c> to suppress the badge.
    /// </summary>
    public GlassBuilder InputPassword(string placeholder = "", bool showCapsLockHint = true)
    {
        _inputMode = GlassInputMode.Password;
        _inputPlaceholder = placeholder;
        _inputShowCapsLockHint = showCapsLockHint;
        return this;
    }

    /// <summary>Adds a multi-line text input.</summary>
    public GlassBuilder InputMultiline(string placeholder = "", string defaultValue = "")
    {
        _inputMode = GlassInputMode.Multiline;
        _inputPlaceholder = placeholder;
        _inputDefault = defaultValue;
        return this;
    }

    /// <summary>Adds a drop-down list. The optional <paramref name="defaultItem"/> is pre-selected.</summary>
    public GlassBuilder InputDropdown(IEnumerable<string> items, string defaultItem = null)
    {
        _inputMode = GlassInputMode.Dropdown;
        _inputDropdownItems = items != null ? [.. items] : [];
        _inputDefault = defaultItem;
        return this;
    }

    /// <summary>Adds an expandable "Show details" panel (handy for stack traces and diagnostics).</summary>
    public GlassBuilder Detail(string detail) { _detailText = detail; return this; }

    /// <summary>Adds an indeterminate (marquee) progress bar.</summary>
    public GlassBuilder ProgressIndeterminate()
    {
        _showProgress = true;
        _progressValue = -1;
        return this;
    }

    /// <summary>Adds a determinate progress bar at <paramref name="value"/> of <paramref name="max"/>.</summary>
    public GlassBuilder Progress(int value, int max = 100)
    {
        _showProgress = true;
        _progressValue = value;
        _progressMax = max;
        return this;
    }

    /// <summary>
    /// Selects the directional flow animation painted over the progress bar so it
    /// matches the operation in progress — e.g. <see cref="GlassProgressActivity.Upload"/>,
    /// <see cref="GlassProgressActivity.Download"/>, or <see cref="GlassProgressActivity.Sync"/>.
    /// Layers on top of either a determinate (<see cref="Progress(int, int)"/>) or
    /// indeterminate (<see cref="ProgressIndeterminate"/>) bar; the value is unaffected.
    /// </summary>
    public GlassBuilder ProgressActivity(GlassProgressActivity activity)
    {
        _showProgress = true;
        _progressActivity = activity;
        return this;
    }

    /// <summary>Mirrors the layout for right-to-left languages.</summary>
    public GlassBuilder RightToLeft(bool enable = true) { _rightToLeft = enable; return this; }

    /// <summary>Shows the dialog modally and returns just the button that was pressed.</summary>
    public DialogResult Show() => GlassMessage.CoreEx(_owner, BuildConfig()).Button;

    /// <summary>Shows the dialog modally and returns the full <see cref="GlassResult"/> (button + checkbox + input).</summary>
    public GlassResult ShowEx() => GlassMessage.CoreEx(_owner, BuildConfig());

    /// <summary>
    /// Shows the dialog without blocking the UI thread and returns just the button
    /// that was pressed. The optional <paramref name="cancellationToken"/> closes the
    /// dialog (yielding <see cref="DialogResult.Cancel"/>) if cancelled while open.
    /// </summary>
    public async Task<DialogResult> ShowAsync(CancellationToken cancellationToken = default)
        => (await GlassMessage.CoreExAsync(_owner, BuildConfig(), cancellationToken).ConfigureAwait(true)).Button;

    /// <summary>
    /// Shows the dialog without blocking the UI thread and returns the full
    /// <see cref="GlassResult"/> (button + checkbox + input) once it closes.
    /// </summary>
    public Task<GlassResult> ShowExAsync(CancellationToken cancellationToken = default)
        => GlassMessage.CoreExAsync(_owner, BuildConfig(), cancellationToken);

    /// <summary>
    /// Shows a non-blocking progress dialog and returns a
    /// <see cref="GlassProgressController"/> to update and close it while work runs.
    /// Use with <see cref="Progress(int, int)"/> for a determinate bar (the controller
    /// updates the value) or <see cref="ProgressIndeterminate"/> for a marquee.
    /// </summary>
    public GlassProgressController ShowProgress()
    {
        if (!_showProgress)
        {
            // Default to a determinate bar starting at 0 so the controller has
            // something to drive even if the caller forgot to call Progress().
            _showProgress = true;
            _progressValue = 0;
        }

        return GlassMessage.CoreProgress(_owner, BuildConfig());
    }

    // Snapshots the accumulated fields into the config the dialog consumes.
    private GlassDialogConfig BuildConfig() => new()
    {
        Message = _message,
        Title = _title,
        Icon = _icon,
        CustomIcon = _customIcon,
        Buttons = _buttons,
        DefaultButton = _defaultButton,
        Theme = _theme ?? GlassMessage.DefaultTheme ?? GlassTheme.Default,
        CustomLabels = _customLabels,
        Animation = _animation,
        AutoCloseMs = _autoCloseMs,
        CheckBoxLabel = _checkBoxLabel,
        CheckBoxDefault = _checkBoxDefault,
        InputMode = _inputMode,
        InputPlaceholder = _inputPlaceholder,
        InputDropdownItems = _inputDropdownItems,
        InputDefault = _inputDefault,
        InputShowCapsLockHint = _inputShowCapsLockHint,
        DetailText = _detailText,
        ShowProgress = _showProgress,
        ProgressValue = _progressValue,
        ProgressMax = _progressMax,
        ProgressActivity = _progressActivity,
        RightToLeft = _rightToLeft,
        UseRoundedCorners = _roundedCorners,
        PlaySound = _playSound,
    };
}
