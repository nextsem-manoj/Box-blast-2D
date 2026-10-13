using System.Collections.Generic;
using UnityEngine;

namespace BoxBlast
{
    /// <summary>
    /// Generates modern 3D glossy spheres/balls and circular socket sprites at runtime.
    /// Features dynamic lighting, specular glass highlights, and rim glows.
    /// </summary>
    public static class SpriteFactory
    {
        private static readonly Dictionary<string, Sprite> s_SpriteCache = new Dictionary<string, Sprite>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            s_SpriteCache.Clear();
        }

        public static void ClearSpriteCache()
        {
            s_SpriteCache.Clear();
        }

        public static readonly Color[] BlockColors = new Color[]
        {
            new Color(0.95f, 0.22f, 0.22f), // Ruby Red
            new Color(0.98f, 0.44f, 0.10f), // Sunset Orange
            new Color(0.98f, 0.75f, 0.12f), // Golden Yellow
            new Color(0.25f, 0.78f, 0.15f), // Emerald Green
            new Color(0.12f, 0.74f, 0.94f), // Crystal Cyan
            new Color(0.18f, 0.50f, 0.95f), // Sapphire Blue
            new Color(0.62f, 0.30f, 0.88f), // Amethyst Purple
            new Color(0.95f, 0.28f, 0.65f)  // Neon Pink
        };

        public static void PrewarmAll()
        {
            for (int i = 0; i < BlockColors.Length; i++)
            {
                GetBlockSprite(BlockColors[i], BallSkinType.Classic);
            }
            LoadOrFallbackSprite("SettingsBackground_Concept2");
            LoadOrFallbackSprite("Settings_Toggle_3D_ON");
            LoadOrFallbackSprite("Settings_Toggle_3D_OFF");
            LoadOrFallbackSprite("Settings_Btn_CloseX");
            LoadOrFallbackSprite("Settings_Btn_BallDesigns");
            LoadOrFallbackSprite("Settings_Btn_Restart");
            LoadOrFallbackSprite("Settings_Btn_Home");
            LoadOrFallbackSprite("Sphere_Candy_PinkSwirl");
            LoadOrFallbackSprite("Sphere_Candy_OrangePlanet");
            LoadOrFallbackSprite("Sphere_Candy_LimeMint");
            LoadOrFallbackSprite("Sphere_Candy_CyanWave");
            LoadOrFallbackSprite("Sphere_Candy_PurpleBubble");
            LoadOrFallbackSprite("Sphere_Candy_RainbowStriped");
            LoadOrFallbackSprite("WardrobeBackground_Concept1");
            LoadOrFallbackSprite("Wardrobe_Btn_Equipped");
            LoadOrFallbackSprite("Wardrobe_Btn_Equip");
            LoadOrFallbackSprite("Wardrobe_Icon_Lock");
            LoadOrFallbackSprite("Wardrobe_Btn_CloseX");
            LoadOrFallbackSprite("Wardrobe_Skin_CandySwirl");
            LoadOrFallbackSprite("Wardrobe_Skin_StarCore");
            LoadOrFallbackSprite("Wardrobe_Skin_OrangePlanet");
            LoadOrFallbackSprite("Wardrobe_Skin_LimeMint");
            LoadOrFallbackSprite("Wardrobe_Skin_RainbowBlast");
            LoadOrFallbackSprite("Wardrobe_Skin_MysticOrb");
            LoadOrFallbackSprite("Wardrobe_Skin_SolarFlare");
            LoadOrFallbackSprite("Wardrobe_Skin_LightningPlasma");
            LoadOrFallbackSprite("Wardrobe_Skin_CyanWave");
            GetHeaderPlaqueSprite(false);
            GetHeaderPlaqueSprite(true);
            GemIconFactory.PrewarmAll();
        }

        /// <summary>
        /// Generates a realistic 3D glossy sphere with dynamic specular glass highlight,
        /// ambient rim glow, and procedural skin pattern overlays (Star Core, Diamond Crystal, Neon Pulse, etc.).
        /// Automatically adopts the equipped skin from SkinManager unless an explicit skin is requested.
        /// </summary>
        public static Sprite GetBlockSprite(Color baseColor, BallSkinType? skinType = null)
        {
            BallSkinType effectiveSkin = skinType.HasValue ? skinType.Value : SkinManager.CurrentSkin;
            string key = $"Sphere_{effectiveSkin}_{ColorUtility.ToHtmlStringRGBA(baseColor)}";
            if (s_SpriteCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            // When a ball design is equipped (Candy Swirl, Star Core, Orange Planet, etc.),
            // return the authentic, crisp 3D ball skin render
            Sprite skinSprite = GetSkinBlockSprite(effectiveSkin);
            if (skinSprite != null)
            {
                s_SpriteCache[key] = skinSprite;
                return skinSprite;
            }

            string resSphere = GetConcept1SphereName(baseColor);
            if (!string.IsNullOrEmpty(resSphere))
            {
                Sprite loadedSphere = LoadOrFallbackSprite(resSphere);
                if (loadedSphere != null)
                {
                    s_SpriteCache[key] = loadedSphere;
                    return loadedSphere;
                }
            }

            int size = 128;
            float radius = 59f;
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);

            Texture2D tex = CreateCrispTexture(size, size);
            Color[] pixels = new Color[size * size];
            Vector3 lightDir = new Vector3(-0.45f, 0.55f, 0.70f).normalized;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - center.x) / radius;
                    float dy = (y - center.y) / radius;
                    float distSq = dx * dx + dy * dy;

                    if (distSq > 1.04f)
                    {
                        pixels[y * size + x] = Color.clear;
                        continue;
                    }

                    // Anti-aliased outer edge
                    float edgeAlpha = Mathf.Clamp01((1.02f - Mathf.Sqrt(distSq)) * 36f);

                    // Spherical surface normal
                    float z = Mathf.Sqrt(Mathf.Max(0f, 1f - Mathf.Clamp01(distSq)));
                    Vector3 normal = new Vector3(dx, dy, z).normalized;

                    // 1. Lambertian Diffuse with warm ambient base
                    float diffuse = Mathf.Max(0.22f, Vector3.Dot(normal, lightDir));

                    // 2. Specular Glass Highlight (sharp reflection spot on outer glass surface)
                    Vector3 reflectDir = Vector3.Reflect(-lightDir, normal);
                    float specular = Mathf.Pow(Mathf.Max(0f, Vector3.Dot(reflectDir, new Vector3(0, 0, 1))), 32f) * 0.90f;

                    // 3. Secondary Glass Glint (Soft oval glint on glass surface)
                    float glintDist = Mathf.Sqrt(Mathf.Pow(dx + 0.32f, 2f) + Mathf.Pow(dy - 0.32f, 2f));
                    float glint = Mathf.Pow(Mathf.Clamp01(1f - glintDist / 0.38f), 2.2f) * 0.45f;

                    // 4. Fresnel Rim Light (Luminous glow around sphere perimeter)
                    float fresnel = Mathf.Pow(1.0f - z, 3.0f) * 0.40f;

                    // 5. Interior Sphere Base Body Color
                    Color bodyColor = baseColor * (diffuse * 0.85f + 0.15f);

                    // 6. Apply Unique Ball Skin Overlay Pattern to interior
                    if (distSq <= 1.0f && effectiveSkin != BallSkinType.Classic)
                    {
                        bodyColor = ApplySkinPattern(bodyColor, effectiveSkin, dx, dy, z, baseColor, lightDir);
                    }

                    // 7. Composite Outer Glass Layer (Rim Fresnel + Specular Glints ON TOP)
                    Color finalColor = bodyColor + (baseColor * fresnel) + (Color.white * (specular + glint));

                    finalColor.a = baseColor.a * edgeAlpha;
                    pixels[y * size + x] = finalColor;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            s_SpriteCache[key] = sprite;
            return sprite;
        }

        private static Color ApplySkinPattern(Color sphereColor, BallSkinType skin, float dx, float dy, float z, Color baseColor, Vector3 lightDir)
        {
            float r = Mathf.Sqrt(dx * dx + dy * dy);
            float angle = Mathf.Atan2(dy, dx);

            switch (skin)
            {
                case BallSkinType.Classic:
                    // Pure pristine 3D glossy marble
                    break;

                case BallSkinType.StarCore:
                {
                    // Radiant 8-Point Golden Celestial Star
                    float ax = Mathf.Abs(dx); float ay = Mathf.Abs(dy);
                    float beamV = Mathf.Exp(-ax * 28f) * Mathf.Max(0f, 0.60f - ay);
                    float beamH = Mathf.Exp(-ay * 28f) * Mathf.Max(0f, 0.60f - ax);
                    float diag1 = Mathf.Exp(-Mathf.Abs(ax - ay) * 24f) * Mathf.Max(0f, 0.45f - r) * 0.75f;
                    float diag2 = Mathf.Exp(-Mathf.Abs(ax + ay) * 24f) * Mathf.Max(0f, 0.45f - r) * 0.75f;
                    float starRays = Mathf.Clamp01(beamV + beamH + diag1 + diag2);
                    float nucleus = Mathf.Pow(Mathf.Clamp01(1f - r / 0.24f), 2.2f);

                    Color goldStar = Color.Lerp(new Color(1.0f, 0.85f, 0.25f), Color.white, nucleus);
                    sphereColor = Color.Lerp(sphereColor, goldStar, Mathf.Clamp01(starRays * 1.15f + nucleus * 0.95f));
                    break;
                }

                case BallSkinType.OrangePlanet:
                {
                    // Striped Citrus Cosmic Planet Bands
                    float band = Mathf.Sin((dy + dx * 0.35f) * 16f);
                    Color warmGold = new Color(1.0f, 0.65f, 0.12f);
                    sphereColor = Color.Lerp(sphereColor, warmGold, Mathf.Clamp01(band * 0.55f));
                    if (r < 0.22f) sphereColor = Color.Lerp(sphereColor, Color.white, 0.35f);
                    break;
                }

                case BallSkinType.LimeMint:
                {
                    // Refreshing Emerald Mint Candy Swirl
                    float swirl = angle + r * 4.5f;
                    float sVal = Mathf.Sin(swirl * 5f);
                    if (sVal > 0.1f)
                    {
                        sphereColor = Color.Lerp(sphereColor, Color.white, Mathf.Clamp01(sVal * 0.65f));
                    }
                    break;
                }

                case BallSkinType.GalaxySwirl:
                {
                    // Radiant Celestial Galaxy:
                    // 1. Brilliant Crystalline Diamond Star at Core with 4 Lens-Flare Cross Rays
                    if (r < 0.42f)
                    {
                        float nucleus = Mathf.Pow(Mathf.Clamp01(1f - r / 0.20f), 2.5f);
                        float ax = Mathf.Abs(dx); float ay = Mathf.Abs(dy);
                        float beamV = Mathf.Exp(-ax * 36f) * Mathf.Max(0f, 0.40f - ay);
                        float beamH = Mathf.Exp(-ay * 36f) * Mathf.Max(0f, 0.40f - ax);
                        float diag = Mathf.Exp(-Mathf.Abs(ax - ay) * 28f) * Mathf.Max(0f, 0.26f - r) * 0.6f 
                                   + Mathf.Exp(-Mathf.Abs(ax + ay) * 28f) * Mathf.Max(0f, 0.26f - r) * 0.6f;
                        float starIntensity = Mathf.Clamp01(nucleus * 1.15f + (beamV + beamH) * 0.95f + diag);
                        sphereColor = Color.Lerp(sphereColor, Color.white, starIntensity * 0.95f);
                    }

                    // 2. Slender, Luminous Starlight Spiral Arms (Quadratic Curvature)
                    float spiral = angle - 2.2f * (r * r);
                    float arm = Mathf.Pow(Mathf.Max(0f, Mathf.Cos(2f * spiral)), 8f);
                    if (arm > 0.05f && r > 0.14f && r < 0.82f)
                    {
                        float edgeFade = Mathf.Sin((r - 0.14f) / (0.82f - 0.14f) * Mathf.PI);
                        float starlight = arm * edgeFade;
                        Color starlightColor = Color.Lerp(new Color(1f, 0.92f, 0.65f), Color.white, arm);
                        sphereColor += starlightColor * (starlight * 0.80f);
                    }

                    // 3. Twinkling Diamond Stardust Crystals
                    float stardust = Mathf.Sin(dx * 32f) * Mathf.Cos(dy * 32f);
                    if (stardust > 0.86f && r > 0.16f && r < 0.78f)
                    {
                        sphereColor += Color.white * 0.60f;
                    }
                    break;
                }

                case BallSkinType.SunBurst:
                {
                    // Radiant Golden Solar Corona & Sunburst Flares
                    float ray = Mathf.Pow(Mathf.Max(0f, Mathf.Cos(8f * angle)), 4f);
                    if (ray > 0.15f && r > 0.18f && r < 0.82f)
                    {
                        float flare = ray * (1f - Mathf.Abs(r - 0.50f) / 0.35f);
                        sphereColor = Color.Lerp(sphereColor, new Color(1.0f, 0.85f, 0.22f), flare * 0.85f);
                    }
                    if (r < 0.28f)
                    {
                        float sunCore = Mathf.Pow(1f - r / 0.28f, 1.8f);
                        sphereColor = Color.Lerp(sphereColor, new Color(1.0f, 0.98f, 0.80f), sunCore * 0.90f);
                    }
                    break;
                }

                case BallSkinType.RainbowBlast:
                {
                    // Prismatic Holographic Sheen
                    float rainbowT = Mathf.Repeat(angle / (2f * Mathf.PI) + r * 0.5f, 1.0f);
                    Color rainbow = Color.HSVToRGB(rainbowT, 0.85f, 1.0f);
                    sphereColor = Color.Lerp(sphereColor, rainbow, 0.55f * z);
                    break;
                }

                case BallSkinType.CyanWave:
                {
                    // Ocean Tidal Wave Swirl
                    float wave = Mathf.Sin(dx * 12f + Mathf.Cos(dy * 10f) * 2f);
                    Color cyanOcean = new Color(0.15f, 0.92f, 1.0f);
                    sphereColor = Color.Lerp(sphereColor, cyanOcean, Mathf.Clamp01(wave * 0.60f));
                    break;
                }
            }

            return sphereColor;
        }

        private static float DistanceToLightningBolt(float px, float py)
        {
            Vector2 p = new Vector2(px, py);
            Vector2 p1 = new Vector2(0.12f, 0.58f);
            Vector2 p2 = new Vector2(-0.06f, 0.10f);
            Vector2 p3 = new Vector2(0.10f, -0.06f);
            Vector2 p4 = new Vector2(-0.14f, -0.58f);

            float d1 = DistToSegment(p, p1, p2);
            float d2 = DistToSegment(p, p2, p3);
            float d3 = DistToSegment(p, p3, p4);

            return Mathf.Min(d1, Mathf.Min(d2, d3));
        }

        private static float DistToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / Vector2.Dot(ab, ab));
            return Vector2.Distance(p, a + t * ab);
        }

        private static bool IsInsideCrown(float x, float y, out float alpha, out bool isJewel)
        {
            alpha = 0f;
            isJewel = false;

            Vector2[] jewels = new Vector2[] { new Vector2(-0.28f, 0.22f), new Vector2(0f, 0.32f), new Vector2(0.28f, 0.22f) };
            for (int i = 0; i < jewels.Length; i++)
            {
                if (Vector2.Distance(new Vector2(x, y), jewels[i]) <= 0.055f)
                {
                    alpha = 1f;
                    isJewel = true;
                    return true;
                }
            }

            if (x >= -0.32f && x <= 0.32f && y >= -0.22f && y <= -0.10f)
            {
                alpha = 1f;
                return true;
            }

            if (x >= -0.30f && x <= 0.30f && y >= -0.10f)
            {
                float peakProfile;
                float ax = Mathf.Abs(x);
                if (ax <= 0.14f)
                {
                    peakProfile = Mathf.Lerp(0.30f, 0.02f, ax / 0.14f);
                }
                else
                {
                    peakProfile = Mathf.Lerp(0.02f, 0.20f, (ax - 0.14f) / 0.14f);
                }

                if (y <= peakProfile)
                {
                    alpha = 1f;
                    return true;
                }
            }

            return false;
        }
        private static string GetConcept1SphereName(Color c)
        {
            float bestDist = float.MaxValue;
            int bestIdx = 4;
            for (int i = 0; i < BlockColors.Length; i++)
            {
                Color bc = BlockColors[i];
                float dr = c.r - bc.r;
                float dg = c.g - bc.g;
                float db = c.b - bc.b;
                float d = dr * dr + dg * dg + db * db;
                if (d < bestDist)
                {
                    bestDist = d;
                    bestIdx = i;
                }
            }

            switch (bestIdx)
            {
                case 0: return "Sphere_3D_Red";
                case 1: return "Sphere_3D_Orange";
                case 2: return "Sphere_3D_Yellow";
                case 3: return "Sphere_3D_Green";
                case 4: return "Sphere_3D_Cyan";
                case 5: return "Sphere_3D_Blue";
                case 6: return "Sphere_3D_Purple";
                case 7: return "Sphere_3D_Pink";
                default: return "Sphere_3D_Cyan";
            }
        }

        private static Texture2D CreateCrispTexture(int w, int h)
        {
            return new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                anisoLevel = 8,
                wrapMode = TextureWrapMode.Clamp
            };
        }

        /// <summary>
        /// Generates a recessed circular socket/dish for empty board slots.
        /// </summary>
        public static Sprite GetEmptyCellSprite()
        {
            const string key = "CircularSocket_Concept1";
            if (s_SpriteCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            Sprite resSocket = LoadOrFallbackSprite("CircularSocket_Concept1");
            if (resSocket != null)
            {
                s_SpriteCache[key] = resSocket;
                return resSocket;
            }

            int size = 128;
            float radius = 56f;
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);

            Texture2D tex = CreateCrispTexture(size, size);

            Color innerSocket = new Color(0.04f, 0.07f, 0.15f, 1.0f);   // Deep recessed pocket
            Color rimHighlight = new Color(0.22f, 0.38f, 0.62f, 1.0f);  // Soft cyan/blue 3D socket rim
            Color[] pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - center.x) / radius;
                    float dy = (y - center.y) / radius;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist > 1.04f)
                    {
                        pixels[y * size + x] = Color.clear;
                        continue;
                    }

                    float alpha = Mathf.Clamp01((1.02f - dist) * 24f);

                    // Recessed spherical cavity shading: top is in deeper shadow, bottom has light bounce
                    float cavityShade = Mathf.Lerp(0.65f, 1.25f, (dy + 1f) * 0.5f);
                    Color col = (dist > 0.82f) ? rimHighlight : innerSocket * cavityShade;
                    col.a = alpha;

                    pixels[y * size + x] = col;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            s_SpriteCache[key] = sprite;
            return sprite;
        }

        public static Sprite GetPreviewCellSprite(Color baseColor, BallSkinType? skinType = null)
        {
            BallSkinType effectiveSkin = skinType.HasValue ? skinType.Value : SkinManager.CurrentSkin;
            string key = $"SpherePreview_{effectiveSkin}_{ColorUtility.ToHtmlStringRGBA(baseColor)}";
            if (s_SpriteCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            Color ghostColor = baseColor;
            ghostColor.a = 0.52f;
            Sprite ghost = GetBlockSprite(ghostColor, effectiveSkin);
            s_SpriteCache[key] = ghost;
            return ghost;
        }

        /// <summary>
        /// Generates a pulsating glowing ring highlight used for pre-blast line clear anticipation.
        /// </summary>
        public static Sprite GetLineHighlightSprite()
        {
            const string key = "CellLineHighlightGlowRing";
            if (s_SpriteCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int size = 128;
            float radius = 56f;
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);

            Texture2D tex = CreateCrispTexture(size, size);
            Color[] pixels = new Color[size * size];

            Color glowCore = new Color(1.0f, 0.92f, 0.35f, 0.95f); // Brilliant Gold
            Color glowInner = new Color(1.0f, 0.70f, 0.15f, 0.40f); // Warm Amber Fill

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    float normalized = dist / radius;

                    if (normalized > 1.15f)
                    {
                        pixels[y * size + x] = Color.clear;
                        continue;
                    }

                    // Ring band between 0.82 and 1.02
                    float ringDist = Mathf.Abs(normalized - 0.92f);
                    float ringAlpha = Mathf.Clamp01(1f - ringDist / 0.14f);

                    // Soft luminous inner wash
                    float innerAlpha = Mathf.Clamp01(1f - normalized) * 0.35f;

                    Color col = Color.Lerp(glowInner, glowCore, ringAlpha);
                    col.a = Mathf.Max(ringAlpha * 0.95f, innerAlpha);

                    pixels[y * size + x] = col;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            s_SpriteCache[key] = sprite;
            return sprite;
        }

        public static Sprite GetBoardBackgroundSprite()
        {
            const string key = "BoardBackground8x8_Concept1";
            if (s_SpriteCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int size = 512;
            int cornerRadius = 46;
            Texture2D tex = CreateCrispTexture(size, size);

            Color outerBezel    = new Color(0.04f, 0.06f, 0.14f, 1.0f);  // Outer dark chassis bevel
            Color neonOuterTube = new Color(0.18f, 0.82f, 1.0f, 1.0f);   // Outer electric cyan neon line
            Color neonInnerTube = new Color(0.24f, 0.90f, 1.0f, 1.0f);   // Inner electric cyan neon line
            Color grooveColor   = new Color(0.05f, 0.08f, 0.18f, 1.0f);  // Groove between neon tubes
            Color gridBedBase   = new Color(0.06f, 0.09f, 0.18f, 0.99f); // Dark slate navy console bed
            Color gridBedCheck  = new Color(0.07f, 0.11f, 0.22f, 0.99f); // Subtle checkerboard variation

            Color[] pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = GetRoundedRectDistance(x, y, size, size, cornerRadius);
                    if (dist > 1.0f)
                    {
                        pixels[y * size + x] = Color.clear;
                        continue;
                    }

                    float alpha = Mathf.Clamp01((1.0f - dist) * 20f);
                    Color col;

                    if (dist > 0.965f)
                    {
                        col = outerBezel;
                    }
                    else if (dist >= 0.915f && dist <= 0.965f)
                    {
                        // 1. Outer cyan neon tube
                        float centerT = 1f - Mathf.Abs(dist - 0.940f) / 0.025f;
                        col = Color.Lerp(grooveColor, neonOuterTube, Mathf.Clamp01(centerT));
                    }
                    else if (dist >= 0.865f && dist < 0.915f)
                    {
                        // Beveled groove between neon tubes
                        col = grooveColor;
                    }
                    else if (dist >= 0.815f && dist < 0.865f)
                    {
                        // 2. Inner cyan neon tube
                        float centerT = 1f - Mathf.Abs(dist - 0.840f) / 0.025f;
                        col = Color.Lerp(gridBedBase, neonInnerTube, Mathf.Clamp01(centerT));
                    }
                    else if (dist > 0.775f)
                    {
                        // Inner ambient neon glow falloff
                        float glowT = (dist - 0.775f) / 0.040f;
                        col = Color.Lerp(gridBedBase, neonInnerTube * 0.5f, glowT);
                    }
                    else
                    {
                        // 3. Console grid bed with subtle 8x8 checkerboard tiles
                        int tx = (int)(x / (size / 8.0f));
                        int ty = (int)(y / (size / 8.0f));
                        bool check = ((tx + ty) % 2 == 0);
                        col = check ? gridBedCheck : gridBedBase;
                    }

                    col.a = alpha;
                    pixels[y * size + x] = col;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            s_SpriteCache[key] = sprite;
            return sprite;
        }

        public static Sprite GetConcept1PedestalSprite()
        {
            const string key = "Gameplay_Pedestal_Concept1";
            if (s_SpriteCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            Sprite loaded = LoadOrFallbackSprite("Gameplay_Pedestal_Concept1");
            if (loaded != null)
            {
                s_SpriteCache[key] = loaded;
                return loaded;
            }

            return GetPanelSprite(new Color(0.08f, 0.14f, 0.28f, 0.85f), new Color(0.35f, 0.65f, 0.95f, 0.90f), 3f, 24);
        }

        /// <summary>
        /// Generates a clean royal blue backdrop matching the Block Blast screenshot.
        /// </summary>
        public static Sprite GetCosmicStarryBackgroundSprite()
        {
            const string key = "RoyalBlueBackdrop";
            if (s_SpriteCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int size = 256;
            Texture2D tex = CreateCrispTexture(size, size);

            Color centerColor = new Color(0.24f, 0.32f, 0.68f, 1.0f); // Clean Royal Blue Center
            Color edgeColor = new Color(0.20f, 0.27f, 0.60f, 1.0f);   // Matching Blue Perimeter
            Color[] pixels = new Color[size * size];
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float maxR = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - center.x) / maxR;
                    float dy = (y - center.y) / maxR;
                    float dist = Mathf.Clamp01(Mathf.Sqrt(dx * dx + dy * dy));

                    float t = dist * dist * (3f - 2f * dist);
                    pixels[y * size + x] = Color.Lerp(centerColor, edgeColor, t);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            s_SpriteCache[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// Generates a sparkling 4-point diamond star shard for line-clear explosions.
        /// </summary>
        public static Sprite GetSparkleShardSprite()
        {
            const string key = "SparkleShard_HD";
            if (s_SpriteCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int size = 64;
            Texture2D tex = CreateCrispTexture(size, size);
            Color[] pixels = new Color[size * size];
            Vector2 c = new Vector2(size * 0.5f, size * 0.5f);
            float radius = 28f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = Mathf.Abs(x - c.x) / radius;
                    float ny = Mathf.Abs(y - c.y) / radius;

                    // 4-point star formula: sqrt(x) + sqrt(y) <= 1
                    float starVal = Mathf.Sqrt(nx) + Mathf.Sqrt(ny);
                    if (starVal > 1.05f)
                    {
                        pixels[y * size + x] = Color.clear;
                        continue;
                    }

                    float a = Mathf.Clamp01((1.05f - starVal) * 12f);
                    float distCenter = Mathf.Sqrt(nx * nx + ny * ny);
                    Color col = Color.Lerp(Color.white, new Color(1f, 1f, 1f, 0.7f), distCenter);
                    col.a *= a;
                    pixels[y * size + x] = col;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64f);
            s_SpriteCache[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// Generates an expanding shockwave ring for 3x3 sector clears and combo blasts.
        /// </summary>
        public static Sprite GetShockwaveRingSprite()
        {
            const string key = "ShockwaveRing_HD";
            if (s_SpriteCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int size = 128;
            Texture2D tex = CreateCrispTexture(size, size);
            Color[] pixels = new Color[size * size];
            Vector2 c = new Vector2(size * 0.5f, size * 0.5f);
            float maxR = 60f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - c.x) / maxR;
                    float dy = (y - c.y) / maxR;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);

                    if (r < 0.65f || r > 1.0f)
                    {
                        pixels[y * size + x] = Color.clear;
                        continue;
                    }

                    float distFromPeak = Mathf.Abs(r - 0.85f) / 0.15f;
                    float alpha = Mathf.Clamp01(1.0f - distFromPeak);
                    alpha = alpha * alpha;

                    Color col = Color.white;
                    col.a = alpha;
                    pixels[y * size + x] = col;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 128f);
            s_SpriteCache[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// Generates a soft ambient floating stardust glow orb.
        /// </summary>
        public static Sprite GetSoftGlowOrbSprite()
        {
            const string key = "SoftGlowOrb_HD";
            if (s_SpriteCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int size = 64;
            Texture2D tex = CreateCrispTexture(size, size);
            Color[] pixels = new Color[size * size];
            Vector2 c = new Vector2(size * 0.5f, size * 0.5f);
            float maxR = 28f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - c.x) / maxR;
                    float dy = (y - c.y) / maxR;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);

                    if (r > 1.0f)
                    {
                        pixels[y * size + x] = Color.clear;
                        continue;
                    }

                    float a = Mathf.Pow(Mathf.Clamp01(1f - r), 1.8f);
                    Color col = Color.white;
                    col.a = a;
                    pixels[y * size + x] = col;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64f);
            s_SpriteCache[key] = sprite;
            return sprite;
        }

        public static Sprite GetRoundedButtonSprite(Color fillColor, Color borderColor, float borderRatio = 0.08f)
        {
            string key = $"Btn_{ColorUtility.ToHtmlStringRGBA(fillColor)}_{ColorUtility.ToHtmlStringRGBA(borderColor)}_{borderRatio:F2}";
            if (s_SpriteCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int size = 128;
            int radius = 28;
            Texture2D tex = CreateCrispTexture(size, size);

            Color[] pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = GetRoundedRectDistance(x, y, size, size, radius);
                    if (dist > 1.0f)
                    {
                        pixels[y * size + x] = Color.clear;
                        continue;
                    }

                    float alpha = Mathf.Clamp01((1.0f - dist) * 20f);
                    float borderThreshold = 1.0f - borderRatio;
                    Color c = (dist >= borderThreshold) ? borderColor : fillColor;
                    c.a *= alpha;
                    pixels[y * size + x] = c;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Vector4 border = new Vector4(radius + 4, radius + 4, radius + 4, radius + 4);
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
            s_SpriteCache[key] = sprite;
            return sprite;
        }

        public static Sprite Get3DPillButtonSprite(Color baseColor, Color shadowColor)
        {
            string key = $"Btn3D_{ColorUtility.ToHtmlStringRGBA(baseColor)}_{ColorUtility.ToHtmlStringRGBA(shadowColor)}";
            if (s_SpriteCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int size = 128;
            int radius = 32;
            Texture2D tex = CreateCrispTexture(size, size);
            Color[] pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = GetRoundedRectDistance(x, y, size, size, radius);
                    if (dist > 1.0f)
                    {
                        pixels[y * size + x] = Color.clear;
                        continue;
                    }

                    float alpha = Mathf.Clamp01((1.0f - dist) * 20f);
                    float normY = (float)y / size;

                    Color c;
                    // Bottom 18% is tactile 3D shadow bevel
                    if (normY < 0.16f)
                    {
                        c = shadowColor;
                    }
                    else
                    {
                        // Top face with subtle specular gradient
                        float topGrad = (normY - 0.16f) / 0.84f;
                        c = Color.Lerp(baseColor, baseColor + Color.white * 0.14f, topGrad);
                    }

                    c.a *= alpha;
                    pixels[y * size + x] = c;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Vector4 border = new Vector4(radius + 2, radius + 2, radius + 2, radius + 2);
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
            s_SpriteCache[key] = sprite;
            return sprite;
        }

        public static Sprite GetPillSprite(Color fillColor)
        {
            string key = $"Pill_{ColorUtility.ToHtmlStringRGBA(fillColor)}";
            if (s_SpriteCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int size = 128;
            int radius = 48;
            Texture2D tex = CreateCrispTexture(size, size);
            Color[] pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = GetRoundedRectDistance(x, y, size, size, radius);
                    if (dist > 1.0f)
                    {
                        pixels[y * size + x] = Color.clear;
                        continue;
                    }

                    float alpha = Mathf.Clamp01((1.0f - dist) * 16f);
                    Color c = fillColor;
                    c.a *= alpha;
                    pixels[y * size + x] = c;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(false, false);

            Vector4 border = new Vector4(radius + 2, radius + 2, radius + 2, radius + 2);
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
            s_SpriteCache[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// Generates a premium 3D Candy/Arcade pill plaque with an embossed metallic golden bevel,
        /// deep sapphire glass interior, and specular upper highlight.
        /// Perfect for Level, Moves, and Score stat boxes.
        /// </summary>
        public static Sprite GetHeaderPlaqueSprite(bool isCenterHero = false)
        {
            string key = isCenterHero ? "HeaderPlaque_Hero_v1" : "HeaderPlaque_Normal_v1";
            if (s_SpriteCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int width = 256;
            int height = 80;
            int radius = 36;
            Texture2D tex = CreateCrispTexture(width, height);
            Color[] pixels = new Color[width * height];

            // Palette
            Color goldTop = isCenterHero ? new Color(1.0f, 0.94f, 0.60f) : new Color(0.98f, 0.88f, 0.48f); // Bright golden highlight
            Color goldMid = isCenterHero ? new Color(1.0f, 0.78f, 0.16f) : new Color(0.92f, 0.72f, 0.18f); // Vibrant gold
            Color goldBot = isCenterHero ? new Color(0.68f, 0.38f, 0.04f) : new Color(0.55f, 0.30f, 0.03f); // Amber bronze shadow

            Color glassTop = isCenterHero ? new Color(0.08f, 0.22f, 0.52f, 0.98f) : new Color(0.05f, 0.12f, 0.30f, 0.98f);
            Color glassBot = isCenterHero ? new Color(0.04f, 0.10f, 0.28f, 0.98f) : new Color(0.02f, 0.06f, 0.18f, 0.98f);
            Color cyanGlow = new Color(0.20f, 0.85f, 1.0f, 0.45f);

            for (int y = 0; y < height; y++)
            {
                float ny = (float)y / (height - 1); // 0 (bottom) to 1 (top)
                for (int x = 0; x < width; x++)
                {
                    float dist = GetRoundedRectDistance(x, y, width, height, radius);
                    if (dist > 1.04f)
                    {
                        pixels[y * width + x] = Color.clear;
                        continue;
                    }

                    // Antialiased outer contour
                    float outerAlpha = Mathf.Clamp01((1.02f - dist) * 24f);

                    // 1. Embossed 3D Golden Bevel Rim (dist between 0.82 and 1.0)
                    if (dist >= 0.82f)
                    {
                        Color rimColor;
                        if (ny > 0.55f)
                            rimColor = Color.Lerp(goldMid, goldTop, (ny - 0.55f) / 0.45f);
                        else
                            rimColor = Color.Lerp(goldBot, goldMid, ny / 0.55f);

                        // Subtle outer dark shadow bevel at the bottom outer rim
                        if (dist > 0.96f && ny < 0.3f)
                        {
                            rimColor = Color.Lerp(rimColor, new Color(0.25f, 0.12f, 0.02f), 0.5f);
                        }

                        rimColor.a = outerAlpha;
                        pixels[y * width + x] = rimColor;
                    }
                    // 2. Inner Recessed Cavity / Glass Body (dist < 0.82)
                    else
                    {
                        // Glass base vertical gradient
                        Color body = Color.Lerp(glassBot, glassTop, ny);

                        // Inner groove shadow near the golden rim
                        float innerBorderT = Mathf.Clamp01((0.82f - dist) / 0.12f);
                        float grooveShadow = 1.0f - Mathf.Pow(1.0f - innerBorderT, 2.0f) * 0.35f;
                        body *= grooveShadow;

                        // Upper Specular Highlight (soft curved shine on top 45%)
                        if (ny > 0.52f)
                        {
                            float shine = Mathf.Pow((ny - 0.52f) / 0.48f, 1.5f) * 0.32f;
                            body = Color.Lerp(body, Color.white, shine);
                        }

                        // For center hero moves badge: subtle cyan ambient inner rim glow
                        if (isCenterHero && dist > 0.65f)
                        {
                            float glowT = (dist - 0.65f) / 0.17f;
                            body = Color.Lerp(body, cyanGlow, glowT * 0.30f);
                        }

                        body.a = outerAlpha;
                        pixels[y * width + x] = body;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(false, false);

            int sliceW = radius + 6;
            int sliceH = 26;
            Vector4 border = new Vector4(sliceW, sliceH, sliceW, sliceH);
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
            s_SpriteCache[key] = sprite;
            return sprite;
        }

        public static Sprite GetChunkyBlockBlastButtonSprite(Color baseColor, Color shadowColor, Color rimHighlight)
        {
            string key = $"BtnChunkyBB_3D_v2_{ColorUtility.ToHtmlStringRGBA(baseColor)}_{ColorUtility.ToHtmlStringRGBA(shadowColor)}_{ColorUtility.ToHtmlStringRGBA(rimHighlight)}";
            if (s_SpriteCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int width = 256;
            int height = 128;
            int radius = 44;
            int extrusionDepth = 22; // Chunky 3D bottom shelf thickness
            int capInset = 3;        // Inset of raised top cap from sides and top

            Texture2D tex = CreateCrispTexture(width, height);
            Color[] pixels = new Color[width * height];

            int capBottom = extrusionDepth;
            int capTop = height - 1 - capInset;
            int capLeft = capInset;
            int capRight = width - 1 - capInset;
            int capHeight = capTop - capBottom;
            int capRadius = radius - capInset;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Outer capsule distance
                    float dxOuter = 0f;
                    if (x < radius) dxOuter = radius - x;
                    else if (x >= (width - radius)) dxOuter = x - (width - 1 - radius);

                    float dyOuter = 0f;
                    if (y < radius) dyOuter = radius - y;
                    else if (y >= (height - radius)) dyOuter = y - (height - 1 - radius);

                    float distOuter = Mathf.Sqrt(dxOuter * dxOuter + dyOuter * dyOuter) / (float)radius;
                    if (distOuter > 1.0f)
                    {
                        pixels[y * width + x] = Color.clear;
                        continue;
                    }

                    float alpha = Mathf.Clamp01((1.0f - distOuter) * 16f);

                    // Inner raised pressable cap distance
                    bool inCap = false;
                    float distCap = 2.0f;
                    if (y >= capBottom && x >= capLeft && x <= capRight)
                    {
                        float dxCap = 0f;
                        if (x < (capLeft + capRadius)) dxCap = (capLeft + capRadius) - x;
                        else if (x >= (capRight - capRadius)) dxCap = x - (capRight - capRadius);

                        float dyCap = 0f;
                        if (y < (capBottom + capRadius)) dyCap = (capBottom + capRadius) - y;
                        else if (y >= (capTop - capRadius)) dyCap = y - (capTop - capRadius);

                        distCap = Mathf.Sqrt(dxCap * dxCap + dyCap * dyCap) / (float)capRadius;
                        if (distCap <= 1.0f) inCap = true;
                    }

                    Color c;
                    if (inCap && distCap <= 0.98f)
                    {
                        // Raised Pressable Cap Face
                        float capNormY = (float)(y - capBottom) / (float)capHeight; // 0 at bottom, 1 at top

                        // Rich, warm vertical candy gradient
                        c = Color.Lerp(baseColor * 0.88f, baseColor + Color.white * 0.30f, capNormY);

                        // Top curved glass specular shine
                        if (capNormY > 0.50f)
                        {
                            float shineT = (capNormY - 0.50f) / 0.50f;
                            float shine = Mathf.Sin(shineT * Mathf.PI * 0.92f) * 0.46f;
                            c = Color.Lerp(c, Color.white, shine);
                        }

                        // Crisp inner top rim highlight
                        if (capNormY > 0.88f)
                        {
                            float rimT = (capNormY - 0.88f) / 0.12f;
                            c = Color.Lerp(c, rimHighlight, rimT * 0.75f);
                        }
                    }
                    else
                    {
                        // 3D Extrusion Shelf / Bevel Pedestal
                        float shelfNorm = Mathf.Clamp01((float)y / (float)extrusionDepth);
                        float darken = 0.72f + 0.28f * shelfNorm;
                        c = shadowColor * darken;

                        // Subtle dark crease right below the raised cap
                        if (y < capBottom && (capBottom - y) <= 3)
                        {
                            c *= 0.78f;
                        }
                    }

                    c.a *= alpha;
                    pixels[y * width + x] = c;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Vector4 border = new Vector4(radius + 4, extrusionDepth + 4, radius + 4, radius / 2 + 4);
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
            s_SpriteCache[key] = sprite;
            return sprite;
        }

        public static Sprite GetButtonGlossOverlaySprite()
        {
            const string key = "BtnGlossOverlay_Pill";
            if (s_SpriteCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int w = 128;
            int h = 48;
            int radius = 22;
            Texture2D tex = CreateCrispTexture(w, h);
            Color[] pixels = new Color[w * h];

            for (int y = 0; y < h; y++)
            {
                float normY = (float)y / (float)(h - 1); // 0 at bottom, 1 at top
                for (int x = 0; x < w; x++)
                {
                    float dx = 0f;
                    if (x < radius) dx = radius - x;
                    else if (x >= (w - radius)) dx = x - (w - 1 - radius);

                    float dy = 0f;
                    if (y < radius) dy = radius - y;
                    else if (y >= (h - radius)) dy = y - (h - 1 - radius);

                    float dist = Mathf.Sqrt(dx * dx + dy * dy) / (float)radius;
                    if (dist > 1.0f)
                    {
                        pixels[y * w + x] = Color.clear;
                        continue;
                    }

                    float pillAlpha = Mathf.Clamp01((1.0f - dist) * 4.0f);
                    float alpha = pillAlpha * (0.12f + 0.88f * Mathf.Pow(normY, 1.3f));
                    Color c = Color.white;
                    c.a = alpha * 0.55f;
                    pixels[y * w + x] = c;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Vector4 border = new Vector4(radius + 2, radius + 2, radius + 2, radius + 2);
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
            s_SpriteCache[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// Generates a sleek 3D royal sapphire level select card with tone-on-tone bevel,
        /// curved specular glass sheen, and crisp bright rim framing.
        /// </summary>
        public static Sprite Get3DLevelCardSprite(bool isUnlocked)
        {
            string key = $"LevelCard3D_v2_{(isUnlocked ? "Unlocked" : "Locked")}";
            if (s_SpriteCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int width = 128;
            int height = 96;
            int radius = 22;
            int extrusionDepth = 10; // Subtle, sleek 3D bottom bevel (NOT half-and-half burger!)
            int capInset = 2;

            Color baseColor = isUnlocked ? new Color(0.13f, 0.28f, 0.62f) : new Color(0.08f, 0.12f, 0.20f);
            Color topGlowColor = isUnlocked ? new Color(0.24f, 0.52f, 0.90f) : new Color(0.14f, 0.18f, 0.30f);
            Color shadowColor = isUnlocked ? new Color(0.06f, 0.14f, 0.36f) : new Color(0.04f, 0.06f, 0.10f); // Pure tone-on-tone dark navy 3D shelf!
            Color rimColor = isUnlocked ? new Color(0.38f, 0.72f, 1.0f, 0.85f) : new Color(0.20f, 0.26f, 0.38f, 0.50f);

            Texture2D tex = CreateCrispTexture(width, height);
            Color[] pixels = new Color[width * height];

            int capBottom = extrusionDepth;
            int capTop = height - 1 - capInset;
            int capLeft = capInset;
            int capRight = width - 1 - capInset;
            int capHeight = capTop - capBottom;
            int capRadius = radius - capInset;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Outer capsule distance
                    float dxOuter = 0f;
                    if (x < radius) dxOuter = radius - x;
                    else if (x >= (width - radius)) dxOuter = x - (width - 1 - radius);

                    float dyOuter = 0f;
                    if (y < radius) dyOuter = radius - y;
                    else if (y >= (height - radius)) dyOuter = y - (height - 1 - radius);

                    float distOuter = Mathf.Sqrt(dxOuter * dxOuter + dyOuter * dyOuter) / (float)radius;
                    if (distOuter > 1.0f)
                    {
                        pixels[y * width + x] = Color.clear;
                        continue;
                    }

                    float alpha = Mathf.Clamp01((1.0f - distOuter) * 16f);

                    // Inner raised cap
                    bool inCap = false;
                    float distCap = 2.0f;
                    if (y >= capBottom && x >= capLeft && x <= capRight)
                    {
                        float dxCap = 0f;
                        if (x < (capLeft + capRadius)) dxCap = (capLeft + capRadius) - x;
                        else if (x >= (capRight - capRadius)) dxCap = x - (capRight - capRadius);

                        float dyCap = 0f;
                        if (y < (capBottom + capRadius)) dyCap = (capBottom + capRadius) - y;
                        else if (y >= (capTop - capRadius)) dyCap = y - (capTop - capRadius);

                        distCap = Mathf.Sqrt(dxCap * dxCap + dyCap * dyCap) / (float)capRadius;
                        if (distCap <= 1.0f) inCap = true;
                    }

                    Color c;
                    if (inCap && distCap <= 0.98f)
                    {
                        // Raised pressable face
                        float capNormY = (float)(y - capBottom) / (float)capHeight;

                        // Vertical candy gradient: rich base to luminous electric blue
                        c = Color.Lerp(baseColor, topGlowColor, capNormY);

                        // Curved glass specular highlight on upper 38%
                        if (capNormY > 0.55f)
                        {
                            float shineT = (capNormY - 0.55f) / 0.45f;
                            float shine = Mathf.Sin(shineT * Mathf.PI * 0.92f) * 0.38f;
                            c = Color.Lerp(c, Color.white, shine);
                        }

                        // Top rim highlight
                        if (capNormY > 0.88f)
                        {
                            float rimT = (capNormY - 0.88f) / 0.12f;
                            c = Color.Lerp(c, rimColor, rimT * 0.75f);
                        }
                    }
                    else
                    {
                        // 3D bottom bevel shelf
                        float shelfNorm = Mathf.Clamp01((float)y / (float)extrusionDepth);
                        c = Color.Lerp(shadowColor * 0.85f, shadowColor, shelfNorm);

                        if (y < capBottom && (capBottom - y) <= 2)
                        {
                            c *= 0.72f; // Ambient crease
                        }
                    }

                    c.a *= alpha;
                    pixels[y * width + x] = c;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Vector4 cardBorder = new Vector4(radius + 2, extrusionDepth + 2, radius + 2, radius / 2 + 2);
            Sprite cardSprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, cardBorder);
            s_SpriteCache[key] = cardSprite;
            return cardSprite;
        }

        /// <summary>
        /// Generates a perfectly proportioned 3D pill capsule badge that never squashes or deforms.
        /// </summary>
        public static Sprite Get3DPillBadgeSprite(Color baseColor, Color shadowColor)
        {
            string key = $"PillBadge3D_{ColorUtility.ToHtmlStringRGBA(baseColor)}_{ColorUtility.ToHtmlStringRGBA(shadowColor)}";
            if (s_SpriteCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int width = 96;
            int height = 48;
            int radius = 24; // Pure semicircular ends
            int extrusion = 5;

            Texture2D tex = CreateCrispTexture(width, height);
            Color[] pixels = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                float normY = (float)y / (float)(height - 1);
                for (int x = 0; x < width; x++)
                {
                    float dx = 0f;
                    if (x < radius) dx = radius - x;
                    else if (x >= (width - radius)) dx = x - (width - 1 - radius);

                    float dy = 0f;
                    if (y < radius) dy = radius - y;
                    else if (y >= (height - radius)) dy = y - (height - 1 - radius);

                    float dist = Mathf.Sqrt(dx * dx + dy * dy) / (float)radius;
                    if (dist > 1.0f)
                    {
                        pixels[y * width + x] = Color.clear;
                        continue;
                    }

                    float alpha = Mathf.Clamp01((1.0f - dist) * 16f);

                    Color c;
                    if (y >= extrusion)
                    {
                        // Raised pill cap
                        float capNormY = (float)(y - extrusion) / (float)(height - 1 - extrusion);
                        c = Color.Lerp(baseColor * 0.92f, baseColor + Color.white * 0.28f, capNormY);

                        if (capNormY > 0.50f)
                        {
                            float shine = Mathf.Sin((capNormY - 0.50f) / 0.50f * Mathf.PI * 0.90f) * 0.40f;
                            c = Color.Lerp(c, Color.white, shine);
                        }
                    }
                    else
                    {
                        // 3D bottom bevel
                        c = shadowColor;
                    }

                    c.a *= alpha;
                    pixels[y * width + x] = c;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Vector4 border = new Vector4(radius + 2, extrusion + 2, radius + 2, radius / 2 + 2);
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
            s_SpriteCache[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// Generates a floating 3D modal dialog window panel with a double golden beveled frame
        /// and deep cosmic royal navy interior.
        /// </summary>
        public static Sprite Get3DDialogPanelSprite()
        {
            const string key = "DialogPanel_3D_GoldBevel";
            if (s_SpriteCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int size = 192;
            int radius = 38;
            int borderThickness = 9;

            Color goldOuter = new Color(0.92f, 0.68f, 0.12f);
            Color goldHighlight = new Color(1.0f, 0.92f, 0.48f);
            Color innerNavy = new Color(0.08f, 0.13f, 0.24f, 0.98f);
            Color cyanGlowBorder = new Color(0.18f, 0.55f, 0.88f, 0.80f);

            Texture2D tex = CreateCrispTexture(size, size);
            Color[] pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                float normY = (float)y / (float)(size - 1);
                for (int x = 0; x < size; x++)
                {
                    float dist = GetRoundedRectDistance(x, y, size, size, radius);
                    if (dist > 1.0f)
                    {
                        pixels[y * size + x] = Color.clear;
                        continue;
                    }

                    float alpha = Mathf.Clamp01((1.0f - dist) * 16f);

                    Color c;
                    float distFromEdge = (1.0f - dist) * radius;

                    if (distFromEdge < 3.5f)
                    {
                        // Outer gold bevel with top light
                        c = Color.Lerp(goldOuter, goldHighlight, normY);
                    }
                    else if (distFromEdge < 7.5f)
                    {
                        // Glowing cyan accent channel
                        c = cyanGlowBorder;
                    }
                    else if (distFromEdge < borderThickness)
                    {
                        // Inner gold trim
                        c = goldOuter * 0.9f;
                    }
                    else
                    {
                        // Deep royal navy interior with subtle radial gradient
                        float innerDist = Mathf.Clamp01((distFromEdge - borderThickness) / 25f);
                        c = Color.Lerp(innerNavy * 0.80f, innerNavy, innerDist);
                    }

                    c.a *= alpha;
                    pixels[y * size + x] = c;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Vector4 panelBorder = new Vector4(radius + 4, radius + 4, radius + 4, radius + 4);
            Sprite panelSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, panelBorder);
            s_SpriteCache[key] = panelSprite;
            return panelSprite;
        }

        /// <summary>
        /// Generates a juicy 3D circular candy button with extrusion shelf,
        /// curved specular gloss shine, and crisp rim highlight (perfect for Close X buttons).
        /// </summary>
        public static Sprite Get3DCircleButtonSprite(Color baseColor, Color shadowColor)
        {
            string key = $"CircleBtn_3D_{ColorUtility.ToHtmlStringRGBA(baseColor)}_{ColorUtility.ToHtmlStringRGBA(shadowColor)}";
            if (s_SpriteCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int size = 128;
            float r = (size - 6) * 0.5f;
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float extrusion = 14f;

            Texture2D tex = CreateCrispTexture(size, size);
            Color[] pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center.x;
                    float dy = y - center.y;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist > r)
                    {
                        pixels[y * size + x] = Color.clear;
                        continue;
                    }

                    float alpha = Mathf.Clamp01((r - dist) * 4f);

                    float capDy = y - (center.y + extrusion * 0.35f);
                    float capDist = Mathf.Sqrt(dx * dx + capDy * capDy);

                    Color c;
                    if (capDist <= (r - extrusion * 0.4f))
                    {
                        float normY = Mathf.Clamp01((capDy + r) / (r * 2f));
                        c = Color.Lerp(baseColor * 0.90f, baseColor + Color.white * 0.30f, normY);

                        float shineDx = dx + r * 0.25f;
                        float shineDy = capDy - r * 0.30f;
                        float shineDist = Mathf.Sqrt(shineDx * shineDx + shineDy * shineDy);
                        if (shineDist < r * 0.45f)
                        {
                            float shine = Mathf.Pow(Mathf.Clamp01(1f - shineDist / (r * 0.45f)), 2f) * 0.55f;
                            c = Color.Lerp(c, Color.white, shine);
                        }
                    }
                    else
                    {
                        float shelfT = Mathf.Clamp01((y - (center.y - r)) / extrusion);
                        c = shadowColor * (0.75f + 0.25f * shelfT);
                    }

                    c.a *= alpha;
                    pixels[y * size + x] = c;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            s_SpriteCache[key] = sprite;
            return sprite;
        }

        public static Sprite GetHomeBlueGradientSprite()
        {
            const string key = "HomeBlueGradient";
            if (s_SpriteCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int w = 64;
            int h = 256;
            Texture2D tex = CreateCrispTexture(w, h);
            Color[] pixels = new Color[w * h];

            Color topCol = new Color(0.11f, 0.42f, 0.88f);    // Vibrant Electric Blue
            Color midCol = new Color(0.10f, 0.33f, 0.76f);    // Deep Royal Blue
            Color botCol = new Color(0.08f, 0.24f, 0.60f);    // Bottom Navy Blue

            for (int y = 0; y < h; y++)
            {
                float t = (float)y / (h - 1);
                Color c = (t > 0.5f) ? Color.Lerp(midCol, topCol, (t - 0.5f) * 2f) : Color.Lerp(botCol, midCol, t * 2f);
                for (int x = 0; x < w; x++)
                {
                    pixels[y * w + x] = c;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
            s_SpriteCache[key] = sprite;
            return sprite;
        }

        public static Sprite GetPanelSprite(Color fillColor, Color borderColor, float borderThickness = 2.5f, int cornerRadius = 24)
        {
            string key = $"Panel_{ColorUtility.ToHtmlStringRGBA(fillColor)}_{ColorUtility.ToHtmlStringRGBA(borderColor)}_{borderThickness:F1}_{cornerRadius}";
            if (s_SpriteCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int size = 128;
            int radius = Mathf.Clamp(cornerRadius, 12, 36);
            Texture2D tex = CreateCrispTexture(size, size);

            Color[] pixels = new Color[size * size];
            float borderRatio = borderThickness / radius;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = GetRoundedRectDistance(x, y, size, size, radius);
                    if (dist > 1.0f)
                    {
                        pixels[y * size + x] = Color.clear;
                        continue;
                    }

                    float alpha = Mathf.Clamp01((1.0f - dist) * 20f);
                    float borderThreshold = 1.0f - borderRatio;
                    Color c = (dist >= borderThreshold) ? borderColor : fillColor;
                    c.a *= alpha;
                    pixels[y * size + x] = c;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            int sliceMargin = radius + 4;
            Vector4 border = new Vector4(sliceMargin, sliceMargin, sliceMargin, sliceMargin);
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
            s_SpriteCache[key] = sprite;
            return sprite;
        }

        public static Sprite GetPillBadgeSprite(Color fillColor)
        {
            string key = $"PillBadge_{ColorUtility.ToHtmlStringRGBA(fillColor)}";
            if (s_SpriteCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int width = 128;
            int height = 56;
            int radius = 26;
            Texture2D tex = CreateCrispTexture(width, height);

            Color[] pixels = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dist = GetRoundedRectDistance(x, y, width, height, radius);
                    if (dist > 1.0f)
                    {
                        pixels[y * width + x] = Color.clear;
                        continue;
                    }

                    float alpha = Mathf.Clamp01((1.0f - dist) * 20f);
                    Color c = fillColor;
                    c.a *= alpha;
                    pixels[y * width + x] = c;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), height);
            s_SpriteCache[key] = sprite;
            return sprite;
        }

        public static Sprite GetCircleSprite(Color fillColor)
        {
            return GetCircleButtonSprite(fillColor, fillColor, 0f);
        }

        public static Sprite GetCircleButtonSprite(Color fillColor, Color borderColor, float borderRatio = 0.08f)
        {
            string key = $"CircleBtn_{ColorUtility.ToHtmlStringRGBA(fillColor)}_{ColorUtility.ToHtmlStringRGBA(borderColor)}_{borderRatio:F2}";
            if (s_SpriteCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int size = 128;
            Texture2D tex = CreateCrispTexture(size, size);
            Color[] pixels = new Color[size * size];
            float center = (size - 1) * 0.5f;
            float radius = (size - 6) * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy) / radius;
                    if (dist > 1.0f)
                    {
                        pixels[y * size + x] = Color.clear;
                        continue;
                    }

                    float alpha = Mathf.Clamp01((1.0f - dist) * 16f);
                    float borderThreshold = 1.0f - borderRatio;
                    Color c = (dist >= borderThreshold) ? borderColor : fillColor;
                    c.a *= alpha;
                    pixels[y * size + x] = c;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            s_SpriteCache[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// 3D Pixar-style Golden Embossed Modal Panel with double golden bevel rim
        /// and deep cosmic royal blue glass interior. 9-slice compatible.
        /// </summary>
        public static Sprite Get3DGoldFramePanelSprite()
        {
            const string key = "Panel_3D_GoldEmbossed_Frame_HD";
            if (s_SpriteCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int size = 128;
            int radius = 30;
            Texture2D tex = CreateCrispTexture(size, size);
            Color[] pixels = new Color[size * size];

            Color goldLight = new Color(1.0f, 0.94f, 0.52f, 1f);     // #fff085
            Color goldMid = new Color(0.98f, 0.74f, 0.12f, 1f);       // #fbbd1f
            Color goldDark = new Color(0.78f, 0.44f, 0.04f, 1f);      // #c7700a
            Color goldShadow = new Color(0.42f, 0.18f, 0.02f, 1f);    // #6b2e05
            Color innerRidge = new Color(1.0f, 0.88f, 0.40f, 0.75f);
            Color bgTop = new Color(0.06f, 0.12f, 0.28f, 0.98f);      // #0f1f47
            Color bgBottom = new Color(0.03f, 0.06f, 0.16f, 0.98f);   // #081029

            for (int y = 0; y < size; y++)
            {
                float normY = (float)y / (size - 1);
                for (int x = 0; x < size; x++)
                {
                    float dist = GetRoundedRectDistance(x, y, size, size, radius);
                    if (dist > 1.0f)
                    {
                        pixels[y * size + x] = Color.clear;
                        continue;
                    }

                    float alpha = Mathf.Clamp01((1.0f - dist) * 20f);
                    Color c;

                    if (dist >= 0.88f)
                    {
                        // Outer gold rim highlight / shadow
                        c = (normY >= 0.45f) ? Color.Lerp(goldMid, goldLight, (normY - 0.45f) / 0.55f)
                                             : Color.Lerp(goldShadow, goldDark, normY / 0.45f);
                    }
                    else if (dist >= 0.70f)
                    {
                        // Main metallic gold body
                        float bandNorm = (dist - 0.70f) / 0.18f;
                        c = Color.Lerp(goldDark, goldMid, bandNorm);
                        if (dist >= 0.82f) c = Color.Lerp(c, goldLight, 0.35f);
                    }
                    else if (dist >= 0.64f)
                    {
                        // Inner golden bevel edge / rim highlight
                        c = (dist >= 0.67f) ? innerRidge : goldShadow;
                    }
                    else
                    {
                        // Deep royal cosmic interior with subtle vertical glass gradient
                        c = Color.Lerp(bgBottom, bgTop, normY);
                    }

                    c.a *= alpha;
                    pixels[y * size + x] = c;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            int sliceMargin = radius + 4;
            Vector4 border = new Vector4(sliceMargin, sliceMargin, sliceMargin, sliceMargin);
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
            s_SpriteCache[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// Glassy inset card capsule for setting rows with deep royal blue body and subtle cyan-gold border.
        /// </summary>
        public static Sprite GetSettingsRowCapsuleSprite()
        {
            const string key = "Capsule_SettingsRow_Glass_HD";
            if (s_SpriteCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int size = 128;
            int radius = 24;
            Texture2D tex = CreateCrispTexture(size, size);
            Color[] pixels = new Color[size * size];

            Color borderLight = new Color(0.20f, 0.42f, 0.78f, 0.85f);
            Color borderDark = new Color(0.10f, 0.22f, 0.46f, 0.65f);
            Color bgTop = new Color(0.08f, 0.16f, 0.34f, 0.92f);
            Color bgBottom = new Color(0.05f, 0.09f, 0.22f, 0.92f);

            for (int y = 0; y < size; y++)
            {
                float normY = (float)y / (size - 1);
                for (int x = 0; x < size; x++)
                {
                    float dist = GetRoundedRectDistance(x, y, size, size, radius);
                    if (dist > 1.0f)
                    {
                        pixels[y * size + x] = Color.clear;
                        continue;
                    }

                    float alpha = Mathf.Clamp01((1.0f - dist) * 20f);
                    Color c;
                    if (dist >= 0.86f)
                    {
                        c = Color.Lerp(borderDark, borderLight, normY);
                    }
                    else
                    {
                        c = Color.Lerp(bgBottom, bgTop, normY);
                    }

                    c.a *= alpha;
                    pixels[y * size + x] = c;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            int sliceMargin = radius + 4;
            Vector4 border = new Vector4(sliceMargin, sliceMargin, sliceMargin, sliceMargin);
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
            s_SpriteCache[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// 3D tactile pill track for toggle switches (vibrant emerald green for ON, matte slate grey for OFF).
        /// </summary>
        public static Sprite Get3DToggleTrackSprite(bool isOn)
        {
            string key = $"ToggleTrack_3D_{(isOn ? "ON" : "OFF")}";
            if (s_SpriteCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int width = 128;
            int height = 64;
            int radius = 30;
            Texture2D tex = CreateCrispTexture(width, height);
            Color[] pixels = new Color[width * height];

            Color trackTop = isOn ? new Color(0.14f, 0.78f, 0.38f, 1f) : new Color(0.28f, 0.34f, 0.44f, 1f);
            Color trackBottom = isOn ? new Color(0.06f, 0.52f, 0.22f, 1f) : new Color(0.18f, 0.22f, 0.30f, 1f);
            Color rim = isOn ? new Color(0.35f, 0.92f, 0.55f, 0.95f) : new Color(0.40f, 0.48f, 0.60f, 0.85f);
            Color innerShadow = isOn ? new Color(0.04f, 0.36f, 0.15f, 1f) : new Color(0.12f, 0.15f, 0.22f, 1f);

            for (int y = 0; y < height; y++)
            {
                float normY = (float)y / (height - 1);
                for (int x = 0; x < width; x++)
                {
                    float dist = GetRoundedRectDistance(x, y, width, height, radius);
                    if (dist > 1.0f)
                    {
                        pixels[y * width + x] = Color.clear;
                        continue;
                    }

                    float alpha = Mathf.Clamp01((1.0f - dist) * 20f);
                    Color c;
                    if (dist >= 0.85f)
                    {
                        c = rim;
                    }
                    else if (normY >= 0.72f)
                    {
                        // Inset shadow at top of track for 3D depth
                        c = Color.Lerp(trackTop, innerShadow, (normY - 0.72f) / 0.28f);
                    }
                    else
                    {
                        c = Color.Lerp(trackBottom, trackTop, normY / 0.72f);
                    }

                    c.a *= alpha;
                    pixels[y * width + x] = c;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Vector4 border = new Vector4(radius + 2, radius + 2, radius + 2, radius + 2);
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
            s_SpriteCache[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// Glossy 3D spherical white pearl knob for toggle switches with top-left specular gleam and soft drop shadow.
        /// </summary>
        public static Sprite Get3DToggleKnobSprite()
        {
            const string key = "ToggleKnob_3D_Pearl_HD";
            if (s_SpriteCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int size = 96;
            Texture2D tex = CreateCrispTexture(size, size);
            Color[] pixels = new Color[size * size];

            float cX = (size - 1) * 0.5f;
            float cY = (size - 1) * 0.5f;
            float r = (size - 8) * 0.5f;

            Color whiteLight = new Color(1f, 1f, 1f, 1f);
            Color pearlBody = new Color(0.94f, 0.96f, 0.98f, 1f);
            Color pearlShadow = new Color(0.72f, 0.78f, 0.86f, 1f);
            Color rim = new Color(1f, 1f, 1f, 0.9f);

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

                    // 3D spherical gradient with top-left highlight
                    float specDist = Mathf.Sqrt((dx + 0.32f) * (dx + 0.32f) + (dy - 0.32f) * (dy - 0.32f));
                    Color c = Color.Lerp(pearlShadow, pearlBody, (dy + 1f) * 0.5f);

                    if (specDist < 0.40f)
                    {
                        c = Color.Lerp(c, whiteLight, Mathf.Clamp01((0.40f - specDist) / 0.28f));
                    }

                    if (distSq >= 0.88f)
                    {
                        c = Color.Lerp(c, rim, 0.4f);
                    }

                    c.a *= alpha;
                    pixels[y * size + x] = c;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            s_SpriteCache[key] = sprite;
            return sprite;
        }

        private static float GetRoundedRectDistance(int x, int y, int width, int height, int radius)
        {
            int cx = Mathf.Clamp(x, radius, width - 1 - radius);
            int cy = Mathf.Clamp(y, radius, height - 1 - radius);
            float dx = x - cx;
            float dy = y - cy;
            return Mathf.Sqrt(dx * dx + dy * dy) / radius;
        }

        /// <summary>
        /// Generates a blazing neon laser beam sprite with a bright white core and soft glowing aura.
        /// Stretches along cleared rows and columns for high-impact sweep blast animations.
        /// </summary>
        public static Sprite GetLaserBeamSprite()
        {
            const string key = "LaserBeam_HD";
            if (s_SpriteCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int width = 128;
            int height = 32;
            Texture2D tex = CreateCrispTexture(width, height);
            Color[] pixels = new Color[width * height];
            float halfH = (height - 1) * 0.5f;
            float halfW = (width - 1) * 0.5f;

            for (int y = 0; y < height; y++)
            {
                float ny = Mathf.Abs((y - halfH) / halfH);
                float core = Mathf.Pow(Mathf.Clamp01(1f - ny), 6f);
                float glow = Mathf.Pow(Mathf.Clamp01(1f - ny), 1.6f);

                for (int x = 0; x < width; x++)
                {
                    float nx = Mathf.Abs((x - halfW) / halfW);
                    float xFade = Mathf.Clamp01((1f - nx) * 5f);

                    float totalAlpha = (core * 0.7f + glow * 0.3f) * xFade;
                    Color col = Color.Lerp(Color.white, new Color(1f, 1f, 1f, 0.85f), glow);
                    col.a = Mathf.Clamp01(totalAlpha);
                    pixels[y * width + x] = col;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 32f);
            s_SpriteCache[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// Generates a dazzling 4-point lens flare star with glowing circular core.
        /// Used for line intersections and high-energy impact blasts.
        /// </summary>
        public static Sprite GetLensFlareStarSprite()
        {
            const string key = "LensFlareStar_HD";
            if (s_SpriteCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int size = 128;
            Texture2D tex = CreateCrispTexture(size, size);
            Color[] pixels = new Color[size * size];
            float c = (size - 1) * 0.5f;

            for (int y = 0; y < size; y++)
            {
                float dy = (y - c) / c;
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - c) / c;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);

                    // Central circular glow
                    float coreGlow = Mathf.Pow(Mathf.Clamp01(1f - r * 2.2f), 3f);

                    // Horizontal and vertical cross flares
                    float rayX = Mathf.Pow(Mathf.Clamp01(1f - Mathf.Abs(dy) * 9f), 2f) * Mathf.Clamp01(1f - Mathf.Abs(dx));
                    float rayY = Mathf.Pow(Mathf.Clamp01(1f - Mathf.Abs(dx) * 9f), 2f) * Mathf.Clamp01(1f - Mathf.Abs(dy));

                    // Diagonal softer spikes
                    float d1 = Mathf.Abs(dx - dy) * 0.7071f;
                    float d2 = Mathf.Abs(dx + dy) * 0.7071f;
                    float diag1 = Mathf.Pow(Mathf.Clamp01(1f - d1 * 10f), 2f) * Mathf.Clamp01(1f - r * 1.5f);
                    float diag2 = Mathf.Pow(Mathf.Clamp01(1f - d2 * 10f), 2f) * Mathf.Clamp01(1f - r * 1.5f);

                    float total = coreGlow + (rayX + rayY) * 0.9f + (diag1 + diag2) * 0.45f;
                    total = Mathf.Clamp01(total);

                    Color col = Color.white;
                    col.a = total;
                    pixels[y * size + x] = col;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64f);
            s_SpriteCache[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// Ultra-crisp 4K 3D Chubby Cartoon Golden Crown matching Block Blast mobile game crown.
        /// </summary>
        public static Sprite GetChubbyCrownHDSprite()
        {
            const string key = "Icon_ChubbyGoldenCrown_4K_HD";
            if (s_SpriteCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            Sprite loaded = Resources.Load<Sprite>("ChubbyGoldenCrownHD");
            if (loaded != null)
            {
                s_SpriteCache[key] = loaded;
                return loaded;
            }

            Texture2D tex = Resources.Load<Texture2D>("ChubbyGoldenCrownHD");
            if (tex == null)
            {
                string path = System.IO.Path.Combine(Application.dataPath, "Resources", "ChubbyGoldenCrownHD.png");
                if (System.IO.File.Exists(path))
                {
                    byte[] bytes = System.IO.File.ReadAllBytes(path);
                    tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    tex.LoadImage(bytes);
                }
            }

            if (tex != null)
            {
                tex.filterMode = FilterMode.Bilinear;
                Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                s_SpriteCache[key] = sprite;
                return sprite;
            }

            return GemIconFactory.GetChubbyCrownSprite();
        }

        /// <summary>
        /// Ultra-crisp 4K 3D Chubby Candy Balloon Title Logo matching Block Blast mobile game aesthetic.
        /// </summary>
        public static Sprite GetSphereBlastTitleLogoSprite()
        {
            const string key = "Logo_SphereBlast_4K_HD";
            if (s_SpriteCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            Sprite loaded = Resources.Load<Sprite>("SphereBlastLogoHD");
            if (loaded != null && loaded.texture != null)
            {
                // If the sprite was sliced into sub-sprites (e.g. cutting off BLAST), create a full-coverage sprite
                if (loaded.rect.height < loaded.texture.height * 0.95f || loaded.rect.width < loaded.texture.width * 0.95f)
                {
                    Sprite fullSprite = Sprite.Create(
                        loaded.texture,
                        new Rect(0, 0, loaded.texture.width, loaded.texture.height),
                        new Vector2(0.5f, 0.5f),
                        100f
                    );
                    s_SpriteCache[key] = fullSprite;
                    return fullSprite;
                }

                s_SpriteCache[key] = loaded;
                return loaded;
            }

            Texture2D tex = Resources.Load<Texture2D>("SphereBlastLogoHD");
            if (tex == null)
            {
                string path = System.IO.Path.Combine(Application.dataPath, "Resources", "SphereBlastLogoHD.png");
                if (System.IO.File.Exists(path))
                {
                    byte[] bytes = System.IO.File.ReadAllBytes(path);
                    tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    tex.LoadImage(bytes);
                }
            }

            if (tex != null)
            {
                tex.filterMode = FilterMode.Bilinear;
                Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                s_SpriteCache[key] = sprite;
                return sprite;
            }

            return null;
        }

        // =========================================================================
        // 🌌 CONCEPT 3: COSMIC VIGNETTE STUDIO GRADIENT
        // =========================================================================
        public static Sprite GetCosmicVignetteGradientSprite()
        {
            const string key = "Bg_CosmicVignette_Studio";
            if (s_SpriteCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int w = 256;
            int h = 512;
            Texture2D tex = CreateCrispTexture(w, h);
            Color[] pixels = new Color[w * h];

            Vector2 center = new Vector2(w * 0.5f, h * 0.48f); // Center slightly above middle where starburst sits
            Color centerColor = new Color(0.08f, 0.32f, 0.85f); // Vibrant royal blue
            Color midColor = new Color(0.04f, 0.16f, 0.52f);    // Deep sapphire
            Color edgeColor = new Color(0.02f, 0.06f, 0.22f);   // Midnight navy
            Color cornerColor = new Color(0.01f, 0.03f, 0.12f); // Pitch space navy

            float maxDist = Mathf.Sqrt(center.x * center.x + center.y * center.y);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float dx = (x - center.x) / (w * 0.5f);
                    float dy = (y - center.y) / (h * 0.5f);
                    float r = Mathf.Sqrt(dx * dx + dy * dy);

                    Color c;
                    if (r < 0.5f)
                    {
                        c = Color.Lerp(centerColor, midColor, r / 0.5f);
                    }
                    else if (r < 1.0f)
                    {
                        c = Color.Lerp(midColor, edgeColor, (r - 0.5f) / 0.5f);
                    }
                    else
                    {
                        c = Color.Lerp(edgeColor, cornerColor, Mathf.Clamp01((r - 1.0f) / 0.4f));
                    }

                    pixels[y * w + x] = c;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
            s_SpriteCache[key] = sprite;
            return sprite;
        }

        // =========================================================================
        // ✨ CONCEPT 3: VOLUMETRIC STARBURST RAYS
        // =========================================================================
        public static Sprite GetStarburstRaysSprite()
        {
            const string key = "Fx_StarburstRays_HD";
            if (s_SpriteCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int size = 512;
            Texture2D tex = CreateCrispTexture(size, size);
            Color[] pixels = new Color[size * size];
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float maxR = size * 0.48f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center.x;
                    float dy = y - center.y;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);

                    if (r > maxR)
                    {
                        pixels[y * size + x] = Color.clear;
                        continue;
                    }

                    float angle = Mathf.Atan2(dy, dx);
                    // 12 volumetric light beams with alternating intensities
                    float beam1 = Mathf.Pow(Mathf.Max(0f, Mathf.Cos(angle * 6f)), 8f);
                    float beam2 = Mathf.Pow(Mathf.Max(0f, Mathf.Cos(angle * 6f + Mathf.PI * 0.5f)), 14f) * 0.5f;
                    float beam = Mathf.Clamp01(beam1 + beam2);

                    // Soft radial falloff from center to edge
                    float distFalloff = Mathf.Pow(1f - (r / maxR), 1.5f);
                    float centerCore = Mathf.Pow(Mathf.Clamp01(1f - (r / (maxR * 0.28f))), 2f);

                    float alpha = Mathf.Clamp01((beam * 0.28f + centerCore * 0.45f) * distFalloff);

                    Color rayCol = Color.Lerp(new Color(0.35f, 0.78f, 1f), Color.white, centerCore);
                    rayCol.a = alpha;

                    pixels[y * size + x] = rayCol;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            s_SpriteCache[key] = sprite;
            return sprite;
        }

        // =========================================================================
        // ⭐ CONCEPT 3: 3D CANDY STARBURST CORE
        // =========================================================================
        public static Sprite Get3DStarCoreSprite()
        {
            const string key = "Fx_3D_StarCore_Candy";
            if (s_SpriteCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int size = 256;
            Texture2D tex = CreateCrispTexture(size, size);
            Color[] pixels = new Color[size * size];
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);

            float outerR = 86f;
            float innerR = 40f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center.x;
                    float dy = y - center.y;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    float angle = Mathf.Atan2(dy, dx) - Mathf.PI * 0.5f;

                    // 5-pointed star equation
                    float a = Mathf.Repeat(angle + Mathf.PI * 0.2f, Mathf.PI * 0.4f) - Mathf.PI * 0.2f;
                    float starR = innerR * outerR / Mathf.Max(0.01f, innerR * Mathf.Abs(Mathf.Sin(a)) + outerR * Mathf.Abs(Mathf.Cos(a)));

                    // Radiating multi-color light flare needles (cross + diagonal flares)
                    float flareV = Mathf.Exp(-Mathf.Abs(dx) * 0.25f) * Mathf.Max(0f, 1f - Mathf.Abs(dy) / (size * 0.48f));
                    float flareH = Mathf.Exp(-Mathf.Abs(dy) * 0.25f) * Mathf.Max(0f, 1f - Mathf.Abs(dx) / (size * 0.48f));
                    float flareDiag1 = Mathf.Exp(-Mathf.Abs(dx - dy) * 0.22f) * Mathf.Max(0f, 1f - r / (size * 0.42f));
                    float flareDiag2 = Mathf.Exp(-Mathf.Abs(dx + dy) * 0.22f) * Mathf.Max(0f, 1f - r / (size * 0.42f));
                    float flare = Mathf.Clamp01((flareV + flareH) * 0.65f + (flareDiag1 + flareDiag2) * 0.45f);

                    if (r > starR + 2f)
                    {
                        // Outer flare aura
                        if (flare > 0.02f)
                        {
                            Color flareCol = Color.Lerp(new Color(0.35f, 0.85f, 1f), Color.white, flare);
                            flareCol.a = flare * 0.65f;
                            pixels[y * size + x] = flareCol;
                        }
                        else
                        {
                            pixels[y * size + x] = Color.clear;
                        }
                        continue;
                    }

                    // Inside Star Body: 3D Candy Radial Shading
                    float normR = r / starR;
                    float edgeAA = Mathf.Clamp01((starR + 2f - r) * 1.5f);

                    // Specular highlight spot near top-left of star
                    float specDist = Mathf.Sqrt(Mathf.Pow(dx + 16f, 2f) + Mathf.Pow(dy - 18f, 2f));
                    float spec = Mathf.Pow(Mathf.Clamp01(1f - specDist / 38f), 2.5f) * 0.90f;

                    // Pastel rainbow bevel along star border
                    float angleDeg = Mathf.Repeat((angle * Mathf.Rad2Deg + 360f), 360f);
                    Color rimRainbow;
                    if (angleDeg < 90f) rimRainbow = Color.Lerp(new Color(1f, 0.35f, 0.55f), new Color(1f, 0.85f, 0.25f), angleDeg / 90f);
                    else if (angleDeg < 180f) rimRainbow = Color.Lerp(new Color(1f, 0.85f, 0.25f), new Color(0.25f, 0.90f, 0.55f), (angleDeg - 90f) / 90f);
                    else if (angleDeg < 270f) rimRainbow = Color.Lerp(new Color(0.25f, 0.90f, 0.55f), new Color(0.20f, 0.75f, 1f), (angleDeg - 180f) / 90f);
                    else rimRainbow = Color.Lerp(new Color(0.20f, 0.75f, 1f), new Color(1f, 0.35f, 0.55f), (angleDeg - 270f) / 90f);

                    // Core: bright white-gold glowing center
                    Color coreCol = Color.Lerp(Color.white, new Color(1f, 0.96f, 0.65f), normR);
                    Color starColor = Color.Lerp(coreCol, rimRainbow, Mathf.Pow(normR, 1.8f) * 0.85f);
                    starColor = Color.Lerp(starColor, Color.white, spec);

                    starColor.a = edgeAA;
                    pixels[y * size + x] = starColor;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            s_SpriteCache[key] = sprite;
            return sprite;
        }

        // =========================================================================
        // 💫 CONCEPT 3: DUAL GLOWING ORBIT RINGS
        // =========================================================================
        public static Sprite GetGlowOrbitRingSprite()
        {
            const string key = "Fx_DualGlow_OrbitRings";
            if (s_SpriteCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int w = 512;
            int h = 320;
            Texture2D tex = CreateCrispTexture(w, h);
            Color[] pixels = new Color[w * h];
            Vector2 center = new Vector2(w * 0.5f, h * 0.5f);

            // Ring 1: Primary Orbit
            float a1 = 186f;
            float b1 = 104f;
            float thick1 = 4.5f;

            // Ring 2: Secondary Outer Glow Ring
            float a2 = 212f;
            float b2 = 118f;
            float thick2 = 3.0f;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float dx = x - center.x;
                    float dy = y - center.y;

                    // Ellipse 1 distance
                    float d1 = Mathf.Abs(Mathf.Sqrt(Mathf.Pow(dx / a1, 2f) + Mathf.Pow(dy / b1, 2f)) - 1.0f) * a1;
                    // Ellipse 2 distance
                    float d2 = Mathf.Abs(Mathf.Sqrt(Mathf.Pow(dx / a2, 2f) + Mathf.Pow(dy / b2, 2f)) - 1.0f) * a2;

                    float glow1 = Mathf.Exp(-d1 * 0.38f);
                    float glow2 = Mathf.Exp(-d2 * 0.55f);

                    float alpha1 = (d1 <= thick1) ? 0.90f : glow1 * 0.45f;
                    float alpha2 = (d2 <= thick2) ? 0.65f : glow2 * 0.28f;

                    Color c1 = new Color(0.20f, 0.78f, 1.0f, alpha1);
                    Color c2 = new Color(0.75f, 0.35f, 1.0f, alpha2);

                    Color finalCol = c1 + c2 * (1f - c1.a);
                    pixels[y * w + x] = finalCol;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
            s_SpriteCache[key] = sprite;
            return sprite;
        }

        // =========================================================================
        // 📍 CONCEPT 3: 3D MAP PIN BADGE (FOR ADVENTURE BUTTON)
        // =========================================================================
        public static Sprite Get3DMapBadgeSprite()
        {
            const string key = "Badge_3D_MapPin_Concept3";
            if (s_SpriteCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int size = 128;
            Texture2D tex = CreateCrispTexture(size, size);
            Color[] pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // 1. Folded Map Tablet background (isometric folded paper)
                    float mapX = x - 64f;
                    float mapY = y - 48f;
                    bool inMap = (mapX >= -42f && mapX <= 42f && mapY >= -26f && mapY <= 26f);

                    // 2. 3D Coral Map Pin
                    float pinCX = 64f;
                    float pinCY = 72f;
                    float pinHeadR = 24f;
                    Vector2 pinTip = new Vector2(64f, 44f);

                    float dxPin = x - pinCX;
                    float dyPin = y - pinCY;
                    float distHead = Mathf.Sqrt(dxPin * dxPin + dyPin * dyPin);

                    bool inHead = distHead <= pinHeadR;
                    bool inCone = false;
                    if (y >= pinTip.y && y <= pinCY)
                    {
                        float progress = (pinCY - y) / (pinCY - pinTip.y);
                        float coneW = Mathf.Lerp(pinHeadR * 0.92f, 0f, progress);
                        inCone = Mathf.Abs(dxPin) <= coneW;
                    }

                    // Inner White Dot in pin
                    bool inDot = distHead <= 8.5f;

                    Color pix = Color.clear;

                    if (inHead || inCone)
                    {
                        if (inDot)
                        {
                            pix = Color.white;
                        }
                        else
                        {
                            // 3D coral gradient + specular glint
                            float spec = Mathf.Pow(Mathf.Clamp01(1f - Mathf.Sqrt(Mathf.Pow(dxPin + 7f, 2f) + Mathf.Pow(dyPin - 7f, 2f)) / 14f), 2f);
                            Color coralBase = Color.Lerp(new Color(0.92f, 0.22f, 0.38f), new Color(1.0f, 0.48f, 0.60f), (dyPin + pinHeadR) / (pinHeadR * 2f));
                            pix = Color.Lerp(coralBase, Color.white, spec * 0.75f);
                        }
                    }
                    else if (inMap)
                    {
                        // Map paper: pastel mint with golden route line
                        float fold = Mathf.Abs(Mathf.Sin((x - 22f) * 0.12f)) * 0.15f;
                        Color mapCol = Color.Lerp(new Color(0.55f, 0.88f, 0.65f), new Color(0.85f, 0.96f, 0.75f), fold);
                        // Fold edge crease
                        if (Mathf.Abs(mapX - 14f) < 1.5f || Mathf.Abs(mapX + 14f) < 1.5f)
                        {
                            mapCol *= 0.85f;
                        }
                        // Golden path dashed
                        if (Mathf.Abs(mapY - mapX * 0.35f) < 2.5f)
                        {
                            mapCol = new Color(0.98f, 0.78f, 0.20f);
                        }
                        pix = mapCol;
                    }

                    pixels[y * size + x] = pix;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            s_SpriteCache[key] = sprite;
            return sprite;
        }

        // =========================================================================
        // ♾️ CONCEPT 3: 3D RAINBOW INFINITY JEWEL (FOR CLASSIC BUTTON)
        // =========================================================================
        public static Sprite Get3DRainbowInfinitySprite()
        {
            const string key = "Badge_3D_RainbowInfinity_Concept3";
            if (s_SpriteCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int w = 128;
            int h = 80;
            Texture2D tex = CreateCrispTexture(w, h);
            Color[] pixels = new Color[w * h];

            float cX = w * 0.5f;
            float cY = h * 0.5f;
            float loopDist = w * 0.23f;
            float loopR = h * 0.28f;
            float strokeHalf = 9.5f;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float dxL = x - (cX - loopDist);
                    float dxR = x - (cX + loopDist);
                    float dy = y - cY;

                    float distL = Mathf.Abs(Mathf.Sqrt(dxL * dxL + dy * dy) - loopR);
                    float distR = Mathf.Abs(Mathf.Sqrt(dxR * dxR + dy * dy) - loopR);
                    float d = Mathf.Min(distL, distR);

                    if (d <= strokeHalf + 1.2f)
                    {
                        float edgeAA = Mathf.Clamp01(strokeHalf + 1.2f - d);

                        // Rainbow gradient along X axis
                        float normX = (float)x / w;
                        Color rainbow;
                        if (normX < 0.25f) rainbow = Color.Lerp(new Color(0.20f, 0.75f, 1f), new Color(0.25f, 0.95f, 0.65f), normX / 0.25f);
                        else if (normX < 0.50f) rainbow = Color.Lerp(new Color(0.25f, 0.95f, 0.65f), new Color(1.0f, 0.88f, 0.20f), (normX - 0.25f) / 0.25f);
                        else if (normX < 0.75f) rainbow = Color.Lerp(new Color(1.0f, 0.88f, 0.20f), new Color(1.0f, 0.38f, 0.65f), (normX - 0.50f) / 0.25f);
                        else rainbow = Color.Lerp(new Color(1.0f, 0.38f, 0.65f), new Color(0.68f, 0.35f, 1f), (normX - 0.75f) / 0.25f);

                        // 3D specular crest on top curve of loop
                        float tubeCrest = 1f - Mathf.Pow(d / strokeHalf, 2f);
                        Color body = rainbow * (0.80f + tubeCrest * 0.35f);
                        if (tubeCrest > 0.60f && y > cY)
                        {
                            body = Color.Lerp(body, Color.white, (tubeCrest - 0.60f) * 1.8f);
                        }

                        body.a = edgeAA;
                        pixels[y * w + x] = body;
                    }
                    else
                    {
                        pixels[y * w + x] = Color.clear;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
            s_SpriteCache[key] = sprite;
            return sprite;
        }

        // =========================================================================
        // 🍬 CONCEPT 3: SOFT BOKEH 3D BACKGROUND CANDY SPHERE
        // =========================================================================
        public static Sprite GetSoftBokehSphereSprite(Color tint)
        {
            string key = $"BokehSphere_3D_{ColorUtility.ToHtmlStringRGBA(tint)}";
            if (s_SpriteCache.TryGetValue(key, out Sprite cached) && cached != null && cached.texture != null) return cached;

            int size = 128;
            Texture2D tex = CreateCrispTexture(size, size);
            Color[] pixels = new Color[size * size];
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float radius = size * 0.44f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - center.x) / radius;
                    float dy = (y - center.y) / radius;
                    float distSq = dx * dx + dy * dy;

                    if (distSq > 1.0f)
                    {
                        pixels[y * size + x] = Color.clear;
                        continue;
                    }

                    float dist = Mathf.Sqrt(distSq);
                    float edgeSoft = Mathf.Pow(Mathf.Clamp01(1.0f - dist), 0.75f);

                    // Soft specular bubble glint
                    float glintD = Mathf.Sqrt(Mathf.Pow(dx + 0.32f, 2f) + Mathf.Pow(dy - 0.32f, 2f));
                    float glint = Mathf.Pow(Mathf.Clamp01(1.0f - glintD / 0.5f), 2.2f) * 0.55f;

                    Color c = tint * (0.65f + (1f - dist) * 0.35f) + Color.white * glint;
                    c.a = edgeSoft * 0.85f;

                    pixels[y * size + x] = c;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, false);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            s_SpriteCache[key] = sprite;
            return sprite;
        }

        // =========================================================================
        // 🌟 CONCEPT 3: AAA ARTWORK LOADERS (PIXAR-GRADE VISUALS)
        // =========================================================================
        public static Sprite LoadOrFallbackSprite(string resourceName, System.Func<Sprite> fallbackGenerator = null)
        {
            if (s_SpriteCache.TryGetValue(resourceName, out Sprite cached) && cached != null && cached.texture != null)
                return cached;

            // 1. Try Resources.Load<Sprite>
            Sprite sp = Resources.Load<Sprite>(resourceName);
            if (sp != null)
            {
                s_SpriteCache[resourceName] = sp;
                return sp;
            }

            // 2. Try Resources.Load<Texture2D>
            Texture2D tex = Resources.Load<Texture2D>(resourceName);
            if (tex != null)
            {
                sp = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                s_SpriteCache[resourceName] = sp;
                return sp;
            }

            // 3. Try direct disk load from Application.dataPath/Resources/...
            string filePath = System.IO.Path.Combine(Application.dataPath, "Resources", resourceName + ".png");
            if (System.IO.File.Exists(filePath))
            {
                byte[] fileData = System.IO.File.ReadAllBytes(filePath);
                Texture2D diskTex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (diskTex.LoadImage(fileData))
                {
                    diskTex.filterMode = FilterMode.Bilinear;
                    sp = Sprite.Create(diskTex, new Rect(0, 0, diskTex.width, diskTex.height), new Vector2(0.5f, 0.5f), 100f);
                    s_SpriteCache[resourceName] = sp;
                    return sp;
                }
            }

            if (fallbackGenerator != null)
            {
                sp = fallbackGenerator();
                if (sp != null) s_SpriteCache[resourceName] = sp;
                return sp;
            }

            return null;
        }

        public static Sprite GetHomeBackgroundConcept3Sprite()
        {
            return LoadOrFallbackSprite("HomeBackground_Concept3", () => GetCosmicVignetteGradientSprite());
        }

        public static Sprite GetAdventure3DButtonSprite()
        {
            return LoadOrFallbackSprite("Button_Adventure_3D", () => GetPillSprite(new Color(0.98f, 0.72f, 0.12f)));
        }

        public static Sprite GetClassic3DButtonSprite()
        {
            return LoadOrFallbackSprite("Button_Classic_3D", () => GetPillSprite(new Color(0.12f, 0.88f, 0.78f)));
        }

        public static Sprite GetConcept3SphereSprite(int index)
        {
            string[] names = new string[]
            {
                "Sphere_Candy_PinkSwirl",
                "Sphere_Candy_OrangePlanet",
                "Sphere_Candy_LimeMint",
                "Sphere_Candy_CyanWave",
                "Sphere_Candy_PurpleBubble",
                "Sphere_Candy_RainbowStriped"
            };
            int safeIdx = Mathf.Clamp(index, 0, names.Length - 1);
            return LoadOrFallbackSprite(names[safeIdx], () => GetBlockSprite(BlockColors[safeIdx % BlockColors.Length]));
        }

        public static Sprite GetWardrobeSkinSprite(BallSkinType type)
        {
            string name = type switch
            {
                BallSkinType.Classic => "Wardrobe_Skin_CandySwirl",
                BallSkinType.StarCore => "Wardrobe_Skin_StarCore",
                BallSkinType.OrangePlanet => "Wardrobe_Skin_OrangePlanet",
                BallSkinType.LimeMint => "Wardrobe_Skin_LimeMint",
                BallSkinType.RainbowBlast => "Wardrobe_Skin_RainbowBlast",
                BallSkinType.GalaxySwirl => "Wardrobe_Skin_MysticOrb",
                BallSkinType.SunBurst => "Wardrobe_Skin_SolarFlare",
                BallSkinType.LightningVolt => "Wardrobe_Skin_LightningPlasma",
                BallSkinType.CyanWave => "Wardrobe_Skin_CyanWave",
                _ => "Wardrobe_Skin_CandySwirl"
            };
            return LoadOrFallbackSprite(name, () => GetBlockSprite(new Color(0.2f, 0.6f, 1f), type));
        }

        /// <summary>
        /// Retrieves the authentic 2D/3D sprite for an unlocked ball design calibrated to exactly
        /// 1.28 world units (matching classic 128px spheres at 100 PPU).
        /// </summary>
        public static Sprite GetSkinBlockSprite(BallSkinType type)
        {
            string cacheKey = $"SkinBlock_{type}";
            if (s_SpriteCache.TryGetValue(cacheKey, out Sprite cached) && cached != null && cached.texture != null)
                return cached;

            string resName = type switch
            {
                BallSkinType.Classic => "Wardrobe_Skin_CandySwirl",
                BallSkinType.StarCore => "Wardrobe_Skin_StarCore",
                BallSkinType.OrangePlanet => "Wardrobe_Skin_OrangePlanet",
                BallSkinType.LimeMint => "Wardrobe_Skin_LimeMint",
                BallSkinType.RainbowBlast => "Wardrobe_Skin_RainbowBlast",
                BallSkinType.GalaxySwirl => "Wardrobe_Skin_MysticOrb",
                BallSkinType.SunBurst => "Wardrobe_Skin_SolarFlare",
                BallSkinType.LightningVolt => "Wardrobe_Skin_LightningPlasma",
                BallSkinType.CyanWave => "Wardrobe_Skin_CyanWave",
                _ => "Wardrobe_Skin_CandySwirl"
            };

            // 1. Try Sprite loaded from Resources
            Sprite loadedSp = Resources.Load<Sprite>(resName);
            Texture2D tex = (loadedSp != null) ? loadedSp.texture : Resources.Load<Texture2D>(resName);

            // 2. Fallback disk load from Application.dataPath/Resources/...
            if (tex == null)
            {
                string filePath = System.IO.Path.Combine(Application.dataPath, "Resources", resName + ".png");
                if (System.IO.File.Exists(filePath))
                {
                    byte[] fileData = System.IO.File.ReadAllBytes(filePath);
                    tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    if (tex.LoadImage(fileData))
                    {
                        tex.filterMode = FilterMode.Bilinear;
                    }
                }
            }

            if (tex != null)
            {
                // 256x256 texture at 200 PPU = 1.28 world units (matching classic 128px at 100 PPU = 1.28)
                float ppu = (tex.width >= 256) ? 200f : 100f;
                Sprite sp = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), ppu);
                s_SpriteCache[cacheKey] = sp;
                return sp;
            }

            return GetWardrobeSkinSprite(type);
        }
    }
}
