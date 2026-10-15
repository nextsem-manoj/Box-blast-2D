using System;
using System.Collections.Generic;
using UnityEngine;

namespace BoxBlast
{
    [System.Serializable]
    public class AchievementData
    {
        public string Id;
        public string Title;
        public string Description;
        public int TargetValue;
        public int CurrentValue;
        public int CoinReward;
        public BoosterType BoosterReward;
        public bool IsUnlocked;
        public bool IsClaimed;
    }

    /// <summary>
    /// Master Achievements and Quest Badges System for Box Blast 2D.
    /// Tracks lifetime stats, evaluates progression milestones, triggers celebration toasts,
    /// and grants juicy coin and booster rewards.
    /// </summary>
    public class AchievementManager : MonoBehaviour
    {
        public static AchievementManager Instance { get; private set; }

        public event Action<AchievementData> OnAchievementUnlocked;
        public event Action<AchievementData> OnAchievementClaimed;

        private const string PrefsPrefix = "BoxBlast_Ach_";
        private const string PrefsUnlockedPrefix = "BoxBlast_Ach_Unlocked_";
        private const string PrefsClaimedPrefix = "BoxBlast_Ach_Claimed_";

        private readonly List<AchievementData> m_Achievements = new List<AchievementData>();
        public IReadOnlyList<AchievementData> AllAchievements => m_Achievements;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Instance = null;
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            InitializeAchievements();
            LoadProgression();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void InitializeAchievements()
        {
            m_Achievements.Clear();

            m_Achievements.Add(new AchievementData
            {
                Id = "first_line",
                Title = "First Line Blast",
                Description = "Clear your very first line on the board!",
                TargetValue = 1,
                CoinReward = 50,
                BoosterReward = BoosterType.None
            });

            m_Achievements.Add(new AchievementData
            {
                Id = "double_strike",
                Title = "Double Strike",
                Description = "Clear 2 or more lines in a single move!",
                TargetValue = 1,
                CoinReward = 100,
                BoosterReward = BoosterType.None
            });

            m_Achievements.Add(new AchievementData
            {
                Id = "combo_master",
                Title = "Combo Master",
                Description = "Achieve a 3x or higher combo streak!",
                TargetValue = 3,
                CoinReward = 150,
                BoosterReward = BoosterType.Shuffle
            });

            m_Achievements.Add(new AchievementData
            {
                Id = "line_crusher",
                Title = "Line Crusher",
                Description = "Clear 25 total lines across all game modes!",
                TargetValue = 25,
                CoinReward = 120,
                BoosterReward = BoosterType.None
            });

            m_Achievements.Add(new AchievementData
            {
                Id = "gem_hunter",
                Title = "Gem Hunter",
                Description = "Collect 20 Adventure gems from puzzle boards!",
                TargetValue = 20,
                CoinReward = 150,
                BoosterReward = BoosterType.Bomb
            });

            m_Achievements.Add(new AchievementData
            {
                Id = "classic_champion",
                Title = "Classic Champion",
                Description = "Reach 2,000 points in Classic Endless Mode!",
                TargetValue = 2000,
                CoinReward = 200,
                BoosterReward = BoosterType.Cannon
            });

            m_Achievements.Add(new AchievementData
            {
                Id = "adventure_explorer",
                Title = "Adventure Explorer",
                Description = "Complete Level 5 in Adventure Mode!",
                TargetValue = 5,
                CoinReward = 200,
                BoosterReward = BoosterType.None
            });

            m_Achievements.Add(new AchievementData
            {
                Id = "fashion_guru",
                Title = "Fashion Guru",
                Description = "Equip a custom ball design in the Wardrobe!",
                TargetValue = 1,
                CoinReward = 100,
                BoosterReward = BoosterType.None
            });

            m_Achievements.Add(new AchievementData
            {
                Id = "tactical_master",
                Title = "Tactical Master",
                Description = "Use any booster in gameplay!",
                TargetValue = 1,
                CoinReward = 80,
                BoosterReward = BoosterType.Arrow
            });
        }

        private void LoadProgression()
        {
            foreach (var ach in m_Achievements)
            {
                ach.CurrentValue = PlayerPrefs.GetInt(PrefsPrefix + ach.Id, 0);
                ach.IsUnlocked = PlayerPrefs.GetInt(PrefsUnlockedPrefix + ach.Id, 0) == 1;
                ach.IsClaimed = PlayerPrefs.GetInt(PrefsClaimedPrefix + ach.Id, 0) == 1;

                if (!ach.IsUnlocked && ach.CurrentValue >= ach.TargetValue)
                {
                    ach.IsUnlocked = true;
                    PlayerPrefs.SetInt(PrefsUnlockedPrefix + ach.Id, 1);
                }
            }
        }

        private void SaveAchievement(AchievementData ach)
        {
            PlayerPrefs.SetInt(PrefsPrefix + ach.Id, ach.CurrentValue);
            PlayerPrefs.SetInt(PrefsUnlockedPrefix + ach.Id, ach.IsUnlocked ? 1 : 0);
            PlayerPrefs.SetInt(PrefsClaimedPrefix + ach.Id, ach.IsClaimed ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void CheckUnlock(AchievementData ach)
        {
            if (ach.IsUnlocked) return;

            if (ach.CurrentValue >= ach.TargetValue)
            {
                ach.IsUnlocked = true;
                SaveAchievement(ach);

                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlayAchievement();
                }

                HapticManager.TriggerHeavy();
                OnAchievementUnlocked?.Invoke(ach);

                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowAchievementToast(ach);
                }
            }
            else
            {
                SaveAchievement(ach);
            }
        }

        // ==========================================
        // 📊 STAT REPORTING HOOKS
        // ==========================================

        public void ReportLinesCleared(int count)
        {
            if (count <= 0) return;

            // First line
            var firstLine = GetAchievement("first_line");
            if (firstLine != null && !firstLine.IsUnlocked)
            {
                firstLine.CurrentValue = 1;
                CheckUnlock(firstLine);
            }

            // Double strike
            if (count >= 2)
            {
                var dbl = GetAchievement("double_strike");
                if (dbl != null && !dbl.IsUnlocked)
                {
                    dbl.CurrentValue = 1;
                    CheckUnlock(dbl);
                }
            }

            // Total lines
            var totalLines = GetAchievement("line_crusher");
            if (totalLines != null && !totalLines.IsUnlocked)
            {
                totalLines.CurrentValue += count;
                CheckUnlock(totalLines);
            }
        }

        public void ReportCombo(int comboStreak)
        {
            var ach = GetAchievement("combo_master");
            if (ach != null && !ach.IsUnlocked)
            {
                if (comboStreak > ach.CurrentValue)
                {
                    ach.CurrentValue = comboStreak;
                    CheckUnlock(ach);
                }
            }
        }

        public void ReportGemsCollected(int count)
        {
            if (count <= 0) return;
            var ach = GetAchievement("gem_hunter");
            if (ach != null && !ach.IsUnlocked)
            {
                ach.CurrentValue += count;
                CheckUnlock(ach);
            }
        }

        public void ReportScore(int score, bool isClassic)
        {
            if (isClassic)
            {
                var ach = GetAchievement("classic_champion");
                if (ach != null && !ach.IsUnlocked)
                {
                    if (score > ach.CurrentValue)
                    {
                        ach.CurrentValue = score;
                        CheckUnlock(ach);
                    }
                }
            }
        }

        public void ReportLevelCompleted(GameModeType mode, int levelNum)
        {
            if (mode == GameModeType.Adventure)
            {
                var ach = GetAchievement("adventure_explorer");
                if (ach != null && !ach.IsUnlocked)
                {
                    if (levelNum > ach.CurrentValue)
                    {
                        ach.CurrentValue = levelNum;
                        CheckUnlock(ach);
                    }
                }
            }
        }

        public void ReportSkinEquipped(BallSkinType skin)
        {
            if (skin != BallSkinType.Classic)
            {
                var ach = GetAchievement("fashion_guru");
                if (ach != null && !ach.IsUnlocked)
                {
                    ach.CurrentValue = 1;
                    CheckUnlock(ach);
                }
            }
        }

        public void ReportBoosterUsed(BoosterType booster)
        {
            var ach = GetAchievement("tactical_master");
            if (ach != null && !ach.IsUnlocked)
            {
                ach.CurrentValue = 1;
                CheckUnlock(ach);
            }
        }

        public AchievementData GetAchievement(string id)
        {
            for (int i = 0; i < m_Achievements.Count; i++)
            {
                if (m_Achievements[i].Id == id) return m_Achievements[i];
            }
            return null;
        }

        public bool ClaimReward(string achievementId)
        {
            var ach = GetAchievement(achievementId);
            if (ach != null && ach.IsUnlocked && !ach.IsClaimed)
            {
                ach.IsClaimed = true;
                SaveAchievement(ach);

                if (GameManager.Instance != null)
                {
                    if (ach.CoinReward > 0)
                    {
                        GameManager.Instance.AddCoins(ach.CoinReward, true);
                    }
                    if (ach.BoosterReward != BoosterType.None)
                    {
                        GameManager.Instance.AddBooster(ach.BoosterReward, 1);
                    }
                }

                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlayPurchase();
                }

                HapticManager.TriggerLight();
                OnAchievementClaimed?.Invoke(ach);
                return true;
            }
            return false;
        }

        public int GetUnclaimedCount()
        {
            int count = 0;
            for (int i = 0; i < m_Achievements.Count; i++)
            {
                if (m_Achievements[i].IsUnlocked && !m_Achievements[i].IsClaimed)
                {
                    count++;
                }
            }
            return count;
        }
    }
}
