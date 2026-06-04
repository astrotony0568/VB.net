# Drive Read Indicator — Delivery Notes

A small **VB.NET WinForms (.NET 9)** application for **Windows 11** that shows a
blinking dot whenever the selected logical drive is being read. Intended for use
alongside Windows Media Player so you can see when the music drive is actively
being read during playback.

This package is **tested source code** (no installer — per the agreed scope).

---

## 1. What is included

```
DriveReadIndicator.sln                 Visual Studio solution
DriveReadIndicator/                     application source project
    DriveReadIndicator.vbproj
    app.manifest
    Program.vb                          entry point
    MainForm.vb                         UI + timers + monitor wiring
    MainForm.Designer.vb                designer layout
    DriveReadMonitor.vb                 LogicalDisk performance-counter wrapper
    DotIndicatorControl.vb              owner-drawn circular dot
    README.md                           build / change-drive / test / limitations
TEST_NOTES.md                           manual test matrix (T01–T12) + results
TestEvidence/                           screenshots captured during testing
TestMedia/                              royalty-free synthetic .mp3 / .flac for testing
```

Build output (`bin/`, `obj/`) is intentionally not included — it is regenerated
when you build.

---

## 2. How to build and run

Requires **Windows 11**, the **.NET 9 SDK**, and Visual Studio 2022 (17.8+) with
the .NET desktop workload, *or* the `dotnet` CLI.

```powershell
dotnet build -c Release
dotnet run --project DriveReadIndicator
```

Or open `DriveReadIndicator.sln` in Visual Studio and press F5.
Full build instructions are in `DriveReadIndicator/README.md`.

---

## 3. How to choose the monitored drive

Edit one clearly-marked constant near the top of `MainForm.vb`:

```vb
Private Const TargetDriveInstance As String = "E:"
```

It accepts `E`, `E:` or `E:\`. It is currently set to `E:` — change it to your
music drive (for example `D:`).

---

## 4. How it behaves (please read)

The dot blinks **while reads are detected on the selected drive**, and turns off
when no reads are detected. A few points about what that looks like in practice:

- **It indicates physical disk reads, not "is a song playing".** While a track
  plays, Windows usually loads the file into RAM and plays from there, refilling
  its buffer from the disk only every few seconds. So the dot typically **blinks
  in short bursts** (at each buffer refill) rather than staying lit continuously.
  This is normal Windows behaviour, not a fault.

- **Cached files may not blink.** If the same/small file has just been played, or
  was recently read, Windows serves it from cache with **no disk read at all**, so
  the dot will not blink even though the music is playing. Testing a fresh or
  larger file shows the effect clearly. (See `TEST_NOTES.md` for measured
  examples and `TestEvidence/` for screenshots.)

- **It reports all reads on the drive, not only Windows Media Player.** Anything
  reading the drive (Explorer, antivirus, indexing) will also light the dot.
  This is by design — the requirement is drive read activity, not WMP-specific.

If you would prefer the dot to appear more continuous during playback, two values
in `MainForm.vb` can be tuned without any code changes elsewhere:

| Constant                      | Effect of increasing / decreasing |
|-------------------------------|-----------------------------------|
| `ActivityHoldMs` (500)        | Larger = the dot keeps blinking longer between read bursts, looking more continuous. |
| `ReadThresholdBytesPerSecond` (1024) | Lower = catches smaller reads, so the dot lights more readily. |

---

## 5. Testing summary

- Builds clean on Windows 11 + .NET 9 (Release, 0 warnings / 0 errors).
- Manual test matrix T01–T12 executed; see `TEST_NOTES.md` and `TestEvidence/`.
- Tested with actual `.flac` and `.mp3` playback through Windows Media Player,
  including cache-evicted runs that demonstrate the dot activating on real reads.
- **Not yet tested on physical USB hardware** (matrix items T10/T11): the code
  handles a missing/removed drive gracefully and retries every ~3 seconds, but
  the unplug/replug cycle was verified by code inspection rather than on a real
  USB stick. Please let me know if you would like this verified on hardware.

---

## 6. What was intentionally not built (per agreed scope)

No installer, no drive-selection dropdown, no history/logging, no charts/export,
no database, no background service, no multi-drive dashboard, no per-file or
WMP-only monitoring. Any of these can be added if you would like — just ask.
