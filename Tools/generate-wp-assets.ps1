param(
    [string]$Out = 'C:\Users\asafe\Downloads\MSN Money Survival Simulator\WindowsPhone8\Assets'
)

Add-Type -AssemblyName System.Drawing

function New-Bitmap([int]$w, [int]$h) {
    $bmp = New-Object System.Drawing.Bitmap($w, $h)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.Clear([System.Drawing.Color]::FromArgb(255, 11, 16, 21))
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    return @($bmp, $g)
}

function Save-Png($bmp, [string]$path) {
    $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
}

# Application icon (62x62) and tile background (173x173)
foreach ($spec in @(@(62, 'ApplicationIcon.png'), @(173, 'Background.png'))) {
    $size = $spec[0]; $name = $spec[1]
    $pair = New-Bitmap $size $size
    $bmp = $pair[0]; $g = $pair[1]
    $brush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 0, 188, 212))
    $g.FillEllipse($brush, [int]($size * 0.15), [int]($size * 0.15), [int]($size * 0.70), [int]($size * 0.70))
    $dark = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 11, 16, 21))
    $g.FillEllipse($dark, [int]($size * 0.34), [int]($size * 0.30), [int]($size * 0.32), [int]($size * 0.10))
    $g.FillEllipse($dark, [int]($size * 0.38), [int]($size * 0.46), [int]($size * 0.24), [int]($size * 0.24))
    Save-Png $bmp (Join-Path $Out $name)
    $g.Dispose(); $bmp.Dispose()
}

# Splash screen 480x800
$pair = New-Bitmap 480 800
$bmp = $pair[0]; $g = $pair[1]
$font = New-Object System.Drawing.Font('Segoe UI', 26, [System.Drawing.FontStyle]::Bold)
$brush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 242, 247, 250))
$g.DrawString('MSN MONEY', $font, $brush, 52, 330)
$accent = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 0, 188, 212))
$g.DrawString('SURVIVAL SIMULATOR', $font, $accent, 52, 370)
$bmp.Save((Join-Path $Out 'SplashScreenImage.jpg'), [System.Drawing.Imaging.ImageFormat]::Jpeg)
$g.Dispose(); $bmp.Dispose(); $font.Dispose(); $brush.Dispose(); $accent.Dispose()

# Application bar icons 48x48: transparent with white glyphs
$iconDir = Join-Path $Out 'icons'
$white = [System.Drawing.Color]::FromArgb(255, 255, 255, 255)
$pen = New-Object System.Drawing.Pen($white, 3)
$solid = New-Object System.Drawing.SolidBrush($white)

function New-Icon([string]$name, [scriptblock]$draw) {
    $b = New-Object System.Drawing.Bitmap(48, 48)
    $g = [System.Drawing.Graphics]::FromImage($b)
    $g.Clear([System.Drawing.Color]::Transparent)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    & $draw $g
    $b.Save((Join-Path $iconDir $name), [System.Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose(); $b.Dispose()
}

New-Icon 'arena.png' { param($g)
    $g.FillRectangle($solid, 8, 22, 32, 18)
    $p = New-Object System.Drawing.Pen($white, 4)
    $g.DrawLine($p, 6, 22, 24, 7)
    $g.DrawLine($p, 24, 7, 42, 22)
    $g.FillRectangle($solid, 21, 30, 6, 10)
}
New-Icon 'shop.png' { param($g)
    $g.DrawRectangle($pen, 9, 15, 30, 27)
    $g.DrawArc($pen, 16, 6, 16, 18, 180, 180)
}
New-Icon 'stats.png' { param($g)
    $p = New-Object System.Drawing.Pen($white, 4)
    $g.DrawLine($p, 7, 40, 41, 40)
    $g.DrawLine($p, 7, 40, 7, 8)
    $g.DrawLine($p, 12, 32, 20, 24)
    $g.DrawLine($p, 20, 24, 28, 29)
    $g.DrawLine($p, 28, 29, 39, 12)
}
New-Icon 'trophies.png' { param($g)
    $g.FillRectangle($solid, 18, 8, 12, 16)
    $g.DrawArc($pen, 10, 14, 28, 20, 0, 180)
    $g.FillRectangle($solid, 21, 30, 6, 8)
    $g.FillRectangle($solid, 14, 38, 20, 4)
}
New-Icon 'settings.png' { param($g)
    $g.DrawEllipse($pen, 18, 18, 12, 12)
    $p = New-Object System.Drawing.Pen($white, 3)
    foreach ($a in 0,45,90,135,180,225,270,315) {
        $rad = $a * [math]::PI / 180
        $x1 = 24 + 10 * [math]::Cos($rad); $y1 = 24 + 10 * [math]::Sin($rad)
        $x2 = 24 + 16 * [math]::Cos($rad); $y2 = 24 + 16 * [math]::Sin($rad)
        $g.DrawLine($p, [int]$x1, [int]$y1, [int]$x2, [int]$y2)
    }
}
New-Icon 'sound.png' { param($g)
    $p = New-Object System.Drawing.Pen($white, 3)
    $g.DrawLine($p, 8, 20, 16, 20)
    $g.DrawLine($p, 16, 20, 25, 12)
    $g.DrawLine($p, 25, 12, 25, 36)
    $g.DrawLine($p, 25, 36, 16, 28)
    $g.DrawLine($p, 16, 28, 8, 28)
    $g.DrawLine($p, 31, 18, 40, 30)
    $g.DrawLine($p, 40, 18, 31, 30)
}

$pen.Dispose(); $solid.Dispose()
Write-Output 'WP8 assets generated.'
