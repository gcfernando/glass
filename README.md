<div align="center">

# Glass.Message

### A next-level, modern replacement for the Windows `MessageBox`

**Mica / Acrylic backdrops · automatic dark & light theming · fluent builder · async dialogs · inline inputs · progress bars · expandable details · countdown auto-close · toast notifications · full RTL support**

[![Platform](https://img.shields.io/badge/platform-Windows-blue)](#requirements)
[![.NET](https://img.shields.io/badge/.NET-4.8.1%20%7C%208%20%7C%209%20%7C%2010-512BD4)](#requirements)
[![UI](https://img.shields.io/badge/UI-WinForms-5C2D91)](#requirements)
[![License](https://img.shields.io/badge/license-MIT-green)](#license)
[![Version](https://img.shields.io/badge/version-1.0.0-informational)](#)

</div>

---

`Glass.Message` is a drop-in, modern dialog library for Windows desktop apps. If your code already calls `MessageBox.Show(...)`, you can switch to `GlassMessage.Show(...)` and instantly get a beautiful, themed, DPI-aware dialog — **without changing a single argument**. When you need more, a fluent builder unlocks input fields, checkboxes, progress bars, expandable detail panels, countdown auto-close, custom icons, animations, and non-blocking `async` dialogs. A separate `GlassToast` API delivers stacking, auto-dismissing toast notifications.

> **Why developers love it**
> - **Zero-friction migration** — signature-compatible with `System.Windows.Forms.MessageBox`.
> - **Looks native and modern** — Windows 11 rounded corners, Mica/Acrylic, and a glossy themed UI.
> - **Follows the OS** — automatically matches the user's light / dark / high-contrast preference.
> - **Far more than a message** — inputs, checkboxes, progress, details, toasts, and async — all in one tiny library.

---

## Table of Contents

- [Preview](#preview)
- [Features](#features)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Complete Usage Guide](#complete-usage-guide)
- [Demo Walkthrough](#demo-walkthrough)
- [API Overview](#api-overview)
- [Configuration](#configuration)
- [Styling and Customization](#styling-and-customization)
- [Error Handling / Best Practices](#error-handling--best-practices)
- [Project Structure](#project-structure)
- [Requirements](#requirements)
- [Running the Demo](#running-the-demo)
- [FAQ](#faq)
- [Contributing](#contributing)
- [License](#license)
- [Full Example](#full-example)

---

## Preview

The bundled **Glass.Demo** app is a feature gallery — every button maps to one capability of the library.

![Feature Gallery](Images/Feature%20Galary.png)

> **Tip:** Run `Glass.Demo` (see [Running the Demo](#running-the-demo)) to click through every screen shown below live.

Here are the highlights — each is fully documented with code in the [Complete Usage Guide](#complete-usage-guide).

### Drop-in replacement for `MessageBox`

![Drop-in Replace](Images/Drop-in%20Replace%20%28Show%29.png)

A single static call, identical in shape to `MessageBox.Show`, but rendered with the modern Glass chrome.

```csharp
GlassMessage.Show(
    "The selected printer 'HP LaserJet Pro M404dn' is offline.\n" +
    "Check that it is powered on and connected to the network, then try again.",
    "Printer Offline",
    MessageBoxIcon.Warning);
```

**Use it when:** you simply want a better-looking message box with no behavioural change.

### Fluent Builder API

![Fluent Builder API](Images/Fluent%20Builder%20API.png)

For anything beyond a plain message + buttons, start a chainable builder with `GlassMessage.Create(...)`.

```csharp
GlassMessage.Create(
        "Annual_Report_Q4_2025.xlsx has unsaved changes.\n\n" +
        "Closing now will discard every edit made since the last save.")
    .Title("Unsaved Changes")
    .Icon(MessageBoxIcon.Warning)
    .Buttons("Save", "Don't Save", "Cancel")
    .Default(MessageBoxDefaultButton.Button1)
    .Show();
```

**Use it when:** you need custom button labels, inputs, checkboxes, progress, details, animations, or a rich result.

### Themes — Dark, Light, Mica, High Contrast, Classic

| Dark | Light | Mica |
|:---:|:---:|:---:|
| ![Dark](Images/Dark-Light-Mica-HC-Classic-Themes_Dark.png) | ![Light](Images/Dark-Light-Mica-HC-Classic-Themes_Light.png) | ![Mica](Images/Dark-Light-Mica-HC-Classic-Themes_Mica.png) |

| High Contrast | Windows Classic |
|:---:|:---:|
| ![High Contrast](Images/Dark-Light-Mica-HC-Classic-Themes_HC.png) | ![Classic](Images/Dark-Light-Mica-HC-Classic-Themes_Classic.png) |

```csharp
GlassMessage.Create("Night mode active.")
    .Title("Display Profile — Dark")
    .Icon(MessageBoxIcon.Information)
    .Theme(GlassTheme.Dark)      // or .Light / .Mica / .HighContrast / .WindowsClassic
    .Show();
```

**Use it when:** you want a specific look, or to match your app's branding. See [Styling and Customization](#styling-and-customization).

### Toast notifications

![Toast — Bottom Right](Images/Toast%20%E2%80%94%20Bottom-Right.png)

Lightweight, non-modal, auto-dismissing notifications that fade in at a screen corner and stack neatly.

```csharp
GlassToast.Show(new GlassToastOptions
{
    Message  = "Invoice_March_2026.pdf saved to SharePoint · Finance",
    Title    = "Upload Complete",
    Icon     = MessageBoxIcon.Information,
    DurationMs = 4_000,
    Position = ToastPosition.BottomRight,
});
```

**Use it when:** you want to inform the user without interrupting their workflow.

---

## Features

Every feature below is backed by real code in `Glass.Message` and demonstrated in `Glass.Demo`.

| Category | Capability |
|---|---|
| **Drop-in API** | `GlassMessage.Show(...)` overloads that mirror `MessageBox.Show` exactly |
| **Fluent builder** | `GlassMessage.Create(...)` for composable dialogs |
| **Async** | `GlassMessage.ShowAsync(...)` — non-blocking, awaitable, cancellable |
| **Rich result** | `ShowEx()` returns the button **plus** checkbox state **plus** typed input |
| **Theming** | `Dark`, `Light`, `Mica`, `HighContrast`, `WindowsClassic` presets + custom themes |
| **Auto theme** | `GlassTheme.AutoDetect()` follows the Windows light/dark/high-contrast setting |
| **Modern chrome** | Windows 11 rounded corners (DWM), Mica backdrop, Acrylic blur fallback |
| **Inputs** | Single-line text, password (with reveal eye + Caps Lock warning), multi-line, drop-down |
| **Checkbox** | "Don't show again"-style opt-in below the message |
| **Detail panel** | Expandable "Show details" section for stack traces / diagnostics |
| **Progress** | Determinate and indeterminate (marquee) progress bars |
| **Countdown** | Auto-confirm the default button after a delay, with a live circular countdown |
| **Custom icon** | Use any 48×48 `Bitmap` (e.g. a product logo) as the dialog icon |
| **Animations** | `Fade` (default), `SlideDown`, `Scale`, or `None` |
| **Keyboard** | `Ctrl+C` copies title + message; `Enter`/`Esc` map to sensible results |
| **RTL** | Full right-to-left mirrored layout |
| **Toasts** | Six anchor positions, auto-stacking, click actions, async variant |
| **Cross-target** | .NET Framework 4.8.1 and .NET 8 / 9 / 10 (Windows) |

---

## Installation

`Glass.Message` is a standard .NET class library. There are two ways to use it.

### Option A — Project reference (used by the demo today)

The repository wires `Glass.Demo` to the library through a project reference:

```xml
<ItemGroup>
  <ProjectReference Include="..\Glass.Message\Glass.Message.csproj" />
</ItemGroup>
```

Add the same reference from your own WinForms project, or via the CLI:

```bash
dotnet add reference ..\Glass.Message\Glass.Message.csproj
```

### Option B — NuGet package

The library is configured with full NuGet package metadata (`PackageId = Glass.Message`, MIT-licensed). Once published to a feed, install it with:

```bash
dotnet add package Glass.Message
```

```powershell
# Package Manager Console
Install-Package Glass.Message
```

> **Note:** Whichever option you choose, add a single `using Glass;` and you're ready.

---

## Quick Start

```csharp
using Glass;
using System.Windows.Forms;

// 1) (Optional) Set global defaults once at startup — e.g. in Program.Main():
GlassMessage.UseRoundedCorners = true;                 // Windows 11 rounded corners
GlassMessage.DefaultTheme      = GlassTheme.AutoDetect(); // follow the OS theme

// 2) Show a basic, MessageBox-style dialog:
DialogResult result = GlassMessage.Show(
    "Your changes have been saved.",
    "Success",
    MessageBoxIcon.Information);

// 3) React to the result exactly like a classic MessageBox:
if (result == DialogResult.OK)
{
    // ...
}
```

**Expected behaviour:** a modern, themed dialog fades in, centred on the primary work area, with a glossy OK button and an information icon. Pressing **Enter**, clicking **OK**, or pressing **Esc** all return `DialogResult.OK` for a single-button dialog.

> **Tip:** `GlassMessage.Show` is signature-compatible with `MessageBox.Show`. In most codebases you can replace `MessageBox` with `GlassMessage` and everything still compiles.

---

## Complete Usage Guide

A step-by-step tour of every capability, mirroring the demos in `Glass.Demo`.

### 1. Drop-in replacement — `Show`

![Drop-in Replace](Images/Drop-in%20Replace%20%28Show%29.png)

The simplest call. All the classic `MessageBox` overloads exist.

```csharp
GlassMessage.Show(
    "The selected printer 'HP LaserJet Pro M404dn' is offline.\n" +
    "Check that it is powered on and connected to the network, then try again.",
    "Printer Offline",
    MessageBoxIcon.Warning);
```

**Key parameters:** `message`, `title`, `icon` (`MessageBoxIcon`), `buttons` (`MessageBoxButtons`), `defaultButton`, and an optional `owner`/`theme`.
**Best practice:** keep using the returned `DialogResult` exactly as before.

---

### 2. Theme presets — `Theme(...)`

The library ships five ready-made palettes.

| | |
|:---:|:---:|
| ![Dark](Images/Dark-Light-Mica-HC-Classic-Themes_Dark.png) | ![Light](Images/Dark-Light-Mica-HC-Classic-Themes_Light.png) |
| ![Mica](Images/Dark-Light-Mica-HC-Classic-Themes_Mica.png) | ![High Contrast](Images/Dark-Light-Mica-HC-Classic-Themes_HC.png) |

![Windows Classic](Images/Dark-Light-Mica-HC-Classic-Themes_Classic.png)

```csharp
GlassMessage.Create("Mica backdrop applied.")
    .Title("Mica Backdrop Active")
    .Icon(MessageBoxIcon.Information)
    .Theme(GlassTheme.Mica)
    .Show();
```

**Available presets:** `GlassTheme.Dark`, `GlassTheme.Light`, `GlassTheme.Mica`, `GlassTheme.HighContrast`, `GlassTheme.WindowsClassic`.
**Best practice:** set `GlassMessage.DefaultTheme` once instead of repeating `.Theme(...)` everywhere.

---

### 3. Auto-detect the OS theme — `GlassTheme.AutoDetect()`

![Auto-detect OS Theme](Images/Auto-detect%20OS%20Theme.png)

Reads the user's Windows colour preference and returns the matching preset (high-contrast if enabled, otherwise dark or light).

```csharp
var theme    = GlassTheme.AutoDetect();
var modeName = GlassTheme.IsSystemDark() ? "Dark" : "Light";

GlassMessage.Create(
        $"Windows is currently in {modeName} mode, so Glass.Message selected its " +
        $"{modeName.ToLower()} palette automatically.")
    .Title("System Theme Detected")
    .Icon(MessageBoxIcon.Information)
    .Theme(theme)
    .Show();
```

**Helpers:** `GlassTheme.AutoDetect()` (returns a preset) and `GlassTheme.IsSystemDark()` (returns `bool`).
**Best practice:** call `GlassMessage.DefaultTheme = GlassTheme.AutoDetect();` at startup so every dialog follows the OS.

---

### 4. Fluent Builder — `Create(...)`

![Fluent Builder API](Images/Fluent%20Builder%20API.png)

The builder composes a dialog from optional parts. Every method returns the same builder, so calls chain.

```csharp
GlassMessage.Create(
        "Annual_Report_Q4_2025.xlsx has unsaved changes.\n\n" +
        "Closing now will discard every edit made since the last save.")
    .Title("Unsaved Changes")
    .Icon(MessageBoxIcon.Warning)
    .Buttons("Save", "Don't Save", "Cancel")
    .Default(MessageBoxDefaultButton.Button1)
    .Show();
```

**Custom button mapping:** the number of labels selects the closest standard layout, so each click still maps to a meaningful `DialogResult`:

| Labels | Layout | Returned results |
|---|---|---|
| 1 | OK | `OK` |
| 2 | OK / Cancel | `OK`, `Cancel` |
| 3+ | Yes / No / Cancel | `Yes`, `No`, `Cancel` |

> **Note:** With three custom labels, button #2 returns `DialogResult.No` — see the [Security Alert](#16-rich-result--showex) example.

---

### 5. Countdown auto-close — `AutoClose(...)`

![Countdown Auto-Close](Images/Countdown%20Auto-Close%20%20%2810%20s%29.png)

Auto-confirms the **default** button after a delay. A live circular countdown and a `(Ns)` suffix tick down each second.

```csharp
GlassMessage.Create(
        "Your Contoso Portal session is about to expire.\n\n" +
        "Choose 'Stay Signed In' to extend your session by 30 minutes.")
    .Title("Session Expiring — Contoso Portal")
    .Icon(MessageBoxIcon.Warning)
    .Buttons("Stay Signed In", "Sign Out Now")
    .AutoClose(10_000)                       // 10 seconds
    .Animation(GlassAnimation.SlideDown)
    .Show();
```

**Key parameters:** `AutoClose(milliseconds)` (clamped to ≥ 0); the target is whichever button you set via `Default(...)`.
**Best practice:** pair with `Default(...)` so the auto-confirmed action is the safe one.

---

### 6. "Don't show again" checkbox — `CheckBox(...)` + `ShowEx()`

![Don't Show Again](Images/Don't%20Show%20Again.png)

Adds a themed checkbox below the message. Read its state from the rich result.

```csharp
var r = GlassMessage.Create(
        "Drive C: has only 4.2 GB of free space remaining (of 512 GB total).")
    .Title("Low Disk Space — C:\\")
    .Icon(MessageBoxIcon.Warning)
    .Buttons("Open Disk Cleanup", "Dismiss")
    .CheckBox("Don't warn me again for drive C:\\")
    .ShowEx();

if (r.CheckBoxChecked)
{
    // persist the user's preference...
}
```

**Key parameters:** `CheckBox(label, defaultChecked = false)`. Use `ShowEx()` (not `Show()`) to read `GlassResult.CheckBoxChecked`.

---

### 7. Inline text input — `InputText(...)`

![Inline Text Input](Images/Inline%20Text%20Input.png)

Prompt for a value and a button in one dialog.

```csharp
var r = GlassMessage.Create("Enter a new name for the selected folder.")
    .Title("Rename Folder")
    .Icon(MessageBoxIcon.Question)
    .InputText("Folder name", "Client Projects — Q1 2026")  // placeholder, default
    .Buttons("Rename", "Cancel")
    .ShowEx();

if (r.Button == DialogResult.OK && !string.IsNullOrWhiteSpace(r.InputText))
{
    GlassMessage.Show($"Renamed to \"{r.InputText}\" successfully.",
                      "Rename Complete", MessageBoxIcon.Information);
}
```

**Key parameters:** `InputText(placeholder = "", defaultValue = "")`. Read the typed value from `GlassResult.InputText`.
**Best practice:** validate `r.InputText` before using it (e.g. `string.IsNullOrWhiteSpace`).

---

### 8. Password input — `InputPassword(...)`

![Password Input](Images/Password%20Input.png)

A masked field with a **reveal eye** toggle and an automatic **Caps Lock** warning.

```csharp
var r = GlassMessage.Create("Authentication is required to connect to Contoso ERP.")
    .Title("Sign In — Contoso ERP")
    .Icon(MessageBoxIcon.Warning)
    .InputPassword("Active Directory password")
    .Buttons("Connect", "Cancel")
    .ShowEx();

if (r.Button == DialogResult.OK)
{
    // r.InputText holds the entered password
}
```

**Key parameters:** `InputPassword(placeholder = "")`.
**Best practice:** treat `r.InputText` as a secret — avoid logging it; clear it as soon as you're done.

---

### 9. Drop-down input — `InputDropdown(...)`

![Drop-down Input](Images/Drop-down%20Input.png)

A read-only list the user picks from.

```csharp
var r = GlassMessage.Create("Choose the output format for 'Annual_Report_Q4_2025'.")
    .Title("Export Document")
    .Icon(MessageBoxIcon.Question)
    .InputDropdown(
        ["PDF — Portable Document Format",
         "Word (.docx) — Microsoft Word",
         "Excel (.xlsx) — Microsoft Excel",
         "CSV — Comma-Separated Values"],
        "PDF — Portable Document Format")   // pre-selected item
    .Buttons("Export", "Cancel")
    .ShowEx();

if (r.Button == DialogResult.OK)
{
    // r.InputText is the chosen item's text
}
```

**Key parameters:** `InputDropdown(IEnumerable<string> items, string defaultItem = null)`.

> There is also `InputMultiline(placeholder, defaultValue)` for a multi-line text box with a vertical scroll bar.

---

### 10. Expandable detail section — `Detail(...)`

![Expandable Detail Section](Images/Expandable%20Detail%20Section.png)

Hides verbose diagnostics behind a **"Show details ▼"** link — perfect for stack traces and correlation IDs.

```csharp
GlassMessage.Create("OneDrive failed to sync 'Annual_Report_Q4_2025.xlsx'.")
    .Title("Sync Error — OneDrive")
    .Icon(MessageBoxIcon.Error)
    .Detail(
        "System.IO.IOException: The process cannot access the file ...\n" +
        "   at System.IO.FileStream.ValidateFileHandle(SafeFileHandle handle)\n" +
        "Win32 error:     0x80070020 — ERROR_SHARING_VIOLATION\n" +
        "Correlation ID:  a3f8b2e1-4d9c-47f2-8b1a-c6e5d0f92341")
    .Buttons(MessageBoxButtons.RetryCancel)
    .Show();
```

**Key parameters:** `Detail(detailText)`. Toggling the panel resizes the dialog and re-centres it.
**Best practice:** put user-facing guidance in the message, and raw diagnostics in `Detail(...)`.

---

### 11. Determinate progress bar — `Progress(value, max)`

![Determinate Progress Bar](Images/Determinate%20Progress%20Bar.png)

```csharp
GlassMessage.Create("Backing up 'Documents' to OneDrive…")
    .Title("OneDrive Backup in Progress")
    .Icon(MessageBoxIcon.Information)
    .Progress(75, 100)              // 75%
    .Buttons("Cancel Backup")
    .Show();
```

**Key parameters:** `Progress(value, max = 100)`.

---

### 12. Indeterminate progress bar — `ProgressIndeterminate()`

![Indeterminate Progress Bar](Images/Indeterminate%20Progress%20Bar.png)

A marquee bar for work of unknown duration.

```csharp
GlassMessage.Create("Verifying your Microsoft 365 licence with activation servers…")
    .Title("Activating Microsoft 365")
    .Icon(MessageBoxIcon.Information)
    .ProgressIndeterminate()
    .Buttons("Cancel")
    .Show();
```

**Best practice:** use the indeterminate bar when you can't compute a percentage; switch to `Progress(...)` once you can.

---

### 13. Custom bitmap icon — `Icon(Bitmap)`

![Custom Bitmap Icon](Images/Custom%20Bitmap%20Icon.png)

Use any 48×48 bitmap (e.g. a product logo) instead of a system icon.

```csharp
using var bmp = new Bitmap(48, 48);
using (var g = Graphics.FromImage(bmp))
{
    g.SmoothingMode = SmoothingMode.AntiAlias;
    g.FillEllipse(Brushes.DodgerBlue, 2, 2, 44, 44);
    g.DrawString("G", new Font("Segoe UI", 26f, FontStyle.Bold),
        Brushes.White, new PointF(10, 6));
}

GlassMessage.Create("Glass.Message v1.0 is active and your custom branding has been applied.")
    .Title("Glass.Message — Ready")
    .Icon(bmp)                       // Bitmap overload
    .Buttons(MessageBoxButtons.OK)
    .Show();
```

**Key parameters:** `Icon(Bitmap bitmap)` (there's also `Icon(MessageBoxIcon)` for the standard system icons).
**Best practice:** dispose the bitmap with `using` after the dialog returns.

---

### 14. Copy to clipboard — `Ctrl+C`

![Ctrl+C Copy to Clipboard](Images/Ctrl+C%20%20Copy%20to%20Clipboard.png)

Just like the classic message box, pressing **Ctrl+C** while the dialog is focused copies the title and message text to the clipboard — handy for support tickets. No code required.

```csharp
GlassMessage.Create("Connection to the database server failed.\n\n" +
        "Press Ctrl+C to copy this message to the clipboard for the support desk.")
    .Title("Database Connection Failed")
    .Icon(MessageBoxIcon.Error)
    .Buttons(MessageBoxButtons.RetryCancel)
    .Show();
```

> **Tip:** Also built in — **Esc** closes with the most sensible cancel-style result for the current button set, and **Enter** activates the default button.

---

### 15. Animations — `Animation(...)`

| SlideDown | Scale |
|:---:|:---:|
| ![SlideDown Animation](Images/SlideDown%20Animation.png) | ![Scale Animation](Images/Scale%20Animation.png) |

```csharp
// Slide down into place while fading in
GlassMessage.Create("Glass.Message 1.0 is available.")
    .Title("Update Available")
    .Icon(MessageBoxIcon.Information)
    .Buttons("Install Now", "Later")
    .Animation(GlassAnimation.SlideDown)
    .Show();

// Grow from 90% to full size
GlassMessage.Create("This dialog used the Scale animation.")
    .Title("Scale Animation")
    .Animation(GlassAnimation.Scale)
    .Buttons("Nice", "Again")
    .Show();
```

**Options:** `GlassAnimation.Fade` (default), `SlideDown`, `Scale`, `None`.
**Best practice:** use `GlassAnimation.None` in automated UI tests for deterministic timing.

---

### 16. Rich result — `ShowEx()`

![All Buttons + ShowEx Rich Result](Images/All%20Buttons%20+%20ShowEx%20Rich%20Result.png)

`ShowEx()` returns a `GlassResult` carrying the **button**, the **checkbox state**, and any **typed input** at once.

```csharp
var r = GlassMessage.Create(
        "Adobe Acrobat Reader DC (v24.3.21, 698 MB) will be permanently removed.")
    .Title("Uninstall Adobe Acrobat Reader DC")
    .Icon(MessageBoxIcon.Warning)
    .Buttons("Uninstall", "Repair", "Cancel")
    .CheckBox("Also remove personal settings and reading history")
    .ShowEx();

if (r.Button != DialogResult.Cancel)   // user chose Uninstall or Repair
{
    bool alsoRemovePrefs = r.CheckBoxChecked;
    // ...
}
```

> **Note on button mapping:** three custom labels (`Uninstall`, `Repair`, `Cancel`) use the **Yes / No / Cancel** layout, so they return `DialogResult.Yes`, `DialogResult.No`, and `DialogResult.Cancel` respectively. Always confirm which `DialogResult` your labels map to (see the table in [section 4](#4-fluent-builder--create)).

The **Security Alert** demo shows the same idea — three buttons where the second (`Secure account`) returns `DialogResult.No`:

![Security Alert — Sign-in](Images/Security%20Alert%20%E2%80%94%20Sign-in.png)

```csharp
var r = GlassMessage.Create("We noticed a new sign-in to your Contoso account.")
    .Title("New Sign-in to Your Account")
    .Icon(MessageBoxIcon.Warning)
    .Buttons("This was me", "Secure account", "Review activity")
    .Default(MessageBoxDefaultButton.Button1)
    .ShowEx();

if (r.Button == DialogResult.No)   // "Secure account"
{
    GlassMessage.Show("All other sessions have been signed out…",
                      "Account Secured", MessageBoxIcon.Information);
}
```

---

### 17. Async, non-blocking dialogs — `ShowAsync(...)`

![Async ShowAsync](Images/Async%20ShowAsync.png)

Shows the dialog without blocking the UI thread; `await` the chosen button. Pass a `CancellationToken` to close it programmatically (which yields `DialogResult.Cancel`).

```csharp
private static async void SyncWorkspace()
{
    var r = await GlassMessage.ShowAsync(
        "Your local workspace has uncommitted changes and is out of sync.",
        "Sync Workspace — Contoso DevOps",
        MessageBoxIcon.Question,
        MessageBoxButtons.OKCancel);

    GlassMessage.Show(
        r == DialogResult.OK ? "Push started." : "Push skipped.",
        r == DialogResult.OK ? "Pushing to origin/main…" : "Sync Deferred",
        r == DialogResult.OK ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
}
```

**Signature:** `Task<DialogResult> ShowAsync(string message, string title = "", MessageBoxIcon icon = None, MessageBoxButtons buttons = OK, CancellationToken cancellationToken = default)` (plus a theme-aware overload).
**Best practice:** prefer `ShowAsync` from `async` event handlers so the message pump keeps running.

---

### 18. Right-to-left layout — `RightToLeft()`

![RTL Layout](Images/RTL%20%28Right-to-Left%29%20Layout.png)

Mirrors the entire layout for RTL languages such as Arabic and Hebrew.

```csharp
GlassMessage.Create("فشل حفظ الملف: تقرير_الربع_الرابع.docx")
    .Title("فشل الحفظ — القرص ممتلئ")
    .Icon(MessageBoxIcon.Error)
    .Buttons(MessageBoxButtons.RetryCancel)
    .RightToLeft()
    .Show();
```

**Key parameters:** `RightToLeft(enable = true)`.

---

### 19. Real-world compositions

The demo combines features into product-style dialogs:

| Windows Update — Release Notes | Storage Migration Wizard |
|:---:|:---:|
| ![Release Notes](Images/Windows%20Update%20%E2%80%94%20Release%20Notes.png) | ![Storage Migration Wizard](Images/Storage%20Migration%20Wizard.png) |

The **End-User Licence Agreement** dialog pairs a `Detail(...)` panel with a required `CheckBox(...)`:

![End-User Licence Agreement](Images/End-User%20Licence%20Agreement.png)

```csharp
GlassMessage.Create(
        "Please review and accept the licence terms to continue installing Contoso Suite 2026.")
    .Title("End-User Licence Agreement")
    .Icon(MessageBoxIcon.Information)
    .Detail("CONTOSO SUITE 2026 — END-USER LICENCE AGREEMENT\n...")
    .CheckBox("I have read and accept the licence terms")
    .Buttons("Accept and install", "Decline")
    .Show();
```

The **Storage Migration Wizard** combines a drop-down, a checkbox, and a follow-up progress dialog in one flow:

```csharp
var r = GlassMessage.Create("The Storage Migration Wizard will move your user profile…")
    .Title("Storage Migration Wizard")
    .Icon(MessageBoxIcon.Question)
    .InputDropdown(
        ["Move profile and verify (recommended)",
         "Move profile without verification (faster)",
         "Copy only — keep originals in place"],
        "Move profile and verify (recommended)")
    .CheckBox("Restart automatically when the migration finishes")
    .Buttons("Start migration", "Cancel")
    .ShowEx();

if (r.Button == DialogResult.OK)
{
    GlassMessage.Create($"Strategy: {r.InputText}\n" +
            $"Auto-restart: {(r.CheckBoxChecked ? "Yes" : "No")}\n\nCopying 312,940 files…")
        .Title("Migration Started")
        .Icon(MessageBoxIcon.Information)
        .Progress(12, 100)
        .Buttons("Run in background")
        .Show();
}
```

---

### 20. Toast notifications — `GlassToast`

Non-modal, auto-dismissing notifications that fade in at a screen corner.

#### Single toast

![Toast — Bottom Right](Images/Toast%20%E2%80%94%20Bottom-Right.png)

```csharp
GlassToast.Show(new GlassToastOptions
{
    Message    = "Invoice_March_2026.pdf saved to SharePoint · Finance",
    Title      = "Upload Complete",
    Icon       = MessageBoxIcon.Information,
    DurationMs = 4_000,
    Position   = ToastPosition.BottomRight,
});
```

#### Stacking

![Toast — Stacking](Images/Toast%20%E2%80%94%20Stacking.png)

Fire several at the same corner and they stack automatically; when one dismisses, the rest re-pack to close the gap.

```csharp
GlassToast.Show("Build succeeded — 0 errors, 2 warnings", "Glass.Message", MessageBoxIcon.Information);
GlassToast.Show("50 / 50 tests passed · Coverage 91.4 %", "Test Run Complete", MessageBoxIcon.Information);
GlassToast.Show("Deploying to staging-01.contoso.com…", "CI/CD Pipeline", MessageBoxIcon.Warning);
```

#### Any corner (or centre edge)

![Toast — Four Corners](Images/Toast%20%E2%80%94%20Four%20Corners.png)

```csharp
foreach (var (msg, pos) in new (string, ToastPosition)[]
{
    ("Top-left corner",     ToastPosition.TopLeft),
    ("Top-right corner",    ToastPosition.TopRight),
    ("Bottom-left corner",  ToastPosition.BottomLeft),
    ("Bottom-right corner", ToastPosition.BottomRight),
})
{
    GlassToast.Show(new GlassToastOptions
    {
        Message  = msg,
        Title    = "Position",
        Icon     = MessageBoxIcon.Information,
        Position = pos,
    });
}
```

**Positions:** `BottomRight` (default), `BottomLeft`, `TopRight`, `TopLeft`, `BottomCenter`, `TopCenter`.
**Click actions:** set `OnClick` on `GlassToastOptions` — the toast runs your action and dismisses.
**Await one:** `await GlassToast.ShowAsync(options)` completes when the toast closes.

---

## Demo Walkthrough

`Glass.Demo` is a single WinForms window — a 2-column grid of buttons, one per feature.

- **Entry point:** `Program.Main()` calls `ApplicationConfiguration.Initialize()`, sets two global defaults, and runs `DemoForm`:
  ```csharp
  GlassMessage.UseRoundedCorners = true;
  GlassMessage.DefaultTheme      = GlassTheme.AutoDetect();
  Application.Run(new DemoForm());
  ```
- **`DemoForm`** builds its UI from a single `(string Label, Action Action)[]` table — the one place to edit when adding a showcase. Each entry becomes a button whose click invokes a `Demo_*` method.
- **Each `Demo_*` method** is self-contained and demonstrates exactly one capability with realistic, product-style copy.

| Demo button | Method | Feature shown |
|---|---|---|
| Drop-in Replace (Show) | `Demo_Basic` | `GlassMessage.Show` |
| Dark / Light / Mica / HC / Classic Themes | `Demo_Themes` | `Theme(...)` presets |
| Auto-detect OS Theme | `Demo_AutoTheme` | `GlassTheme.AutoDetect()` |
| Fluent Builder API | `Demo_Builder` | `Create(...)` + custom buttons |
| Countdown Auto-Close (10 s) | `Demo_Countdown` | `AutoClose(...)` |
| "Don't Show Again" Checkbox | `Demo_CheckBox` | `CheckBox(...)` + `ShowEx()` |
| Inline Text Input | `Demo_Input` | `InputText(...)` |
| Password Input | `Demo_Password` | `InputPassword(...)` |
| Drop-down Input | `Demo_Dropdown` | `InputDropdown(...)` |
| Expandable Detail Section | `Demo_Detail` | `Detail(...)` |
| Determinate Progress Bar | `Demo_Progress` | `Progress(value, max)` |
| Indeterminate Progress Bar | `Demo_ProgressMarquee` | `ProgressIndeterminate()` |
| Custom Bitmap Icon | `Demo_CustomIcon` | `Icon(Bitmap)` |
| Ctrl+C Copy to Clipboard | `Demo_Copy` | `Ctrl+C` clipboard copy |
| SlideDown Animation | `Demo_Slide` | `Animation(SlideDown)` |
| Scale Animation | `Demo_Scale` | `Animation(Scale)` |
| RTL (Right-to-Left) Layout | `Demo_RTL` | `RightToLeft()` |
| Security Alert — Sign-in | `Demo_SecurityAlert` | 3-button mapping + `ShowEx()` |
| Windows Update — Release Notes | `Demo_ReleaseNotes` | `Default(Button2)` |
| End-User Licence Agreement | `Demo_Eula` | `Detail(...)` + `CheckBox(...)` |
| Storage Migration Wizard | `Demo_Migration` | dropdown + checkbox + progress |
| Toast — Bottom-Right | `Demo_Toast` | `GlassToast.Show(options)` |
| Toast — Stacking | `Demo_ToastStack` | auto-stacking toasts |
| Toast — Four Corners | `Demo_ToastCorners` | `ToastPosition` |
| Async ShowAsync | `Demo_Async` | `await GlassMessage.ShowAsync` |
| All Buttons + ShowEx Rich Result | `Demo_ShowEx` | full `GlassResult` |

See [Running the Demo](#running-the-demo) to launch it.

---

## API Overview

### `GlassMessage` (static facade)

The public entry point. Static methods mirror `MessageBox`.

```csharp
// MessageBox-compatible (several overloads):
DialogResult Show(string message);
DialogResult Show(string message, string title);
DialogResult Show(string message, string title, MessageBoxIcon icon);
DialogResult Show(string message, string title, MessageBoxIcon icon, MessageBoxButtons buttons);
DialogResult Show(string message, string title, MessageBoxIcon icon, MessageBoxButtons buttons, MessageBoxDefaultButton defaultButton);
DialogResult Show(IWin32Window owner, string message, string title, MessageBoxIcon icon, MessageBoxButtons buttons, MessageBoxDefaultButton defaultButton);
DialogResult Show(IWin32Window owner, string message, string title, MessageBoxIcon icon, MessageBoxButtons buttons, MessageBoxDefaultButton defaultButton, GlassTheme theme);

// Async (non-blocking):
Task<DialogResult> ShowAsync(string message, string title = "", MessageBoxIcon icon = None, MessageBoxButtons buttons = OK, CancellationToken cancellationToken = default);
Task<DialogResult> ShowAsync(string message, string title, MessageBoxIcon icon, MessageBoxButtons buttons, GlassTheme theme, CancellationToken cancellationToken = default);

// Fluent builder:
GlassBuilder Create(string message);

// Global defaults:
static GlassTheme DefaultTheme       { get; set; }  // = GlassTheme.Default
static bool       UseRoundedCorners  { get; set; }  // = false
```

### `GlassBuilder` (fluent builder)

Returned by `GlassMessage.Create(...)`. Every method returns the builder for chaining.

| Method | Purpose |
|---|---|
| `Title(string)` | Title-bar caption |
| `Icon(MessageBoxIcon)` / `Icon(Bitmap)` | System icon or custom 48×48 bitmap |
| `Buttons(MessageBoxButtons)` / `Buttons(params string[])` | Standard set or custom labels |
| `Default(MessageBoxDefaultButton)` | Focused / auto-close target button |
| `Theme(GlassTheme)` | Per-dialog theme override |
| `Owner(IWin32Window)` | Owner window (centres on / stays above it) |
| `Animation(GlassAnimation)` | Open/close animation |
| `RoundedCorners(bool = true)` | Per-dialog rounded-corner override |
| `AutoClose(int ms)` | Auto-confirm the default button after a delay |
| `CheckBox(string label, bool defaultChecked = false)` | Add a checkbox |
| `InputText(string placeholder = "", string defaultValue = "")` | Single-line input |
| `InputPassword(string placeholder = "")` | Masked input + reveal + Caps Lock hint |
| `InputMultiline(string placeholder = "", string defaultValue = "")` | Multi-line input |
| `InputDropdown(IEnumerable<string> items, string defaultItem = null)` | Drop-down list |
| `Detail(string)` | Expandable "Show details" panel |
| `Progress(int value, int max = 100)` | Determinate progress bar |
| `ProgressIndeterminate()` | Marquee progress bar |
| `RightToLeft(bool = true)` | Mirror layout for RTL |
| `Show()` | Show modally → `DialogResult` |
| `ShowEx()` | Show modally → `GlassResult` |

### `GlassResult`

```csharp
DialogResult Button          { get; }   // the button pressed
bool         CheckBoxChecked { get; }   // checkbox state
string       InputText       { get; }   // typed/selected value (never null)

// Implicitly converts to DialogResult:
DialogResult d = myGlassResult;
```

### `GlassToast` (static facade) & `GlassToastOptions`

```csharp
// Overloads:
GlassToast.Show(string message, int durationMs = 4_000);
GlassToast.Show(string message, string title, int durationMs = 4_000);
GlassToast.Show(string message, string title, MessageBoxIcon icon, int durationMs = 4_000);
GlassToast.Show(GlassToastOptions options);
Task        GlassToast.ShowAsync(GlassToastOptions options);

// Options:
class GlassToastOptions
{
    string        Message           { get; set; }
    string        Title             { get; set; }
    MessageBoxIcon Icon             { get; set; } = None;
    GlassTheme    Theme             { get; set; }
    int           DurationMs        { get; set; } = 4_000;
    ToastPosition Position          { get; set; } = BottomRight;
    Action        OnClick           { get; set; }
    bool?         UseRoundedCorners { get; set; }
}
```

### Enums

```csharp
enum GlassAnimation { Fade, SlideDown, Scale, None }
enum GlassInputMode { None, Text, Password, Multiline, Dropdown }
enum ToastPosition  { BottomRight, BottomLeft, TopRight, TopLeft, BottomCenter, TopCenter }
```

> `GlassInputMode` is selected for you by the `InputText` / `InputPassword` / `InputMultiline` / `InputDropdown` builder methods — you rarely set it directly.

---

## Configuration

### Global defaults (`GlassMessage`)

| Setting | Type | Default | Effect |
|---|---|---|---|
| `GlassMessage.DefaultTheme` | `GlassTheme` | `GlassTheme.Default` (dark) | Theme used when a dialog/toast doesn't specify one |
| `GlassMessage.UseRoundedCorners` | `bool` | `false` | Global rounded-corners default; per-dialog override via `RoundedCorners(...)` |

Set these once at startup:

```csharp
GlassMessage.UseRoundedCorners = true;
GlassMessage.DefaultTheme      = GlassTheme.AutoDetect();
```

### Per-dialog options

Everything else is configured through the builder (see the [API Overview](#api-overview)). The single **required** value is the message passed to `GlassMessage.Create(message)` / `GlassMessage.Show(message, …)`. All other parts are optional.

### Toast options (`GlassToastOptions`)

| Property | Default | Notes |
|---|---|---|
| `Message` | `""` | The body text |
| `Title` | `null` | Optional bold caption |
| `Icon` | `MessageBoxIcon.None` | Optional system icon |
| `Theme` | falls back to `GlassMessage.DefaultTheme` | Per-toast theme |
| `DurationMs` | `4000` | Time fully visible before fading out |
| `Position` | `ToastPosition.BottomRight` | Anchor corner / edge |
| `OnClick` | `null` | Action run when clicked (also dismisses) |
| `UseRoundedCorners` | `null` | `null` → use global setting |

---

## Styling and Customization

Beyond the five presets, you can construct a fully custom `GlassTheme`. Every colour, font, corner radius, and the window opacity is settable.

```csharp
var brand = new GlassTheme
{
    BackgroundTop    = Color.FromArgb(28, 16, 42),
    BackgroundBottom = Color.FromArgb(14, 8, 24),
    TitleBarTop      = Color.FromArgb(48, 24, 78),
    TitleBarBottom   = Color.FromArgb(28, 14, 48),
    BorderColor      = Color.FromArgb(180, 120, 255),
    AccentColor      = Color.FromArgb(150, 90, 240),  // focus, progress, links
    TitleColor       = Color.FromArgb(235, 225, 255),
    MessageColor     = Color.FromArgb(220, 210, 240),
    ButtonForeColor  = Color.FromArgb(240, 235, 255),
    ButtonFillTop    = Color.FromArgb(60, 36, 96),
    ButtonFillBottom = Color.FromArgb(36, 20, 60),
    CheckBoxColor    = Color.FromArgb(180, 120, 255),
    InputBackColor   = Color.FromArgb(20, 12, 34),
    InputForeColor   = Color.FromArgb(220, 210, 240),
    CornerRadius       = 10,   // window corner radius (0 = square)
    ButtonCornerRadius = 6,    // button corner radius
    Opacity            = 0.97, // base window opacity
};

GlassMessage.Create("Branded dialog ready.")
    .Title("My Product")
    .Theme(brand)
    .Show();

// Or apply it everywhere:
GlassMessage.DefaultTheme = brand;
```

**Theme surface (all settable):**

| Group | Properties |
|---|---|
| Window body | `BackgroundTop`, `BackgroundBottom` |
| Title bar | `TitleBarTop`, `TitleBarBottom`, `TitleColor` |
| Accent / edge | `BorderColor`, `AccentColor` |
| Text | `MessageColor` |
| Buttons | `ButtonForeColor`, `ButtonFillTop`, `ButtonFillBottom` |
| Inputs / checkbox | `InputBackColor`, `InputForeColor`, `CheckBoxColor` |
| Shape | `CornerRadius`, `ButtonCornerRadius` |
| Fonts | `TitleFont`, `MessageFont`, `ButtonFont` |
| Window | `Opacity` |

> **Tip:** The built-in presets (`Default`, `Dark`, `Light`, `Mica`, `HighContrast`, `WindowsClassic`) are shared singletons and are exempt from disposal. Custom themes you create implement `IDisposable` and free their fonts on `Dispose()` — let them be collected normally, or dispose explicitly if you create many short-lived ones.

**Modern chrome:** when rounded corners are enabled, the dialog requests crisp Windows 11 DWM corners and applies a **Mica** backdrop (falling back to **Acrylic** blur on Windows 10), giving a translucent, native feel. On older systems it degrades gracefully to a software-rounded region.

---

## Error Handling / Best Practices

- ✅ **Use `ShowEx()` when you need data back.** `Show()` returns only the button; `ShowEx()` returns the button **and** checkbox **and** input.
- ✅ **Validate input.** `GlassResult.InputText` is never `null` (empty string when there's no input), but still validate content (e.g. `string.IsNullOrWhiteSpace`).
- ✅ **Know your button mapping.** Custom labels map onto OK / OK-Cancel / Yes-No-Cancel by count — confirm which `DialogResult` each label produces (see [section 4](#4-fluent-builder--create)).
- ✅ **Prefer `ShowAsync` in async handlers** so the UI thread keeps pumping messages; pass a `CancellationToken` to dismiss programmatically.
- ✅ **Set global defaults once** (`DefaultTheme`, `UseRoundedCorners`) at startup rather than per call.
- ✅ **Dispose custom bitmap icons** with `using` after the dialog returns.
- ✅ **Use `GlassAnimation.None` in tests** for deterministic, instant dialogs.
- ⚠️ **Windows-only.** The library targets WinForms and Windows; it isn't cross-platform.
- ⚠️ **Treat passwords as secrets.** Don't log `InputText` from a password field.

---

## Project Structure

```
Glass/
├── Glass.sln                     # Solution: library + demo + tests
├── README.md                     # This file
├── LICENSE                       # MIT
├── Glass.Message/                # ── The library ──
│   ├── GlassMessage.cs           # Static facade (Show / ShowAsync / Create)
│   ├── GlassBuilder.cs           # Fluent builder
│   ├── GlassDialog.cs            # The WinForms dialog (rendering, layout, animations)
│   ├── GlassDialogConfig.cs      # Internal settings bag
│   ├── GlassResult.cs            # Rich result (button + checkbox + input)
│   ├── GlassTheme.cs             # Palette + presets + AutoDetect
│   ├── GlassToast.cs             # Toast facade, options, and toast window
│   ├── GlassButton.cs            # Themed button control
│   ├── GlassAnimation.cs         # enum: Fade / SlideDown / Scale / None
│   └── GlassInputMode.cs         # enum: None / Text / Password / Multiline / Dropdown
├── Glass.Demo/                   # ── WinForms feature gallery ──
│   └── Program.cs                # DemoForm + one Demo_* method per feature
├── Glass.Message.Tests/          # ── Unit tests ──
├── Images/                       # Screenshots used by this README
└── tools/                        # Build/signing helper scripts
```

---

## Requirements

| | |
|---|---|
| **OS** | Windows (the library is Windows-only by design) |
| **UI stack** | Windows Forms (`UseWindowsForms`) |
| **Library target frameworks** | `net481`, `net8.0-windows`, `net9.0-windows`, `net10.0-windows` |
| **Demo target framework** | `net8.0-windows` |
| **High-DPI** | `PerMonitorV2` (DPI-aware) |
| **Dependencies** | None beyond the .NET / WinForms framework assemblies |

> The library cross-targets .NET Framework 4.8.1 and modern .NET (8/9/10), so it drops into both legacy and current Windows desktop apps.

---

## Running the Demo

### Visual Studio

1. Open `Glass.sln`.
2. Set **Glass.Demo** as the startup project.
3. Press **F5**.

### Command line

```bash
# from the repository root
dotnet run --project Glass.Demo
```

```bash
# build everything
dotnet build Glass.sln -c Release
```

Click any button in the gallery to launch that feature's dialog or toast.

> **Note:** The projects include an optional post-build Authenticode signing step (`tools\Sign-Output.ps1`) that runs `ContinueOnError`. If you haven't set up a dev certificate, the build still succeeds — signing is simply skipped.

---

## FAQ

**Q: Can I really just replace `MessageBox` with `GlassMessage`?**
A: Yes. `GlassMessage.Show(...)` is signature-compatible with `MessageBox.Show(...)`, so most call sites compile unchanged and keep returning a `DialogResult`.

**Q: How do I get the checkbox state or typed input back?**
A: Call `ShowEx()` (instead of `Show()`) on the builder and read `GlassResult.CheckBoxChecked` / `GlassResult.InputText`.

**Q: My three-button dialog returns weird `DialogResult` values. Why?**
A: Custom labels are mapped to the nearest standard layout by count (1 → OK, 2 → OK/Cancel, 3+ → Yes/No/Cancel). With three labels, the buttons return `Yes` / `No` / `Cancel` in order.

**Q: How do I make dialogs follow the user's Windows theme?**
A: `GlassMessage.DefaultTheme = GlassTheme.AutoDetect();` at startup (it returns high-contrast, dark, or light to match Windows).

**Q: Are toasts modal?**
A: No. Toasts are non-modal, top-most, auto-dismissing windows. They stack at their corner and re-pack when one closes.

**Q: Does it block the UI thread?**
A: `Show()`/`ShowEx()` are modal (blocking). Use `ShowAsync(...)` for a non-blocking, awaitable dialog.

**Q: How do I close an async dialog from code?**
A: Pass a `CancellationToken` to `ShowAsync`; cancelling it closes the dialog and yields `DialogResult.Cancel`.

**Q: Which .NET versions are supported?**
A: The library builds for .NET Framework 4.8.1 and .NET 8 / 9 / 10 (Windows).

---

## Contributing

Contributions are welcome!

1. **Fork** the repository and create a feature branch.
2. Make your change in `Glass.Message`, and — if it's a user-visible feature — **add a `Demo_*` showcase** to `Glass.Demo/Program.cs` by appending an entry to the `demos` table.
3. Add or update tests in **Glass.Message.Tests**.
4. Build the whole solution (`dotnet build Glass.sln`) and run the demo to smoke-test visually.
5. Open a pull request describing the change and including a screenshot for any UI work.

> **Tip:** The demo's `demos` array is the single list to edit when adding or removing a feature showcase — keep it in sync with the library.

---

## License

This project is licensed under the **MIT License** — see the [LICENSE](LICENSE) file for the full text.

```
Copyright (c) 2026 Gehan Fernando
```

---

## Full Example

A single, practical flow that combines a custom theme, the fluent builder, an input, a checkbox, the rich result, async, and a confirming toast.

```csharp
using Glass;
using System.Threading.Tasks;
using System.Windows.Forms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        // 1) Global defaults — modern corners + follow the OS theme.
        GlassMessage.UseRoundedCorners = true;
        GlassMessage.DefaultTheme      = GlassTheme.AutoDetect();

        Application.Run(new MainForm());
    }
}

internal sealed class MainForm : Form
{
    public MainForm()
    {
        Text = "Glass.Message — Full Example";
        var btn = new Button { Text = "Export report…", Dock = DockStyle.Fill };
        btn.Click += async (_, _) => await ExportAsync();
        Controls.Add(btn);
    }

    private async Task ExportAsync()
    {
        // 2) Rich dialog: drop-down + checkbox, read back via ShowEx().
        var choice = GlassMessage.Create(
                "Choose the output format for 'Annual_Report_Q4_2025'.\n\n" +
                "The selected format determines whether charts remain editable.")
            .Title("Export Document")
            .Icon(MessageBoxIcon.Question)
            .InputDropdown(
                ["PDF — Portable Document Format",
                 "Word (.docx) — Microsoft Word",
                 "Excel (.xlsx) — Microsoft Excel"],
                "PDF — Portable Document Format")
            .CheckBox("Open the file when export finishes")
            .Buttons("Export", "Cancel")
            .Animation(GlassAnimation.Scale)
            .ShowEx();

        if (choice.Button != DialogResult.OK)
        {
            GlassToast.Show("Export cancelled.", "Export", MessageBoxIcon.Warning);
            return;
        }

        // 3) Non-blocking progress confirmation.
        var confirm = await GlassMessage.ShowAsync(
            $"Exporting as:\n{choice.InputText}\n\nThis runs in the background.",
            "Export Started",
            MessageBoxIcon.Information,
            MessageBoxButtons.OKCancel);

        if (confirm == DialogResult.OK)
        {
            // 4) Notify without interrupting — a stacking toast.
            GlassToast.Show(new GlassToastOptions
            {
                Title    = "Export Complete",
                Message  = $"{choice.InputText}\nSaved to Downloads"
                           + (choice.CheckBoxChecked ? " · opening now…" : ""),
                Icon     = MessageBoxIcon.Information,
                Position = ToastPosition.BottomRight,
                OnClick  = () => { /* open the file / folder */ },
            });
        }
    }
}
```

---

<div align="center">

**Glass.Message** — built with care by **Gehan Fernando** · Licensed under MIT

*A modern message box, the way Windows dialogs should look.*

</div>
