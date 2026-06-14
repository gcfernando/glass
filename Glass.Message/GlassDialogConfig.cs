// -----------------------------------------------------------------------------
//  Glass.Message — the immutable-by-convention bag of settings that describes a
//  single dialog. Built by GlassBuilder (or GlassMessage) and consumed by
//  GlassDialog when it lays itself out.
//
//  File        : GlassDialogConfig.cs
//  Developer   ::> Gehan Fernando
// -----------------------------------------------------------------------------

using System.Drawing;
using System.Windows.Forms;

namespace Glass;

/// <summary>
/// Plain data object carrying every option a <see cref="GlassDialog"/> needs.
/// Keeping this separate from the form keeps the public API (builder + facade)
/// decoupled from the WinForms implementation.
/// </summary>
internal sealed class GlassDialogConfig
{
    // --- Core content ---------------------------------------------------------
    internal string Message { get; set; } = string.Empty;
    internal string Title { get; set; } = string.Empty;
    internal MessageBoxIcon Icon { get; set; } = MessageBoxIcon.None;
    internal Bitmap CustomIcon { get; set; }
    internal MessageBoxButtons Buttons { get; set; } = MessageBoxButtons.OK;
    internal MessageBoxDefaultButton DefaultButton { get; set; } = MessageBoxDefaultButton.Button1;
    internal GlassTheme Theme { get; set; }

    /// <summary>Optional per-button captions that override the default OK/Cancel/etc. text.</summary>
    internal string[] CustomLabels { get; set; }

    /// <summary>Entrance/exit animation. Defaults to a gentle fade.</summary>
    internal GlassAnimation Animation { get; set; } = GlassAnimation.Fade;

    /// <summary>When greater than zero, the dialog auto-confirms after this many milliseconds.</summary>
    internal int AutoCloseMs { get; set; }

    // --- Optional checkbox ----------------------------------------------------
    internal string CheckBoxLabel { get; set; }
    internal bool CheckBoxDefault { get; set; }

    // --- Optional inline input ------------------------------------------------
    internal GlassInputMode InputMode { get; set; } = GlassInputMode.None;
    internal string InputPlaceholder { get; set; }
    internal string[] InputDropdownItems { get; set; }
    internal string InputDefault { get; set; }
    /// <summary>
    /// When <c>true</c> (the default), the password field shows a "Caps Lock is on"
    /// badge while Caps Lock is active. Set to <c>false</c> to suppress the badge.
    /// </summary>
    internal bool InputShowCapsLockHint { get; set; } = true;

    /// <summary>Text revealed by the expandable "Show details" section (e.g. a stack trace).</summary>
    internal string DetailText { get; set; }

    // --- Optional progress bar ------------------------------------------------
    internal bool ShowProgress { get; set; }

    /// <summary>Current value, or -1 for an indeterminate (marquee) bar.</summary>
    internal int ProgressValue { get; set; } = -1;
    internal int ProgressMax { get; set; } = 100;

    /// <summary>
    /// The real-world operation the bar represents, which selects the directional
    /// flow animation painted over the bar. Defaults to <see cref="GlassProgressActivity.None"/>
    /// (a plain bar).
    /// </summary>
    internal GlassProgressActivity ProgressActivity { get; set; } = GlassProgressActivity.None;

    internal bool RightToLeft { get; set; }

    /// <summary>
    /// Per-dialog rounded-corner override. <c>null</c> means "fall back to the
    /// global <see cref="GlassMessage.UseRoundedCorners"/> setting".
    /// </summary>
    internal bool? UseRoundedCorners { get; set; }

    /// <summary>
    /// Per-dialog system-sound override. <c>null</c> means "fall back to the global
    /// <see cref="GlassMessage.PlaySystemSounds"/> setting". When enabled, the
    /// dialog plays the Windows sound that matches its <see cref="Icon"/> as it opens.
    /// </summary>
    internal bool? PlaySound { get; set; }

    // --- Convenience flags used by the layout pass ----------------------------
    internal bool HasCheckBox => CheckBoxLabel != null;
    internal bool HasInput => InputMode != GlassInputMode.None;
    internal bool HasDetail => DetailText != null;
    internal bool HasProgress => ShowProgress;
}
