Add-Type -AssemblyName System.Drawing
$bmp = New-Object System.Drawing.Bitmap("c:\Users\nextsem\Box-blast-2D\Assets\Resources\AdventureMap_Option1.png")

# Find the brown banner. The banner is near the bottom.
# Let's inspect vertical slice at X = 384 (center) from Y = 1000 to 1370
for ($y = 1050; $y -lt 1370; $y += 10) {
    $c = $bmp.GetPixel(384, $y)
    $uiY = 688 - $y
    Write-Output "Y_pixel=$y (uiY=$uiY) -> RGB=($($c.R), $($c.G), $($c.B))"
}

$bmp.Dispose()
