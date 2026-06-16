# Changelog

All notable changes to **Glass.Message** are documented here.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

> **How releases work:** every tagged release on GitHub ships downloadable
> assets — the multi-framework NuGet package (`.nupkg` + `.snupkg`), a raw DLL
> zip per target framework, and self-contained demo apps (x64 + x86) you can run
> without installing .NET. The notes below become the release description
> automatically. See the [Releases page](../../releases).

## [1.0.5] - 2026-06-16

A polish and code-quality release. The live progress dialog now animates more
smoothly, a hairline rendering artifact on the bar is gone, and the source has
been cleaned up to satisfy the Roslynator analyzer set. No public API changes.

### Fixed

- **Smoother live progress** — calling `GlassProgressController.SetValue(...)` now
  eases the determinate fill toward the new value instead of snapping to it, so a
  jump (e.g. 10 % → 60 %) glides across roughly half a second. The animation timer
  spins up only while the bar is catching up and retires itself once it settles,
  so an idle dialog still does no per-frame work.
- **Hairline "background edge" on the progress bar** — the determinate fill was
  drawn through an *aliased* region clip whose hard edge exposed a 1 px rim of the
  lighter track behind the bar; the gradient also bled a thin seam at its edge.
  The fill is now painted as an antialiased rounded path with an inset gradient,
  so the bar reads as a clean, solid pill with no light fringe.

### Changed

- **Code-quality cleanup (internal only)** — applied the Roslynator style/analyzer
  fixes across the library and tests: conditional access (`?.`) over null-check
  ternaries and chains, a read-only backing field, a static helper, concrete
  field types for owner-drawn controls, and a trimmed empty type body. Behaviour
  and the public API are unchanged.

## [1.0.4] - 2026-06-14

A feature release that makes the live progress dialog *activity-aware* — the
bar can now animate to match the operation it represents (upload, download,
sync, compress, …) — plus visual-polish and performance fixes for the progress
bar.

### Added

- **`ProgressActivity(GlassProgressActivity)` builder method** and the new
  **`GlassProgressActivity`** enum (17 activities). The progress bar paints a
  distinct, eye-catching animation grouped into six visual families:

  | Family | Animation | Activities |
  |---|---|---|
  | **Packets** | glowing dots glide along | `Upload` · `Download` · `Stream` |
  | **Chevrons** | diagonal bands march | `FileTransfer` · `Import` |
  | **Segments** | rounded blocks march | `Compress` · `Extract` · `Export` |
  | **Wave** | a sinusoidal ripple | `Backup` · `Restore` · `Sync` |
  | **Pulse** | the fill breathes | `Encrypt` · `Decrypt` · `Connecting` |
  | **Comet** | a soft shine sweeps | `Install` · `Search` · `Processing` |

  Stripes flow forward for outgoing work, backward for incoming, and ease both
  ways for a two-way sync. Works on both determinate and indeterminate bars; use
  `GlassProgressActivity.None` for a plain bar.
- **`GlassProgressController.SetActivity(GlassProgressActivity)`** — change the
  flow animation live as an operation moves between phases (e.g. `Compress` then
  `Upload`).

### Fixed

- **Progress bar "box" artifact** — the progress panel now paints the dialog's
  themed gradient behind itself (like every other custom control) instead of
  clearing to a flat colour, so it blends into the window seamlessly rather than
  showing a dark rectangle around the track.
- **Progress bar visibility** — boosted the contrast of every activity animation
  so the styles read clearly at the standard bar height.

### Performance

- **No per-frame allocations in the animated progress panel** — the glow-dot
  sprite, themed-background gradient, track border pen, flow brushes, and chevron
  geometry are all cached/reused, so an animating progress dialog no longer
  churns GDI+ objects at 30 fps.

## [1.0.3] - 2026-06-13

A maintenance release — documentation, packaging, and CI only. No library code
or public API changes.

### Changed

- **Docs** — removed an unsubstantiated adoption claim from the NuGet README
  (`README.nuget.md`).
- **Packaging** — switched to a more reliable NuGet version badge and refreshed
  the README badges (camo cache `cacheSeconds` lowered so the version badge
  updates promptly).
- **CI** — bumped the release workflow actions to Node 24: `actions/checkout@v6`,
  `actions/upload-artifact@v7`, `actions/download-artifact@v8`, and
  `actions/setup-dotnet@v5`; step names normalised to PascalCase.

## [1.0.2] - 2026-06-12

### Added

- **`showCapsLockHint` parameter on `InputPassword()`** — opt out of the Caps Lock
  badge per dialog with `.InputPassword("placeholder", showCapsLockHint: false)`.
  The hint stays on by default; suppress it for PIN entry, kiosk flows, or anywhere
  uppercase input is intentional. Showcased by the new **Password — Caps Lock Hint
  Off** demo (`Demo_PasswordNoCapsLock`).

### Fixed

#### Thread safety & concurrency
- **Volatile static properties** — `DefaultTheme`, `UseRoundedCorners`, and
  `PlaySystemSounds` in `GlassMessage` now use `volatile` backing fields, ensuring
  writes from any thread are immediately visible on all architectures including ARM64.
- **CancellationToken callback crash** (C1) — the `ct.Register` callback in
  `ShowModeless` now wraps `BeginInvoke` in a `try/catch` for
  `InvalidOperationException` / `ObjectDisposedException`, preventing an unhandled
  exception that could crash the process when the dialog's handle is destroyed
  concurrently with cancellation.
- **Hanging async tasks on shutdown** (C2, C7) — `GlassMessage.ShowModeless` and
  `GlassToast.ShowAsync` now subscribe to the form's `Disposed` event and call
  `TrySetResult` as a safety net, guaranteeing that every awaited task completes even
  when `Application.Exit` disposes forms without firing `FormClosed`.
- **Concurrent GDI+ bitmap access** (C5) — system icons loaded from
  `SystemIcons`/`MessageBoxIcon` are now cloned per-dialog via a new `ResolveIcon`
  helper. The `_ownsIconBitmap` flag ensures clones are disposed while shared bitmaps
  are left alone.
- **Toast options mutation** — `GlassToast.Show/ShowAsync` resolved the theme into a
  local variable instead of writing back into the caller's `GlassToastOptions`, so
  the same options object can be reused safely.
- **Toast race: form added to active list before handlers registered** — `FormClosed`
  and `Disposed` are now registered before `_active.Add`, eliminating the window
  where a fast OS `WM_CLOSE` could leave a ghost entry in the stack.
- **Toast `OnClick` exception suppressing dismiss** — wrapped in `try/finally` so a
  throwing callback never prevents the toast from closing.

#### Animation & DPI
- **Scale animation close-button hit-test** (#5) — `_closeBtnBounds` is now
  recalculated after every scale frame in `ApplyAnimationFrame`, so the × button
  remains clickable throughout the opening/closing animation.
- **DPI change corrupting animation state** (#7) — `Rebuild()` now resets all
  animation flags (`_scaleActive`, `_slideActive`, `_fadingOut`) and disposes the
  fade timer before rebuilding, preventing leftover state from corrupting the next
  animation cycle.
- **Wrong initial DPI scale** — the dialog now reads the DPI of the monitor under
  the cursor at construction time via `GetDpiForMonitor` + `MonitorFromPoint`
  P/Invokes instead of using the primary monitor's device context.
- **Width cap using primary monitor** — `MeasureForm` now accepts a `targetScreen`
  parameter and caps width against the target screen's working area, not the primary.

#### GDI+ resource management
- **Per-frame pen allocation** (#11) — the input border pen is now pre-allocated
  once in the constructor as `_inputBorderPen` and disposed in `Dispose()`, avoiding
  GDI+ handle churn on every `OnPaint` call.
- **Preset theme double-instantiation** (#18) — `GlassTheme.Dark` is now the same
  object reference as `GlassTheme.Default`, halving font allocations for the default
  case and making equality checks (`Dark == Default`) correct.

#### Drag behaviour
- **Dialog draggable off-screen** (#3) — `OnMouseMove` now calls `ClampToScreen`
  after each drag update, keeping the title bar (and drag handle) always reachable.

#### API correctness
- **`GlassResult` null implicit conversion** (#19) — the `implicit operator
  DialogResult` now uses `r?.Button ?? DialogResult.None` instead of `r.Button`,
  preventing a `NullReferenceException` when the result is assigned from `null`.
- **`InputDropdown` null argument crash** (#1) — a `null` `items` enumerable no
  longer throws; an empty dropdown is created instead.
- **`EM_SETCUEBANNER` wParam** — the `wParam` for the `EM_SETCUEBANNER` message in
  `PlaceholderTextBox` is now `1u` (redraw even if focused), matching the MSDN spec
  and ensuring placeholder text renders reliably.
- **`GlassDialogConfig` property access modifiers** (#17) — all properties on the
  already-`internal` `GlassDialogConfig` class are now explicitly `internal`,
  removing misleading `public` modifiers that had no external effect but implied
  wider accessibility than intended.

#### Docs & XML
- **`Buttons(params string[])` XML doc** (#4) — updated to document the 3-label
  maximum and explain that labels beyond the third are silently ignored.

#### Tests
- Added `[CollectionDefinition("GlassStaticState")]` and applied
  `[Collection("GlassStaticState")]` to `GlassBuilderTests` and
  `GlassMessageStaticTests`, serialising the classes that mutate global static state
  so they cannot interfere with each other under xUnit parallel execution.
- Added `CalcToastLocationTests` — pure coordinate-math tests for all six
  `ToastPosition` values plus stack-offset assertions.
- Added `V102RegressionTests` — targeted regression tests for the `GlassResult` null
  conversion, `Dark == Default` identity, and `InputDropdown(null)` safety.
- Updated xUnit packages: `xunit` and `xunit.runner.visualstudio` → **2.9.3**,
  `Microsoft.NET.Test.Sdk` → **17.12.0**.

#### Progress controller
- Documented the intentional fire-and-forget design of `BeginInvoke` in
  `GlassProgressController.Marshal` — `SetValue`/`SetMessage` are best-effort UI
  refreshes that must not block worker threads; `Completion` remains the correct
  await point.

#### Marquee timer
- Halved marquee progress timer pressure: interval 16 ms → 33 ms (≈ 30 fps),
  phase step scaled proportionally so visual speed is unchanged.

---

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

[1.0.5]: ../../releases/tag/v1.0.5
[1.0.4]: ../../releases/tag/v1.0.4
[1.0.3]: ../../releases/tag/v1.0.3
[1.0.2]: ../../releases/tag/v1.0.2
[1.0.1]: ../../releases/tag/v1.0.1
