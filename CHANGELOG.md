# Changelog

All notable changes to **Glass.Message** are documented here.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

> **How releases work:** every tagged release on GitHub ships downloadable
> assets — the multi-framework NuGet package (`.nupkg` + `.snupkg`), a raw DLL
> zip per target framework, and self-contained demo apps (x64 + x86) you can run
> without installing .NET. The notes below become the release description
> automatically. See the [Releases page](../../releases).

## [1.0.1] - 2026-06-06

First public release. 🎉

### Added

- **Drop-in `MessageBox` replacement** — `GlassMessage.Show(...)` overloads that
  are signature-compatible with `System.Windows.Forms.MessageBox`.
- **Fluent builder** — `GlassMessage.Create(...)` for composing dialogs.
- **Async dialogs** — non-blocking, awaitable, and cancellable via
  `CancellationToken`:
  - `GlassMessage.ShowAsync(...)` for the basic message-box shape.
  - `GlassBuilder.ShowAsync()` / `ShowExAsync()` so rich dialogs (input, checkbox,
    dropdown) can be awaited too. Cancellation yields `DialogResult.Cancel`.
- **Rich result** — `ShowEx()` / `ShowExAsync()` return a `GlassResult` (button +
  checkbox state + typed input); implicitly converts to `DialogResult`.
- **Live progress** — `GlassBuilder.ShowProgress()` returns a thread-safe
  `GlassProgressController` to update the bar (`SetValue`) and caption
  (`SetMessage`) while work runs, `Complete()` it, await `Completion`, and detect
  user dismissal via `WasCanceledByUser`.
- **Theming** — `Dark`, `Light`, `Mica`, `HighContrast`, and `WindowsClassic`
  presets, plus fully customisable `GlassTheme` instances.
- **Auto theme detection** — `GlassTheme.AutoDetect()` / `IsSystemDark()` follow
  the Windows light / dark / high-contrast preference.
- **Modern chrome** — Windows 11 rounded corners (DWM), Mica backdrop, and an
  Acrylic blur fallback on Windows 10.
- **Inline inputs** — single-line text, password (reveal eye + Caps Lock hint),
  multi-line, and drop-down.
- **Checkbox**, **expandable detail panel**, **determinate / indeterminate
  progress bars**, **countdown auto-close**, and **custom bitmap icons**.
- **System sounds** — `GlassBuilder.Sound()` and the global
  `GlassMessage.PlaySystemSounds` play the Windows sound matching the icon when a
  dialog opens, like the classic `MessageBox`.
- **Animations** — `Fade` (default), `SlideDown`, `Scale`, and `None`.
- **Keyboard** — `Ctrl+C` copies title + message; `Enter` / `Esc` map to sensible
  results.
- **Right-to-left** mirrored layout.
- **Toast notifications** — `GlassToast` with six anchor positions, auto-stacking,
  click actions, and an async variant. Toasts are **multi-monitor aware**: they
  auto-target the active window's screen (then the cursor's, then primary) and
  stack per-screen; override with `GlassToastOptions.Screen`.
- **Cross-target build** — .NET Framework 4.8.1 and .NET 8 / 9 / 10 (Windows);
  AnyCPU, so the library runs in both x86 and x64 processes.
- **Release pipeline** — pushing a `v*` tag builds every target framework, runs
  the tests, and publishes a GitHub Release with notes and downloadable assets.

[1.0.1]: ../../releases/tag/v1.0.1
