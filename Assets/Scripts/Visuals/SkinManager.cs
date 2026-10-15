using System;
using System.Collections.Generic;
using UnityEngine;

namespace BoxBlast
{
    public enum BallSkinType
    {
        Classic = 0,        // Pink Candy Swirl (Default)
        StarCore = 1,       // Glowing Golden Star Core (Level 2)
        OrangePlanet = 2,   // Striped Citrus Cosmic Planet (Level 4)
        LimeMint = 3,       // Emerald Mint Swirl Striped Orb (Level 6)
        RainbowBlast = 4,   // Iridescent Rainbow Striped Orb (Level 8)
        GalaxySwirl = 5,    // Mystic Cosmic Spiral Nebula Orb (Level 10)
        SunBurst = 6,       // Fiery Solar Corona Plasma Orb (Level 12)
        LightningVolt = 7,  // High-Voltage Lightning Plasma Orb (Level 14)
        CyanWave = 8        // Ocean Cyan Wave Swirl Orb (Level 16)
    }

    [System.Serializable]
    public class SkinInfo
    {
        public BallSkinType Type;
        public string Name;
        public string Description;
        public int UnlockAdventureLevel; // 0 = default, 1 = lvl 1, 2 = lvl 2...
        public string Emoji;
    }

    /// <summary>
    /// Manages ball skin progression, unlocking, equipping, and live board updates.
    /// Adventure mode awards a new unique ball skin upon level completion!
    /// </summary>
    public static class SkinManager
    {
        private const string PREF_EQUIPPED_SKIN = "EquippedBallSkin";
        private const string PREF_SKIN_UNLOCKED_PREFIX = "SkinUnlocked_";

        public static event Action<BallSkinType> OnSkinChanged;

        private static readonly List<SkinInfo> s_Skins = new List<SkinInfo>
        {
            new SkinInfo
            {
                Type = BallSkinType.Classic,
                Name = "Classic Spheres",
                Description = "Vibrant multi-colored 3D glossy spheres (Red, Blue, Yellow, Green, Cyan, Purple).",
                UnlockAdventureLevel = 0,
                Emoji = "🔮"
            },
            new SkinInfo
            {
                Type = BallSkinType.StarCore,
                Name = "Star Core",
                Description = "A radiant golden star blazing inside the glass sphere.",
                UnlockAdventureLevel = 5,
                Emoji = ""
            },
            new SkinInfo
            {
                Type = BallSkinType.OrangePlanet,
                Name = "Orange Planet",
                Description = "Striped citrus planetary orb with warm golden glow.",
                UnlockAdventureLevel = 10,
                Emoji = ""
            },
            new SkinInfo
            {
                Type = BallSkinType.LimeMint,
                Name = "Lime Mint",
                Description = "Refreshing emerald mint candy swirl sphere.",
                UnlockAdventureLevel = 15,
                Emoji = ""
            },
            new SkinInfo
            {
                Type = BallSkinType.RainbowBlast,
                Name = "Rainbow Blast",
                Description = "Juicy vibrant rainbow spectrum glossy sphere.",
                UnlockAdventureLevel = 20,
                Emoji = ""
            },
            new SkinInfo
            {
                Type = BallSkinType.GalaxySwirl,
                Name = "Mystic Orb",
                Description = "A swirling cosmic spiral nebula with shimmering stardust.",
                UnlockAdventureLevel = 25,
                Emoji = ""
            },
            new SkinInfo
            {
                Type = BallSkinType.SunBurst,
                Name = "Solar Flare",
                Description = "Blazing solar flares radiating from a luminous core.",
                UnlockAdventureLevel = 30,
                Emoji = ""
            },
            new SkinInfo
            {
                Type = BallSkinType.LightningVolt,
                Name = "Lightning Plasma",
                Description = "Crackling electric plasma thunderbolts discharging pure energy.",
                UnlockAdventureLevel = 35,
                Emoji = ""
            },
            new SkinInfo
            {
                Type = BallSkinType.CyanWave,
                Name = "Cyan Wave",
                Description = "Swirling tidal ocean azure wave sphere.",
                UnlockAdventureLevel = 40,
                Emoji = ""
            }
        };

        public static List<SkinInfo> GetAllSkins() => s_Skins;

        public static SkinInfo GetSkinInfo(BallSkinType type)
        {
            for (int i = 0; i < s_Skins.Count; i++)
            {
                if (s_Skins[i].Type == type) return s_Skins[i];
            }
            return s_Skins[0];
        }

        public static SkinInfo GetSkinForLevel(int adventureLevel)
        {
            for (int i = 0; i < s_Skins.Count; i++)
            {
                if (s_Skins[i].UnlockAdventureLevel == adventureLevel)
                {
                    return s_Skins[i];
                }
            }
            return null;
        }

        public static BallSkinType CurrentSkin
        {
            get
            {
                string saved = PlayerPrefs.GetString(PREF_EQUIPPED_SKIN, BallSkinType.Classic.ToString());
                if (Enum.TryParse(saved, out BallSkinType skin) && IsSkinUnlocked(skin))
                {
                    return skin;
                }
                return BallSkinType.Classic;
            }
            set
            {
                PlayerPrefs.SetString(PREF_EQUIPPED_SKIN, value.ToString());
                PlayerPrefs.Save();
            }
        }

        public static void ResetToDefaultClassic()
        {
            CurrentSkin = BallSkinType.Classic;
            SpriteFactory.ClearSpriteCache();
            OnSkinChanged?.Invoke(BallSkinType.Classic);
        }

        public static bool IsSkinUnlocked(BallSkinType type)
        {
            if (type == BallSkinType.Classic) return true;
            if (PlayerPrefs.GetInt(PREF_SKIN_UNLOCKED_PREFIX + type, 0) == 1) return true;
            SkinInfo info = GetSkinInfo(type);
            if (info != null && info.UnlockAdventureLevel > 0)
            {
                return LevelManager.GetLevelStars(GameModeType.Adventure, info.UnlockAdventureLevel) > 0;
            }
            return false;
        }

        public static void UnlockSkin(BallSkinType type)
        {
            PlayerPrefs.SetInt(PREF_SKIN_UNLOCKED_PREFIX + type, 1);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Checks if completing the given Adventure level awards a new ball skin.
        /// Unlocks the skin and returns true with skin info if newly unlocked.
        /// </summary>
        public static bool CheckAndUnlockLevelSkin(int adventureLevel, out SkinInfo newlyUnlocked)
        {
            newlyUnlocked = null;
            for (int i = 0; i < s_Skins.Count; i++)
            {
                SkinInfo skin = s_Skins[i];
                if (skin.UnlockAdventureLevel == adventureLevel)
                {
                    bool wasAlreadyNotified = PlayerPrefs.GetInt(PREF_SKIN_UNLOCKED_PREFIX + skin.Type, 0) == 1;
                    if (!wasAlreadyNotified)
                    {
                        UnlockSkin(skin.Type);
                        newlyUnlocked = skin;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Equips a ball skin and immediately updates all active blocks on the grid and in the spawner dock.
        /// </summary>
        public static void EquipSkin(BallSkinType type)
        {
            if (!IsSkinUnlocked(type)) return;

            CurrentSkin = type;
            SpriteFactory.ClearSpriteCache();
            OnSkinChanged?.Invoke(type);

            // Live update all blocks currently on screen
            if (GridManager.Instance != null)
            {
                GridManager.Instance.RefreshAllCellSprites();
            }

            if (ShapeSpawner.Instance != null)
            {
                ShapeSpawner.Instance.RefreshActiveShapeSprites();
            }

            HapticManager.TriggerLight();
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayPlace();
            }

            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.ReportSkinEquipped(type);
            }
        }
    }
}
