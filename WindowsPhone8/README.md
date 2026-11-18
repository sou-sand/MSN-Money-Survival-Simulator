# Windows Phone 8.0+ edition

A native **Windows Phone Silverlight 8.0** port of the clicker. Because it targets 8.0,
the same XAP runs on Windows Phone 8.0, 8.1 and 10 Mobile. Built with the installed
Windows Phone 8.0 SDK and **MSBuild 12.0** (not the 8.1 Store MSBuild 14.0 pipeline).

## What's inside

- Persistent HUD + phone `ApplicationBar` (4 icon buttons, 6 menu items including sound).
- Five swipe-free views swapped into a `ContentControl`: Arena, Upgrades, Stats,
  Trophies, Settings. The game timer lives in the shell, so progress never resets.
- Full game parity with the 8.1 edition: combos, bosses, passive income, 10 upgrades,
  12 achievements, timeline reboot, offline income, sound and reduced-motion settings.
- Touch-sized controls, floating damage numbers, confetti, kill banner and toasts.
- Saves to `IsolatedStorageSettings`; saves on suspend and close.

## Build

```powershell
$msb = 'C:\Program Files (x86)\MSBuild\12.0\Bin\MSBuild.exe'
$prj = '.\MSN Money Survival Simulator WP8.csproj'

& $msb $prj /t:Rebuild /p:Configuration=Debug   /p:Platform=AnyCPU
& $msb $prj /t:Rebuild /p:Configuration=Release /p:Platform=AnyCPU
```

Output: `Bin\Debug\<name>.xap` and `Bin\Release\<name>.xap`.

Those builds are unsigned (`SignXap=false`), which some emulator deployment tools accept.
Signed copies are produced with the included developer certificate:

```powershell
$signer = 'C:\Program Files (x86)\Microsoft SDKs\Windows Phone\v8.0\Tools\XapSignTool\XapSignTool.exe'
$signtoolDir = 'C:\Program Files (x86)\Windows Kits\10\bin\10.0.19041.0\x86'
& $signer sign /SignTool $signtoolDir /f '.\Cert\wp8dev.pfx' /p 'msnwp8' '.\Bin\Release\MSN Money Survival Simulator WP8.xap'
```

Certificate password: `msnwp8`.

## Deploy

- **Emulator** — use `WP8-Emulator-Debug.xap` (signed) or `WP8-Emulator-Debug-unsigned.xap`
  via the Windows Phone Application Deployment tool (`XapDeploy`) or Visual Studio's
  Deploy action. Pick any WP8 emulator image (WVGA/WXGA/720p).
- **Phone** — use `WP8-Phone-Release.xap` after registering the device with the
  Windows Phone Developer Registration tool and installing `WP8-Developer.cer` on the
  phone (Settings → certificates / via the registration tool). If your device uses a
  developer certificate issued by your own account, re-sign the XAP with that certificate
  using `XapSignTool` and the PFX from your account.

The included `WP8-Developer.cer` / `.pfx` is a self-signed development certificate. It is
**not** a Store signing certificate and must be trusted on the target device/emulator.
If your deployment tool rejects it, use the `-unsigned.xap` variant and let the tool sign
with your own developer certificate.

All ready-to-deploy files are also copied to `..\LatestBuild\` with `WP8-` prefixes.
