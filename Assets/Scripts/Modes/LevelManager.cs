using System.Collections.Generic;
using UnityEngine;

namespace BoxBlast
{
    public enum DefeatReason
    {
        OutOfMoves,
        BoardFull
    }

    /// <summary>
    /// Master controller for Adventure, Event, and Task modes.
    /// Manages level goals, moves limit, victory/defeat evaluation, and persistent progression.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        public GameModeType CurrentMode { get; private set; } = GameModeType.Adventure;
        public int CurrentLevelNumber { get; private set; } = 1;
        public LevelData CurrentLevel { get; private set; }

        public int MovesRemaining { get; private set; }
        public int LinesCleared { get; private set; }
        public int ColorsCollected { get; private set; }
        public Dictionary<GemType, int> CollectedGems { get; private set; } = new Dictionary<GemType, int>();
        public bool IsLevelActive { get; private set; }
        public bool IsLevelCompleted { get; private set; }

        public const int MaxExtraMovesPerLevel = 3;
        public int ExtraMovesUsed { get; private set; } = 0;
        public int ExtraMovesRemaining => Mathf.Max(0, MaxExtraMovesPerLevel - ExtraMovesUsed);
        public bool CanAddExtraMoves => ExtraMovesRemaining > 0;

        public const int MaxAdventureLevels = 40;
        public static int GetMaxLevel(GameModeType mode) => (mode == GameModeType.Adventure) ? MaxAdventureLevels : 20;

        private const string PrefsPrefix = "BoxBlast_Progression_";

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
                // One-time progression reset migration so player starts fresh from Level 1
                // with levels 2..40 properly locked (clearing legacy fake unlocked data)
                if (PlayerPrefs.GetInt("BoxBlast_Progression_ResetToLevel1_V3", 0) == 0)
                {
                    ResetAllProgress(GameModeType.Adventure);
                    UnlockLevel(GameModeType.Adventure, 1);
                    ResetAllProgress(GameModeType.Event);
                    UnlockLevel(GameModeType.Event, 1);
                    ResetAllProgress(GameModeType.Task);
                    UnlockLevel(GameModeType.Task, 1);
                    PlayerPrefs.SetInt("BoxBlast_Progression_ResetToLevel1_V3", 1);
                    PlayerPrefs.Save();
                }
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void StartLevel(GameModeType mode, int levelNum)
        {
            CurrentMode = mode;
            int maxLvl = GetMaxLevel(mode);
            CurrentLevelNumber = Mathf.Clamp(levelNum, 1, maxLvl);

            if (!IsLevelUnlocked(mode, CurrentLevelNumber))
            {
                Debug.LogWarning($"Level {CurrentLevelNumber} in {mode} is locked!");
                return;
            }

            // Stop any ongoing delayed defeat/victory evaluation coroutines from previous runs
            StopAllCoroutines();

            if (UIManager.Instance != null)
            {
                UIManager.Instance.HideDefeatModal();
                UIManager.Instance.HideVictoryModal();
                UIManager.Instance.HideGameOver();
            }

            CurrentLevel = LevelDatabase.GetLevel(mode, CurrentLevelNumber);

            MovesRemaining = CurrentLevel.MovesLimit;
            LinesCleared = 0;
            ColorsCollected = 0;
            CollectedGems.Clear();
            ExtraMovesUsed = 0;
            // CRITICAL: Level is inactive during setup so cleanup/resets cannot trigger gameplay events
            IsLevelActive = false;
            IsLevelCompleted = false;

            DraggableShape.ResetActiveDrag();

            // 1. Activate level FIRST so managers know Adventure layout is active
            IsLevelActive = true;

            // 2. Switch Grid and Spawner to Adventure mode layout
            bool isLandscape = Screen.width > Screen.height;
            if (GridManager.Instance != null)
            {
                GridManager.Instance.UpdateLayout(isLandscape);
                GridManager.Instance.ResetGrid();
                GridManager.Instance.LoadPattern(CurrentLevel.InitialPattern, CurrentLevel.InitialGems);
            }
            if (ShapeSpawner.Instance != null)
            {
                ShapeSpawner.Instance.UpdateLayout(isLandscape);
                ShapeSpawner.Instance.SpawnBatch();
            }

            // 4. Reset Score in GameManager & mark Classic inactive
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnLevelStarted();
            }

            // 5. Update Level HUD & ensure Adventure level HUD is fully visible
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowLevelHUD(CurrentLevel, MovesRemaining, GetGoalProgressText());
                UIManager.Instance.UpdateGemObjectives(CollectedGems, CurrentLevel.TargetGems);
                UIManager.Instance.UpdateHUDVisibility();

                // 6. First-Time Tutorial Hand Guide on Adventure Level 1
                if (CurrentMode == GameModeType.Adventure && CurrentLevelNumber == 1 && PlayerPrefs.GetInt("BoxBlast_Tutorial_Completed", 0) == 0)
                {
                    UIManager.Instance.ShowTutorialGuide();
                }
            }
        }

        public void StopLevel()
        {
            StopAllCoroutines();
            IsLevelActive = false;
            IsLevelCompleted = false;
            DraggableShape.ResetActiveDrag();
            if (UIManager.Instance != null)
            {
                UIManager.Instance.HideDefeatModal();
                UIManager.Instance.HideVictoryModal();
                UIManager.Instance.HideLevelHUD();
                UIManager.Instance.HideTutorialGuide(false);
            }
            if (GridManager.Instance != null)
            {
                GridManager.Instance.ResetGrid();
            }
            if (ShapeSpawner.Instance != null)
            {
                ShapeSpawner.Instance.ClearAll();
            }
        }

        public void OnMoveMade()
        {
            if (!IsLevelActive || IsLevelCompleted) return;

            if (CurrentLevel.MovesLimit > 0)
            {
                MovesRemaining--;
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.UpdateMoves(MovesRemaining);

                    // Low Moves Heartbeat Tension Alert when moves are running low (<= 5)
                    if (MovesRemaining <= 5 && MovesRemaining > 0)
                    {
                        UIManager.Instance.TriggerLowMovesWarning(MovesRemaining);
                    }
                }

                // Check victory after move
                if (CheckGoalReached())
                {
                    CompleteLevel();
                    return;
                }

                // Out of moves check: Give any active line clear animations & flying gems time to land before evaluating defeat!
                if (MovesRemaining <= 0)
                {
                    StartCoroutine(EvaluateDelayedDefeat());
                    return;
                }
            }
        }

        private System.Collections.IEnumerator EvaluateDelayedDefeat()
        {
            // Grace period: allow line blast wave (up to 0.40s) and flying gems to reach HUD
            yield return new WaitForSeconds(0.60f);

            if (!IsLevelActive || IsLevelCompleted) yield break;

            if (CheckGoalReached())
            {
                CompleteLevel();
            }
            else if (MovesRemaining <= 0)
            {
                FailLevel(DefeatReason.OutOfMoves);
            }
        }

        public void OnLinesCleared(int linesCount, List<Color> clearedBallColors)
        {
            if (!IsLevelActive || IsLevelCompleted) return;

            LinesCleared += linesCount;

            Color targetCol = SpriteFactory.BlockColors[CurrentLevel.TargetColorIndex];
            for (int i = 0; i < clearedBallColors.Count; i++)
            {
                if (IsMatchingColor(clearedBallColors[i], targetCol))
                {
                    ColorsCollected++;
                }
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateGoalProgress(GetGoalProgressText());
            }

            if (CheckGoalReached())
            {
                CompleteLevel();
            }
        }

        public void OnGemCollected(GemType gem, Vector3 worldPos)
        {
            if (!IsLevelActive || IsLevelCompleted || gem == GemType.None) return;

            if (!CollectedGems.ContainsKey(gem)) CollectedGems[gem] = 0;
            CollectedGems[gem]++;

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayCoin();
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.FlyGemToHUD(gem, worldPos);
                UIManager.Instance.UpdateGemObjectives(CollectedGems, CurrentLevel?.TargetGems);
                UIManager.Instance.UpdateGoalProgress(GetGoalProgressText());
            }

            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.ReportGemsCollected(1);
            }

            if (CheckGoalReached())
            {
                CompleteLevel();
            }
        }

        private bool CheckGoalReached()
        {
            switch (CurrentLevel.GoalType)
            {
                case LevelGoalType.ScoreTarget:
                    return GameManager.Instance != null && GameManager.Instance.CurrentScore >= CurrentLevel.TargetScore;

                case LevelGoalType.ClearLines:
                    return LinesCleared >= CurrentLevel.TargetLines;

                case LevelGoalType.CollectColors:
                    return ColorsCollected >= CurrentLevel.TargetColorCount;

                case LevelGoalType.ClearPattern:
                    return GridManager.Instance != null && GridManager.Instance.GetOccupiedCount() <= 3;

                case LevelGoalType.CollectGems:
                    if (CurrentLevel.TargetGems == null || CurrentLevel.TargetGems.Count == 0) return true;
                    foreach (var kvp in CurrentLevel.TargetGems)
                    {
                        int have = CollectedGems.ContainsKey(kvp.Key) ? CollectedGems[kvp.Key] : 0;
                        if (have < kvp.Value) return false;
                    }
                    return true;
            }
            return false;
        }

        public string GetGoalProgressText()
        {
            if (CurrentLevel == null) return "";

            switch (CurrentLevel.GoalType)
            {
                case LevelGoalType.ScoreTarget:
                    int curScore = (GameManager.Instance != null) ? GameManager.Instance.CurrentScore : 0;
                    return $"Score: {curScore}/{CurrentLevel.TargetScore}";

                case LevelGoalType.ClearLines:
                    return $"Lines: {LinesCleared}/{CurrentLevel.TargetLines}";

                case LevelGoalType.CollectColors:
                    return $"Gems: {ColorsCollected}/{CurrentLevel.TargetColorCount}";

                case LevelGoalType.ClearPattern:
                    int remaining = (GridManager.Instance != null) ? GridManager.Instance.GetOccupiedCount() : 0;
                    return $"Clear Pattern: {remaining} left";

                case LevelGoalType.CollectGems:
                    if (CurrentLevel.TargetGems == null || CurrentLevel.TargetGems.Count == 0) return "COLLECT GEMS";
                    int totalTarget = 0;
                    int totalHave = 0;
                    foreach (var kvp in CurrentLevel.TargetGems)
                    {
                        totalTarget += kvp.Value;
                        int have = CollectedGems.ContainsKey(kvp.Key) ? CollectedGems[kvp.Key] : 0;
                        totalHave += Mathf.Min(have, kvp.Value);
                    }
                    return $"GEMS: {totalHave}/{totalTarget}";
            }
            return "";
        }

        private void CompleteLevel()
        {
            if (IsLevelCompleted) return;
            IsLevelCompleted = true;
            IsLevelActive = false;

            // Calculate Stars (1 to 3 based on remaining moves efficiency)
            int stars = 1;
            float ratio = (CurrentLevel != null && CurrentLevel.MovesLimit > 0) ? (float)MovesRemaining / CurrentLevel.MovesLimit : 0.5f;
            if (ratio >= 0.30f || MovesRemaining >= 5) stars = 3;
            else if (ratio >= 0.12f || MovesRemaining >= 2) stars = 2;

            SaveProgress(CurrentMode, CurrentLevelNumber, stars);
            UnlockLevel(CurrentMode, CurrentLevelNumber + 1);

            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.ReportLevelCompleted(CurrentMode, CurrentLevelNumber);
            }

            SkinInfo unlockedSkin = null;
            if (CurrentMode == GameModeType.Adventure)
            {
                SkinManager.CheckAndUnlockLevelSkin(CurrentLevelNumber, out unlockedSkin);
            }

            HapticManager.TriggerHeavy();

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayCombo();
            }

            if (UIManager.Instance != null)
            {
                int maxLvl = GetMaxLevel(CurrentMode);
                UIManager.Instance.ShowLevelCompleteModal(CurrentMode, CurrentLevelNumber, stars, CurrentLevelNumber < maxLvl, unlockedSkin);
            }
        }

        public void FailLevel(DefeatReason reason = DefeatReason.OutOfMoves)
        {
            if (IsLevelCompleted) return;
            IsLevelActive = false;

            HapticManager.TriggerMedium();

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayGameOver();
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowLevelFailedModal(CurrentMode, CurrentLevelNumber, reason);
            }
        }

        public bool AddExtraMoves(int extraMoves = 5)
        {
            if (CanAddExtraMoves)
            {
                DraggableShape.ResetActiveDrag();
                ExtraMovesUsed++;
                MovesRemaining += extraMoves;
                IsLevelActive = true;
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.HideDefeatModal();
                    UIManager.Instance.UpdateMoves(MovesRemaining);
                }

                // If board is full / crowded, clear center 5x5 breathing room!
                if (GridManager.Instance != null && GridManager.Instance.GetOccupiedCount() >= 32)
                {
                    GridManager.Instance.ReviveClear();
                }

                // Immediately refresh all 3 boxes with a fresh, playable batch of shapes!
                if (ShapeSpawner.Instance != null)
                {
                    ShapeSpawner.Instance.SpawnBatch();
                }

                FloatingText.Create(Vector3.zero, $"+{extraMoves} MOVES!", new Color(0.2f, 0.95f, 0.4f), 7.5f);
                return true;
            }
            FloatingText.Create(Vector3.zero, "No Extra Moves Left!", Color.yellow, 6f);
            return false;
        }

        public void NextLevel()
        {
            int maxLvl = GetMaxLevel(CurrentMode);
            if (CurrentLevelNumber < maxLvl)
            {
                StartLevel(CurrentMode, CurrentLevelNumber + 1);
            }
            else
            {
                // Reached end of mode levels
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.OpenLevelSelect(CurrentMode);
                }
            }
        }

        public void RestartCurrentLevel()
        {
            StartLevel(CurrentMode, CurrentLevelNumber);
        }

        #region PlayerPrefs Persistence
        public static bool IsLevelUnlocked(GameModeType mode, int level)
        {
            if (level <= 1) return true;
            // Level is unlocked when the preceding level has been completed (has at least 1 star)
            // or when explicitly unlocked
            if (GetLevelStars(mode, level - 1) > 0) return true;
            string key = $"{PrefsPrefix}{mode}_{level}_unlocked";
            return PlayerPrefs.GetInt(key, 0) == 1;
        }

        public static int GetHighestUnlockedLevel(GameModeType mode)
        {
            int maxLvl = GetMaxLevel(mode);
            for (int i = maxLvl; i >= 1; i--)
            {
                if (IsLevelUnlocked(mode, i)) return i;
            }
            return 1;
        }

        public static void ResetAllProgress(GameModeType mode)
        {
            int maxLvl = GetMaxLevel(mode);
            for (int i = 1; i <= maxLvl; i++)
            {
                PlayerPrefs.DeleteKey($"{PrefsPrefix}{mode}_{i}_unlocked");
                PlayerPrefs.DeleteKey($"{PrefsPrefix}{mode}_{i}_stars");
            }
            PlayerPrefs.Save();
        }

        public static void UnlockLevel(GameModeType mode, int level)
        {
            int maxLvl = GetMaxLevel(mode);
            if (level > maxLvl) return;
            string key = $"{PrefsPrefix}{mode}_{level}_unlocked";
            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.Save();
        }

        public static int GetLevelStars(GameModeType mode, int level)
        {
            string key = $"{PrefsPrefix}{mode}_{level}_stars";
            return PlayerPrefs.GetInt(key, 0);
        }

        public static void SaveProgress(GameModeType mode, int level, int stars)
        {
            string key = $"{PrefsPrefix}{mode}_{level}_stars";
            int previousStars = PlayerPrefs.GetInt(key, 0);
            if (stars > previousStars)
            {
                PlayerPrefs.SetInt(key, stars);
                PlayerPrefs.Save();
            }
        }
        #endregion

        private static bool IsMatchingColor(Color a, Color b)
        {
            return Mathf.Abs(a.r - b.r) < 0.15f && Mathf.Abs(a.g - b.g) < 0.15f && Mathf.Abs(a.b - b.b) < 0.15f;
        }
    }
}
