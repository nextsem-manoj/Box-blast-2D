Add-Type -AssemblyName System.Drawing
$bmp = New-Object System.Drawing.Bitmap("c:\Users\nextsem\Box-blast-2D\Assets\Resources\AdventureMap_Option1.png")

# Let's inspect the upper road from y=400 to y=600 in steps of 10
Write-Output "Upper Road Slices (Y_px from 600 down to 350):"
for ($y = 600; $y -ge 350; $y -= 15) {
    $goldX = @()
    for ($x = 100; $x -lt 668; $x += 2) {
        $p = $bmp.GetPixel($x, $y)
        if ($p.R -gt 190 -and $p.G -gt 150 -and $p.B -lt 120) {
            $goldX += $x
        }
    }
    if ($goldX.Count -gt 3) {
        $avgX = ($goldX | Measure-Object -Average).Average
        $uiX = [math]::Round($avgX - 384, 1)
        $uiY = [math]::Round(688 - $y, 1)
        Write-Output "Y_px=$y -> UI=( $uiX, $uiY ) width=$($goldX.Count*2)"
    }
}

$bmp.Dispose()
