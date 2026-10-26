Add-Type -AssemblyName System.Drawing
$bmp = New-Object System.Drawing.Bitmap("c:\Users\nextsem\Box-blast-2D\Assets\Resources\AdventureMap_Option1.png")

$pts = @(
    @{ lvl = 1;  x =  150; y = -405 },
    @{ lvl = 2;  x =   50; y = -388 },
    @{ lvl = 3;  x =  -55; y = -368 },
    @{ lvl = 4;  x = -155; y = -350 },
    @{ lvl = 5;  x = -192; y = -315 },
    @{ lvl = 6;  x = -175; y = -270 },
    @{ lvl = 7;  x =  -70; y = -245 },
    @{ lvl = 8;  x =   40; y = -220 },
    @{ lvl = 9;  x =  145; y = -195 },
    @{ lvl = 10; x =  202; y = -155 },
    @{ lvl = 11; x =   70; y = -100 },
    @{ lvl = 12; x =  -50; y =  -80 },
    @{ lvl = 13; x = -145; y =  -65 },
    @{ lvl = 14; x = -182; y =  -25 },
    @{ lvl = 15; x = -155; y =   18 },
    @{ lvl = 16; x =   -5; y =   40 },
    @{ lvl = 17; x =  125; y =   65 },
    @{ lvl = 18; x =  166; y =   95 },
    @{ lvl = 19; x =   30; y =  135 },
    @{ lvl = 20; x = -120; y =  175 }
)

Write-Output "Testing points on golden road:"
foreach ($p in $pts) {
    $px = [int]($p.x + 384)
    $py = [int](688 - $p.y)
    $c = $bmp.GetPixel($px, $py)
    $isGold = ($c.R -gt 150 -and $c.G -gt 120 -and $c.B -lt 140)
    Write-Output "Node $($p.lvl) at ($($p.x), $($p.y)) -> Px=($px, $py) RGB=($($c.R), $($c.G), $($c.B)) Gold=$isGold"
}

# Also test Chest 1 and Chest 2
Write-Output "`nTesting Chests:"
$chests = @(
    @{ name = "Chest 1"; x = 155; y = -85 },
    @{ name = "Chest 2"; x = -30; y = 260 }
)
foreach ($ch in $chests) {
    $px = [int]($ch.x + 384)
    $py = [int](688 - $ch.y)
    $c = $bmp.GetPixel($px, $py)
    Write-Output "$($ch.name) at ($($ch.x), $($ch.y)) -> Px=($px, $py) RGB=($($c.R), $($c.G), $($c.B))"
}

$bmp.Dispose()
