# Generates the game's tiny WAV effects and Windows 8.1 tile/splash artwork.
# Run with: powershell -ExecutionPolicy Bypass -File Tools\generate-assets.ps1
param(
    [string]$AssetsRoot = (Join-Path $PSScriptRoot '..\MSN Money Survival Simulator\Assets')
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

function Write-Wav {
    param([string]$Path, [double[]]$Samples, [int]$Rate = 22050)
    $stream = New-Object System.IO.MemoryStream
    $writer = New-Object System.IO.BinaryWriter($stream)
    $dataLength = $Samples.Count * 2
    $writer.Write([char[]]'RIFF'); $writer.Write([int](36 + $dataLength)); $writer.Write([char[]]'WAVE')
    $writer.Write([char[]]'fmt '); $writer.Write([int]16); $writer.Write([int16]1)
    $writer.Write([int16]1); $writer.Write([int]$Rate); $writer.Write([int]($Rate * 2))
    $writer.Write([int16]2); $writer.Write([int16]16)
    $writer.Write([char[]]'data'); $writer.Write([int]$dataLength)
    foreach ($sample in $Samples) {
        $value = [Math]::Max(-1, [Math]::Min(1, $sample))
        $writer.Write([int16][Math]::Round($value * 32767))
    }
    $writer.Flush()
    [IO.File]::WriteAllBytes($Path, $stream.ToArray())
    $writer.Dispose(); $stream.Dispose()
}

function New-Tone {
    param([double]$Duration, [double]$FromHz, [double]$ToHz = 0, [double]$Volume = 0.25)
    $rate = 22050
    $count = [int]($Duration * $rate)
    $result = New-Object 'double[]' $count
    $phase = 0.0
    for ($i = 0; $i -lt $count; $i++) {
        $t = $i / [double]$rate
        $progress = $t / $Duration
        $hz = if ($ToHz -gt 0) { $FromHz + ($ToHz - $FromHz) * $progress } else { $FromHz }
        $phase += 2 * [Math]::PI * $hz / $rate
        $envelope = [Math]::Pow(1 - $progress, 1.6)
        $result[$i] = [Math]::Sin($phase) * $envelope * $Volume
    }
    return ,$result
}

$sfxDir = Join-Path $AssetsRoot 'sfx'
New-Item -ItemType Directory -Path $sfxDir -Force | Out-Null

Write-Wav (Join-Path $sfxDir 'click.wav')  (New-Tone 0.05 900 0 0.22)
Write-Wav (Join-Path $sfxDir 'combo.wav')  (New-Tone 0.08 1320 0 0.20)
Write-Wav (Join-Path $sfxDir 'buy.wav')    (New-Tone 0.13 520 980 0.22)
Write-Wav (Join-Path $sfxDir 'kill.wav')   (New-Tone 0.42 700 120 0.28)
Write-Wav (Join-Path $sfxDir 'unlock.wav') (New-Tone 0.36 620 1500 0.20)

function New-Bitmap {
    param([int]$Width, [int]$Height, [string]$File)
    $bmp = New-Object System.Drawing.Bitmap($Width, $Height)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
    $g.Clear([System.Drawing.Color]::FromArgb(11, 16, 21))
    return @{ Bitmap = $bmp; Graphics = $g; File = $File }
}

function Save-And-Duplicate {
    param($Ctx)
    $dir = Split-Path $Ctx.File -Parent
    $name = Split-Path $Ctx.File -Leaf
    $base = [IO.Path]::GetFileNameWithoutExtension($name)
    $ext = [IO.Path]::GetExtension($name)
    $Ctx.Bitmap.Save($Ctx.File, [System.Drawing.Imaging.ImageFormat]::Png)
    $scaleName = if ($name -like '*.scale-100.png') { $name } else { "$base.scale-100$ext" }
    $Ctx.Bitmap.Save((Join-Path $dir $scaleName), [System.Drawing.Imaging.ImageFormat]::Png)
    $Ctx.Graphics.Dispose(); $Ctx.Bitmap.Dispose()
}

$teal = [System.Drawing.Color]::FromArgb(0, 188, 212)
$warm = [System.Drawing.Color]::FromArgb(255, 179, 0)
$white = [System.Drawing.Color]::White

# Square logo
$c = New-Bitmap 150 150 (Join-Path $AssetsRoot 'Logo.scale-100.png')
$c.Graphics.FillEllipse((New-Object System.Drawing.SolidBrush($teal)), 15, 15, 120, 120)
$c.Graphics.DrawString('$', (New-Object System.Drawing.Font('Segoe UI', 78, [System.Drawing.FontStyle]::Bold)),
    (New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(11,16,21))), 40, 8)
Save-And-Duplicate $c

# Small / store logo
$c = New-Bitmap 30 30 (Join-Path $AssetsRoot 'SmallLogo.scale-100.png')
$c.Graphics.FillRectangle((New-Object System.Drawing.SolidBrush($teal)), 0, 0, 30, 30)
$c.Graphics.DrawString('$', (New-Object System.Drawing.Font('Segoe UI', 22, [System.Drawing.FontStyle]::Bold)),
    (New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(11,16,21))), 7, 0)
Save-And-Duplicate $c

$c = New-Bitmap 50 50 (Join-Path $AssetsRoot 'StoreLogo.scale-100.png')
$c.Graphics.FillRectangle((New-Object System.Drawing.SolidBrush($teal)), 0, 0, 50, 50)
$c.Graphics.DrawString('$', (New-Object System.Drawing.Font('Segoe UI', 36, [System.Drawing.FontStyle]::Bold)),
    (New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(11,16,21))), 11, 1)
Save-And-Duplicate $c

# Splash screen
$c = New-Bitmap 620 300 (Join-Path $AssetsRoot 'SplashScreen.scale-100.png')
$c.Graphics.FillEllipse((New-Object System.Drawing.SolidBrush($teal)), 40, 70, 150, 150)
$c.Graphics.DrawString('$', (New-Object System.Drawing.Font('Segoe UI', 100, [System.Drawing.FontStyle]::Bold)),
    (New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(11,16,21))), 72, 62)
$c.Graphics.DrawString('MSN MONEY', (New-Object System.Drawing.Font('Segoe UI', 34, [System.Drawing.FontStyle]::Bold)),
    (New-Object System.Drawing.SolidBrush($white)), 220, 118)
$c.Graphics.DrawString('SURVIVAL SIMULATOR', (New-Object System.Drawing.Font('Segoe UI', 17, [System.Drawing.FontStyle]::Regular)),
    (New-Object System.Drawing.SolidBrush($warm)), 222, 162)
Save-And-Duplicate $c

Write-Host 'Assets generated in:' $AssetsRoot
