# 🪟 Glass.Message

**The modern `MessageBox` replacement for Windows desktop apps (WinForms).**

Mica & Acrylic backdrops · automatic dark/light theming · fluent builder · async dialogs · inline inputs · live progress · toasts · full RTL.

[![NuGet](https://img.shields.io/nuget/v/Glass.Message?color=0078D6&logo=nuget)](https://www.nuget.org/packages/Glass.Message)
[![Downloads](https://img.shields.io/nuget/dt/Glass.Message?color=3da639)](https://www.nuget.org/packages/Glass.Message)
![.NET](https://img.shields.io/badge/.NET-4.8.1%20%7C%208%20%7C%209%20%7C%2010-512BD4?logo=dotnet&logoColor=white)
![License](https://img.shields.io/badge/license-MIT-3da639)

![Glass.Message feature gallery](https://raw.githubusercontent.com/gcfernando/glass/main/Images/Feature%20Gallery.png)

## Why Glass.Message?

You already write `MessageBox.Show(...)`. Change the type name to `GlassMessage` and your dull system dialog becomes a modern, themed, DPI-aware, animated one — **with zero other code changes.** The `Show(...)` overloads are signature-compatible with `System.Windows.Forms.MessageBox` and still return a `DialogResult`.

```csharp
// Before — flat, dated, fixed grey chrome
MessageBox.Show("The printer is offline.", "Printer Offline",
    MessageBoxButtons.OK, MessageBoxIcon.Warning);

// After — modern, themed, animated (same result)
GlassMessage.Show("The printer is offline.", "Printer Offline",
    MessageBoxIcon.Warning);
```

## Install

```
dotnet add package Glass.Message
```

## Quick start

```csharp
using Glass;
using System.Windows.Forms;

// Optional global defaults, once at startup:
GlassMessage.UseRoundedCorners = true;                    // Windows 11 rounded corners
GlassMessage.DefaultTheme      = GlassTheme.AutoDetect(); // follow the OS light/dark theme

// A basic, MessageBox-style dialog:
DialogResult result = GlassMessage.Show(
    "Your changes have been saved.", "Success", MessageBoxIcon.Information);
```

## Features

- **Drop-in API** — `GlassMessage.Show(...)` mirrors `MessageBox.Show`.
- **Fluent builder** — `GlassMessage.Create(...)` for composable dialogs.
- **Async** — `ShowAsync()` / `ShowExAsync()` on the facade and the builder: non-blocking, awaitable, cancellable.
- **Rich result** — `ShowEx()` returns button **+** checkbox state **+** typed input.
- **Live progress** — `ShowProgress()` returns a thread-safe `GlassProgressController` you update while work runs.
- **Theming** — Dark, Light, Mica, High Contrast, Windows Classic, plus custom themes and OS auto-detect.
- **Modern chrome** — Windows 11 rounded corners (DWM) + Mica backdrop, Acrylic fallback on Windows 10.
- **Inputs** — single-line text, password (reveal eye + Caps Lock hint), multi-line, drop-down.
- **More** — checkbox, expandable detail panel, determinate/indeterminate progress, countdown auto-close, custom bitmap icons, system sounds, animations, `Ctrl+C` copy, full RTL.
- **Toasts** — six anchor positions, auto-stacking, multi-monitor aware, click actions, async variant.
- **Targets** — .NET Framework 4.8.1 and .NET 8 / 9 / 10 (Windows); AnyCPU (x86 + x64).

## A few examples

```csharp
// Fluent builder with custom buttons
GlassMessage.Create("Annual_Report_Q4_2025.xlsx has unsaved changes.")
    .Title("Unsaved Changes")
    .Icon(MessageBoxIcon.Warning)
    .Buttons("Save", "Don't Save", "Cancel")
    .Show();

// Input + checkbox, read everything back
var r = GlassMessage.Create("Enter a new name for the folder.")
    .Title("Rename Folder")
    .InputText("Folder name", "Client Projects")
    .CheckBox("Apply to all items")
    .Buttons("Rename", "Cancel")
    .ShowEx();
if (r.Button == DialogResult.OK) { var name = r.InputText; }

// Non-blocking, awaitable
var choice = await GlassMessage.ShowAsync(
    "Push local changes to origin/main?", "Sync",
    MessageBoxIcon.Question, MessageBoxButtons.OKCancel);

// Toast notification
GlassToast.Show("Invoice saved to SharePoint", "Upload Complete", MessageBoxIcon.Information);
```

![Fluent builder](https://raw.githubusercontent.com/gcfernando/glass/main/Images/Fluent%20Builder%20API.png)

![Password input with reveal eye and Caps Lock hint](https://raw.githubusercontent.com/gcfernando/glass/main/Images/Password%20Input.png)

## Documentation

- 📖 **Full docs, gallery & API:** https://github.com/gcfernando/glass
- 📜 **Changelog:** https://github.com/gcfernando/glass/blob/main/CHANGELOG.md
- ⬇️ **Releases & downloads:** https://github.com/gcfernando/glass/releases

---

Windows-only (WinForms). MIT Licensed · © 2026 Gehan Fernando
