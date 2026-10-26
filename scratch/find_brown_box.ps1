Add-Type -AssemblyName System.Drawing
$bmp = New-Object System.Drawing.Bitmap("c:\Users\nextsem\Box-blast-2D\Assets\Resources\AdventureMap_Option1.png")

$boxPixels = @()
for ($y = 900; $y -lt 1350; $y++) {
    for ($x = 100; $x -lt 668; $x++) {
        $p = $bmp.GetPixel($x, $y)
        if ($p.R -gt 90 -and $p.R -lt 150 -and $p.G -gt 45 -and $p.G -lt 85 -and $p.B -gt 10 -and $p.B -lt 40) {
            $boxPixels += [PSCustomObject]@{ X = $x; Y = $y }
        }
    }
}

if ($boxPixels.Count -gt 0) {
    $minX = ($boxPixels | Measure-Object -Property X -Minimum).Minimum
    $maxX = ($boxPixels | Measure-Object -Property X -Maximum).Maximum
    $minY = ($boxPixels | Measure-Object -Property Y -Minimum).Minimum
    $maxY = ($boxPixels | Measure-Object -Property Y -Maximum).Maximum
    $centerX = ($minX + $maxX) / 2
    $centerY = ($minY + $maxY) / 2
    $uiX = $centerX - 384
    $uiY = 688 - $centerY
    $w = $maxX - $minX
    $h = $maxY - $minY
    Write-Output "Brown Banner Inner Box: X=[$minX, $maxX], Y=[$minY, $maxY], Size=($w, $h)"
    Write-Output "Center UI coordinate: uiX=$uiX, uiY=$uiY"
}

$bmp.Dispose()
