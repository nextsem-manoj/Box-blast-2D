Add-Type -AssemblyName System.Drawing
$bmp = New-Object System.Drawing.Bitmap("c:\Users\nextsem\Box-blast-2D\Assets\Resources\AdventureMap_Option1.png")

# Let's inspect the column x=384 from y=1100 to y=1300
Write-Output "Sampling center column X=384:"
for ($y = 1150; $y -le 1280; $y += 5) {
    $c = $bmp.GetPixel(384, $y)
    $uiY = 688 - $y
    Write-Output "Y_pixel=$y -> UI_Y=$uiY | RGB=($($c.R), $($c.G), $($c.B))"
}

$bmp.Dispose()
