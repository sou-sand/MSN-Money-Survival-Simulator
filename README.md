# MSN Money Survival Simulator

A clicker game built on top of the existing Windows 8.1 Store project. The original dark-humour
mascot (`Assets/assets/adolficon.png`) and the "terminate MSN Money" premise are kept; everything
else — pages, game engine, artwork, audio and persistence — is new.

# Download and play

Web: https://sou-sand.github.io/killmsnmoney

Download (W8.1+ | x32, x64, x86 and ARM): https://github.com/sou-sand/MSN-Money-Survival-Simulator/releases

## Three editions

### 1. Windows 8.1 Store app (the original project)

A real WinRT XAML app targeting `8.1` / OS `6.3`, using:

- **Top and bottom app bars** (`CommandBar` + `AppBarButton`) for navigation, sound and back.
- **Theme transitions**: `RepositionThemeTransition` in the grids. Page-level content transitions
  were deliberately removed: on Windows 8.1 an early `ContentThemeTransition`/`EntranceThemeTransition`
  can leave a page invisible (blank arena). Navigation waits for `Frame.Loaded` and falls back to a
  visible error dialog if a page fails.
- **Storyboard animations**: pulsing target ring, floating damage numbers, confetti on boss kills,
  fade-in/out kill banner.
- **A `Frame`-based shell**: the HUD/app bars persist while Arena, Upgrades, Stats, Trophies and
  Settings are injected as dynamic pages.
- **Tiles/splash art**, suspension-aware saving, offline income, generated WAV sound effects.

Build (works on this machine with the installed MSBuild 14.0 / Windows 8.1 SDK):

```powershell
& 'C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe' `
  '.\MSN Money Survival Simulator.sln' /t:Rebuild /p:Configuration=Release /p:Platform=x86
```

Ready-made packages:

- `MSN Money Survival Simulator\AppPackages\MSN Money Survival Simulator_1.0.0.0_x86_Test\...x86.appx`
- `MSN Money Survival Simulator\AppPackages\MSN Money Survival Simulator_1.0.0.0_x64_Test\...x64.appx`
- `MSN Money Survival Simulator\AppPackages\MSN Money Survival Simulator_1.0.0.0_ARM_Test\...ARM.appx`
- Debug packages under the matching `..._Debug_Test\` folders

A copy of every Release/Debug package and certificate, with simple names, is in `LatestBuild\`.

On a Windows 8.1 device, enable sideloading (or Developer Mode / an enterprise licence), install the
`temporary key` certificate from the `_Test` folder, then install the `.appx`.

### 2. Browser edition (runs on any Windows version)

Folder: `Web/`. Double-click `Web/index.html`, or serve it properly:

```powershell
cd '.\Web'
node serve.js 8777
# open http://localhost:8777
```

It mirrors the 8.1 game exactly: same upgrades, balance, bosses, combos, achievements, prestige,
offline income, app-bar navigation, page transitions and generated sound effects (Web Audio).
Progress is saved in `localStorage`.

The browser edition is **mobile friendly**: responsive layouts from 320 px phones through tablets
and desktop, a touch-sized bottom tab bar on small screens, safe-area support, 44 px+ tap targets,
no double-tap zoom on the mascot, and Web Audio unlocked on first touch.

### 3. Windows Phone 8.0+ edition (native Silverlight)

Folder: `WindowsPhone8/`. A real Windows Phone Silverlight 8.0 app, so the same XAP runs on
WP8.0, WP8.1 and 10 Mobile. Built with the WP8.0 SDK and MSBuild 12.0. Includes a persistent
HUD, phone ApplicationBar, five views, full game parity, isolated-storage saves, and signed or
unsigned XAPs for emulator and device deployment. See `WindowsPhone8/README.md` for build and
deployment steps. Ready-to-deploy packages are in `LatestBuild/` with `WP8-` prefixes.

## How to play

- Click the target to deal damage and earn cash. Clicks within 1.25 s build a combo up to **x3**.
- Cash also damages the current target; defeating it pays a bonus and spawns a stronger target.
- **Upgrades** add click damage, passive cash/second, or a permanent multiplicative boost.
- **Trophies** unlock automatically. **Stats** shows lifetime totals and a cash-flow chart.
- After **5 targets**, **Settings → Reboot Timeline** resets the run for a permanent **x1.3**
  multiplier per reboot. Trophies and lifetime stats survive.
- Sound and reduced-motion toggles live in Settings. Progress auto-saves every ~10 s, on suspend,
  and on exit; returning after a break grants 50% passive income (capped at 8 h).

## Regenerating assets

```powershell
powershell -ExecutionPolicy Bypass -File '.\Tools\generate-assets.ps1'
```

Recreates the tile/splash PNGs and the five WAV effects (`click`, `combo`, `buy`, `kill`, `unlock`).
The browser edition's copies live in `Web/assets/`.

