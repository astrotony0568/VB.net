# Drive Read Indicator

A small VB.NET WinForms (.NET 9) app for Windows 11 that shows a blinking dot
whenever the selected logical drive (e.g. `E:`) is being read. Intended for use
alongside Windows Media Player so the user can see when the music drive is
actively being read during playback.

## Requirements

- Windows 11
- .NET 9 SDK
- Visual Studio 2022 (17.8+) with the .NET desktop development workload, OR the
  `dotnet` CLI

## How to Build

### Visual Studio

1. Open `DriveReadIndicator.sln` in Visual Studio.
2. Let it restore NuGet packages.
3. Build (`Ctrl+Shift+B`).
4. Run (`F5`).

### Command line

```powershell
cd DriveReadIndicator
dotnet build -c Release
dotnet run --project DriveReadIndicator
```

> **If you installed the .NET 9 SDK to a user-local folder** (e.g. via
> `dotnet-install.ps1 -InstallDir $env:LOCALAPPDATA\Microsoft\dotnet`)
> rather than the default `C:\Program Files\dotnet`, also set
> `DOTNET_ROOT` so the built `.exe` can find `hostfxr.dll`:
>
> ```powershell
> [Environment]::SetEnvironmentVariable(
>     'DOTNET_ROOT',
>     "$env:LOCALAPPDATA\Microsoft\dotnet",
>     'User')
> ```
>
> Without this, double-clicking `DriveReadIndicator.exe` will fail with
> `Failed to resolve hostfxr.dll [not found]. Error code: 0x80008083`.

## How to Change the Drive

Edit this constant near the top of `MainForm.vb`:

```vb
Private Const TargetDriveInstance As String = "E:"
```

Accepted formats: `C`, `C:`, `C:\` — they are all normalised to the
performance-counter instance form (`C:`). Typical examples:

- `C:`
- `D:`
- `E:`

You can also tune the sampling behaviour (also in `MainForm.vb`):

| Constant                       | Default | Purpose                                                                 |
| ------------------------------ | ------: | ----------------------------------------------------------------------- |
| `MonitorIntervalMs`            |     200 | How often the app samples the drive read counter.                       |
| `BlinkIntervalMs`              |     150 | How fast the dot toggles while activity is detected.                    |
| `ActivityHoldMs`               |     500 | Keeps the dot blinking briefly after a read so short MP3 reads are seen. |
| `ReadThresholdBytesPerSecond`  |    1024 | Ignores tiny/noisy reads. Lower if MP3 reads are too brief to register. |

If MP3 playback does not visibly blink enough, the plan recommends:

```
ActivityHoldMs = 800
ReadThresholdBytesPerSecond = 1
```

## How to Test

1. Put a `.mp3` file and a `.flac` file on the monitored drive (e.g.
   `E:\MusicTest\test.mp3`, `E:\MusicTest\test.flac`).
2. Start the app — the dot should be off and the status should read
   `Status: Monitoring E:`.
3. Open Windows Media Player and start playback from the monitored drive.
4. Watch the dot blink while the drive is read.

To double-check that the dot really tracks the drive, open Performance Monitor
(`perfmon`) and add the counter:

```
LogicalDisk(E:)\Disk Read Bytes/sec
```

The dot should blink while that counter is non-zero.

## Expected Behaviour

- **FLAC** files tend to cause frequent or near-continuous reads — the dot may
  appear almost always lit during playback.
- **MP3** files are smaller and buffered in chunks — the dot tends to blink in
  short bursts.
- **Cached playback**: if the same file is played repeatedly, Windows may serve
  the data from RAM cache and the dot may not blink. Use a fresh/larger file
  to test in that case.
- **Other processes**: this app reports *all* reads on the selected drive, not
  just Windows Media Player. Anything else reading the drive (Explorer
  thumbnails, antivirus, indexing) will also light the dot. This is by design.

## Limitations & Troubleshooting

- **Drive letter changes.** The monitored drive is a hard-coded constant.
  If a USB drive gets a different letter, update `TargetDriveInstance` in
  `MainForm.vb`.
- **USB unplug.** If the monitored drive disappears mid-run, the dot turns off
  and the status shows `Drive E: not available`. The app retries every ~3
  seconds and recovers automatically once the drive comes back.
- **Performance counters disabled.** On some hardened systems the LogicalDisk
  category may be missing or rebuild-required. To restore, run
  `lodctr /r` from an elevated command prompt and retry.
- **No history / no logging.** This is intentional per the project scope.
- **Antivirus / Indexing.** Background scanners count as drive reads. Expect
  brief blinks even when no app is doing anything obvious.

## Source layout

```
DriveReadIndicator/
├── DriveReadIndicator.sln
└── DriveReadIndicator/
    ├── DriveReadIndicator.vbproj
    ├── app.manifest
    ├── Program.vb            ' application entry point
    ├── MainForm.vb           ' UI + timers + monitor wiring
    ├── MainForm.Designer.vb  ' designer-generated UI layout
    ├── DriveReadMonitor.vb   ' wraps the LogicalDisk performance counters
    ├── DotIndicatorControl.vb' owner-drawn circular dot
    └── README.md
```
