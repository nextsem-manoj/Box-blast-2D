Add-Type -AssemblyName System.Drawing
$bmp = New-Object System.Drawing.Bitmap("c:\Users\nextsem\Box-blast-2D\Assets\Resources\AdventureMap_Option1.png")

# Let's inspect the road centerline from y=1100 down to y=350 in steps of 20
$lines = @()
for ($y = 1100; $y -ge 360; $y -= 10) {
    $goldX = @()
    for ($x = 100; $x -lt 668; $x += 2) {
        $p = $bmp.GetPixel($x, $y)
        # Gold ribbon center core: High R & G, low B
        if ($p.R -gt 200 -and $p.G -gt 160 -and $p.B -lt 110) {
            $goldX += $x
        }
    }
    if ($goldX.Count -gt 5) {
        $minX = ($goldX | Measure-Object -Minimum).Minimum
        $maxX = ($goldX | Measure-Object -Maximum).Maximum
        $centerX = ($minX + $maxX) / 2
        $uiX = [math]::Round($centerX - 384, 1)
        $uiY = [math]::Round(688 - $y, 1)
        $w = $maxX - $minX
        $lines += [PSCustomObject]@{ Y_px = $y; UI_X = $uiX; UI_Y = $uiY; Width = $w }
    }
}

Write-Output "Road Centerline ($($lines.Count) slices):"
$lines | Format-Table -AutoSize

$bmp.Dispose()
