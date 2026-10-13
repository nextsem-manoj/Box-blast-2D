using System.Collections.Generic;
using UnityEngine;

namespace BoxBlast
{
    /// <summary>
    /// Generates high quality, ultra-sharp vector-style icons at runtime for Trophies, Stars, Gems, 
    /// Boosters (Cannon, Bomb, Bow & Arrow, Shuffle), Coins, Crown, and Settings Gear.
    /// Uses mipmapped textures and high-contrast, bold silhouettes to guarantee razor-sharp rendering on all screens.
    /// </summary>
    public static class GemIconFactory
    {
        private static readonly Dictionary<string, Sprite> s_IconCache = new Dictionary<string, Sprite>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            s_IconCache.Clear();
        }

        private static Texture2D CreateEmptyTexture(int w, int h)
        {
            return new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                anisoLevel = 8,
                wrapMode = TextureWrapMode.Clamp
            };
        }

        // ==========================================
        // ⭐ STAR SPRITE (ULTRA-HD 3D GOLD JEWEL)
        // ==========================================
        public static Sprite GetStarSprite(bool filled)
        {
            string key = $"Star_3D_Ultra_{filled}_v6";
            if (s_IconCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            // 1. Try loading AAA Master 3D Star textures from Resources
            string resName = filled ? "Star_3D_Gold_HD" : "Star_3D_Empty_HD";
            Sprite loaded = Resources.Load<Sprite>(resName);
            if (loaded != null)
            {
                s_IconCache[key] = loaded;
                return loaded;
            }

            int size = 256;
            Texture2D tex = CreateEmptyTexture(size, size);
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float outerR = 118f;
            float innerR = outerR * 0.42f;

            Color[] pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                float dy = y - center.y;
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center.x;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    if (r < 0.001f)
                    {
                        pixels[y * size + x] = filled ? new Color(1.0f, 0.98f, 0.65f) : new Color(0.12f, 0.22f, 0.42f, 0.9f);
                        continue;
                    }

                    // Angle from top (pointing upward)
                    float angle = Mathf.Atan2(dy, dx) - Mathf.PI * 0.5f;
                    float angleNorm = Mathf.Repeat(angle, Mathf.PI * 2f);

                    // A 5-pointed star has 5 sectors of 72 degrees (2*PI / 5)
                    float sector = Mathf.PI * 2f / 5f;
                    float halfSector = sector * 0.5f;
                    float relAngle = Mathf.Repeat(angleNorm + halfSector, sector) - halfSector;
                    float absRel = Mathf.Abs(relAngle);

                    // True straight-line 5-pointed star boundary
                    float denom = innerR * Mathf.Sin(halfSector - absRel) + outerR * Mathf.Sin(absRel);
                    float maxR = (denom > 0.001f) ? (innerR * outerR * Mathf.Sin(halfSector) / denom) : innerR;

                    if (r > maxR + 1.8f)
                    {
                        pixels[y * size + x] = Color.clear;
                        continue;
                    }

                    // Crisp anti-aliased edge
                    float alpha = Mathf.Clamp01(maxR + 1.8f - r);

                    if (filled)
                    {
                        // 10-Facet 3D Bevel Lighting
                        bool isLeadingFacet = relAngle < 0f;
                        float facetFrac = Mathf.Abs(relAngle) / halfSector;
                        float rNorm = r / maxR;
                        float topBias = Mathf.Clamp01((dy + outerR) / (2f * outerR));

                        // Base gold color depending on facet illumination
                        Color baseGold = isLeadingFacet
                            ? Color.Lerp(new Color(1.0f, 0.96f, 0.55f), new Color(1.0f, 0.85f, 0.18f), rNorm * 0.5f)
                            : Color.Lerp(new Color(0.92f, 0.68f, 0.10f), new Color(0.75f, 0.42f, 0.04f), rNorm * 0.6f);

                        // Top sky light illumination
                        baseGold = Color.Lerp(baseGold, new Color(1.0f, 0.98f, 0.72f), topBias * 0.35f);

                        // Specular shine near top crest
                        if (dy > 0f && isLeadingFacet && rNorm < 0.85f && facetFrac < 0.45f)
                        {
                            float spec = Mathf.Pow(1f - facetFrac / 0.45f, 2f) * 0.75f;
                            baseGold = Color.Lerp(baseGold, Color.white, spec);
                        }

                        // Sharp golden-amber contour border
                        if (r > maxR - 4.5f)
                        {
                            float borderT = (r - (maxR - 4.5f)) / 4.5f;
                            Color rimCol = new Color(0.55f, 0.28f, 0.02f, 1f);
                            baseGold = Color.Lerp(baseGold, rimCol, borderT * 0.92f);
                        }

                        baseGold.a = alpha;
                        pixels[y * size + x] = baseGold;
                    }
                    else
                    {
                        // Unfilled: Translucent sapphire socket with bright golden rim
                        Color body = new Color(0.04f, 0.08f, 0.22f, 0.85f);
                        if (r > maxR - 4.5f)
                        {
                            float borderT = (r - (maxR - 4.5f)) / 4.5f;
                            body = Color.Lerp(body, new Color(0.90f, 0.75f, 0.25f, 0.95f), borderT);
                        }
                        else
                        {
                            float innerT = r / maxR;
                            body = Color.Lerp(new Color(0.02f, 0.05f, 0.15f, 0.92f), body, innerT);
                        }
                        body.a = alpha;
                        pixels[y * size + x] = body;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(false, false);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            s_IconCache[key] = sprite;
            return sprite;
        }

        // ==========================================
        // 🏆 GOLDEN TROPHY
        // ==========================================
        public static Sprite GetTrophySprite()
        {
            const string key = "GoldenTrophy_HD";
            if (s_IconCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int size = 128;
            Texture2D tex = CreateEmptyTexture(size, size);
            Color brightGold = new Color(1.0f, 0.92f, 0.45f);
            Color darkGold = new Color(0.85f, 0.55f, 0.08f);

            Color[] pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x - 64f) / 48f;
                    float ny = (y - 64f) / 48f;
                    bool inTrophy = false;

                    if (ny >= -0.1f && ny <= 0.65f)
                    {
                        float cupWidth = Mathf.Lerp(0.35f, 0.85f, (ny + 0.1f) / 0.75f);
                        if (Mathf.Abs(nx) <= cupWidth) inTrophy = true;
                    }
                    else if (ny < -0.1f && ny >= -0.55f)
                    {
                        if (Mathf.Abs(nx) <= 0.16f) inTrophy = true;
                    }
                    else if (ny < -0.55f && ny >= -0.85f)
                    {
                        if (Mathf.Abs(nx) <= 0.75f) inTrophy = true;
                    }

                    if (inTrophy)
                    {
                        float shade = (nx < -0.2f) ? 1.15f : ((nx > 0.3f) ? 0.82f : 1.0f);
                        Color c = Color.Lerp(darkGold, brightGold, shade * 0.7f);
                        pixels[y * size + x] = c;
                    }
                    else
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            s_IconCache[key] = sprite;
            return sprite;
        }

        // ==========================================
        // 🚀 BOOSTER 1: HEAVY SIEGE CANNON (CLEAR ROW/COLUMN)
        // High-contrast golden-brass barrel with crimson accents & electric cyan blast
        // ==========================================
        public static Sprite GetCannonSprite()
        {
            const string key = "Booster_Cannon_Crisp256";
            if (s_IconCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int size = 256;
            Texture2D tex = CreateEmptyTexture(size, size);

            // Rich warm golden brass palette (high contrast on dark blue UI)
            Color darkContour = new Color(0.06f, 0.08f, 0.14f, 1f);
            Color brassDark = new Color(0.72f, 0.44f, 0.06f);
            Color brassMid = new Color(1.0f, 0.78f, 0.18f);
            Color brassHigh = new Color(1.0f, 0.96f, 0.65f);
            Color crimsonAccent = new Color(0.92f, 0.18f, 0.24f);
            Color ironBlack = new Color(0.18f, 0.22f, 0.30f);
            Color blastCyan = new Color(0.00f, 0.95f, 1.0f);
            Color blastWhite = new Color(1.0f, 1.0f, 1.0f);

            Color[] pixels = new Color[size * size];
            Vector2 c = new Vector2(size * 0.44f, size * 0.44f);
            float radius = 108f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - c.x) / radius;
                    float dy = (y - c.y) / radius;
                    Color pixel = Color.clear;

                    // 1. Heavy Wooden/Iron Wheel at bottom-left
                    float wx = dx + 0.40f;
                    float wy = dy + 0.40f;
                    float wDist = Mathf.Sqrt(wx * wx + wy * wy);
                    if (wDist <= 0.44f)
                    {
                        float edgeA = Mathf.Clamp01((0.44f - wDist) * 28f);
                        if (wDist > 0.40f) pixel = darkContour;
                        else if (wDist > 0.33f) pixel = ironBlack;
                        else if (wDist > 0.26f) pixel = brassMid;
                        else if (wDist > 0.14f)
                        {
                            float spoke = Mathf.Repeat(Mathf.Atan2(wy, wx) * 3f / Mathf.PI, 1f);
                            pixel = (spoke < 0.32f) ? brassMid : crimsonAccent;
                        }
                        else if (wDist > 0.08f) pixel = brassDark;
                        else pixel = brassHigh; // Shiny center axle bolt

                        pixel.a *= edgeA;
                    }

                    // 2. Carriage Mounting Bracket
                    if (pixel.a == 0f && dx >= -0.45f && dx <= -0.05f && dy >= -0.45f && dy <= -0.08f)
                    {
                        pixel = ironBlack;
                    }

                    // 3. Cannon Barrel (42 degrees rotation)
                    float cos42 = 0.7431f;
                    float sin42 = 0.6691f;
                    float rotX = dx * cos42 - dy * sin42;
                    float rotY = dx * sin42 + dy * cos42;

                    // Barrel spans rotY from -0.45 (breech) to +0.72 (muzzle)
                    if (rotY >= -0.45f && rotY <= 0.74f)
                    {
                        float taper = Mathf.Lerp(0.32f, 0.25f, (rotY + 0.45f) / 1.19f);
                        float absRotX = Mathf.Abs(rotX);

                        if (absRotX <= taper)
                        {
                            float bEdge = Mathf.Clamp01((taper - absRotX) * 28f);
                            float normX = (rotX + taper) / (2f * taper);

                            // Dark outline on sides
                            if (absRotX > taper - 0.035f)
                            {
                                pixel = darkContour;
                            }
                            // Flared Muzzle Bell (rotY > 0.58f)
                            else if (rotY > 0.58f)
                            {
                                float ringShade = Mathf.Lerp(0.85f, 1.30f, normX);
                                pixel = (rotY > 0.68f) ? darkContour : brassHigh * ringShade;
                            }
                            // Raised Golden Rings
                            else if ((rotY >= 0.18f && rotY <= 0.30f) || (rotY >= -0.36f && rotY <= -0.24f))
                            {
                                float ringShade = Mathf.Lerp(0.80f, 1.25f, normX);
                                pixel = brassHigh * ringShade;
                            }
                            // Breech cap (crimson band)
                            else if (rotY < -0.36f)
                            {
                                pixel = Color.Lerp(crimsonAccent * 0.7f, crimsonAccent, normX);
                            }
                            // Main polished brass barrel body with glossy specular ridge
                            else
                            {
                                float spec = Mathf.Pow(Mathf.Clamp01(1f - Mathf.Abs(normX - 0.35f) * 2.8f), 3f);
                                pixel = Color.Lerp(brassDark, brassMid, normX) + Color.white * (spec * 0.9f);
                            }

                            pixel.a = bEdge;
                        }
                    }

                    // 4. Giant Blazing Electric Cyan Plasma Flare bursting from Muzzle
                    float blastDist = Mathf.Sqrt(Mathf.Pow(rotX, 2f) + Mathf.Pow(rotY - 0.86f, 2f));
                    if (blastDist < 0.38f)
                    {
                        float bAlpha = Mathf.Pow(Mathf.Clamp01(1f - blastDist / 0.38f), 1.6f);
                        Color blastCol = Color.Lerp(blastCyan, blastWhite, Mathf.Clamp01(1f - blastDist / 0.16f));
                        blastCol.a = bAlpha;
                        pixel = (pixel.a > 0f) ? Color.Lerp(pixel, blastCol, bAlpha * 0.9f) : blastCol;
                    }

                    pixels[y * size + x] = pixel;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            s_IconCache[key] = sprite;
            return sprite;
        }

        // ==========================================
        // 💣 BOOSTER 2: SUPER BOMB (CLEAR 3x3 AREA)
        // Glossy 3D obsidian sphere, gold lightning emblem & blazing fire starburst
        // ==========================================
        public static Sprite GetBombSprite()
        {
            const string key = "Booster_Bomb_Crisp256";
            if (s_IconCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int size = 256;
            Texture2D tex = CreateEmptyTexture(size, size);

            Color bombDeep = new Color(0.12f, 0.14f, 0.22f);
            Color bombMid = new Color(0.24f, 0.28f, 0.40f);
            Color bombRim = new Color(0.45f, 0.60f, 0.85f);
            Color brassNeck = new Color(1.0f, 0.80f, 0.18f);
            Color brassHigh = new Color(1.0f, 0.95f, 0.55f);
            Color fuseRope = new Color(0.90f, 0.82f, 0.68f);
            Color sparkYellow = new Color(1.0f, 0.95f, 0.20f);
            Color sparkOrange = new Color(1.0f, 0.42f, 0.05f);
            Color boltGold = new Color(1.0f, 0.88f, 0.15f);

            Color[] pixels = new Color[size * size];
            Vector2 c = new Vector2(size * 0.50f, size * 0.43f);
            float radius = 98f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - c.x) / radius;
                    float dy = (y - c.y) / radius;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    Color pixel = Color.clear;

                    // 1. Bomb Sphere Body
                    if (dist <= 1.0f)
                    {
                        float edgeA = Mathf.Clamp01((1.0f - dist) * 28f);

                        // 3D Spherical Normal
                        float z = Mathf.Sqrt(Mathf.Max(0f, 1f - dist * dist));
                        Vector3 normal = new Vector3(dx, dy, z);
                        Vector3 lightDir = new Vector3(-0.45f, 0.55f, 0.70f).normalized;

                        float diff = Mathf.Max(0.25f, Vector3.Dot(normal, lightDir));
                        float spec = Mathf.Pow(Mathf.Max(0f, Vector3.Dot(normal, (lightDir + Vector3.forward).normalized)), 22f);
                        float rim = Mathf.Pow(1f - z, 2.5f) * 0.55f;

                        pixel = Color.Lerp(bombDeep, bombMid, diff) + bombRim * rim;
                        pixel += Color.white * (spec * 0.95f);

                        // Large Crescent Secondary Glint on top-left
                        float glintDist = Mathf.Sqrt(Mathf.Pow(dx + 0.38f, 2f) + Mathf.Pow(dy - 0.38f, 2f));
                        float glint = Mathf.Pow(Mathf.Clamp01(1f - glintDist / 0.36f), 2.2f) * 0.45f;
                        pixel += Color.white * glint;

                        // 2. Bold Glowing Golden ⚡ Thunder Bolt in Center of Bomb
                        float lx = dx * 1.7f;
                        float ly = dy * 1.7f;
                        bool inBolt = false;
                        if (ly >= -0.15f && ly <= 0.75f && lx >= -0.35f && lx <= 0.40f)
                        {
                            if (lx >= (ly - 0.75f) * 0.65f && lx <= (ly + 0.15f) * 0.55f + 0.12f) inBolt = true;
                        }
                        if (ly >= -0.75f && ly <= 0.15f && lx >= -0.45f && lx <= 0.35f)
                        {
                            if (lx >= (ly - 0.15f) * 0.55f - 0.18f && lx <= (ly + 0.75f) * 0.65f) inBolt = true;
                        }

                        if (inBolt)
                        {
                            float boltCore = Mathf.Clamp01(1f - Mathf.Abs(lx) * 2.2f);
                            pixel = Color.Lerp(boltGold, Color.white, boltCore);
                        }

                        pixel.a = edgeA;
                    }

                    // 3. Heavy Brass Neck Cap on Top
                    if (pixel.a == 0f && Mathf.Abs(dx) <= 0.30f && dy >= 0.88f && dy <= 1.25f)
                    {
                        float nAlpha = Mathf.Clamp01((0.30f - Mathf.Abs(dx)) * 28f);
                        float shade = Mathf.Lerp(0.80f, 1.25f, (dx + 0.30f) / 0.60f);
                        pixel = ((dy > 1.16f) ? brassHigh : brassNeck) * shade;
                        pixel.a = nAlpha;
                    }

                    // 4. Thick Curved Fuse Rope
                    float fx = dx - 0.05f;
                    float fy = dy - 1.22f;
                    if (pixel.a == 0f && fx >= -0.05f && fx <= 0.58f && fy >= -0.05f && fy <= 0.50f)
                    {
                        float targetY = Mathf.Sin(fx * 3.1415f / 0.52f * 0.5f) * 0.38f;
                        float cordDist = Mathf.Abs(fy - targetY);
                        if (cordDist <= 0.08f)
                        {
                            float cA = Mathf.Clamp01((0.08f - cordDist) * 32f);
                            pixel = fuseRope;
                            pixel.a = cA;
                        }
                    }

                    // 5. Giant Blazing Multi-Point Fire Starburst
                    float sx = dx - 0.52f;
                    float sy = dy - 1.56f;
                    float sparkR = Mathf.Sqrt(sx * sx + sy * sy);
                    if (sparkR < 0.46f)
                    {
                        float angle = Mathf.Atan2(sy, sx);
                        float starR = 0.18f + 0.25f * Mathf.Pow(Mathf.Abs(Mathf.Cos(angle * 4f)), 4f);
                        starR += 0.16f * Mathf.Pow(Mathf.Abs(Mathf.Sin(angle * 4f + 0.785f)), 6f);

                        if (sparkR <= starR)
                        {
                            float sA = Mathf.Clamp01((starR - sparkR) * 24f);
                            Color sCol = (sparkR < 0.12f) ? Color.white : Color.Lerp(sparkYellow, sparkOrange, sparkR / starR);
                            sCol.a = sA;
                            pixel = (pixel.a > 0f) ? Color.Lerp(pixel, sCol, sA) : sCol;
                        }
                    }

                    pixels[y * size + x] = pixel;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            s_IconCache[key] = sprite;
            return sprite;
        }

        // ==========================================
        // 🏹 BOOSTER 3: BOW & BROADHEAD ARROW (CLEAR SINGLE BLOCK)
        // Chunky golden recurve bow, glowing cyan string & heavy ruby-tipped spearhead
        // ==========================================
        public static Sprite GetArrowSprite()
        {
            const string key = "Booster_Arrow_Crisp256";
            if (s_IconCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int size = 256;
            Texture2D tex = CreateEmptyTexture(size, size);

            Color bowDark = new Color(0.72f, 0.45f, 0.08f);
            Color bowGold = new Color(1.0f, 0.82f, 0.20f);
            Color bowHigh = new Color(1.0f, 0.96f, 0.60f);
            Color stringCyan = new Color(0.10f, 0.95f, 1.0f);
            Color shaftSteel = new Color(0.85f, 0.92f, 1.0f);
            Color arrowRuby = new Color(0.98f, 0.20f, 0.32f);
            Color arrowGoldTip = new Color(1.0f, 0.90f, 0.30f);

            Color[] pixels = new Color[size * size];
            Vector2 c = new Vector2(size * 0.50f, size * 0.50f);
            float radius = 108f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - c.x) / radius;
                    float dy = (y - c.y) / radius;
                    Color pixel = Color.clear;

                    // 1. Chunky Golden Archery Bow (Thick curved arch)
                    float arcX = dx + 0.38f;
                    float arcY = dy;
                    float arcR = Mathf.Sqrt(arcX * arcX + arcY * arcY);

                    if (arcR >= 0.60f && arcR <= 0.94f && dx < 0.28f && Mathf.Abs(dy) <= 0.85f)
                    {
                        float bowThickness = Mathf.Min(arcR - 0.60f, 0.94f - arcR) * 24f;
                        float bA = Mathf.Clamp01(bowThickness);
                        float shade = Mathf.Lerp(0.85f, 1.30f, (arcY + 0.85f) / 1.70f);
                        Color bCol = Color.Lerp(bowDark, (arcR > 0.77f) ? bowHigh : bowGold, (arcR - 0.60f) / 0.34f) * shade;
                        bCol.a = bA;
                        pixel = bCol;
                    }

                    // 2. Thick Glowing Neon Cyan Bowstring
                    if (pixel.a == 0f && Mathf.Abs(dx - 0.20f) <= 0.06f && Mathf.Abs(dy) <= 0.82f)
                    {
                        float strA = Mathf.Clamp01((0.06f - Mathf.Abs(dx - 0.20f)) * 32f);
                        Color sCol = stringCyan;
                        sCol.a = strA * 0.95f;
                        pixel = sCol;
                    }

                    // 3. Heavy Steel Arrow Shaft (Wide chunky shaft)
                    if (Mathf.Abs(dy) <= 0.085f && dx >= -0.42f && dx <= 0.44f)
                    {
                        float sA = Mathf.Clamp01((0.085f - Mathf.Abs(dy)) * 32f);
                        float sNorm = (dy + 0.085f) / 0.17f;
                        Color shaftCol = Color.Lerp(shaftSteel * 0.7f, shaftSteel * 1.3f, sNorm);
                        shaftCol.a = sA;
                        pixel = (pixel.a > 0f) ? Color.Lerp(pixel, shaftCol, sA) : shaftCol;
                    }

                    // 4. Giant Heavy Triangular Spearhead Arrowhead
                    if (dx >= 0.35f && dx <= 0.92f)
                    {
                        float headHalfWidth = (0.92f - dx) * 0.85f;
                        float dArrow = Mathf.Abs(dy);
                        if (dArrow <= headHalfWidth)
                        {
                            float headA = Mathf.Clamp01((headHalfWidth - dArrow) * 28f);
                            float spine = Mathf.Clamp01(1f - dArrow * 4f);
                            Color hCol = Color.Lerp(arrowRuby, arrowGoldTip, spine);
                            hCol.a = headA;
                            pixel = hCol;
                        }
                    }

                    // 5. Arrow Tail Fletching Feathers
                    if (dx >= -0.52f && dx <= -0.22f)
                    {
                        float fletchH = Mathf.Abs(dy);
                        float targetH = (dx + 0.52f) * 0.85f;
                        if (fletchH >= 0.05f && fletchH <= targetH && fletchH <= 0.30f)
                        {
                            float fA = Mathf.Clamp01((0.30f - fletchH) * 24f);
                            Color fCol = arrowRuby;
                            fCol.a = fA;
                            pixel = fCol;
                        }
                    }

                    pixels[y * size + x] = pixel;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            s_IconCache[key] = sprite;
            return sprite;
        }

        // ==========================================
        // 🔄 BOOSTER 4: SHUFFLE / REFRESH
        // Massive, thick, juicy circular recycling arrows (Neon Cyan & Electric Pink)
        // ==========================================
        public static Sprite GetShuffleSprite()
        {
            const string key = "Booster_Shuffle_Crisp256";
            if (s_IconCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int size = 256;
            Texture2D tex = CreateEmptyTexture(size, size);

            Color cyanDeep = new Color(0.00f, 0.65f, 0.95f);
            Color cyanBright = new Color(0.10f, 0.98f, 1.0f);
            Color pinkDeep = new Color(0.85f, 0.05f, 0.60f);
            Color pinkBright = new Color(1.0f, 0.25f, 0.80f);

            Color[] pixels = new Color[size * size];
            Vector2 c = new Vector2(size * 0.50f, size * 0.50f);
            float maxR = 104f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - c.x) / maxR;
                    float dy = (y - c.y) / maxR;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    Color pixel = Color.clear;

                    float angle = Mathf.Atan2(dy, dx);

                    // 1. Two Thick Curved Circular Arcs (Outer ~0.90, Inner ~0.48, thickness ~0.42)
                    if (r >= 0.46f && r <= 0.88f)
                    {
                        float rMid = (0.46f + 0.88f) * 0.5f;
                        float rThick = (0.88f - 0.46f) * 0.5f;
                        float edgeA = Mathf.Clamp01((rThick - Mathf.Abs(r - rMid)) * 26f);

                        float a1 = Mathf.Repeat(angle + 0.45f, Mathf.PI * 2f);
                        float a2 = Mathf.Repeat(angle + 0.45f + Mathf.PI, Mathf.PI * 2f);

                        // Cyan Top-Right Arc
                        if (a1 >= 0.28f && a1 <= 2.85f)
                        {
                            float norm = (a1 - 0.28f) / 2.57f;
                            float spec = Mathf.Pow(Mathf.Clamp01(1f - Mathf.Abs(r - rMid) / rThick), 2.2f);
                            Color cCol = Color.Lerp(cyanDeep, cyanBright, norm) + Color.white * (spec * 0.60f);
                            cCol.a = edgeA;
                            pixel = cCol;
                        }
                        // Pink Bottom-Left Arc
                        else if (a2 >= 0.28f && a2 <= 2.85f)
                        {
                            float norm = (a2 - 0.28f) / 2.57f;
                            float spec = Mathf.Pow(Mathf.Clamp01(1f - Mathf.Abs(r - rMid) / rThick), 2.2f);
                            Color pCol = Color.Lerp(pinkDeep, pinkBright, norm) + Color.white * (spec * 0.60f);
                            pCol.a = edgeA;
                            pixel = pCol;
                        }
                    }

                    // 2. Large Bold Cyan Arrowhead 1 (Pointing down-right)
                    Vector2 head1Pos = new Vector2(0.67f, -0.10f);
                    float h1x = (dx - head1Pos.x);
                    float h1y = (dy - head1Pos.y);
                    float rot1X = h1x * 0.819f + h1y * 0.573f;
                    float rot1Y = -h1x * 0.573f + h1y * 0.819f;
                    if (rot1Y >= -0.28f && rot1Y <= 0.18f && Mathf.Abs(rot1X) <= (0.18f - rot1Y) * 0.95f)
                    {
                        float hA = Mathf.Clamp01(((0.18f - rot1Y) * 0.95f - Mathf.Abs(rot1X)) * 26f);
                        Color hCol = cyanBright;
                        hCol.a = hA;
                        pixel = (pixel.a > 0f) ? Color.Lerp(pixel, hCol, hA) : hCol;
                    }

                    // 3. Large Bold Pink Arrowhead 2 (Pointing up-left)
                    Vector2 head2Pos = new Vector2(-0.67f, 0.10f);
                    float h2x = (dx - head2Pos.x);
                    float h2y = (dy - head2Pos.y);
                    float rot2X = h2x * 0.819f + h2y * 0.573f;
                    float rot2Y = -h2x * 0.573f + h2y * 0.819f;
                    if (rot2Y <= 0.28f && rot2Y >= -0.18f && Mathf.Abs(rot2X) <= (rot2Y + 0.18f) * 0.95f)
                    {
                        float hA = Mathf.Clamp01(((rot2Y + 0.18f) * 0.95f - Mathf.Abs(rot2X)) * 26f);
                        Color hCol = pinkBright;
                        hCol.a = hA;
                        pixel = (pixel.a > 0f) ? Color.Lerp(pixel, hCol, hA) : hCol;
                    }

                    pixels[y * size + x] = pixel;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            s_IconCache[key] = sprite;
            return sprite;
        }

        // ==========================================
        // 🪙 GOLD COIN ICON
        // Thick golden coin with beveled rim & high-contrast embossed star
        // ==========================================
        public static Sprite GetCoinSprite()
        {
            const string key = "GoldCoin_Crisp256";
            if (s_IconCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int size = 256;
            Texture2D tex = CreateEmptyTexture(size, size);
            Color coinGold = new Color(1.0f, 0.78f, 0.12f);
            Color coinHighlight = new Color(1.0f, 0.96f, 0.55f);
            Color coinRim = new Color(0.85f, 0.50f, 0.05f);
            Color starWhite = new Color(1.0f, 0.98f, 0.88f);

            Color[] pixels = new Color[size * size];
            Vector2 c = new Vector2(size * 0.50f, size * 0.50f);
            float radius = 108f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - c.x) / radius;
                    float dy = (y - c.y) / radius;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);

                    if (r > 1.0f)
                    {
                        pixels[y * size + x] = Color.clear;
                        continue;
                    }

                    float edgeA = Mathf.Clamp01((1.0f - r) * 28f);
                    Color pixel;

                    // Outer thick gold rim
                    if (r > 0.76f)
                    {
                        pixel = (dy > 0f) ? coinHighlight : coinRim;
                    }
                    // Inner beveled face
                    else
                    {
                        float faceGrad = Mathf.Lerp(0.85f, 1.25f, (dy + 1f) * 0.5f);
                        pixel = coinGold * faceGrad;

                        // Embossed star in center
                        if (r <= 0.48f)
                        {
                            float angle = Mathf.Atan2(dy, dx) - Mathf.PI * 0.5f;
                            float a = Mathf.Repeat(angle + Mathf.PI * 0.2f, Mathf.PI * 0.4f) - Mathf.PI * 0.2f;
                            float maxR = 0.18f * 0.45f / Mathf.Max(0.01f, 0.18f * Mathf.Abs(Mathf.Sin(a)) + 0.45f * Mathf.Abs(Mathf.Cos(a)));
                            if (r <= maxR)
                            {
                                pixel = Color.Lerp(pixel, starWhite, 0.90f);
                            }
                        }
                    }

                    pixel.a *= edgeA;
                    pixels[y * size + x] = pixel;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            s_IconCache[key] = sprite;
            return sprite;
        }

        // ==========================================
        // 👑 GOLDEN CROWN ICON
        // Imperial 3-peak crown with ruby and emerald gems
        // ==========================================
        public static Sprite GetCrownSprite()
        {
            const string key = "GoldenCrown_Masterpiece256";
            if (s_IconCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int size = 256;
            Texture2D tex = CreateEmptyTexture(size, size);
            Color[] pixels = new Color[size * size];
            Vector2 c = new Vector2(size * 0.50f, size * 0.42f);

            Color goldLight = new Color(1.0f, 0.94f, 0.52f);
            Color goldShadow = new Color(0.68f, 0.40f, 0.04f);
            Color goldRim = new Color(1.0f, 0.98f, 0.78f);
            Color velvetRed = new Color(0.65f, 0.08f, 0.20f);
            Color velvetDark = new Color(0.35f, 0.04f, 0.10f);
            Color rubyRed = new Color(1.0f, 0.18f, 0.32f);
            Color sapphireBlue = new Color(0.12f, 0.65f, 1.0f);
            Color diamondWhite = new Color(0.95f, 0.98f, 1.0f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x - c.x) / 96f;
                    float ny = (y - c.y) / 84f;
                    Color pixel = Color.clear;

                    // 1. Inner Royal Velvet Dome (behind golden arches)
                    float domeR = Mathf.Sqrt(nx * nx + (ny - 0.05f) * (ny - 0.05f) * 1.5f);
                    if (domeR <= 0.74f && ny >= -0.35f && ny <= 0.55f)
                    {
                        float domeShade = Mathf.Pow(Mathf.Clamp01(1f - domeR / 0.74f), 0.7f);
                        float edgeA = Mathf.Clamp01((0.74f - domeR) * 20f);
                        pixel = Color.Lerp(velvetDark, velvetRed, domeShade);
                        pixel *= (0.75f + 0.25f * Mathf.Cos(nx * 8f));
                        pixel.a = edgeA;
                    }

                    // 2. Crown Spires Profile
                    bool inSpire = false;
                    float spireAlpha = 0f;
                    float spireShade = 0f;

                    // Central Spire (Gothic curved arch)
                    float distCenter = Mathf.Abs(nx);
                    float centerPeakH = 0.82f - Mathf.Pow(distCenter / 0.32f, 1.4f) * 0.75f;
                    if (distCenter <= 0.32f && ny >= -0.35f && ny <= centerPeakH)
                    {
                        float edgeA = Mathf.Clamp01((centerPeakH - ny) * 22f) * Mathf.Clamp01((0.32f - distCenter) * 24f);
                        inSpire = true;
                        spireAlpha = Mathf.Max(spireAlpha, edgeA);
                        spireShade = Mathf.Lerp(0.85f, 1.30f, (ny + 0.35f) / 1.17f) * (1f - distCenter * 0.8f);
                    }

                    // Left & Right Outer Spires
                    float distSideL = Mathf.Abs(nx + 0.62f);
                    float sidePeakHL = 0.68f - Mathf.Pow(distSideL / 0.28f, 1.4f) * 0.65f;
                    if (distSideL <= 0.28f && ny >= -0.35f && ny <= sidePeakHL)
                    {
                        float edgeA = Mathf.Clamp01((sidePeakHL - ny) * 22f) * Mathf.Clamp01((0.28f - distSideL) * 24f);
                        inSpire = true;
                        spireAlpha = Mathf.Max(spireAlpha, edgeA);
                        spireShade = Mathf.Lerp(0.80f, 1.20f, (ny + 0.35f) / 1.03f);
                    }

                    float distSideR = Mathf.Abs(nx - 0.62f);
                    float sidePeakHR = 0.68f - Mathf.Pow(distSideR / 0.28f, 1.4f) * 0.65f;
                    if (distSideR <= 0.28f && ny >= -0.35f && ny <= sidePeakHR)
                    {
                        float edgeA = Mathf.Clamp01((sidePeakHR - ny) * 22f) * Mathf.Clamp01((0.28f - distSideR) * 24f);
                        inSpire = true;
                        spireAlpha = Mathf.Max(spireAlpha, edgeA);
                        spireShade = Mathf.Lerp(0.80f, 1.20f, (ny + 0.35f) / 1.03f);
                    }

                    // Composite Spire Gold
                    if (inSpire)
                    {
                        Color gCol = Color.Lerp(goldShadow, goldLight, spireShade * 0.65f);
                        if (Mathf.Abs(nx) < 0.06f || distSideL < 0.05f || distSideR < 0.05f)
                        {
                            gCol += Color.white * 0.25f;
                        }
                        pixel = Color.Lerp(pixel, gCol, spireAlpha);
                    }

                    // 3. Spire Peak Jewels / Orbs
                    // Center Top Cross / Diamond Star
                    float dStar = Mathf.Sqrt(nx * nx + (ny - 0.88f) * (ny - 0.88f));
                    if (dStar <= 0.14f)
                    {
                        float dEdge = Mathf.Clamp01((0.14f - dStar) * 26f);
                        float dGlint = Mathf.Pow(Mathf.Clamp01(1f - dStar / 0.14f), 2f);
                        Color dCol = Color.Lerp(diamondWhite, Color.white, dGlint);
                        pixel = Color.Lerp(pixel, dCol, dEdge);
                    }

                    // Left & Right Top Ruby Orbs
                    float dOrbL = Mathf.Sqrt((nx + 0.62f) * (nx + 0.62f) + (ny - 0.72f) * (ny - 0.72f));
                    if (dOrbL <= 0.11f)
                    {
                        float oEdge = Mathf.Clamp01((0.11f - dOrbL) * 26f);
                        float oGlint = Mathf.Pow(Mathf.Clamp01(1f - dOrbL / 0.11f), 2f);
                        Color oCol = Color.Lerp(rubyRed, Color.white, oGlint * 0.8f);
                        pixel = Color.Lerp(pixel, oCol, oEdge);
                    }

                    float dOrbR = Mathf.Sqrt((nx - 0.62f) * (nx - 0.62f) + (ny - 0.72f) * (ny - 0.72f));
                    if (dOrbR <= 0.11f)
                    {
                        float oEdge = Mathf.Clamp01((0.11f - dOrbR) * 26f);
                        float oGlint = Mathf.Pow(Mathf.Clamp01(1f - dOrbR / 0.11f), 2f);
                        Color oCol = Color.Lerp(rubyRed, Color.white, oGlint * 0.8f);
                        pixel = Color.Lerp(pixel, oCol, oEdge);
                    }

                    // 4. Base Headband (Embossed Curved Golden Diadem with Gemstones)
                    float bandArch = -0.38f + 0.06f * (1f - nx * nx);
                    float bandDistY = ny - bandArch;
                    if (bandDistY >= -0.26f && bandDistY <= 0.10f && Mathf.Abs(nx) <= 0.85f)
                    {
                        float edgeX = Mathf.Clamp01((0.85f - Mathf.Abs(nx)) * 24f);
                        float edgeY = Mathf.Clamp01((0.10f - bandDistY) * 22f) * Mathf.Clamp01((bandDistY + 0.26f) * 22f);
                        float bandAlpha = edgeX * edgeY;

                        float normY = (bandDistY + 0.26f) / 0.36f;
                        Color bandCol = Color.Lerp(goldShadow, goldLight, normY);
                        if (normY > 0.82f || normY < 0.18f) bandCol = Color.Lerp(bandCol, goldRim, 0.75f);

                        // Center Big Ruby Gemstone
                        if (Mathf.Abs(nx) <= 0.12f && bandDistY >= -0.16f && bandDistY <= 0.02f)
                        {
                            float gDist = Mathf.Sqrt((nx / 0.12f) * (nx / 0.12f) + ((bandDistY + 0.07f) / 0.09f) * ((bandDistY + 0.07f) / 0.09f));
                            if (gDist <= 1.0f)
                            {
                                float gemShine = Mathf.Pow(Mathf.Clamp01(1f - gDist), 1.8f);
                                bandCol = Color.Lerp(rubyRed, Color.white, gemShine * 0.75f);
                            }
                        }
                        // Left & Right Sapphire Gems
                        else if ((Mathf.Abs(nx + 0.40f) <= 0.09f || Mathf.Abs(nx - 0.40f) <= 0.09f) && bandDistY >= -0.15f && bandDistY <= 0.01f)
                        {
                            float sOffset = Mathf.Abs(nx) - 0.40f;
                            float sDist = Mathf.Sqrt((sOffset / 0.09f) * (sOffset / 0.09f) + ((bandDistY + 0.07f) / 0.08f) * ((bandDistY + 0.07f) / 0.08f));
                            if (sDist <= 1.0f)
                            {
                                float gemShine = Mathf.Pow(Mathf.Clamp01(1f - sDist), 1.8f);
                                bandCol = Color.Lerp(sapphireBlue, Color.white, gemShine * 0.75f);
                            }
                        }

                        pixel = Color.Lerp(pixel, bandCol, bandAlpha);
                    }

                    pixels[y * size + x] = pixel;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            s_IconCache[key] = sprite;
            return sprite;
        }

        // ==========================================
        // ⚙️ SETTINGS GEAR ICON
        // Heavy 6-tooth mechanical cog with chrome bevel & central axle bolt
        // ==========================================
        public static Sprite GetGearSprite()
        {
            const string key = "SettingsGear_Crisp256";
            if (s_IconCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int size = 256;
            Texture2D tex = CreateEmptyTexture(size, size);
            Color gearSilver = new Color(0.92f, 0.95f, 1.0f);
            Color gearBevel = new Color(0.55f, 0.65f, 0.82f);
            Color gearShadow = new Color(0.25f, 0.32f, 0.48f);
            Color centerHole = new Color(0.08f, 0.12f, 0.20f);
            Color centerAxle = new Color(0.75f, 0.85f, 1.0f);

            Color[] pixels = new Color[size * size];
            Vector2 c = new Vector2(size * 0.50f, size * 0.50f);
            float baseR = 104f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - c.x) / baseR;
                    float dy = (y - c.y) / baseR;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    Color pixel = Color.clear;

                    // 6-Tooth Cog Shape:
                    float angle = Mathf.Atan2(dy, dx);
                    float cogWave = Mathf.Cos(angle * 6f);
                    float outerBound = 0.74f + 0.22f * Mathf.Clamp01(cogWave * 2.8f);

                    if (r <= outerBound)
                    {
                        float edgeA = Mathf.Clamp01((outerBound - r) * 28f);

                        // Center Hole & Axle Bolt
                        if (r <= 0.26f)
                        {
                            pixel = (r <= 0.12f) ? centerAxle : centerHole;
                        }
                        // Recessed inner plate
                        else if (r <= 0.45f)
                        {
                            float rimShade = Mathf.Lerp(0.80f, 1.25f, (dy + 1f) * 0.5f);
                            pixel = gearShadow * rimShade;
                        }
                        // Outer Cog Rim & Teeth
                        else
                        {
                            float norm = (dy + 1f) * 0.5f;
                            float spec = Mathf.Pow(Mathf.Clamp01(1f - Mathf.Abs(r - 0.65f) * 3.5f), 2f);
                            pixel = Color.Lerp(gearBevel, gearSilver, norm) + Color.white * (spec * 0.55f);
                        }

                        pixel.a = edgeA;
                    }

                    pixels[y * size + x] = pixel;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            s_IconCache[key] = sprite;
            return sprite;
        }

        // ==========================================
        // 🏠 HOME / MENU ICON
        // ==========================================
        public static Sprite GetHomeSprite()
        {
            const string key = "HomeIcon_Crisp256";
            if (s_IconCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int size = 256;
            Texture2D tex = CreateEmptyTexture(size, size);
            Color homeWhite = new Color(0.95f, 0.98f, 1.0f);
            Color roofGold = new Color(1.0f, 0.85f, 0.25f);
            Color doorDark = new Color(0.06f, 0.10f, 0.18f);

            Color[] pixels = new Color[size * size];
            Vector2 c = new Vector2(size * 0.50f, size * 0.46f);
            float baseR = 100f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - c.x) / baseR;
                    float dy = (y - c.y) / baseR;
                    Color pixel = Color.clear;

                    // 1. Triangular Roof
                    if (dy >= 0.05f && dy <= 0.88f)
                    {
                        float roofHalfWidth = (0.88f - dy) * 1.15f;
                        if (Mathf.Abs(dx) <= roofHalfWidth)
                        {
                            float rEdge = Mathf.Clamp01((roofHalfWidth - Mathf.Abs(dx)) * 28f);
                            pixel = roofGold * rEdge;
                        }
                    }

                    // Chimney on right
                    if (pixel.a == 0f && dx >= 0.35f && dx <= 0.52f && dy >= 0.35f && dy <= 0.82f)
                    {
                        pixel = roofGold;
                    }

                    // 2. Square House Body
                    if (dy >= -0.72f && dy <= 0.10f && Mathf.Abs(dx) <= 0.62f)
                    {
                        float bEdge = Mathf.Min(0.62f - Mathf.Abs(dx), dy + 0.72f) * 28f;
                        bEdge = Mathf.Clamp01(bEdge);

                        // Door
                        if (Mathf.Abs(dx) <= 0.22f && dy <= -0.15f)
                        {
                            pixel = doorDark;
                        }
                        else
                        {
                            pixel = homeWhite;
                        }
                        pixel.a = bEdge;
                    }

                    pixels[y * size + x] = pixel;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            s_IconCache[key] = sprite;
            return sprite;
        }

        // ==========================================
        // 🔒 PADLOCK ICON
        // Crisp vector padlock for locked levels
        // ==========================================
        public static Sprite GetLockSprite()
        {
            const string key = "LockIcon_Crisp256";
            if (s_IconCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int size = 256;
            Texture2D tex = CreateEmptyTexture(size, size);
            Color shackleSilver = new Color(0.85f, 0.90f, 0.98f);
            Color shackleShadow = new Color(0.38f, 0.45f, 0.58f);
            Color bodyGoldTop = new Color(1.0f, 0.85f, 0.28f);
            Color bodyGoldBottom = new Color(0.80f, 0.52f, 0.08f);
            Color keyholeColor = new Color(0.12f, 0.16f, 0.25f);

            Color[] pixels = new Color[size * size];
            Vector2 c = new Vector2(size * 0.50f, size * 0.44f);
            float baseR = 100f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - c.x) / baseR;
                    float dy = (y - c.y) / baseR;
                    Color pixel = Color.clear;

                    // 1. Shackle (Arch)
                    // Centered arch at top: dy from 0.08 to 0.82
                    if (dy >= 0.08f)
                    {
                        float archCenterY = 0.40f;
                        float archDy = dy - archCenterY;
                        float distArch = Mathf.Sqrt(dx * dx + Mathf.Max(0f, archDy) * Mathf.Max(0f, archDy));
                        float outerR = 0.42f;
                        float innerR = 0.24f;

                        if (distArch <= outerR && (distArch >= innerR || archDy < 0f && Mathf.Abs(dx) >= innerR && Mathf.Abs(dx) <= outerR))
                        {
                            float edgeOuter = Mathf.Clamp01((outerR - distArch) * 28f);
                            float edgeInner = (archDy >= 0f) ? Mathf.Clamp01((distArch - innerR) * 28f) : Mathf.Clamp01((Mathf.Abs(dx) - innerR) * 28f);
                            float a = Mathf.Min(edgeOuter, edgeInner);

                            float spec = Mathf.Pow(Mathf.Clamp01(1f - Mathf.Abs(distArch - 0.33f) * 6f), 2f);
                            pixel = Color.Lerp(shackleShadow, shackleSilver, (dy + 0.5f) * 0.65f) + Color.white * (spec * 0.4f);
                            pixel.a = a;
                        }
                    }

                    // 2. Lock Body (Solid Rounded Box)
                    // dy from -0.70 to 0.12, dx from -0.56 to 0.56
                    if (dy >= -0.70f && dy <= 0.12f && Mathf.Abs(dx) <= 0.56f)
                    {
                        float cornerRadius = 0.14f;
                        float boxX = Mathf.Abs(dx) - (0.56f - cornerRadius);
                        float boxY = Mathf.Max(dy - (0.12f - cornerRadius), -0.70f + cornerRadius - dy);

                        float bodyDist = 0f;
                        if (boxX > 0f && boxY > 0f) bodyDist = Mathf.Sqrt(boxX * boxX + boxY * boxY) - cornerRadius;
                        else if (boxX > 0f) bodyDist = boxX;
                        else if (boxY > 0f) bodyDist = boxY;

                        if (bodyDist <= 0.02f)
                        {
                            float bEdge = Mathf.Clamp01((0.02f - bodyDist) * 28f);
                            float normY = (dy + 0.70f) / 0.82f;
                            pixel = Color.Lerp(bodyGoldBottom, bodyGoldTop, normY);

                            // Inner Keyhole
                            // Circle at dy = -0.22, r=0.08. Slot from dy=-0.44 to -0.22, width 0.04
                            float khDist = Mathf.Sqrt(dx * dx + (dy + 0.22f) * (dy + 0.22f));
                            bool inKeyholeSlot = (Mathf.Abs(dx) <= 0.045f && dy >= -0.45f && dy <= -0.22f);
                            if (khDist <= 0.085f || inKeyholeSlot)
                            {
                                pixel = keyholeColor;
                            }

                            pixel.a = bEdge;
                        }
                    }

                    pixels[y * size + x] = pixel;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            s_IconCache[key] = sprite;
            return sprite;
        }

        // ==========================================
        // ♾️ INFINITY ICON (Classic Mode)
        // ==========================================
        public static Sprite GetInfinitySprite()
        {
            string key = "Icon_Infinity_HD";
            if (s_IconCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int size = 128;
            Texture2D tex = CreateEmptyTexture(size, size);
            Color[] pixels = new Color[size * size];

            float cX = size * 0.5f;
            float cY = size * 0.5f;
            float loopDist = size * 0.22f;  // center of loops
            float loopRadius = size * 0.17f; // radius of loop circles
            float strokeHalf = size * 0.055f; // thickness

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dxL = x - (cX - loopDist);
                    float dxR = x - (cX + loopDist);
                    float dy = y - cY;

                    float distL = Mathf.Abs(Mathf.Sqrt(dxL * dxL + dy * dy) - loopRadius);
                    float distR = Mathf.Abs(Mathf.Sqrt(dxR * dxR + dy * dy) - loopRadius);
                    float d = Mathf.Min(distL, distR);

                    if (d <= strokeHalf + 1.2f)
                    {
                        float a = Mathf.Clamp01(strokeHalf + 1.2f - d);
                        pixels[y * size + x] = new Color(1f, 1f, 1f, a);
                    }
                    else
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            s_IconCache[key] = sprite;
            return sprite;
        }

        // ==========================================
        // 📍 MAP PIN ICON (Adventure Mode)
        // ==========================================
        public static Sprite GetMapPinSprite()
        {
            string key = "Icon_MapPin_HD";
            if (s_IconCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int size = 128;
            Texture2D tex = CreateEmptyTexture(size, size);
            Color[] pixels = new Color[size * size];

            float cX = size * 0.5f;
            float headY = size * 0.62f;
            float headR = size * 0.26f;
            float holeR = size * 0.11f;
            Vector2 tip = new Vector2(cX, size * 0.14f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - cX;
                    float dyHead = y - headY;
                    float distHead = Mathf.Sqrt(dx * dx + dyHead * dyHead);

                    bool inHead = distHead <= headR;
                    bool inHole = distHead <= holeR;

                    // Cone sides from head to tip
                    bool inCone = false;
                    if (y >= tip.y && y <= headY)
                    {
                        float progress = (headY - y) / (headY - tip.y); // 0 at head, 1 at tip
                        float coneWidth = Mathf.Lerp(headR * 0.94f, 0f, progress);
                        inCone = Mathf.Abs(dx) <= coneWidth;
                    }

                    if ((inHead || inCone) && !inHole)
                    {
                        float alpha = 1f;
                        if (distHead > headR - 1.5f && inHead && !inCone)
                        {
                            alpha = Mathf.Clamp01(headR - distHead);
                        }
                        if (distHead < holeR + 1.5f)
                        {
                            alpha = Mathf.Min(alpha, Mathf.Clamp01(distHead - holeR));
                        }
                        pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                    }
                    else
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            s_IconCache[key] = sprite;
            return sprite;
        }

        // ==========================================
        // 🟨 GOLDEN TETROMINO CLUSTER (Center Home Graphic)
        // ==========================================
        public static Sprite GetTetrominoClusterSprite()
        {
            string key = "Icon_TetrominoCluster_HD";
            if (s_IconCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int size = 256;
            Texture2D tex = CreateEmptyTexture(size, size);
            Color[] pixels = new Color[size * size];

            Color cubeTop = new Color(1.0f, 0.90f, 0.30f);      // Bright gold highlight
            Color cubeBody = new Color(1.0f, 0.72f, 0.05f);     // Amber gold body
            Color cubeShadow = new Color(0.85f, 0.52f, 0.02f);   // 3D bottom bevel
            Color cubeOutline = new Color(0.65f, 0.38f, 0.01f);  // Seam

            float bSize = 52f;
            float cX = size * 0.5f;
            float cY = size * 0.5f;

            Vector2[] blockCenters = new Vector2[]
            {
                new Vector2(cX - bSize * 0.85f, cY + bSize * 0.52f),
                new Vector2(cX + bSize * 0.15f, cY + bSize * 0.52f),
                new Vector2(cX + bSize * 0.15f, cY - bSize * 0.48f),
                new Vector2(cX + bSize * 1.15f, cY - bSize * 0.48f)
            };

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Color pixel = Color.clear;

                    for (int b = 0; b < blockCenters.Length; b++)
                    {
                        float dx = Mathf.Abs(x - blockCenters[b].x);
                        float dy = Mathf.Abs(y - blockCenters[b].y);
                        float half = bSize * 0.46f;
                        float radius = 8f;

                        float qx = Mathf.Max(0f, dx - (half - radius));
                        float qy = Mathf.Max(0f, dy - (half - radius));
                        float dist = Mathf.Sqrt(qx * qx + qy * qy) - radius;

                        if (dist <= 0f)
                        {
                            float relY = (y - (blockCenters[b].y - half)) / (bSize * 0.92f);
                            Color c;
                            if (relY > 0.65f) c = Color.Lerp(cubeBody, cubeTop, (relY - 0.65f) / 0.35f);
                            else c = Color.Lerp(cubeShadow, cubeBody, relY / 0.65f);

                            if (dx > half - 4f || dy > half - 4f)
                            {
                                c = Color.Lerp(c, cubeOutline, 0.45f);
                            }

                            pixel = c;
                            break;
                        }
                    }

                    pixels[y * size + x] = pixel;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            s_IconCache[key] = sprite;
            return sprite;
        }

        // ==========================================
        // 💎 GEMSTONE SPRITES (Adventure Objectives)
        public static void PrewarmAll()
        {
            GetGemSprite(GemType.BlueDiamond);
            GetGemSprite(GemType.OrangePentagon);
            GetGemSprite(GemType.YellowStar);
            GetGemSprite(GemType.RedRuby);
            GetCurvedGoldPlaqueSprite();
            GetCandyCloseXSprite();
            GetSpeakerIcon3D();
            GetMusicNoteIcon3D();
            GetVibrateIcon3D();
            GetSparkleIcon3D();
            GetPlayTriangleSprite();
            GetRestartArrowSprite();
        }

        public static Sprite GetGemSprite(GemType type)
        {
            if (type == GemType.None) return null;

            string key = $"Gem_{type}_HD";
            if (s_IconCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            string resName = null;
            switch (type)
            {
                case GemType.BlueDiamond: resName = "Gem_3D_Diamond"; break;
                case GemType.OrangePentagon: resName = "Gem_3D_Pentagon"; break;
                case GemType.YellowStar: resName = "Gem_3D_Star"; break;
                case GemType.RedRuby: resName = "Gem_3D_Ruby"; break;
            }

            if (!string.IsNullOrEmpty(resName))
            {
                Sprite loadedGem = SpriteFactory.LoadOrFallbackSprite(resName);
                if (loadedGem != null)
                {
                    s_IconCache[key] = loadedGem;
                    return loadedGem;
                }
            }

            int size = 128;
            Texture2D tex = CreateEmptyTexture(size, size);
            Color[] pixels = new Color[size * size];
            Vector2 c = new Vector2(size * 0.5f, size * 0.5f);

            for (int y = 0; y < size; y++)
            {
                float dy = y - c.y;
                for (int x = 0; x < size; x++)
                {
                    float dx = x - c.x;
                    Color pixel = Color.clear;

                    switch (type)
                    {
                        case GemType.BlueDiamond:
                        {
                            // Faceted Rhombus Diamond Gem
                            float rx = Mathf.Abs(dx) / 48f;
                            float ry = Mathf.Abs(dy) / 54f;
                            float d = rx + ry;

                            if (d <= 1.04f)
                            {
                                float edgeAlpha = Mathf.Clamp01((1.04f - d) * 20f);
                                // Facet segmentation
                                bool isUpper = dy > 0f;
                                bool isLeft = dx < 0f;
                                float innerD = Mathf.Abs(dx) / 22f + Mathf.Abs(dy) / 26f;

                                Color baseCol;
                                if (innerD <= 0.65f)
                                {
                                    // Central table facet
                                    baseCol = new Color(0.45f, 0.92f, 1.0f);
                                }
                                else if (isUpper)
                                {
                                    baseCol = isLeft ? new Color(0.60f, 0.95f, 1.0f) : new Color(0.25f, 0.80f, 1.0f);
                                }
                                else
                                {
                                    baseCol = isLeft ? new Color(0.12f, 0.60f, 0.98f) : new Color(0.04f, 0.40f, 0.90f);
                                }

                                // Specular glint spot on upper-left
                                float glintDist = Mathf.Sqrt((dx + 12f) * (dx + 12f) + (dy - 16f) * (dy - 16f));
                                if (glintDist < 12f)
                                {
                                    float g = Mathf.Clamp01(1f - glintDist / 12f);
                                    baseCol = Color.Lerp(baseCol, Color.white, g * 0.90f);
                                }

                                // Outer bevel border
                                if (d > 0.88f)
                                {
                                    baseCol = Color.Lerp(baseCol, new Color(0.85f, 0.98f, 1.0f), 0.70f);
                                }

                                baseCol.a = edgeAlpha;
                                pixel = baseCol;
                            }
                            break;
                        }

                        case GemType.OrangePentagon:
                        {
                            // Faceted 5-Sided Pentagon Gem
                            // Distance to regular 5-gon
                            float r = Mathf.Sqrt(dx * dx + dy * dy);
                            float angle = Mathf.Atan2(dy, dx) - Mathf.PI * 0.5f;
                            float a = Mathf.Repeat(angle + Mathf.PI * 0.2f, Mathf.PI * 0.4f) - Mathf.PI * 0.2f;
                            float polyR = r * Mathf.Cos(a);
                            float maxR = 48f;

                            if (polyR <= maxR + 1.5f)
                            {
                                float edgeAlpha = Mathf.Clamp01((maxR + 1.5f - polyR) * 16f);
                                bool isUpper = dy > 4f;
                                bool isLeft = dx < 0f;
                                float innerR = r / maxR;

                                Color baseCol;
                                if (innerR < 0.38f)
                                {
                                    baseCol = new Color(1.0f, 0.88f, 0.45f); // Golden core
                                }
                                else if (isUpper)
                                {
                                    baseCol = isLeft ? new Color(1.0f, 0.78f, 0.22f) : new Color(1.0f, 0.65f, 0.12f);
                                }
                                else
                                {
                                    baseCol = isLeft ? new Color(0.95f, 0.45f, 0.05f) : new Color(0.82f, 0.30f, 0.02f);
                                }

                                // Specular glint
                                float glintDist = Mathf.Sqrt((dx + 12f) * (dx + 12f) + (dy - 16f) * (dy - 16f));
                                if (glintDist < 12f)
                                {
                                    float g = Mathf.Clamp01(1f - glintDist / 12f);
                                    baseCol = Color.Lerp(baseCol, Color.white, g * 0.85f);
                                }

                                // Bevel border
                                if (polyR > maxR - 5f)
                                {
                                    baseCol = Color.Lerp(baseCol, new Color(1.0f, 0.95f, 0.65f), 0.65f);
                                }

                                baseCol.a = edgeAlpha;
                                pixel = baseCol;
                            }
                            break;
                        }

                        case GemType.YellowStar:
                        {
                            // Faceted Golden Star Gem
                            float r = Mathf.Sqrt(dx * dx + dy * dy);
                            float angle = Mathf.Atan2(dy, dx) - Mathf.PI * 0.5f;
                            float outerR = 52f;
                            float innerR = 22f;
                            float a = Mathf.Repeat(angle + Mathf.PI * 0.2f, Mathf.PI * 0.4f) - Mathf.PI * 0.2f;
                            float maxR = innerR * outerR / Mathf.Max(0.01f, innerR * Mathf.Abs(Mathf.Sin(a)) + outerR * Mathf.Abs(Mathf.Cos(a)));

                            if (r <= maxR + 1.2f)
                            {
                                float edgeAlpha = Mathf.Clamp01((maxR + 1.2f - r) * 16f);
                                // Alternating star ray facets
                                float facetAngle = Mathf.Repeat(angle + Mathf.PI * 0.1f, Mathf.PI * 0.2f);
                                bool facetLight = facetAngle > Mathf.PI * 0.1f;

                                Color baseCol = facetLight ? new Color(1.0f, 0.96f, 0.35f) : new Color(0.96f, 0.74f, 0.08f);

                                if (r < 16f)
                                {
                                    baseCol = Color.Lerp(baseCol, Color.white, (16f - r) / 16f * 0.85f);
                                }

                                // Specular glint
                                float glintDist = Mathf.Sqrt((dx + 8f) * (dx + 8f) + (dy - 14f) * (dy - 14f));
                                if (glintDist < 10f)
                                {
                                    float g = Mathf.Clamp01(1f - glintDist / 10f);
                                    baseCol = Color.Lerp(baseCol, Color.white, g * 0.90f);
                                }

                                // Bevel border
                                if (r > maxR - 4f)
                                {
                                    baseCol = Color.Lerp(baseCol, new Color(1.0f, 1.0f, 0.80f), 0.65f);
                                }

                                baseCol.a = edgeAlpha;
                                pixel = baseCol;
                            }
                            break;
                        }

                        case GemType.RedRuby:
                        {
                            // Faceted Hexagonal Ruby Flower Gem
                            float r = Mathf.Sqrt(dx * dx + dy * dy);
                            float angle = Mathf.Atan2(dy, dx);
                            float a = Mathf.Repeat(angle + Mathf.PI / 6f, Mathf.PI / 3f) - Mathf.PI / 6f;
                            float hexR = r * Mathf.Cos(a);
                            float maxR = 48f;

                            if (hexR <= maxR + 1.5f)
                            {
                                float edgeAlpha = Mathf.Clamp01((maxR + 1.5f - hexR) * 16f);
                                bool isUpper = dy > 0f;
                                bool isLeft = dx < 0f;
                                float innerHex = hexR / maxR;

                                Color baseCol;
                                if (innerHex < 0.42f)
                                {
                                    baseCol = new Color(1.0f, 0.45f, 0.65f); // Brilliant table
                                }
                                else if (isUpper)
                                {
                                    baseCol = isLeft ? new Color(1.0f, 0.28f, 0.48f) : new Color(0.92f, 0.15f, 0.35f);
                                }
                                else
                                {
                                    baseCol = isLeft ? new Color(0.80f, 0.08f, 0.24f) : new Color(0.60f, 0.04f, 0.18f);
                                }

                                // Specular glint
                                float glintDist = Mathf.Sqrt((dx + 12f) * (dx + 12f) + (dy - 14f) * (dy - 14f));
                                if (glintDist < 12f)
                                {
                                    float g = Mathf.Clamp01(1f - glintDist / 12f);
                                    baseCol = Color.Lerp(baseCol, Color.white, g * 0.90f);
                                }

                                // Bevel border
                                if (hexR > maxR - 5f)
                                {
                                    baseCol = Color.Lerp(baseCol, new Color(1.0f, 0.75f, 0.85f), 0.65f);
                                }

                                baseCol.a = edgeAlpha;
                                pixel = baseCol;
                            }
                            break;
                        }
                    }

                    pixels[y * size + x] = pixel;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Sprite spr = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            s_IconCache[key] = spr;
            return spr;
        }

        // ==========================================
        // 👑 CHUBBY CARTOON 3D GOLDEN CROWN
        // Identical to Block Blast style crown perched on the center letter
        // ==========================================
        public static Sprite GetChubbyCrownSprite()
        {
            Sprite hd = SpriteFactory.GetChubbyCrownHDSprite();
            if (hd != null) return hd;

            const string key = "Icon_ChubbyCartoonCrown_HD";
            if (s_IconCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int size = 160;
            Texture2D tex = CreateEmptyTexture(size, size);
            Color[] pixels = new Color[size * size];

            Color goldLight = new Color(1.0f, 0.94f, 0.40f);
            Color goldMid = new Color(1.0f, 0.74f, 0.06f);
            Color goldDark = new Color(0.86f, 0.46f, 0.02f);
            Color goldShadow = new Color(0.62f, 0.28f, 0.01f);
            Color whiteSheen = new Color(1.0f, 1.0f, 0.94f);
            Color rubyRed = new Color(0.96f, 0.18f, 0.28f);

            float cX = size * 0.5f;
            float baseY = size * 0.28f;
            float baseHalfW = size * 0.36f;
            float baseH = size * 0.14f;

            // Spire tips (center, left, right)
            Vector2 tipC = new Vector2(cX, size * 0.74f);
            Vector2 tipL = new Vector2(cX - size * 0.27f, size * 0.66f);
            Vector2 tipR = new Vector2(cX + size * 0.27f, size * 0.66f);

            // Ball tops on spires
            float ballRC = size * 0.105f;
            float ballRLR = size * 0.09f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - cX;

                    // 1. Crown Balls on Spires
                    float dBallC = Mathf.Sqrt((x - tipC.x) * (x - tipC.x) + (y - tipC.y) * (y - tipC.y));
                    float dBallL = Mathf.Sqrt((x - tipL.x) * (x - tipL.x) + (y - tipL.y) * (y - tipL.y));
                    float dBallR = Mathf.Sqrt((x - tipR.x) * (x - tipR.x) + (y - tipR.y) * (y - tipR.y));

                    bool inBallC = dBallC <= ballRC;
                    bool inBallL = dBallL <= ballRLR;
                    bool inBallR = dBallR <= ballRLR;

                    if (inBallC || inBallL || inBallR)
                    {
                        float d = inBallC ? dBallC / ballRC : (inBallL ? dBallL / ballRLR : dBallR / ballRLR);
                        float ballAlpha = Mathf.Clamp01((1.0f - d) * 12f);
                        float normY = inBallC ? (y - tipC.y) / ballRC : (inBallL ? (y - tipL.y) / ballRLR : (y - tipR.y) / ballRLR);
                        float normX = inBallC ? (x - tipC.x) / ballRC : (inBallL ? (x - tipL.x) / ballRLR : (x - tipR.x) / ballRLR);

                        // 3D spherical shading with top-left specular highlight
                        Color ballCol = Color.Lerp(goldMid, goldLight, (normY + 0.5f) * 0.8f);
                        float specDist = Mathf.Sqrt((normX + 0.3f) * (normX + 0.3f) + (normY - 0.35f) * (normY - 0.35f));
                        if (specDist < 0.45f)
                        {
                            ballCol = Color.Lerp(ballCol, whiteSheen, Mathf.Clamp01((0.45f - specDist) / 0.3f));
                        }
                        if (normY < -0.4f)
                        {
                            ballCol = Color.Lerp(goldDark, ballCol, (normY + 1.0f) / 0.6f);
                        }

                        ballCol.a = ballAlpha;
                        pixels[y * size + x] = ballCol;
                        continue;
                    }

                    // 2. Crown Spires & Body Profile
                    bool inBody = false;

                    if (y >= baseY - 4f && y <= tipC.y)
                    {
                        float valleyY = baseY + (tipC.y - baseY) * 0.35f;

                        // Center spire triangle
                        float centerSpireW = Mathf.Lerp(size * 0.18f, 0f, Mathf.Clamp01((y - valleyY) / (tipC.y - valleyY)));
                        bool inCenterSpire = (y >= valleyY && Mathf.Abs(dx) <= centerSpireW);

                        // Left/right spires
                        float sideTipX = size * 0.27f;
                        float sideSpireW = Mathf.Lerp(size * 0.14f, 0f, Mathf.Clamp01((y - valleyY) / (tipL.y - valleyY)));
                        bool inSideSpire = (y >= valleyY && Mathf.Abs(Mathf.Abs(dx) - sideTipX) <= sideSpireW);

                        // Lower crown body connecting base to valley
                        float bodyW = Mathf.Lerp(baseHalfW, size * 0.38f, Mathf.Clamp01((y - baseY) / (valleyY - baseY)));
                        bool inLowerBody = (y <= valleyY && Mathf.Abs(dx) <= bodyW);

                        inBody = inCenterSpire || inSideSpire || inLowerBody;
                    }

                    // 3. Arched Base Rim
                    float archOffset = 6f * (1.0f - (dx * dx) / (baseHalfW * baseHalfW));
                    bool inBase = (y >= baseY - baseH * 0.5f - archOffset && y <= baseY + baseH * 0.5f - archOffset && Mathf.Abs(dx) <= baseHalfW);

                    if (inBody || inBase)
                    {
                        float vGrad = Mathf.Clamp01((float)y / size);
                        Color c = Color.Lerp(goldDark, goldLight, vGrad);

                        // Soft 3D bevel shading
                        if (y < baseY)
                        {
                            c = Color.Lerp(goldShadow, goldDark, (y - (baseY - baseH)) / baseH);
                        }

                        // Center jewel in base
                        if (inBase && Mathf.Abs(dx) < 9f && Mathf.Abs(y - (baseY - archOffset)) < 9f)
                        {
                            float dJ = Mathf.Sqrt(dx * dx + (y - (baseY - archOffset)) * (y - (baseY - archOffset)));
                            if (dJ <= 8f)
                            {
                                c = Color.Lerp(rubyRed, new Color(1f, 0.7f, 0.8f), Mathf.Clamp01((8f - dJ) / 8f));
                            }
                        }

                        // Side gold studs
                        if (inBase && (Mathf.Abs(Mathf.Abs(dx) - baseHalfW * 0.6f) < 6f) && Mathf.Abs(y - (baseY - archOffset)) < 6f)
                        {
                            c = goldLight;
                        }

                        c.a = 1.0f;
                        pixels[y * size + x] = c;
                    }
                    else
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            s_IconCache[key] = sprite;
            return sprite;
        }

        // ==========================================
        // 🏆 WIN RIBBON BADGE
        // For Daily Victories streak card (Crown/Trophy + Green "WIN" banner)
        // ==========================================
        public static Sprite GetWinRibbonBadgeSprite()
        {
            const string key = "Icon_WinRibbonBadge_HD";
            if (s_IconCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int size = 128;
            Texture2D tex = CreateEmptyTexture(size, size);
            Color[] pixels = new Color[size * size];

            float cX = size * 0.5f;
            float cY = size * 0.58f;
            float shieldR = size * 0.42f;

            Color shieldBlue = new Color(0.12f, 0.38f, 0.82f);
            Color shieldDark = new Color(0.06f, 0.22f, 0.55f);
            Color gold = new Color(1.0f, 0.84f, 0.15f);
            Color greenRibbon = new Color(0.12f, 0.76f, 0.32f);
            Color greenDark = new Color(0.06f, 0.52f, 0.20f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - cX;
                    float dy = y - cY;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    Color pix = Color.clear;

                    // 1. Blue circular / arched shield
                    if (dist <= shieldR && y >= size * 0.20f)
                    {
                        float a = Mathf.Clamp01((shieldR - dist) * 8f);
                        Color sc = Color.Lerp(shieldDark, shieldBlue, (float)y / size);
                        if (dist > shieldR - 4f) sc = gold;
                        pix = sc;
                        pix.a = a;

                        // Mini Golden Crown inside shield
                        if (y >= size * 0.40f && y <= size * 0.74f && Mathf.Abs(dx) <= size * 0.25f)
                        {
                            pix = gold;
                        }
                    }

                    // 2. Green Ribbon banner across bottom
                    if (y >= size * 0.12f && y <= size * 0.34f && Mathf.Abs(dx) <= size * 0.44f)
                    {
                        float ribbonT = (y - size * 0.12f) / (size * 0.22f);
                        Color rc = Color.Lerp(greenDark, greenRibbon, ribbonT);
                        if (y < size * 0.15f || y > size * 0.31f) rc = Color.Lerp(rc, gold, 0.4f);
                        pix = rc;
                    }

                    pixels[y * size + x] = pix;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            s_IconCache[key] = sprite;
            return sprite;
        }

        // ==========================================
        // ✓ CHECKMARK SPRITE
        // ==========================================
        public static Sprite GetCheckmarkSprite()
        {
            const string key = "Icon_Checkmark_HD";
            if (s_IconCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int size = 64;
            Texture2D tex = CreateEmptyTexture(size, size);
            Color[] pixels = new Color[size * size];
            float c = size * 0.5f;
            float r = size * 0.44f;

            Color circleBg = new Color(0.82f, 0.86f, 0.94f, 0.95f);
            Color circleBorder = new Color(0.70f, 0.75f, 0.86f, 1f);
            Color checkColor = new Color(0.35f, 0.45f, 0.65f, 1f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - c;
                    float dy = y - c;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);

                    if (d <= r)
                    {
                        float a = Mathf.Clamp01((r - d) * 6f);
                        Color col = (d >= r - 2.5f) ? circleBorder : circleBg;

                        float tickDist1 = PointToSegmentDistance(new Vector2(dx, dy), new Vector2(-10, 0), new Vector2(-3, -8));
                        float tickDist2 = PointToSegmentDistance(new Vector2(dx, dy), new Vector2(-3, -8), new Vector2(11, 8));
                        float tickDist = Mathf.Min(tickDist1, tickDist2);

                        if (tickDist <= 2.8f)
                        {
                            col = Color.Lerp(checkColor, col, Mathf.Clamp01(tickDist - 1.6f));
                        }

                        col.a = a;
                        pixels[y * size + x] = col;
                    }
                    else
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            s_IconCache[key] = sprite;
            return sprite;
        }

        private static float PointToSegmentDistance(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 pa = p - a, ba = b - a;
            float h = Mathf.Clamp01(Vector2.Dot(pa, ba) / Vector2.Dot(ba, ba));
            return (pa - ba * h).magnitude;
        }

        // ==========================================
        // 🎖️ MEDAL ICON (Top Right Home Button)
        // ==========================================
        public static Sprite GetMedalSprite()
        {
            const string key = "Icon_Medal_HD";
            if (s_IconCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int size = 128;
            Texture2D tex = CreateEmptyTexture(size, size);
            Color[] pixels = new Color[size * size];

            float cX = size * 0.5f;
            float cY = size * 0.40f;
            float medalR = size * 0.32f;

            Color goldBright = new Color(1.0f, 0.88f, 0.25f);
            Color goldDark = new Color(0.85f, 0.58f, 0.05f);
            Color ribbonRed = new Color(0.92f, 0.22f, 0.28f);
            Color ribbonBlue = new Color(0.15f, 0.50f, 0.95f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - cX;
                    float dy = y - cY;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    Color pix = Color.clear;

                    // Ribbon tails on top
                    if (y >= size * 0.45f && y <= size * 0.92f)
                    {
                        if (dx >= -size * 0.26f && dx <= -size * 0.02f)
                        {
                            pix = ribbonRed;
                        }
                        else if (dx >= size * 0.02f && dx <= size * 0.26f)
                        {
                            pix = ribbonBlue;
                        }
                    }

                    // Golden Medal circle
                    if (dist <= medalR)
                    {
                        float a = Mathf.Clamp01((medalR - dist) * 8f);
                        Color c = Color.Lerp(goldDark, goldBright, (float)y / size);
                        if (dist > medalR - 3f) c = new Color(1f, 0.96f, 0.6f);
                        if (dist <= medalR * 0.55f) c = Color.Lerp(goldBright, new Color(1f, 1f, 0.8f), 0.5f);
                        c.a = a;
                        pix = c;
                    }

                    pixels[y * size + x] = pix;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            s_IconCache[key] = sprite;
            return sprite;
        }

        // ==========================================
        // 🌟 3D SETTINGS MODAL ASSETS (CONCEPT 2)
        // ==========================================

        /// <summary>
        /// 3D Pixar-style Curved Golden Plaque Banner Header with golden bevel, embossed rim, and warm honey sheen.
        /// 9-slice compatible for smooth scaling.
        /// </summary>
        public static Sprite GetCurvedGoldPlaqueSprite()
        {
            const string key = "Plaque_3D_CurvedGold_HD";
            if (s_IconCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int width = 160;
            int height = 80;
            int radius = 32;
            Texture2D tex = CreateEmptyTexture(width, height);
            Color[] pixels = new Color[width * height];

            Color goldLight = new Color(1.0f, 0.96f, 0.65f, 1f);     // Specular rim
            Color goldMid = new Color(1.0f, 0.78f, 0.15f, 1f);       // Bright gold face
            Color goldWarm = new Color(0.92f, 0.58f, 0.08f, 1f);      // Warm honey gold
            Color goldBevel = new Color(0.68f, 0.32f, 0.03f, 1f);     // Deep bevel shadow
            Color innerGlow = new Color(1.0f, 0.88f, 0.35f, 1f);

            for (int y = 0; y < height; y++)
            {
                float normY = (float)y / (height - 1);
                for (int x = 0; x < width; x++)
                {
                    int cx = Mathf.Clamp(x, radius, width - 1 - radius);
                    int cy = Mathf.Clamp(y, radius, height - 1 - radius);
                    float dx = x - cx;
                    float dy = y - cy;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy) / radius;

                    if (dist > 1.0f)
                    {
                        pixels[y * width + x] = Color.clear;
                        continue;
                    }

                    float alpha = Mathf.Clamp01((1.0f - dist) * 20f);
                    Color c;

                    if (dist >= 0.84f)
                    {
                        // Outer 3D bevel rim: light on top, dark on bottom
                        c = (normY >= 0.45f) ? Color.Lerp(goldMid, goldLight, (normY - 0.45f) / 0.55f)
                                             : Color.Lerp(goldBevel, goldWarm, normY / 0.45f);
                    }
                    else if (dist >= 0.72f)
                    {
                        // Inner beveled ridge
                        c = (dist >= 0.78f) ? goldBevel : innerGlow;
                    }
                    else
                    {
                        // Rich golden plaque face with vertical sunburst gradient
                        c = Color.Lerp(goldWarm, goldMid, normY);
                        float normX = (float)x / (width - 1);
                        float centerFactor = 1.0f - Mathf.Abs(normX - 0.5f) * 1.5f;
                        if (centerFactor > 0f)
                        {
                            c = Color.Lerp(c, goldLight, centerFactor * 0.22f * normY);
                        }
                    }

                    c.a *= alpha;
                    pixels[y * width + x] = c;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Vector4 border = new Vector4(radius + 4, radius + 4, radius + 4, radius + 4);
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
            s_IconCache[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// Glossy 3D Pixar-style Candy Ruby Red circular Close (X) button with specular gloss and clean white 'X'.
        /// </summary>
        public static Sprite GetCandyCloseXSprite()
        {
            const string key = "Btn_Candy_CloseX_3D_HD";
            if (s_IconCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int size = 96;
            Texture2D tex = CreateEmptyTexture(size, size);
            Color[] pixels = new Color[size * size];

            float cX = (size - 1) * 0.5f;
            float cY = (size - 1) * 0.5f;
            float r = (size - 8) * 0.5f;

            Color redLight = new Color(0.98f, 0.35f, 0.35f, 1f);     // #fa5a5a
            Color redMid = new Color(0.90f, 0.16f, 0.16f, 1f);       // #e62929
            Color redShadow = new Color(0.55f, 0.06f, 0.06f, 1f);    // #8c0f0f
            Color whiteSheen = new Color(1f, 1f, 1f, 0.75f);
            Color xWhite = new Color(1f, 1f, 1f, 1f);

            for (int y = 0; y < size; y++)
            {
                float dy = (y - cY) / r;
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - cX) / r;
                    float distSq = dx * dx + dy * dy;

                    if (distSq > 1.04f)
                    {
                        pixels[y * size + x] = Color.clear;
                        continue;
                    }

                    float alpha = Mathf.Clamp01((1.02f - Mathf.Sqrt(distSq)) * 24f);

                    // 1. Candy red dome gradient
                    Color c = Color.Lerp(redShadow, redMid, (dy + 1f) * 0.5f);
                    if (dy > 0.15f)
                    {
                        c = Color.Lerp(c, redLight, (dy - 0.15f) / 0.85f);
                    }

                    // 2. Top-left gloss arc
                    float specDist = Mathf.Sqrt((dx + 0.28f) * (dx + 0.28f) + (dy - 0.36f) * (dy - 0.36f));
                    if (specDist < 0.42f)
                    {
                        c = Color.Lerp(c, whiteSheen, Mathf.Clamp01((0.42f - specDist) / 0.26f));
                    }

                    // 3. Crisp white 'X' in center (thickness 0.13, length 0.48)
                    float u = Mathf.Abs(dx + dy) * 0.7071f;
                    float v = Mathf.Abs(dx - dy) * 0.7071f;
                    float armDist = Mathf.Min(u, v);
                    float armLen = Mathf.Max(u, v);

                    if (armDist <= 0.13f && armLen <= 0.48f)
                    {
                        float xAlpha = Mathf.Clamp01((0.13f - armDist) * 32f);
                        if (armLen > 0.42f) xAlpha *= Mathf.Clamp01((0.48f - armLen) * 25f);
                        c = Color.Lerp(c, xWhite, xAlpha);
                    }

                    c.a *= alpha;
                    pixels[y * size + x] = c;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            s_IconCache[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// 3D Pixar-style Golden Speaker with soundwaves and specular highlights.
        /// </summary>
        public static Sprite GetSpeakerIcon3D()
        {
            const string key = "Icon_3D_Speaker_HD";
            if (s_IconCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int size = 128;
            Texture2D tex = CreateEmptyTexture(size, size);
            Color[] pixels = new Color[size * size];

            Color goldLight = new Color(1.0f, 0.95f, 0.50f, 1f);
            Color goldMid = new Color(1.0f, 0.76f, 0.12f, 1f);
            Color goldDark = new Color(0.72f, 0.38f, 0.03f, 1f);
            Color whiteGlint = new Color(1f, 1f, 0.95f, 1f);

            for (int y = 0; y < size; y++)
            {
                float ny = (y - 64) / 48f;
                for (int x = 0; x < size; x++)
                {
                    float nx = (x - 64) / 48f;
                    Color pix = Color.clear;

                    // 1. Speaker back body (rectangular block)
                    bool inBack = (nx >= -0.75f && nx <= -0.32f && Mathf.Abs(ny) <= 0.28f);

                    // 2. Speaker flared horn (cone flaring right)
                    bool inHorn = false;
                    if (nx > -0.32f && nx <= 0.15f)
                    {
                        float hornHalfH = 0.28f + (nx - (-0.32f)) * 0.95f;
                        if (Mathf.Abs(ny) <= hornHalfH) inHorn = true;
                    }

                    if (inBack || inHorn)
                    {
                        Color c = Color.Lerp(goldDark, goldMid, (ny + 0.6f) * 0.8f);
                        if (ny > 0.1f) c = Color.Lerp(c, goldLight, (ny - 0.1f) / 0.6f);
                        if (nx < -0.35f && ny > 0.15f) c = Color.Lerp(c, whiteGlint, 0.4f);
                        pix = c;
                    }

                    // 3. Soundwaves (inner & outer concentric arcs)
                    float arcDist = Mathf.Sqrt((nx - (-0.05f)) * (nx - (-0.05f)) + ny * ny);
                    float arcAngle = Mathf.Atan2(ny, nx - (-0.05f)) * Mathf.Rad2Deg;

                    if (Mathf.Abs(arcAngle) <= 48f)
                    {
                        // Inner wave (radius ~ 0.42)
                        float w1 = Mathf.Abs(arcDist - 0.42f);
                        if (w1 <= 0.08f)
                        {
                            float a = Mathf.Clamp01((0.08f - w1) * 20f);
                            Color c = Color.Lerp(goldMid, goldLight, (ny + 0.5f));
                            c.a = a;
                            pix = c;
                        }

                        // Outer wave (radius ~ 0.72)
                        float w2 = Mathf.Abs(arcDist - 0.72f);
                        if (w2 <= 0.08f)
                        {
                            float a = Mathf.Clamp01((0.08f - w2) * 20f);
                            Color c = Color.Lerp(goldMid, goldLight, (ny + 0.5f));
                            c.a = a;
                            pix = c;
                        }
                    }

                    pixels[y * size + x] = pix;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            s_IconCache[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// 3D Pixar-style Golden Beamed Musical Notes with rich metallic bevel and specular shine.
        /// </summary>
        public static Sprite GetMusicNoteIcon3D()
        {
            const string key = "Icon_3D_MusicNote_HD";
            if (s_IconCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int size = 128;
            Texture2D tex = CreateEmptyTexture(size, size);
            Color[] pixels = new Color[size * size];

            Color goldLight = new Color(1.0f, 0.96f, 0.55f, 1f);
            Color goldMid = new Color(1.0f, 0.76f, 0.12f, 1f);
            Color goldDark = new Color(0.72f, 0.38f, 0.03f, 1f);

            Vector2 head1 = new Vector2(38f, 42f);
            Vector2 head2 = new Vector2(82f, 54f);
            float rA = 15f;
            float rB = 10f;

            for (int y = 0; y < size; y++)
            {
                float normY = (float)y / size;
                for (int x = 0; x < size; x++)
                {
                    Color pix = Color.clear;

                    float dx1 = x - head1.x;
                    float dy1 = y - head1.y;
                    float rotX1 = dx1 * 0.906f + dy1 * 0.422f;
                    float rotY1 = -dx1 * 0.422f + dy1 * 0.906f;
                    float dHead1 = (rotX1 * rotX1) / (rA * rA) + (rotY1 * rotY1) / (rB * rB);

                    float dx2 = x - head2.x;
                    float dy2 = y - head2.y;
                    float rotX2 = dx2 * 0.906f + dy2 * 0.422f;
                    float rotY2 = -dx2 * 0.422f + dy2 * 0.906f;
                    float dHead2 = (rotX2 * rotX2) / (rA * rA) + (rotY2 * rotY2) / (rB * rB);

                    bool inHead1 = dHead1 <= 1.0f;
                    bool inHead2 = dHead2 <= 1.0f;

                    bool inStem1 = (x >= 47 && x <= 55 && y >= 42 && y <= 98);
                    bool inStem2 = (x >= 91 && x <= 99 && y >= 54 && y <= 110);

                    bool inBeam = false;
                    if (x >= 47 && x <= 99)
                    {
                        float beamT = (x - 47f) / (99f - 47f);
                        float beamCenterY = Mathf.Lerp(93f, 105f, beamT);
                        if (Mathf.Abs(y - beamCenterY) <= 7f) inBeam = true;
                    }

                    if (inHead1 || inHead2 || inStem1 || inStem2 || inBeam)
                    {
                        Color c = Color.Lerp(goldDark, goldMid, normY);
                        if (normY > 0.6f) c = Color.Lerp(c, goldLight, (normY - 0.6f) / 0.4f);

                        float alpha = 1f;
                        if (inHead1 && !inStem1 && !inBeam) alpha = Mathf.Clamp01((1.0f - dHead1) * 8f);
                        if (inHead2 && !inStem2 && !inBeam) alpha = Mathf.Clamp01((1.0f - dHead2) * 8f);

                        c.a = alpha;
                        pix = c;
                    }

                    pixels[y * size + x] = pix;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            s_IconCache[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// 3D Pixar-style Golden Smartphone with vibration ripples on both sides.
        /// </summary>
        public static Sprite GetVibrateIcon3D()
        {
            const string key = "Icon_3D_VibratePhone_HD";
            if (s_IconCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int size = 128;
            Texture2D tex = CreateEmptyTexture(size, size);
            Color[] pixels = new Color[size * size];

            Color goldLight = new Color(1.0f, 0.96f, 0.55f, 1f);
            Color goldMid = new Color(1.0f, 0.76f, 0.12f, 1f);
            Color goldDark = new Color(0.72f, 0.38f, 0.03f, 1f);
            Color screenCol = new Color(0.12f, 0.22f, 0.42f, 1f);

            for (int y = 0; y < size; y++)
            {
                float ny = (y - 64) / 48f;
                for (int x = 0; x < size; x++)
                {
                    float nx = (x - 64) / 48f;
                    Color pix = Color.clear;

                    if (Mathf.Abs(nx) <= 0.40f && Mathf.Abs(ny) <= 0.72f)
                    {
                        Color c = Color.Lerp(goldDark, goldMid, (ny + 0.75f) / 1.5f);
                        if (ny > 0.3f) c = Color.Lerp(c, goldLight, (ny - 0.3f) / 0.45f);

                        if (Mathf.Abs(nx) <= 0.28f && ny >= -0.52f && ny <= 0.52f)
                        {
                            c = screenCol;
                        }
                        if (Mathf.Abs(nx) <= 0.14f && ny >= -0.65f && ny <= -0.58f)
                        {
                            c = goldMid;
                        }

                        pix = c;
                    }

                    float absNx = Mathf.Abs(nx);
                    if (absNx >= 0.52f && absNx <= 0.95f && Mathf.Abs(ny) <= 0.65f)
                    {
                        float w1 = Mathf.Abs(absNx - 0.62f);
                        if (w1 <= 0.06f)
                        {
                            float a = Mathf.Clamp01((0.06f - w1) * 25f);
                            Color c = goldMid; c.a = a;
                            pix = c;
                        }

                        float w2 = Mathf.Abs(absNx - 0.85f);
                        if (w2 <= 0.06f)
                        {
                            float a = Mathf.Clamp01((0.06f - w2) * 25f);
                            Color c = goldMid; c.a = a;
                            pix = c;
                        }
                    }

                    pixels[y * size + x] = pix;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            s_IconCache[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// 3D Pixar-style Radiant 4-Point Golden Blast Star / Sparkle with specular shine.
        /// </summary>
        public static Sprite GetSparkleIcon3D()
        {
            const string key = "Icon_3D_SparkleStar_HD";
            if (s_IconCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int size = 128;
            Texture2D tex = CreateEmptyTexture(size, size);
            Color[] pixels = new Color[size * size];

            Color goldLight = new Color(1.0f, 0.98f, 0.70f, 1f);
            Color goldMid = new Color(1.0f, 0.78f, 0.15f, 1f);
            Color goldDark = new Color(0.72f, 0.38f, 0.03f, 1f);
            Color whiteCore = new Color(1f, 1f, 1f, 1f);

            for (int y = 0; y < size; y++)
            {
                float dy = (y - 64) / 54f;
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - 64) / 54f;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);

                    if (r > 1.0f)
                    {
                        pixels[y * size + x] = Color.clear;
                        continue;
                    }

                    float astroid = Mathf.Sqrt(Mathf.Abs(dx)) + Mathf.Sqrt(Mathf.Abs(dy));
                    if (astroid <= 1.0f)
                    {
                        float a = Mathf.Clamp01((1.0f - astroid) * 16f);
                        Color c = Color.Lerp(goldDark, goldMid, (1.0f - r));
                        if (r < 0.35f) c = Color.Lerp(c, goldLight, (0.35f - r) / 0.35f);
                        if (r < 0.15f) c = Color.Lerp(c, whiteCore, (0.15f - r) / 0.15f);
                        c.a = a;
                        pixels[y * size + x] = c;
                    }
                    else
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            s_IconCache[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// 3D Pixar-style Golden Play Triangle pointing right with metallic gradient and specular sheen.
        /// </summary>
        public static Sprite GetPlayTriangleSprite()
        {
            const string key = "Icon_3D_PlayTriangle_HD";
            if (s_IconCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int size = 128;
            Texture2D tex = CreateEmptyTexture(size, size);
            Color[] pixels = new Color[size * size];

            Color goldLight = new Color(1.0f, 0.98f, 0.70f, 1f);
            Color goldMid = new Color(1.0f, 0.80f, 0.16f, 1f);
            Color goldDark = new Color(0.72f, 0.40f, 0.04f, 1f);

            for (int y = 0; y < size; y++)
            {
                float ny = (y - 64) / 36f;
                for (int x = 0; x < size; x++)
                {
                    float nx = (x - 36) / 64f;

                    if (nx >= 0f && nx <= 1f)
                    {
                        float halfH = (1f - nx);
                        if (Mathf.Abs(ny) <= halfH)
                        {
                            float edgeDist = Mathf.Min(nx, halfH - Mathf.Abs(ny)) * 36f;
                            float a = Mathf.Clamp01(edgeDist * 4f);

                            Color c = Color.Lerp(goldDark, goldMid, (ny + 1f) * 0.5f);
                            if (ny > 0f) c = Color.Lerp(c, goldLight, ny);
                            c.a = a;
                            pixels[y * size + x] = c;
                            continue;
                        }
                    }

                    pixels[y * size + x] = Color.clear;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            s_IconCache[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// 3D Pixar-style Golden Curved Restart / Reload Arrow.
        /// </summary>
        public static Sprite GetRestartArrowSprite()
        {
            const string key = "Icon_3D_RestartArrow_HD";
            if (s_IconCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int size = 128;
            Texture2D tex = CreateEmptyTexture(size, size);
            Color[] pixels = new Color[size * size];

            Color goldLight = new Color(1.0f, 0.98f, 0.70f, 1f);
            Color goldMid = new Color(1.0f, 0.80f, 0.16f, 1f);
            Color goldDark = new Color(0.72f, 0.40f, 0.04f, 1f);

            Vector2 center = new Vector2(64, 64);
            float innerR = 26f;
            float outerR = 44f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center.x;
                    float dy = y - center.y;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;

                    Color pix = Color.clear;

                    if (r >= innerR && r <= outerR && (angle >= -120f && angle <= 160f))
                    {
                        float edgeDist = Mathf.Min(r - innerR, outerR - r);
                        float a = Mathf.Clamp01(edgeDist * 4f);
                        Color c = Color.Lerp(goldDark, goldMid, (dy + 64f) / 128f);
                        if (dy > 0) c = Color.Lerp(c, goldLight, dy / 64f);
                        c.a = a;
                        pix = c;
                    }

                    float adx = x - 26f;
                    float ady = y - 78f;
                    if (ady >= -14f && ady <= 14f && adx >= -12f && adx <= 14f)
                    {
                        if (adx >= -12f && adx <= (14f - Mathf.Abs(ady) * 1.6f))
                        {
                            pix = goldMid;
                        }
                    }

                    pixels[y * size + x] = pix;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            s_IconCache[key] = sprite;
            return sprite;
        }
    }
}
