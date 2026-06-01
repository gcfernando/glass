# Changelog

All notable changes to **Glass.Message** are documented here.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

> **How releases work:** every tagged release on GitHub ships downloadable
> assets — the multi-framework NuGet package (`.nupkg` + `.snupkg`), a raw DLL
> zip per target framework, and self-contained demo apps (x64 + x86) you can run
> without installing .NET. The notes below become the release description
> automatically. See the [Releases page](../../releases).

## [Unreleased]

_Changes landed on `main` but not yet released will appear here._

## [1.0.0] - 2026-06-01

First public release. 🎉

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
- **Cross-target build** — .NET Framework 4.8.1 and .NET 8 / 9 / 10 (Windows);
  AnyCPU, so the library runs in both x86 and x64 processes.
- **Release pipeline** — pushing a `v*` tag builds every target framework, runs
  the tests, and publishes a GitHub Release with notes and downloadable assets.

[Unreleased]: ../../compare/v1.0.0...HEAD
[1.0.0]: ../../releases/tag/v1.0.0
