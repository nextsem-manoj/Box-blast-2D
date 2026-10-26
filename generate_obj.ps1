param(
    [string]$OutDir = "C:\Users\nextsem\Box-blast-2D-SVG\Assets\Models"
)

if (-not (Test-Path $OutDir)) {
    New-Item -ItemType Directory -Force -Path $OutDir | Out-Null
}

function Generate-RoundedCube {
    param([string]$FilePath, [double]$Size = 1.0, [double]$Bevel = 0.18, [int]$Subdiv = 6)

    $half = $Size * 0.5
    $inner = $half - $Bevel
    $verts = [System.Collections.Generic.List[string]]::new()
    $norms = [System.Collections.Generic.List[string]]::new()
    $uvs   = [System.Collections.Generic.List[string]]::new()
    $faces = [System.Collections.Generic.List[string]]::new()

    $normals = @(
        @(0.0, 0.0, 1.0), @(0.0, 0.0, -1.0),
        @(0.0, 1.0, 0.0), @(0.0, -1.0, 0.0),
        @(1.0, 0.0, 0.0), @(-1.0, 0.0, 0.0)
    )
    $rights = @(
        @(1.0, 0.0, 0.0), @(-1.0, 0.0, 0.0),
        @(1.0, 0.0, 0.0), @(1.0, 0.0, 0.0),
        @(0.0, 0.0, -1.0), @(0.0, 0.0, 1.0)
    )
    $ups = @(
        @(0.0, 1.0, 0.0), @(0.0, 1.0, 0.0),
        @(0.0, 0.0, -1.0), @(0.0, 0.0, 1.0),
        @(0.0, 1.0, 0.0), @(0.0, 1.0, 0.0)
    )

    $vertCount = 0

    for ($f = 0; $f -lt 6; $f++) {
        $baseIndex = $vertCount
        $fN = $normals[$f]
        $fR = $rights[$f]
        $fU = $ups[$f]

        for ($y = 0; $y -le $Subdiv; $y++) {
            $ty = ($y / [double]$Subdiv) * 2.0 - 1.0
            for ($x = 0; $x -le $Subdiv; $x++) {
                $tx = ($x / [double]$Subdiv) * 2.0 - 1.0

                $px = $fN[0] * $half + $fR[0] * ($tx * $half) + $fU[0] * ($ty * $half)
                $py = $fN[1] * $half + $fR[1] * ($tx * $half) + $fU[1] * ($ty * $half)
                $pz = $fN[2] * $half + $fR[2] * ($tx * $half) + $fU[2] * ($ty * $half)

                $cx = [Math]::Max(-$inner, [Math]::Min($inner, $px))
                $cy = [Math]::Max(-$inner, [Math]::Min($inner, $py))
                $cz = [Math]::Max(-$inner, [Math]::Min($inner, $pz))

                $dx = $px - $cx
                $dy = $py - $cy
                $dz = $pz - $cz
                $dist = [Math]::Sqrt($dx*$dx + $dy*$dy + $dz*$dz)

                if ($dist -gt 0.0001) {
                    $nx = $dx / $dist
                    $ny = $dy / $dist
                    $nz = $dz / $dist
                    $fx = $cx + $nx * $Bevel
                    $fy = $cy + $ny * $Bevel
                    $fz = $cz + $nz * $Bevel
                } else {
                    $nx = $fN[0]; $ny = $fN[1]; $nz = $fN[2]
                    $fx = $px; $fy = $py; $fz = $pz
                }

                $u = $x / [double]$Subdiv
                $v = $y / [double]$Subdiv

                $verts.Add(([string]::Format([System.Globalization.CultureInfo]::InvariantCulture, "v {0:F4} {1:F4} {2:F4}", $fx, $fy, $fz)))
                $norms.Add(([string]::Format([System.Globalization.CultureInfo]::InvariantCulture, "vn {0:F4} {1:F4} {2:F4}", $nx, $ny, $nz)))
                $uvs.Add(([string]::Format([System.Globalization.CultureInfo]::InvariantCulture, "vt {0:F4} {1:F4}", $u, $v)))
                $vertCount++
            }
        }

        $stride = $Subdiv + 1
        for ($y = 0; $y -lt $Subdiv; $y++) {
            for ($x = 0; $x -lt $Subdiv; $x++) {
                $i0 = $baseIndex + $y * $stride + $x + 1
                $i1 = $i0 + 1
                $i2 = $baseIndex + ($y + 1) * $stride + $x + 1
                $i3 = $i2 + 1

                $faces.Add(([string]::Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}", $i0, $i2, $i1)))
                $faces.Add(([string]::Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}", $i1, $i2, $i3)))
            }
        }
    }

    $sb = [System.Text.StringBuilder]::new()
    $sb.AppendLine("# Box Blast 3D Rounded Chamfered Cube") | Out-Null
    $sb.AppendLine("o Cube_Rounded_Beveled") | Out-Null
    foreach ($v in $verts) { $sb.AppendLine($v) | Out-Null }
    foreach ($n in $norms) { $sb.AppendLine($n) | Out-Null }
    foreach ($u in $uvs)   { $sb.AppendLine($u) | Out-Null }
    $sb.AppendLine("s 1") | Out-Null
    foreach ($f in $faces) { $sb.AppendLine($f) | Out-Null }

    [System.IO.File]::WriteAllText($FilePath, $sb.ToString(), [System.Text.Encoding]::UTF8)
    Write-Host "Created: $FilePath"
}

function Generate-Sphere {
    param([string]$FilePath, [double]$Radius = 0.5, [int]$LatSegs = 20, [int]$LonSegs = 24)

    $verts = [System.Collections.Generic.List[string]]::new()
    $norms = [System.Collections.Generic.List[string]]::new()
    $uvs   = [System.Collections.Generic.List[string]]::new()
    $faces = [System.Collections.Generic.List[string]]::new()

    for ($lat = 0; $lat -le $LatSegs; $lat++) {
        $v = $lat / [double]$LatSegs
        $theta = $v * [Math]::PI
        $sinTheta = [Math]::Sin($theta)
        $cosTheta = [Math]::Cos($theta)

        for ($lon = 0; $lon -le $LonSegs; $lon++) {
            $u = $lon / [double]$LonSegs
            $phi = $u * [Math]::PI * 2.0
            $sinPhi = [Math]::Sin($phi)
            $cosPhi = [Math]::Cos($phi)

            $nx = $sinTheta * $cosPhi
            $ny = $cosTheta
            $nz = $sinTheta * $sinPhi

            $px = $nx * $Radius
            $py = $ny * $Radius
            $pz = $nz * $Radius

            $verts.Add(([string]::Format([System.Globalization.CultureInfo]::InvariantCulture, "v {0:F4} {1:F4} {2:F4}", $px, $py, $pz)))
            $norms.Add(([string]::Format([System.Globalization.CultureInfo]::InvariantCulture, "vn {0:F4} {1:F4} {2:F4}", $nx, $ny, $nz)))
            $uvs.Add(([string]::Format([System.Globalization.CultureInfo]::InvariantCulture, "vt {0:F4} {1:F4}", $u, 1.0 - $v)))
        }
    }

    $stride = $LonSegs + 1
    for ($lat = 0; $lat -lt $LatSegs; $lat++) {
        for ($lon = 0; $lon -lt $LonSegs; $lon++) {
            $c = $lat * $stride + $lon + 1
            $n = $c + $stride

            $faces.Add(([string]::Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}", $c, $n, $c + 1)))
            $faces.Add(([string]::Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}", $c + 1, $n, $n + 1)))
        }
    }

    $sb = [System.Text.StringBuilder]::new()
    $sb.AppendLine("# Box Blast 3D Crystal Sphere") | Out-Null
    $sb.AppendLine("o Sphere_Crystal_Smooth") | Out-Null
    foreach ($v in $verts) { $sb.AppendLine($v) | Out-Null }
    foreach ($n in $norms) { $sb.AppendLine($n) | Out-Null }
    foreach ($u in $uvs)   { $sb.AppendLine($u) | Out-Null }
    $sb.AppendLine("s 1") | Out-Null
    foreach ($f in $faces) { $sb.AppendLine($f) | Out-Null }

    [System.IO.File]::WriteAllText($FilePath, $sb.ToString(), [System.Text.Encoding]::UTF8)
    Write-Host "Created: $FilePath"
}

function Generate-DiamondGem {
    param([string]$FilePath, [double]$Radius = 0.45, [double]$TableY = 0.25, [double]$CuletY = -0.35)

    $sb = [System.Text.StringBuilder]::new()
    $sb.AppendLine("# Box Blast 3D Faceted Diamond") | Out-Null
    $sb.AppendLine("o Gem_Diamond_Faceted") | Out-Null

    $numFacets = 8
    # Table center
    $sb.AppendLine([string]::Format([System.Globalization.CultureInfo]::InvariantCulture, "v 0.0000 {0:F4} 0.0000", $TableY))
    
    # Table ring (v 2..9)
    for ($i = 0; $i -lt $numFacets; $i++) {
        $ang = ($i * 360.0 / $numFacets) * ([Math]::PI / 180.0)
        $x = [Math]::Cos($ang) * $Radius * 0.55
        $z = [Math]::Sin($ang) * $Radius * 0.55
        $sb.AppendLine([string]::Format([System.Globalization.CultureInfo]::InvariantCulture, "v {0:F4} {1:F4} {2:F4}", $x, $TableY, $z))
    }

    # Girdle ring (v 10..17)
    for ($i = 0; $i -lt $numFacets; $i++) {
        $ang = (($i + 0.5) * 360.0 / $numFacets) * ([Math]::PI / 180.0)
        $x = [Math]::Cos($ang) * $Radius
        $z = [Math]::Sin($ang) * $Radius
        $sb.AppendLine([string]::Format([System.Globalization.CultureInfo]::InvariantCulture, "v {0:F4} 0.0000 {1:F4}", $x, $z))
    }

    # Culet tip (v 18)
    $sb.AppendLine([string]::Format([System.Globalization.CultureInfo]::InvariantCulture, "v 0.0000 {0:F4} 0.0000", $CuletY))

    $sb.AppendLine("s 1")

    # Table faces
    for ($i = 0; $i -lt $numFacets; $i++) {
        $v1 = 2 + $i
        $v2 = 2 + (($i + 1) % $numFacets)
        $sb.AppendLine("f 1 $v1 $v2")
    }

    # Crown facets
    for ($i = 0; $i -lt $numFacets; $i++) {
        $t1 = 2 + $i
        $t2 = 2 + (($i + 1) % $numFacets)
        $g = 10 + $i
        $prevG = 10 + (($i - 1 + $numFacets) % $numFacets)

        $sb.AppendLine("f $t1 $g $t2")
        $sb.AppendLine("f $t1 $prevG $g")
    }

    # Pavilion facets
    for ($i = 0; $i -lt $numFacets; $i++) {
        $g1 = 10 + $i
        $g2 = 10 + (($i + 1) % $numFacets)
        $sb.AppendLine("f 18 $g2 $g1")
    }

    [System.IO.File]::WriteAllText($FilePath, $sb.ToString(), [System.Text.Encoding]::UTF8)
    Write-Host "Created: $FilePath"
}

function Generate-BoardPedestal {
    param([string]$FilePath, [double]$Width = 6.4, [double]$Height = 0.5, [double]$Border = 0.45)

    $hw = $Width * 0.5
    $iw = $hw - $Border
    $recess = 0.15
    $baseH = -$Height

    $sb = [System.Text.StringBuilder]::new()
    $sb.AppendLine("# Box Blast 3D Board Pedestal") | Out-Null
    $sb.AppendLine("o Board_Pedestal_8x8") | Out-Null

    # Vertices
    # Outer top (1..4)
    $sb.AppendLine([string]::Format([System.Globalization.CultureInfo]::InvariantCulture, "v {0:F4} 0.0000 {0:F4}", -$hw))
    $sb.AppendLine([string]::Format([System.Globalization.CultureInfo]::InvariantCulture, "v {0:F4} 0.0000 {1:F4}", $hw, -$hw))
    $sb.AppendLine([string]::Format([System.Globalization.CultureInfo]::InvariantCulture, "v {0:F4} 0.0000 {0:F4}", $hw))
    $sb.AppendLine([string]::Format([System.Globalization.CultureInfo]::InvariantCulture, "v {0:F4} 0.0000 {1:F4}", -$hw, $hw))

    # Inner top rim (5..8)
    $sb.AppendLine([string]::Format([System.Globalization.CultureInfo]::InvariantCulture, "v {0:F4} 0.0000 {0:F4}", -$iw))
    $sb.AppendLine([string]::Format([System.Globalization.CultureInfo]::InvariantCulture, "v {0:F4} 0.0000 {1:F4}", $iw, -$iw))
    $sb.AppendLine([string]::Format([System.Globalization.CultureInfo]::InvariantCulture, "v {0:F4} 0.0000 {0:F4}", $iw))
    $sb.AppendLine([string]::Format([System.Globalization.CultureInfo]::InvariantCulture, "v {0:F4} 0.0000 {1:F4}", -$iw, $iw))

    # Recess floor (9..12)
    $sb.AppendLine([string]::Format([System.Globalization.CultureInfo]::InvariantCulture, "v {0:F4} {2:F4} {0:F4}", -$iw, -$iw, -$recess))
    $sb.AppendLine([string]::Format([System.Globalization.CultureInfo]::InvariantCulture, "v {0:F4} {2:F4} {1:F4}", $iw, -$iw, -$recess))
    $sb.AppendLine([string]::Format([System.Globalization.CultureInfo]::InvariantCulture, "v {0:F4} {2:F4} {0:F4}", $iw, $iw, -$recess))
    $sb.AppendLine([string]::Format([System.Globalization.CultureInfo]::InvariantCulture, "v {0:F4} {2:F4} {1:F4}", -$iw, $iw, -$recess))

    # Bottom base (13..16)
    $bw = $hw * 0.94
    $sb.AppendLine([string]::Format([System.Globalization.CultureInfo]::InvariantCulture, "v {0:F4} {2:F4} {0:F4}", -$bw, -$bw, $baseH))
    $sb.AppendLine([string]::Format([System.Globalization.CultureInfo]::InvariantCulture, "v {0:F4} {2:F4} {1:F4}", $bw, -$bw, $baseH))
    $sb.AppendLine([string]::Format([System.Globalization.CultureInfo]::InvariantCulture, "v {0:F4} {2:F4} {0:F4}", $bw, $bw, $baseH))
    $sb.AppendLine([string]::Format([System.Globalization.CultureInfo]::InvariantCulture, "v {0:F4} {2:F4} {1:F4}", -$bw, $bw, $baseH))

    # Faces
    # Top border quads
    $sb.AppendLine("f 1 2 6 5")
    $sb.AppendLine("f 2 3 7 6")
    $sb.AppendLine("f 3 4 8 7")
    $sb.AppendLine("f 4 1 5 8")

    # Recess wall quads
    $sb.AppendLine("f 5 6 10 9")
    $sb.AppendLine("f 6 7 11 10")
    $sb.AppendLine("f 7 8 12 11")
    $sb.AppendLine("f 8 5 9 12")

    # Recess floor
    $sb.AppendLine("f 9 10 11 12")

    # Outer skirt bevel quads
    $sb.AppendLine("f 2 1 13 14")
    $sb.AppendLine("f 3 2 14 15")
    $sb.AppendLine("f 4 3 15 16")
    $sb.AppendLine("f 1 4 16 13")

    # Bottom plinth
    $sb.AppendLine("f 13 16 15 14")

    [System.IO.File]::WriteAllText($FilePath, $sb.ToString(), [System.Text.Encoding]::UTF8)
    Write-Host "Created: $FilePath"
}

# Execute generation
Generate-RoundedCube -FilePath "$OutDir\Cube_Rounded_Beveled.obj"
Generate-Sphere -FilePath "$OutDir\Sphere_Crystal_Smooth.obj"
Generate-DiamondGem -FilePath "$OutDir\Gem_Diamond_Faceted.obj"
Generate-BoardPedestal -FilePath "$OutDir\Board_Pedestal_8x8.obj"

Write-Host "All 3D Models generated successfully in $OutDir"
