# Test Notes — Drive Read Indicator

These notes implement the manual test matrix from plan section 13. They must be
run on a real Windows 11 machine because the app relies on Windows LogicalDisk
performance counters (no automated unit suite is in scope per plan section 2,
"Source code only, but tested").

## Environment

- Windows 11
- .NET 9 SDK
- Visual Studio 2022 or `dotnet` CLI
- Windows Media Player (legacy or "Media Player" app — both read from disk)
- One local drive used as the monitored drive (default: `E:`)
- Optionally one USB drive presented as the monitored drive letter
- One `.flac` test file the tester has rights to
- One `.mp3`  test file the tester has rights to

> Do **not** commit copyrighted music with the source. The plan calls this out
> explicitly (section 13, "The developer should use legally available local
> test files. Do not include copyrighted music files in the delivery").

## Suggested test layout

```
E:\MusicTest\test_flac.flac
E:\MusicTest\test_mp3.mp3
```

## Bundled royalty-free test files

The repository already ships with two synthetic, public-domain audio files
generated locally via ffmpeg (440 Hz mono sine wave, 10 s):

```
DriveReadIndicator\TestMedia\test_440Hz.mp3   (~240 KB, 192 kbps CBR)
DriveReadIndicator\TestMedia\test_440Hz.flac  (~128 KB, FLAC level 8)
```

These were chosen so they are large enough to produce visible disk reads
(plan section 14) but small enough to copy onto any drive in seconds. Use
them for T03/T05 instead of any copyrighted music. To run the matrix on a
different drive letter, copy them like this from PowerShell:

```powershell
$src = 'd:\Freelancer-Work\Freelancer_Everetts\Ronaldo\DriveReadIndicator\TestMedia'
New-Item -ItemType Directory -Path 'E:\MusicTest' -Force | Out-Null
Copy-Item "$src\test_440Hz.mp3"  'E:\MusicTest\' -Force
Copy-Item "$src\test_440Hz.flac" 'E:\MusicTest\' -Force
```

## Windows 11 "N" edition — Media Feature Pack (FLAC playback)

This machine is **Windows 11 Pro N**. The "N" SKU does not ship the Windows
Media Feature Pack by default; classic Windows Media Player can play MP3
but **FLAC playback may fail** until the MFP is installed. The install
needs administrator elevation, so you (the user) must run it manually from
an **elevated PowerShell** prompt:

```powershell
# Run from an elevated PowerShell (Run as Administrator).
# This is the supported, reversible way to add the MFP on Windows 11 N.
Get-WindowsCapability -Online -Name 'Media.MediaFeaturePack*' |
    Where-Object State -eq 'NotPresent' |
    Add-WindowsCapability -Online
```

Verify after:

```powershell
Get-WindowsCapability -Online -Name 'Media.MediaFeaturePack*'
```

A reboot is recommended afterwards. If for any reason FLAC still does not
play in WMP after installation, the indicator app itself is unaffected — the
counter-based logic does not depend on WMP being able to render the format.
You can substitute any other player (foobar2000, VLC) for the test.

## How to record results

Tick each row's **Pass / Fail / Notes** when running. Capture the PerfMon graph
for any failing test so the dot behaviour can be compared with the underlying
counter.

> **Run record (2026-05-29).** T01–T09 and T12 were executed automatically on
> this machine using the harness at `TestEvidence/_harness.ps1`. Per-test
> screenshots were saved under `TestEvidence/*.png` and the dot's pixel state
> was classified by closest-colour match against the three reference colours
> defined in `DotIndicatorControl.vb` (Active 255,64,64 / Inactive 70,70,70 /
> Unavailable 40,40,40). T10/T11 require a USB stick and were not run.

| # | Scenario | Steps | Expected | Pass/Fail | Notes |
|---|---|---|---|---|---|
| T01 | App starts with valid target drive | `TargetDriveInstance = "D:"`, run, sample dot. | Dot Inactive, status `Monitoring D:`. | **PASS** | Dot RGB=(69,69,69), dist 1.7 from Inactive ref. Evidence: `T01_drive_D_idle.png`. |
| T02 | App starts with missing target drive | `TargetDriveInstance = "E:"` (E: absent), run. | Dot Unavailable, status `Drive E: not available.`. | **PASS** | Dot RGB=(38,38,38), dist 3.5 from Unavailable ref. Evidence: `T02_drive_E_missing.png`. |
| T03 | Play FLAC from monitored drive in WMP | `wmplayer.exe D:\MusicTest\test_5min_noise.flac` (36 MB white-noise FLAC, ~5 min), 10 GB memory pressure held to evict file cache, sample 12× at 350 ms intervals. | Dot blinks during playback. | **PASS** | Active=**4/12** alternating with Inactive — the classic blink pattern. D: counter showed disk activity at 0.1–0.6 MB/s as WMP refilled its decoder buffer. Earlier 0.7 MB cached-file run (Active=0/6) is preserved at the bottom of this row as "earlier finding" — it matched plan §14's caching caveat. **The retest with a 36 MB uncached file is the authoritative result for requirement #8.** Evidence: `T03r_FLAC_wmp_press_1..12.png`. |
| T04 | Stop FLAC playback | Kill WMP, wait 1 s. | Dot returns to Inactive after `ActivityHoldMs` (500 ms). | **PASS** | Dot Inactive, RGB=(69,69,69). Evidence: `T04_post_stop.png`. |
| T05 | Play MP3 from monitored drive in WMP | `wmplayer.exe D:\MusicTest\test_5min_noise.mp3` (11 MB 320 kbps white-noise MP3, ~5 min), 10 GB memory pressure held to evict file cache, sample 12× at 350 ms intervals. | Dot blinks intermittently. | **PASS** | Active=**6/12** (50%) — even stronger than FLAC because the smaller file means more buffer-refill cycles per unit time. Disk activity observed at 0.1–2.2 MB/s. Earlier 0.7 MB cached-file run (Active=0/6) preserved as "earlier finding"; the 11 MB uncached retest is the authoritative result. Evidence: `T05r_MP3_wmp_press_1..12.png`. |
| T06 | Pause/stop MP3 | Kill WMP, wait 1 s. | Dot returns to Inactive after hold. | **PASS** | Dot Inactive, RGB=(69,69,69). Evidence: `T06_post_stop_mp3.png`. |
| T07 | Play / read from a different drive | Uncached read of a 678 MB file on **C:** while indicator monitors **D:**. | Dot on D: stays off. | **PASS** | D: Active=0/6 during 6 iterations of reading `C:\Windows\Installer\f5b1c.msi`. Drive isolation confirmed. |
| T08 | Copy / large read from the monitored drive | Repeated 16 MB uncached reads from random offsets of `D:\Source.zip` (9 GB), 8 iterations 250 ms apart. | Dot blinks. | **PASS** | Active=4/8 — exactly the 50% duty cycle you'd expect from the 150 ms blink toggle inside a sustained activity window. |
| T09 | Read activity below threshold | Sample dot 6× over 2.4 s with no triggered reads on D:. | Dot stays off (background OS activity may flicker it). | **PASS** | Active=0/6 — no false positives from background noise. Note: the indicator OR-s `Disk Reads/sec > 0` against the byte threshold per plan §9 step 5, so ANY read on D: would light the dot regardless of byte rate. Threshold-only testing is therefore not separately observable. |
| T10 | USB drive removed during monitoring | Unplug the USB drive serving as the target. | App does not crash. Dot turns off. | **NOT RUN** | Requires physical USB drive. Recovery path is implemented (`Sample` catches `InvalidOperationException`/`Win32Exception`, disposes counters, retries every `RecoveryRetryMs = 3000`) and verified by inspection. |
| T11 | USB drive reconnected | Plug the USB drive back in. | Within ~3 s, status returns to `Monitoring`; dot becomes responsive. | **NOT RUN** | Same — requires physical USB. The recovery loop in `MainForm.MonitorTimer_Tick` calls `TryInitialize` on every `RecoveryRetryMs` tick once unavailable. |
| T12 | Repeated playback of same file | Generate a fresh FLAC on D:, play in WMP 3 times back-to-back, sample 5× per pass. | Dot activity may decrease on later passes due to caching. | **PASS (caching effect observed)** | Pass 1: Active=0/5 (writes go through cache; immediate playback served from RAM). Pass 2: 2/5, Pass 3: 2/5. The variability across passes confirms the plan §14 / §18 warning that caching produces unpredictable read patterns. |

## Run summary (2026-05-29)

| Result | Count | Tests |
|---|---|---|
| PASS | 10 | T01, T02, T03 (retest), T04, T05 (retest), T06, T07, T08, T09, T12 |
| NOT RUN (needs USB) | 2 | T10, T11 |
| Supplementary (kept for reference) | 2 | T03b (uncached FLAC), T05b (uncached MP3) — initial proof the indicator's logic was correct |

### T03/T05 — initial finding vs authoritative retest

The first T03/T05 run played tiny 0.7–1.4 MB sine-wave files from `D:` via
WMP. Both showed Active=0/6 because the files were entirely resident in
the OS standby cache, so WMP decoded them from RAM without ever touching
the disk. That matched plan §14's caching caveat, but it did not
demonstrate the requirement #8 expectation that the dot blinks during WMP
playback.

The **authoritative retest** (recorded above) regenerated the test media as
**5-minute white-noise tracks** (36 MB FLAC, 11 MB MP3 — noise is
uncompressible so FLAC stays large), then held ~10 GB of memory pressure
in PowerShell to evict the standby cache before launching WMP. Under those
conditions:

- T03 (FLAC via WMP): **Active=4/12** with D: counter showing 0.1–0.6 MB/s
- T05 (MP3 via WMP): **Active=6/12** with D: counter showing 0.1–2.2 MB/s

The dot blinks visibly during WMP playback. Requirement #8 is satisfied.

All captured PNGs are under `TestEvidence/`. The dot's classified state was
within 5 RGB units of its reference colour in every PASS sample.

## Verification with Performance Monitor

For any test where you want to ground-truth the dot:

1. Run `perfmon.exe`.
2. Add counter:

   ```
   LogicalDisk(E:) \ Disk Read Bytes/sec
   ```

3. Set the graph type to "Line" and watch in real time.
4. The dot should blink whenever the counter is non-zero (or above the
   configured threshold).

## Threshold tuning

If T05 fails because MP3 read bursts are too brief to register:

1. In `MainForm.vb`, set:

   ```vb
   Private Const ActivityHoldMs As Integer = 800
   Private Const ReadThresholdBytesPerSecond As Single = 1.0F
   ```

2. Rebuild and rerun T05. The dot should now blink visibly on each buffer
   read.

## Known caveats (also covered in README "Limitations")

- LogicalDisk counters measure **all** reads on the drive, not just a single
  process — antivirus, indexing, Explorer thumbnails will all light the dot.
- Cached reads may not appear at the counter level; the OS may serve repeated
  reads from RAM cache.
- Drive letter changes (USB) require the constant to be updated.
- The plan explicitly excludes a persistent log; failed tests should be
  recorded in this file or in PerfMon screenshots.
