# Changelog

All notable changes to **Glass.Message** are documented here.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

> **How releases work:** every tagged release on GitHub ships downloadable
> assets — the multi-framework NuGet package (`.nupkg` + `.snupkg`), a raw DLL
> zip per target framework, and a self-contained demo app you can run without
> installing .NET. See the [Releases page](../../releases).

## [Unreleased]

_Changes landed on `main` but not yet released will appear here._

## [1.0.1] - 2026-06-01

### Fixed

- **EULA / checkbox dialog:** the checkbox label (e.g. _"I have read and accept
  the licence terms"_) was truncated to a few characters after expanding the
  **Show details** panel. The checkbox is now sized deterministically instead of
  relying on a layout pass that a runtime rebuild skipped.

- **Demo — "Toast — Four Corners":** the fourth toast appeared at the bottom
  **centre** instead of the bottom-right corner. It now uses
  `ToastPosition.BottomRight` so all four corners are demonstrated.

### Added

- **Release pipeline** (`.github/workflows/release.yml`): publishing a GitHub
  Release now builds all target frameworks on Windows, runs the tests, and
  attaches downloadable assets to the release automatically.
- **Categorised release notes** configuration (`.github/release.yml`).
- **This changelog.**

## [1.0.0] - 2026-05-31

### Added

- **Drop-in `MessageBox` replacement** — `GlassMessage.Show(...)` overloads that
  are signature-compatible with `System.Windows.Forms.MessageBox`.
- **Fluent builder** — `GlassMessage.Create(...)` for composing dialogs.
- **Async dialogs** — `GlassMessage.ShowAsync(...)`: non-blocking, awaitable, and
  cancellable via `CancellationToken`.
- **Rich result** — `ShowEx()` returns a `GlassResult` (button + checkbox state +
  typed input); implicitly converts to `DialogResult`.
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
- **Animations** — `Fade` (default), `SlideDown`, `Scale`, and `None`.
- **Keyboard** — `Ctrl+C` copies title + message; `Enter` / `Esc` map to sensible
  results.
- **Right-to-left** mirrored layout.
- **Toast notifications** — `GlassToast` with six anchor positions, auto-stacking,
  click actions, and an async variant.
- **Cross-target build** — .NET Framework 4.8.1 and .NET 8 / 9 / 10 (Windows).

[Unreleased]: ../../compare/v1.0.1...HEAD
[1.0.1]: ../../compare/v1.0.0...v1.0.1
[1.0.0]: ../../releases/tag/v1.0.0
