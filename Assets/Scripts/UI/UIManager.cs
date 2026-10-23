using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace BoxBlast
{
    /// <summary>
    /// Master UI manager controlling Responsive Landscape & Portrait Layouts,
    /// Coin Wallet, Crown Score Card, Far-Right Boosters Sidebar, Booster Shop Modal,
    /// Settings Modal, Level HUDs, and Modals.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        private Font m_DefaultFont;
        private Font m_CandyFont;
        private Font m_HudCleanFont;
        private Transform m_RootCanvas;
        private GraphicRaycaster m_Raycaster;

        // HUD Panels
        private GameObject m_BestScoreObj;
        private GameObject m_ScoreCardObj;
        private GameObject m_CoinsWalletObj;
        private GameObject m_SettingsBtnObj;
        private GameObject m_HomeBtnObj;
        private GameObject m_HeaderBallSkinBtnObj;
        private Image m_HeaderBallSkinImg;
        private GameObject m_BoosterSidebarObj;
        private GameObject m_ClassicHUD;
        private GameObject m_LevelHUD;
        private CanvasScaler m_CanvasScaler;

        // Score & Coin Texts
        private Text m_ScoreText;
        private Text m_BestScoreText;
        private Text m_CoinsText;
        private Text m_ComboText;

        // Game Over Texts & Buttons
        private Text m_GameOverScoreText;
        private Text m_GameOverBestText;
        private GameObject m_GameOverNewBestObj;
        private GameObject m_GameOverReviveBtn;
        private Text m_GameOverReviveText;
        private GameObject m_GameOverRestartBtn;
        private GameObject m_GameOverHomeBtn;

        // Level Defeat Modal Elements
        private GameObject m_DefeatExtraMovesBtn;
        private Text m_DefeatExtraMovesText;
        private Text m_DefeatTitleText;
        private Text m_DefeatSubtitleText;
        private GameObject m_DefeatRetryBtn;
        private GameObject m_DefeatLvlSelectBtn;
        private GameObject m_DefeatModeBtn;

        // Level In-Game HUD Texts
        private Text m_LevelTitleText;
        private Text m_LevelGoalText;
        private Text m_LevelMovesText;
        private Text m_LevelScoreText;
        private RectTransform m_LevelMovesBoxRt;

        // Booster UI Elements
        private readonly Dictionary<BoosterType, (GameObject buttonObj, Text badgeText, Image highlightImg)> m_BoosterButtons =
            new Dictionary<BoosterType, (GameObject, Text, Image)>();

        // Modals & Popups
        private GameObject m_TitleSplashScreen;
        private GameObject m_BoosterShopModal;
        private GameObject m_SettingsModal;
        private GameObject m_ModeSelectPanel;
        private GameObject m_HomeWardrobeBtn;
        private GameObject m_HomeSettingsBtn;
        private GameObject m_LevelSelectPanel;
        private GameObject m_GameOverPanel;
        private GameObject m_VictoryModal;
        private Text m_VictoryTitleText;
        private Text m_VictorySubtitleText;
        private GameObject m_VictoryNextBtnObj;
        private GameObject m_VictoryNextDisabledOverlay;
        private Image[] m_VictoryStarImgs = new Image[3];
        private GameObject m_DefeatModal;

        // Ball Skin Wardrobe & Victory Reward
        private GameObject m_SkinWardrobeModal;
        private Transform m_SkinGridContainer;
        private GameObject m_VictorySkinRewardObj;
        private Image m_VictorySkinPreviewImg;
        private Text m_VictorySkinNameText;
        private Button m_VictorySkinEquipBtn;
        private Text m_VictorySkinEquipBtnText;

        // Settings Dynamic Toggles
        private Image m_SfxBtnImg;
        private Text m_SfxBtnText;
        private Image m_MusicBtnImg;
        private Text m_MusicBtnText;
        private Image m_HapticBtnImg;
        private Text m_HapticBtnText;
        private Image m_BlastFxBtnImg;
        private Text m_BlastFxBtnText;
        private float m_LastToggleTime = 0f;

        // 3D Settings Dynamic Toggle Elements (Concept 2)
        private Image m_SfxToggleTrackImg;
        private RectTransform m_SfxToggleKnobRt;
        private Text m_SfxToggleStatusText;

        private Image m_MusicToggleTrackImg;
        private RectTransform m_MusicToggleKnobRt;
        private Text m_MusicToggleStatusText;

        private Image m_HapticToggleTrackImg;
        private RectTransform m_HapticToggleKnobRt;
        private Text m_HapticToggleStatusText;

        private Image m_BlastFxToggleTrackImg;
        private RectTransform m_BlastFxToggleKnobRt;
        private Text m_BlastFxToggleStatusText;

        // Level & Mode Select Dynamic Elements
        private Transform m_LevelGridContainer;
        private ScrollRect m_LevelScrollRect;
        private GalaxyRoadGraphic m_GalaxyRoad;
        private Text m_LevelSelectTitleText;
        private Text m_LevelSelectStarsText;
        private RectTransform m_LevelSelectStarsRt;
        private RectTransform m_LevelSelectWardrobeRt;
        private RectTransform m_LevelSelectTrailTitleRt;
        private Image m_LevelSelectStarsPillImg;
        private Text m_AdventureFooterText;
        private Text m_ModeCardAdvStarsText;
        private Text m_ModeCardEventStarsText;
        private Text m_ModeCardTaskStarsText;
        private Text m_ModeCardClassicBestText;
        private Text m_HomeClassicBadgeText;
        private Text m_HomeAdventureBadgeText;
        private Image m_HomeEquippedBallIcon;
        private Text m_HomeEquippedBallNameText;
        private GameObject m_HomeCenterBallShowcase;
        private Image m_HomeCenterBallImage;
        private RectTransform m_HomeBallDesignsPillRt;
        private RectTransform m_HomeCenterBallRt;
        private RectTransform m_HomeBallDesignsPromptRt;
        private Text m_DailyStreakText;

        // Adventure Gem Objectives in Level HUD
        private GameObject m_GemObjectivesBar;
        private GameObject m_LevelGoalBadgeObj;
        private readonly Dictionary<GemType, (GameObject rootObj, Image iconImg, Text countText, RectTransform iconRt, Image checkImg)> m_GemObjectiveItems =
            new Dictionary<GemType, (GameObject, Image, Text, RectTransform, Image)>();

        // Level Select Page Navigation (Legacy fields preserved)
#pragma warning disable 0414
        private int m_LevelSelectPage = 0;
        private GameObject m_LevelPageNavObj;
        private Text m_LevelPageNavText;
        private Text m_TopChapterBtnText;
        private RectTransform m_PrevChapterSideRt;
        private Text m_PrevChapterSideText;
        private RectTransform m_NextChapterSideRt;
        private Text m_NextChapterSideText;
#pragma warning restore 0414

        // Shop Coin Balance Text
        private Text m_ShopCoinsText;

        // Dynamic Juice: Low Moves Alert
        private Coroutine m_LowMovesCoroutine;

        // Dynamic Juice: First-Time Tutorial Guide
        private GameObject m_TutorialOverlayObj;
        private RectTransform m_TutorialHandRt;
        private Coroutine m_TutorialAnimCoroutine;
        private bool m_IsTutorialActive = false;

        // Dynamic Juice: Combo & Multi-line Callouts
        private GameObject m_ComboCalloutObj;
        private RectTransform m_ComboCalloutRt;
        private Text m_ComboCalloutText;
        private Image m_ComboCalloutBg;
        private Coroutine m_ComboCalloutCoroutine;

        // Meta Progression: Achievements & Quests
        private GameObject m_AchievementToastObj;
        private RectTransform m_AchievementToastRt;
        private Text m_AchievementToastTitle;
        private Text m_AchievementToastDesc;
        private Coroutine m_AchievementToastCoroutine;
        private GameObject m_AchievementsModal;
        private Transform m_AchievementsListContainer;
        private Text m_AchievementsUnclaimedBadgeText;
        private GameObject m_HomeAchievementsBtn;

        // Layout Tracking
        private bool m_IsLandscape = true;
        private int m_LastScreenWidth;
        private int m_LastScreenHeight;
        private GameModeType m_ActiveViewingMode = GameModeType.Adventure;
        private bool m_IsLevelSelectTransitioning = false;
        private Coroutine m_ToastCoroutine;
        private GameObject m_ToastObj;

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

            m_CandyFont = Resources.Load<Font>("LilitaOne");
            if (m_CandyFont == null) m_CandyFont = Font.CreateDynamicFontFromOSFont("Comic Sans MS", 36);
            if (m_CandyFont == null) m_CandyFont = Font.CreateDynamicFontFromOSFont("Arial", 36);
            if (m_CandyFont == null) m_CandyFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (m_CandyFont == null) m_CandyFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

            m_DefaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (m_DefaultFont == null) m_DefaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (m_DefaultFont == null) m_DefaultFont = Font.CreateDynamicFontFromOSFont("Arial", 24);

            m_HudCleanFont = Font.CreateDynamicFontFromOSFont("Segoe UI", 48);
            if (m_HudCleanFont == null) m_HudCleanFont = Font.CreateDynamicFontFromOSFont("Arial", 48);
            if (m_HudCleanFont == null) m_HudCleanFont = (m_CandyFont != null) ? m_CandyFont : m_DefaultFont;

            m_IsLandscape = Screen.width > Screen.height;
            m_LastScreenWidth = Screen.width;
            m_LastScreenHeight = Screen.height;

            SpriteFactory.PrewarmAll();

            BuildFullUI();
        }

        private void Start()
        {
            if (m_TitleSplashScreen != null) m_TitleSplashScreen.SetActive(false);
            if (m_LevelSelectPanel != null) m_LevelSelectPanel.SetActive(false);
            if (m_LevelHUD != null) m_LevelHUD.SetActive(false);
            if (m_VictoryModal != null) m_VictoryModal.SetActive(false);
            if (m_DefeatModal != null) m_DefeatModal.SetActive(false);

            // Open Home Page immediately on game start per user request
            OpenModeSelect();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            SkinManager.OnSkinChanged -= HandleSkinChangedForHome;
            SkinManager.OnSkinChanged -= HandleSkinChangedForHeader;
        }

        private void Update()
        {
            // Auto-detect screen rotation or resize
            if (Screen.width != m_LastScreenWidth || Screen.height != m_LastScreenHeight)
            {
                m_LastScreenWidth = Screen.width;
                m_LastScreenHeight = Screen.height;
                bool landscape = Screen.width > Screen.height;
                if (landscape != m_IsLandscape)
                {
                    ApplyOrientationLayout(landscape);
                }
                if (m_LevelSelectPanel != null && m_LevelSelectPanel.activeSelf)
                {
                    RefreshLevelSelectGrid(m_ActiveViewingMode);
                }
            }

            // Android Hardware / Gesture Back Button handling
            if (InputHelper.IsBackButtonPressed())
            {
                HandleBackNavigation();
            }
        }

        public void HandleBackNavigation()
        {
            if (m_BoosterShopModal != null && m_BoosterShopModal.activeSelf)
            {
                m_BoosterShopModal.SetActive(false);
                return;
            }
            if (m_SkinWardrobeModal != null && m_SkinWardrobeModal.activeSelf)
            {
                m_SkinWardrobeModal.SetActive(false);
                return;
            }
            if (m_SettingsModal != null && m_SettingsModal.activeSelf)
            {
                m_SettingsModal.SetActive(false);
                return;
            }
            if (m_LevelSelectPanel != null && m_LevelSelectPanel.activeSelf)
            {
                m_LevelSelectPanel.SetActive(false);
                OpenModeSelect();
                return;
            }
            if (m_ModeSelectPanel != null && m_ModeSelectPanel.activeSelf)
            {
                // Already on Home Screen - quit on mobile back
                Application.Quit();
                return;
            }
            if (m_GameOverPanel != null && m_GameOverPanel.activeSelf)
            {
                m_GameOverPanel.SetActive(false);
                OpenModeSelect();
                return;
            }
            if (m_VictoryModal != null && m_VictoryModal.activeSelf)
            {
                m_VictoryModal.SetActive(false);
                OpenModeSelect();
                return;
            }
            if (m_DefeatModal != null && m_DefeatModal.activeSelf)
            {
                m_DefeatModal.SetActive(false);
                OpenModeSelect();
                return;
            }
            if (m_TitleSplashScreen != null && m_TitleSplashScreen.activeSelf)
            {
                Application.Quit();
                return;
            }

            // In gameplay: open settings as pause menu
            OpenSettings();
        }

        // ==========================================
        // ðŸ”„ ORIENTATION & RESPONSIVE LAYOUT
        // ==========================================
        public void ApplyOrientationLayout(bool isLandscape)
        {
            m_IsLandscape = isLandscape;

            if (m_CanvasScaler != null)
            {
                if (isLandscape)
                {
                    m_CanvasScaler.referenceResolution = new Vector2(1920, 1080);
                    m_CanvasScaler.matchWidthOrHeight = 0.5f;
                }
                else
                {
                    m_CanvasScaler.referenceResolution = new Vector2(1080, 1920);
                    m_CanvasScaler.matchWidthOrHeight = 0f;
                }
                m_CanvasScaler.dynamicPixelsPerUnit = 4.0f;
            }

            if (GridManager.Instance != null) GridManager.Instance.UpdateLayout(isLandscape);
            if (ShapeSpawner.Instance != null) ShapeSpawner.Instance.UpdateLayout(isLandscape);

            // Safe Area calculations for mobile notches, dynamic islands, punch holes
            float safeTop = 0f;
            float safeLeft = 0f;
            float safeRight = 0f;
            if (Screen.height > 0 && Screen.width > 0)
            {
                Rect safeArea = Screen.safeArea;
                float topPixels = Screen.height - (safeArea.y + safeArea.height);
                float leftPixels = safeArea.x;
                float rightPixels = Screen.width - (safeArea.x + safeArea.width);

                float refHeight = isLandscape ? 1080f : 1920f;
                float refWidth = isLandscape ? 1920f : 1080f;

                safeTop = (topPixels / Screen.height) * refHeight;
                safeLeft = (leftPixels / Screen.width) * refWidth;
                safeRight = (rightPixels / Screen.width) * refWidth;

                if (Application.isMobilePlatform && !isLandscape)
                {
                    safeTop = Mathf.Max(safeTop, 45f);
                }
            }

            // 0. Home Button: Top-Left Cyan Crystal Gem Button (Option 2 Design B)
            float topBtnY = isLandscape ? -(20 + safeTop) : -(36 + safeTop);

            // 0. Top-Left: Home Button (Option 2 Design B)
            if (m_HomeBtnObj != null)
            {
                RectTransform rt = m_HomeBtnObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(0f, 1f);
                rt.pivot = new Vector2(0f, 1f);
                float xPos = isLandscape ? (35 + safeLeft) : (36 + safeLeft);
                rt.anchoredPosition = new Vector2(xPos, topBtnY);
            }

            // 1. Settings Gear: Top-Right Golden Gear Button (Option 2 Design B)
            if (m_SettingsBtnObj != null)
            {
                RectTransform rt = m_SettingsBtnObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(1f, 1f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(1f, 1f);
                float xPos = isLandscape ? -(35 + safeRight) : -(36 + safeRight);
                rt.anchoredPosition = new Vector2(xPos, topBtnY);
            }

            // 1b. Active Ball Skin Sphere: Beside Settings Gear on Top-Right (Option 2 Design B + Ball Preview)
            if (m_HeaderBallSkinBtnObj != null)
            {
                RectTransform rt = m_HeaderBallSkinBtnObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(1f, 1f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(1f, 1f);
                float xPos = isLandscape ? -(132 + safeRight) : -(138 + safeRight);
                rt.anchoredPosition = new Vector2(xPos, topBtnY);
            }

            // 2. Current Score Hero Plaque with Crown and Best Score ribbon: Top Center (Option 2 Design B)
            if (m_ScoreCardObj != null)
            {
                RectTransform rt = m_ScoreCardObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 1f);
                rt.anchorMax = new Vector2(0.5f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
                float yPos = isLandscape ? -(10 + safeTop) : -(16 + safeTop);
                rt.anchoredPosition = new Vector2(0, yPos);
            }

            // 4. Adventure Level HUD: Concept 1 Golden Arcade Header Banner with safe area padding
            if (m_LevelHUD != null)
            {
                RectTransform rt = m_LevelHUD.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 1f);
                rt.anchorMax = new Vector2(0.5f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
                // Guarantee breathing room for Android punch-hole selfie cameras and status bars
                float minTopNotchPad = 26f;
                float hudY = isLandscape ? 0f : -Mathf.Max(safeTop * 0.5f, minTopNotchPad);
                rt.anchoredPosition = new Vector2(0f, hudY);
            }

            if (m_GemObjectivesBar != null)
            {
                RectTransform gobRt = m_GemObjectivesBar.GetComponent<RectTransform>();
                if (gobRt != null)
                {
                    float objY = isLandscape ? -145f : -535f;
                    gobRt.anchoredPosition = new Vector2(0f, objY);
                    gobRt.sizeDelta = new Vector2(880f, 118f);
                }
            }

            // 5. Home Screen Top Utility Buttons (Wardrobe Star & Settings Gear for Android)
            if (m_HomeWardrobeBtn != null)
            {
                RectTransform wbRt = m_HomeWardrobeBtn.GetComponent<RectTransform>();
                float padTop = Mathf.Max(safeTop + 30f, 80f);
                float padSide = Mathf.Max(safeLeft + 35f, 55f);
                wbRt.anchoredPosition = new Vector2(padSide, -padTop);
            }
            if (m_HomeSettingsBtn != null)
            {
                RectTransform stRt = m_HomeSettingsBtn.GetComponent<RectTransform>();
                float padTop = Mathf.Max(safeTop + 30f, 80f);
                float padSide = Mathf.Max(safeRight + 35f, 55f);
                stRt.anchoredPosition = new Vector2(-padSide, -padTop);
            }

            // Ensure other unused HUD items are disabled
            if (m_CoinsWalletObj != null) m_CoinsWalletObj.SetActive(false);
            if (m_HomeBtnObj != null) m_HomeBtnObj.SetActive(false);
            if (m_BoosterSidebarObj != null) m_BoosterSidebarObj.SetActive(false);

            UpdateHUDVisibility();
            ReorganizeBoosterButtons(isLandscape);

            if (m_SkinWardrobeModal != null && m_SkinWardrobeModal.activeSelf)
            {
                Transform scene = m_SkinWardrobeModal.transform.Find("WardrobeSceneCard");
                if (scene != null)
                {
                    float canvasHeight = isLandscape ? 1080f : 1920f;
                    float canvasWidth = isLandscape ? 1920f : 1080f;
                    if (m_RootCanvas != null)
                    {
                        RectTransform cRt = m_RootCanvas.GetComponent<RectTransform>();
                        if (cRt != null && cRt.rect.height > 0) canvasHeight = cRt.rect.height;
                        if (cRt != null && cRt.rect.width > 0) canvasWidth = cRt.rect.width;
                    }
                    float fitScale = canvasHeight / 1376f;
                    if (768f * fitScale > canvasWidth) fitScale = canvasWidth / 768f;
                    scene.localScale = new Vector3(fitScale, fitScale, 1f);
                }
            }
            if (m_SettingsModal != null && m_SettingsModal.activeSelf)
            {
                Transform scene = m_SettingsModal.transform.Find("SettingsSceneCard");
                if (scene != null)
                {
                    float canvasHeight = isLandscape ? 1080f : 1920f;
                    float canvasWidth = isLandscape ? 1920f : 1080f;
                    if (m_RootCanvas != null)
                    {
                        RectTransform cRt = m_RootCanvas.GetComponent<RectTransform>();
                        if (cRt != null && cRt.rect.height > 0) canvasHeight = cRt.rect.height;
                        if (cRt != null && cRt.rect.width > 0) canvasWidth = cRt.rect.width;
                    }
                    float fitScale = canvasHeight / 1376f;
                    if (768f * fitScale > canvasWidth) fitScale = canvasWidth / 768f;
                    scene.localScale = new Vector3(fitScale, fitScale, 1f);
                }
            }
            if (m_VictoryModal != null && m_VictoryModal.activeSelf)
            {
                Transform scene = m_VictoryModal.transform.Find("VictorySceneCard");
                if (scene != null)
                {
                    float canvasHeight = isLandscape ? 1080f : 1920f;
                    float canvasWidth = isLandscape ? 1920f : 1080f;
                    if (m_RootCanvas != null)
                    {
                        RectTransform cRt = m_RootCanvas.GetComponent<RectTransform>();
                        if (cRt != null && cRt.rect.height > 0) canvasHeight = cRt.rect.height;
                        if (cRt != null && cRt.rect.width > 0) canvasWidth = cRt.rect.width;
                    }
                    float fitScale = canvasHeight / 1376f;
                    if (768f * fitScale > canvasWidth) fitScale = canvasWidth / 768f;
                    scene.localScale = new Vector3(fitScale, fitScale, 1f);
                }
            }
        }

        private void ReorganizeBoosterButtons(bool isLandscape)
        {
            BoosterType[] order = { BoosterType.Cannon, BoosterType.Bomb, BoosterType.Arrow, BoosterType.Shuffle };

            if (isLandscape)
            {
                // Vertical stack: Top to bottom
                float startY = 180f;
                float stepY = -120f;

                for (int i = 0; i < order.Length; i++)
                {
                    if (m_BoosterButtons.TryGetValue(order[i], out var item) && item.buttonObj != null)
                    {
                        RectTransform rt = item.buttonObj.GetComponent<RectTransform>();
                        rt.anchoredPosition = new Vector2(0, startY + i * stepY);
                    }
                }
            }
            else
            {
                // Horizontal dock: Left to right
                float startX = -210f;
                float stepX = 140f;

                for (int i = 0; i < order.Length; i++)
                {
                    if (m_BoosterButtons.TryGetValue(order[i], out var item) && item.buttonObj != null)
                    {
                        RectTransform rt = item.buttonObj.GetComponent<RectTransform>();
                        rt.anchoredPosition = new Vector2(startX + i * stepX, 0);
                    }
                }
            }
        }

        // ==========================================
        // ðŸ—ï¸ UI CONSTRUCTION
        // ==========================================
        private void BuildFullUI()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) canvas = FindAnyObjectByType<Canvas>();

            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("GameCanvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                m_CanvasScaler = canvasObj.AddComponent<CanvasScaler>();
                m_CanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                m_CanvasScaler.referenceResolution = new Vector2(1920, 1080);
                m_CanvasScaler.matchWidthOrHeight = 0.5f;
                m_CanvasScaler.dynamicPixelsPerUnit = 4.0f;
                canvasObj.AddComponent<GraphicRaycaster>();
            }
            else
            {
                m_CanvasScaler = canvas.GetComponent<CanvasScaler>();
                if (m_CanvasScaler != null) m_CanvasScaler.dynamicPixelsPerUnit = 4.0f;
            }

            m_RootCanvas = canvas.transform;
            m_Raycaster = canvas.GetComponent<GraphicRaycaster>();

            // 0. Top-Left: Home Button (Option 2 Design B: 3D Cyan Crystal Gem Button)
            BuildHomeButton();

            // 1. Top-Right: Settings Gear Button (Option 2 Design B: 3D Golden Gear Button)
            BuildSettingsButton();

            // 1b. Top-Right: Active Ball Skin Sphere Button (Option 2 Design B + 3D Ball Preview)
            BuildHeaderBallSkinButton();

            // 2. Top-Center: Current Score Hero Plaque (Option 2 Design B: 3D Royal Golden Arch Plaque)
            BuildScoreCard();

            // 3. Integrated Best Score Ribbon (Option 2 Design B: Hanging Golden Ribbon on Plaque)
            BuildBestScoreHUD();

            // 4. Modals & Screens
            BuildSettingsModal();
            BuildClassicGameOverModal();
            BuildModeSelectScreen();
            BuildLevelSelectScreen();
            BuildLevelHUD();
            BuildVictoryModal();
            BuildDefeatModal();
            BuildSkinWardrobeModal();
            BuildAchievementsModal();
            BuildAchievementToastUI();
            BuildComboCalloutUI();
            BuildTutorialOverlay();

            m_IsLandscape = Screen.width > Screen.height;
            ApplyOrientationLayout(m_IsLandscape);
            UpdateHUDVisibility();
        }

        private void BuildBestScoreHUD()
        {
            m_BestScoreObj = new GameObject("BestScoreRibbon");
            Transform parentTransform = (m_ScoreCardObj != null) ? m_ScoreCardObj.transform : m_RootCanvas;
            m_BestScoreObj.transform.SetParent(parentTransform, false);

            RectTransform rt = m_BestScoreObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            // Positioned right on the golden ribbon surface of Option 2 Design B (perfectly centered on ribbon)
            rt.anchoredPosition = new Vector2(24f, 52f);
            rt.sizeDelta = new Vector2(250f, 38f);

            int best = (GameManager.Instance != null) ? GameManager.Instance.BestScore : 0;
            Font candyFont = (m_CandyFont != null) ? m_CandyFont : m_DefaultFont;
            GameObject bestObj = CreateText(m_BestScoreObj.transform, Vector2.zero, new Vector2(250f, 38f), $"BEST: {best:N0}", 24, FontStyle.Bold, new Color(0.35f, 0.16f, 0.01f, 1.0f), TextAnchor.MiddleCenter, candyFont);
            m_BestScoreText = bestObj.GetComponent<Text>();
            // Crisp warm golden embossed highlight shadow underneath
            MakeTextCrisp(m_BestScoreText, new Color(1.0f, 0.95f, 0.70f, 0.85f), 0.8f);
        }

        private void BuildSettingsButton()
        {
            m_SettingsBtnObj = new GameObject("SettingsTopButton");
            m_SettingsBtnObj.transform.SetParent(m_RootCanvas, false);

            RectTransform rt = m_SettingsBtnObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.sizeDelta = new Vector2(88f, 88f);

            // Option 2 (Design B): 3D Golden Gear Cogwheel Settings Button
            Image bg = m_SettingsBtnObj.AddComponent<Image>();
            bg.sprite = SpriteFactory.GetGoldenGearSettingsButtonSprite();
            bg.raycastTarget = true;

            Button btn = m_SettingsBtnObj.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(() =>
            {
                if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
                HapticManager.TriggerLight();
                StartCoroutine(AnimateButtonPress(rt, () =>
                {
                    OpenSettings();
                }));
            });
        }

        private void BuildHeaderBallSkinButton()
        {
            m_HeaderBallSkinBtnObj = new GameObject("HeaderBallSkinButton");
            m_HeaderBallSkinBtnObj.transform.SetParent(m_RootCanvas, false);

            RectTransform rt = m_HeaderBallSkinBtnObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.sizeDelta = new Vector2(88f, 88f);

            // 1. Subtle Golden/Cyan Ring Backing matching Option 2 design
            Image ringBg = m_HeaderBallSkinBtnObj.AddComponent<Image>();
            ringBg.sprite = SpriteFactory.GetCircleButtonSprite(new Color(0.06f, 0.12f, 0.28f, 0.90f), new Color(1.0f, 0.84f, 0.24f, 0.95f), 0.10f);
            ringBg.raycastTarget = true;

            // 2. 3D Ball Skin Icon
            GameObject ballObj = new GameObject("BallIcon");
            ballObj.transform.SetParent(m_HeaderBallSkinBtnObj.transform, false);
            RectTransform ballRt = ballObj.AddComponent<RectTransform>();
            ballRt.anchorMin = new Vector2(0.5f, 0.5f);
            ballRt.anchorMax = new Vector2(0.5f, 0.5f);
            ballRt.anchoredPosition = Vector2.zero;
            ballRt.sizeDelta = new Vector2(68f, 68f);

            m_HeaderBallSkinImg = ballObj.AddComponent<Image>();
            m_HeaderBallSkinImg.raycastTarget = false;
            UpdateHeaderBallSkinPreview();

            // 3. Button Click Action: Opens Ball Skin Wardrobe
            Button btn = m_HeaderBallSkinBtnObj.AddComponent<Button>();
            btn.targetGraphic = ringBg;
            btn.onClick.AddListener(() =>
            {
                if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
                HapticManager.TriggerLight();
                StartCoroutine(AnimateButtonPress(rt, () =>
                {
                    OpenSkinWardrobe();
                }));
            });

            SkinManager.OnSkinChanged -= HandleSkinChangedForHeader;
            SkinManager.OnSkinChanged += HandleSkinChangedForHeader;
        }

        private void HandleSkinChangedForHeader(BallSkinType skin)
        {
            UpdateHeaderBallSkinPreview();
        }

        private void UpdateHeaderBallSkinPreview()
        {
            if (m_HeaderBallSkinImg != null)
            {
                BallSkinType equipped = SkinManager.CurrentSkin;
                Sprite skinSp = SpriteFactory.GetWardrobeSkinSprite(equipped);
                if (skinSp != null)
                {
                    m_HeaderBallSkinImg.sprite = skinSp;
                    m_HeaderBallSkinImg.color = Color.white;
                }
            }
        }

        private void BuildHomeButton()
        {
            m_HomeBtnObj = new GameObject("HomeTopButton");
            m_HomeBtnObj.transform.SetParent(m_RootCanvas, false);

            RectTransform rt = m_HomeBtnObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.sizeDelta = new Vector2(88f, 88f);

            // Option 2 (Design B): 3D Cyan Crystal Gem Button with House Icon
            Image bg = m_HomeBtnObj.AddComponent<Image>();
            bg.sprite = SpriteFactory.GetCyanCrystalHomeButtonSprite();
            bg.raycastTarget = true;

            Button btn = m_HomeBtnObj.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(() =>
            {
                if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
                HapticManager.TriggerLight();
                StartCoroutine(AnimateButtonPress(rt, () =>
                {
                    OpenModeSelect();
                }));
            });
        }

        private void BuildCoinsWallet()
        {
            // Removed from gameplay per user request
        }

        private void BuildScoreCard()
        {
            m_ScoreCardObj = new GameObject("CurrentScoreCenter");
            m_ScoreCardObj.transform.SetParent(m_RootCanvas, false);

            RectTransform rt = m_ScoreCardObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(500f, 290f);

            // Option 2 (Design B): 3D Royal Golden Arch Plaque with Crown and Ribbon
            Image plaqueBg = m_ScoreCardObj.AddComponent<Image>();
            plaqueBg.sprite = SpriteFactory.GetRoyalArchHeaderPlaqueSprite();
            plaqueBg.raycastTarget = false;

            // Big Bold Juicy White Score centered in the celestial sapphire cavity
            int score = (GameManager.Instance != null) ? GameManager.Instance.CurrentScore : 0;
            Font candyFont = (m_CandyFont != null) ? m_CandyFont : m_DefaultFont;
            GameObject scoreObj = CreateText(m_ScoreCardObj.transform, new Vector2(0f, -6f), new Vector2(400f, 100f), score.ToString("N0"), 76, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, candyFont);
            m_ScoreText = scoreObj.GetComponent<Text>();
            m_ScoreText.alignByGeometry = true;
            MakeTextCrisp(m_ScoreText, new Color(0.04f, 0.12f, 0.35f, 0.95f), 2.0f);

            // Combo Notification Popup
            GameObject comboObj = CreateText(m_RootCanvas, new Vector2(0, 280), new Vector2(600, 90), "", 46, FontStyle.Bold, new Color(1.0f, 0.85f, 0.2f), TextAnchor.MiddleCenter);
            m_ComboText = comboObj.GetComponent<Text>();
            comboObj.SetActive(false);
        }

        private void BuildBoosterSidebar()
        {
            // Boosters removed per user request
        }

        private void CreateBoosterButton(BoosterType type, Sprite iconSprite, string label)
        {
            GameObject btnObj = new GameObject($"Booster_{type}");
            btnObj.transform.SetParent(m_BoosterSidebarObj.transform, false);
            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(98, 98);

            // Rounded Squircle Outer Rim & Highlight
            Image highlight = btnObj.AddComponent<Image>();
            highlight.sprite = SpriteFactory.GetRoundedButtonSprite(new Color(0.10f, 0.16f, 0.30f, 0.95f), new Color(0.32f, 0.50f, 0.82f, 1.0f));

            // Button trigger
            Button btn = btnObj.AddComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                if (GameManager.Instance != null) GameManager.Instance.UseBoosterOrOpenShop(type);
            });

            // Center Icon (Ultra-sharp HD vector icon)
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(btnObj.transform, false);
            RectTransform iconRt = iconObj.AddComponent<RectTransform>();
            iconRt.sizeDelta = new Vector2(80, 80);
            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.sprite = iconSprite;
            iconImg.raycastTarget = false;

            // Pill Badge (Underneath)
            GameObject badgeObj = new GameObject("Badge");
            badgeObj.transform.SetParent(btnObj.transform, false);
            RectTransform badgeRt = badgeObj.AddComponent<RectTransform>();
            badgeRt.anchoredPosition = new Vector2(0, -44);
            badgeRt.sizeDelta = new Vector2(72, 28);
            Image badgeBg = badgeObj.AddComponent<Image>();
            badgeBg.sprite = SpriteFactory.GetPillBadgeSprite(new Color(0.12f, 0.78f, 0.32f));
            badgeBg.raycastTarget = false;

            GameObject badgeTxtObj = CreateText(badgeObj.transform, Vector2.zero, new Vector2(72, 28), "2", 20, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            Text badgeTxt = badgeTxtObj.GetComponent<Text>();

            m_BoosterButtons[type] = (btnObj, badgeTxt, highlight);
        }

        public void UpdateBoosterBar()
        {
            foreach (var kvp in m_BoosterButtons)
            {
                BoosterType type = kvp.Key;
                int count = (GameManager.Instance != null) ? GameManager.Instance.GetBoosterCount(type) : 0;

                if (kvp.Value.badgeText != null)
                {
                    kvp.Value.badgeText.text = (count > 0) ? $"{count}" : "SHOP";
                }
            }
        }

        public void UpdateBoosterSelection(BoosterType activeBooster)
        {
            foreach (var kvp in m_BoosterButtons)
            {
                if (kvp.Value.highlightImg != null)
                {
                    if (kvp.Key == activeBooster && activeBooster != BoosterType.None)
                    {
                        // Glowing golden highlight
                        kvp.Value.highlightImg.sprite = SpriteFactory.GetRoundedButtonSprite(new Color(0.18f, 0.22f, 0.38f, 1f), new Color(1.0f, 0.85f, 0.20f, 1f), 0.12f);
                    }
                    else
                    {
                        // Normal blue
                        kvp.Value.highlightImg.sprite = SpriteFactory.GetRoundedButtonSprite(new Color(0.10f, 0.16f, 0.30f, 0.95f), new Color(0.32f, 0.50f, 0.82f, 1.0f));
                    }
                }
            }
        }

        // ==========================================
        // ðŸ›’ BOOSTER SHOP MODAL
        // ==========================================
        private void BuildBoosterShopModal()
        {
            m_BoosterShopModal = new GameObject("BoosterShopModal");
            m_BoosterShopModal.transform.SetParent(m_RootCanvas, false);

            RectTransform rt = m_BoosterShopModal.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.sizeDelta = Vector2.zero;

            // Dim backdrop
            Image bg = m_BoosterShopModal.AddComponent<Image>();
            bg.color = new Color(0.04f, 0.06f, 0.12f, 0.92f);
            bg.raycastTarget = true;
            Button bgBtn = m_BoosterShopModal.AddComponent<Button>();
            bgBtn.transition = Selectable.Transition.None;
            bgBtn.onClick.AddListener(() =>
            {
                if (m_BoosterShopModal != null) m_BoosterShopModal.SetActive(false);
            });

            // Dialog Window
            GameObject winObj = new GameObject("ShopDialog");
            winObj.transform.SetParent(m_BoosterShopModal.transform, false);
            RectTransform winRt = winObj.AddComponent<RectTransform>();
            winRt.sizeDelta = new Vector2(980, 720);
            Image winBg = winObj.AddComponent<Image>();
            winBg.sprite = SpriteFactory.GetPanelSprite(new Color(0.08f, 0.14f, 0.28f, 0.98f), new Color(0.24f, 0.40f, 0.68f, 1f), 2.5f, 20);
            winBg.type = Image.Type.Sliced;
            winBg.raycastTarget = true;

            // Title Banner
            GameObject titleObj = CreateText(winObj.transform, new Vector2(0, 300), new Vector2(700, 90), "BOOSTER SHOP", 64, FontStyle.Bold, new Color(1.0f, 0.84f, 0.25f), TextAnchor.MiddleCenter);

            // Coins Balance Header Pill
            GameObject balanceObj = new GameObject("CoinBalance");
            balanceObj.transform.SetParent(winObj.transform, false);
            RectTransform balRt = balanceObj.AddComponent<RectTransform>();
            balRt.anchoredPosition = new Vector2(0, 220);
            balRt.sizeDelta = new Vector2(300, 60);
            Image balBg = balanceObj.AddComponent<Image>();
            balBg.color = new Color(0.12f, 0.22f, 0.40f);

            int coins = (GameManager.Instance != null) ? GameManager.Instance.CurrentCoins : 200;
            GameObject balTxtObj = CreateText(balanceObj.transform, Vector2.zero, new Vector2(300, 60), $"{coins:N0} COINS", 30, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            m_ShopCoinsText = balTxtObj.GetComponent<Text>();

            // 4 Shop Cards in a row
            BoosterType[] shopTypes = { BoosterType.Cannon, BoosterType.Bomb, BoosterType.Arrow, BoosterType.Shuffle };
            Sprite[] shopIcons = { GemIconFactory.GetCannonSprite(), GemIconFactory.GetBombSprite(), GemIconFactory.GetArrowSprite(), GemIconFactory.GetShuffleSprite() };
            string[] shopTitles = { "CANNON", "SUPER BOMB", "ARROW", "SHUFFLE" };
            string[] shopDescs = { "Blasts a full\nrow or column", "Blasts a 3x3\nboard area", "Removes any\none block", "Shuffle three\nfresh shapes" };

            float startX = -345f;
            float stepX = 230f;

            for (int i = 0; i < 4; i++)
            {
                BoosterType bType = shopTypes[i];
                int price = GameManager.GetBoosterPrice(bType);

                GameObject cardObj = new GameObject($"Card_{bType}");
                cardObj.transform.SetParent(winObj.transform, false);
                RectTransform cardRt = cardObj.AddComponent<RectTransform>();
                cardRt.anchoredPosition = new Vector2(startX + i * stepX, 20);
                cardRt.sizeDelta = new Vector2(210, 310);
                Image cardBg = cardObj.AddComponent<Image>();
                cardBg.color = new Color(0.12f, 0.20f, 0.38f);

                // Title
                CreateText(cardObj.transform, new Vector2(0, 115), new Vector2(210, 45), shopTitles[i], 24, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);

                // Icon
                GameObject icon = new GameObject("Icon");
                icon.transform.SetParent(cardObj.transform, false);
                RectTransform iRt = icon.AddComponent<RectTransform>();
                iRt.anchoredPosition = new Vector2(0, 35);
                iRt.sizeDelta = new Vector2(100, 100);
                Image iImg = icon.AddComponent<Image>();
                iImg.sprite = shopIcons[i];
                iImg.raycastTarget = false;

                // Description
                CreateText(cardObj.transform, new Vector2(0, -45), new Vector2(210, 50), shopDescs[i], 18, FontStyle.Normal, new Color(0.85f, 0.90f, 1f, 0.85f), TextAnchor.MiddleCenter);

                // BUY Button
                GameObject buyBtn = CreateButton(cardObj.transform, new Vector2(0, -105), new Vector2(180, 60), new Color(0.14f, 0.72f, 0.32f), $"BUY {price}", 24);
                buyBtn.GetComponent<Button>().onClick.AddListener(() =>
                {
                    if (GameManager.Instance != null)
                    {
                        if (GameManager.Instance.BuyBooster(bType))
                        {
                            UpdateCoins(GameManager.Instance.CurrentCoins);
                        }
                    }
                });
            }

            // Free Coins Option at Bottom: "FREE +50 COINS"
            GameObject adBtn = CreateButton(winObj.transform, new Vector2(0, -220), new Vector2(520, 80), new Color(1.0f, 0.70f, 0.10f), "FREE +50 COINS (WATCH AD)", 30);
            adBtn.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.AddCoins(50, true);
                    UpdateCoins(GameManager.Instance.CurrentCoins);
                    FloatingText.Create(Vector3.zero, "+50 COINS!", new Color(1f, 0.9f, 0.2f), 7f);
                }
            });

            // Close Button (X)
            GameObject closeBtn = CreateButton(winObj.transform, new Vector2(430, 310), new Vector2(76, 76), new Color(0.85f, 0.25f, 0.25f), "X", 34);
            closeBtn.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (m_BoosterShopModal != null) m_BoosterShopModal.SetActive(false);
            });
            closeBtn.transform.SetAsLastSibling();

            m_BoosterShopModal.SetActive(false);
        }

        public void OpenBoosterShop()
        {
            if (m_BoosterShopModal != null)
            {
                m_BoosterShopModal.SetActive(true);
                if (m_ShopCoinsText != null && GameManager.Instance != null)
                {
                    m_ShopCoinsText.text = $"{GameManager.Instance.CurrentCoins:N0} COINS";
                }
            }
        }

        // ==========================================
        // âš™ï¸ 3D ARCADE SETTINGS MODAL (CONCEPT 2 EXACT)
        // ==========================================
        private void BuildSettingsModal()
        {
            m_SettingsModal = new GameObject("SettingsModal", typeof(RectTransform));
            m_SettingsModal.transform.SetParent(m_RootCanvas, false);

            RectTransform rt = m_SettingsModal.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Fullscreen backdrop matching background deep space navy
            Image bg = m_SettingsModal.AddComponent<Image>();
            bg.color = new Color(0.04f, 0.07f, 0.16f, 0.98f);
            bg.raycastTarget = true;
            Button bgBtn = m_SettingsModal.AddComponent<Button>();
            bgBtn.transition = Selectable.Transition.None;
            bgBtn.onClick.AddListener(() =>
            {
                if (m_SettingsModal != null) m_SettingsModal.SetActive(false);
            });

            // Centered 3D Scene Viewport Card (768 x 1376 native aspect ratio)
            GameObject sceneObj = new GameObject("SettingsSceneCard", typeof(RectTransform));
            sceneObj.transform.SetParent(m_SettingsModal.transform, false);
            RectTransform sceneRt = sceneObj.GetComponent<RectTransform>();
            sceneRt.anchorMin = new Vector2(0.5f, 0.5f);
            sceneRt.anchorMax = new Vector2(0.5f, 0.5f);
            sceneRt.pivot = new Vector2(0.5f, 0.5f);
            sceneRt.anchoredPosition = Vector2.zero;
            sceneRt.sizeDelta = new Vector2(768, 1376);

            // Dynamically scale sceneRt to fit screen height perfectly
            float canvasHeight = 1920f;
            float canvasWidth = 1080f;
            if (m_RootCanvas != null)
            {
                RectTransform cRt = m_RootCanvas.GetComponent<RectTransform>();
                if (cRt != null && cRt.rect.height > 0) canvasHeight = cRt.rect.height;
                if (cRt != null && cRt.rect.width > 0) canvasWidth = cRt.rect.width;
            }
            float fitScale = canvasHeight / 1376f;
            if (768f * fitScale > canvasWidth)
            {
                fitScale = canvasWidth / 768f;
            }
            sceneRt.localScale = new Vector3(fitScale, fitScale, 1f);

            // 1. High-Definition 3D Render Artwork (Exact Concept 2 Background + Card + Star Orb)
            Image sceneImg = sceneObj.AddComponent<Image>();
            sceneImg.sprite = SpriteFactory.LoadOrFallbackSprite("SettingsBackground_Concept2");
            sceneImg.color = Color.white;
            sceneImg.raycastTarget = true;

            // 2. 3D Ruby Red Close (X) Button (placed at top-right X: 228, Y: 406)
            GameObject settingsCloseObj = new GameObject("SettingsCloseBtn", typeof(RectTransform));
            settingsCloseObj.transform.SetParent(sceneObj.transform, false);
            RectTransform sCloseRt = settingsCloseObj.GetComponent<RectTransform>();
            sCloseRt.anchoredPosition = new Vector2(228f, 406f);
            sCloseRt.sizeDelta = new Vector2(92f, 92f);
            Image sCloseImg = settingsCloseObj.AddComponent<Image>();
            sCloseImg.sprite = SpriteFactory.LoadOrFallbackSprite("Settings_Btn_CloseX");
            sCloseImg.color = Color.white;
            sCloseImg.raycastTarget = true;
            Button sCloseBtn = settingsCloseObj.AddComponent<Button>();
            sCloseBtn.onClick.AddListener(() =>
            {
                StartCoroutine(AnimateButtonPress(sCloseRt, () =>
                {
                    if (m_SettingsModal != null) m_SettingsModal.SetActive(false);
                }));
            });
            settingsCloseObj.transform.SetAsLastSibling();

            // 3. Sound Toggle (X: 152.5, Y: 183)
            CreateConcept2Toggle(sceneObj.transform, new Vector2(152.5f, 183f), new Vector2(138f, 68f), OnToggleSfxClicked, out m_SfxToggleTrackImg);

            // 4. Music Toggle (X: 152.5, Y: 39.5)
            CreateConcept2Toggle(sceneObj.transform, new Vector2(152.5f, 39.5f), new Vector2(138f, 68f), OnToggleMusicClicked, out m_MusicToggleTrackImg);

            // 5. Vibration Toggle (X: 152.5, Y: -107.5)
            CreateConcept2Toggle(sceneObj.transform, new Vector2(152.5f, -107.5f), new Vector2(138f, 68f), OnToggleHapticsClicked, out m_HapticToggleTrackImg);

            // 6. Ball Designs Button (X: 0, Y: -262, Size: 384 x 86)
            CreateConcept2Button(sceneObj.transform, new Vector2(0f, -262f), new Vector2(384f, 86f), "Settings_Btn_BallDesigns", () =>
            {
                m_SettingsModal.SetActive(false);
                OpenSkinWardrobe();
            });

            // 7. Restart Game Button (X: 0, Y: -385, Size: 384 x 86)
            CreateConcept2Button(sceneObj.transform, new Vector2(0f, -385f), new Vector2(384f, 86f), "Settings_Btn_Restart", () =>
            {
                m_SettingsModal.SetActive(false);
                if (LevelManager.Instance != null && LevelManager.Instance.IsLevelActive)
                {
                    LevelManager.Instance.RestartCurrentLevel();
                }
                else if (GameManager.Instance != null)
                {
                    GameManager.Instance.RestartGame();
                }
            });

            // 8. Home Button (X: 0, Y: -508, Size: 384 x 86)
            CreateConcept2Button(sceneObj.transform, new Vector2(0f, -508f), new Vector2(384f, 86f), "Settings_Btn_Home", OnHomeClicked);

            RefreshSettingsUI();
            m_SettingsModal.SetActive(false);
        }

        private GameObject CreateConcept2Button(Transform parent, Vector2 anchoredPos, Vector2 size, string spriteName, System.Action onClick)
        {
            GameObject obj = new GameObject(spriteName);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            Image img = obj.AddComponent<Image>();
            img.sprite = SpriteFactory.LoadOrFallbackSprite(spriteName);
            img.color = Color.white;
            img.raycastTarget = true;

            Button btn = obj.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(() =>
            {
                StartCoroutine(AnimateButtonPress(rt, onClick));
            });

            return obj;
        }

        private void CreateConcept2Toggle(
            Transform parent, 
            Vector2 anchoredPos, 
            Vector2 size, 
            UnityEngine.Events.UnityAction onToggleClicked, 
            out Image toggleImg)
        {
            GameObject obj = new GameObject("Toggle");
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            toggleImg = obj.AddComponent<Image>();
            toggleImg.sprite = SpriteFactory.LoadOrFallbackSprite("Settings_Toggle_3D_ON");
            toggleImg.color = Color.white;
            toggleImg.raycastTarget = true;

            Button btn = obj.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(() =>
            {
                StartCoroutine(AnimateButtonPress(rt, onToggleClicked.Invoke));
            });
        }

        private System.Collections.IEnumerator AnimateModalOpen(RectTransform dialogRt, float baseScale)
        {
            if (dialogRt == null) yield break;

            Vector3 startScale = new Vector3(baseScale * 0.88f, baseScale * 0.88f, 1f);
            Vector3 overshootScale = new Vector3(baseScale * 1.03f, baseScale * 1.03f, 1f);
            Vector3 endScale = new Vector3(baseScale, baseScale, 1f);

            float elapsed = 0f;
            const float dur1 = 0.12f;
            while (elapsed < dur1)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / dur1);
                dialogRt.localScale = Vector3.Lerp(startScale, overshootScale, Mathf.Sin(t * Mathf.PI * 0.5f));
                yield return null;
            }

            elapsed = 0f;
            const float dur2 = 0.08f;
            while (elapsed < dur2)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / dur2);
                dialogRt.localScale = Vector3.Lerp(overshootScale, endScale, t);
                yield return null;
            }
            dialogRt.localScale = endScale;
        }

        private void OnHomeClicked()
        {
            if (m_SettingsModal != null) m_SettingsModal.SetActive(false);
            HideDefeatModal();
            HideVictoryModal();
            HideGameOver();
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.StopLevel();
            }
            OpenModeSelect();
        }

        private void OnToggleSfxClicked()
        {
            if (Time.unscaledTime - m_LastToggleTime < 0.25f) return;
            m_LastToggleTime = Time.unscaledTime;

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.ToggleSFX();
            }
            else
            {
                bool cur = PlayerPrefs.GetInt("BoxBlast_SFX", 1) == 1;
                PlayerPrefs.SetInt("BoxBlast_SFX", cur ? 0 : 1);
                PlayerPrefs.Save();
            }
            HapticManager.TriggerLight();
            RefreshSettingsUI();
        }

        private void OnToggleMusicClicked()
        {
            if (Time.unscaledTime - m_LastToggleTime < 0.25f) return;
            m_LastToggleTime = Time.unscaledTime;

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.ToggleMusic();
            }
            else
            {
                bool cur = PlayerPrefs.GetInt("BoxBlast_Music", 1) == 1;
                PlayerPrefs.SetInt("BoxBlast_Music", cur ? 0 : 1);
                PlayerPrefs.Save();
            }
            HapticManager.TriggerLight();
            RefreshSettingsUI();
        }

        private void OnToggleHapticsClicked()
        {
            if (Time.unscaledTime - m_LastToggleTime < 0.25f) return;
            m_LastToggleTime = Time.unscaledTime;

            HapticManager.ToggleHaptics();
            HapticManager.TriggerLight();
            RefreshSettingsUI();
        }

        private void RefreshSettingsUI()
        {
            // 1. SFX Toggle
            bool sfxOn = (SoundManager.Instance != null) ? SoundManager.Instance.IsSfxEnabled : (PlayerPrefs.GetInt("BoxBlast_SFX", 1) == 1);
            if (m_SfxToggleTrackImg != null)
            {
                m_SfxToggleTrackImg.sprite = SpriteFactory.LoadOrFallbackSprite(sfxOn ? "Settings_Toggle_3D_ON" : "Settings_Toggle_3D_OFF");
            }

            // 2. Music Toggle
            bool musicOn = (SoundManager.Instance != null) ? SoundManager.Instance.IsMusicEnabled : (PlayerPrefs.GetInt("BoxBlast_Music", 1) == 1);
            if (m_MusicToggleTrackImg != null)
            {
                m_MusicToggleTrackImg.sprite = SpriteFactory.LoadOrFallbackSprite(musicOn ? "Settings_Toggle_3D_ON" : "Settings_Toggle_3D_OFF");
            }

            // 3. Vibration / Haptics Toggle
            bool hapticOn = HapticManager.IsHapticsEnabled;
            if (m_HapticToggleTrackImg != null)
            {
                m_HapticToggleTrackImg.sprite = SpriteFactory.LoadOrFallbackSprite(hapticOn ? "Settings_Toggle_3D_ON" : "Settings_Toggle_3D_OFF");
            }
        }

        public void OpenSettings()
        {
            if (m_SettingsModal == null) BuildSettingsModal();
            RefreshSettingsUI();
            if (m_SettingsModal != null)
            {
                m_SettingsModal.SetActive(true);
                m_SettingsModal.transform.SetAsLastSibling();

                Transform scene = m_SettingsModal.transform.Find("SettingsSceneCard");
                if (scene != null)
                {
                    float canvasHeight = 1920f;
                    float canvasWidth = 1080f;
                    if (m_RootCanvas != null)
                    {
                        RectTransform cRt = m_RootCanvas.GetComponent<RectTransform>();
                        if (cRt != null && cRt.rect.height > 0) canvasHeight = cRt.rect.height;
                        if (cRt != null && cRt.rect.width > 0) canvasWidth = cRt.rect.width;
                    }
                    float fitScale = canvasHeight / 1376f;
                    if (768f * fitScale > canvasWidth)
                    {
                        fitScale = canvasWidth / 768f;
                    }
                    scene.localScale = new Vector3(fitScale, fitScale, 1f);

                    StopCoroutine("AnimateModalOpen");
                    StartCoroutine(AnimateModalOpen(scene.GetComponent<RectTransform>(), fitScale));
                }
            }
        }

        // ==========================================
        // ðŸš€ TITLE & SPLASH SCREEN
        // ==========================================
        private void BuildTitleSplashScreen()
        {
            m_TitleSplashScreen = new GameObject("TitleSplashScreen");
            m_TitleSplashScreen.transform.SetParent(m_RootCanvas, false);

            RectTransform rt = m_TitleSplashScreen.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            // Fullscreen backdrop
            Image bg = m_TitleSplashScreen.AddComponent<Image>();
            bg.color = new Color(0.04f, 0.07f, 0.14f, 0.98f);
            bg.raycastTarget = true;

            // Tap anywhere on background advances to mode select
            Button tapBtn = m_TitleSplashScreen.AddComponent<Button>();
            tapBtn.onClick.AddListener(OnTitleScreenTapped);

            // Centered Main Content Dialog Card
            GameObject cardObj = new GameObject("TitleDialog");
            cardObj.transform.SetParent(m_TitleSplashScreen.transform, false);
            RectTransform cardRt = cardObj.AddComponent<RectTransform>();
            cardRt.sizeDelta = new Vector2(700, 780);
            Image cardBg = cardObj.AddComponent<Image>();
            cardBg.sprite = SpriteFactory.GetPanelSprite(new Color(0.07f, 0.12f, 0.22f, 0.98f), new Color(0.25f, 0.45f, 0.80f, 1f), 3f, 24);
            cardBg.type = Image.Type.Sliced;
            cardBg.raycastTarget = false;

            // 1. Welcome Pill Badge
            GameObject pillObj = new GameObject("WelcomeBadge");
            pillObj.transform.SetParent(cardObj.transform, false);
            RectTransform pRt = pillObj.AddComponent<RectTransform>();
            pRt.anchoredPosition = new Vector2(0, 310);
            pRt.sizeDelta = new Vector2(380, 44);
            Image pBg = pillObj.AddComponent<Image>();
            pBg.sprite = SpriteFactory.GetPillBadgeSprite(new Color(0.12f, 0.24f, 0.48f, 0.9f));
            pBg.raycastTarget = false;

            CreateText(pillObj.transform, Vector2.zero, new Vector2(380, 44), "WELCOME PUZZLE MASTER", 20, FontStyle.Bold, new Color(1.0f, 0.85f, 0.25f), TextAnchor.MiddleCenter);

            // 2. Big Vibrant Game Title
            CreateText(cardObj.transform, new Vector2(0, 225), new Vector2(660, 95), "BOX BLAST 2D", 68, FontStyle.Bold, new Color(1.0f, 0.88f, 0.25f), TextAnchor.MiddleCenter);

            // 3. Subtitle Tagline
            CreateText(cardObj.transform, new Vector2(0, 155), new Vector2(600, 40), "MATCH - BLAST - SOLVE", 24, FontStyle.Bold, new Color(0.55f, 0.82f, 1.0f), TextAnchor.MiddleCenter);

            // 4. Center Golden Trophy / Crown Emblem
            GameObject emblemObj = new GameObject("CenterEmblem");
            emblemObj.transform.SetParent(cardObj.transform, false);
            RectTransform eRt = emblemObj.AddComponent<RectTransform>();
            eRt.anchoredPosition = new Vector2(0, 15);
            eRt.sizeDelta = new Vector2(170, 170);

            Image eBg = emblemObj.AddComponent<Image>();
            eBg.sprite = SpriteFactory.GetRoundedButtonSprite(new Color(0.10f, 0.16f, 0.32f, 0.95f), new Color(1.0f, 0.84f, 0.25f, 1f), 0.08f);
            eBg.raycastTarget = false;

            GameObject iconObj = new GameObject("EmblemIcon");
            iconObj.transform.SetParent(emblemObj.transform, false);
            RectTransform iRt = iconObj.AddComponent<RectTransform>();
            iRt.sizeDelta = new Vector2(120, 120);
            Image iImg = iconObj.AddComponent<Image>();
            iImg.sprite = GemIconFactory.GetCrownSprite();
            iImg.raycastTarget = false;

            // 5. Feature Highlights
            CreateText(cardObj.transform, new Vector2(0, -115), new Vector2(620, 50), "4 Game Modes  -  80+ Levels  -  Juicy Combos", 22, FontStyle.Normal, new Color(0.78f, 0.85f, 0.98f, 0.9f), TextAnchor.MiddleCenter);

            // 6. Big Radiant Start Button
            GameObject startBtn = CreateButton(cardObj.transform, new Vector2(0, -210), new Vector2(460, 80), new Color(0.12f, 0.72f, 0.35f), "START GAME", 32);
            startBtn.GetComponent<Button>().onClick.AddListener(OnTitleScreenTapped);

            // 7. Hint text
            CreateText(cardObj.transform, new Vector2(0, -310), new Vector2(500, 36), "Tap anywhere to continue", 18, FontStyle.Normal, new Color(0.55f, 0.65f, 0.80f, 0.7f), TextAnchor.MiddleCenter);

            m_TitleSplashScreen.SetActive(false);
        }

        public void ShowTitleSplashScreen()
        {
            if (m_TitleSplashScreen != null)
            {
                m_TitleSplashScreen.SetActive(true);
                m_TitleSplashScreen.transform.SetAsLastSibling();
            }
        }

        public void HideTitleSplashScreen()
        {
            if (m_TitleSplashScreen != null)
            {
                m_TitleSplashScreen.SetActive(false);
            }
        }

        public void OnTitleScreenTapped()
        {
            HideTitleSplashScreen();
            OpenModeSelect();
        }

        public void OpenLevelSelect(GameModeType mode)
        {
            HideDefeatModal();
            HideVictoryModal();
            HideGameOver();
            if (m_ModeSelectPanel != null) m_ModeSelectPanel.SetActive(false);
            m_IsLevelSelectTransitioning = false;

            if (m_LevelSelectPanel == null)
            {
                BuildLevelSelectScreen();
            }

            m_ActiveViewingMode = mode;
            int maxLvl = LevelManager.GetMaxLevel(mode);
            int activeLevel = 1;
            for (int l = 1; l <= maxLvl; l++)
            {
                if (LevelManager.IsLevelUnlocked(mode, l))
                {
                    activeLevel = l;
                    if (LevelManager.GetLevelStars(mode, l) == 0)
                    {
                        break;
                    }
                }
                else break;
            }
            m_LevelSelectPage = 0;
            RefreshLevelSelectGrid(mode);
            if (m_LevelSelectPanel != null)
            {
                m_LevelSelectPanel.SetActive(true);
                m_LevelSelectPanel.transform.SetAsLastSibling();

                Transform scene = m_LevelSelectPanel.transform.Find("AdventureSceneCard");
                if (scene != null)
                {
                    float canvasHeight = 1920f;
                    float canvasWidth = 1080f;
                    if (m_RootCanvas != null)
                    {
                        RectTransform cRt = m_RootCanvas.GetComponent<RectTransform>();
                        if (cRt != null && cRt.rect.height > 0) canvasHeight = cRt.rect.height;
                        if (cRt != null && cRt.rect.width > 0) canvasWidth = cRt.rect.width;
                    }
                    float fitScale = canvasHeight / 1376f;
                    if (768f * fitScale > canvasWidth)
                    {
                        fitScale = canvasWidth / 768f;
                    }
                    scene.localScale = new Vector3(fitScale, fitScale, 1f);

                    StopCoroutine("AnimateModalOpen");
                    StartCoroutine(AnimateModalOpen(scene.GetComponent<RectTransform>(), fitScale));
                }
            }
            ScrollToActiveLevel(mode);
            UpdateHUDVisibility();
        }

        public void OpenModeSelect()
        {
            HideDefeatModal();
            HideVictoryModal();
            HideGameOver();
            if (m_LevelSelectPanel != null) m_LevelSelectPanel.SetActive(false);
            if (LevelManager.Instance != null && (LevelManager.Instance.IsLevelActive || LevelManager.Instance.IsLevelCompleted))
            {
                LevelManager.Instance.StopLevel();
            }
            m_IsLevelSelectTransitioning = false;
            if (m_ModeSelectPanel == null) BuildModeSelectScreen();
            RefreshModeCardStats();
            if (m_ModeSelectPanel != null)
            {
                m_ModeSelectPanel.SetActive(true);
                m_ModeSelectPanel.transform.SetAsLastSibling();
            }
            UpdateHUDVisibility();

            if (PlayerPrefs.GetInt("BoxBlast_HomeTour_Seen", 0) == 0)
            {
                StartCoroutine(DelayedShowHomeTour(0.35f));
            }
        }

        public int GetTotalModeStars(GameModeType mode)
        {
            int total = 0;
            int maxLvl = LevelManager.GetMaxLevel(mode);
            for (int i = 1; i <= maxLvl; i++)
            {
                total += LevelManager.GetLevelStars(mode, i);
            }
            return total;
        }

        private void RefreshModeCardStats()
        {
            int bestScore = (GameManager.Instance != null) ? GameManager.Instance.BestScore : PlayerPrefs.GetInt("BoxBlast_BestScore", 0);
            int dailyStreak = PlayerPrefs.GetInt("BoxBlast_DailyStreak", 0);
            if (m_DailyStreakText != null)
            {
                m_DailyStreakText.text = $"{dailyStreak} DAYS";
            }
            if (m_HomeClassicBadgeText != null)
            {
                m_HomeClassicBadgeText.text = bestScore > 0 ? $"BEST: {bestScore:N0}" : "PLAY";
                m_HomeClassicBadgeText.color = bestScore > 0 ? new Color(1.0f, 0.88f, 0.25f) : Color.white;
            }
            if (m_HomeAdventureBadgeText != null)
            {
                int currentAdvLvl = LevelManager.GetHighestUnlockedLevel(GameModeType.Adventure);
                m_HomeAdventureBadgeText.text = $"LEVEL {currentAdvLvl}";
            }
            if (m_ModeCardClassicBestText != null && GameManager.Instance != null)
            {
                m_ModeCardClassicBestText.text = $"Best: {GameManager.Instance.BestScore:N0}";
            }
            if (m_ModeCardAdvStarsText != null)
            {
                int maxAdv = LevelManager.GetMaxLevel(GameModeType.Adventure);
                m_ModeCardAdvStarsText.text = $"{GetTotalModeStars(GameModeType.Adventure)} / {maxAdv * 3} Stars";
            }
            if (m_ModeCardEventStarsText != null)
            {
                m_ModeCardEventStarsText.text = $"{GetTotalModeStars(GameModeType.Event)} / 60 Stars";
            }
            if (m_ModeCardTaskStarsText != null)
            {
                m_ModeCardTaskStarsText.text = $"{GetTotalModeStars(GameModeType.Task)} / 60 Stars";
            }

            if (m_AchievementsUnclaimedBadgeText != null)
            {
                int unclaimed = (AchievementManager.Instance != null) ? AchievementManager.Instance.GetUnclaimedCount() : 0;
                m_AchievementsUnclaimedBadgeText.text = unclaimed.ToString();
                m_AchievementsUnclaimedBadgeText.transform.parent.gameObject.SetActive(unclaimed > 0);
            }

            UpdateHomeEquippedBallPreview();
        }

        // ==========================================
        // ðŸ“Š HUD & LEVEL SUPPORT
        // ==========================================
        public void UpdateScore(int currentScore, int bestScore)
        {
            if (m_ScoreText != null)
            {
                m_ScoreText.text = currentScore.ToString("N0");
                StartCoroutine(PunchScale(m_ScoreText.rectTransform, 1.12f, 0.16f));
            }
            if (m_BestScoreText != null) m_BestScoreText.text = $"BEST: {bestScore:N0}";
            if (m_LevelScoreText != null) m_LevelScoreText.text = $"<color=#FFECA8>SCORE:</color> {currentScore:N0}";
        }

        public void UpdateCoins(int currentCoins)
        {
            if (m_CoinsText != null) m_CoinsText.text = currentCoins.ToString("N0");
            if (m_ShopCoinsText != null) m_ShopCoinsText.text = $"{currentCoins:N0} COINS";
        }

        public void ShowCombo(int comboCount, int bonusPoints)
        {
            // Kept simple: combo text disabled per user preference
        }

        private void HideCombo()
        {
            if (m_ComboText != null) m_ComboText.gameObject.SetActive(false);
        }

        public void UpdateHUDVisibility()
        {
            bool isHomeOpen = (m_ModeSelectPanel != null && m_ModeSelectPanel.activeSelf);
            bool isLevelSelectOpen = (m_LevelSelectPanel != null && m_LevelSelectPanel.activeSelf);
            bool isLevel = (LevelManager.Instance != null && LevelManager.Instance.IsLevelActive);

            bool showGameplayHUD = !isHomeOpen && !isLevelSelectOpen;

            if (m_BestScoreObj != null) m_BestScoreObj.SetActive(showGameplayHUD && !isLevel);
            if (m_SettingsBtnObj != null) m_SettingsBtnObj.SetActive(showGameplayHUD && !isLevel);
            if (m_HeaderBallSkinBtnObj != null) m_HeaderBallSkinBtnObj.SetActive(showGameplayHUD && !isLevel);
            if (m_ScoreCardObj != null) m_ScoreCardObj.SetActive(showGameplayHUD && !isLevel);
            if (m_LevelHUD != null) m_LevelHUD.SetActive(showGameplayHUD && isLevel);
            if (m_HomeBtnObj != null) m_HomeBtnObj.SetActive(showGameplayHUD && !isLevel);
            if (m_CoinsWalletObj != null) m_CoinsWalletObj.SetActive(false);
            if (m_BoosterSidebarObj != null) m_BoosterSidebarObj.SetActive(false);
        }

        public void ShowLevelHUD(LevelData level, int moves, string goalText)
        {
            HideDefeatModal();
            HideVictoryModal();
            HideGameOver();
            if (m_LevelHUD != null)
            {
                if (m_LevelTitleText != null) m_LevelTitleText.text = $"LEVEL {level.LevelNumber}";
                if (m_LevelMovesText != null) m_LevelMovesText.text = (level.MovesLimit > 0) ? $"<color=#9FE8FF>MOVES:</color> <color=#FFDE38>{moves}</color>" : "<color=#9FE8FF>MOVES:</color> --";
                if (m_LevelGoalText != null) m_LevelGoalText.text = goalText;
                if (m_LevelScoreText != null && GameManager.Instance != null)
                {
                    m_LevelScoreText.text = $"<color=#FFECA8>SCORE:</color> {GameManager.Instance.CurrentScore:N0}";
                }

                bool isGemLevel = (level.GoalType == LevelGoalType.CollectGems && level.TargetGems != null && level.TargetGems.Count > 0);
                if (m_LevelGoalBadgeObj != null) m_LevelGoalBadgeObj.SetActive(!isGemLevel);
                if (m_GemObjectivesBar != null) m_GemObjectivesBar.SetActive(isGemLevel);
            }
            UpdateHUDVisibility();
        }

        public void HideLevelHUD()
        {
            UpdateHUDVisibility();
        }

        public void UpdateMoves(int movesRemaining)
        {
            if (m_LevelMovesText != null)
            {
                if (movesRemaining <= 5 && movesRemaining > 0)
                {
                    m_LevelMovesText.text = $"<color=#FFB0B0>MOVES:</color> <color=#FF4444>{movesRemaining}</color>";
                }
                else
                {
                    m_LevelMovesText.text = $"<color=#9FE8FF>MOVES:</color> <color=#FFDE38>{movesRemaining}</color>";
                }
            }
        }

        public void TriggerLowMovesWarning(int movesLeft)
        {
            if (m_LevelMovesBoxRt == null || m_LevelMovesText == null) return;
            if (m_LowMovesCoroutine != null) StopCoroutine(m_LowMovesCoroutine);
            m_LowMovesCoroutine = StartCoroutine(AnimateLowMovesHeartbeat(movesLeft));
        }

        private System.Collections.IEnumerator AnimateLowMovesHeartbeat(int movesLeft)
        {
            if (m_LevelMovesBoxRt == null) yield break;

            if (SoundManager.Instance != null) SoundManager.Instance.PlayTensionTick();
            HapticManager.TriggerHeavy();

            if (movesLeft == 5)
            {
                FloatingText.CreateBadge(Vector3.up * 1.5f, "ONLY 5 MOVES REMAINING!", new Color(1f, 0.95f, 0.4f), new Color(0.85f, 0.2f, 0.2f));
            }

            float[] scales = new float[] { 1.25f, 1.05f, 1.20f, 1.0f };
            float[] times = new float[] { 0.10f, 0.08f, 0.10f, 0.12f };

            Vector3 origScale = Vector3.one;
            for (int step = 0; step < scales.Length; step++)
            {
                Vector3 start = m_LevelMovesBoxRt.localScale;
                Vector3 target = origScale * scales[step];
                float dur = times[step];
                float elapsed = 0f;
                while (elapsed < dur)
                {
                    elapsed += Time.unscaledDeltaTime;
                    m_LevelMovesBoxRt.localScale = Vector3.Lerp(start, target, elapsed / dur);
                    yield return null;
                }
            }
            m_LevelMovesBoxRt.localScale = origScale;
        }

        public void UpdateGoalProgress(string goalProgressText)
        {
            if (m_LevelGoalText != null) m_LevelGoalText.text = goalProgressText;
        }

        public void UpdateGemObjectives(Dictionary<GemType, int> collected, Dictionary<GemType, int> targets)
        {
            if (m_GemObjectivesBar == null) return;

            if (targets == null || targets.Count == 0)
            {
                m_GemObjectivesBar.SetActive(false);
                if (m_LevelGoalBadgeObj != null) m_LevelGoalBadgeObj.SetActive(true);
                return;
            }

            m_GemObjectivesBar.SetActive(true);
            if (m_LevelGoalBadgeObj != null) m_LevelGoalBadgeObj.SetActive(false);

            List<GemType> activeGems = new List<GemType>();
            GemType[] allGems = new GemType[] { GemType.BlueDiamond, GemType.OrangePentagon, GemType.YellowStar, GemType.RedRuby };
            for (int i = 0; i < allGems.Length; i++)
            {
                if (targets.ContainsKey(allGems[i]) && targets[allGems[i]] > 0)
                {
                    activeGems.Add(allGems[i]);
                }
            }

            float slotSpacing = 155f;
            if (activeGems.Count == 3) slotSpacing = 185f;
            else if (activeGems.Count == 2) slotSpacing = 215f;
            else if (activeGems.Count <= 1) slotSpacing = 0f;

            float totalSpan = (activeGems.Count - 1) * slotSpacing;
            float startX = -totalSpan * 0.5f;

            for (int i = 0; i < allGems.Length; i++)
            {
                GemType g = allGems[i];
                if (!m_GemObjectiveItems.ContainsKey(g)) continue;

                var item = m_GemObjectiveItems[g];
                int activeIndex = activeGems.IndexOf(g);
                if (activeIndex >= 0)
                {
                    item.rootObj.SetActive(true);
                    float posX = startX + activeIndex * slotSpacing;
                    item.rootObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(posX, 0);

                    int targetVal = targets[g];
                    int currentVal = (collected != null && collected.ContainsKey(g)) ? collected[g] : 0;
                    int remaining = Mathf.Max(0, targetVal - currentVal);

                    if (remaining == 0)
                    {
                        // Objective complete! Show crisp 3D green checkmark badge with pop animation
                        if (item.checkImg != null)
                        {
                            if (!item.checkImg.gameObject.activeSelf)
                            {
                                item.checkImg.gameObject.SetActive(true);
                                StartCoroutine(PunchScale(item.checkImg.rectTransform, 1.4f, 0.22f));
                            }
                        }
                        if (item.countText != null)
                        {
                            if (item.checkImg != null)
                            {
                                item.countText.gameObject.SetActive(false);
                            }
                            else
                            {
                                item.countText.gameObject.SetActive(true);
                                item.countText.text = "<color=#40FF80>\u2713</color>";
                                item.countText.fontSize = 36;
                            }
                        }
                    }
                    else
                    {
                        // In-progress: show remaining count ("x1", "x5", etc.)
                        if (item.checkImg != null)
                        {
                            item.checkImg.gameObject.SetActive(false);
                        }
                        if (item.countText != null)
                        {
                            item.countText.gameObject.SetActive(true);
                            item.countText.text = $"x{remaining}";
                            item.countText.color = Color.white;
                            item.countText.fontSize = 36;
                            MakeTextCrisp(item.countText, new Color(0.01f, 0.05f, 0.18f, 0.95f), 1.0f);
                        }
                    }
                }
                else
                {
                    item.rootObj.SetActive(false);
                }
            }
        }

        public void FlyGemToHUD(GemType gem, Vector3 worldStartPos)
        {
            if (m_RootCanvas == null || gem == GemType.None) return;
            StartCoroutine(AnimateFlyingGem(gem, worldStartPos));
        }

        private System.Collections.IEnumerator AnimateFlyingGem(GemType gem, Vector3 worldStartPos)
        {
            GameObject flyObj = new GameObject($"FlyGem_{gem}");
            flyObj.transform.SetParent(m_RootCanvas, false);
            RectTransform rt = flyObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(48, 48);

            Image img = flyObj.AddComponent<Image>();
            img.sprite = GemIconFactory.GetGemSprite(gem);
            img.raycastTarget = false;

            Vector3 targetScreenPos = new Vector3(Screen.width * 0.5f, Screen.height - 80f, 0f);
            if (m_GemObjectiveItems.ContainsKey(gem) && m_GemObjectiveItems[gem].iconRt != null && m_GemObjectiveItems[gem].rootObj.activeInHierarchy)
            {
                targetScreenPos = m_GemObjectiveItems[gem].iconRt.position;
            }

            Vector3 startScreenPos = Camera.main != null ? Camera.main.WorldToScreenPoint(worldStartPos) : worldStartPos;
            startScreenPos.z = 0f;
            targetScreenPos.z = 0f;

            RectTransform canvasRt = m_RootCanvas.GetComponent<RectTransform>();
            Vector2 startCanvasPos, targetCanvasPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, startScreenPos, null, out startCanvasPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, targetScreenPos, null, out targetCanvasPos);

            rt.anchoredPosition = startCanvasPos;
            rt.localScale = Vector3.one * 1.3f;

            float duration = 0.5f;
            float elapsed = 0f;
            Vector2 midPoint = (startCanvasPos + targetCanvasPos) * 0.5f + new Vector2(Random.Range(-40f, 40f), 70f);

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float easeT = t * t * (3f - 2f * t);

                Vector2 pos = (1f - easeT) * (1f - easeT) * startCanvasPos + 2f * (1f - easeT) * easeT * midPoint + easeT * easeT * targetCanvasPos;
                rt.anchoredPosition = pos;

                float scale = Mathf.Lerp(1.3f, 0.9f, easeT);
                rt.localScale = new Vector3(scale, scale, 1f);

                yield return null;
            }

            Destroy(flyObj);

            if (m_GemObjectiveItems.ContainsKey(gem) && m_GemObjectiveItems[gem].iconRt != null)
            {
                StartCoroutine(PunchScale(m_GemObjectiveItems[gem].iconRt, 1.4f, 0.2f));
            }
        }

        private System.Collections.IEnumerator PunchScale(RectTransform targetRt, float punchScale, float duration)
        {
            if (targetRt == null) yield break;
            Vector3 originalScale = Vector3.one;
            float half = duration * 0.5f;
            float elapsed = 0f;

            while (elapsed < half)
            {
                elapsed += Time.unscaledDeltaTime;
                if (targetRt == null) yield break;
                float t = elapsed / half;
                targetRt.localScale = Vector3.Lerp(originalScale, originalScale * punchScale, t);
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < half)
            {
                elapsed += Time.unscaledDeltaTime;
                if (targetRt == null) yield break;
                float t = elapsed / half;
                targetRt.localScale = Vector3.Lerp(originalScale * punchScale, originalScale, t);
                yield return null;
            }

            if (targetRt != null) targetRt.localScale = originalScale;
        }

        public void ShowGameOver(int finalScore, int bestScore, bool isNewBest)
        {
            if (m_GameOverPanel == null) BuildClassicGameOverModal();
            if (m_GameOverScoreText != null) m_GameOverScoreText.text = $"{finalScore:N0}";
            if (m_GameOverBestText != null) m_GameOverBestText.text = $"BEST: {bestScore:N0}";
            if (m_GameOverNewBestObj != null) m_GameOverNewBestObj.SetActive(isNewBest);

            bool canRevive = (GameManager.Instance != null && GameManager.Instance.CanRevive);
            int remaining = (GameManager.Instance != null) ? GameManager.Instance.RevivesRemaining : 0;

            if (m_GameOverReviveBtn != null)
            {
                Button btn = m_GameOverReviveBtn.GetComponent<Button>();
                if (btn != null) btn.interactable = canRevive;
                if (m_GameOverReviveText != null)
                {
                    m_GameOverReviveText.text = canRevive
                        ? (remaining == 1 ? "REVIVE (1 CHANCE LEFT)" : $"REVIVE ({remaining} CHANCES LEFT)")
                        : "NO REVIVES LEFT";
                    m_GameOverReviveText.color = canRevive ? Color.white : new Color(1f, 1f, 1f, 0.55f);
                }
            }

            if (m_GameOverPanel != null)
            {
                RectTransform pRt = m_GameOverPanel.GetComponent<RectTransform>();
                if (pRt != null)
                {
                    pRt.anchorMin = Vector2.zero;
                    pRt.anchorMax = Vector2.one;
                    pRt.offsetMin = Vector2.zero;
                    pRt.offsetMax = Vector2.zero;
                    pRt.sizeDelta = Vector2.zero;
                }

                m_GameOverPanel.SetActive(true);
                m_GameOverPanel.transform.SetAsLastSibling();

                Transform scene = m_GameOverPanel.transform.Find("GameOverSceneCard");
                if (scene != null)
                {
                    float canvasHeight = 1920f;
                    float canvasWidth = 1080f;
                    if (m_RootCanvas != null)
                    {
                        RectTransform cRt = m_RootCanvas.GetComponent<RectTransform>();
                        if (cRt != null && cRt.rect.height > 0) canvasHeight = cRt.rect.height;
                        if (cRt != null && cRt.rect.width > 0) canvasWidth = cRt.rect.width;
                    }
                    float scaleByH = canvasHeight / 1376f;
                    float scaleByW = (canvasWidth * 1.08f) / 768f;
                    float fitScale = Mathf.Min(scaleByH, scaleByW);
                    scene.localScale = new Vector3(fitScale, fitScale, 1f);

                    StopCoroutine("AnimateModalOpen");
                    StartCoroutine(AnimateModalOpen(scene.GetComponent<RectTransform>(), fitScale));
                }
            }
        }

        public void HideGameOver()
        {
            if (m_GameOverPanel != null) m_GameOverPanel.SetActive(false);
        }

        public void ShowLevelCompleteModal(GameModeType mode, int levelNum, int stars, bool hasNextLevel, SkinInfo unlockedSkin = null)
        {
            if (m_VictoryModal == null) BuildVictoryModal();
            if (m_VictoryModal != null)
            {
                if (m_VictoryTitleText != null)
                {
                    m_VictoryTitleText.text = $"LEVEL {levelNum} CLEAR!";
                }
                if (m_VictorySubtitleText != null)
                {
                    int totalLvl = LevelManager.GetMaxLevel(mode);
                    m_VictorySubtitleText.text = hasNextLevel ? $"Earned {stars} Star{(stars > 1 ? "s" : "")}! Level {levelNum + 1} Unlocked" : $"All {totalLvl} Levels Completed! Master Champion!";
                    m_VictorySubtitleText.fontStyle = FontStyle.Normal;
                    m_VictorySubtitleText.color = new Color(0.20f, 0.08f, 0.01f, 1f);
                    m_VictorySubtitleText.fontSize = 24;
                    m_VictorySubtitleText.resizeTextForBestFit = true;
                    m_VictorySubtitleText.resizeTextMinSize = 16;
                    m_VictorySubtitleText.resizeTextMaxSize = 25;

                    // Remove blurry light shadow that causes ghosting / muddy halo on golden ribbon
                    Shadow sh = m_VictorySubtitleText.GetComponent<Shadow>();
                    if (sh != null) DestroyImmediate(sh);
                    Outline ot = m_VictorySubtitleText.GetComponent<Outline>();
                    if (ot != null) DestroyImmediate(ot);

                    RectTransform sRt = m_VictorySubtitleText.rectTransform.parent as RectTransform;
                    if (sRt != null)
                    {
                        sRt.anchoredPosition = new Vector2(0f, -81.5f);
                        sRt.sizeDelta = new Vector2(560f, 44f);
                    }
                    m_VictorySubtitleText.rectTransform.sizeDelta = new Vector2(540f, 40f);
                }
                if (m_VictoryNextBtnObj != null)
                {
                    m_VictoryNextBtnObj.SetActive(hasNextLevel);
                }
                if (m_VictoryNextDisabledOverlay != null)
                {
                    m_VictoryNextDisabledOverlay.SetActive(!hasNextLevel);
                }
                if (m_VictoryStarImgs != null)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (m_VictoryStarImgs[i] != null)
                        {
                            m_VictoryStarImgs[i].sprite = GemIconFactory.GetStarSprite(i < stars);
                        }
                    }
                }

                // Ball Skin Reward Showcase
                if (m_VictorySkinRewardObj != null)
                {
                    if (unlockedSkin != null)
                    {
                        m_VictorySkinRewardObj.SetActive(true);
                        if (m_VictorySkinPreviewImg != null)
                        {
                            m_VictorySkinPreviewImg.sprite = SpriteFactory.GetBlockSprite(new Color(0.18f, 0.50f, 0.95f), unlockedSkin.Type);
                        }
                        if (m_VictorySkinNameText != null)
                        {
                            m_VictorySkinNameText.text = $"{unlockedSkin.Name.ToUpper()} BALL UNLOCKED!";
                        }
                        if (m_VictorySkinEquipBtn != null)
                        {
                            m_VictorySkinEquipBtn.onClick.RemoveAllListeners();
                            m_VictorySkinEquipBtn.onClick.AddListener(() =>
                            {
                                SkinManager.EquipSkin(unlockedSkin.Type);
                                if (m_VictorySkinEquipBtnText != null) m_VictorySkinEquipBtnText.text = "EQUIPPED";
                                m_VictorySkinEquipBtn.interactable = false;
                            });
                            if (m_VictorySkinEquipBtnText != null) m_VictorySkinEquipBtnText.text = "EQUIP NOW";
                            m_VictorySkinEquipBtn.interactable = true;
                        }
                    }
                    else
                    {
                        m_VictorySkinRewardObj.SetActive(false);
                    }
                }

                m_VictoryModal.SetActive(true);
                m_VictoryModal.transform.SetAsLastSibling();

                Transform scene = m_VictoryModal.transform.Find("VictorySceneCard");
                if (scene != null)
                {
                    float canvasHeight = 1920f;
                    float canvasWidth = 1080f;
                    if (m_RootCanvas != null)
                    {
                        RectTransform cRt = m_RootCanvas.GetComponent<RectTransform>();
                        if (cRt != null && cRt.rect.height > 0) canvasHeight = cRt.rect.height;
                        if (cRt != null && cRt.rect.width > 0) canvasWidth = cRt.rect.width;
                    }
                    float fitScale = Mathf.Max(canvasHeight / 1376f, canvasWidth / 768f);
                    scene.localScale = new Vector3(fitScale, fitScale, 1f);

                    StopCoroutine("AnimateModalOpen");
                    StartCoroutine(AnimateModalOpen(scene.GetComponent<RectTransform>(), fitScale));
                }

                // Celebratory Confetti & Fireworks
                if (ParticleFXManager.Instance != null)
                {
                    ParticleFXManager.Instance.PlayComboCelebration(Vector3.zero, 5);
                }
            }
        }

        public void ShowLevelFailedModal(GameModeType mode, int levelNum, DefeatReason reason = DefeatReason.OutOfMoves)
        {
            if (m_DefeatModal == null) BuildDefeatModal();
            if (m_DefeatModal != null)
            {
                if (reason == DefeatReason.BoardFull)
                {
                    if (m_DefeatSubtitleText != null)
                    {
                        m_DefeatSubtitleText.text = "No pieces can fit on board! Don't give up.";
                    }
                }
                else
                {
                    if (m_DefeatSubtitleText != null)
                    {
                        m_DefeatSubtitleText.text = "You ran out of moves. Don't give up! Try again.";
                    }
                }

                bool canExtra = (LevelManager.Instance != null && LevelManager.Instance.CanAddExtraMoves);
                int remaining = (LevelManager.Instance != null) ? LevelManager.Instance.ExtraMovesRemaining : 0;

                if (m_DefeatExtraMovesBtn != null)
                {
                    m_DefeatExtraMovesBtn.SetActive(true);
                    Button extraBtn = m_DefeatExtraMovesBtn.GetComponent<Button>();
                    if (extraBtn != null) extraBtn.interactable = canExtra;
                    if (m_DefeatExtraMovesText != null)
                    {
                        m_DefeatExtraMovesText.text = canExtra ? $"+5 MOVES ({remaining} LEFT)" : "NO EXTRA MOVES";
                    }
                }

                // Dynamic responsive card scale for 9:16 aspect ratio across all mobile displays (matching Settings modal)
                Transform scene = m_DefeatModal.transform.Find("DefeatSceneCard");
                if (scene != null)
                {
                    float canvasHeight = 1920f;
                    float canvasWidth = 1080f;
                    if (m_RootCanvas != null)
                    {
                        RectTransform cRt = m_RootCanvas.GetComponent<RectTransform>();
                        if (cRt != null && cRt.rect.height > 0)
                        {
                            canvasHeight = cRt.rect.height;
                            canvasWidth = cRt.rect.width;
                        }
                    }
                    float scaleByH = canvasHeight / 1376f;
                    float scaleByW = (canvasWidth * 1.08f) / 768f;
                    float fitScale = Mathf.Min(scaleByH, scaleByW);
                    scene.localScale = new Vector3(fitScale, fitScale, 1f);

                    StopCoroutine("AnimateModalOpen");
                    StartCoroutine(AnimateModalOpen(scene.GetComponent<RectTransform>(), fitScale));
                }

                m_DefeatModal.SetActive(true);
                m_DefeatModal.transform.SetAsLastSibling();
            }
        }

        public void HideDefeatModal()
        {
            if (m_DefeatModal != null)
            {
                m_DefeatModal.SetActive(false);
            }
        }

        public void HideVictoryModal()
        {
            if (m_VictoryModal != null)
            {
                m_VictoryModal.SetActive(false);
            }
        }

        private void BuildLevelHUD()
        {
            m_LevelHUD = new GameObject("LevelHUD");
            m_LevelHUD.transform.SetParent(m_RootCanvas, false);

            RectTransform hudRt = m_LevelHUD.AddComponent<RectTransform>();
            hudRt.anchorMin = new Vector2(0.5f, 1f);
            hudRt.anchorMax = new Vector2(0.5f, 1f);
            hudRt.pivot = new Vector2(0.5f, 1f);
            hudRt.anchoredPosition = new Vector2(0f, 0f);
            hudRt.sizeDelta = new Vector2(1080f, 484f);

            // 3D Royal Golden Crown & Sphere Blast Ribbon Header
            Image bg = m_LevelHUD.AddComponent<Image>();
            Sprite headerSprite = Resources.Load<Sprite>("Gameplay_Header_Concept1");
            bg.sprite = (headerSprite != null) ? headerSprite : SpriteFactory.GetPanelSprite(new Color(0.07f, 0.12f, 0.25f, 0.98f), new Color(0.85f, 0.70f, 0.25f, 1f), 3f, 20);
            bg.color = Color.white;
            bg.raycastTarget = false;

            bool hasHeaderArt = (headerSprite != null);

            // Interactive 3D Gear Settings Button over the header's golden gear badge
            GameObject gearBtnObj = new GameObject("HeaderSettingsBtn");
            gearBtnObj.transform.SetParent(m_LevelHUD.transform, false);
            RectTransform gearRt = gearBtnObj.AddComponent<RectTransform>();
            gearRt.anchorMin = new Vector2(0.5f, 1f);
            gearRt.anchorMax = new Vector2(0.5f, 1f);
            gearRt.pivot = new Vector2(0.5f, 0.5f);
            gearRt.anchoredPosition = new Vector2(460f, -112f);
            gearRt.sizeDelta = new Vector2(130f, 130f);
            Image gearImg = gearBtnObj.AddComponent<Image>();
            if (hasHeaderArt)
            {
                // Background artwork already features the 3D Gear Cogwheel button.
                // Keep the button transparent so only one single button is visible without ghosting duplicates.
                gearImg.color = new Color(1f, 1f, 1f, 0.001f);
            }
            else
            {
                gearImg.sprite = SpriteFactory.GetGoldenGearSettingsButtonSprite();
                gearImg.color = Color.white;
            }
            gearImg.raycastTarget = true;

            GameObject gGlowObj = new GameObject("GearGlowHighlight");
            gGlowObj.transform.SetParent(gearBtnObj.transform, false);
            RectTransform ggRt = gGlowObj.AddComponent<RectTransform>();
            ggRt.anchorMin = Vector2.zero; ggRt.anchorMax = Vector2.one; ggRt.sizeDelta = Vector2.zero;
            Image ggImg = gGlowObj.AddComponent<Image>();
            ggImg.sprite = SpriteFactory.GetSoftGlowOrbSprite();
            ggImg.color = new Color(1f, 0.85f, 0.25f, 0f);
            ggImg.raycastTarget = false;

            Button gearBtn = gearBtnObj.AddComponent<Button>();
            gearBtn.transition = Selectable.Transition.None;
            gearBtn.navigation = new Navigation { mode = Navigation.Mode.None };
            gearBtn.onClick.AddListener(() =>
            {
                if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
                HapticManager.TriggerLight();
                StartCoroutine(AnimateButtonPress(gearRt, OpenSettings));
            });

            // Interactive 3D Home Button over the header's top-left cyan crystal badge
            GameObject homeBtnObj = new GameObject("HeaderHomeBtn");
            homeBtnObj.transform.SetParent(m_LevelHUD.transform, false);
            RectTransform homeRt = homeBtnObj.AddComponent<RectTransform>();
            homeRt.anchorMin = new Vector2(0.5f, 1f);
            homeRt.anchorMax = new Vector2(0.5f, 1f);
            homeRt.pivot = new Vector2(0.5f, 0.5f);
            homeRt.anchoredPosition = new Vector2(-460f, -110f);
            homeRt.sizeDelta = new Vector2(130f, 130f);
            Image homeImg = homeBtnObj.AddComponent<Image>();
            if (hasHeaderArt)
            {
                // Background artwork already features the 3D Cyan Crystal Home button.
                // Keep the button transparent so only one single button is visible without ghosting duplicates.
                homeImg.color = new Color(1f, 1f, 1f, 0.001f);
            }
            else
            {
                homeImg.sprite = SpriteFactory.GetCyanCrystalHomeButtonSprite();
                homeImg.color = Color.white;
            }
            homeImg.raycastTarget = true;

            GameObject hGlowObj = new GameObject("HomeGlowHighlight");
            hGlowObj.transform.SetParent(homeBtnObj.transform, false);
            RectTransform hgRt = hGlowObj.AddComponent<RectTransform>();
            hgRt.anchorMin = Vector2.zero; hgRt.anchorMax = Vector2.one; hgRt.sizeDelta = Vector2.zero;
            Image hgImg = hGlowObj.AddComponent<Image>();
            hgImg.sprite = SpriteFactory.GetSoftGlowOrbSprite();
            hgImg.color = new Color(0.2f, 0.90f, 1f, 0f);
            hgImg.raycastTarget = false;

            Button homeBtn = homeBtnObj.AddComponent<Button>();
            homeBtn.transition = Selectable.Transition.None;
            homeBtn.navigation = new Navigation { mode = Navigation.Mode.None };
            homeBtn.onClick.AddListener(() =>
            {
                if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
                HapticManager.TriggerLight();
                StartCoroutine(AnimateButtonPress(homeRt, () =>
                {
                    OnHomeClicked();
                }));
            });

            Font hudFont = (m_CandyFont != null) ? m_CandyFont : ((m_HudCleanFont != null) ? m_HudCleanFont : m_DefaultFont);

            // 1. Left Plaque: Level Indicator - Centered over 3D sapphire plaque at X = -331, Y = -406
            GameObject levelBoxObj = new GameObject("HeaderLevelBox");
            levelBoxObj.transform.SetParent(m_LevelHUD.transform, false);
            RectTransform lbRt = levelBoxObj.AddComponent<RectTransform>();
            lbRt.anchorMin = new Vector2(0.5f, 1f);
            lbRt.anchorMax = new Vector2(0.5f, 1f);
            lbRt.pivot = new Vector2(0.5f, 0.5f);
            lbRt.anchoredPosition = new Vector2(-331f, -406f);
            lbRt.sizeDelta = new Vector2(250f, 75f);

            GameObject titleObj = CreateText(levelBoxObj.transform, Vector2.zero, new Vector2(240f, 55f), "LEVEL 1", 32, FontStyle.Normal, new Color(1f, 0.96f, 0.88f), TextAnchor.MiddleCenter, hudFont);
            m_LevelTitleText = titleObj.GetComponent<Text>();
            m_LevelTitleText.resizeTextForBestFit = true;
            m_LevelTitleText.resizeTextMinSize = 22;
            m_LevelTitleText.resizeTextMaxSize = 32;
            MakeTextCrisp(m_LevelTitleText, new Color(0.02f, 0.05f, 0.16f, 0.95f), 1.0f);

            // 2. Center Hero Plaque: Moves Counter - Centered over winged crest plaque at X = 0, Y = -406
            GameObject movesBoxObj = new GameObject("HeaderMovesBox");
            movesBoxObj.transform.SetParent(m_LevelHUD.transform, false);
            m_LevelMovesBoxRt = movesBoxObj.AddComponent<RectTransform>();
            m_LevelMovesBoxRt.anchorMin = new Vector2(0.5f, 1f);
            m_LevelMovesBoxRt.anchorMax = new Vector2(0.5f, 1f);
            m_LevelMovesBoxRt.pivot = new Vector2(0.5f, 0.5f);
            m_LevelMovesBoxRt.anchoredPosition = new Vector2(0f, -406f);
            m_LevelMovesBoxRt.sizeDelta = new Vector2(260f, 75f);

            GameObject movesObj = CreateText(movesBoxObj.transform, Vector2.zero, new Vector2(250f, 55f), "<color=#9FE8FF>MOVES:</color> <color=#FFDE38>40</color>", 34, FontStyle.Normal, Color.white, TextAnchor.MiddleCenter, hudFont);
            m_LevelMovesText = movesObj.GetComponent<Text>();
            m_LevelMovesText.resizeTextForBestFit = true;
            m_LevelMovesText.resizeTextMinSize = 24;
            m_LevelMovesText.resizeTextMaxSize = 34;
            MakeTextCrisp(m_LevelMovesText, new Color(0.01f, 0.04f, 0.18f, 0.95f), 1.0f);

            // 3. Right Plaque: Score with Star Icon - Centered over 3D gold plaque at X = 328, Y = -406
            GameObject scoreBoxObj = new GameObject("HeaderScoreBox");
            scoreBoxObj.transform.SetParent(m_LevelHUD.transform, false);
            RectTransform sbRt = scoreBoxObj.AddComponent<RectTransform>();
            sbRt.anchorMin = new Vector2(0.5f, 1f);
            sbRt.anchorMax = new Vector2(0.5f, 1f);
            sbRt.pivot = new Vector2(0.5f, 0.5f);
            sbRt.anchoredPosition = new Vector2(328f, -406f);
            sbRt.sizeDelta = new Vector2(250f, 75f);

            int score = (GameManager.Instance != null) ? GameManager.Instance.CurrentScore : 0;
            GameObject lScoreObj = CreateText(scoreBoxObj.transform, new Vector2(-18f, 0f), new Vector2(180f, 55f), $"<color=#FFECA8>SCORE:</color> {score:N0}", 30, FontStyle.Normal, Color.white, TextAnchor.MiddleCenter, hudFont);
            m_LevelScoreText = lScoreObj.GetComponent<Text>();
            m_LevelScoreText.resizeTextForBestFit = true;
            m_LevelScoreText.resizeTextMinSize = 20;
            m_LevelScoreText.resizeTextMaxSize = 30;
            MakeTextCrisp(m_LevelScoreText, new Color(0.40f, 0.20f, 0.02f, 0.95f), 1.0f);

            // 3D Shiny Golden Star Icon inside Score Plaque
            GameObject starObj = new GameObject("ScoreStarIcon");
            starObj.transform.SetParent(scoreBoxObj.transform, false);
            RectTransform starRt = starObj.AddComponent<RectTransform>();
            starRt.anchorMin = new Vector2(1f, 0.5f);
            starRt.anchorMax = new Vector2(1f, 0.5f);
            starRt.pivot = new Vector2(1f, 0.5f);
            starRt.anchoredPosition = new Vector2(-12f, 0f);
            starRt.sizeDelta = new Vector2(34f, 34f);
            Image starImg = starObj.AddComponent<Image>();
            Sprite starSprite = Resources.Load<Sprite>("Gem_3D_Star");
            starImg.sprite = (starSprite != null) ? starSprite : GemIconFactory.GetStarSprite(true);
            starImg.raycastTarget = false;

            // 4. Floating 3D Ornate Objectives Capsule Bar - Positioned neatly with comfortable spacing
            m_GemObjectivesBar = new GameObject("GemObjectivesBar");
            m_GemObjectivesBar.transform.SetParent(m_LevelHUD.transform, false);
            RectTransform gobRt = m_GemObjectivesBar.AddComponent<RectTransform>();
            gobRt.anchorMin = new Vector2(0.5f, 1f);
            gobRt.anchorMax = new Vector2(0.5f, 1f);
            gobRt.pivot = new Vector2(0.5f, 0.5f);
            gobRt.anchoredPosition = new Vector2(0f, -535f);
            gobRt.sizeDelta = new Vector2(880f, 118f);
            Image gobBg = m_GemObjectivesBar.AddComponent<Image>();
            Sprite objCapsule = Resources.Load<Sprite>("Gameplay_Objectives_Concept1");
            gobBg.sprite = (objCapsule != null) ? objCapsule : SpriteFactory.GetPillBadgeSprite(new Color(0.04f, 0.08f, 0.18f, 0.70f));
            gobBg.color = Color.white;
            gobBg.raycastTarget = false;

            // Alternative Line/Star Goal Badge (for non-gem levels)
            GameObject goalBadge = new GameObject("GoalBadge");
            goalBadge.transform.SetParent(m_GemObjectivesBar.transform, false);
            RectTransform gRt = goalBadge.AddComponent<RectTransform>();
            gRt.anchorMin = new Vector2(0.5f, 0.5f);
            gRt.anchorMax = new Vector2(0.5f, 0.5f);
            gRt.pivot = new Vector2(0.5f, 0.5f);
            gRt.anchoredPosition = Vector2.zero;
            gRt.sizeDelta = new Vector2(560f, 90f);
            Image gBg = goalBadge.AddComponent<Image>();
            gBg.color = new Color(1f, 1f, 1f, 0.001f);
            gBg.raycastTarget = false;

            GameObject goalObj = CreateText(goalBadge.transform, Vector2.zero, new Vector2(540f, 75f), "Lines: 0/3", 34, FontStyle.Normal, new Color(0.40f, 0.98f, 0.70f), TextAnchor.MiddleCenter, hudFont);
            m_LevelGoalText = goalObj.GetComponent<Text>();
            m_LevelGoalText.resizeTextForBestFit = true;
            m_LevelGoalText.resizeTextMinSize = 22;
            m_LevelGoalText.resizeTextMaxSize = 34;
            m_LevelGoalBadgeObj = goalBadge;

            // 4 Dynamic Gem Slots inside the Capsule Bar
            m_GemObjectiveItems.Clear();
            GemType[] hudGems = new GemType[] { GemType.BlueDiamond, GemType.OrangePentagon, GemType.YellowStar, GemType.RedRuby };
            float[] offsets = new float[] { -270f, -90f, 90f, 270f };

            for (int i = 0; i < hudGems.Length; i++)
            {
                GemType gType = hudGems[i];
                GameObject itemObj = new GameObject($"GemSlot_{gType}");
                itemObj.transform.SetParent(m_GemObjectivesBar.transform, false);
                RectTransform itemRt = itemObj.AddComponent<RectTransform>();
                itemRt.anchorMin = new Vector2(0.5f, 0.5f);
                itemRt.anchorMax = new Vector2(0.5f, 0.5f);
                itemRt.pivot = new Vector2(0.5f, 0.5f);
                itemRt.anchoredPosition = new Vector2(offsets[i], 0f);
                itemRt.sizeDelta = new Vector2(170f, 95f);

                // 3D Gem Icon
                GameObject iconObj = new GameObject("Icon");
                iconObj.transform.SetParent(itemObj.transform, false);
                RectTransform iconRt = iconObj.AddComponent<RectTransform>();
                iconRt.anchorMin = new Vector2(0.5f, 0.5f);
                iconRt.anchorMax = new Vector2(0.5f, 0.5f);
                iconRt.pivot = new Vector2(0.5f, 0.5f);
                iconRt.anchoredPosition = new Vector2(-36f, 0f);
                iconRt.sizeDelta = new Vector2(66f, 66f);
                Image iconImg = iconObj.AddComponent<Image>();
                iconImg.sprite = GemIconFactory.GetGemSprite(gType);
                iconImg.raycastTarget = false;

                // Crisp count text matching Concept 1 ("x15", "x10", etc.)
                GameObject countObj = CreateText(itemObj.transform, new Vector2(34f, 0f), new Vector2(90f, 60f), "x0", 42, FontStyle.Normal, Color.white, TextAnchor.MiddleLeft, hudFont);
                Text countTxt = countObj.GetComponent<Text>();
                countTxt.resizeTextForBestFit = true;
                countTxt.resizeTextMinSize = 28;
                countTxt.resizeTextMaxSize = 42;
                MakeTextCrisp(countTxt, new Color(0.01f, 0.05f, 0.18f, 0.95f), 1.0f);

                // 3D Emerald Green Checkmark Badge for completed objective
                GameObject checkObj = new GameObject("CheckmarkBadge");
                checkObj.transform.SetParent(itemObj.transform, false);
                RectTransform checkRt = checkObj.AddComponent<RectTransform>();
                checkRt.anchorMin = new Vector2(0.5f, 0.5f);
                checkRt.anchorMax = new Vector2(0.5f, 0.5f);
                checkRt.pivot = new Vector2(0.5f, 0.5f);
                checkRt.anchoredPosition = new Vector2(32f, 0f);
                checkRt.sizeDelta = new Vector2(44f, 44f);
                Image checkImg = checkObj.AddComponent<Image>();
                checkImg.sprite = SpriteFactory.GetGreenCheckmarkBadgeSprite();
                checkImg.raycastTarget = false;
                checkObj.SetActive(false);

                m_GemObjectiveItems[gType] = (itemObj, iconImg, countTxt, iconRt, checkImg);
            }
            m_GemObjectivesBar.SetActive(false);

            // Ensure interactive header buttons sit at the top of the hierarchy inside m_LevelHUD
            homeBtnObj.transform.SetAsLastSibling();
            gearBtnObj.transform.SetAsLastSibling();

            m_LevelHUD.SetActive(false);
        }

        private void BuildModeSelectScreen()
        {
            m_ModeSelectPanel = new GameObject("ModeSelectPanel");
            m_ModeSelectPanel.transform.SetParent(m_RootCanvas, false);
            RectTransform rt = m_ModeSelectPanel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.sizeDelta = Vector2.zero;

            // Fullscreen cosmic space backdrop - fills entire screen (0 black bars on any phone or desktop)
            Image panelBg = m_ModeSelectPanel.AddComponent<Image>();
            panelBg.sprite = null;
            panelBg.color = new Color(0.015f, 0.048f, 0.24f, 1f); // Seamless deep space cosmic navy matching artwork edges exactly
            panelBg.type = Image.Type.Simple;
            panelBg.raycastTarget = true;

            // Centered Content Card strictly locked to 1152 x 2064 (0.55814 aspect ratio)
            // This mathematically guarantees all 8 element spheres, the central star halo,
            // and the golden cosmic ring remain 100% PERFECT CIRCLES on any screen!
            GameObject cardObj = new GameObject("HomeContentCard");
            cardObj.transform.SetParent(m_ModeSelectPanel.transform, false);
            RectTransform cardRt = cardObj.AddComponent<RectTransform>();
            cardRt.anchorMin = Vector2.zero;
            cardRt.anchorMax = Vector2.one;
            cardRt.sizeDelta = Vector2.zero;

            AspectRatioFitter arf = cardObj.AddComponent<AspectRatioFitter>();
            arf.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            arf.aspectRatio = 1152f / 2064f; // 0.55814f exact master artwork aspect ratio

            // Concept 3 Master 3D Artwork (Ball Blast crown, 8 element spheres, rainbow star, 3D Adventure & Classic buttons)
            Image bg = cardObj.AddComponent<Image>();
            bg.sprite = SpriteFactory.GetHomeBackgroundConcept3Sprite();
            bg.color = Color.white;
            bg.preserveAspect = true;
            bg.raycastTarget = false;

            // 1. Interactive Button: ADVENTURE (Directly over the painted 3D Golden Adventure button)
            GameObject advBtnObj = new GameObject("Button_Adventure");
            advBtnObj.transform.SetParent(cardObj.transform, false);
            RectTransform advRt = advBtnObj.AddComponent<RectTransform>();
            advRt.anchorMin = new Vector2(0.140f, 0.155f);
            advRt.anchorMax = new Vector2(0.860f, 0.255f);
            advRt.offsetMin = Vector2.zero;
            advRt.offsetMax = Vector2.zero;

            Image advImg = advBtnObj.AddComponent<Image>();
            advImg.color = Color.clear;
            advImg.raycastTarget = true;

            // Subtle golden glow flash on tap
            GameObject advGlowObj = new GameObject("AdventureGlowHighlight");
            advGlowObj.transform.SetParent(advBtnObj.transform, false);
            RectTransform agRt = advGlowObj.AddComponent<RectTransform>();
            agRt.anchorMin = Vector2.zero; agRt.anchorMax = Vector2.one; agRt.sizeDelta = Vector2.zero;
            Image agImg = advGlowObj.AddComponent<Image>();
            agImg.sprite = SpriteFactory.GetPillSprite(Color.white);
            agImg.color = new Color(1.0f, 0.88f, 0.30f, 0f);
            agImg.raycastTarget = false;

            Button advBtn = advBtnObj.AddComponent<Button>();
            advBtn.transition = Selectable.Transition.None;
            advBtn.onClick.AddListener(() =>
            {
                if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
                HapticManager.TriggerLight();
                StartCoroutine(AnimateButtonFlash(agImg, () =>
                {
                    m_ModeSelectPanel.SetActive(false);
                    OpenLevelSelect(GameModeType.Adventure);
                }));
            });

            // 2. Interactive Button: CLASSIC MODE (Directly over the painted 3D Royal Sapphire button)
            GameObject classicBtnObj = new GameObject("Button_Classic");
            classicBtnObj.transform.SetParent(cardObj.transform, false);
            RectTransform clRt = classicBtnObj.AddComponent<RectTransform>();
            clRt.anchorMin = new Vector2(0.140f, 0.055f);
            clRt.anchorMax = new Vector2(0.760f, 0.145f);
            clRt.offsetMin = Vector2.zero;
            clRt.offsetMax = Vector2.zero;

            Image clImg = classicBtnObj.AddComponent<Image>();
            clImg.color = Color.clear;
            clImg.raycastTarget = true;

            // Royal Sapphire Glow Highlight on tap
            GameObject clGlowObj = new GameObject("ClassicGlowHighlight");
            clGlowObj.transform.SetParent(classicBtnObj.transform, false);
            RectTransform cgRt = clGlowObj.AddComponent<RectTransform>();
            cgRt.anchorMin = Vector2.zero; cgRt.anchorMax = Vector2.one; cgRt.sizeDelta = Vector2.zero;
            Image cgImg = clGlowObj.AddComponent<Image>();
            cgImg.sprite = SpriteFactory.GetPillSprite(Color.white);
            cgImg.color = new Color(0.30f, 0.65f, 1.0f, 0f);
            cgImg.raycastTarget = false;

            Button clBtn = classicBtnObj.AddComponent<Button>();
            clBtn.transition = Selectable.Transition.None;
            clBtn.onClick.AddListener(() =>
            {
                if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
                HapticManager.TriggerLight();
                StartCoroutine(AnimateButtonFlash(cgImg, () =>
                {
                    m_ModeSelectPanel.SetActive(false);
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.StartClassicGame();
                    }
                    UpdateHUDVisibility();
                }));
            });

            // 3. Top-Right: Settings Gear Hitbox (Directly over top-right golden gear socket)
            GameObject settingsBtnObj = new GameObject("Button_Settings");
            settingsBtnObj.transform.SetParent(cardObj.transform, false);
            RectTransform stRt = settingsBtnObj.AddComponent<RectTransform>();
            stRt.anchorMin = new Vector2(0.790f, 0.875f);
            stRt.anchorMax = new Vector2(0.985f, 0.975f);
            stRt.offsetMin = Vector2.zero;
            stRt.offsetMax = Vector2.zero;

            Image stImg = settingsBtnObj.AddComponent<Image>();
            stImg.color = Color.clear;
            stImg.raycastTarget = true;

            GameObject stGlowObj = new GameObject("SettingsGlowHighlight");
            stGlowObj.transform.SetParent(settingsBtnObj.transform, false);
            RectTransform sgRt = stGlowObj.AddComponent<RectTransform>();
            sgRt.anchorMin = Vector2.zero; sgRt.anchorMax = Vector2.one; sgRt.sizeDelta = Vector2.zero;
            Image sgImg = stGlowObj.AddComponent<Image>();
            sgImg.sprite = SpriteFactory.GetSoftGlowOrbSprite();
            sgImg.color = new Color(1f, 1f, 1f, 0f);
            sgImg.raycastTarget = false;

            Button stBtn = settingsBtnObj.AddComponent<Button>();
            stBtn.transition = Selectable.Transition.None;
            stBtn.onClick.AddListener(() =>
            {
                if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
                HapticManager.TriggerLight();
                StartCoroutine(AnimateButtonFlash(sgImg, OpenSettings));
            });
            m_HomeSettingsBtn = settingsBtnObj;

            // 4. Top-Left: Wardrobe Crystal Socket (Directly over top-left blue crystal orb)
            GameObject wardrobeBtnObj = new GameObject("Button_Wardrobe");
            wardrobeBtnObj.transform.SetParent(cardObj.transform, false);
            RectTransform wdRt = wardrobeBtnObj.AddComponent<RectTransform>();
            wdRt.anchorMin = new Vector2(0.015f, 0.875f);
            wdRt.anchorMax = new Vector2(0.205f, 0.975f);
            wdRt.offsetMin = Vector2.zero;
            wdRt.offsetMax = Vector2.zero;
            m_HomeBallDesignsPillRt = wdRt;

            Image wdImg = wardrobeBtnObj.AddComponent<Image>();
            wdImg.color = Color.clear;
            wdImg.raycastTarget = true;

            GameObject wdGlowObj = new GameObject("WardrobeGlowHighlight");
            wdGlowObj.transform.SetParent(wardrobeBtnObj.transform, false);
            RectTransform wgRt = wdGlowObj.AddComponent<RectTransform>();
            wgRt.anchorMin = Vector2.zero; wgRt.anchorMax = Vector2.one; wgRt.sizeDelta = Vector2.zero;
            Image wgImg = wdGlowObj.AddComponent<Image>();
            wgImg.sprite = SpriteFactory.GetSoftGlowOrbSprite();
            wgImg.color = new Color(0.40f, 0.80f, 1f, 0f);
            wgImg.raycastTarget = false;

            Button wdBtn = wardrobeBtnObj.AddComponent<Button>();
            wdBtn.transition = Selectable.Transition.None;
            wdBtn.onClick.AddListener(() =>
            {
                if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
                HapticManager.TriggerLight();
                StartCoroutine(AnimateButtonFlash(wgImg, OpenSkinWardrobe));
            });
            m_HomeWardrobeBtn = wardrobeBtnObj;

            // 5. Crown Peak Diamond Twinkle Star (Living rotating sparkle on crown peak)
            GameObject crownStarGlint = new GameObject("CrownStarGlint");
            crownStarGlint.transform.SetParent(cardObj.transform, false);
            RectTransform csgRt = crownStarGlint.AddComponent<RectTransform>();
            csgRt.anchorMin = new Vector2(0.50f, 0.980f);
            csgRt.anchorMax = new Vector2(0.50f, 0.980f);
            csgRt.anchoredPosition = Vector2.zero;
            csgRt.sizeDelta = new Vector2(36, 36);
            Image csgImg = crownStarGlint.AddComponent<Image>();
            csgImg.sprite = GemIconFactory.GetStarSprite(true);
            csgImg.color = new Color(1f, 1f, 1f, 0.95f);
            csgImg.raycastTarget = false;

            // Interactive Golden Crown Tap Easter Egg
            GameObject crownTapObj = new GameObject("Crown_InteractiveZone");
            crownTapObj.transform.SetParent(cardObj.transform, false);
            RectTransform crRt = crownTapObj.AddComponent<RectTransform>();
            crRt.anchorMin = new Vector2(0.34f, 0.875f);
            crRt.anchorMax = new Vector2(0.66f, 0.985f);
            crRt.offsetMin = Vector2.zero;
            crRt.offsetMax = Vector2.zero;
            Image crImg = crownTapObj.AddComponent<Image>();
            crImg.color = Color.clear;
            crImg.raycastTarget = true;
            Button crBtn = crownTapObj.AddComponent<Button>();
            crBtn.transition = Selectable.Transition.None;
            crBtn.onClick.AddListener(() =>
            {
                if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
                HapticManager.TriggerLight();
                StartCoroutine(AnimateCrownSparkle(csgRt));
            });

            // 6. Interactive Treasure Chest Easter Egg (Directly over the 3D glowing gold chest at bottom right)
            GameObject chestBtnObj = new GameObject("Button_TreasureChest");
            chestBtnObj.transform.SetParent(cardObj.transform, false);
            RectTransform chRt = chestBtnObj.AddComponent<RectTransform>();
            chRt.anchorMin = new Vector2(0.74f, 0.025f);
            chRt.anchorMax = new Vector2(0.99f, 0.170f);
            chRt.offsetMin = Vector2.zero;
            chRt.offsetMax = Vector2.zero;
            Image chImg = chestBtnObj.AddComponent<Image>();
            chImg.color = Color.clear;
            chImg.raycastTarget = true;
            Button chBtn = chestBtnObj.AddComponent<Button>();
            chBtn.transition = Selectable.Transition.None;
            chBtn.onClick.AddListener(() =>
            {
                if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
                HapticManager.TriggerMedium();
                FloatingText.Create(Vector3.zero, "TREASURE VAULT!", Color.yellow);
            });

            // 7. Living Idle Animation: Twinkling crown jewel, breathing buttons
            HomeButtonBreather breather = m_ModeSelectPanel.AddComponent<HomeButtonBreather>();
            breather.crownStarGlintRt = csgRt;
            breather.adventureBtnRt = advRt;
            breather.classicBtnRt = clRt;
            breather.ballDesignsPillRt = wdRt;

            SkinManager.OnSkinChanged -= HandleSkinChangedForHome;
            SkinManager.OnSkinChanged += HandleSkinChangedForHome;
            UpdateHomeEquippedBallPreview();

            m_ModeSelectPanel.SetActive(false);
        }

        private List<BokehDriftData> BuildHomeBackgroundBokeh(Transform parent, float refHeight)
        {
            var list = new List<BokehDriftData>();
            GameObject bokehContainer = new GameObject("BackgroundBokehSpheres");
            bokehContainer.transform.SetParent(parent, false);
            RectTransform bcRt = bokehContainer.AddComponent<RectTransform>();
            bcRt.anchorMin = Vector2.zero; bcRt.anchorMax = Vector2.one; bcRt.sizeDelta = Vector2.zero;

            // 10 cinematic floating bokeh candy spheres at varying depths and positions (matching Concept 3)
            var orbConfigs = new (Vector2 pos, float size, Color tint, float speed, float ampX, float ampY, float phase)[]
            {
                // Left Side
                (new Vector2(-380f, 480f), 76f, new Color(0.95f, 0.22f, 0.35f), 1.2f, 8f, 14f, 0f),       // Ruby
                (new Vector2(-420f, 160f), 54f, new Color(0.20f, 0.75f, 0.95f), 1.5f, 10f, 16f, 1.2f),     // Cyan
                (new Vector2(-360f, -120f), 64f, new Color(0.98f, 0.35f, 0.20f), 1.1f, 6f, 12f, 2.5f),    // Orange
                (new Vector2(-410f, -420f), 78f, new Color(0.15f, 0.55f, 0.95f), 1.3f, 9f, 15f, 3.8f),    // Blue
                (new Vector2(-280f, -620f), 50f, new Color(0.68f, 0.25f, 0.88f), 1.6f, 7f, 13f, 4.2f),    // Amethyst

                // Right Side
                (new Vector2(390f, 520f), 72f, new Color(0.98f, 0.55f, 0.12f), 1.3f, 8f, 15f, 0.8f),      // Amber
                (new Vector2(420f, 220f), 58f, new Color(0.95f, 0.32f, 0.45f), 1.4f, 9f, 14f, 2.1f),      // Coral
                (new Vector2(370f, -80f), 74f, new Color(0.20f, 0.85f, 0.98f), 1.0f, 7f, 12f, 3.4f),      // Cyan
                (new Vector2(400f, -380f), 56f, new Color(0.92f, 0.30f, 0.75f), 1.5f, 10f, 16f, 4.6f),    // Hot Pink
                (new Vector2(320f, -640f), 82f, new Color(0.95f, 0.45f, 0.22f), 1.2f, 8f, 13f, 5.2f),     // Orange-red
            };

            for (int i = 0; i < orbConfigs.Length; i++)
            {
                var c = orbConfigs[i];
                GameObject orbObj = new GameObject($"BokehOrb_{i}");
                orbObj.transform.SetParent(bokehContainer.transform, false);
                RectTransform oRt = orbObj.AddComponent<RectTransform>();
                oRt.anchoredPosition = c.pos;
                oRt.sizeDelta = new Vector2(c.size, c.size);
                Image oImg = orbObj.AddComponent<Image>();
                oImg.sprite = SpriteFactory.GetSoftBokehSphereSprite(c.tint);
                oImg.raycastTarget = false;

                list.Add(new BokehDriftData
                {
                    rt = oRt,
                    basePos = c.pos,
                    speed = c.speed,
                    ampX = c.ampX,
                    ampY = c.ampY,
                    phase = c.phase
                });
            }

            return list;
        }

        private void BuildLevelSelectScreen()
        {
            if (m_LevelSelectPanel != null)
            {
                Destroy(m_LevelSelectPanel);
                m_LevelSelectPanel = null;
            }

            m_LevelSelectPanel = new GameObject("LevelSelectPanel");
            m_LevelSelectPanel.transform.SetParent(m_RootCanvas, false);
            RectTransform rt = m_LevelSelectPanel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.sizeDelta = Vector2.zero;

            // 1. Cosmic Navy Backdrop outside card
            Image mapBg = m_LevelSelectPanel.AddComponent<Image>();
            Sprite spaceBg = SpriteFactory.LoadOrFallbackSprite("HomeBackground_Concept3");
            if (spaceBg != null)
            {
                mapBg.sprite = spaceBg;
                mapBg.color = Color.white;
            }
            else
            {
                mapBg.color = new Color(0.04f, 0.07f, 0.16f, 1f);
            }

            float canvasHeight = 1920f;
            float canvasWidth = 1080f;
            if (m_RootCanvas != null)
            {
                RectTransform canvasRt = m_RootCanvas.GetComponent<RectTransform>();
                if (canvasRt != null && canvasRt.rect.height > 0) canvasHeight = canvasRt.rect.height;
                if (canvasRt != null && canvasRt.rect.width > 0) canvasWidth = canvasRt.rect.width;
            }

            // Safe Area calculations for notch / status bar
            float safeTop = 0f;
            float safeLeft = 0f;
            if (Screen.height > 0 && Screen.width > 0)
            {
                Rect safeArea = Screen.safeArea;
                float topPixels = Screen.height - (safeArea.y + safeArea.height);
                float leftPixels = safeArea.x;
                safeTop = (topPixels / Screen.height) * canvasHeight;
                safeLeft = (leftPixels / Screen.width) * 1080f;
            }

            // 2. Main Adventure Scene Card (768 x 1376 native aspect ratio)
            GameObject sceneCard = new GameObject("AdventureSceneCard");
            sceneCard.transform.SetParent(m_LevelSelectPanel.transform, false);
            RectTransform scRt = sceneCard.AddComponent<RectTransform>();
            scRt.anchorMin = new Vector2(0.5f, 0.5f);
            scRt.anchorMax = new Vector2(0.5f, 0.5f);
            scRt.pivot = new Vector2(0.5f, 0.5f);
            scRt.anchoredPosition = Vector2.zero;
            scRt.sizeDelta = new Vector2(768f, 1376f);

            float fitScale = canvasHeight / 1376f;
            if (768f * fitScale > canvasWidth)
            {
                fitScale = canvasWidth / 768f;
            }
            scRt.localScale = new Vector3(fitScale, fitScale, 1f);

            // High-Definition Option 1 Master Artwork (Frame + Crown + Trail + Stardust + Plaque)
            Image scImg = sceneCard.AddComponent<Image>();
            Sprite mapSprite = SpriteFactory.LoadOrFallbackSprite("AdventureMap_Frame");
            if (mapSprite == null) mapSprite = SpriteFactory.LoadOrFallbackSprite("AdventureMap_Option1");
            if (mapSprite == null) mapSprite = SpriteFactory.LoadOrFallbackSprite("AdventureMap_Concept2");
            if (mapSprite != null)
            {
                scImg.sprite = mapSprite;
                scImg.color = Color.white;
            }
            else
            {
                scImg.color = new Color(0.04f, 0.07f, 0.16f, 1f);
            }
            scImg.raycastTarget = true;

            // 3. Continuous Adventure Scroll View (Vertical Viewport nestled between top & bottom plaques)
            GameObject scrollViewObj = new GameObject("AdventureScrollView");
            scrollViewObj.transform.SetParent(sceneCard.transform, false);
            RectTransform svRt = scrollViewObj.AddComponent<RectTransform>();
            svRt.anchorMin = new Vector2(0.5f, 0.5f);
            svRt.anchorMax = new Vector2(0.5f, 0.5f);
            svRt.pivot = new Vector2(0.5f, 0.5f);
            svRt.anchoredPosition = new Vector2(0f, -70f);
            svRt.sizeDelta = new Vector2(590f, 800f);

            RectMask2D mask = scrollViewObj.AddComponent<RectMask2D>();
            mask.softness = new Vector2Int(0, 24);

            ScrollRect scrollRect = scrollViewObj.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.elasticity = 0.10f;
            scrollRect.inertia = true;
            scrollRect.decelerationRate = 0.135f;
            scrollRect.scrollSensitivity = 28f;
            m_LevelScrollRect = scrollRect;

            // 3b. Scroll Content: Continuous 40-level winding highway ribbon
            const float totalContentH = 3850f;
            GameObject contentObj = new GameObject("ScrollContent");
            contentObj.transform.SetParent(scrollViewObj.transform, false);
            RectTransform cRt = contentObj.AddComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0.5f, 0f);
            cRt.anchorMax = new Vector2(0.5f, 0f);
            cRt.pivot = new Vector2(0.5f, 0f);
            cRt.anchoredPosition = Vector2.zero;
            cRt.sizeDelta = new Vector2(590f, totalContentH);
            scrollRect.content = cRt;

            // 3c. Procedural 3D Golden Road Graphic
            GameObject roadObj = new GameObject("GalaxyRoadRibbon");
            roadObj.transform.SetParent(contentObj.transform, false);
            RectTransform roadRt = roadObj.AddComponent<RectTransform>();
            roadRt.anchorMin = new Vector2(0.5f, 0f);
            roadRt.anchorMax = new Vector2(0.5f, 0f);
            roadRt.pivot = new Vector2(0.5f, 0f);
            roadRt.anchoredPosition = Vector2.zero;
            roadRt.sizeDelta = new Vector2(590f, totalContentH);

            m_GalaxyRoad = roadObj.AddComponent<GalaxyRoadGraphic>();
            m_GalaxyRoad.Setup(40, 92f, 130f, 175f, Mathf.PI / 4.0f, Mathf.PI / 2.0f);

            // 3d. Node Container for Level Buttons & Milestone Chests
            GameObject nodeContainer = new GameObject("NodeContainer");
            nodeContainer.transform.SetParent(contentObj.transform, false);
            RectTransform ncRt = nodeContainer.AddComponent<RectTransform>();
            ncRt.anchorMin = new Vector2(0.5f, 0f);
            ncRt.anchorMax = new Vector2(0.5f, 0f);
            ncRt.pivot = new Vector2(0.5f, 0f);
            ncRt.anchoredPosition = Vector2.zero;
            ncRt.sizeDelta = new Vector2(590f, totalContentH);
            m_LevelGridContainer = nodeContainer.transform;

            // 4. Interactive Ruby Red Close 'X' Button (Upper Right on frame at (305, 397))
            GameObject closeBtnObj = new GameObject("RubyCloseButton");
            closeBtnObj.transform.SetParent(sceneCard.transform, false);
            RectTransform cbRt = closeBtnObj.AddComponent<RectTransform>();
            cbRt.anchorMin = new Vector2(0.5f, 0.5f);
            cbRt.anchorMax = new Vector2(0.5f, 0.5f);
            cbRt.pivot = new Vector2(0.5f, 0.5f);
            cbRt.anchoredPosition = new Vector2(305f, 397f);
            cbRt.sizeDelta = new Vector2(92f, 92f);

            Image cbImg = closeBtnObj.AddComponent<Image>();
            cbImg.color = new Color(1f, 1f, 1f, 0.001f);
            cbImg.raycastTarget = true;

            Button cbBtn = closeBtnObj.AddComponent<Button>();
            cbBtn.targetGraphic = cbImg;
            cbBtn.onClick.AddListener(() =>
            {
                if (m_IsLevelSelectTransitioning) return;
                m_IsLevelSelectTransitioning = true;
                if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
                HapticManager.TriggerLight();
                StartCoroutine(AnimateButtonPress(cbRt, () =>
                {
                    if (m_LevelSelectPanel != null) m_LevelSelectPanel.SetActive(false);
                    OpenModeSelect();
                    UpdateHUDVisibility();
                    m_IsLevelSelectTransitioning = false;
                }));
            });

            // 5. Dynamic Footer Text on the Bottom Gold Plaque (Centered at Y = -532f)
            GameObject footerObj = new GameObject("AdventureFooterText");
            footerObj.transform.SetParent(sceneCard.transform, false);
            RectTransform ftRt = footerObj.AddComponent<RectTransform>();
            ftRt.anchorMin = new Vector2(0.5f, 0.5f);
            ftRt.anchorMax = new Vector2(0.5f, 0.5f);
            ftRt.pivot = new Vector2(0.5f, 0.5f);
            ftRt.anchoredPosition = new Vector2(0f, -518f);
            ftRt.sizeDelta = new Vector2(520f, 40f);

            Text ftText = footerObj.AddComponent<Text>();
            ftText.font = (m_CandyFont != null) ? m_CandyFont : m_DefaultFont;
            ftText.fontSize = 24;
            ftText.alignment = TextAnchor.MiddleCenter;
            ftText.color = new Color(1.0f, 0.94f, 0.70f, 1f);
            MakeTextCrisp(ftText, new Color(0.28f, 0.12f, 0.02f, 0.95f), 1.2f);
            m_AdventureFooterText = ftText;

            // 6. Cyan Crystal Home Button (Safe Area Top-Left)
            GameObject backBtn = new GameObject("BackButton");
            backBtn.transform.SetParent(m_LevelSelectPanel.transform, false);
            RectTransform bbRt = backBtn.AddComponent<RectTransform>();
            bbRt.anchorMin = new Vector2(0f, 1f);
            bbRt.anchorMax = new Vector2(0f, 1f);
            bbRt.pivot = new Vector2(0f, 1f);
            float homeLeft = Mathf.Max(safeLeft + 35f, 36f);
            float homeTop = Mathf.Max(safeTop + 35f, 36f);
            bbRt.anchoredPosition = new Vector2(homeLeft, -homeTop);
            bbRt.sizeDelta = new Vector2(96f, 96f);

            Image bbImg = backBtn.AddComponent<Image>();
            bbImg.sprite = SpriteFactory.GetCyanCrystalHomeButtonSprite();
            bbImg.color = Color.white;
            bbImg.raycastTarget = true;

            Button bbComp = backBtn.AddComponent<Button>();
            bbComp.targetGraphic = bbImg;
            bbComp.onClick.AddListener(() =>
            {
                if (m_IsLevelSelectTransitioning) return;
                m_IsLevelSelectTransitioning = true;
                if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
                HapticManager.TriggerLight();
                StartCoroutine(AnimateButtonPress(bbRt, () =>
                {
                    if (m_LevelSelectPanel != null) m_LevelSelectPanel.SetActive(false);
                    OpenModeSelect();
                    UpdateHUDVisibility();
                    m_IsLevelSelectTransitioning = false;
                }));
            });

            // 7. Floating Side Chapter Buttons for multi-chapter navigation
            float chBtnY = -532f;
            // Prev Chapter Button (< CH 1)
            GameObject prevSideObj = new GameObject("PrevChapterSideButton");
            prevSideObj.transform.SetParent(sceneCard.transform, false);
            m_PrevChapterSideRt = prevSideObj.AddComponent<RectTransform>();
            m_PrevChapterSideRt.anchorMin = new Vector2(0.5f, 0.5f);
            m_PrevChapterSideRt.anchorMax = new Vector2(0.5f, 0.5f);
            m_PrevChapterSideRt.pivot = new Vector2(0.5f, 0.5f);
            m_PrevChapterSideRt.anchoredPosition = new Vector2(-310f, chBtnY);
            m_PrevChapterSideRt.sizeDelta = new Vector2(96f, 52f);

            Image prevSideImg = prevSideObj.AddComponent<Image>();
            prevSideImg.sprite = SpriteFactory.GetPillSprite(new Color(0.18f, 0.10f, 0.04f, 0.95f));
            prevSideImg.raycastTarget = true;

            Button prevSideBtn = prevSideObj.AddComponent<Button>();
            prevSideBtn.targetGraphic = prevSideImg;
            prevSideBtn.onClick.AddListener(() =>
            {
                if (m_IsLevelSelectTransitioning || m_LevelSelectPage <= 0) return;
                if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
                HapticManager.TriggerLight();
                m_LevelSelectPage--;
                RefreshLevelSelectGrid(m_ActiveViewingMode);
            });

            GameObject prevTxtObj = CreateText(prevSideObj.transform, Vector2.zero, new Vector2(96f, 52f), "< CH 1", 18, FontStyle.Bold, new Color(1.0f, 0.90f, 0.40f, 1f), TextAnchor.MiddleCenter, m_CandyFont);
            m_PrevChapterSideText = prevTxtObj.GetComponent<Text>();
            MakeTextCrisp(m_PrevChapterSideText, new Color(0.12f, 0.06f, 0.02f, 0.95f), 1f);
            prevSideObj.SetActive(false);

            // Next Chapter Button (CH 2 >)
            GameObject nextSideObj = new GameObject("NextChapterSideButton");
            nextSideObj.transform.SetParent(sceneCard.transform, false);
            m_NextChapterSideRt = nextSideObj.AddComponent<RectTransform>();
            m_NextChapterSideRt.anchorMin = new Vector2(0.5f, 0.5f);
            m_NextChapterSideRt.anchorMax = new Vector2(0.5f, 0.5f);
            m_NextChapterSideRt.pivot = new Vector2(0.5f, 0.5f);
            m_NextChapterSideRt.anchoredPosition = new Vector2(310f, chBtnY);
            m_NextChapterSideRt.sizeDelta = new Vector2(96f, 52f);

            Image nextSideImg = nextSideObj.AddComponent<Image>();
            nextSideImg.sprite = SpriteFactory.GetPillSprite(new Color(0.18f, 0.10f, 0.04f, 0.95f));
            nextSideImg.raycastTarget = true;

            Button nextSideBtn = nextSideObj.AddComponent<Button>();
            nextSideBtn.targetGraphic = nextSideImg;
            nextSideBtn.onClick.AddListener(() =>
            {
                int maxPage = (LevelManager.GetMaxLevel(m_ActiveViewingMode) - 1) / 20;
                if (m_IsLevelSelectTransitioning || m_LevelSelectPage >= maxPage) return;
                if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
                HapticManager.TriggerLight();
                m_LevelSelectPage++;
                RefreshLevelSelectGrid(m_ActiveViewingMode);
            });

            GameObject nextTxtObj = CreateText(nextSideObj.transform, Vector2.zero, new Vector2(96f, 52f), "CH 2 >", 18, FontStyle.Bold, new Color(1.0f, 0.90f, 0.40f, 1f), TextAnchor.MiddleCenter, m_CandyFont);
            m_NextChapterSideText = nextTxtObj.GetComponent<Text>();
            MakeTextCrisp(m_NextChapterSideText, new Color(0.12f, 0.06f, 0.02f, 0.95f), 1f);
            nextSideObj.SetActive(false);

            m_LevelSelectPanel.SetActive(false);
        }

                private void RefreshLevelSelectGrid(GameModeType mode)
        {
            if (m_LevelGridContainer == null) return;
            m_ActiveViewingMode = mode;
            m_IsLevelSelectTransitioning = false;

            int maxLvl = Mathf.Max(40, LevelManager.GetMaxLevel(mode));
            m_LevelSelectPage = 0;

            if (m_PrevChapterSideRt != null) m_PrevChapterSideRt.gameObject.SetActive(false);
            if (m_NextChapterSideRt != null) m_NextChapterSideRt.gameObject.SetActive(false);

            int totalStars = GetTotalModeStars(mode);

            // Clear previous child overlays
            for (int i = m_LevelGridContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(m_LevelGridContainer.GetChild(i).gameObject);
            }

            // Calculate current active play level: lowest unlocked level not yet completed (0 stars)
            int currentPlayLevel = 1;
            for (int l = 1; l <= maxLvl; l++)
            {
                if (LevelManager.IsLevelUnlocked(mode, l))
                {
                    currentPlayLevel = l;
                    if (LevelManager.GetLevelStars(mode, l) == 0)
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            // Update Dynamic Footer Text: "Current Level: 24       Total Stars: â˜… 68"
            if (m_AdventureFooterText != null)
            {
                m_AdventureFooterText.text = $"Current Level: {currentPlayLevel}       Total Stars: \u2605 {totalStars}";
            }

            // Authentic 3D Sphere Sprites for the road
            string[] sphereSpriteNames = new string[]
            {
                "Sphere_3D_Red",
                "Sphere_3D_Pink",
                "Sphere_3D_Purple",
                "Sphere_3D_Blue",
                "Sphere_3D_Cyan",
                "Sphere_3D_Green",
                "Sphere_3D_Yellow",
                "Sphere_3D_Orange"
            };

            const float btnSize = 74f;
            const float activeBtnSize = 82f;

            // Generate all 40 levels continuously along the 3D golden highway
            for (int targetLvl = 1; targetLvl <= maxLvl; targetLvl++)
            {
                bool isUnlocked = LevelManager.IsLevelUnlocked(mode, targetLvl);
                int stars = LevelManager.GetLevelStars(mode, targetLvl);
                bool isCurrentActive = (targetLvl == currentPlayLevel);

                Vector2 pos = (m_GalaxyRoad != null)
                    ? m_GalaxyRoad.GetLevelPosition(targetLvl)
                    : new Vector2(Mathf.Sin((targetLvl - 1) * 0.7853982f + 1.5707963f) * 175f, 130f + (targetLvl - 1) * 92f);

                GameObject btnObj = new GameObject($"LevelButton_{targetLvl}");
                btnObj.transform.SetParent(m_LevelGridContainer, false);
                RectTransform bRt = btnObj.AddComponent<RectTransform>();
                bRt.anchorMin = new Vector2(0.5f, 0f);
                bRt.anchorMax = new Vector2(0.5f, 0f);
                bRt.pivot = new Vector2(0.5f, 0.5f);
                bRt.anchoredPosition = pos;
                float curSize = isCurrentActive ? activeBtnSize : btnSize;
                bRt.sizeDelta = new Vector2(curSize, curSize);

                // 1. If Active: Add glowing pulsing aura halo behind the sphere
                if (isCurrentActive)
                {
                    GameObject auraObj = new GameObject("ActiveAura");
                    auraObj.transform.SetParent(btnObj.transform, false);
                    RectTransform aRt = auraObj.AddComponent<RectTransform>();
                    aRt.sizeDelta = new Vector2(115f, 115f);
                    Image aImg = auraObj.AddComponent<Image>();
                    aImg.sprite = SpriteFactory.GetCircleSprite(new Color(1.0f, 0.88f, 0.25f, 0.50f));
                    aImg.raycastTarget = false;
                    StartCoroutine(PulseActiveHalo(aRt));
                }

                // Milestone Level Golden Ring for every 5th level (5, 10, 15, 20...)
                if (targetLvl % 5 == 0)
                {
                    GameObject msObj = new GameObject("MilestoneAura");
                    msObj.transform.SetParent(btnObj.transform, false);
                    RectTransform msRt = msObj.AddComponent<RectTransform>();
                    msRt.sizeDelta = new Vector2(curSize * 1.25f, curSize * 1.25f);
                    Image msImg = msObj.AddComponent<Image>();
                    msImg.sprite = SpriteFactory.GetCircleSprite(new Color(1.0f, 0.82f, 0.2f, 0.35f));
                    msImg.raycastTarget = false;
                }

                // Base Sphere Image: Render vibrant 3D sphere for every level
                GameObject sphereObj = new GameObject("SphereImg");
                sphereObj.transform.SetParent(btnObj.transform, false);
                RectTransform sRt = sphereObj.AddComponent<RectTransform>();
                sRt.sizeDelta = new Vector2(curSize, curSize);
                Image sImg = sphereObj.AddComponent<Image>();
                sImg.raycastTarget = false;

                string spName = sphereSpriteNames[(targetLvl - 1) % sphereSpriteNames.Length];
                Sprite sphereSprite = SpriteFactory.LoadOrFallbackSprite(spName);
                if (sphereSprite != null)
                {
                    sImg.sprite = sphereSprite;
                    sImg.color = isUnlocked ? Color.white : new Color(0.70f, 0.72f, 0.82f, 0.95f);
                }
                else
                {
                    sImg.sprite = SpriteFactory.GetCircleSprite(new Color(0.2f, 0.7f, 1f));
                }

                // Level Number Text in Center of Sphere
                GameObject numObj = CreateText(btnObj.transform, Vector2.zero, new Vector2(70f, 44f), targetLvl.ToString(), isCurrentActive ? 32 : 26, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, m_CandyFont);
                Text numTxt = numObj.GetComponent<Text>();
                MakeTextCrisp(numTxt, new Color(0.02f, 0.04f, 0.14f, 0.95f), 1.2f);

                // Stars Display Tray
                GameObject starsTray = new GameObject("StarsTray");
                starsTray.transform.SetParent(btnObj.transform, false);
                RectTransform stRt = starsTray.AddComponent<RectTransform>();
                stRt.anchoredPosition = new Vector2(0f, -38f);
                stRt.sizeDelta = new Vector2(82f, 26f);

                Image stBg = starsTray.AddComponent<Image>();
                stBg.sprite = SpriteFactory.GetRoundedButtonSprite(new Color(0.05f, 0.10f, 0.26f, 0.90f), new Color(0.98f, 0.82f, 0.25f, 0.85f), 0.12f);
                stBg.raycastTarget = false;

                // Star 1 (Left: 20x20)
                GameObject s1 = new GameObject("Star_1");
                s1.transform.SetParent(starsTray.transform, false);
                RectTransform s1Rt = s1.AddComponent<RectTransform>();
                s1Rt.anchoredPosition = new Vector2(-20f, 1f);
                s1Rt.sizeDelta = new Vector2(20f, 20f);
                s1Rt.localEulerAngles = new Vector3(0f, 0f, 8f);
                Image s1Img = s1.AddComponent<Image>();
                s1Img.sprite = GemIconFactory.GetStarSprite(stars >= 1);
                s1Img.raycastTarget = false;

                // Star 2 (Center Hero Crown: 26x26)
                GameObject s2 = new GameObject("Star_2");
                s2.transform.SetParent(starsTray.transform, false);
                RectTransform s2Rt = s2.AddComponent<RectTransform>();
                s2Rt.anchoredPosition = new Vector2(0f, 3f);
                s2Rt.sizeDelta = new Vector2(26f, 26f);
                Image s2Img = s2.AddComponent<Image>();
                s2Img.sprite = GemIconFactory.GetStarSprite(stars >= 2);
                s2Img.raycastTarget = false;

                // Star 3 (Right: 20x20)
                GameObject s3 = new GameObject("Star_3");
                s3.transform.SetParent(starsTray.transform, false);
                RectTransform s3Rt = s3.AddComponent<RectTransform>();
                s3Rt.anchoredPosition = new Vector2(20f, 1f);
                s3Rt.sizeDelta = new Vector2(20f, 20f);
                s3Rt.localEulerAngles = new Vector3(0f, 0f, -8f);
                Image s3Img = s3.AddComponent<Image>();
                s3Img.sprite = GemIconFactory.GetStarSprite(stars >= 3);
                s3Img.raycastTarget = false;

                // If Locked: Add dark tint shield + golden lock badge
                if (!isUnlocked)
                {
                    GameObject lockOverlay = new GameObject("LockOverlay");
                    lockOverlay.transform.SetParent(btnObj.transform, false);
                    RectTransform loRt = lockOverlay.AddComponent<RectTransform>();
                    loRt.sizeDelta = new Vector2(curSize, curSize);
                    Image loImg = lockOverlay.AddComponent<Image>();
                    loImg.sprite = SpriteFactory.GetCircleSprite(new Color(0.04f, 0.06f, 0.16f, 0.60f));
                    loImg.raycastTarget = false;

                    GameObject lockBadge = new GameObject("LockBadge");
                    lockBadge.transform.SetParent(btnObj.transform, false);
                    RectTransform lbRt = lockBadge.AddComponent<RectTransform>();
                    lbRt.anchoredPosition = new Vector2(20f, 20f);
                    lbRt.sizeDelta = new Vector2(24f, 28f);
                    Image lbImg = lockBadge.AddComponent<Image>();
                    lbImg.sprite = SpriteFactory.LoadOrFallbackSprite("Wardrobe_Icon_Lock");
                    if (lbImg.sprite == null) lbImg.sprite = GemIconFactory.GetStarSprite(true);
                    lbImg.color = new Color(1f, 0.90f, 0.35f, 0.98f);
                    lbImg.raycastTarget = false;
                }

                // Active Play Badge above the sphere
                if (isCurrentActive)
                {
                    GameObject playBadgeObj = new GameObject("PlayBadge");
                    playBadgeObj.transform.SetParent(btnObj.transform, false);
                    RectTransform pbRt = playBadgeObj.AddComponent<RectTransform>();
                    pbRt.anchoredPosition = new Vector2(0f, 48f);
                    pbRt.sizeDelta = new Vector2(86f, 32f);

                    Image pbImg = playBadgeObj.AddComponent<Image>();
                    Sprite pbs = SpriteFactory.LoadOrFallbackSprite("PlayBadge3D");
                    if (pbs != null)
                    {
                        pbImg.sprite = pbs;
                        pbImg.color = Color.white;
                    }
                    else
                    {
                        pbImg.sprite = SpriteFactory.GetPillSprite(new Color(1.0f, 0.75f, 0.12f, 1f));
                    }
                    pbImg.raycastTarget = false;

                    GameObject playTxtObj = CreateText(playBadgeObj.transform, Vector2.zero, new Vector2(86f, 30f), "PLAY", 18, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, m_CandyFont);
                    Text playTxt = playTxtObj.GetComponent<Text>();
                    MakeTextCrisp(playTxt, new Color(0.55f, 0.25f, 0.02f, 0.95f), 1.0f);
                }

                // 4. Interactive Touch Target covering button
                Image bImg = btnObj.AddComponent<Image>();
                bImg.color = new Color(1f, 1f, 1f, 0.001f);
                bImg.raycastTarget = true;

                Button btn = btnObj.AddComponent<Button>();
                btn.targetGraphic = bImg;
                btn.transition = Selectable.Transition.None;

                btn.onClick.AddListener(() =>
                {
                    if (m_IsLevelSelectTransitioning) return;

                    if (LevelManager.IsLevelUnlocked(mode, targetLvl))
                    {
                        m_IsLevelSelectTransitioning = true;
                        if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
                        HapticManager.TriggerLight();

                        // Immediate touch feedback halo
                        GameObject touchHalo = new GameObject("TouchHalo");
                        touchHalo.transform.SetParent(btnObj.transform, false);
                        RectTransform thRt = touchHalo.AddComponent<RectTransform>();
                        thRt.sizeDelta = new Vector2(curSize * 1.35f, curSize * 1.35f);
                        Image thImg = touchHalo.AddComponent<Image>();
                        thImg.sprite = SpriteFactory.GetCircleSprite(new Color(1f, 0.95f, 0.4f, 0.70f));
                        thImg.raycastTarget = false;

                        StartCoroutine(AnimateButtonPress(bRt, () =>
                        {
                            if (LevelManager.Instance != null)
                            {
                                LevelManager.Instance.StartLevel(mode, targetLvl);
                            }
                            if (m_LevelSelectPanel != null) m_LevelSelectPanel.SetActive(false);
                            UpdateHUDVisibility();
                            m_IsLevelSelectTransitioning = false;
                        }));
                    }
                    else
                    {
                        if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
                        HapticManager.TriggerLight();
                        ShowToast($"Level {targetLvl} is locked! Complete Level {targetLvl - 1} first!");
                    }
                });
            }

            // Milestone Chests positioned at Option 1's authentic 3D treasure chest locations
            // Milestone Chests positioned along the continuous trail in scenic outer coves
            (Vector2 pos, int targetLevel, BallSkinType skinType, string skinName)[] milestoneChests =
                new (Vector2, int, BallSkinType, string)[]
            {
                (new Vector2( 140f,  498f),  5, BallSkinType.LimeMint,      "Emerald Mint Ball Skin!"),
                (new Vector2(-140f,  958f), 10, BallSkinType.OrangePlanet,  "Orange Planet Ball Skin!"),
                (new Vector2( 140f, 1380f), 15, BallSkinType.StarCore,      "Golden Star Core Ball Skin!"),
                (new Vector2( 140f, 1878f), 20, BallSkinType.RainbowBlast,  "Rainbow Blast Ball Skin!"),
                (new Vector2(-140f, 2338f), 25, BallSkinType.LightningVolt, "Lightning Volt Ball Skin!"),
                (new Vector2( 140f, 2798f), 30, BallSkinType.SunBurst,      "Solar Flare Ball Skin!"),
                (new Vector2( 140f, 3220f), 35, BallSkinType.GalaxySwirl,   "Galaxy Nebula Ball Skin!"),
                (new Vector2(-140f, 3718f), 40, BallSkinType.CyanWave,      "Royal Cyan Wave Ball Skin!")
            };

            for (int ci = 0; ci < milestoneChests.Length; ci++)
            {
                var chestInfo = milestoneChests[ci];
                int targetLvl = chestInfo.targetLevel;
                BallSkinType skin = chestInfo.skinType;
                string rewardText = chestInfo.skinName;

                bool isConditionMet = LevelManager.IsLevelUnlocked(mode, targetLvl + 1) || LevelManager.GetLevelStars(mode, targetLvl) > 0;
                bool isClaimed = PlayerPrefs.GetInt("ChestClaimed_Level_" + targetLvl, 0) == 1 || (skin != BallSkinType.Classic && SkinManager.IsSkinUnlocked(skin));

                GameObject chestObj = new GameObject($"MilestoneChest_{targetLvl}");
                chestObj.transform.SetParent(m_LevelGridContainer, false);
                RectTransform cRt = chestObj.AddComponent<RectTransform>();
                cRt.anchorMin = new Vector2(0.5f, 0f);
                cRt.anchorMax = new Vector2(0.5f, 0f);
                cRt.pivot = new Vector2(0.5f, 0.5f);
                cRt.anchoredPosition = chestInfo.pos;
                cRt.sizeDelta = new Vector2(100f, 100f);

                // Invisible touch/raycast target covering the full chest region
                Image cTouchImg = chestObj.AddComponent<Image>();
                cTouchImg.color = new Color(1f, 1f, 1f, 0.001f);
                cTouchImg.raycastTarget = true;

                // 1. If Claimable & Not Yet Opened: Glowing Golden Aura behind the chest
                if (isConditionMet && !isClaimed)
                {
                    GameObject auraObj = new GameObject("ChestAura");
                    auraObj.transform.SetParent(chestObj.transform, false);
                    RectTransform aRt = auraObj.AddComponent<RectTransform>();
                    aRt.sizeDelta = new Vector2(120f, 120f);
                    Image aImg = auraObj.AddComponent<Image>();
                    aImg.sprite = SpriteFactory.GetCircleSprite(new Color(1f, 0.86f, 0.22f, 0.55f));
                    aImg.raycastTarget = false;
                    StartCoroutine(PulseActiveHalo(aRt));
                }

                // 2. 3D Chest Visual
                GameObject imgObj = new GameObject("ChestVisual");
                imgObj.transform.SetParent(chestObj.transform, false);
                RectTransform imgRt = imgObj.AddComponent<RectTransform>();
                imgRt.sizeDelta = new Vector2(96f, 96f);
                Image cImg = imgObj.AddComponent<Image>();
                cImg.raycastTarget = false;

                if (isClaimed)
                {
                    cImg.sprite = SpriteFactory.GetTreasureChestOpenSprite();
                    cImg.color = Color.white;

                    if (skin != BallSkinType.Classic)
                    {
                        GameObject ballInChest = new GameObject("BallInsideChest");
                        ballInChest.transform.SetParent(chestObj.transform, false);
                        RectTransform bicRt = ballInChest.AddComponent<RectTransform>();
                        bicRt.anchoredPosition = new Vector2(0f, 8f);
                        bicRt.sizeDelta = new Vector2(40f, 40f);

                        Image bicImg = ballInChest.AddComponent<Image>();
                        bicImg.sprite = SpriteFactory.GetWardrobeSkinSprite(skin);
                        bicImg.raycastTarget = false;

                        GameObject badge = new GameObject("SkinBadge");
                        badge.transform.SetParent(chestObj.transform, false);
                        RectTransform bRt = badge.AddComponent<RectTransform>();
                        bRt.anchoredPosition = new Vector2(28f, 28f);
                        bRt.sizeDelta = new Vector2(28f, 28f);
                        Image bImg = badge.AddComponent<Image>();
                        bImg.sprite = SpriteFactory.GetGreenCheckmarkBadgeSprite();
                        bImg.raycastTarget = false;
                    }
                }
                else
                {
                    cImg.sprite = SpriteFactory.GetTreasureChestClosedSprite();
                    cImg.color = Color.white;
                    if (isConditionMet) StartCoroutine(AnimateBouncyChest(cRt));
                }

                // Level Pill Badge below chest
                GameObject tagObj = new GameObject("LevelTag");
                tagObj.transform.SetParent(chestObj.transform, false);
                RectTransform tagRt = tagObj.AddComponent<RectTransform>();
                tagRt.anchoredPosition = new Vector2(0f, -38f);
                tagRt.sizeDelta = new Vector2(74f, 20f);
                Image tagBg = tagObj.AddComponent<Image>();
                tagBg.raycastTarget = false;

                if (isClaimed)
                {
                    tagBg.sprite = SpriteFactory.GetPillSprite(new Color(0.04f, 0.22f, 0.12f, 0.95f));
                    GameObject tagTxtObj = CreateText(tagObj.transform, Vector2.zero, new Vector2(74f, 20f), $"LVL {targetLvl} \u2713", 12, FontStyle.Bold, new Color(0.45f, 1f, 0.55f, 1f), TextAnchor.MiddleCenter, m_CandyFont);
                    Text tagTxt = tagTxtObj.GetComponent<Text>();
                    MakeTextCrisp(tagTxt, new Color(0.01f, 0.08f, 0.04f, 0.95f), 1f);
                }
                else
                {
                    tagBg.sprite = SpriteFactory.GetPillSprite(isConditionMet ? new Color(0.65f, 0.45f, 0.02f, 0.95f) : new Color(0.04f, 0.08f, 0.22f, 0.92f));
                    GameObject tagTxtObj = CreateText(tagObj.transform, Vector2.zero, new Vector2(74f, 20f), isConditionMet ? $"LVL {targetLvl}!" : $"LVL {targetLvl}", 12, FontStyle.Bold, isConditionMet ? new Color(1f, 0.96f, 0.4f, 1f) : new Color(1f, 0.88f, 0.35f, 1f), TextAnchor.MiddleCenter, m_CandyFont);
                    Text tagTxt = tagTxtObj.GetComponent<Text>();
                    MakeTextCrisp(tagTxt, new Color(0.02f, 0.04f, 0.12f, 0.95f), 1f);
                }

                // 4. Interactive Touch Button
                Button cBtn = chestObj.AddComponent<Button>();
                cBtn.targetGraphic = cTouchImg;
                cBtn.transition = Selectable.Transition.None;
                cBtn.onClick.AddListener(() =>
                {
                    if (m_IsLevelSelectTransitioning) return;
                    if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
                    HapticManager.TriggerLight();

                    if (!isClaimed && isConditionMet)
                    {
                        ShowChestUnboxingModal(targetLvl, skin, rewardText);
                    }
                    else
                    {
                        ShowChestPreviewModal(targetLvl, skin, rewardText, isClaimed);
                    }
                });

                chestObj.transform.SetAsLastSibling();
            }
        }

        private System.Collections.IEnumerator PulseActiveHalo(RectTransform haloRt)
        {
            if (haloRt == null) yield break;
            Vector3 baseScale = Vector3.one;
            while (haloRt != null && haloRt.gameObject.activeInHierarchy)
            {
                float t = Mathf.PingPong(Time.unscaledTime * 1.4f, 1f);
                float s = Mathf.Lerp(0.95f, 1.15f, t);
                haloRt.localScale = baseScale * s;
                yield return null;
            }
        }

        private System.Collections.IEnumerator AnimateBouncyChest(RectTransform rt)
        {
            if (rt == null) yield break;
            Vector3 baseScale = Vector3.one;
            while (rt != null && rt.gameObject.activeInHierarchy)
            {
                float t = Mathf.PingPong(Time.unscaledTime * 1.6f, 1f);
                float sY = Mathf.Lerp(0.96f, 1.08f, t);
                float sX = Mathf.Lerp(1.04f, 0.96f, t);
                rt.localScale = new Vector3(baseScale.x * sX, baseScale.y * sY, 1f);
                yield return null;
            }
        }

        private GameObject m_ChestPreviewModal;

        public void ShowChestPreviewModal(int milestoneLevel, BallSkinType skinType, string fallbackReward, bool isClaimed)
        {
            if (m_RootCanvas == null)
            {
                Canvas canvas = GetComponentInParent<Canvas>();
                if (canvas != null) m_RootCanvas = canvas.transform;
                else if (m_LevelSelectPanel != null) m_RootCanvas = m_LevelSelectPanel.transform.parent;
            }
            if (m_RootCanvas == null) return;
            if (m_ChestPreviewModal != null)
            {
                Destroy(m_ChestPreviewModal);
                m_ChestPreviewModal = null;
            }

            m_ChestPreviewModal = new GameObject("ChestPreviewModal");
            m_ChestPreviewModal.transform.SetParent(m_RootCanvas, false);
            m_ChestPreviewModal.transform.SetAsLastSibling();

            RectTransform rootRt = m_ChestPreviewModal.AddComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.sizeDelta = Vector2.zero;

            // Fullscreen backdrop matching background deep space navy
            Image bg = m_ChestPreviewModal.AddComponent<Image>();
            bg.color = new Color(0.04f, 0.07f, 0.16f, 0.98f);
            bg.raycastTarget = true;
            Button bgBtn = m_ChestPreviewModal.AddComponent<Button>();
            bgBtn.transition = Selectable.Transition.None;
            bgBtn.onClick.AddListener(() =>
            {
                if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
                HapticManager.TriggerLight();
                if (m_ChestPreviewModal != null) Destroy(m_ChestPreviewModal);
                m_ChestPreviewModal = null;
            });

            // Centered 3D Scene Viewport Card (768 x 1376 native aspect ratio matching Settings/Concept 2)
            GameObject sceneObj = new GameObject("ChestSceneCard", typeof(RectTransform));
            sceneObj.transform.SetParent(m_ChestPreviewModal.transform, false);
            RectTransform sceneRt = sceneObj.GetComponent<RectTransform>();
            sceneRt.anchorMin = new Vector2(0.5f, 0.5f);
            sceneRt.anchorMax = new Vector2(0.5f, 0.5f);
            sceneRt.pivot = new Vector2(0.5f, 0.5f);
            sceneRt.anchoredPosition = Vector2.zero;
            sceneRt.sizeDelta = new Vector2(768, 1376);

            // Dynamically scale sceneRt to fit screen height perfectly
            float canvasHeight = 1920f;
            float canvasWidth = 1080f;
            if (m_RootCanvas != null)
            {
                RectTransform cRt = m_RootCanvas.GetComponent<RectTransform>();
                if (cRt != null && cRt.rect.height > 0) canvasHeight = cRt.rect.height;
                if (cRt != null && cRt.rect.width > 0) canvasWidth = cRt.rect.width;
            }
            float fitScale = canvasHeight / 1376f;
            if (768f * fitScale > canvasWidth)
            {
                fitScale = canvasWidth / 768f;
            }
            sceneRt.localScale = new Vector3(fitScale, fitScale, 1f);

            // 1. High-Definition 3D Royal Vault Render Artwork (Matching Settings Concept 2 Frame & Crown)
            Image sceneImg = sceneObj.AddComponent<Image>();
            sceneImg.sprite = SpriteFactory.LoadOrFallbackSprite("ChestBackground_Concept1");
            sceneImg.color = Color.white;
            sceneImg.raycastTarget = true;

            // 2. Candy 3D Ruby Red Close (X) Button (placed directly over painted red X at top right X: 306, Y: 393)
            GameObject closeObj = new GameObject("ChestCloseBtn", typeof(RectTransform));
            closeObj.transform.SetParent(sceneObj.transform, false);
            RectTransform closeRt = closeObj.GetComponent<RectTransform>();
            closeRt.anchoredPosition = new Vector2(306f, 393f);
            closeRt.sizeDelta = new Vector2(96f, 96f);
            Image closeImg = closeObj.AddComponent<Image>();
            closeImg.color = Color.clear; // Clear so painted red 3D 'X' is fully visible without dark square artifact
            closeImg.raycastTarget = true;
            Button closeBtn = closeObj.AddComponent<Button>();

            System.Action closeAction = () =>
            {
                if (m_ChestPreviewModal != null) Destroy(m_ChestPreviewModal);
                m_ChestPreviewModal = null;
            };

            closeBtn.onClick.AddListener(() =>
            {
                StartCoroutine(AnimateButtonPress(closeRt, () =>
                {
                    if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
                    HapticManager.TriggerLight();
                    closeAction();
                }));
            });
            closeObj.transform.SetAsLastSibling();

            SkinInfo info = SkinManager.GetSkinInfo(skinType);
            string skinDisplayName = (info != null && !string.IsNullOrEmpty(info.Name)) ? info.Name.ToUpper() : "COSMIC SPHERE";
            string skinDesc = (info != null && !string.IsNullOrEmpty(info.Description)) ? info.Description : "Exclusive 3D ball design!";

            // 3. Milestone Tag (Under Plaque, X: 0, Y: 245)
            GameObject stateTag = new GameObject("StateTag");
            stateTag.transform.SetParent(sceneObj.transform, false);
            RectTransform stRt = stateTag.AddComponent<RectTransform>();
            stRt.anchoredPosition = new Vector2(0f, 245f);
            stRt.sizeDelta = new Vector2(440f, 44f);
            Image stBg = stateTag.AddComponent<Image>();
            stBg.sprite = SpriteFactory.GetPillSprite(isClaimed ? new Color(0.08f, 0.42f, 0.18f, 0.95f) : new Color(0.65f, 0.35f, 0.05f, 0.95f));
            stBg.raycastTarget = false;

            string statusStr = isClaimed ? $"LEVEL {milestoneLevel} • BALL SKIN UNLOCKED!" : $"LEVEL {milestoneLevel} • EXCLUSIVE REWARD";
            GameObject stTxtObj = CreateText(stateTag.transform, Vector2.zero, new Vector2(420f, 40f), statusStr, 20, FontStyle.Normal, Color.white, TextAnchor.MiddleCenter, m_CandyFont);
            Text stTxt = stTxtObj.GetComponent<Text>();
            MakeTextCrisp(stTxt, new Color(0.02f, 0.04f, 0.12f, 0.95f), 1f);

            // 4. 3D Chest Visual (Centered in celestial starry chamber)
            GameObject chestImgObj = new GameObject("Chest3D");
            chestImgObj.transform.SetParent(sceneObj.transform, false);
            RectTransform chestRt = chestImgObj.AddComponent<RectTransform>();
            chestRt.anchoredPosition = new Vector2(0f, -40f);
            chestRt.sizeDelta = new Vector2(320f, 320f);
            Image chestImg = chestImgObj.AddComponent<Image>();
            chestImg.sprite = isClaimed ? SpriteFactory.GetTreasureChestOpenSprite() : SpriteFactory.GetTreasureChestClosedSprite();
            chestImg.raycastTarget = false;

            // 5. Floating 3D Ball Skin in its pure, vibrant authentic colors (no yellow tint)
            GameObject ballObj = new GameObject("InspectedBall");
            ballObj.transform.SetParent(sceneObj.transform, false);
            RectTransform ballRt = ballObj.AddComponent<RectTransform>();
            ballRt.anchoredPosition = new Vector2(0f, 130f);
            ballRt.sizeDelta = new Vector2(200f, 200f);
            Image ballImg = ballObj.AddComponent<Image>();
            ballImg.sprite = SpriteFactory.GetWardrobeSkinSprite(skinType);
            ballImg.raycastTarget = false;
            StartCoroutine(FloatBallSkin(ballRt));

            // 6. Subtitle Card with Skin Name & Lore Description (Translucent celestial glass with golden rim)
            GameObject subCard = new GameObject("SubCard");
            subCard.transform.SetParent(sceneObj.transform, false);
            RectTransform scRt = subCard.AddComponent<RectTransform>();
            scRt.anchoredPosition = new Vector2(0f, -205f);
            scRt.sizeDelta = new Vector2(520f, 96f);
            Image scBg = subCard.AddComponent<Image>();
            scBg.sprite = SpriteFactory.GetPanelSprite(new Color(0.04f, 0.09f, 0.24f, 0.92f), new Color(0.85f, 0.65f, 0.20f, 0.85f), 2f, 16);
            scBg.type = Image.Type.Sliced;
            scBg.raycastTarget = false;

            GameObject nameTxtObj = CreateText(subCard.transform, new Vector2(0f, 18f), new Vector2(500f, 36f), $"{skinDisplayName} BALL", 26, FontStyle.Normal, new Color(1f, 0.88f, 0.35f, 1f), TextAnchor.MiddleCenter, m_CandyFont);
            Text nameTxt = nameTxtObj.GetComponent<Text>();
            MakeTextCrisp(nameTxt, new Color(0.02f, 0.04f, 0.12f, 0.95f), 1.1f);

            string descLine = isClaimed ? skinDesc : $"Complete Level {milestoneLevel} in Adventure Mode to unlock!";
            Color descCol = isClaimed ? new Color(0.85f, 0.92f, 1f, 0.90f) : new Color(1f, 0.82f, 0.40f, 0.95f);
            GameObject descTxtObj = CreateText(subCard.transform, new Vector2(0f, -18f), new Vector2(500f, 30f), descLine, 19, FontStyle.Normal, descCol, TextAnchor.MiddleCenter, m_CandyFont);
            Text descTxt = descTxtObj.GetComponent<Text>();
            MakeTextCrisp(descTxt, new Color(0.02f, 0.04f, 0.12f, 0.95f), 0.8f);

            // 7. Action Button Tray
            GameObject btnTray = new GameObject("ButtonTray");
            btnTray.transform.SetParent(sceneObj.transform, false);
            RectTransform btRt = btnTray.AddComponent<RectTransform>();
            btRt.anchoredPosition = new Vector2(0f, -345f);
            btRt.sizeDelta = new Vector2(560f, 85f);

            if (isClaimed)
            {
                bool isCurrentlyEquipped = (SkinManager.CurrentSkin == skinType);

                // Button 1: EQUIP NOW
                GameObject equipBtnObj = new GameObject("EquipBtn");
                equipBtnObj.transform.SetParent(btnTray.transform, false);
                RectTransform ebRt = equipBtnObj.AddComponent<RectTransform>();
                ebRt.anchoredPosition = new Vector2(-125f, 0f);
                ebRt.sizeDelta = new Vector2(260f, 78f);
                Image ebBg = equipBtnObj.AddComponent<Image>();
                Color ebCol1 = isCurrentlyEquipped ? new Color(0.15f, 0.45f, 0.20f, 1f) : new Color(0.98f, 0.72f, 0.15f, 1f);
                Color ebCol2 = isCurrentlyEquipped ? new Color(0.08f, 0.25f, 0.10f, 1f) : new Color(0.55f, 0.28f, 0.02f, 1f);
                ebBg.sprite = SpriteFactory.GetRoundedButtonSprite(ebCol1, ebCol2, 0.18f);
                Button ebBtn = equipBtnObj.AddComponent<Button>();

                string eqBtnStr = isCurrentlyEquipped ? "EQUIPPED" : "EQUIP NOW";
                GameObject ebTxtObj = CreateText(equipBtnObj.transform, Vector2.zero, new Vector2(240f, 70f), eqBtnStr, 24, FontStyle.Normal, Color.white, TextAnchor.MiddleCenter, m_CandyFont);
                Text ebTxt = ebTxtObj.GetComponent<Text>();
                MakeTextCrisp(ebTxt, new Color(0.02f, 0.04f, 0.12f, 0.95f), 1.1f);

                if (isCurrentlyEquipped)
                {
                    ebBtn.interactable = false;
                }
                else
                {
                    ebBtn.onClick.AddListener(() =>
                    {
                        SkinManager.EquipSkin(skinType);
                        ebTxt.text = "EQUIPPED!";
                        ebBtn.interactable = false;
                        ebBg.sprite = SpriteFactory.GetRoundedButtonSprite(new Color(0.15f, 0.45f, 0.20f, 1f), new Color(0.08f, 0.25f, 0.10f, 1f), 0.18f);
                        HapticManager.TriggerLight();
                        if (SoundManager.Instance != null) SoundManager.Instance.PlayPlace();
                        StartCoroutine(DelayedCloseModal(0.8f, closeAction));
                    });
                }

                // Button 2: CLOSE
                GameObject closeBtnObj = new GameObject("CloseBtn");
                closeBtnObj.transform.SetParent(btnTray.transform, false);
                RectTransform obRt = closeBtnObj.AddComponent<RectTransform>();
                obRt.anchoredPosition = new Vector2(135f, 0f);
                obRt.sizeDelta = new Vector2(200f, 78f);
                Image obBg = closeBtnObj.AddComponent<Image>();
                obBg.sprite = SpriteFactory.GetRoundedButtonSprite(new Color(0.08f, 0.22f, 0.50f, 1f), new Color(0.04f, 0.10f, 0.28f, 1f), 0.18f);
                Button obBtn = closeBtnObj.AddComponent<Button>();

                GameObject obTxtObj = CreateText(closeBtnObj.transform, Vector2.zero, new Vector2(180f, 70f), "CLOSE", 24, FontStyle.Normal, Color.white, TextAnchor.MiddleCenter, m_CandyFont);
                Text obTxt = obTxtObj.GetComponent<Text>();
                MakeTextCrisp(obTxt, new Color(0.02f, 0.06f, 0.18f, 0.95f), 1.0f);
                obBtn.onClick.AddListener(() =>
                {
                    if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
                    HapticManager.TriggerLight();
                    closeAction();
                });
            }
            else
            {
                // Locked Preview: Button 1 = PLAY LEVEL (if unlocked) or CLOSE
                bool canPlayLevel = LevelManager.IsLevelUnlocked(GameModeType.Adventure, milestoneLevel);

                if (canPlayLevel)
                {
                    GameObject playBtnObj = new GameObject("PlayBtn");
                    playBtnObj.transform.SetParent(btnTray.transform, false);
                    RectTransform pbRt = playBtnObj.AddComponent<RectTransform>();
                    pbRt.anchoredPosition = new Vector2(-115f, 0f);
                    pbRt.sizeDelta = new Vector2(270f, 78f);
                    Image pbBg = playBtnObj.AddComponent<Image>();
                    pbBg.sprite = SpriteFactory.GetRoundedButtonSprite(new Color(0.98f, 0.72f, 0.15f, 1f), new Color(0.55f, 0.28f, 0.02f, 1f), 0.18f);
                    Button pbBtn = playBtnObj.AddComponent<Button>();

                    GameObject pbTxtObj = CreateText(playBtnObj.transform, Vector2.zero, new Vector2(250f, 70f), $"PLAY LVL {milestoneLevel}", 23, FontStyle.Normal, Color.white, TextAnchor.MiddleCenter, m_CandyFont);
                    Text pbTxt = pbTxtObj.GetComponent<Text>();
                    MakeTextCrisp(pbTxt, new Color(0.45f, 0.22f, 0.02f, 0.95f), 1.1f);

                    pbBtn.onClick.AddListener(() =>
                    {
                        if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
                        HapticManager.TriggerLight();
                        closeAction();
                        if (LevelManager.Instance != null)
                        {
                            LevelManager.Instance.StartLevel(GameModeType.Adventure, milestoneLevel);
                        }
                        if (m_LevelSelectPanel != null) m_LevelSelectPanel.SetActive(false);
                        UpdateHUDVisibility();
                    });

                    // Button 2: CLOSE
                    GameObject closeBtnObj = new GameObject("CloseBtn");
                    closeBtnObj.transform.SetParent(btnTray.transform, false);
                    RectTransform obRt = closeBtnObj.AddComponent<RectTransform>();
                    obRt.anchoredPosition = new Vector2(145f, 0f);
                    obRt.sizeDelta = new Vector2(190f, 78f);
                    Image obBg = closeBtnObj.AddComponent<Image>();
                    obBg.sprite = SpriteFactory.GetRoundedButtonSprite(new Color(0.08f, 0.22f, 0.50f, 1f), new Color(0.04f, 0.10f, 0.28f, 1f), 0.18f);
                    Button obBtn = closeBtnObj.AddComponent<Button>();

                    GameObject obTxtObj = CreateText(closeBtnObj.transform, Vector2.zero, new Vector2(170f, 70f), "CLOSE", 24, FontStyle.Normal, Color.white, TextAnchor.MiddleCenter, m_CandyFont);
                    Text obTxt = obTxtObj.GetComponent<Text>();
                    MakeTextCrisp(obTxt, new Color(0.02f, 0.06f, 0.18f, 0.95f), 1.0f);
                    obBtn.onClick.AddListener(() =>
                    {
                        if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
                        HapticManager.TriggerLight();
                        closeAction();
                    });
                }
                else
                {
                    // Locked: Single centered "GOT IT!" button
                    GameObject okBtnObj = new GameObject("GotItBtn");
                    okBtnObj.transform.SetParent(btnTray.transform, false);
                    RectTransform obRt = okBtnObj.AddComponent<RectTransform>();
                    obRt.anchoredPosition = Vector2.zero;
                    obRt.sizeDelta = new Vector2(300f, 78f);
                    Image obBg = okBtnObj.AddComponent<Image>();
                    obBg.sprite = SpriteFactory.GetRoundedButtonSprite(new Color(0.08f, 0.22f, 0.50f, 1f), new Color(0.04f, 0.10f, 0.28f, 1f), 0.18f);
                    Button obBtn = okBtnObj.AddComponent<Button>();

                    GameObject obTxtObj = CreateText(okBtnObj.transform, Vector2.zero, new Vector2(280f, 70f), "GOT IT!", 25, FontStyle.Normal, Color.white, TextAnchor.MiddleCenter, m_CandyFont);
                    Text obTxt = obTxtObj.GetComponent<Text>();
                    MakeTextCrisp(obTxt, new Color(0.02f, 0.06f, 0.18f, 0.95f), 1.0f);
                    obBtn.onClick.AddListener(() =>
                    {
                        if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
                        HapticManager.TriggerLight();
                        closeAction();
                    });
                }
            }

            StartCoroutine(AnimateModalOpen(sceneRt, fitScale));
        }

        private System.Collections.IEnumerator RotateGodRays(RectTransform raysRt)
        {
            if (raysRt == null) yield break;
            while (raysRt != null && raysRt.gameObject.activeInHierarchy)
            {
                raysRt.Rotate(0f, 0f, -25f * Time.unscaledDeltaTime);
                yield return null;
            }
        }

        private System.Collections.IEnumerator FloatBallSkin(RectTransform ballRt)
        {
            if (ballRt == null) yield break;
            Vector2 basePos = ballRt.anchoredPosition;
            while (ballRt != null && ballRt.gameObject.activeInHierarchy)
            {
                float t = Mathf.PingPong(Time.unscaledTime * 1.5f, 1f);
                float offsetY = Mathf.SmoothStep(-8f, 8f, t);
                ballRt.anchoredPosition = new Vector2(basePos.x, basePos.y + offsetY);
                yield return null;
            }
        }

        private GameObject m_ChestUnboxingModal;

        public void ShowChestUnboxingModal(int milestoneLevel, BallSkinType skinType, string fallbackReward)
        {
            if (m_RootCanvas == null)
            {
                Canvas canvas = GetComponentInParent<Canvas>();
                if (canvas != null) m_RootCanvas = canvas.transform;
                else if (m_LevelSelectPanel != null) m_RootCanvas = m_LevelSelectPanel.transform.parent;
            }
            if (m_RootCanvas == null) return;
            if (m_ChestUnboxingModal != null)
            {
                Destroy(m_ChestUnboxingModal);
                m_ChestUnboxingModal = null;
            }

            m_ChestUnboxingModal = new GameObject("ChestUnboxingModal");
            m_ChestUnboxingModal.transform.SetParent(m_RootCanvas, false);
            m_ChestUnboxingModal.transform.SetAsLastSibling();

            RectTransform rootRt = m_ChestUnboxingModal.AddComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.sizeDelta = Vector2.zero;

            // Fullscreen Dark Backdrop
            Image bg = m_ChestUnboxingModal.AddComponent<Image>();
            bg.color = new Color(0.02f, 0.04f, 0.12f, 0.94f);
            bg.raycastTarget = true;

            StartCoroutine(AnimateChestUnboxingSequence(m_ChestUnboxingModal, milestoneLevel, skinType, fallbackReward));
        }

        private System.Collections.IEnumerator AnimateChestUnboxingSequence(GameObject modalObj, int milestoneLevel, BallSkinType skinType, string fallbackReward)
        {
            if (modalObj == null) yield break;

            // Container
            GameObject container = new GameObject("Container");
            container.transform.SetParent(modalObj.transform, false);
            RectTransform cRt = container.AddComponent<RectTransform>();
            cRt.anchoredPosition = Vector2.zero;
            cRt.sizeDelta = new Vector2(720f, 900f);

            // 1. Title Text
            GameObject titleObj = CreateText(container.transform, new Vector2(0f, 330f), new Vector2(700f, 80f), $"LEVEL {milestoneLevel} CHEST!", 44, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, m_CandyFont);
            Text titleTxt = titleObj.GetComponent<Text>();
            MakeTextCrisp(titleTxt, new Color(0.50f, 0.25f, 0.02f, 0.95f), 1.2f);

            // 2. 3D Chest GameObject
            GameObject chestImgObj = new GameObject("Chest3D");
            chestImgObj.transform.SetParent(container.transform, false);
            RectTransform chestRt = chestImgObj.AddComponent<RectTransform>();
            chestRt.anchoredPosition = new Vector2(0f, -20f);
            chestRt.sizeDelta = new Vector2(340f, 340f);
            Image chestImg = chestImgObj.AddComponent<Image>();
            chestImg.sprite = SpriteFactory.GetTreasureChestClosedSprite();
            chestImg.raycastTarget = false;

            // Phase 1: Anticipation Rattle & Shake (0.75 seconds)
            float shakeDur = 0.75f;
            float elapsed = 0f;
            float nextHaptic = 0f;
            while (elapsed < shakeDur && modalObj != null)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = elapsed / shakeDur;
                float shakeX = Mathf.Sin(elapsed * 45f) * (8f + progress * 6f);
                float tiltZ = Mathf.Cos(elapsed * 40f) * (4f + progress * 5f);
                chestRt.anchoredPosition = new Vector2(shakeX, -20f);
                chestRt.localEulerAngles = new Vector3(0f, 0f, tiltZ);
                chestRt.localScale = Vector3.one * (1f + progress * 0.12f);

                if (elapsed >= nextHaptic)
                {
                    nextHaptic += 0.16f;
                    HapticManager.TriggerMedium();
                    if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
                }
                yield return null;
            }

            // Phase 2: POP OPEN & BURST!
            if (modalObj == null) yield break;
            chestImg.sprite = SpriteFactory.GetTreasureChestOpenSprite();
            chestRt.anchoredPosition = new Vector2(0f, -20f);
            chestRt.localEulerAngles = Vector3.zero;

            if (SoundManager.Instance != null) SoundManager.Instance.PlayAchievement();
            HapticManager.TriggerHeavy();

            // Elastic pop scale on chest
            float popDur = 0.25f;
            elapsed = 0f;
            while (elapsed < popDur && modalObj != null)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / popDur;
                float s = Mathf.Lerp(1.35f, 1.05f, Mathf.Sin(t * Mathf.PI * 0.5f));
                chestRt.localScale = Vector3.one * s;
                yield return null;
            }
            chestRt.localScale = Vector3.one * 1.05f;

            // Phase 3: New Ball Design Emerges from the Open Chest in its true colors
            SkinInfo info = SkinManager.GetSkinInfo(skinType);
            GameObject ballObj = new GameObject("EmergingBall");
            ballObj.transform.SetParent(container.transform, false);
            ballObj.transform.SetAsLastSibling();
            RectTransform ballRt = ballObj.AddComponent<RectTransform>();
            ballRt.anchoredPosition = new Vector2(0f, -20f);
            ballRt.sizeDelta = new Vector2(190f, 190f);
            ballRt.localScale = Vector3.one * 0.2f;

            Image ballImg = ballObj.AddComponent<Image>();
            ballImg.sprite = SpriteFactory.GetWardrobeSkinSprite(skinType);
            ballImg.raycastTarget = false;

            // Float upward animation
            float floatDur = 0.85f;
            elapsed = 0f;
            while (elapsed < floatDur && modalObj != null)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / floatDur);
                // Ease out cubic
                float smooth = 1f - Mathf.Pow(1f - t, 3f);
                float curY = Mathf.Lerp(-20f, 175f, smooth);
                float curScale = Mathf.Lerp(0.2f, 1.30f, smooth);
                ballRt.anchoredPosition = new Vector2(0f, curY);
                ballRt.localScale = Vector3.one * curScale;
                yield return null;
            }
            ballRt.anchoredPosition = new Vector2(0f, 175f);
            ballRt.localScale = Vector3.one * 1.30f;

            if (SoundManager.Instance != null) SoundManager.Instance.PlayCombo();

            // Phase 4: Announcement & Equip Action
            titleTxt.text = "NEW BALL UNLOCKED!";
            MakeTextCrisp(titleTxt, new Color(0.95f, 0.65f, 0.05f, 0.98f), 1.2f);

            // Subtitle Card
            GameObject subCard = new GameObject("SubCard");
            subCard.transform.SetParent(container.transform, false);
            RectTransform scRt = subCard.AddComponent<RectTransform>();
            scRt.anchoredPosition = new Vector2(0f, -145f);
            scRt.sizeDelta = new Vector2(620f, 90f);
            Image scBg = subCard.AddComponent<Image>();
            scBg.sprite = SpriteFactory.GetPillSprite(new Color(0.04f, 0.09f, 0.24f, 0.92f));
            scBg.raycastTarget = false;

            string skinDisplayName = (info != null && !string.IsNullOrEmpty(info.Name)) ? info.Name.ToUpper() : "COSMIC SPHERE";
            string skinDesc = (info != null && !string.IsNullOrEmpty(info.Description)) ? info.Description : "Exclusive 3D ball design!";

            GameObject nameTxtObj = CreateText(subCard.transform, new Vector2(0f, 16f), new Vector2(600f, 40f), $"{skinDisplayName} BALL", 28, FontStyle.Bold, new Color(1f, 0.88f, 0.35f, 1f), TextAnchor.MiddleCenter, m_CandyFont);
            Text nameTxt = nameTxtObj.GetComponent<Text>();
            MakeTextCrisp(nameTxt, new Color(0.02f, 0.04f, 0.12f, 0.95f), 1.1f);

            GameObject descTxtObj = CreateText(subCard.transform, new Vector2(0f, -20f), new Vector2(580f, 32f), skinDesc, 20, FontStyle.Normal, new Color(0.85f, 0.92f, 1f, 0.90f), TextAnchor.MiddleCenter, m_CandyFont);
            Text descTxt = descTxtObj.GetComponent<Text>();
            MakeTextCrisp(descTxt, new Color(0.02f, 0.04f, 0.12f, 0.95f), 0.8f);

            // Mark Skin Unlocked and Chest Claimed in PlayerPrefs
            if (skinType != BallSkinType.Classic)
            {
                SkinManager.UnlockSkin(skinType);
            }
            PlayerPrefs.SetInt("ChestClaimed_Level_" + milestoneLevel, 1);
            PlayerPrefs.Save();

            // Button Tray
            GameObject btnTray = new GameObject("ButtonTray");
            btnTray.transform.SetParent(container.transform, false);
            RectTransform btRt = btnTray.AddComponent<RectTransform>();
            btRt.anchoredPosition = new Vector2(0f, -265f);
            btRt.sizeDelta = new Vector2(620f, 85f);

            // Button 1: EQUIP NOW (Gold 3D)
            GameObject equipBtnObj = new GameObject("EquipBtn");
            equipBtnObj.transform.SetParent(btnTray.transform, false);
            RectTransform ebRt = equipBtnObj.AddComponent<RectTransform>();
            ebRt.anchoredPosition = new Vector2(-120f, 0f);
            ebRt.sizeDelta = new Vector2(280f, 78f);
            Image ebBg = equipBtnObj.AddComponent<Image>();
            ebBg.sprite = SpriteFactory.GetRoundedButtonSprite(new Color(0.98f, 0.72f, 0.15f, 1f), new Color(0.55f, 0.28f, 0.02f, 1f), 0.18f);
            Button ebBtn = equipBtnObj.AddComponent<Button>();

            GameObject ebTxtObj = CreateText(equipBtnObj.transform, Vector2.zero, new Vector2(260f, 70f), "EQUIP NOW", 28, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, m_CandyFont);
            Text ebTxt = ebTxtObj.GetComponent<Text>();
            MakeTextCrisp(ebTxt, new Color(0.45f, 0.22f, 0.02f, 0.95f), 1.1f);

            // Button 2: AWESOME (Cosmic Navy)
            GameObject okBtnObj = new GameObject("AwesomeBtn");
            okBtnObj.transform.SetParent(btnTray.transform, false);
            RectTransform obRt = okBtnObj.AddComponent<RectTransform>();
            obRt.anchoredPosition = new Vector2(170f, 0f);
            obRt.sizeDelta = new Vector2(200f, 78f);
            Image obBg = okBtnObj.AddComponent<Image>();
            obBg.sprite = SpriteFactory.GetRoundedButtonSprite(new Color(0.08f, 0.18f, 0.40f, 1f), new Color(0.20f, 0.45f, 0.85f, 1f), 0.18f);
            Button obBtn = okBtnObj.AddComponent<Button>();

            GameObject obTxtObj = CreateText(okBtnObj.transform, Vector2.zero, new Vector2(180f, 70f), "AWESOME", 24, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, m_CandyFont);
            Text obTxt = obTxtObj.GetComponent<Text>();
            MakeTextCrisp(obTxt, new Color(0.02f, 0.06f, 0.18f, 0.95f), 1.0f);

            System.Action closeAction = () =>
            {
                if (modalObj != null) Destroy(modalObj);
                m_ChestUnboxingModal = null;
                RefreshLevelSelectGrid(GameModeType.Adventure);
            };

            ebBtn.onClick.AddListener(() =>
            {
                SkinManager.EquipSkin(skinType);
                ebTxt.text = "EQUIPPED!";
                ebBtn.interactable = false;
                HapticManager.TriggerLight();
                if (SoundManager.Instance != null) SoundManager.Instance.PlayPlace();
                StartCoroutine(DelayedCloseModal(1.0f, closeAction));
            });

            obBtn.onClick.AddListener(() =>
            {
                if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
                HapticManager.TriggerLight();
                closeAction();
            });
        }

        private System.Collections.IEnumerator DelayedCloseModal(float delay, System.Action callback)
        {
            yield return new WaitForSecondsRealtime(delay);
            callback?.Invoke();
        }

        public void ShowMilestoneRewardCard(string rawReward)
        {
            if (m_RootCanvas == null) return;
            if (m_ToastCoroutine != null)
            {
                StopCoroutine(m_ToastCoroutine);
                m_ToastCoroutine = null;
            }
            if (m_ToastObj != null)
            {
                Destroy(m_ToastObj);
                m_ToastObj = null;
            }

            m_ToastCoroutine = StartCoroutine(AnimateMilestoneRewardCard(rawReward));
        }

        private System.Collections.IEnumerator AnimateMilestoneRewardCard(string rawReward)
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlayAchievement();
            HapticManager.TriggerMedium();

            string mainTitle = rawReward;
            string subTitle = "Special Adventure Milestone!";

            if (rawReward.Contains(":"))
            {
                string[] parts = rawReward.Split(new char[] { ':' }, 2);
                mainTitle = parts[0].Trim();
                subTitle = parts[1].Trim();
            }
            else if (rawReward.EndsWith("Skin!"))
            {
                mainTitle = rawReward.Replace("Skin!", "Ball Skin").Trim();
                subTitle = "Exclusive Cosmic Design!";
            }

            Sprite iconSprite = GemIconFactory.GetTrophySprite();
            if (rawReward.IndexOf("Crown", System.StringComparison.OrdinalIgnoreCase) >= 0 || rawReward.IndexOf("Sovereign", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                iconSprite = GemIconFactory.GetCrownSprite();
            }
            else if (rawReward.IndexOf("Coin", System.StringComparison.OrdinalIgnoreCase) >= 0 || rawReward.IndexOf("Vault", System.StringComparison.OrdinalIgnoreCase) >= 0 || rawReward.IndexOf("Treasury", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                iconSprite = GemIconFactory.GetCoinSprite();
            }
            else if (rawReward.IndexOf("Ball", System.StringComparison.OrdinalIgnoreCase) >= 0 || rawReward.IndexOf("Skin", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                iconSprite = GemIconFactory.GetSparkleIcon3D();
            }

            m_ToastObj = new GameObject("MilestoneRewardCard");
            m_ToastObj.transform.SetParent(m_RootCanvas, false);
            m_ToastObj.transform.SetAsLastSibling();

            RectTransform cardRt = m_ToastObj.AddComponent<RectTransform>();
            cardRt.anchorMin = new Vector2(0.5f, 0.5f);
            cardRt.anchorMax = new Vector2(0.5f, 0.5f);
            cardRt.anchoredPosition = new Vector2(0f, 10f);
            cardRt.sizeDelta = new Vector2(760f, 200f);

            // 1. Soft glowing outer backdrop aura
            GameObject glowObj = new GameObject("OuterGlow");
            glowObj.transform.SetParent(m_ToastObj.transform, false);
            RectTransform glowRt = glowObj.AddComponent<RectTransform>();
            glowRt.sizeDelta = new Vector2(810f, 250f);
            Image glowImg = glowObj.AddComponent<Image>();
            glowImg.sprite = SpriteFactory.GetPillSprite(new Color(0.02f, 0.05f, 0.16f, 0.88f));
            glowImg.raycastTarget = false;

            // 2. High-Definition 3D Royal Golden-Beveled Plaque
            Image bg = m_ToastObj.AddComponent<Image>();
            bg.sprite = SpriteFactory.GetHeaderPlaqueSprite(true);
            bg.type = Image.Type.Sliced;
            bg.color = Color.white;
            bg.raycastTarget = true;

            // Interactive dismiss on tap
            Button cardBtn = m_ToastObj.AddComponent<Button>();
            cardBtn.transition = Selectable.Transition.None;
            cardBtn.onClick.AddListener(() =>
            {
                if (m_ToastCoroutine != null) StopCoroutine(m_ToastCoroutine);
                StartCoroutine(AnimateToastDismiss(m_ToastObj));
            });

            // 3. Left 3D Icon Container
            GameObject iconBox = new GameObject("IconContainer");
            iconBox.transform.SetParent(m_ToastObj.transform, false);
            RectTransform ibRt = iconBox.AddComponent<RectTransform>();
            ibRt.anchoredPosition = new Vector2(-270f, 0f);
            ibRt.sizeDelta = new Vector2(130f, 130f);

            // Radial Aura behind icon
            GameObject auraObj = new GameObject("IconAura");
            auraObj.transform.SetParent(iconBox.transform, false);
            RectTransform aRt = auraObj.AddComponent<RectTransform>();
            aRt.sizeDelta = new Vector2(120f, 120f);
            Image aImg = auraObj.AddComponent<Image>();
            aImg.sprite = SpriteFactory.GetSoftGlowOrbSprite();
            aImg.color = new Color(1.0f, 0.82f, 0.25f, 0.35f);
            aImg.raycastTarget = false;

            // 3D Trophy / Crown Sprite Icon
            GameObject iconObj = new GameObject("RewardIcon");
            iconObj.transform.SetParent(iconBox.transform, false);
            RectTransform iRt = iconObj.AddComponent<RectTransform>();
            iRt.sizeDelta = new Vector2(92f, 92f);
            Image iImg = iconObj.AddComponent<Image>();
            iImg.sprite = iconSprite;
            iImg.preserveAspect = true;
            iImg.raycastTarget = false;

            // Diamond Twinkle Star on corner of icon
            GameObject twinkleObj = new GameObject("IconSparkle");
            twinkleObj.transform.SetParent(iconBox.transform, false);
            RectTransform twRt = twinkleObj.AddComponent<RectTransform>();
            twRt.anchoredPosition = new Vector2(36f, 36f);
            twRt.sizeDelta = new Vector2(34f, 34f);
            Image twImg = twinkleObj.AddComponent<Image>();
            twImg.sprite = GemIconFactory.GetStarSprite(true);
            twImg.color = new Color(1f, 0.95f, 0.70f, 0.95f);
            twImg.raycastTarget = false;

            // 4. Right Content Area (Anchored X: -180 to +350)
            // 4a. Header Category Pill Tag
            GameObject tagPill = new GameObject("HeaderTagPill");
            tagPill.transform.SetParent(m_ToastObj.transform, false);
            RectTransform tpRt = tagPill.AddComponent<RectTransform>();
            tpRt.anchoredPosition = new Vector2(75f, 54f);
            tpRt.sizeDelta = new Vector2(300f, 30f);
            Image tpImg = tagPill.AddComponent<Image>();
            tpImg.sprite = SpriteFactory.Get3DPillBadgeSprite(new Color(0.12f, 0.42f, 0.85f), new Color(0.04f, 0.18f, 0.45f));
            tpImg.type = Image.Type.Sliced;
            tpImg.raycastTarget = false;

            GameObject tagTxtObj = CreateText(tagPill.transform, Vector2.zero, new Vector2(300f, 30f), "MILESTONE REWARD", 18, FontStyle.Normal, new Color(0.65f, 0.94f, 1.0f), TextAnchor.MiddleCenter, m_CandyFont);
            MakeTextCrisp(tagTxtObj.GetComponent<Text>(), new Color(0.01f, 0.05f, 0.20f, 0.95f), 1.2f);

            // 4b. Main Reward Title (Radiant Sun Gold, Razor Sharp, No Faux-Bold Smearing)
            GameObject titleObj = CreateText(m_ToastObj.transform, new Vector2(75f, 12f), new Vector2(490f, 44f), mainTitle.ToUpper(), 30, FontStyle.Normal, new Color(1.0f, 0.92f, 0.42f), TextAnchor.MiddleLeft, m_CandyFont);
            Text titleTxt = titleObj.GetComponent<Text>();
            titleTxt.resizeTextForBestFit = true;
            titleTxt.resizeTextMinSize = 22;
            titleTxt.resizeTextMaxSize = 30;
            MakeTextCrisp(titleTxt, new Color(0.32f, 0.15f, 0.02f, 0.98f), 2.2f);

            // 4c. Subtitle / Description (Clean Ice Cream White)
            GameObject subObj = CreateText(m_ToastObj.transform, new Vector2(75f, -28f), new Vector2(490f, 36f), subTitle, 22, FontStyle.Normal, new Color(0.92f, 0.97f, 1.0f), TextAnchor.MiddleLeft, m_CandyFont);
            Text subTxt = subObj.GetComponent<Text>();
            subTxt.resizeTextForBestFit = true;
            subTxt.resizeTextMinSize = 16;
            subTxt.resizeTextMaxSize = 22;
            MakeTextCrisp(subTxt, new Color(0.02f, 0.08f, 0.24f, 0.95f), 1.5f);

            // 4d. Bottom Tap to Close Hint
            GameObject hintObj = CreateText(m_ToastObj.transform, new Vector2(75f, -62f), new Vector2(490f, 22f), "TAP TO DISMISS", 14, FontStyle.Normal, new Color(1.0f, 1.0f, 1.0f, 0.45f), TextAnchor.MiddleLeft, m_CandyFont);
            MakeTextCrisp(hintObj.GetComponent<Text>(), new Color(0.02f, 0.04f, 0.12f, 0.90f), 1.0f);

            // Juicy Spring Pop In
            m_ToastObj.transform.localScale = Vector3.zero;
            float elapsed = 0f;
            float inDur = 0.22f;
            while (elapsed < inDur)
            {
                elapsed += Time.unscaledDeltaTime;
                float p = elapsed / inDur;
                float s = Mathf.Sin(p * Mathf.PI * 0.5f);
                if (m_ToastObj != null) m_ToastObj.transform.localScale = Vector3.one * s;
                if (twinkleObj != null) twinkleObj.transform.Rotate(0f, 0f, 180f * Time.unscaledDeltaTime);
                yield return null;
            }
            if (m_ToastObj != null) m_ToastObj.transform.localScale = Vector3.one;

            // Dwell on screen
            float stay = 3.5f;
            while (stay > 0f)
            {
                stay -= Time.unscaledDeltaTime;
                if (twinkleObj != null) twinkleObj.transform.Rotate(0f, 0f, 90f * Time.unscaledDeltaTime);
                yield return null;
            }

            if (m_ToastObj != null)
            {
                yield return StartCoroutine(AnimateToastDismiss(m_ToastObj));
            }
            m_ToastCoroutine = null;
        }

        public void ShowToast(string message, float duration = 2.4f)
        {
            if (m_RootCanvas == null) return;
            if (m_ToastCoroutine != null)
            {
                StopCoroutine(m_ToastCoroutine);
                m_ToastCoroutine = null;
            }
            if (m_ToastObj != null)
            {
                Destroy(m_ToastObj);
                m_ToastObj = null;
            }

            m_ToastCoroutine = StartCoroutine(AnimateToast(message, duration));
        }

        private System.Collections.IEnumerator AnimateToast(string message, float duration)
        {
            m_ToastObj = new GameObject("ToastBanner");
            m_ToastObj.transform.SetParent(m_RootCanvas, false);
            m_ToastObj.transform.SetAsLastSibling();

            RectTransform rt = m_ToastObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0f, 30f);
            rt.sizeDelta = new Vector2(740f, 110f);

            // 1. Soft glowing outer shadow
            GameObject glowObj = new GameObject("OuterGlow");
            glowObj.transform.SetParent(m_ToastObj.transform, false);
            RectTransform glowRt = glowObj.AddComponent<RectTransform>();
            glowRt.sizeDelta = new Vector2(780f, 140f);
            Image glowImg = glowObj.AddComponent<Image>();
            glowImg.sprite = SpriteFactory.GetPillSprite(new Color(0.02f, 0.05f, 0.16f, 0.88f));
            glowImg.raycastTarget = false;

            // 2. 3D Beveled Royal Badge
            Image bg = m_ToastObj.AddComponent<Image>();
            bg.sprite = SpriteFactory.Get3DPillBadgeSprite(new Color(0.08f, 0.16f, 0.32f, 0.98f), new Color(0.03f, 0.08f, 0.20f, 1f));
            bg.type = Image.Type.Sliced;
            bg.raycastTarget = true;

            Button dismissBtn = m_ToastObj.AddComponent<Button>();
            dismissBtn.transition = Selectable.Transition.None;
            dismissBtn.onClick.AddListener(() =>
            {
                if (m_ToastCoroutine != null) StopCoroutine(m_ToastCoroutine);
                StartCoroutine(AnimateToastDismiss(m_ToastObj));
            });

            // 3. Icon on left
            GameObject iconObj = new GameObject("ToastIcon");
            iconObj.transform.SetParent(m_ToastObj.transform, false);
            RectTransform iconRt = iconObj.AddComponent<RectTransform>();
            iconRt.anchoredPosition = new Vector2(-305f, 0f);
            iconRt.sizeDelta = new Vector2(56f, 56f);
            Image iconImg = iconObj.AddComponent<Image>();
            bool isLocked = message.IndexOf("locked", System.StringComparison.OrdinalIgnoreCase) >= 0;
            iconImg.sprite = isLocked ? GemIconFactory.GetLockSprite() : GemIconFactory.GetStarSprite(true);
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;

            // 4. Clean crisp text
            GameObject textObj = new GameObject("ToastText");
            textObj.transform.SetParent(m_ToastObj.transform, false);
            RectTransform trt = textObj.AddComponent<RectTransform>();
            trt.anchoredPosition = new Vector2(25f, 0f);
            trt.sizeDelta = new Vector2(610f, 85f);

            Text txt = textObj.AddComponent<Text>();
            txt.font = m_CandyFont != null ? m_CandyFont : m_DefaultFont;
            txt.fontSize = 25;
            txt.fontStyle = FontStyle.Normal; // Strictly Normal to prevent faux-bold contour blurring!
            txt.color = isLocked ? new Color(1.0f, 0.88f, 0.50f) : new Color(0.70f, 0.94f, 1.0f);
            txt.alignment = TextAnchor.MiddleCenter;
            txt.text = message;
            txt.resizeTextForBestFit = true;
            txt.resizeTextMinSize = 18;
            txt.resizeTextMaxSize = 25;
            txt.raycastTarget = false;
            MakeTextCrisp(txt, new Color(0.02f, 0.06f, 0.20f, 0.98f), 1.8f);

            // Pop in
            m_ToastObj.transform.localScale = Vector3.zero;
            float elapsed = 0f;
            float inDur = 0.18f;
            while (elapsed < inDur)
            {
                elapsed += Time.unscaledDeltaTime;
                float p = Mathf.Sin((elapsed / inDur) * Mathf.PI * 0.5f);
                if (m_ToastObj != null) m_ToastObj.transform.localScale = Vector3.one * p;
                yield return null;
            }
            if (m_ToastObj != null) m_ToastObj.transform.localScale = Vector3.one;

            yield return new WaitForSecondsRealtime(duration);

            if (m_ToastObj != null)
            {
                yield return StartCoroutine(AnimateToastDismiss(m_ToastObj));
            }
            m_ToastCoroutine = null;
        }

        private System.Collections.IEnumerator AnimateToastDismiss(GameObject toast)
        {
            if (toast == null) yield break;
            CanvasGroup cg = toast.GetComponent<CanvasGroup>();
            if (cg == null) cg = toast.AddComponent<CanvasGroup>();
            Vector3 startScale = toast.transform.localScale;
            float elapsed = 0f;
            float dur = 0.16f;
            while (elapsed < dur && toast != null)
            {
                elapsed += Time.unscaledDeltaTime;
                float p = elapsed / dur;
                if (cg != null) cg.alpha = 1f - p;
                toast.transform.localScale = startScale * (1f - p * 0.25f);
                yield return null;
            }
            if (toast != null) Destroy(toast);
            if (m_ToastObj == toast) m_ToastObj = null;
        }

        private void ScrollToActiveLevel(GameModeType mode)
        {
            if (m_LevelScrollRect == null) return;
            int maxLvl = Mathf.Max(40, LevelManager.GetMaxLevel(mode));
            int currentPlayLevel = 1;
            for (int l = 1; l <= maxLvl; l++)
            {
                if (LevelManager.IsLevelUnlocked(mode, l))
                {
                    currentPlayLevel = l;
                    if (LevelManager.GetLevelStars(mode, l) == 0) break;
                }
                else break;
            }

            float py = 130f + (currentPlayLevel - 1) * 92f;
            float totalHeight = 3850f;
            float viewportHeight = 800f;
            float maxScrollY = totalHeight - viewportHeight; // 3050f
            float targetScrollY = Mathf.Clamp(py - viewportHeight * 0.5f, 0f, maxScrollY);
            float targetNorm = Mathf.Clamp01(targetScrollY / maxScrollY);

            StopCoroutine("AnimateScrollToLevel");
            StartCoroutine(AnimateScrollToLevel(targetNorm));
        }

        private System.Collections.IEnumerator AnimateScrollToLevel(float targetNorm)
        {
            if (m_LevelScrollRect == null) yield break;
            yield return null; // Wait 1 frame for UI layout
            float startNorm = m_LevelScrollRect.verticalNormalizedPosition;
            float elapsed = 0f;
            float duration = 0.5f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float ease = Mathf.SmoothStep(0f, 1f, t);
                if (m_LevelScrollRect != null)
                {
                    m_LevelScrollRect.verticalNormalizedPosition = Mathf.Lerp(startNorm, targetNorm, ease);
                }
                yield return null;
            }
            if (m_LevelScrollRect != null)
            {
                m_LevelScrollRect.verticalNormalizedPosition = targetNorm;
            }
        }

        private void BuildVictoryModal()
        {
            m_VictoryModal = new GameObject("VictoryModal", typeof(RectTransform));
            m_VictoryModal.transform.SetParent(m_RootCanvas, false);
            RectTransform rt = m_VictoryModal.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.sizeDelta = Vector2.zero;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            // Fullscreen backdrop matching background deep space navy (blocks raycasts to board)
            Image bg = m_VictoryModal.AddComponent<Image>();
            bg.color = new Color(0.04f, 0.07f, 0.16f, 0.98f);
            bg.raycastTarget = true;
            // Backdrop intentionally does not dismiss on tap to prevent accidental closure into frozen state

            // Centered 3D Scene Viewport Card (768 x 1376 native aspect ratio matching Concept 1)
            GameObject sceneObj = new GameObject("VictorySceneCard", typeof(RectTransform));
            sceneObj.transform.SetParent(m_VictoryModal.transform, false);
            RectTransform sceneRt = sceneObj.GetComponent<RectTransform>();
            sceneRt.anchorMin = new Vector2(0.5f, 0.5f);
            sceneRt.anchorMax = new Vector2(0.5f, 0.5f);
            sceneRt.pivot = new Vector2(0.5f, 0.5f);
            sceneRt.anchoredPosition = Vector2.zero;
            sceneRt.sizeDelta = new Vector2(768, 1376);

            // 1. High-Definition 3D Royal Vault Render Artwork (Concept 1 full page)
            Image sceneImg = sceneObj.AddComponent<Image>();
            sceneImg.sprite = SpriteFactory.LoadOrFallbackSprite("VictoryBackground_Concept1");
            sceneImg.color = Color.white;
            sceneImg.raycastTarget = true;

            // 2. Candy 3D Ruby Red Close (X) Button (placed directly over painted red X at X: 296, Y: 426)
            GameObject closeObj = new GameObject("VictoryCloseBtn", typeof(RectTransform));
            closeObj.transform.SetParent(sceneObj.transform, false);
            RectTransform closeRt = closeObj.GetComponent<RectTransform>();
            closeRt.anchoredPosition = new Vector2(296f, 426f);
            closeRt.sizeDelta = new Vector2(90f, 90f);

            Image closeImg = closeObj.AddComponent<Image>();
            closeImg.sprite = SpriteFactory.LoadOrFallbackSprite("Victory_Btn_CloseX");
            closeImg.color = (closeImg.sprite != null) ? Color.white : Color.clear;
            closeImg.raycastTarget = true;

            Button closeBtn = closeObj.AddComponent<Button>();
            closeBtn.onClick.AddListener(() =>
            {
                StartCoroutine(AnimateButtonPress(closeRt, () =>
                {
                    if (m_VictoryModal != null) m_VictoryModal.SetActive(false);
                    if (LevelManager.Instance != null)
                    {
                        LevelManager.Instance.StopLevel();
                        OpenLevelSelect(LevelManager.Instance.CurrentMode);
                    }
                    else
                    {
                        OpenModeSelect();
                    }
                }));
            });
            closeObj.transform.SetAsLastSibling();

            // 3. Subtitle Text Pill ("Earned 3 Stars! Level X Unlocked") inside golden ribbon banner
            GameObject subPillObj = new GameObject("SubtitlePill", typeof(RectTransform));
            subPillObj.transform.SetParent(sceneObj.transform, false);
            RectTransform spRt = subPillObj.GetComponent<RectTransform>();
            spRt.anchoredPosition = new Vector2(0f, -81.5f);
            spRt.sizeDelta = new Vector2(560f, 44f);
            Image spImg = subPillObj.AddComponent<Image>();
            spImg.color = Color.clear; // Golden ribbon is already illustrated on the artwork
            spImg.raycastTarget = false;

            GameObject subObj = CreateText(subPillObj.transform, Vector2.zero, new Vector2(540f, 40f), "", 24, FontStyle.Normal, new Color(0.20f, 0.08f, 0.01f, 1f), TextAnchor.MiddleCenter, m_CandyFont);
            m_VictorySubtitleText = subObj.GetComponent<Text>();
            m_VictorySubtitleText.resizeTextForBestFit = true;
            m_VictorySubtitleText.resizeTextMinSize = 16;
            m_VictorySubtitleText.resizeTextMaxSize = 25;

            // Remove blurry light shadow that causes ghosting / muddy halo on golden ribbon
            Shadow oldShadow = m_VictorySubtitleText.GetComponent<Shadow>();
            if (oldShadow != null) DestroyImmediate(oldShadow);
            Outline oldOutline = m_VictorySubtitleText.GetComponent<Outline>();
            if (oldOutline != null) DestroyImmediate(oldOutline);

            // 4. Ball Skin Reward Showcase Card (Appears when an Adventure level awards a new ball design!)
            m_VictorySkinRewardObj = new GameObject("SkinRewardCard", typeof(RectTransform));
            m_VictorySkinRewardObj.transform.SetParent(sceneObj.transform, false);
            RectTransform rwRt = m_VictorySkinRewardObj.GetComponent<RectTransform>();
            rwRt.anchoredPosition = new Vector2(0, 50);
            rwRt.sizeDelta = new Vector2(560, 84);
            Image rwBg = m_VictorySkinRewardObj.AddComponent<Image>();
            rwBg.sprite = SpriteFactory.GetPanelSprite(new Color(0.10f, 0.18f, 0.38f, 0.98f), new Color(0.35f, 0.65f, 1f, 1f), 2f, 16);
            rwBg.type = Image.Type.Sliced;

            // Reward Ball Icon Preview (Left)
            GameObject pIconObj = new GameObject("RewardIcon");
            pIconObj.transform.SetParent(m_VictorySkinRewardObj.transform, false);
            RectTransform pIconRt = pIconObj.AddComponent<RectTransform>();
            pIconRt.anchoredPosition = new Vector2(-215, 0);
            pIconRt.sizeDelta = new Vector2(64, 64);
            m_VictorySkinPreviewImg = pIconObj.AddComponent<Image>();
            m_VictorySkinPreviewImg.raycastTarget = false;

            // Reward Subtitle & Name
            CreateText(m_VictorySkinRewardObj.transform, new Vector2(0, 16), new Vector2(300, 26), "NEW BALL DESIGN UNLOCKED!", 18, FontStyle.Bold, new Color(1f, 0.85f, 0.25f), TextAnchor.MiddleCenter, m_CandyFont);
            GameObject nameTxtObj = CreateText(m_VictorySkinRewardObj.transform, new Vector2(0, -14), new Vector2(300, 26), "STAR CORE BALL", 18, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, m_CandyFont);
            m_VictorySkinNameText = nameTxtObj.GetComponent<Text>();

            // Equip Button (Right)
            GameObject equipBtn = CreateButton(m_VictorySkinRewardObj.transform, new Vector2(205, 0), new Vector2(135, 52), new Color(0.12f, 0.68f, 0.35f), "EQUIP", 18);
            m_VictorySkinEquipBtn = equipBtn.GetComponent<Button>();
            m_VictorySkinEquipBtnText = equipBtn.GetComponentInChildren<Text>();
            m_VictorySkinRewardObj.SetActive(false);

            // 5. 3D Arcade Buttons (2x2 Grid Layout matching Concept 2)
            // Button 1: NEXT LEVEL (Green button at Top-Left X: -126, Y: -248)
            m_VictoryNextBtnObj = CreateVictoryButton(sceneObj.transform, new Vector2(-126f, -248f), new Vector2(216f, 114f), () =>
            {
                m_VictoryModal.SetActive(false);
                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.NextLevel();
                }
                UpdateHUDVisibility();
            }, "Victory_Btn_NextLevel");

            // Disabled overlay for Level 40 / final level
            GameObject nextDisabledObj = new GameObject("Victory_NextBtn_DisabledOverlay");
            nextDisabledObj.transform.SetParent(sceneObj.transform, false);
            RectTransform ndRt = nextDisabledObj.AddComponent<RectTransform>();
            ndRt.anchoredPosition = new Vector2(-126f, -248f);
            ndRt.sizeDelta = new Vector2(216f, 114f);
            Image ndImg = nextDisabledObj.AddComponent<Image>();
            ndImg.sprite = SpriteFactory.GetPillSprite(new Color(0.04f, 0.08f, 0.16f, 0.72f));
            ndImg.type = Image.Type.Sliced;
            ndImg.raycastTarget = true;
            GameObject ndTxtObj = CreateText(nextDisabledObj.transform, Vector2.zero, new Vector2(200, 50), "COMPLETED", 20, FontStyle.Bold, new Color(0.75f, 0.82f, 0.95f, 0.88f), TextAnchor.MiddleCenter, m_CandyFont);
            MakeTextCrisp(ndTxtObj.GetComponent<Text>(), new Color(0.02f, 0.04f, 0.10f, 0.95f), 1.0f);
            m_VictoryNextDisabledOverlay = nextDisabledObj;
            m_VictoryNextDisabledOverlay.SetActive(false);

            // Button 2: LEVEL SELECT (Blue button at Top-Right X: 124, Y: -248)
            CreateVictoryButton(sceneObj.transform, new Vector2(124f, -248f), new Vector2(216f, 114f), () =>
            {
                m_VictoryModal.SetActive(false);
                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.StopLevel();
                    OpenLevelSelect(LevelManager.Instance.CurrentMode);
                }
            }, "Victory_Btn_LevelSelect");

            // Button 3: BALL DESIGNS (Purple button at Bottom-Left X: -126, Y: -410)
            CreateVictoryButton(sceneObj.transform, new Vector2(-126f, -410f), new Vector2(216f, 114f), () =>
            {
                m_VictoryModal.SetActive(false);
                OpenSkinWardrobe();
            }, "Victory_Btn_BallDesigns");

            // Button 4: HOME (Orange button at Bottom-Right X: 124, Y: -410)
            CreateVictoryButton(sceneObj.transform, new Vector2(124f, -410f), new Vector2(216f, 114f), () =>
            {
                m_VictoryModal.SetActive(false);
                if (LevelManager.Instance != null) LevelManager.Instance.StopLevel();
                OnHomeClicked();
            }, "Victory_Btn_Home");

            m_VictoryModal.SetActive(false);
        }

        private GameObject CreateVictoryButton(Transform parent, Vector2 anchoredPos, Vector2 size, System.Action onClick, string name = "Button")
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            Image img = obj.AddComponent<Image>();
            img.sprite = SpriteFactory.LoadOrFallbackSprite(name);
            img.color = (img.sprite != null) ? Color.white : Color.clear;
            img.raycastTarget = true;

            Button btn = obj.AddComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                StartCoroutine(AnimateButtonPress(rt, onClick));
            });

            return obj;
        }

        private GameObject CreateTransparentButton(Transform parent, Vector2 anchoredPos, Vector2 size, System.Action onClick, string name = "Button")
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            Image img = obj.AddComponent<Image>();
            img.color = Color.clear;
            img.raycastTarget = true;

            Button btn = obj.AddComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                StartCoroutine(AnimateButtonPress(rt, onClick));
            });

            return obj;
        }

        private void BuildDefeatModal()
        {
            m_DefeatModal = new GameObject("DefeatModal", typeof(RectTransform));
            m_DefeatModal.transform.SetParent(m_RootCanvas, false);
            RectTransform rt = m_DefeatModal.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.sizeDelta = Vector2.zero;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            // Fullscreen backdrop matching deep space cosmic navy (blocks raycasts to board)
            Image bg = m_DefeatModal.AddComponent<Image>();
            bg.color = new Color(0.04f, 0.07f, 0.16f, 0.98f);
            bg.raycastTarget = true;
            // Backdrop intentionally does not dismiss on tap to prevent accidental closure into frozen 0-moves state

            // Centered 3D Scene Viewport Card (768 x 1376 native aspect ratio matching Concept 1)
            GameObject sceneObj = new GameObject("DefeatSceneCard", typeof(RectTransform));
            sceneObj.transform.SetParent(m_DefeatModal.transform, false);
            RectTransform sceneRt = sceneObj.GetComponent<RectTransform>();
            sceneRt.anchorMin = new Vector2(0.5f, 0.5f);
            sceneRt.anchorMax = new Vector2(0.5f, 0.5f);
            sceneRt.pivot = new Vector2(0.5f, 0.5f);
            sceneRt.anchoredPosition = Vector2.zero;
            sceneRt.sizeDelta = new Vector2(768, 1376);

            // 1. High-Definition 3D Defeat Render Artwork (Concept 1)
            Image sceneImg = sceneObj.AddComponent<Image>();
            sceneImg.sprite = SpriteFactory.LoadOrFallbackSprite("DefeatBackground_Concept1");
            sceneImg.color = Color.white;
            sceneImg.raycastTarget = true;

            // 2. Candy 3D Ruby Red Close (X) Button (placed directly over painted red X at X: 255, Y: 450)
            GameObject closeObj = new GameObject("DefeatCloseBtn", typeof(RectTransform));
            closeObj.transform.SetParent(sceneObj.transform, false);
            RectTransform closeRt = closeObj.GetComponent<RectTransform>();
            closeRt.anchoredPosition = new Vector2(255f, 450f);
            closeRt.sizeDelta = new Vector2(88f, 88f);

            Image closeImg = closeObj.AddComponent<Image>();
            closeImg.color = Color.clear;
            closeImg.raycastTarget = true;

            Button closeBtn = closeObj.AddComponent<Button>();
            closeBtn.onClick.AddListener(() =>
            {
                StartCoroutine(AnimateButtonPress(closeRt, () =>
                {
                    if (m_DefeatModal != null) m_DefeatModal.SetActive(false);
                    if (LevelManager.Instance != null)
                    {
                        LevelManager.Instance.StopLevel();
                        OpenLevelSelect(LevelManager.Instance.CurrentMode);
                    }
                    else
                    {
                        OpenModeSelect();
                    }
                }));
            });
            closeObj.transform.SetAsLastSibling();

            // 3. Subtitle Text Overlay (Centered on subtitle pill at Y: 205)
            GameObject subObj = CreateText(sceneObj.transform, new Vector2(0, 205), new Vector2(540, 50), "You ran out of moves. Don't give up! Try again.", 23, FontStyle.Normal, new Color(0.95f, 0.96f, 1f), TextAnchor.MiddleCenter, m_CandyFont);
            m_DefeatSubtitleText = subObj.GetComponent<Text>();
            MakeTextCrisp(m_DefeatSubtitleText, new Color(0.04f, 0.08f, 0.22f, 0.95f), 1.6f);

            // 4. Button 1: +5 MOVES (Gold button at Y: -95)
            m_DefeatExtraMovesBtn = CreateTransparentButton(sceneObj.transform, new Vector2(0, -95), new Vector2(400, 75), () =>
            {
                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.AddExtraMoves(5);
                }
            }, "Defeat_Btn_ExtraMoves");

            // Dynamic text on top of +5 MOVES button
            GameObject extraTxtObj = CreateText(m_DefeatExtraMovesBtn.transform, Vector2.zero, new Vector2(380, 50), "+5 MOVES (3 LEFT)", 27, FontStyle.Normal, Color.white, TextAnchor.MiddleCenter, m_CandyFont);
            m_DefeatExtraMovesText = extraTxtObj.GetComponent<Text>();
            MakeTextCrisp(m_DefeatExtraMovesText, new Color(0.60f, 0.35f, 0.02f, 0.95f), 1.8f);

            // 5. Button 2: RETRY LEVEL (Coral button at Y: -190)
            m_DefeatRetryBtn = CreateTransparentButton(sceneObj.transform, new Vector2(0, -190), new Vector2(400, 75), () =>
            {
                m_DefeatModal.SetActive(false);
                if (LevelManager.Instance != null) LevelManager.Instance.RestartCurrentLevel();
            }, "Defeat_Btn_Retry");

            // 6. Button 3: LEVEL SELECT (Blue button at Y: -285)
            m_DefeatLvlSelectBtn = CreateTransparentButton(sceneObj.transform, new Vector2(0, -285), new Vector2(400, 75), () =>
            {
                m_DefeatModal.SetActive(false);
                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.StopLevel();
                    OpenLevelSelect(LevelManager.Instance.CurrentMode);
                }
            }, "Defeat_Btn_LevelSelect");

            // 7. Button 4: MAIN MENU (Navy button at Y: -380)
            m_DefeatModeBtn = CreateTransparentButton(sceneObj.transform, new Vector2(0, -380), new Vector2(400, 75), () =>
            {
                m_DefeatModal.SetActive(false);
                if (LevelManager.Instance != null) LevelManager.Instance.StopLevel();
                OpenModeSelect();
            }, "Defeat_Btn_MainMenu");

            m_DefeatModal.SetActive(false);
        }

        // ==========================================
        // BALL DESIGNS WARDROBE MODAL
        // ==========================================
        public void OpenSkinWardrobe()
        {
            if (m_SkinWardrobeModal == null) BuildSkinWardrobeModal();
            RefreshSkinWardrobeGrid();
            if (m_SkinWardrobeModal != null)
            {
                m_SkinWardrobeModal.SetActive(true);
                m_SkinWardrobeModal.transform.SetAsLastSibling();

                Transform scene = m_SkinWardrobeModal.transform.Find("WardrobeSceneCard");
                if (scene != null)
                {
                    float canvasHeight = 1920f;
                    float canvasWidth = 1080f;
                    if (m_RootCanvas != null)
                    {
                        RectTransform cRt = m_RootCanvas.GetComponent<RectTransform>();
                        if (cRt != null && cRt.rect.height > 0) canvasHeight = cRt.rect.height;
                        if (cRt != null && cRt.rect.width > 0) canvasWidth = cRt.rect.width;
                    }
                    float fitScale = canvasHeight / 1376f;
                    if (768f * fitScale > canvasWidth)
                    {
                        fitScale = canvasWidth / 768f;
                    }
                    scene.localScale = new Vector3(fitScale, fitScale, 1f);

                    StopCoroutine("AnimateModalOpen");
                    StartCoroutine(AnimateModalOpen(scene.GetComponent<RectTransform>(), fitScale));
                }
            }
        }

        private void BuildSkinWardrobeModal()
        {
            m_SkinWardrobeModal = new GameObject("SkinWardrobeModal", typeof(RectTransform));
            m_SkinWardrobeModal.transform.SetParent(m_RootCanvas, false);
            RectTransform rt = m_SkinWardrobeModal.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Fullscreen backdrop matching background deep space navy
            Image bg = m_SkinWardrobeModal.AddComponent<Image>();
            bg.color = new Color(0.04f, 0.07f, 0.16f, 0.98f);
            bg.raycastTarget = true;
            Button bgBtn = m_SkinWardrobeModal.AddComponent<Button>();
            bgBtn.onClick.AddListener(CloseSkinWardrobe);

            // Centered 3D Scene Viewport Card (768 x 1376 native aspect ratio matching Concept 1)
            GameObject sceneObj = new GameObject("WardrobeSceneCard", typeof(RectTransform));
            sceneObj.transform.SetParent(m_SkinWardrobeModal.transform, false);
            RectTransform sceneRt = sceneObj.GetComponent<RectTransform>();
            sceneRt.anchorMin = new Vector2(0.5f, 0.5f);
            sceneRt.anchorMax = new Vector2(0.5f, 0.5f);
            sceneRt.pivot = new Vector2(0.5f, 0.5f);
            sceneRt.anchoredPosition = Vector2.zero;
            sceneRt.sizeDelta = new Vector2(768, 1376);

            // Dynamically scale sceneRt to fit screen height perfectly
            float canvasHeight = 1920f;
            float canvasWidth = 1080f;
            if (m_RootCanvas != null)
            {
                RectTransform cRt = m_RootCanvas.GetComponent<RectTransform>();
                if (cRt != null && cRt.rect.height > 0) canvasHeight = cRt.rect.height;
                if (cRt != null && cRt.rect.width > 0) canvasWidth = cRt.rect.width;
            }
            float fitScale = canvasHeight / 1376f;
            if (768f * fitScale > canvasWidth)
            {
                fitScale = canvasWidth / 768f;
            }
            sceneRt.localScale = new Vector3(fitScale, fitScale, 1f);

            // 1. High-Definition 3D Royal Vault Render Artwork (Concept 1)
            Image sceneImg = sceneObj.AddComponent<Image>();
            sceneImg.sprite = SpriteFactory.LoadOrFallbackSprite("WardrobeBackground_Concept1");
            sceneImg.color = Color.white;
            sceneImg.raycastTarget = true;

            // 2. Candy 3D Ruby Red Close (X) Button (placed directly over painted red X at X: 275, Y: 408)
            GameObject closeObj = new GameObject("WardrobeCloseBtn", typeof(RectTransform));
            closeObj.transform.SetParent(sceneObj.transform, false);
            RectTransform closeRt = closeObj.GetComponent<RectTransform>();
            closeRt.anchoredPosition = new Vector2(275f, 408f);
            closeRt.sizeDelta = new Vector2(96f, 96f);

            Image closeImg = closeObj.AddComponent<Image>();
            closeImg.color = Color.clear; // Clear so painted red 3D 'X' is fully visible without dark square artifact
            closeImg.raycastTarget = true;

            Button closeBtn = closeObj.AddComponent<Button>();
            closeBtn.onClick.AddListener(() =>
            {
                StartCoroutine(AnimateButtonPress(closeRt, CloseSkinWardrobe));
            });
            closeObj.transform.SetAsLastSibling();

            // 3. Grid Container for 3x3 Cards
            GameObject gridObj = new GameObject("SkinsGridContainer", typeof(RectTransform));
            gridObj.transform.SetParent(sceneObj.transform, false);
            RectTransform gRt = gridObj.GetComponent<RectTransform>();
            gRt.anchoredPosition = new Vector2(0, -60);
            gRt.sizeDelta = new Vector2(580, 800);
            m_SkinGridContainer = gridObj.transform;

            m_SkinWardrobeModal.SetActive(false);
        }

        public void CloseSkinWardrobe()
        {
            if (m_SkinWardrobeModal != null)
            {
                m_SkinWardrobeModal.SetActive(false);
            }

            // Immediately refresh home preview
            UpdateHomeEquippedBallPreview();

            // Immediately refresh board cells and spawner blocks with the equipped skin
            if (GridManager.Instance != null)
            {
                GridManager.Instance.RefreshAllCellSprites();
            }

            if (ShapeSpawner.Instance != null)
            {
                ShapeSpawner.Instance.RefreshActiveShapeSprites();
            }

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayButtonClick();
            }
        }

        private void HandleSkinChangedForHome(BallSkinType newSkin)
        {
            UpdateHomeEquippedBallPreview();
        }

        private void UpdateHomeEquippedBallPreview()
        {
            BallSkinType skinType = SkinManager.CurrentSkin;
            SkinInfo info = SkinManager.GetSkinInfo(skinType);
            Sprite skinSprite = SpriteFactory.GetWardrobeSkinSprite(skinType);

            if (m_HomeEquippedBallIcon != null)
            {
                m_HomeEquippedBallIcon.sprite = skinSprite;
            }
            if (m_HomeEquippedBallNameText != null)
            {
                m_HomeEquippedBallNameText.text = info != null ? info.Name.ToUpper() : "CLASSIC";
            }
            if (m_HomeCenterBallImage != null)
            {
                if (skinType != BallSkinType.Classic)
                {
                    m_HomeCenterBallImage.sprite = skinSprite;
                    m_HomeCenterBallImage.color = Color.white;
                    if (m_HomeCenterBallShowcase != null) m_HomeCenterBallShowcase.SetActive(true);
                }
                else
                {
                    // In Classic mode, let the native 5-point rainbow star of Concept 3 shine through cleanly
                    m_HomeCenterBallImage.color = new Color(1f, 1f, 1f, 0f);
                }
            }
        }

        private void RefreshSkinWardrobeGrid()
        {
            if (m_SkinGridContainer == null) return;

            for (int i = m_SkinGridContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(m_SkinGridContainer.GetChild(i).gameObject);
            }

            List<SkinInfo> allSkins = SkinManager.GetAllSkins();
            BallSkinType equipped = SkinManager.CurrentSkin;

            float[] colX = new float[] { -178f, 0f, 178f };
            float[] rowY = new float[] { 236f, -18f, -272f };

            int maxDisplay = Mathf.Min(9, allSkins.Count);
            for (int i = 0; i < maxDisplay; i++)
            {
                SkinInfo skin = allSkins[i];
                int col = i % 3;
                int row = i / 3;
                Vector2 pos = new Vector2(colX[col], rowY[row]);

                bool isUnlocked = SkinManager.IsSkinUnlocked(skin.Type);
                bool isEquipped = (equipped == skin.Type);

                // Slot Card
                GameObject card = new GameObject($"SkinCard_{skin.Type}");
                card.transform.SetParent(m_SkinGridContainer, false);
                RectTransform cRt = card.AddComponent<RectTransform>();
                cRt.anchoredPosition = pos;
                cRt.sizeDelta = new Vector2(166, 236);

                Image cBg = card.AddComponent<Image>();
                Color bgColor = isEquipped
                    ? new Color(0.06f, 0.20f, 0.44f, 0.96f)
                    : (isUnlocked ? new Color(0.04f, 0.11f, 0.26f, 0.92f) : new Color(0.03f, 0.07f, 0.16f, 0.85f));
                Color borderColor = isEquipped
                    ? new Color(0.20f, 0.95f, 0.98f, 1f)
                    : (isUnlocked ? new Color(0.22f, 0.45f, 0.85f, 0.75f) : new Color(0.16f, 0.24f, 0.38f, 0.55f));
                cBg.sprite = SpriteFactory.GetRoundedButtonSprite(bgColor, borderColor, 0.08f);
                cBg.raycastTarget = true;

                // 1. Title Text at top - Bold crisp title with shadow for high contrast & legibility
                Color titleCol = isEquipped ? new Color(1.0f, 0.92f, 0.45f) : (isUnlocked ? Color.white : new Color(0.75f, 0.82f, 0.95f));
                GameObject titleObj = CreateText(card.transform, new Vector2(0, 78), new Vector2(156, 30), skin.Name.ToUpper(), 16, FontStyle.Bold, titleCol, TextAnchor.MiddleCenter, m_CandyFont);
                Text titleTxt = titleObj.GetComponent<Text>();
                MakeTextCrisp(titleTxt, new Color(0.02f, 0.05f, 0.16f, 0.98f), 1.2f);

                // 2. 3D Sphere Icon in center (Size: 96 x 96)
                GameObject iconObj = new GameObject("BallIcon");
                iconObj.transform.SetParent(card.transform, false);
                RectTransform iconRt = iconObj.AddComponent<RectTransform>();
                iconRt.anchoredPosition = new Vector2(0, 14);
                iconRt.sizeDelta = new Vector2(96, 96);
                Image iconImg = iconObj.AddComponent<Image>();
                iconImg.sprite = SpriteFactory.GetWardrobeSkinSprite(skin.Type);
                iconImg.color = isUnlocked ? Color.white : new Color(0.65f, 0.70f, 0.82f, 0.60f);
                iconImg.raycastTarget = false;

                // 3. Bottom Action Area (Y: -78)
                BallSkinType capturedType = skin.Type;
                if (isEquipped)
                {
                    // Green 3D "EQUIPPED âœ“" Button
                    GameObject btnObj = new GameObject("EquippedBadge");
                    btnObj.transform.SetParent(card.transform, false);
                    RectTransform bRt = btnObj.AddComponent<RectTransform>();
                    bRt.anchoredPosition = new Vector2(0, -78);
                    bRt.sizeDelta = new Vector2(136, 42);
                    Image bImg = btnObj.AddComponent<Image>();
                    bImg.sprite = SpriteFactory.LoadOrFallbackSprite("Wardrobe_Btn_Equipped");
                    bImg.color = Color.white;
                    bImg.raycastTarget = false;
                }
                else if (isUnlocked)
                {
                    // Blue 3D "EQUIP" Button
                    GameObject btnObj = new GameObject("EquipBtn");
                    btnObj.transform.SetParent(card.transform, false);
                    RectTransform bRt = btnObj.AddComponent<RectTransform>();
                    bRt.anchoredPosition = new Vector2(0, -78);
                    bRt.sizeDelta = new Vector2(130, 42);
                    Image bImg = btnObj.AddComponent<Image>();
                    bImg.sprite = SpriteFactory.LoadOrFallbackSprite("Wardrobe_Btn_Equip");
                    bImg.color = Color.white;
                    bImg.raycastTarget = true;

                    Button btn = btnObj.AddComponent<Button>();
                    btn.onClick.AddListener(() =>
                    {
                        StartCoroutine(AnimateButtonPress(cRt, () =>
                        {
                            SkinManager.EquipSkin(capturedType);
                            RefreshSkinWardrobeGrid();
                        }));
                    });

                    // Tapping anywhere on the card or ball icon also equips this ball design!
                    Button cardBtn = card.AddComponent<Button>();
                    cardBtn.onClick.AddListener(() =>
                    {
                        StartCoroutine(AnimateButtonPress(cRt, () =>
                        {
                            SkinManager.EquipSkin(capturedType);
                            RefreshSkinWardrobeGrid();
                        }));
                    });
                }
                else
                {
                    // Golden 3D Padlock + Level requirement
                    GameObject lockContainer = new GameObject("LockContainer");
                    lockContainer.transform.SetParent(card.transform, false);
                    RectTransform lcRt = lockContainer.AddComponent<RectTransform>();
                    lcRt.anchoredPosition = new Vector2(0, -78);
                    lcRt.sizeDelta = new Vector2(130, 42);

                    GameObject padObj = new GameObject("PadlockIcon");
                    padObj.transform.SetParent(lockContainer.transform, false);
                    RectTransform pRt = padObj.AddComponent<RectTransform>();
                    pRt.anchoredPosition = new Vector2(-28, 0);
                    pRt.sizeDelta = new Vector2(28, 34);
                    Image pImg = padObj.AddComponent<Image>();
                    pImg.sprite = SpriteFactory.LoadOrFallbackSprite("Wardrobe_Icon_Lock");
                    pImg.color = Color.white;
                    pImg.raycastTarget = false;

                    CreateText(lockContainer.transform, new Vector2(16, 0), new Vector2(76, 28), $"Lvl {skin.UnlockAdventureLevel}", 15, FontStyle.Bold, new Color(1f, 0.84f, 0.30f), TextAnchor.MiddleLeft);

                    // Click card for hint
                    Button lockBtn = card.AddComponent<Button>();
                    lockBtn.onClick.AddListener(() =>
                    {
                        HapticManager.TriggerLight();
                        FloatingText.CreateBadge(card.transform.position, $"Unlock at Adventure Level {skin.UnlockAdventureLevel}!", Color.white, new Color(0.85f, 0.25f, 0.25f));
                    });
                }
            }
        }

        private void BuildClassicGameOverModal()
        {
            m_GameOverPanel = new GameObject("GameOverPanel", typeof(RectTransform));
            m_GameOverPanel.transform.SetParent(m_RootCanvas, false);
            RectTransform panelRt = m_GameOverPanel.GetComponent<RectTransform>();
            panelRt.anchorMin = Vector2.zero; panelRt.anchorMax = Vector2.one; panelRt.sizeDelta = Vector2.zero;
            panelRt.offsetMin = Vector2.zero; panelRt.offsetMax = Vector2.zero;

            // Fullscreen backdrop matching background deep space navy (blocks raycasts to board)
            Image panelImg = m_GameOverPanel.AddComponent<Image>();
            panelImg.color = new Color(0.04f, 0.07f, 0.16f, 0.98f);
            panelImg.raycastTarget = true;

            // Centered 3D Royal Baroque Scene Viewport Card: 768 x 1376 native aspect ratio (exactly like SettingsSceneCard)
            GameObject sceneObj = new GameObject("GameOverSceneCard", typeof(RectTransform));
            sceneObj.transform.SetParent(m_GameOverPanel.transform, false);
            RectTransform sceneRt = sceneObj.GetComponent<RectTransform>();
            sceneRt.anchorMin = new Vector2(0.5f, 0.5f);
            sceneRt.anchorMax = new Vector2(0.5f, 0.5f);
            sceneRt.pivot = new Vector2(0.5f, 0.5f);
            sceneRt.anchoredPosition = Vector2.zero;
            sceneRt.sizeDelta = new Vector2(768f, 1376f);

            // 1. High-Definition 3D Royal Baroque GameOver Render Artwork
            Image sceneImg = sceneObj.AddComponent<Image>();
            sceneImg.sprite = SpriteFactory.LoadOrFallbackSprite("GameOverBackground_Concept1");
            sceneImg.color = Color.white;
            sceneImg.raycastTarget = true;

            // 2. Score Plaque Overlays (Centered on royal blue glass card)
            // Final Score Header Label (Sky Cyan) - Large, sharp, clear
            GameObject scoreLabel = CreateText(sceneObj.transform, new Vector2(0f, 192f), new Vector2(400f, 48f), "FINAL SCORE", 34, FontStyle.Normal, new Color(0.65f, 0.92f, 1.0f), TextAnchor.MiddleCenter, m_CandyFont);
            MakeTextCrisp(scoreLabel.GetComponent<Text>(), new Color(0.02f, 0.08f, 0.28f, 0.95f), 2.0f);

            // Huge Sparkling Score Digits (Razor sharp native resolution)
            GameObject scoreVal = CreateText(sceneObj.transform, new Vector2(0f, 122f), new Vector2(460f, 110f), "0", 96, FontStyle.Normal, new Color(1.0f, 0.95f, 0.72f), TextAnchor.MiddleCenter, m_CandyFont);
            m_GameOverScoreText = scoreVal.GetComponent<Text>();
            MakeTextCrisp(m_GameOverScoreText, new Color(0.04f, 0.08f, 0.26f, 0.98f), 3.0f);

            // Best Score Pill Row (with Golden Trophy)
            GameObject bestPillObj = new GameObject("BestScoreRow");
            bestPillObj.transform.SetParent(sceneObj.transform, false);
            RectTransform bpRt = bestPillObj.AddComponent<RectTransform>();
            bpRt.anchoredPosition = new Vector2(0f, 48f);
            bpRt.sizeDelta = new Vector2(360f, 48f);

            // Trophy Icon
            GameObject trophyObj = new GameObject("TrophyIcon");
            trophyObj.transform.SetParent(bestPillObj.transform, false);
            RectTransform trRt = trophyObj.AddComponent<RectTransform>();
            trRt.anchoredPosition = new Vector2(-110f, 0f);
            trRt.sizeDelta = new Vector2(42f, 42f);
            Image trImg = trophyObj.AddComponent<Image>();
            trImg.sprite = GemIconFactory.GetTrophySprite();
            trImg.raycastTarget = false;

            // Best Score Text
            GameObject bestVal = CreateText(bestPillObj.transform, new Vector2(16f, 0f), new Vector2(260f, 46f), "BEST: 0", 32, FontStyle.Normal, new Color(1.0f, 0.88f, 0.25f), TextAnchor.MiddleCenter, m_CandyFont);
            m_GameOverBestText = bestVal.GetComponent<Text>();
            MakeTextCrisp(m_GameOverBestText, new Color(0.28f, 0.14f, 0.02f, 0.95f), 2.0f);

            // Glowing New Record Badge (Appears right above score card when highscore broken)
            m_GameOverNewBestObj = new GameObject("NewRecordBadge");
            m_GameOverNewBestObj.transform.SetParent(sceneObj.transform, false);
            RectTransform nrbRt = m_GameOverNewBestObj.AddComponent<RectTransform>();
            nrbRt.anchoredPosition = new Vector2(0f, 248f);
            nrbRt.sizeDelta = new Vector2(340f, 42f);
            Image nrbImg = m_GameOverNewBestObj.AddComponent<Image>();
            nrbImg.sprite = SpriteFactory.Get3DPillBadgeSprite(new Color(1f, 0.82f, 0.18f), new Color(0.65f, 0.38f, 0.02f));
            nrbImg.type = Image.Type.Sliced;
            nrbImg.raycastTarget = false;

            GameObject nrbTxt = CreateText(m_GameOverNewBestObj.transform, Vector2.zero, new Vector2(320f, 36f), "NEW BEST RECORD!", 24, FontStyle.Normal, Color.white, TextAnchor.MiddleCenter, m_CandyFont);
            MakeTextCrisp(nrbTxt.GetComponent<Text>(), new Color(0.42f, 0.22f, 0.02f, 0.98f), 2.0f);
            m_GameOverNewBestObj.SetActive(false);

            // 3. Button 1: REVIVE (Golden 3D Pill Button placed at Y: -112)
            m_GameOverReviveBtn = CreateTransparentButton(sceneObj.transform, new Vector2(0f, -112f), new Vector2(430f, 90f), () =>
            {
                if (GameManager.Instance != null)
                {
                    if (GameManager.Instance.ReviveGame())
                    {
                        m_GameOverPanel.SetActive(false);
                    }
                    else
                    {
                        HapticManager.TriggerLight();
                        FloatingText.Create(Vector3.zero, "No Revives Left!", new Color(1f, 0.35f, 0.35f), 6f);
                    }
                }
            }, "Btn_Revive");

            // Revive Icon (Sparkle Jewel)
            GameObject rIconObj = new GameObject("ReviveIcon");
            rIconObj.transform.SetParent(m_GameOverReviveBtn.transform, false);
            RectTransform rIconRt = rIconObj.AddComponent<RectTransform>();
            rIconRt.anchoredPosition = new Vector2(-155f, 2f);
            rIconRt.sizeDelta = new Vector2(40f, 40f);
            Image rIconImg = rIconObj.AddComponent<Image>();
            rIconImg.sprite = GemIconFactory.GetSparkleIcon3D();
            rIconImg.raycastTarget = false;

            GameObject rTxtObj = CreateText(m_GameOverReviveBtn.transform, new Vector2(18f, 2f), new Vector2(350f, 54f), "REVIVE (3 CHANCES LEFT)", 32, FontStyle.Normal, Color.white, TextAnchor.MiddleCenter, m_CandyFont);
            m_GameOverReviveText = rTxtObj.GetComponent<Text>();
            MakeTextCrisp(m_GameOverReviveText, new Color(0.38f, 0.18f, 0.02f, 0.95f), 2.5f);

            // 4. Button 2: PLAY AGAIN (Emerald Green 3D Pill Button at Y: -224)
            m_GameOverRestartBtn = CreateTransparentButton(sceneObj.transform, new Vector2(0f, -224f), new Vector2(430f, 90f), () =>
            {
                m_GameOverPanel.SetActive(false);
                if (GameManager.Instance != null) GameManager.Instance.RestartGame();
            }, "Btn_Restart");

            // Play Again Icon (Restart curved arrow)
            GameObject pIconObj = new GameObject("RestartIcon");
            pIconObj.transform.SetParent(m_GameOverRestartBtn.transform, false);
            RectTransform pIconRt = pIconObj.AddComponent<RectTransform>();
            pIconRt.anchoredPosition = new Vector2(-135f, 2f);
            pIconRt.sizeDelta = new Vector2(40f, 40f);
            Image pIconImg = pIconObj.AddComponent<Image>();
            pIconImg.sprite = GemIconFactory.GetRestartArrowSprite();
            pIconImg.raycastTarget = false;

            GameObject pTxtObj = CreateText(m_GameOverRestartBtn.transform, new Vector2(18f, 2f), new Vector2(330f, 54f), "PLAY AGAIN", 38, FontStyle.Normal, Color.white, TextAnchor.MiddleCenter, m_CandyFont);
            MakeTextCrisp(pTxtObj.GetComponent<Text>(), new Color(0.02f, 0.24f, 0.08f, 0.95f), 2.5f);

            // 5. Button 3: HOME (Sapphire Blue 3D Pill Button at Y: -340)
            m_GameOverHomeBtn = CreateTransparentButton(sceneObj.transform, new Vector2(0f, -340f), new Vector2(430f, 90f), () =>
            {
                m_GameOverPanel.SetActive(false);
                OpenModeSelect();
            }, "Btn_Home");

            // Home Icon
            GameObject hIconObj = new GameObject("HomeIcon");
            hIconObj.transform.SetParent(m_GameOverHomeBtn.transform, false);
            RectTransform hIconRt = hIconObj.AddComponent<RectTransform>();
            hIconRt.anchoredPosition = new Vector2(-110f, 2f);
            hIconRt.sizeDelta = new Vector2(40f, 40f);
            Image hIconImg = hIconObj.AddComponent<Image>();
            hIconImg.sprite = GemIconFactory.GetHomeSprite();
            hIconImg.raycastTarget = false;

            GameObject hTxtObj = CreateText(m_GameOverHomeBtn.transform, new Vector2(18f, 2f), new Vector2(280f, 54f), "HOME", 40, FontStyle.Normal, Color.white, TextAnchor.MiddleCenter, m_CandyFont);
            MakeTextCrisp(hTxtObj.GetComponent<Text>(), new Color(0.03f, 0.14f, 0.38f, 0.95f), 2.5f);

            m_GameOverPanel.SetActive(false);
        }

        // ==========================================
        // ðŸ› ï¸ HELPER CREATORS
        // ==========================================
        private GameObject CreateText(Transform parent, Vector2 anchoredPos, Vector2 size, string text, int fontSize, FontStyle style, Color color, TextAnchor align, Font font = null)
        {
            GameObject obj = new GameObject("Text");
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            Text txt = obj.AddComponent<Text>();
            txt.font = (font != null) ? font : m_DefaultFont;
            txt.text = text;
            txt.fontSize = fontSize;
            // Lilita One is naturally an ultra-bold display font. Applying FontStyle.Bold or rich text <b> triggers faux-bold dilation in Unity which smears glyph contours.
            if (txt.font != null && txt.font.name.IndexOf("Lilita", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                txt.fontStyle = FontStyle.Normal;
                if (!string.IsNullOrEmpty(txt.text))
                {
                    txt.text = txt.text.Replace("<b>", "").Replace("</b>", "");
                }
            }
            else
            {
                txt.fontStyle = style;
            }
            txt.color = color;
            txt.alignment = align;
            txt.horizontalOverflow = HorizontalWrapMode.Overflow;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            txt.raycastTarget = false;
            return obj;
        }

        private static void MakeTextCrisp(Text txt, Color outlineOrShadowColor, float distance = 1.0f)
        {
            if (txt == null) return;
            // Strip any faux-bold tags that cause glyph smearing on Lilita One
            if (txt.font != null && txt.font.name.IndexOf("Lilita", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (!string.IsNullOrEmpty(txt.text) && (txt.text.Contains("<b>") || txt.text.Contains("</b>")))
                {
                    txt.text = txt.text.Replace("<b>", "").Replace("</b>", "");
                }
                txt.fontStyle = FontStyle.Normal;
            }

            // Remove any multi-pass Outline component that creates blurry vertex ghosting around mobile text
            Outline existingOutline = txt.gameObject.GetComponent<Outline>();
            if (existingOutline != null)
            {
                DestroyImmediate(existingOutline);
            }

            // Crisp directional 3D drop shadow directly beneath characters without choking interior letter counters
            Shadow shadow = txt.gameObject.GetComponent<Shadow>();
            if (shadow == null) shadow = txt.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(outlineOrShadowColor.r, outlineOrShadowColor.g, outlineOrShadowColor.b, 0.90f);
            float d = Mathf.Clamp(distance, 1.2f, 3.0f);
            shadow.effectDistance = new Vector2(0f, -d);
            shadow.useGraphicAlpha = true;
        }

        private GameObject CreateButton(Transform parent, Vector2 anchoredPos, Vector2 size, Color bgColor, string label, int fontSize)
        {
            GameObject obj = new GameObject("Button");
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            Image img = obj.AddComponent<Image>();
            img.sprite = SpriteFactory.GetRoundedButtonSprite(bgColor, new Color(1f, 1f, 1f, 0.28f), 0.05f);
            img.type = Image.Type.Sliced;
            img.color = Color.white;
            img.raycastTarget = true;

            Button btn = obj.AddComponent<Button>();

            if (!string.IsNullOrEmpty(label))
            {
                CreateText(obj.transform, Vector2.zero, size, label, fontSize, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            }
            return obj;
        }

        private GameObject CreateCircularIconButton(Transform parent, Vector2 anchoredPos, float diameter, Sprite icon, Color bgColor)
        {
            GameObject obj = new GameObject("IconButton");
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(diameter, diameter);

            Image img = obj.AddComponent<Image>();
            img.sprite = SpriteFactory.GetCircleButtonSprite(bgColor, new Color(1f, 1f, 1f, 0.28f), 0.08f);
            img.color = Color.white;
            img.raycastTarget = true;
            obj.AddComponent<Button>();

            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(obj.transform, false);
            RectTransform iRt = iconObj.AddComponent<RectTransform>();
            iRt.anchoredPosition = Vector2.zero;
            iRt.sizeDelta = new Vector2(diameter * 0.62f, diameter * 0.62f);
            Image iImg = iconObj.AddComponent<Image>();
            iImg.sprite = icon;
            iImg.raycastTarget = false;

            return obj;
        }

        private GameObject CreateIconButtonWithText(Transform parent, Vector2 anchoredPos, Vector2 size, Color bgColor, Sprite icon, string label, int fontSize)
        {
            GameObject obj = CreateButton(parent, anchoredPos, size, bgColor, "", fontSize);

            if (icon != null)
            {
                GameObject iconObj = new GameObject("BtnIcon");
                iconObj.transform.SetParent(obj.transform, false);
                RectTransform iconRt = iconObj.AddComponent<RectTransform>();
                iconRt.anchoredPosition = new Vector2(-size.x * 0.28f, 0);
                iconRt.sizeDelta = new Vector2(size.y * 0.58f, size.y * 0.58f);
                Image iconImg = iconObj.AddComponent<Image>();
                iconImg.sprite = icon;
                iconImg.raycastTarget = false;
            }

            CreateText(obj.transform, new Vector2(icon != null ? 22 : 0, 0), new Vector2(size.x - 90, size.y), label, fontSize, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            return obj;
        }

        private System.Collections.IEnumerator AnimateButtonPress(RectTransform targetRt, System.Action onComplete)
        {
            if (targetRt == null)
            {
                onComplete?.Invoke();
                yield break;
            }

            HapticManager.TriggerLight();
            if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();

            Vector3 baseScale = Vector3.one;
            float elapsed = 0f;
            float duration = 0.07f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = elapsed / duration;
                float scale = Mathf.Lerp(1.0f, 0.94f, progress);
                targetRt.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }

            elapsed = 0f;
            duration = 0.10f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = elapsed / duration;
                float scale = Mathf.Lerp(0.94f, 1.04f, progress);
                targetRt.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }

            targetRt.localScale = baseScale;
            onComplete?.Invoke();
        }

        private System.Collections.IEnumerator AnimateButtonFlash(Image flashImg, System.Action onComplete)
        {
            if (flashImg == null)
            {
                onComplete?.Invoke();
                yield break;
            }

            Color baseColor = flashImg.color;
            float targetAlpha = 0.38f;
            float duration = 0.18f;
            float elapsed = 0f;

            // Fast burst highlight in
            while (elapsed < duration * 0.4f)
            {
                elapsed += Time.unscaledDeltaTime;
                float a = Mathf.Lerp(0f, targetAlpha, elapsed / (duration * 0.4f));
                flashImg.color = new Color(baseColor.r, baseColor.g, baseColor.b, a);
                yield return null;
            }

            // Smooth ease-out fade
            elapsed = 0f;
            while (elapsed < duration * 0.6f)
            {
                elapsed += Time.unscaledDeltaTime;
                float a = Mathf.Lerp(targetAlpha, 0f, elapsed / (duration * 0.6f));
                flashImg.color = new Color(baseColor.r, baseColor.g, baseColor.b, a);
                yield return null;
            }

            flashImg.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0f);
            onComplete?.Invoke();
        }

        private System.Collections.IEnumerator AnimateCrownSparkle(RectTransform starRt)
        {
            if (starRt == null) yield break;
            Vector3 origScale = starRt.localScale;
            float elapsed = 0f;
            float dur = 0.32f;
            while (elapsed < dur)
            {
                elapsed += Time.unscaledDeltaTime;
                float p = elapsed / dur;
                float s = 1f + 0.9f * Mathf.Sin(p * Mathf.PI);
                starRt.localScale = origScale * s;
                starRt.localRotation = Quaternion.Euler(0, 0, p * 360f);
                yield return null;
            }
            starRt.localScale = origScale;
        }

        // ==========================================
        // ðŸ‘† FIRST-TIME TUTORIAL HAND GUIDE (FTUE)
        // ==========================================
        private void BuildTutorialOverlay()
        {
            if (m_TutorialOverlayObj != null) return;

            m_TutorialOverlayObj = new GameObject("TutorialOverlay", typeof(RectTransform));
            m_TutorialOverlayObj.transform.SetParent(m_RootCanvas, false);
            RectTransform rt = m_TutorialOverlayObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.sizeDelta = Vector2.zero;

            // Translucent dark vignette (raycastTarget = false so piece dragging is NEVER blocked!)
            Image bg = m_TutorialOverlayObj.AddComponent<Image>();
            bg.color = new Color(0.02f, 0.04f, 0.12f, 0.40f);
            bg.raycastTarget = false;

            // Guidance Banner
            GameObject bannerObj = new GameObject("TutorialBanner");
            bannerObj.transform.SetParent(m_TutorialOverlayObj.transform, false);
            RectTransform bRt = bannerObj.AddComponent<RectTransform>();
            bRt.anchorMin = new Vector2(0.5f, 0.5f);
            bRt.anchorMax = new Vector2(0.5f, 0.5f);
            bRt.pivot = new Vector2(0.5f, 0.5f);
            bRt.anchoredPosition = new Vector2(0f, 130f);
            bRt.sizeDelta = new Vector2(580f, 74f);

            Image bImg = bannerObj.AddComponent<Image>();
            bImg.sprite = SpriteFactory.Get3DPillBadgeSprite(new Color(0.12f, 0.45f, 0.95f), new Color(0.04f, 0.18f, 0.45f));
            bImg.type = Image.Type.Sliced;
            bImg.raycastTarget = false;

            Font font = (m_CandyFont != null) ? m_CandyFont : m_DefaultFont;
            GameObject txtObj = CreateText(bannerObj.transform, Vector2.zero, new Vector2(550f, 60f), "DRAG SHAPE TO BLAST LINES!", 25, FontStyle.Normal, Color.white, TextAnchor.MiddleCenter, font);
            MakeTextCrisp(txtObj.GetComponent<Text>(), new Color(0.02f, 0.08f, 0.25f, 0.95f), 1.0f);

            // Animated Pointing Hand Icon
            GameObject handObj = new GameObject("TutorialHand");
            handObj.transform.SetParent(m_TutorialOverlayObj.transform, false);
            m_TutorialHandRt = handObj.AddComponent<RectTransform>();
            m_TutorialHandRt.sizeDelta = new Vector2(96f, 96f);
            m_TutorialHandRt.pivot = new Vector2(0.38f, 0.88f);
            Image hImg = handObj.AddComponent<Image>();
            hImg.sprite = GemIconFactory.GetTutorialHandSprite();
            hImg.raycastTarget = false;

            m_TutorialOverlayObj.SetActive(false);
        }

        public void ShowTutorialGuide()
        {
            if (PlayerPrefs.GetInt("BoxBlast_Tutorial_Completed", 0) == 1) return;

            if (m_TutorialOverlayObj == null) BuildTutorialOverlay();
            if (m_TutorialOverlayObj == null) return;

            m_IsTutorialActive = true;
            m_TutorialOverlayObj.SetActive(true);
            m_TutorialOverlayObj.transform.SetAsLastSibling();

            if (m_TutorialAnimCoroutine != null) StopCoroutine(m_TutorialAnimCoroutine);
            m_TutorialAnimCoroutine = StartCoroutine(AnimateTutorialGuideLoop());

            if (SoundManager.Instance != null) SoundManager.Instance.PlayTutorialPrompt();
        }

        public void OnPlayerStartedDrag()
        {
            if (m_IsTutorialActive)
            {
                HideTutorialGuide(false);
            }
        }

        public void HideTutorialGuide(bool completed)
        {
            if (!m_IsTutorialActive && (m_TutorialOverlayObj == null || !m_TutorialOverlayObj.activeSelf)) return;
            m_IsTutorialActive = false;

            if (m_TutorialAnimCoroutine != null)
            {
                StopCoroutine(m_TutorialAnimCoroutine);
                m_TutorialAnimCoroutine = null;
            }

            if (m_TutorialOverlayObj != null)
            {
                m_TutorialOverlayObj.SetActive(false);
            }

            if (completed)
            {
                PlayerPrefs.SetInt("BoxBlast_Tutorial_Completed", 1);
                PlayerPrefs.Save();
                FloatingText.CreateBadge(Vector3.up * 0.5f, "GREAT JOB! LINE BLASTED!", Color.white, new Color(0.12f, 0.65f, 0.35f));
            }
        }

        private System.Collections.IEnumerator AnimateTutorialGuideLoop()
        {
            while (m_IsTutorialActive && m_TutorialHandRt != null)
            {
                Vector3 startWorld = (ShapeSpawner.Instance != null) ? ShapeSpawner.Instance.transform.position : new Vector3(0f, -4.3f, 0f);
                Vector3 targetWorld = (GridManager.Instance != null) ? (Vector3)GridManager.Instance.GridCenter : new Vector3(0f, 0.4f, 0f);

                Camera cam = Camera.main;
                Vector2 startScreen = (cam != null) ? (Vector2)cam.WorldToScreenPoint(startWorld + new Vector3(0.2f, -0.2f, 0f)) : new Vector2(Screen.width * 0.5f, Screen.height * 0.22f);
                Vector2 targetScreen = (cam != null) ? (Vector2)cam.WorldToScreenPoint(targetWorld) : new Vector2(Screen.width * 0.5f, Screen.height * 0.55f);

                RectTransformUtility.ScreenPointToLocalPointInRectangle(m_RootCanvas as RectTransform, startScreen, null, out Vector2 startLocal);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(m_RootCanvas as RectTransform, targetScreen, null, out Vector2 targetLocal);

                m_TutorialHandRt.anchoredPosition = startLocal;
                m_TutorialHandRt.localScale = Vector3.zero;

                float popElapsed = 0f;
                while (popElapsed < 0.22f)
                {
                    popElapsed += Time.unscaledDeltaTime;
                    float s = Mathf.Lerp(0f, 1.15f, popElapsed / 0.22f);
                    m_TutorialHandRt.localScale = Vector3.one * s;
                    yield return null;
                }
                m_TutorialHandRt.localScale = Vector3.one;
                yield return new WaitForSecondsRealtime(0.15f);

                float glideElapsed = 0f;
                float glideDuration = 1.1f;
                while (glideElapsed < glideDuration)
                {
                    glideElapsed += Time.unscaledDeltaTime;
                    float t = Mathf.SmoothStep(0f, 1f, glideElapsed / glideDuration);
                    Vector2 curPos = Vector2.Lerp(startLocal, targetLocal, t);
                    curPos.x += Mathf.Sin(t * Mathf.PI) * 25f;
                    m_TutorialHandRt.anchoredPosition = curPos;
                    yield return null;
                }

                yield return new WaitForSecondsRealtime(0.35f);
                float fadeElapsed = 0f;
                while (fadeElapsed < 0.25f)
                {
                    fadeElapsed += Time.unscaledDeltaTime;
                    float s = Mathf.Lerp(1.0f, 0f, fadeElapsed / 0.25f);
                    m_TutorialHandRt.localScale = Vector3.one * s;
                    yield return null;
                }
                yield return new WaitForSecondsRealtime(0.20f);
            }
        }

        // ==========================================
        // ðŸŒŸ FIRST-TIME HOME FEATURE GUIDED TOUR (FTUE)
        // ==========================================
        private GameObject m_HomeTourModalObj;
        private int m_CurrentTourStep = 0;
        private Text m_TourStepBadgeText;
        private Text m_TourTitleText;
        private Text m_TourDescText;
        private Image m_TourIconImg;
        private Text m_TourNextBtnText;
        private RectTransform m_TourHandRt;
        private Coroutine m_TourHandCoroutine;

        public void ShowHomeFeatureTour(bool force = false)
        {
            if (!force && PlayerPrefs.GetInt("BoxBlast_HomeTour_Seen", 0) == 1) return;

            if (m_HomeTourModalObj == null)
            {
                BuildHomeFeatureTourModal();
            }

            if (m_HomeTourModalObj == null) return;

            m_CurrentTourStep = 0;
            m_HomeTourModalObj.SetActive(true);
            m_HomeTourModalObj.transform.SetAsLastSibling();
            UpdateTourStepUI();

            if (SoundManager.Instance != null) SoundManager.Instance.PlayTutorialPrompt();
            HapticManager.TriggerLight();
        }

        private System.Collections.IEnumerator DelayedShowHomeTour(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            if (m_ModeSelectPanel != null && m_ModeSelectPanel.activeSelf)
            {
                ShowHomeFeatureTour(false);
            }
        }

        private void BuildHomeFeatureTourModal()
        {
            if (m_HomeTourModalObj != null) return;

            m_HomeTourModalObj = new GameObject("HomeFeatureTourModal", typeof(RectTransform));
            m_HomeTourModalObj.transform.SetParent(m_RootCanvas, false);
            RectTransform rt = m_HomeTourModalObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            // Semi-transparent dark cosmic backdrop
            Image bg = m_HomeTourModalObj.AddComponent<Image>();
            bg.color = new Color(0.02f, 0.05f, 0.15f, 0.82f);
            bg.raycastTarget = true; // Blocks clicks behind while tour is active

            // Main Tour Card (680 x 520)
            GameObject cardObj = new GameObject("TourCard", typeof(RectTransform));
            cardObj.transform.SetParent(m_HomeTourModalObj.transform, false);
            RectTransform cRt = cardObj.GetComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0.5f, 0.5f);
            cRt.anchorMax = new Vector2(0.5f, 0.5f);
            cRt.pivot = new Vector2(0.5f, 0.5f);
            cRt.anchoredPosition = new Vector2(0f, 40f);
            cRt.sizeDelta = new Vector2(680f, 520f);

            Image cardBg = cardObj.AddComponent<Image>();
            cardBg.sprite = SpriteFactory.GetRoundedButtonSprite(new Color(0.08f, 0.16f, 0.38f, 0.98f), new Color(0.18f, 0.42f, 0.85f, 1f), 0.16f);
            cardBg.type = Image.Type.Sliced;

            Font font = (m_CandyFont != null) ? m_CandyFont : m_DefaultFont;

            // Step Badge Pill at top of Card (X: 0, Y: 215)
            GameObject badgeObj = new GameObject("StepBadge");
            badgeObj.transform.SetParent(cardObj.transform, false);
            RectTransform bRt = badgeObj.AddComponent<RectTransform>();
            bRt.anchoredPosition = new Vector2(0f, 215f);
            bRt.sizeDelta = new Vector2(220f, 48f);
            Image bImg = badgeObj.AddComponent<Image>();
            bImg.sprite = SpriteFactory.Get3DPillBadgeSprite(new Color(0.98f, 0.72f, 0.15f), new Color(0.55f, 0.28f, 0.02f));
            bImg.type = Image.Type.Sliced;
            GameObject bTxtObj = CreateText(badgeObj.transform, Vector2.zero, new Vector2(210f, 44f), "STEP 1 OF 4", 20, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, font);
            m_TourStepBadgeText = bTxtObj.GetComponent<Text>();
            MakeTextCrisp(m_TourStepBadgeText, new Color(0.35f, 0.15f, 0.02f, 0.95f), 1.0f);

            // Close / Skip Button at top-right of Card (X: 285, Y: 215)
            GameObject skipBtnObj = new GameObject("SkipBtn");
            skipBtnObj.transform.SetParent(cardObj.transform, false);
            RectTransform sRt = skipBtnObj.AddComponent<RectTransform>();
            sRt.anchoredPosition = new Vector2(285f, 215f);
            sRt.sizeDelta = new Vector2(90f, 44f);
            Image sBg = skipBtnObj.AddComponent<Image>();
            sBg.sprite = SpriteFactory.GetRoundedButtonSprite(new Color(0.25f, 0.30f, 0.45f, 0.8f), new Color(0.12f, 0.15f, 0.25f, 1f), 0.2f);
            Button sBtn = skipBtnObj.AddComponent<Button>();
            sBtn.onClick.AddListener(CloseHomeTour);
            GameObject sTxtObj = CreateText(skipBtnObj.transform, Vector2.zero, new Vector2(80f, 40f), "SKIP", 18, FontStyle.Bold, new Color(0.85f, 0.90f, 1f), TextAnchor.MiddleCenter, font);
            MakeTextCrisp(sTxtObj.GetComponent<Text>(), new Color(0.05f, 0.08f, 0.18f, 0.9f), 0.8f);

            // Big 3D Icon (X: 0, Y: 115, Size: 96 x 96)
            GameObject iconObj = new GameObject("TourIcon");
            iconObj.transform.SetParent(cardObj.transform, false);
            RectTransform iRt = iconObj.AddComponent<RectTransform>();
            iRt.anchoredPosition = new Vector2(0f, 115f);
            iRt.sizeDelta = new Vector2(96f, 96f);
            m_TourIconImg = iconObj.AddComponent<Image>();
            m_TourIconImg.sprite = GemIconFactory.GetGemSprite(GemType.YellowStar);
            m_TourIconImg.raycastTarget = false;

            // Title Text (X: 0, Y: 35)
            GameObject titleObj = CreateText(cardObj.transform, new Vector2(0f, 35f), new Vector2(620f, 60f), "HOW TO PLAY: BLOCK BLAST", 28, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, font);
            m_TourTitleText = titleObj.GetComponent<Text>();
            m_TourTitleText.resizeTextForBestFit = true;
            m_TourTitleText.resizeTextMinSize = 20;
            m_TourTitleText.resizeTextMaxSize = 28;
            MakeTextCrisp(m_TourTitleText, new Color(0.01f, 0.05f, 0.18f, 0.95f), 1.1f);

            // Description Text Box (X: 0, Y: -65, Size: 580 x 140)
            GameObject descObj = CreateText(cardObj.transform, new Vector2(0f, -65f), new Vector2(580f, 130f), "Description goes here...", 22, FontStyle.Normal, new Color(0.88f, 0.94f, 1f), TextAnchor.MiddleCenter, font);
            m_TourDescText = descObj.GetComponent<Text>();
            m_TourDescText.resizeTextForBestFit = true;
            m_TourDescText.resizeTextMinSize = 18;
            m_TourDescText.resizeTextMaxSize = 22;
            m_TourDescText.lineSpacing = 1.15f;
            MakeTextCrisp(m_TourDescText, new Color(0.02f, 0.06f, 0.18f, 0.95f), 0.9f);

            // Bottom NEXT Button (X: 0, Y: -185, Size: 320 x 74)
            GameObject nextBtnObj = new GameObject("TourNextBtn");
            nextBtnObj.transform.SetParent(cardObj.transform, false);
            RectTransform nRt = nextBtnObj.AddComponent<RectTransform>();
            nRt.anchoredPosition = new Vector2(0f, -185f);
            nRt.sizeDelta = new Vector2(320f, 74f);
            Image nBg = nextBtnObj.AddComponent<Image>();
            nBg.sprite = SpriteFactory.GetRoundedButtonSprite(new Color(0.98f, 0.72f, 0.15f), new Color(0.55f, 0.28f, 0.02f), 0.18f);
            Button nBtn = nextBtnObj.AddComponent<Button>();
            nBtn.onClick.AddListener(OnTourNextClicked);
            GameObject nTxtObj = CreateText(nextBtnObj.transform, Vector2.zero, new Vector2(300f, 65f), "NEXT >", 28, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, font);
            m_TourNextBtnText = nTxtObj.GetComponent<Text>();
            MakeTextCrisp(m_TourNextBtnText, new Color(0.35f, 0.15f, 0.02f, 0.95f), 1.1f);

            // Animated Pointing Hand Floating Indicator
            GameObject handObj = new GameObject("TourHand");
            handObj.transform.SetParent(m_HomeTourModalObj.transform, false);
            m_TourHandRt = handObj.AddComponent<RectTransform>();
            m_TourHandRt.sizeDelta = new Vector2(100f, 100f);
            m_TourHandRt.pivot = new Vector2(0.38f, 0.88f);
            Image hImg = handObj.AddComponent<Image>();
            hImg.sprite = GemIconFactory.GetTutorialHandSprite();
            hImg.raycastTarget = false;

            m_HomeTourModalObj.SetActive(false);
        }

        private void UpdateTourStepUI()
        {
            if (m_TourStepBadgeText != null) m_TourStepBadgeText.text = $"STEP {m_CurrentTourStep + 1} OF 4";

            Vector2 targetHandPos = Vector2.zero;

            switch (m_CurrentTourStep)
            {
                case 0: // ðŸŽ® Gameplay Basics
                    if (m_TourTitleText != null) m_TourTitleText.text = "HOW TO PLAY: BLOCK BLAST";
                    if (m_TourDescText != null) m_TourDescText.text = "Drag shapes from the dock onto the 8x8 grid!\n\nFill complete rows or columns to BLAST lines, clear space, and collect starry points!";
                    if (m_TourIconImg != null) m_TourIconImg.sprite = GemIconFactory.GetGemSprite(GemType.YellowStar);
                    if (m_TourNextBtnText != null) m_TourNextBtnText.text = "NEXT >";
                    targetHandPos = new Vector2(0f, -220f);
                    break;

                case 1: // ðŸ† Classic Mode & Best Score
                    if (m_TourTitleText != null) m_TourTitleText.text = "CLASSIC MODE & BEST SCORE";
                    if (m_TourDescText != null) m_TourDescText.text = "Play endless rounds without move limits!\n\nYour all-time BEST SCORE is always tracked right on the Classic card badge!";
                    if (m_TourIconImg != null) m_TourIconImg.sprite = Resources.Load<Sprite>("ChubbyGoldenCrownHD") ?? GemIconFactory.GetGemSprite(GemType.RedRuby);
                    if (m_TourNextBtnText != null) m_TourNextBtnText.text = "NEXT >";
                    targetHandPos = new Vector2(-190f, -260f);
                    break;

                case 2: // ðŸ—ºï¸ Adventure Mode & 40 Levels
                    if (m_TourTitleText != null) m_TourTitleText.text = "ADVENTURE GALAXY MAP";
                    if (m_TourDescText != null) m_TourDescText.text = "Tap Adventure Mode to journey through 40 levels!\n\nCollect target gems (Rubies, Diamonds, Stars) to unlock treasure chests and earn stars!";
                    if (m_TourIconImg != null) m_TourIconImg.sprite = GemIconFactory.GetGemSprite(GemType.BlueDiamond);
                    if (m_TourNextBtnText != null) m_TourNextBtnText.text = "NEXT >";
                    targetHandPos = new Vector2(190f, -260f);
                    break;

                case 3: // ðŸŽ¨ Ball Wardrobe & Skins
                    if (m_TourTitleText != null) m_TourTitleText.text = "3D BALL WARDROBE";
                    if (m_TourDescText != null) m_TourDescText.text = "Tap the Ball Sphere icon on the top-right anytime!\n\nUnlock and equip epic 3D candy balls, cosmic swirls, and planet designs!";
                    if (m_TourIconImg != null) m_TourIconImg.sprite = GemIconFactory.GetGemSprite(GemType.OrangePentagon);
                    if (m_TourNextBtnText != null) m_TourNextBtnText.text = "LET'S PLAY!";
                    targetHandPos = new Vector2(260f, 380f);
                    break;
            }

            if (m_TourHandCoroutine != null) StopCoroutine(m_TourHandCoroutine);
            m_TourHandCoroutine = StartCoroutine(AnimateTourHandBounce(targetHandPos));
        }

        private System.Collections.IEnumerator AnimateTourHandBounce(Vector2 targetPos)
        {
            if (m_TourHandRt == null) yield break;
            while (m_HomeTourModalObj != null && m_HomeTourModalObj.activeSelf)
            {
                float t = Time.unscaledTime * 3.5f;
                float bounce = Mathf.Sin(t) * 16f;
                m_TourHandRt.anchoredPosition = targetPos + new Vector2(bounce * 0.7f, -bounce * 0.7f);
                yield return null;
            }
        }

        private void OnTourNextClicked()
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
            HapticManager.TriggerLight();

            m_CurrentTourStep++;
            if (m_CurrentTourStep >= 4)
            {
                CloseHomeTour();
                FloatingText.CreateBadge(Vector3.up * 0.5f, "HAVE FUN PLAYING!", Color.white, new Color(0.12f, 0.65f, 0.35f));
                if (SoundManager.Instance != null) SoundManager.Instance.PlayAchievement();
            }
            else
            {
                UpdateTourStepUI();
            }
        }

        private void CloseHomeTour()
        {
            if (m_TourHandCoroutine != null)
            {
                StopCoroutine(m_TourHandCoroutine);
                m_TourHandCoroutine = null;
            }

            if (m_HomeTourModalObj != null)
            {
                m_HomeTourModalObj.SetActive(false);
            }

            PlayerPrefs.SetInt("BoxBlast_HomeTour_Seen", 1);
            PlayerPrefs.Save();
        }

        // ==========================================
        // âš¡ DYNAMIC JUICY COMBO CALLOUTS
        // ==========================================
        private void BuildComboCalloutUI()
        {
            if (m_ComboCalloutObj != null) return;

            m_ComboCalloutObj = new GameObject("ComboCalloutBanner", typeof(RectTransform));
            m_ComboCalloutObj.transform.SetParent(m_RootCanvas, false);
            m_ComboCalloutRt = m_ComboCalloutObj.GetComponent<RectTransform>();
            m_ComboCalloutRt.anchorMin = new Vector2(0.5f, 0.5f);
            m_ComboCalloutRt.anchorMax = new Vector2(0.5f, 0.5f);
            m_ComboCalloutRt.pivot = new Vector2(0.5f, 0.5f);
            m_ComboCalloutRt.sizeDelta = new Vector2(460f, 95f);

            m_ComboCalloutBg = m_ComboCalloutObj.AddComponent<Image>();
            m_ComboCalloutBg.sprite = SpriteFactory.Get3DPillBadgeSprite(new Color(1f, 0.82f, 0.18f), new Color(0.65f, 0.38f, 0.02f));
            m_ComboCalloutBg.type = Image.Type.Sliced;
            m_ComboCalloutBg.raycastTarget = false;

            Font font = (m_CandyFont != null) ? m_CandyFont : m_DefaultFont;
            GameObject txtObj = CreateText(m_ComboCalloutObj.transform, Vector2.zero, new Vector2(440f, 85f), "GREAT!", 48, FontStyle.Normal, Color.white, TextAnchor.MiddleCenter, font);
            m_ComboCalloutText = txtObj.GetComponent<Text>();
            MakeTextCrisp(m_ComboCalloutText, new Color(0.18f, 0.08f, 0.02f, 0.98f), 1.2f);

            m_ComboCalloutObj.SetActive(false);
        }

        public void ShowComboCallout(int combo, int linesCleared, Vector3 worldPos)
        {
            if (combo < 2 && linesCleared < 2) return;

            if (m_ComboCalloutObj == null) BuildComboCalloutUI();
            if (m_ComboCalloutObj == null) return;

            if (m_ComboCalloutCoroutine != null) StopCoroutine(m_ComboCalloutCoroutine);
            m_ComboCalloutCoroutine = StartCoroutine(AnimateComboCallout(combo, linesCleared, worldPos));
        }

        private System.Collections.IEnumerator AnimateComboCallout(int combo, int linesCleared, Vector3 worldPos)
        {
            string callout = "GREAT!";
            Color pillTop = new Color(1f, 0.85f, 0.20f);
            Color pillBot = new Color(0.68f, 0.38f, 0.04f);

            if (linesCleared >= 4)
            {
                callout = "MEGA BLAST!";
                pillTop = new Color(1f, 0.40f, 0.15f);
                pillBot = new Color(0.55f, 0.12f, 0.04f);
            }
            else if (linesCleared == 3)
            {
                callout = "TRIPLE BLAST!";
                pillTop = new Color(0.20f, 0.85f, 1.0f);
                pillBot = new Color(0.04f, 0.35f, 0.58f);
            }
            else if (linesCleared == 2 && combo < 3)
            {
                callout = "DOUBLE BLAST!";
                pillTop = new Color(1.0f, 0.78f, 0.18f);
                pillBot = new Color(0.60f, 0.32f, 0.02f);
            }
            else if (combo >= 5)
            {
                callout = "UNBELIEVABLE!";
                pillTop = new Color(1f, 0.22f, 0.55f);
                pillBot = new Color(0.55f, 0.04f, 0.25f);
            }
            else if (combo == 4)
            {
                callout = "AWESOME!";
                pillTop = new Color(0.85f, 0.25f, 1.0f);
                pillBot = new Color(0.45f, 0.08f, 0.60f);
            }
            else if (combo == 3)
            {
                callout = "AMAZING!";
                pillTop = new Color(1.0f, 0.55f, 0.12f);
                pillBot = new Color(0.65f, 0.22f, 0.02f);
            }

            if (m_ComboCalloutText != null) m_ComboCalloutText.text = callout;
            if (m_ComboCalloutBg != null)
            {
                m_ComboCalloutBg.sprite = SpriteFactory.Get3DPillBadgeSprite(pillTop, pillBot);
            }

            Camera cam = Camera.main;
            Vector2 screenPos = (cam != null) ? (Vector2)cam.WorldToScreenPoint(worldPos + Vector3.up * 0.8f) : new Vector2(Screen.width * 0.5f, Screen.height * 0.58f);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(m_RootCanvas as RectTransform, screenPos, null, out Vector2 localPos);

            m_ComboCalloutRt.anchoredPosition = localPos;
            m_ComboCalloutRt.localScale = Vector3.zero;
            m_ComboCalloutObj.SetActive(true);
            m_ComboCalloutObj.transform.SetAsLastSibling();

            if (ParticleFXManager.Instance != null)
            {
                ParticleFXManager.Instance.PlayZoneClearShockwave(worldPos);
                ParticleFXManager.Instance.PlayComboCelebration(worldPos, combo);
            }

            if (combo >= 3 && SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayComboFanfare(combo);
            }

            float popElapsed = 0f;
            float popDur = 0.26f;
            while (popElapsed < popDur)
            {
                popElapsed += Time.unscaledDeltaTime;
                float progress = popElapsed / popDur;
                float scale = (progress < 0.65f) ? Mathf.Lerp(0f, 1.32f, progress / 0.65f) : Mathf.Lerp(1.32f, 1.0f, (progress - 0.65f) / 0.35f);
                m_ComboCalloutRt.localScale = Vector3.one * scale;
                yield return null;
            }
            m_ComboCalloutRt.localScale = Vector3.one;

            float holdElapsed = 0f;
            float holdDur = 0.65f;
            Vector2 startPos = m_ComboCalloutRt.anchoredPosition;
            while (holdElapsed < holdDur)
            {
                holdElapsed += Time.unscaledDeltaTime;
                m_ComboCalloutRt.anchoredPosition = startPos + new Vector2(0f, holdElapsed * 60f);
                yield return null;
            }

            float outElapsed = 0f;
            float outDur = 0.18f;
            while (outElapsed < outDur)
            {
                outElapsed += Time.unscaledDeltaTime;
                float s = Mathf.Lerp(1.0f, 0f, outElapsed / outDur);
                m_ComboCalloutRt.localScale = Vector3.one * s;
                yield return null;
            }

            m_ComboCalloutObj.SetActive(false);
        }

        // ==========================================
        // ðŸ† ACHIEVEMENTS & QUEST BADGES SYSTEM UI
        // ==========================================
        private void BuildAchievementToastUI()
        {
            if (m_AchievementToastObj != null) return;

            m_AchievementToastObj = new GameObject("AchievementToast", typeof(RectTransform));
            m_AchievementToastObj.transform.SetParent(m_RootCanvas, false);
            m_AchievementToastRt = m_AchievementToastObj.GetComponent<RectTransform>();
            m_AchievementToastRt.anchorMin = new Vector2(0.5f, 1f);
            m_AchievementToastRt.anchorMax = new Vector2(0.5f, 1f);
            m_AchievementToastRt.pivot = new Vector2(0.5f, 1f);
            m_AchievementToastRt.anchoredPosition = new Vector2(0f, 140f);
            m_AchievementToastRt.sizeDelta = new Vector2(560f, 96f);

            Image bg = m_AchievementToastObj.AddComponent<Image>();
            bg.sprite = SpriteFactory.Get3DPillBadgeSprite(new Color(0.12f, 0.45f, 0.95f), new Color(0.04f, 0.18f, 0.45f));
            bg.type = Image.Type.Sliced;
            bg.raycastTarget = false;

            GameObject trophyObj = new GameObject("TrophyIcon");
            trophyObj.transform.SetParent(m_AchievementToastObj.transform, false);
            RectTransform trRt = trophyObj.AddComponent<RectTransform>();
            trRt.anchoredPosition = new Vector2(-220f, 0f);
            trRt.sizeDelta = new Vector2(54f, 54f);
            Image trImg = trophyObj.AddComponent<Image>();
            trImg.sprite = GemIconFactory.GetTrophySprite();
            trImg.raycastTarget = false;

            Font font = (m_CandyFont != null) ? m_CandyFont : m_DefaultFont;

            GameObject headObj = CreateText(m_AchievementToastObj.transform, new Vector2(30f, 20f), new Vector2(420f, 30f), "ACHIEVEMENT UNLOCKED!", 19, FontStyle.Normal, new Color(1.0f, 0.88f, 0.25f), TextAnchor.MiddleLeft, font);
            m_AchievementToastTitle = headObj.GetComponent<Text>();
            MakeTextCrisp(m_AchievementToastTitle, new Color(0.35f, 0.18f, 0.02f, 0.95f), 1.0f);

            GameObject descObj = CreateText(m_AchievementToastObj.transform, new Vector2(30f, -16f), new Vector2(420f, 32f), "First Line Blast (+50 Coins)", 21, FontStyle.Normal, Color.white, TextAnchor.MiddleLeft, font);
            m_AchievementToastDesc = descObj.GetComponent<Text>();
            MakeTextCrisp(m_AchievementToastDesc, new Color(0.02f, 0.08f, 0.22f, 0.95f), 1.0f);

            m_AchievementToastObj.SetActive(false);
        }

        public void ShowAchievementToast(AchievementData ach)
        {
            if (ach == null) return;
            if (m_AchievementToastObj == null) BuildAchievementToastUI();
            if (m_AchievementToastObj == null) return;

            if (m_AchievementToastCoroutine != null) StopCoroutine(m_AchievementToastCoroutine);
            m_AchievementToastCoroutine = StartCoroutine(AnimateAchievementToast(ach));
        }

        private System.Collections.IEnumerator AnimateAchievementToast(AchievementData ach)
        {
            if (m_AchievementToastTitle != null) m_AchievementToastTitle.text = "ðŸ† ACHIEVEMENT UNLOCKED!";
            if (m_AchievementToastDesc != null)
            {
                string rewardStr = (ach.CoinReward > 0) ? $"+{ach.CoinReward} Coins" : "";
                if (ach.BoosterReward != BoosterType.None) rewardStr += (rewardStr.Length > 0 ? $" & 1 {ach.BoosterReward}" : $"1 {ach.BoosterReward}");
                m_AchievementToastDesc.text = $"{ach.Title} ({rewardStr})";
            }

            m_AchievementToastObj.SetActive(true);
            m_AchievementToastObj.transform.SetAsLastSibling();

            float elapsed = 0f;
            float slideDur = 0.35f;
            Vector2 offscreen = new Vector2(0f, 120f);
            Vector2 onscreen = new Vector2(0f, -65f);

            while (elapsed < slideDur)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / slideDur);
                m_AchievementToastRt.anchoredPosition = Vector2.Lerp(offscreen, onscreen, t);
                yield return null;
            }
            m_AchievementToastRt.anchoredPosition = onscreen;

            yield return new WaitForSecondsRealtime(2.8f);

            elapsed = 0f;
            while (elapsed < slideDur)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / slideDur);
                m_AchievementToastRt.anchoredPosition = Vector2.Lerp(onscreen, offscreen, t);
                yield return null;
            }
            m_AchievementToastRt.anchoredPosition = offscreen;
            m_AchievementToastObj.SetActive(false);
        }

        private void BuildAchievementsModal()
        {
            if (m_AchievementsModal != null) return;

            m_AchievementsModal = new GameObject("AchievementsModal", typeof(RectTransform));
            m_AchievementsModal.transform.SetParent(m_RootCanvas, false);
            RectTransform rt = m_AchievementsModal.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.sizeDelta = Vector2.zero;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            Image bg = m_AchievementsModal.AddComponent<Image>();
            bg.color = new Color(0.04f, 0.07f, 0.16f, 0.98f);
            bg.raycastTarget = true;
            Button bgBtn = m_AchievementsModal.AddComponent<Button>();
            bgBtn.onClick.AddListener(CloseAchievementsModal);

            GameObject sceneObj = new GameObject("AchievementsSceneCard", typeof(RectTransform));
            sceneObj.transform.SetParent(m_AchievementsModal.transform, false);
            RectTransform sceneRt = sceneObj.GetComponent<RectTransform>();
            sceneRt.anchorMin = new Vector2(0.5f, 0.5f);
            sceneRt.anchorMax = new Vector2(0.5f, 0.5f);
            sceneRt.pivot = new Vector2(0.5f, 0.5f);
            sceneRt.anchoredPosition = Vector2.zero;
            sceneRt.sizeDelta = new Vector2(768, 1376);

            Image sceneImg = sceneObj.AddComponent<Image>();
            sceneImg.sprite = SpriteFactory.LoadOrFallbackSprite("WardrobeBackground_Concept1");
            sceneImg.color = Color.white;
            sceneImg.raycastTarget = true;

            GameObject closeObj = new GameObject("AchievementsCloseBtn", typeof(RectTransform));
            closeObj.transform.SetParent(sceneObj.transform, false);
            RectTransform closeRt = closeObj.GetComponent<RectTransform>();
            closeRt.anchoredPosition = new Vector2(275f, 408f);
            closeRt.sizeDelta = new Vector2(96f, 96f);

            Image closeImg = closeObj.AddComponent<Image>();
            closeImg.color = Color.clear;
            closeImg.raycastTarget = true;

            Button closeBtn = closeObj.AddComponent<Button>();
            closeBtn.onClick.AddListener(() =>
            {
                StartCoroutine(AnimateButtonPress(closeRt, CloseAchievementsModal));
            });
            closeObj.transform.SetAsLastSibling();

            Font font = (m_CandyFont != null) ? m_CandyFont : m_DefaultFont;
            GameObject titleObj = CreateText(sceneObj.transform, new Vector2(0f, 408f), new Vector2(460f, 65f), "ACHIEVEMENTS", 42, FontStyle.Normal, new Color(1.0f, 0.92f, 0.45f), TextAnchor.MiddleCenter, font);
            MakeTextCrisp(titleObj.GetComponent<Text>(), new Color(0.35f, 0.18f, 0.02f, 0.98f), 1.2f);

            GameObject subObj = CreateText(sceneObj.transform, new Vector2(0f, 345f), new Vector2(580f, 40f), "Complete challenges to claim free Coins & Boosters!", 22, FontStyle.Normal, new Color(0.85f, 0.92f, 1.0f), TextAnchor.MiddleCenter, font);
            MakeTextCrisp(subObj.GetComponent<Text>(), new Color(0.04f, 0.12f, 0.32f, 0.98f), 1.0f);

            GameObject scrollObj = new GameObject("AchievementsScrollView", typeof(RectTransform));
            scrollObj.transform.SetParent(sceneObj.transform, false);
            RectTransform sRt = scrollObj.GetComponent<RectTransform>();
            sRt.anchoredPosition = new Vector2(0f, -45f);
            sRt.sizeDelta = new Vector2(620f, 760f);

            ScrollRect sr = scrollObj.AddComponent<ScrollRect>();
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Elastic;

            GameObject maskObj = new GameObject("Viewport", typeof(RectTransform));
            maskObj.transform.SetParent(scrollObj.transform, false);
            RectTransform mRt = maskObj.GetComponent<RectTransform>();
            mRt.anchorMin = Vector2.zero; mRt.anchorMax = Vector2.one; mRt.sizeDelta = Vector2.zero;
            Image maskImg = maskObj.AddComponent<Image>();
            maskImg.color = Color.white;
            Mask mask = maskObj.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            GameObject contentObj = new GameObject("Content", typeof(RectTransform));
            contentObj.transform.SetParent(maskObj.transform, false);
            RectTransform cRt = contentObj.GetComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0.5f, 1f);
            cRt.anchorMax = new Vector2(0.5f, 1f);
            cRt.pivot = new Vector2(0.5f, 1f);
            cRt.anchoredPosition = Vector2.zero;
            cRt.sizeDelta = new Vector2(600f, 1200f);

            VerticalLayoutGroup vlg = contentObj.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 16f;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = false;
            vlg.childControlHeight = false;

            ContentSizeFitter csf = contentObj.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            sr.viewport = mRt;
            sr.content = cRt;
            m_AchievementsListContainer = contentObj.transform;

            m_AchievementsModal.SetActive(false);
        }

        public void OpenAchievementsModal()
        {
            if (m_AchievementsModal == null) BuildAchievementsModal();
            RefreshAchievementsList();
            if (m_AchievementsModal != null)
            {
                m_AchievementsModal.SetActive(true);
                m_AchievementsModal.transform.SetAsLastSibling();

                Transform scene = m_AchievementsModal.transform.Find("AchievementsSceneCard");
                if (scene != null)
                {
                    float canvasHeight = 1920f;
                    float canvasWidth = 1080f;
                    if (m_RootCanvas != null)
                    {
                        RectTransform cRt = m_RootCanvas.GetComponent<RectTransform>();
                        if (cRt != null && cRt.rect.height > 0) canvasHeight = cRt.rect.height;
                        if (cRt != null && cRt.rect.width > 0) canvasWidth = cRt.rect.width;
                    }
                    float fitScale = canvasHeight / 1376f;
                    if (768f * fitScale > canvasWidth) fitScale = canvasWidth / 768f;
                    scene.localScale = new Vector3(fitScale, fitScale, 1f);
                }
            }
        }

        public void CloseAchievementsModal()
        {
            if (m_AchievementsModal != null)
            {
                m_AchievementsModal.SetActive(false);
            }
            RefreshModeCardStats();
        }

        private void RefreshAchievementsList()
        {
            if (m_AchievementsListContainer == null || AchievementManager.Instance == null) return;

            for (int i = m_AchievementsListContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(m_AchievementsListContainer.GetChild(i).gameObject);
            }

            var allAch = AchievementManager.Instance.AllAchievements;
            Font font = (m_CandyFont != null) ? m_CandyFont : m_DefaultFont;

            foreach (var ach in allAch)
            {
                GameObject cardObj = new GameObject($"Card_{ach.Id}", typeof(RectTransform));
                cardObj.transform.SetParent(m_AchievementsListContainer, false);
                RectTransform cRt = cardObj.GetComponent<RectTransform>();
                cRt.sizeDelta = new Vector2(590f, 130f);

                Image cardBg = cardObj.AddComponent<Image>();
                Color bgColor = ach.IsClaimed
                    ? new Color(0.04f, 0.08f, 0.18f, 0.75f)
                    : (ach.IsUnlocked ? new Color(0.06f, 0.18f, 0.38f, 0.95f) : new Color(0.04f, 0.10f, 0.24f, 0.90f));
                Color borderColor = ach.IsClaimed
                    ? new Color(0.18f, 0.24f, 0.38f, 0.6f)
                    : (ach.IsUnlocked ? new Color(1.0f, 0.85f, 0.25f, 1f) : new Color(0.20f, 0.45f, 0.80f, 0.75f));
                cardBg.sprite = SpriteFactory.GetRoundedButtonSprite(bgColor, borderColor, 0.08f);

                // Trophy Icon
                GameObject trObj = new GameObject("Trophy");
                trObj.transform.SetParent(cardObj.transform, false);
                RectTransform tRt = trObj.AddComponent<RectTransform>();
                tRt.anchoredPosition = new Vector2(-235f, 0f);
                tRt.sizeDelta = new Vector2(64f, 64f);
                Image tImg = trObj.AddComponent<Image>();
                tImg.sprite = GemIconFactory.GetTrophySprite();
                tImg.color = ach.IsClaimed ? new Color(0.6f, 0.65f, 0.75f, 0.6f) : (ach.IsUnlocked ? Color.white : new Color(0.6f, 0.75f, 0.9f, 0.75f));
                tImg.raycastTarget = false;

                // Title
                Color titleCol = ach.IsClaimed ? new Color(0.7f, 0.75f, 0.85f) : (ach.IsUnlocked ? new Color(1f, 0.92f, 0.45f) : Color.white);
                GameObject title = CreateText(cardObj.transform, new Vector2(-40f, 32f), new Vector2(300f, 32f), ach.Title, 22, FontStyle.Normal, titleCol, TextAnchor.MiddleLeft, font);
                MakeTextCrisp(title.GetComponent<Text>(), new Color(0.02f, 0.06f, 0.18f, 0.95f), 1.0f);

                // Description
                GameObject desc = CreateText(cardObj.transform, new Vector2(-40f, 0f), new Vector2(300f, 28f), ach.Description, 17, FontStyle.Normal, new Color(0.75f, 0.85f, 1.0f), TextAnchor.MiddleLeft, font);
                MakeTextCrisp(desc.GetComponent<Text>(), new Color(0.02f, 0.04f, 0.12f, 0.95f), 0.8f);

                // Progress Bar
                GameObject trackObj = new GameObject("ProgressBarTrack");
                trackObj.transform.SetParent(cardObj.transform, false);
                RectTransform trkRt = trackObj.AddComponent<RectTransform>();
                trkRt.anchoredPosition = new Vector2(-40f, -34f);
                trkRt.sizeDelta = new Vector2(290f, 18f);
                Image trkImg = trackObj.AddComponent<Image>();
                trkImg.sprite = SpriteFactory.GetPillSprite(new Color(0.02f, 0.05f, 0.12f, 0.9f));
                trkImg.raycastTarget = false;

                float progress = Mathf.Clamp01((float)ach.CurrentValue / Mathf.Max(1, ach.TargetValue));

                GameObject fillObj = new GameObject("Fill");
                fillObj.transform.SetParent(trackObj.transform, false);
                RectTransform fRt = fillObj.AddComponent<RectTransform>();
                fRt.anchorMin = new Vector2(0f, 0.5f);
                fRt.anchorMax = new Vector2(0f, 0.5f);
                fRt.pivot = new Vector2(0f, 0.5f);
                fRt.anchoredPosition = new Vector2(2f, 0f);
                fRt.sizeDelta = new Vector2(Mathf.Max(8f, (trkRt.sizeDelta.x - 4f) * progress), 14f);
                Image fImg = fillObj.AddComponent<Image>();
                fImg.sprite = SpriteFactory.GetPillSprite(ach.IsUnlocked ? new Color(0.20f, 0.90f, 0.40f) : new Color(0.20f, 0.70f, 0.95f));
                fImg.raycastTarget = false;

                string progStr = ach.IsUnlocked ? $"{ach.TargetValue} / {ach.TargetValue}" : $"{ach.CurrentValue} / {ach.TargetValue}";
                GameObject progTxt = CreateText(trackObj.transform, Vector2.zero, new Vector2(290f, 18f), progStr, 13, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, font);
                MakeTextCrisp(progTxt.GetComponent<Text>(), Color.black, 0.6f);

                // Right Button: CLAIM / CLAIMED / LOCKED
                GameObject btnObj = new GameObject("ActionBtn");
                btnObj.transform.SetParent(cardObj.transform, false);
                RectTransform bRt = btnObj.AddComponent<RectTransform>();
                bRt.anchoredPosition = new Vector2(205f, 0f);
                bRt.sizeDelta = new Vector2(145f, 54f);

                string capturedId = ach.Id;
                if (ach.IsClaimed)
                {
                    Image bImg = btnObj.AddComponent<Image>();
                    bImg.sprite = SpriteFactory.Get3DPillBadgeSprite(new Color(0.25f, 0.32f, 0.45f), new Color(0.12f, 0.16f, 0.24f));
                    bImg.type = Image.Type.Sliced;
                    bImg.raycastTarget = false;

                    GameObject bTxt = CreateText(btnObj.transform, Vector2.zero, new Vector2(140f, 40f), "CLAIMED", 17, FontStyle.Normal, new Color(0.65f, 0.75f, 0.88f), TextAnchor.MiddleCenter, font);
                    MakeTextCrisp(bTxt.GetComponent<Text>(), Color.black, 0.8f);
                }
                else if (ach.IsUnlocked)
                {
                    Image bImg = btnObj.AddComponent<Image>();
                    bImg.sprite = SpriteFactory.Get3DPillBadgeSprite(new Color(1.0f, 0.84f, 0.20f), new Color(0.68f, 0.38f, 0.04f));
                    bImg.type = Image.Type.Sliced;
                    bImg.raycastTarget = true;

                    string rewardTxt = (ach.CoinReward > 0) ? $"CLAIM (+{ach.CoinReward})" : "CLAIM";
                    GameObject bTxt = CreateText(btnObj.transform, Vector2.zero, new Vector2(140f, 40f), rewardTxt, 17, FontStyle.Normal, Color.white, TextAnchor.MiddleCenter, font);
                    MakeTextCrisp(bTxt.GetComponent<Text>(), new Color(0.40f, 0.22f, 0.02f, 0.98f), 1.0f);

                    Button btn = btnObj.AddComponent<Button>();
                    btn.onClick.AddListener(() =>
                    {
                        StartCoroutine(AnimateButtonPress(bRt, () =>
                        {
                            if (AchievementManager.Instance != null && AchievementManager.Instance.ClaimReward(capturedId))
                            {
                                RefreshAchievementsList();
                            }
                        }));
                    });
                }
                else
                {
                    Image bImg = btnObj.AddComponent<Image>();
                    bImg.sprite = SpriteFactory.Get3DPillBadgeSprite(new Color(0.14f, 0.22f, 0.38f), new Color(0.08f, 0.12f, 0.22f));
                    bImg.type = Image.Type.Sliced;
                    bImg.raycastTarget = false;

                    string rewardTxt = (ach.CoinReward > 0) ? $"+{ach.CoinReward} Coins" : "LOCKED";
                    GameObject bTxt = CreateText(btnObj.transform, Vector2.zero, new Vector2(140f, 40f), rewardTxt, 16, FontStyle.Normal, new Color(0.55f, 0.70f, 0.90f), TextAnchor.MiddleCenter, font);
                    MakeTextCrisp(bTxt.GetComponent<Text>(), Color.black, 0.8f);
                }
            }
        }
    }

    /// <summary>
    /// Smooth organic breathing animation for Home Screen mode cards and SPHERE BLAST 3D Logo.
    /// Exactly matches the playful living feel of Block Blast.
    /// </summary>
    [System.Serializable]
    public class BokehDriftData
    {
        public RectTransform rt;
        public Vector2 basePos;
        public float speed;
        public float ampX;
        public float ampY;
        public float phase;
    }

    /// <summary>
    /// Smooth organic breathing animation for Home Screen mode cards, SPHERE BLAST 3D Logo,
    /// background volumetric rays, and cinematic depth-of-field bokeh candies.
    /// </summary>
    public class HomeButtonBreather : MonoBehaviour
    {
        public RectTransform classicBtnRt;
        public RectTransform classicShadowRt;
        public RectTransform adventureBtnRt;
        public RectTransform adventureShadowRt;
        public RectTransform logoRt;
        public RectTransform crownStarGlintRt;
        public RectTransform bgRaysRt;
        public RectTransform ballDesignsPillRt;
        public RectTransform centerShowcaseBallRt;
        public RectTransform ballDesignsPromptRt;
        public List<BokehDriftData> bokehItems = new List<BokehDriftData>();

        private void Update()
        {
            float t = Time.unscaledTime;

            // 0. Volumetric Background Light Rays Slow Shimmer
            if (bgRaysRt != null)
            {
                bgRaysRt.localRotation = Quaternion.Euler(0, 0, t * 4f);
            }

            // 0b. Cinematic Background Bokeh Floating Drift
            for (int i = 0; i < bokehItems.Count; i++)
            {
                var item = bokehItems[i];
                if (item.rt != null)
                {
                    float ox = Mathf.Sin(t * item.speed + item.phase) * item.ampX;
                    float oy = Mathf.Cos(t * item.speed * 0.8f + item.phase) * item.ampY;
                    item.rt.anchoredPosition = item.basePos + new Vector2(ox, oy);
                }
            }

            // 1. Classic & Adventure Cards Organic Breathing
            if (classicBtnRt != null)
            {
                float clScale = 1.0f + 0.016f * Mathf.Sin(t * 2.4f);
                classicBtnRt.localScale = new Vector3(clScale, clScale, 1f);
                if (classicShadowRt != null)
                {
                    classicShadowRt.localScale = new Vector3(clScale, clScale, 1f);
                }
            }

            if (adventureBtnRt != null)
            {
                float advScale = 1.0f + 0.020f * Mathf.Sin(t * 2.4f + 1.4f);
                adventureBtnRt.localScale = new Vector3(advScale, advScale, 1f);
                if (adventureShadowRt != null)
                {
                    adventureShadowRt.localScale = new Vector3(advScale, advScale, 1f);
                }
            }

            // 2. SPHERE BLAST 3D Candy Logo Gentle Breathing Pulse & Living Sway
            if (logoRt != null)
            {
                float logoPulse = 1.0f + 0.016f * Mathf.Sin(t * 2.4f);
                float logoTilt = Mathf.Sin(t * 1.8f) * 1.0f;
                logoRt.localScale = new Vector3(logoPulse, logoPulse, 1f);
                logoRt.localRotation = Quaternion.Euler(0, 0, logoTilt);
            }

            // 3. Twinkling Diamond Star on Crown Peak
            if (crownStarGlintRt != null)
            {
                crownStarGlintRt.localRotation = Quaternion.Euler(0, 0, t * 50f);
                float glintPulse = 0.85f + 0.35f * Mathf.Sin(t * 3.5f);
                crownStarGlintRt.localScale = new Vector3(glintPulse, glintPulse, 1f);
            }

            // 4. Home Page 3D Ball Designs Pill Organic Floating Pulse
            if (ballDesignsPillRt != null)
            {
                float pillPulse = 1.0f + 0.015f * Mathf.Sin(t * 2.2f);
                ballDesignsPillRt.localScale = new Vector3(pillPulse, pillPulse, 1f);
            }

            // 5. Central Cosmic Wheel Equipped Ball Showcase Bob & Breathing
            if (centerShowcaseBallRt != null)
            {
                float ballPulse = 1.0f + 0.035f * Mathf.Sin(t * 2.6f);
                float ballBob = Mathf.Sin(t * 1.8f) * 4f;
                centerShowcaseBallRt.localScale = new Vector3(ballPulse, ballPulse, 1f);
                centerShowcaseBallRt.anchoredPosition = new Vector2(0f, ballBob);
            }

            // 6. Floating Ball Designs Prompt Banner Gentle Breathing
            if (ballDesignsPromptRt != null)
            {
                float bannerPulse = 1.0f + 0.02f * Mathf.Sin(t * 2.8f);
                ballDesignsPromptRt.localScale = new Vector3(bannerPulse, bannerPulse, 1f);
            }
        }
    }
}
