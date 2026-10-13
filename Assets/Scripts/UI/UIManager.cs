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
        private Text m_LevelSelectTitleText;
        private Text m_LevelSelectStarsText;
        private RectTransform m_LevelSelectStarsRt;
        private RectTransform m_LevelSelectWardrobeRt;
        private Image m_LevelSelectStarsPillImg;
        private Text m_ModeCardAdvStarsText;
        private Text m_ModeCardEventStarsText;
        private Text m_ModeCardTaskStarsText;
        private Text m_ModeCardClassicBestText;
        private Text m_HomeClassicBadgeText;
        private Text m_HomeAdventureBadgeText;
        private Text m_DailyStreakText;

        // Adventure Gem Objectives in Level HUD
        private GameObject m_GemObjectivesBar;
        private GameObject m_LevelGoalBadgeObj;
        private readonly Dictionary<GemType, (GameObject rootObj, Image iconImg, Text countText, RectTransform iconRt)> m_GemObjectiveItems =
            new Dictionary<GemType, (GameObject, Image, Text, RectTransform)>();

        // Level Select Page Navigation (for 40 Adventure levels)
        private int m_LevelSelectPage = 0;
        private GameObject m_LevelPageNavObj;
        private Text m_LevelPageNavText;
        private Text m_TopChapterBtnText;
        private RectTransform m_PrevChapterSideRt;
        private Text m_PrevChapterSideText;
        private RectTransform m_NextChapterSideRt;
        private Text m_NextChapterSideText;

        // Shop Coin Balance Text
        private Text m_ShopCoinsText;

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
        // 🔄 ORIENTATION & RESPONSIVE LAYOUT
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
                m_CanvasScaler.dynamicPixelsPerUnit = 2.5f;
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

            // 0. Home Button (< HOME): Top-Left Corner
            if (m_HomeBtnObj != null)
            {
                RectTransform rt = m_HomeBtnObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(0f, 1f);
                rt.pivot = new Vector2(0f, 1f);
                float xPos = isLandscape ? (30 + safeLeft) : (35 + safeLeft);
                float yPos = isLandscape ? -(30 + safeTop) : -(45 + safeTop);
                rt.anchoredPosition = new Vector2(xPos, yPos);
            }

            // 1. Best Score (Crown + High Score): Right next to Home button
            if (m_BestScoreObj != null)
            {
                RectTransform rt = m_BestScoreObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(0f, 1f);
                rt.pivot = new Vector2(0f, 1f);
                float xPos = isLandscape ? (170 + safeLeft) : (178 + safeLeft);
                float yPos = isLandscape ? -(30 + safeTop) : -(45 + safeTop);
                rt.anchoredPosition = new Vector2(xPos, yPos);
            }

            // 2. Settings Gear: Top-Right Corner
            if (m_SettingsBtnObj != null)
            {
                RectTransform rt = m_SettingsBtnObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(1f, 1f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(1f, 1f);
                rt.anchoredPosition = new Vector2(-(45 + safeRight), -(45 + safeTop));
            }

            // 3. Current Score (Player's score): Top Center
            if (m_ScoreCardObj != null)
            {
                RectTransform rt = m_ScoreCardObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 1f);
                rt.anchorMax = new Vector2(0.5f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
                float yPos = isLandscape ? -(35 + safeTop) : -(85 + safeTop);
                rt.anchoredPosition = new Vector2(0, yPos);
            }

            // 4. Adventure Level HUD: Concept 1 Golden Arcade Header Banner flush to top
            if (m_LevelHUD != null)
            {
                RectTransform rt = m_LevelHUD.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 1f);
                rt.anchorMax = new Vector2(0.5f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
                rt.anchoredPosition = Vector2.zero;
            }

            if (m_GemObjectivesBar != null)
            {
                RectTransform gobRt = m_GemObjectivesBar.GetComponent<RectTransform>();
                if (gobRt != null)
                {
                    gobRt.anchoredPosition = isLandscape ? new Vector2(0f, -145f) : new Vector2(0f, -550f);
                    gobRt.sizeDelta = new Vector2(760f, 76f);
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
        // 🏗️ UI CONSTRUCTION
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
                m_CanvasScaler.dynamicPixelsPerUnit = 2.5f;
                canvasObj.AddComponent<GraphicRaycaster>();
            }
            else
            {
                m_CanvasScaler = canvas.GetComponent<CanvasScaler>();
                if (m_CanvasScaler != null) m_CanvasScaler.dynamicPixelsPerUnit = 2.5f;
            }

            m_RootCanvas = canvas.transform;
            m_Raycaster = canvas.GetComponent<GraphicRaycaster>();

            // 0. Top-Left: Home Button (matching Adventure < HOME)
            BuildHomeButton();

            // 1. Top-Left: Best Score (👑 + Gold Score matching Image 1)
            BuildBestScoreHUD();

            // 2. Top-Right: Settings Gear Button (Clean gear icon matching Image 1)
            BuildSettingsButton();

            // 3. Top-Center: Current Score (Big White Bold Number matching Image 1)
            BuildScoreCard();

            // 4. Modals & Screens
            BuildSettingsModal();
            BuildClassicGameOverModal();
            BuildModeSelectScreen();
            BuildLevelSelectScreen();
            BuildLevelHUD();
            BuildVictoryModal();
            BuildDefeatModal();
            BuildSkinWardrobeModal();

            m_IsLandscape = Screen.width > Screen.height;
            ApplyOrientationLayout(m_IsLandscape);
            UpdateHUDVisibility();
        }

        private void BuildBestScoreHUD()
        {
            m_BestScoreObj = new GameObject("BestScoreTopLeft");
            m_BestScoreObj.transform.SetParent(m_RootCanvas, false);

            RectTransform rt = m_BestScoreObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.sizeDelta = new Vector2(250, 62);

            // Translucent Royal Navy Pill Badge to frame the Crown and Score with crisp contrast
            Image bsBg = m_BestScoreObj.AddComponent<Image>();
            bsBg.sprite = SpriteFactory.GetPillBadgeSprite(new Color(0.04f, 0.09f, 0.22f, 0.85f));
            bsBg.raycastTarget = false;

            // Crown Icon (Golden crown matching Image 1)
            GameObject crownIcon = new GameObject("CrownIcon");
            crownIcon.transform.SetParent(m_BestScoreObj.transform, false);
            RectTransform cIconRt = crownIcon.AddComponent<RectTransform>();
            cIconRt.anchorMin = new Vector2(0f, 0.5f);
            cIconRt.anchorMax = new Vector2(0f, 0.5f);
            cIconRt.pivot = new Vector2(0f, 0.5f);
            cIconRt.anchoredPosition = new Vector2(10, 0);
            cIconRt.sizeDelta = new Vector2(46, 46);
            Image cIconImg = crownIcon.AddComponent<Image>();
            cIconImg.sprite = GemIconFactory.GetCrownSprite();
            cIconImg.raycastTarget = false;

            // Best Score Value (Bold gold/yellow text matching Image 1)
            int best = (GameManager.Instance != null) ? GameManager.Instance.BestScore : 0;
            GameObject bestObj = CreateText(m_BestScoreObj.transform, new Vector2(62, 0), new Vector2(180, 56), best.ToString("N0"), 38, FontStyle.Bold, new Color(1.0f, 0.88f, 0.25f), TextAnchor.MiddleLeft);
            RectTransform btRt = bestObj.GetComponent<RectTransform>();
            btRt.anchorMin = new Vector2(0f, 0.5f);
            btRt.anchorMax = new Vector2(0f, 0.5f);
            btRt.pivot = new Vector2(0f, 0.5f);
            btRt.anchoredPosition = new Vector2(62, 0);
            m_BestScoreText = bestObj.GetComponent<Text>();
            MakeTextCrisp(m_BestScoreText, new Color(0.35f, 0.16f, 0.02f, 0.95f), 1.2f);
        }

        private void BuildSettingsButton()
        {
            m_SettingsBtnObj = new GameObject("SettingsTopButton");
            m_SettingsBtnObj.transform.SetParent(m_RootCanvas, false);

            RectTransform rt = m_SettingsBtnObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.sizeDelta = new Vector2(72, 72);

            // Transparent touch target for effortless tapping
            Image bg = m_SettingsBtnObj.AddComponent<Image>();
            bg.color = new Color(1f, 1f, 1f, 0.001f);
            bg.raycastTarget = true;

            Button btn = m_SettingsBtnObj.AddComponent<Button>();
            btn.onClick.AddListener(OpenSettings);

            // Crisp Gear Icon matching Image 1
            GameObject iconObj = new GameObject("GearIcon");
            iconObj.transform.SetParent(m_SettingsBtnObj.transform, false);
            RectTransform iconRt = iconObj.AddComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0.5f, 0.5f);
            iconRt.anchorMax = new Vector2(0.5f, 0.5f);
            iconRt.anchoredPosition = Vector2.zero;
            iconRt.sizeDelta = new Vector2(58, 58);
            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.sprite = GemIconFactory.GetGearSprite();
            iconImg.color = new Color(0.88f, 0.94f, 1.0f, 0.95f);
            iconImg.raycastTarget = false;
        }

        private void BuildHomeButton()
        {
            m_HomeBtnObj = new GameObject("HomeTopButton");
            m_HomeBtnObj.transform.SetParent(m_RootCanvas, false);

            RectTransform rt = m_HomeBtnObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.sizeDelta = new Vector2(130f, 62f);

            // Translucent Royal Navy Pill with Cyan Border matching Adventure Theme
            Image bg = m_HomeBtnObj.AddComponent<Image>();
            bg.sprite = SpriteFactory.GetRoundedButtonSprite(new Color(0.06f, 0.16f, 0.36f, 0.90f), new Color(0.18f, 0.85f, 0.95f, 0.95f), 0.12f);
            bg.raycastTarget = true;

            Button btn = m_HomeBtnObj.AddComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
                HapticManager.TriggerLight();
                StartCoroutine(AnimateButtonPress(rt, () =>
                {
                    OpenModeSelect();
                }));
            });

            // 3D Home House Icon
            GameObject iconObj = new GameObject("HomeIcon");
            iconObj.transform.SetParent(m_HomeBtnObj.transform, false);
            RectTransform iconRt = iconObj.AddComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0f, 0.5f);
            iconRt.anchorMax = new Vector2(0f, 0.5f);
            iconRt.pivot = new Vector2(0f, 0.5f);
            iconRt.anchoredPosition = new Vector2(10f, 0f);
            iconRt.sizeDelta = new Vector2(36f, 36f);
            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.sprite = GemIconFactory.GetHomeSprite();
            iconImg.color = Color.white;
            iconImg.raycastTarget = false;

            // "HOME" Text in crisp LilitaOne / Candy font
            Font font = (m_CandyFont != null) ? m_CandyFont : m_DefaultFont;
            GameObject txtObj = CreateText(m_HomeBtnObj.transform, new Vector2(18f, 0f), new Vector2(76f, 44f), "HOME", 24, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, font);
            Text txt = txtObj.GetComponent<Text>();
            MakeTextCrisp(txt, new Color(0.02f, 0.06f, 0.18f, 0.95f), 1.0f);
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
            rt.sizeDelta = new Vector2(400, 110);

            // Big White Current Score matching Image 1
            int score = (GameManager.Instance != null) ? GameManager.Instance.CurrentScore : 0;
            GameObject scoreObj = CreateText(m_ScoreCardObj.transform, Vector2.zero, new Vector2(400, 110), score.ToString("N0"), 76, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            m_ScoreText = scoreObj.GetComponent<Text>();

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
        // 🛒 BOOSTER SHOP MODAL
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
        // ⚙️ 3D ARCADE SETTINGS MODAL (CONCEPT 2 EXACT)
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

            // 2. Candy 3D Ruby Red Close (X) Button (placed directly over painted red X at X: 248, Y: 341)
            GameObject settingsCloseObj = new GameObject("SettingsCloseBtn", typeof(RectTransform));
            settingsCloseObj.transform.SetParent(sceneObj.transform, false);
            RectTransform sCloseRt = settingsCloseObj.GetComponent<RectTransform>();
            sCloseRt.anchoredPosition = new Vector2(248f, 341f);
            sCloseRt.sizeDelta = new Vector2(96f, 96f);
            Image sCloseImg = settingsCloseObj.AddComponent<Image>();
            sCloseImg.color = Color.clear;
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

            // 3. Sound Toggle (X: 134, Y: 208)
            CreateConcept2Toggle(sceneObj.transform, new Vector2(134, 208), new Vector2(124, 56), OnToggleSfxClicked, out m_SfxToggleTrackImg);

            // 4. Music Toggle (X: 134, Y: 100)
            CreateConcept2Toggle(sceneObj.transform, new Vector2(134, 100), new Vector2(124, 56), OnToggleMusicClicked, out m_MusicToggleTrackImg);

            // 5. Vibration Toggle (X: 134, Y: -8)
            CreateConcept2Toggle(sceneObj.transform, new Vector2(134, -8), new Vector2(124, 56), OnToggleHapticsClicked, out m_HapticToggleTrackImg);

            // 6. Ball Designs Button (X: 0, Y: -105, Size: 404 x 82)
            CreateConcept2Button(sceneObj.transform, new Vector2(0, -105), new Vector2(404, 82), "Settings_Btn_BallDesigns", () =>
            {
                m_SettingsModal.SetActive(false);
                OpenSkinWardrobe();
            });

            // 7. Restart Game Button (X: 0, Y: -199, Size: 404 x 82)
            CreateConcept2Button(sceneObj.transform, new Vector2(0, -199), new Vector2(404, 82), "Settings_Btn_Restart", () =>
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

            // 8. Home Button (X: 0, Y: -293, Size: 404 x 82)
            CreateConcept2Button(sceneObj.transform, new Vector2(0, -293), new Vector2(404, 82), "Settings_Btn_Home", OnHomeClicked);

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
            if (LevelManager.Instance != null && LevelManager.Instance.IsLevelActive)
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
        // 🚀 TITLE & SPLASH SCREEN
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
            int maxPage = (maxLvl - 1) / 20;
            m_LevelSelectPage = Mathf.Clamp((activeLevel - 1) / 20, 0, maxPage);
            RefreshLevelSelectGrid(mode);
            if (m_LevelSelectPanel != null)
            {
                m_LevelSelectPanel.SetActive(true);
                m_LevelSelectPanel.transform.SetAsLastSibling();
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
                m_DailyStreakText.text = $"×{dailyStreak}";
            }
            if (m_HomeClassicBadgeText != null)
            {
                m_HomeClassicBadgeText.text = bestScore > 0 ? $"👑 {bestScore:N0}" : "PLAY ▶";
                m_HomeClassicBadgeText.color = bestScore > 0 ? new Color(1.0f, 0.88f, 0.25f) : Color.white;
            }
            if (m_HomeAdventureBadgeText != null)
            {
                int currentAdvLvl = LevelManager.GetHighestUnlockedLevel(GameModeType.Adventure);
                m_HomeAdventureBadgeText.text = $"LVL {currentAdvLvl} ▶";
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
        }

        // ==========================================
        // 📊 HUD & LEVEL SUPPORT
        // ==========================================
        public void UpdateScore(int currentScore, int bestScore)
        {
            if (m_ScoreText != null) m_ScoreText.text = currentScore.ToString("N0");
            if (m_BestScoreText != null) m_BestScoreText.text = bestScore.ToString("N0");
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
                if (movesRemaining <= 3 && movesRemaining > 0)
                {
                    m_LevelMovesText.text = $"<color=#FFB0B0>MOVES:</color> <color=#FF4444>{movesRemaining}</color>";
                    StartCoroutine(PunchScale(m_LevelMovesBoxRt != null ? m_LevelMovesBoxRt : m_LevelMovesText.rectTransform, 1.15f, 0.18f));
                }
                else
                {
                    m_LevelMovesText.text = $"<color=#9FE8FF>MOVES:</color> <color=#FFDE38>{movesRemaining}</color>";
                }
            }
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
                        item.countText.text = "<color=#40FF80>✓</color>";
                        item.countText.fontSize = 36;
                    }
                    else
                    {
                        item.countText.text = $"x{remaining}";
                        item.countText.color = Color.white;
                        item.countText.fontSize = 36;
                    }
                    MakeTextCrisp(item.countText, new Color(0.01f, 0.05f, 0.18f, 0.95f), 1.0f);
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
                m_GameOverReviveBtn.SetActive(canRevive);
                if (canRevive && m_GameOverReviveText != null)
                {
                    m_GameOverReviveText.text = remaining == 1 ? "REVIVE (1 CHANCE LEFT)" : $"REVIVE ({remaining} CHANCES LEFT)";
                }
            }

            if (m_GameOverRestartBtn != null)
            {
                RectTransform rt = m_GameOverRestartBtn.GetComponent<RectTransform>();
                rt.anchoredPosition = canRevive ? new Vector2(0, -128) : new Vector2(0, -70);
            }
            if (m_GameOverHomeBtn != null)
            {
                RectTransform rt = m_GameOverHomeBtn.GetComponent<RectTransform>();
                rt.anchoredPosition = canRevive ? new Vector2(0, -206) : new Vector2(0, -150);
            }

            if (m_GameOverPanel != null)
            {
                m_GameOverPanel.SetActive(true);
                m_GameOverPanel.transform.SetAsLastSibling();
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
                }
                if (m_VictoryNextBtnObj != null)
                {
                    m_VictoryNextBtnObj.SetActive(hasNextLevel);
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

                // Dynamic responsive card scale for 9:16 aspect ratio across all mobile displays
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
                    float scale = Mathf.Min(canvasWidth / 768f, canvasHeight / 1376f, 1.0f);
                    scene.localScale = Vector3.one * scale;
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

            // Interactive 3D Gear Settings Button over the header's golden gear badge
            GameObject gearBtnObj = new GameObject("HeaderSettingsBtn");
            gearBtnObj.transform.SetParent(m_LevelHUD.transform, false);
            RectTransform gearRt = gearBtnObj.AddComponent<RectTransform>();
            gearRt.anchorMin = new Vector2(0.5f, 1f);
            gearRt.anchorMax = new Vector2(0.5f, 1f);
            gearRt.pivot = new Vector2(0.5f, 0.5f);
            gearRt.anchoredPosition = new Vector2(462f, -172f);
            gearRt.sizeDelta = new Vector2(110f, 110f);
            Image gearImg = gearBtnObj.AddComponent<Image>();
            gearImg.color = new Color(0f, 0f, 0f, 0.001f);
            gearImg.raycastTarget = true;
            Button gearBtn = gearBtnObj.AddComponent<Button>();
            gearBtn.onClick.AddListener(OpenSettings);

            // Interactive 3D Home Button over the header's top-left golden badge
            GameObject homeBtnObj = new GameObject("HeaderHomeBtn");
            homeBtnObj.transform.SetParent(m_LevelHUD.transform, false);
            RectTransform homeRt = homeBtnObj.AddComponent<RectTransform>();
            homeRt.anchorMin = new Vector2(0.5f, 1f);
            homeRt.anchorMax = new Vector2(0.5f, 1f);
            homeRt.pivot = new Vector2(0.5f, 0.5f);
            homeRt.anchoredPosition = new Vector2(-462f, -172f);
            homeRt.sizeDelta = new Vector2(110f, 110f);
            Image homeImg = homeBtnObj.AddComponent<Image>();
            homeImg.color = new Color(0f, 0f, 0f, 0.001f);
            homeImg.raycastTarget = true;
            Button homeBtn = homeBtnObj.AddComponent<Button>();
            homeBtn.onClick.AddListener(() =>
            {
                if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
                HapticManager.TriggerLight();
                OnHomeClicked();
            });

            Font hudFont = (m_CandyFont != null) ? m_CandyFont : ((m_HudCleanFont != null) ? m_HudCleanFont : m_DefaultFont);

            // 1. Left Plaque: Level Indicator - Centered over 3D sapphire plaque at X = -331, Y = -425
            GameObject levelBoxObj = new GameObject("HeaderLevelBox");
            levelBoxObj.transform.SetParent(m_LevelHUD.transform, false);
            RectTransform lbRt = levelBoxObj.AddComponent<RectTransform>();
            lbRt.anchorMin = new Vector2(0.5f, 1f);
            lbRt.anchorMax = new Vector2(0.5f, 1f);
            lbRt.pivot = new Vector2(0.5f, 0.5f);
            lbRt.anchoredPosition = new Vector2(-331f, -425f);
            lbRt.sizeDelta = new Vector2(250f, 75f);

            GameObject titleObj = CreateText(levelBoxObj.transform, Vector2.zero, new Vector2(240f, 55f), "LEVEL 1", 32, FontStyle.Normal, new Color(1f, 0.96f, 0.88f), TextAnchor.MiddleCenter, hudFont);
            m_LevelTitleText = titleObj.GetComponent<Text>();
            m_LevelTitleText.resizeTextForBestFit = true;
            m_LevelTitleText.resizeTextMinSize = 22;
            m_LevelTitleText.resizeTextMaxSize = 32;
            MakeTextCrisp(m_LevelTitleText, new Color(0.02f, 0.05f, 0.16f, 0.95f), 1.0f);

            // 2. Center Hero Plaque: Moves Counter - Centered over winged crest plaque at X = 0, Y = -425
            GameObject movesBoxObj = new GameObject("HeaderMovesBox");
            movesBoxObj.transform.SetParent(m_LevelHUD.transform, false);
            m_LevelMovesBoxRt = movesBoxObj.AddComponent<RectTransform>();
            m_LevelMovesBoxRt.anchorMin = new Vector2(0.5f, 1f);
            m_LevelMovesBoxRt.anchorMax = new Vector2(0.5f, 1f);
            m_LevelMovesBoxRt.pivot = new Vector2(0.5f, 0.5f);
            m_LevelMovesBoxRt.anchoredPosition = new Vector2(0f, -425f);
            m_LevelMovesBoxRt.sizeDelta = new Vector2(260f, 75f);

            GameObject movesObj = CreateText(movesBoxObj.transform, Vector2.zero, new Vector2(250f, 55f), "<color=#9FE8FF>MOVES:</color> <color=#FFDE38>40</color>", 34, FontStyle.Normal, Color.white, TextAnchor.MiddleCenter, hudFont);
            m_LevelMovesText = movesObj.GetComponent<Text>();
            m_LevelMovesText.resizeTextForBestFit = true;
            m_LevelMovesText.resizeTextMinSize = 24;
            m_LevelMovesText.resizeTextMaxSize = 34;
            MakeTextCrisp(m_LevelMovesText, new Color(0.01f, 0.04f, 0.18f, 0.95f), 1.0f);

            // 3. Right Plaque: Score with Star Icon - Centered over 3D gold plaque at X = 328, Y = -425
            GameObject scoreBoxObj = new GameObject("HeaderScoreBox");
            scoreBoxObj.transform.SetParent(m_LevelHUD.transform, false);
            RectTransform sbRt = scoreBoxObj.AddComponent<RectTransform>();
            sbRt.anchorMin = new Vector2(0.5f, 1f);
            sbRt.anchorMax = new Vector2(0.5f, 1f);
            sbRt.pivot = new Vector2(0.5f, 0.5f);
            sbRt.anchoredPosition = new Vector2(328f, -425f);
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
            gobRt.anchoredPosition = new Vector2(0f, -550f);
            gobRt.sizeDelta = new Vector2(760f, 76f);
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
            gRt.sizeDelta = new Vector2(460f, 65f);
            Image gBg = goalBadge.AddComponent<Image>();
            gBg.color = new Color(1f, 1f, 1f, 0.001f);
            gBg.raycastTarget = false;

            GameObject goalObj = CreateText(goalBadge.transform, Vector2.zero, new Vector2(440f, 55f), "Lines: 0/3", 26, FontStyle.Normal, new Color(0.40f, 0.98f, 0.70f), TextAnchor.MiddleCenter, hudFont);
            m_LevelGoalText = goalObj.GetComponent<Text>();
            m_LevelGoalText.resizeTextForBestFit = true;
            m_LevelGoalText.resizeTextMinSize = 16;
            m_LevelGoalText.resizeTextMaxSize = 26;
            m_LevelGoalBadgeObj = goalBadge;

            // 4 Dynamic Gem Slots inside the Capsule Bar
            m_GemObjectiveItems.Clear();
            GemType[] hudGems = new GemType[] { GemType.BlueDiamond, GemType.OrangePentagon, GemType.YellowStar, GemType.RedRuby };
            float[] offsets = new float[] { -225f, -75f, 75f, 225f };

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
                itemRt.sizeDelta = new Vector2(140f, 65f);

                // 3D Gem Icon
                GameObject iconObj = new GameObject("Icon");
                iconObj.transform.SetParent(itemObj.transform, false);
                RectTransform iconRt = iconObj.AddComponent<RectTransform>();
                iconRt.anchorMin = new Vector2(0.5f, 0.5f);
                iconRt.anchorMax = new Vector2(0.5f, 0.5f);
                iconRt.pivot = new Vector2(0.5f, 0.5f);
                iconRt.anchoredPosition = new Vector2(-30f, 0f);
                iconRt.sizeDelta = new Vector2(52f, 52f);
                Image iconImg = iconObj.AddComponent<Image>();
                iconImg.sprite = GemIconFactory.GetGemSprite(gType);
                iconImg.raycastTarget = false;

                // Crisp count text matching Concept 1 ("x15", "x10", etc.)
                GameObject countObj = CreateText(itemObj.transform, new Vector2(26f, 0f), new Vector2(76f, 46f), "x0", 36, FontStyle.Normal, Color.white, TextAnchor.MiddleLeft, hudFont);
                Text countTxt = countObj.GetComponent<Text>();
                countTxt.resizeTextForBestFit = true;
                countTxt.resizeTextMinSize = 24;
                countTxt.resizeTextMaxSize = 36;
                MakeTextCrisp(countTxt, new Color(0.01f, 0.05f, 0.18f, 0.95f), 1.0f);

                m_GemObjectiveItems[gType] = (itemObj, iconImg, countTxt, iconRt);
            }
            m_GemObjectivesBar.SetActive(false);

            m_LevelHUD.SetActive(false);
        }

        private void BuildModeSelectScreen()
        {
            m_ModeSelectPanel = new GameObject("ModeSelectPanel");
            m_ModeSelectPanel.transform.SetParent(m_RootCanvas, false);
            RectTransform rt = m_ModeSelectPanel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.sizeDelta = Vector2.zero;

            // Fullscreen Cosmic Studio Vignette Backdrop (Concept 3)
            Image bg = m_ModeSelectPanel.AddComponent<Image>();
            bg.sprite = SpriteFactory.GetHomeBackgroundConcept3Sprite();
            bg.color = Color.white;
            bg.raycastTarget = true;

            // Background Volumetric Starburst Light Needles (radiating from center cluster)
            GameObject bgRaysObj = new GameObject("Background_StarburstRays");
            bgRaysObj.transform.SetParent(m_ModeSelectPanel.transform, false);
            RectTransform bgrRt = bgRaysObj.AddComponent<RectTransform>();
            bgrRt.anchorMin = new Vector2(0.5f, 0.5f);
            bgrRt.anchorMax = new Vector2(0.5f, 0.5f);
            bgrRt.anchoredPosition = new Vector2(0f, 20f);
            bgrRt.sizeDelta = new Vector2(1300f, 1300f);
            Image bgrImg = bgRaysObj.AddComponent<Image>();
            bgrImg.sprite = SpriteFactory.GetStarburstRaysSprite();
            bgrImg.color = new Color(1f, 1f, 1f, 0.18f); // Subtle shimmer over background rays
            bgrImg.raycastTarget = false;

            // Safe Area calculations for Android mobile notches, punch holes, and curved edges
            float safeTop = 0f;
            float safeBottom = 0f;
            float safeLeft = 0f;
            float safeRight = 0f;
            float refHeight = 1920f;
            float refWidth = 1080f;
            if (Screen.height > 0 && Screen.width > 0)
            {
                Rect safeArea = Screen.safeArea;
                float topPixels = Screen.height - (safeArea.y + safeArea.height);
                float bottomPixels = safeArea.y;
                float leftPixels = safeArea.x;
                float rightPixels = Screen.width - (safeArea.x + safeArea.width);

                float scale = refWidth / Screen.width;
                refHeight = Screen.height * scale;

                safeTop = topPixels * scale;
                safeBottom = bottomPixels * scale;
                safeLeft = leftPixels * scale;
                safeRight = rightPixels * scale;
            }
            float padTop = Mathf.Max(safeTop + 30f, 80f);
            float padBottom = Mathf.Max(safeBottom + 40f, 90f);
            float padLeft = Mathf.Max(safeLeft + 35f, 55f);
            float padRight = Mathf.Max(safeRight + 35f, 55f);

            // Top-Left: Settings Gear Button (Circular tactile button)
            m_HomeSettingsBtn = CreateCircularIconButton(m_ModeSelectPanel.transform, new Vector2(padLeft, -padTop), 72, GemIconFactory.GetGearSprite(), new Color(0.10f, 0.28f, 0.70f, 0.85f));
            RectTransform stRt = m_HomeSettingsBtn.GetComponent<RectTransform>();
            stRt.anchorMin = new Vector2(0f, 1f); stRt.anchorMax = new Vector2(0f, 1f); stRt.pivot = new Vector2(0f, 1f);
            m_HomeSettingsBtn.GetComponent<Button>().onClick.AddListener(OpenSettings);

            // Top-Right: "Medal" Icon Button (matching reference image: Medal icon with "Medal" text underneath)
            GameObject medalBtnObj = new GameObject("Button_Medal");
            medalBtnObj.transform.SetParent(m_ModeSelectPanel.transform, false);
            RectTransform medRt = medalBtnObj.AddComponent<RectTransform>();
            medRt.anchorMin = new Vector2(1f, 1f); medRt.anchorMax = new Vector2(1f, 1f); medRt.pivot = new Vector2(1f, 1f);
            medRt.anchoredPosition = new Vector2(-padRight, -padTop);
            medRt.sizeDelta = new Vector2(80, 90);

            Image medImg = medalBtnObj.AddComponent<Image>();
            medImg.color = Color.clear;
            medImg.raycastTarget = true;

            GameObject medalIconObj = new GameObject("MedalIcon");
            medalIconObj.transform.SetParent(medalBtnObj.transform, false);
            RectTransform miRt = medalIconObj.AddComponent<RectTransform>();
            miRt.anchoredPosition = new Vector2(0, 14);
            miRt.sizeDelta = new Vector2(60, 60);
            Image miImg = medalIconObj.AddComponent<Image>();
            miImg.sprite = GemIconFactory.GetMedalSprite();
            miImg.raycastTarget = false;

            GameObject medTxtObj = CreateText(medalBtnObj.transform, new Vector2(0, -28), new Vector2(80, 24), "Medal", 19, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, m_CandyFont);
            Shadow medTxtSh = medTxtObj.AddComponent<Shadow>();
            medTxtSh.effectColor = new Color(0.04f, 0.15f, 0.45f, 0.90f);
            medTxtSh.effectDistance = new Vector2(1, -1);

            Button medBtn = medalBtnObj.AddComponent<Button>();
            medBtn.onClick.AddListener(() =>
            {
                if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
                HapticManager.TriggerLight();
                StartCoroutine(AnimateButtonPress(medRt, OpenSkinWardrobe));
            });
            m_HomeWardrobeBtn = medalBtnObj;

            // Concept 3: Floating Bokeh (Built directly into high-res background plate)
            var bokehItems = new List<BokehDriftData>();

            // Center Content Holder (scaled dynamically for 1080p Android phones across 16:9, 18:9, 19.5:9, 20:9)
            GameObject centerObj = new GameObject("HomeCenterContent");
            centerObj.transform.SetParent(m_ModeSelectPanel.transform, false);
            RectTransform cRt = centerObj.AddComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0.5f, 0.5f);
            cRt.anchorMax = new Vector2(0.5f, 0.5f);
            cRt.anchoredPosition = Vector2.zero;
            cRt.sizeDelta = new Vector2(900, refHeight);

            // Responsive vertical layout coordinates matching Concept 3 artwork:
            float titleOffsetY = Mathf.Clamp((refHeight - 1920f) * 0.18f, 0f, 85f);
            float titleCenterY = 460f + titleOffsetY;

            float clBtnY = -(refHeight * 0.28f);
            float minAllowedClY = -(refHeight / 2f) + padBottom + 64f + 25f;
            if (clBtnY < minAllowedClY) clBtnY = minAllowedClY;
            float advBtnY = clBtnY + 160f;
            float clusterY = 20f;

            // 0. Ambient Celestial Logo Glow Halo (Soft radial glow centered behind title)
            GameObject titleGlowObj = new GameObject("TitleBackdropGlow");
            titleGlowObj.transform.SetParent(centerObj.transform, false);
            RectTransform tgRt = titleGlowObj.AddComponent<RectTransform>();
            tgRt.anchoredPosition = new Vector2(0, titleCenterY);
            tgRt.sizeDelta = new Vector2(720, 420);
            Image tgImg = titleGlowObj.AddComponent<Image>();
            tgImg.sprite = SpriteFactory.GetSoftGlowOrbSprite();
            tgImg.color = new Color(0.20f, 0.52f, 1.0f, 0.28f);
            tgImg.raycastTarget = false;

            // 1. Center 3D Spheres Orbital Cluster with Dynamic Visuals & Juicy Animations!
            // Placed symmetrically in the center over the glowing starburst core
            GameObject spheresClusterObj = new GameObject("CenterSpheresCluster");
            spheresClusterObj.transform.SetParent(centerObj.transform, false);
            RectTransform clusterRt = spheresClusterObj.AddComponent<RectTransform>();
            clusterRt.anchoredPosition = new Vector2(0, clusterY);
            clusterRt.sizeDelta = new Vector2(420, 420);

            HomeSpheresAnimator sphereAnim = spheresClusterObj.AddComponent<HomeSpheresAnimator>();
            sphereAnim.SetBaseCenterY(clusterY);
            sphereAnim.Initialize();

            // 2. Master 3D Candy Balloon Title Logo ("SPHERE BLAST" + Chubby Golden Crown)
            GameObject logoObj = new GameObject("Title_SphereBlast_MasterLogo");
            logoObj.transform.SetParent(centerObj.transform, false);
            RectTransform logoRt = logoObj.AddComponent<RectTransform>();
            logoRt.anchoredPosition = new Vector2(0, titleCenterY);
            logoRt.sizeDelta = new Vector2(640, 440);

            Image logoImg = logoObj.AddComponent<Image>();
            logoImg.sprite = SpriteFactory.GetSphereBlastTitleLogoSprite();
            logoImg.preserveAspect = true;
            logoImg.raycastTarget = false;

            // Interactive Golden Crown tap reaction area
            GameObject crownTapObj = new GameObject("Crown_InteractiveZone");
            crownTapObj.transform.SetParent(logoObj.transform, false);
            RectTransform crRt = crownTapObj.AddComponent<RectTransform>();
            crRt.anchoredPosition = new Vector2(0f, 130f);
            crRt.sizeDelta = new Vector2(180, 140);
            Button crBtn = crownTapObj.AddComponent<Button>();
            crBtn.transition = Selectable.Transition.None;
            crBtn.onClick.AddListener(() =>
            {
                if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
                HapticManager.TriggerLight();
                StartCoroutine(AnimateButtonPress(logoRt, null));
            });

            // Diamond Twinkle Star on crown peak
            GameObject crownStarGlint = new GameObject("CrownStarGlint");
            crownStarGlint.transform.SetParent(crownTapObj.transform, false);
            RectTransform csgRt = crownStarGlint.AddComponent<RectTransform>();
            csgRt.anchoredPosition = new Vector2(0f, 32f);
            csgRt.sizeDelta = new Vector2(34, 34);
            Image csgImg = crownStarGlint.AddComponent<Image>();
            csgImg.sprite = GemIconFactory.GetStarSprite(true);
            csgImg.color = new Color(1f, 1f, 1f, 0.95f);
            csgImg.raycastTarget = false;

            // 5a. Adventure Button (Concept 3 Pixar-Grade 3D Pill Button with baked 3D text & map pin badge)
            GameObject advBtnObj = new GameObject("Button_Adventure");
            advBtnObj.transform.SetParent(centerObj.transform, false);
            RectTransform advRt = advBtnObj.AddComponent<RectTransform>();
            advRt.anchoredPosition = new Vector2(0, advBtnY);
            advRt.sizeDelta = new Vector2(530, 148);

            Image advImg = advBtnObj.AddComponent<Image>();
            advImg.sprite = SpriteFactory.GetAdventure3DButtonSprite();
            advImg.preserveAspect = true;
            advImg.raycastTarget = true;

            Button advBtn = advBtnObj.AddComponent<Button>();
            advBtn.onClick.AddListener(() =>
            {
                if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
                HapticManager.TriggerLight();
                StartCoroutine(AnimateButtonPress(advRt, () =>
                {
                    m_ModeSelectPanel.SetActive(false);
                    OpenLevelSelect(GameModeType.Adventure);
                }));
            });

            // 6a. Classic Button (Concept 3 Pixar-Grade 3D Pill Button with baked 3D text & rainbow infinity jewel)
            GameObject classicBtnObj = new GameObject("Button_Classic");
            classicBtnObj.transform.SetParent(centerObj.transform, false);
            RectTransform clRt = classicBtnObj.AddComponent<RectTransform>();
            clRt.anchoredPosition = new Vector2(0, clBtnY);
            clRt.sizeDelta = new Vector2(530, 134);

            Image clImg = classicBtnObj.AddComponent<Image>();
            clImg.sprite = SpriteFactory.GetClassic3DButtonSprite();
            clImg.preserveAspect = true;
            clImg.raycastTarget = true;

            Button clBtn = classicBtnObj.AddComponent<Button>();
            clBtn.onClick.AddListener(() =>
            {
                if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
                HapticManager.TriggerLight();
                StartCoroutine(AnimateButtonPress(clRt, () =>
                {
                    m_ModeSelectPanel.SetActive(false);
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.StartClassicGame();
                    }
                    UpdateHUDVisibility();
                }));
            });

            // 7. Living Animations: Add HomeButtonBreather for gentle idle pulse, crown twinkle & drifting rays
            HomeButtonBreather breather = centerObj.AddComponent<HomeButtonBreather>();
            breather.classicBtnRt = clRt;
            breather.adventureBtnRt = advRt;
            breather.logoRt = logoRt;
            breather.crownStarGlintRt = csgRt;
            breather.bgRaysRt = bgrRt;
            if (bokehItems != null) breather.bokehItems = bokehItems;

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

            // 1. Concept 2 AAA 3D Artwork Backdrop
            Image mapBg = m_LevelSelectPanel.AddComponent<Image>();
            Sprite mapSprite = SpriteFactory.LoadOrFallbackSprite("AdventureMap_Concept2");
            if (mapSprite == null) mapSprite = SpriteFactory.LoadOrFallbackSprite("HomeBackground_Concept3");
            if (mapSprite != null)
            {
                mapBg.sprite = mapSprite;
                mapBg.color = Color.white;
            }
            else
            {
                mapBg.color = new Color(0.04f, 0.07f, 0.16f, 1f);
            }

            float canvasHeight = 1920f;
            if (m_RootCanvas != null)
            {
                RectTransform canvasRt = m_RootCanvas.GetComponent<RectTransform>();
                if (canvasRt != null && canvasRt.rect.height > 0)
                {
                    canvasHeight = canvasRt.rect.height;
                }
            }
            float scaleY = canvasHeight / 1920f;

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

            // 2. Interactive Navigation Buttons directly mapped to the artwork
            // A. Back Button (< HOME) - Anchored to Top-Left with generous touch target & safe area
            GameObject backBtn = new GameObject("BackButton");
            backBtn.transform.SetParent(m_LevelSelectPanel.transform, false);
            RectTransform bbRt = backBtn.AddComponent<RectTransform>();
            bbRt.anchorMin = new Vector2(0f, 1f);
            bbRt.anchorMax = new Vector2(0f, 1f);
            bbRt.pivot = new Vector2(0f, 1f);
            float homeLeft = Mathf.Max(safeLeft + 15f, 25f);
            float homeTop = Mathf.Max(safeTop + 10f, 79f * scaleY - 45f);
            bbRt.anchoredPosition = new Vector2(homeLeft, -homeTop);
            bbRt.sizeDelta = new Vector2(250f, 105f);

            Image bbImg = backBtn.AddComponent<Image>();
            bbImg.color = new Color(1f, 1f, 1f, 0.001f);
            bbImg.raycastTarget = true;

            Button bbComp = backBtn.AddComponent<Button>();
            ColorBlock bcb = bbComp.colors;
            bcb.normalColor = new Color(1f, 1f, 1f, 0.001f);
            bcb.highlightedColor = new Color(0.12f, 0.88f, 0.78f, 0.25f);
            bcb.pressedColor = new Color(0.12f, 0.88f, 0.78f, 0.45f);
            bbComp.colors = bcb;

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

            // Floating Side Chapter Buttons - Docked at bottom corners away from trail tracks
            // Left Floating Arrow (Bottom-Left)
            GameObject prevSideObj = new GameObject("PrevChapterSideButton");
            prevSideObj.transform.SetParent(m_LevelSelectPanel.transform, false);
            m_PrevChapterSideRt = prevSideObj.AddComponent<RectTransform>();
            m_PrevChapterSideRt.anchorMin = new Vector2(0f, 0f);
            m_PrevChapterSideRt.anchorMax = new Vector2(0f, 0f);
            m_PrevChapterSideRt.pivot = new Vector2(0f, 0f);
            m_PrevChapterSideRt.anchoredPosition = new Vector2(Mathf.Max(safeLeft + 20f, 26f), 30f);
            m_PrevChapterSideRt.sizeDelta = new Vector2(130f, 68f);

            Image prevImg = prevSideObj.AddComponent<Image>();
            prevImg.sprite = SpriteFactory.GetHeaderPlaqueSprite(false);
            prevImg.color = Color.white;
            prevImg.raycastTarget = true;

            GameObject prevTxtObj = CreateText(prevSideObj.transform, Vector2.zero, new Vector2(130f, 68f), "◀ CH 1", 21, FontStyle.Bold, new Color(1.0f, 0.88f, 0.35f), TextAnchor.MiddleCenter, m_CandyFont);
            m_PrevChapterSideText = prevTxtObj.GetComponent<Text>();
            MakeTextCrisp(m_PrevChapterSideText, new Color(0.35f, 0.18f, 0.02f, 0.95f), 1.0f);

            Button prevComp = prevSideObj.AddComponent<Button>();
            prevComp.onClick.AddListener(() =>
            {
                if (m_IsLevelSelectTransitioning) return;
                if (m_LevelSelectPage > 0)
                {
                    m_IsLevelSelectTransitioning = true;
                    if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
                    HapticManager.TriggerLight();
                    StartCoroutine(AnimateButtonPress(m_PrevChapterSideRt, () =>
                    {
                        m_LevelSelectPage--;
                        RefreshLevelSelectGrid(m_ActiveViewingMode);
                        m_IsLevelSelectTransitioning = false;
                    }));
                }
            });

            // Right Floating Arrow (Bottom-Right)
            GameObject nextSideObj = new GameObject("NextChapterSideButton");
            nextSideObj.transform.SetParent(m_LevelSelectPanel.transform, false);
            m_NextChapterSideRt = nextSideObj.AddComponent<RectTransform>();
            m_NextChapterSideRt.anchorMin = new Vector2(1f, 0f);
            m_NextChapterSideRt.anchorMax = new Vector2(1f, 0f);
            m_NextChapterSideRt.pivot = new Vector2(1f, 0f);
            m_NextChapterSideRt.anchoredPosition = new Vector2(-Mathf.Max(safeLeft + 20f, 26f), 30f);
            m_NextChapterSideRt.sizeDelta = new Vector2(130f, 68f);

            Image nextImg = nextSideObj.AddComponent<Image>();
            nextImg.sprite = SpriteFactory.GetHeaderPlaqueSprite(true);
            nextImg.color = Color.white;
            nextImg.raycastTarget = true;

            GameObject nextTxtObj = CreateText(nextSideObj.transform, Vector2.zero, new Vector2(130f, 68f), "CH 2 ▶", 21, FontStyle.Bold, new Color(1.0f, 0.95f, 0.50f), TextAnchor.MiddleCenter, m_CandyFont);
            m_NextChapterSideText = nextTxtObj.GetComponent<Text>();
            MakeTextCrisp(m_NextChapterSideText, new Color(0.40f, 0.20f, 0.02f, 0.95f), 1.0f);

            Button nextComp = nextSideObj.AddComponent<Button>();
            nextComp.onClick.AddListener(() =>
            {
                if (m_IsLevelSelectTransitioning) return;
                int maxPage = (LevelManager.GetMaxLevel(m_ActiveViewingMode) - 1) / 20;
                if (m_LevelSelectPage < maxPage)
                {
                    m_IsLevelSelectTransitioning = true;
                    if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
                    HapticManager.TriggerLight();
                    StartCoroutine(AnimateButtonPress(m_NextChapterSideRt, () =>
                    {
                        m_LevelSelectPage++;
                        RefreshLevelSelectGrid(m_ActiveViewingMode);
                        m_IsLevelSelectTransitioning = false;
                    }));
                }
            });

            // B. Ball Designs Button (Wardrobe)
            GameObject wardrobeBtn = new GameObject("WardrobeButton");
            wardrobeBtn.transform.SetParent(m_LevelSelectPanel.transform, false);
            m_LevelSelectWardrobeRt = wardrobeBtn.AddComponent<RectTransform>();
            m_LevelSelectWardrobeRt.anchorMin = new Vector2(0.5f, 1f);
            m_LevelSelectWardrobeRt.anchorMax = new Vector2(0.5f, 1f);
            m_LevelSelectWardrobeRt.pivot = new Vector2(0.5f, 1f);
            m_LevelSelectWardrobeRt.anchoredPosition = new Vector2(130.8f, -canvasHeight * (362.5f / 1376f));
            m_LevelSelectWardrobeRt.sizeDelta = new Vector2(235f, 52f);
            Image wbImg = wardrobeBtn.AddComponent<Image>();
            wbImg.color = new Color(1f, 1f, 1f, 0.001f);
            wbImg.raycastTarget = true;
            Button wbComp = wardrobeBtn.AddComponent<Button>();
            ColorBlock wcb = wbComp.colors;
            wcb.normalColor = new Color(1f, 1f, 1f, 0.001f);
            wcb.highlightedColor = new Color(0.85f, 0.35f, 0.95f, 0.25f);
            wcb.pressedColor = new Color(0.85f, 0.35f, 0.95f, 0.45f);
            wbComp.colors = wcb;
            wbComp.onClick.AddListener(() =>
            {
                if (m_IsLevelSelectTransitioning) return;
                if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
                HapticManager.TriggerLight();
                OpenSkinWardrobe();
            });

            // C. Dynamic Stars Badge Text (directly mapped inside the artwork's golden pill)
            GameObject starsBadgeObj = new GameObject("StarsBadge");
            starsBadgeObj.transform.SetParent(m_LevelSelectPanel.transform, false);
            m_LevelSelectStarsRt = starsBadgeObj.AddComponent<RectTransform>();
            m_LevelSelectStarsRt.anchorMin = new Vector2(0.5f, 1f);
            m_LevelSelectStarsRt.anchorMax = new Vector2(0.5f, 1f);
            m_LevelSelectStarsRt.pivot = new Vector2(0.5f, 0.5f);
            m_LevelSelectStarsRt.anchoredPosition = new Vector2(-114f, -canvasHeight * (357.5f / 1376f));
            m_LevelSelectStarsRt.sizeDelta = new Vector2(180f, 38f);

            GameObject starsTxtObj = CreateText(starsBadgeObj.transform, Vector2.zero, new Vector2(180f, 38f), "", 24, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, m_CandyFont);
            m_LevelSelectStarsText = starsTxtObj.GetComponent<Text>();
            MakeTextCrisp(m_LevelSelectStarsText, new Color(0.40f, 0.22f, 0.02f, 0.95f), 1.0f);

            // 3. Node Container for interactive level spheres & chests
            GameObject nodeContainer = new GameObject("NodeContainer");
            nodeContainer.transform.SetParent(m_LevelSelectPanel.transform, false);
            RectTransform ncRt = nodeContainer.AddComponent<RectTransform>();
            ncRt.anchorMin = Vector2.zero; ncRt.anchorMax = Vector2.one; ncRt.sizeDelta = Vector2.zero;
            m_LevelGridContainer = nodeContainer.transform;

            m_LevelSelectPanel.SetActive(false);
        }

        private void RefreshLevelSelectGrid(GameModeType mode)
        {
            if (m_LevelGridContainer == null) return;
            m_ActiveViewingMode = mode;
            m_IsLevelSelectTransitioning = false;

            float canvasHeight = 1920f;
            if (m_RootCanvas != null)
            {
                RectTransform canvasRt = m_RootCanvas.GetComponent<RectTransform>();
                if (canvasRt != null && canvasRt.rect.height > 0)
                {
                    canvasHeight = canvasRt.rect.height;
                }
            }
            float scaleY = canvasHeight / 1920f;

            if (m_LevelSelectStarsRt != null)
            {
                m_LevelSelectStarsRt.pivot = new Vector2(0.5f, 0.5f);
                m_LevelSelectStarsRt.anchoredPosition = new Vector2(-114f, -canvasHeight * (357.5f / 1376f));
            }
            if (m_LevelSelectWardrobeRt != null)
            {
                m_LevelSelectWardrobeRt.anchoredPosition = new Vector2(130.8f, -canvasHeight * (362.5f / 1376f));
            }

            int maxLvl = LevelManager.GetMaxLevel(mode);
            int maxPage = (maxLvl - 1) / 20;

            if (m_PrevChapterSideRt != null)
            {
                m_PrevChapterSideRt.gameObject.SetActive(m_LevelSelectPage > 0);
                if (m_PrevChapterSideText != null) m_PrevChapterSideText.text = $"◀ CH {m_LevelSelectPage}";
            }
            if (m_NextChapterSideRt != null)
            {
                m_NextChapterSideRt.gameObject.SetActive(m_LevelSelectPage < maxPage);
                if (m_NextChapterSideText != null) m_NextChapterSideText.text = $"CH {m_LevelSelectPage + 2} ▶";
            }

            int totalStars = GetTotalModeStars(mode);
            if (m_LevelSelectStarsText != null)
            {
                m_LevelSelectStarsText.text = $"{totalStars} / {maxLvl * 3} STARS";
            }

            // Clear previous child overlays
            for (int i = m_LevelGridContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(m_LevelGridContainer.GetChild(i).gameObject);
            }

            // Calculate the current active play level:
            // The lowest unlocked level that is not yet completed (or highest unlocked if all completed)
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

            // Exact 20 Node Coordinates measured directly from Royal Gem Adventure Map
            Vector2[] nodePositions = new Vector2[]
            {
                new Vector2(  -9.8f, -812.1f), // Level 1 (Center bottom start)
                new Vector2( 147.7f, -675.3f), // Level 2
                new Vector2( 305.2f, -566.5f), // Level 3
                new Vector2( 198.3f, -463.3f), // Level 4
                new Vector2(  -8.4f, -496.7f), // Level 5
                new Vector2(-201.1f, -516.3f), // Level 6
                new Vector2(-317.8f, -428.4f), // Level 7
                new Vector2(-151.9f, -263.7f), // Level 8
                new Vector2(  33.8f, -273.5f), // Level 9
                new Vector2( 213.8f, -279.1f), // Level 10
                new Vector2( 327.7f, -180.0f), // Level 11
                new Vector2( 206.7f,  -74.0f), // Level 12
                new Vector2(-199.7f,  -53.0f), // Level 13
                new Vector2(-312.2f,   32.1f), // Level 14
                new Vector2(-191.2f,  160.5f), // Level 15
                new Vector2( -26.7f,  161.9f), // Level 16
                new Vector2( 170.2f,  140.9f), // Level 17
                new Vector2( 320.6f,  209.3f), // Level 18
                new Vector2( 191.2f,  318.1f), // Level 19 (Top middle-right golden disc)
                new Vector2(   0.0f,  340.5f), // Level 20 (Top center-left final golden disc)
            };

            string[] sphereSpriteNames = new string[]
            {
                "Sphere_3D_Cyan",    // Level 1
                "Sphere_3D_Pink",    // Level 2
                "Sphere_3D_Yellow",  // Level 3
                "Sphere_3D_Green",   // Level 4
                "Sphere_3D_Purple",  // Level 5
                "Sphere_3D_Red",     // Level 6
                "Sphere_3D_Blue",    // Level 7
                "Sphere_3D_Pink",    // Level 8
                "Sphere_3D_Orange",  // Level 9
                "Sphere_3D_Green",   // Level 10
                "Sphere_3D_Purple",  // Level 11
                "Sphere_3D_Cyan",    // Level 12
                "Sphere_3D_Orange",  // Level 13
                "Sphere_3D_Pink",    // Level 14
                "Sphere_3D_Blue",    // Level 15
                "Sphere_3D_Green",   // Level 16
                "Sphere_3D_Purple",  // Level 17
                "Sphere_3D_Yellow",  // Level 18
                "Sphere_3D_Red",     // Level 19
                "Sphere_3D_Cyan",    // Level 20
            };

            int startLevel = m_LevelSelectPage * 20 + 1;
            for (int i = 0; i < nodePositions.Length; i++)
            {
                int targetLvl = startLevel + i;
                if (targetLvl > maxLvl) break;

                bool isUnlocked = LevelManager.IsLevelUnlocked(mode, targetLvl);
                int stars = LevelManager.GetLevelStars(mode, targetLvl);
                bool isCurrentActive = (targetLvl == currentPlayLevel);

                GameObject btnObj = new GameObject($"LevelButton_{targetLvl}");
                btnObj.transform.SetParent(m_LevelGridContainer, false);
                RectTransform bRt = btnObj.AddComponent<RectTransform>();
                bRt.anchoredPosition = new Vector2(nodePositions[i].x, nodePositions[i].y * scaleY);
                float btnSize = isCurrentActive ? 174f : 160f;
                bRt.sizeDelta = new Vector2(btnSize, btnSize);

                // 1. If Active: Add glowing pulsing aura halo behind the sphere
                if (isCurrentActive)
                {
                    GameObject auraObj = new GameObject("ActiveAura");
                    auraObj.transform.SetParent(btnObj.transform, false);
                    RectTransform aRt = auraObj.AddComponent<RectTransform>();
                    aRt.sizeDelta = new Vector2(215f, 215f);
                    Image aImg = auraObj.AddComponent<Image>();
                    aImg.sprite = SpriteFactory.GetCircleSprite(new Color(1.0f, 0.88f, 0.25f, 0.45f));
                    aImg.raycastTarget = false;
                    StartCoroutine(PulseActiveHalo(aRt));
                }

                // 2. Base Sphere Image
                GameObject sphereObj = new GameObject("SphereImg");
                sphereObj.transform.SetParent(btnObj.transform, false);
                RectTransform sRt = sphereObj.AddComponent<RectTransform>();
                sRt.sizeDelta = new Vector2(btnSize, btnSize);
                Image sImg = sphereObj.AddComponent<Image>();
                sImg.raycastTarget = false;

                if (!isUnlocked)
                {
                    // Locked: Deep space navy metallic 3D sphere with visible highlights
                    Sprite blueSphere = SpriteFactory.LoadOrFallbackSprite("Sphere_3D_Blue");
                    if (blueSphere != null)
                    {
                        sImg.sprite = blueSphere;
                        sImg.color = new Color(0.28f, 0.38f, 0.62f, 1f);
                    }
                    else
                    {
                        sImg.sprite = SpriteFactory.GetCircleSprite(new Color(0.18f, 0.24f, 0.42f, 1f));
                    }

                    // Level Number Text in top half of locked sphere (large & bold for mobile screens)
                    GameObject numObj = CreateText(btnObj.transform, new Vector2(0f, 20f), new Vector2(104f, 48f), targetLvl.ToString(), 42, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, m_CandyFont);
                    Text numTxt = numObj.GetComponent<Text>();
                    MakeTextCrisp(numTxt, new Color(0.04f, 0.08f, 0.20f, 0.95f), 1.2f);

                    // Clean Golden Padlock Icon in bottom half of locked sphere
                    GameObject lockObj = new GameObject("LockIcon");
                    lockObj.transform.SetParent(btnObj.transform, false);
                    RectTransform lRt = lockObj.AddComponent<RectTransform>();
                    lRt.anchoredPosition = new Vector2(0f, -24f);
                    lRt.sizeDelta = new Vector2(40f, 46f);
                    Image lImg = lockObj.AddComponent<Image>();
                    lImg.sprite = SpriteFactory.LoadOrFallbackSprite("Wardrobe_Icon_Lock");
                    if (lImg.sprite == null) lImg.sprite = GemIconFactory.GetStarSprite(true);
                    lImg.color = Color.white;
                    lImg.raycastTarget = false;
                }
                else
                {
                    // Unlocked (Active or Completed): Rich vibrant 3D sphere
                    string spName = (i < sphereSpriteNames.Length) ? sphereSpriteNames[i] : "Sphere_3D_Cyan";
                    Sprite sphereSprite = SpriteFactory.LoadOrFallbackSprite(spName);
                    if (sphereSprite != null)
                    {
                        sImg.sprite = sphereSprite;
                        sImg.color = Color.white;
                    }
                    else
                    {
                        sImg.sprite = SpriteFactory.GetCircleSprite(new Color(0.2f, 0.7f, 1f));
                    }

                    // Level Number Text in Center of Sphere (prominent & juicy for mobile)
                    GameObject numObj = CreateText(btnObj.transform, Vector2.zero, new Vector2(110f, 70f), targetLvl.ToString(), isCurrentActive ? 52 : 46, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, m_CandyFont);
                    Text numTxt = numObj.GetComponent<Text>();
                    MakeTextCrisp(numTxt, new Color(0.02f, 0.04f, 0.14f, 0.95f), 1.2f);

                    if (isCurrentActive)
                    {
                        // Active Play Badge above the sphere
                        GameObject playBadgeObj = new GameObject("PlayBadge");
                        playBadgeObj.transform.SetParent(btnObj.transform, false);
                        RectTransform pbRt = playBadgeObj.AddComponent<RectTransform>();
                        pbRt.anchoredPosition = new Vector2(0f, 74f);
                        pbRt.sizeDelta = new Vector2(118f, 44f);

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

                        GameObject playTxtObj = CreateText(playBadgeObj.transform, Vector2.zero, new Vector2(118f, 42f), "PLAY", 24, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, m_CandyFont);
                        Text playTxt = playTxtObj.GetComponent<Text>();
                        MakeTextCrisp(playTxt, new Color(0.55f, 0.25f, 0.02f, 0.95f), 1.0f);
                    }

                    // Stars Display: If level has earned stars, show prominent, gorgeous 3D gold jewel stars
                    if (stars > 0)
                    {
                        GameObject starsTray = new GameObject("StarsTray");
                        starsTray.transform.SetParent(btnObj.transform, false);
                        RectTransform stRt = starsTray.AddComponent<RectTransform>();
                        stRt.anchoredPosition = new Vector2(0f, -74f);
                        stRt.sizeDelta = new Vector2(168f, 54f);

                        // Polished royal navy glass pill with golden rim (replaces harsh pitch-black box)
                        Image stBg = starsTray.AddComponent<Image>();
                        stBg.sprite = SpriteFactory.GetRoundedButtonSprite(new Color(0.06f, 0.14f, 0.32f, 0.88f), new Color(0.98f, 0.82f, 0.25f, 0.90f), 0.10f);
                        stBg.raycastTarget = false;

                        // Star 1 (Left: 48x48, tilted +9°)
                        GameObject s1 = new GameObject("Star_1");
                        s1.transform.SetParent(starsTray.transform, false);
                        RectTransform s1Rt = s1.AddComponent<RectTransform>();
                        s1Rt.anchoredPosition = new Vector2(-46f, 1f);
                        s1Rt.sizeDelta = new Vector2(48f, 48f);
                        s1Rt.localEulerAngles = new Vector3(0f, 0f, 9f);
                        Image s1Img = s1.AddComponent<Image>();
                        s1Img.sprite = GemIconFactory.GetStarSprite(stars >= 1);
                        s1Img.raycastTarget = false;

                        // Star 2 (Center Hero Crown: 58x58, elevated at Y = 7)
                        GameObject s2 = new GameObject("Star_2");
                        s2.transform.SetParent(starsTray.transform, false);
                        RectTransform s2Rt = s2.AddComponent<RectTransform>();
                        s2Rt.anchoredPosition = new Vector2(0f, 7f);
                        s2Rt.sizeDelta = new Vector2(58f, 58f);
                        Image s2Img = s2.AddComponent<Image>();
                        s2Img.sprite = GemIconFactory.GetStarSprite(stars >= 2);
                        s2Img.raycastTarget = false;

                        // Star 3 (Right: 48x48, tilted -9°)
                        GameObject s3 = new GameObject("Star_3");
                        s3.transform.SetParent(starsTray.transform, false);
                        RectTransform s3Rt = s3.AddComponent<RectTransform>();
                        s3Rt.anchoredPosition = new Vector2(46f, 1f);
                        s3Rt.sizeDelta = new Vector2(48f, 48f);
                        s3Rt.localEulerAngles = new Vector3(0f, 0f, -9f);
                        Image s3Img = s3.AddComponent<Image>();
                        s3Img.sprite = GemIconFactory.GetStarSprite(stars >= 3);
                        s3Img.raycastTarget = false;
                    }
                }

                // 3. Invisible Button Overlay for Touch Interaction
                Image bImg = btnObj.AddComponent<Image>();
                bImg.color = new Color(1f, 1f, 1f, 0.001f);
                bImg.raycastTarget = true;

                Button btn = btnObj.AddComponent<Button>();
                ColorBlock lcb = btn.colors;
                lcb.normalColor = new Color(1f, 1f, 1f, 0.001f);
                lcb.highlightedColor = new Color(1f, 1f, 1f, 0.15f);
                lcb.pressedColor = new Color(1f, 0.90f, 0.30f, 0.35f);
                btn.colors = lcb;

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
                        thRt.sizeDelta = new Vector2(btnSize * 1.35f, btnSize * 1.35f);
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

            // Milestone Chests mapped to 3D chests along the cosmic road
            (Vector2 pos, string skinName)[] milestoneChests;
            if (m_LevelSelectPage == 0)
            {
                milestoneChests = new (Vector2, string)[]
                {
                    (new Vector2(-399.4f, -337.7f * scaleY), "Star Core Ball Skin!"),
                    (new Vector2( 416.2f, -491.2f * scaleY), "Orange Planet Ball Skin!"),
                    (new Vector2( 402.2f,  -58.6f * scaleY), "Mystic Orb Ball Skin!"),
                    (new Vector2(-399.4f,  108.8f * scaleY), "Solar Flare Ball Skin!"),
                    (new Vector2( 353.0f,  304.2f * scaleY), "Cyan Wave Ball Skin!")
                };
            }
            else
            {
                milestoneChests = new (Vector2, string)[]
                {
                    (new Vector2(-399.4f, -337.7f * scaleY), "Cosmic Matrix Vault: +500 Coins & Extra Moves!"),
                    (new Vector2( 416.2f, -491.2f * scaleY), "Starlight Treasury: +800 Coins & Power Burst!"),
                    (new Vector2( 402.2f,  -58.6f * scaleY), "Nebula Cache: +1,000 Coins & Star Gems!"),
                    (new Vector2(-399.4f,  108.8f * scaleY), "Grand Valkyrie Vault: +1,500 Coins & Royal Shield!"),
                    (new Vector2( 353.0f,  304.2f * scaleY), "Crown of Sovereigns: Master Champion Trophy!")
                };
            }

            for (int ci = 0; ci < milestoneChests.Length; ci++)
            {
                var chestInfo = milestoneChests[ci];
                GameObject chestObj = new GameObject($"MilestoneChest_{ci + 1}");
                chestObj.transform.SetParent(m_LevelGridContainer, false);
                RectTransform cRt = chestObj.AddComponent<RectTransform>();
                cRt.anchoredPosition = chestInfo.pos;
                cRt.sizeDelta = new Vector2(130f, 130f);
                Image cImg = chestObj.AddComponent<Image>();
                cImg.color = new Color(1f, 1f, 1f, 0.001f);
                cImg.raycastTarget = true;
                Button cBtn = chestObj.AddComponent<Button>();
                string rewardText = chestInfo.skinName;
                cBtn.onClick.AddListener(() =>
                {
                    if (m_IsLevelSelectTransitioning) return;
                    if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
                    HapticManager.TriggerLight();
                    ShowToast($"Milestone Reward: {rewardText}");
                });
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

        public void ShowToast(string message, float duration = 2.0f)
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
            rt.anchoredPosition = new Vector2(0f, -60f);
            rt.sizeDelta = new Vector2(640f, 90f);

            Image bg = m_ToastObj.AddComponent<Image>();
            bg.sprite = SpriteFactory.GetPillSprite(new Color(0.06f, 0.10f, 0.20f, 0.95f));
            bg.raycastTarget = false;

            GameObject textObj = new GameObject("ToastText");
            textObj.transform.SetParent(m_ToastObj.transform, false);
            RectTransform trt = textObj.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.sizeDelta = Vector2.zero;

            Text txt = textObj.AddComponent<Text>();
            txt.font = m_CandyFont != null ? m_CandyFont : m_DefaultFont;
            txt.fontSize = 28;
            txt.fontStyle = FontStyle.Bold;
            txt.color = new Color(1.0f, 0.88f, 0.25f);
            txt.alignment = TextAnchor.MiddleCenter;
            txt.text = message;
            txt.raycastTarget = false;

            Vector3 baseScale = Vector3.one;
            m_ToastObj.transform.localScale = Vector3.zero;

            float elapsed = 0f;
            float inDur = 0.16f;
            while (elapsed < inDur)
            {
                elapsed += Time.unscaledDeltaTime;
                float p = Mathf.Sin((elapsed / inDur) * Mathf.PI * 0.5f);
                if (m_ToastObj != null) m_ToastObj.transform.localScale = baseScale * p;
                yield return null;
            }
            if (m_ToastObj != null) m_ToastObj.transform.localScale = baseScale;

            yield return new WaitForSecondsRealtime(duration);

            elapsed = 0f;
            float outDur = 0.20f;
            CanvasGroup cg = (m_ToastObj != null) ? m_ToastObj.AddComponent<CanvasGroup>() : null;
            while (elapsed < outDur && m_ToastObj != null)
            {
                elapsed += Time.unscaledDeltaTime;
                float p = elapsed / outDur;
                if (cg != null) cg.alpha = 1f - p;
                if (m_ToastObj != null) m_ToastObj.transform.localScale = baseScale * (1f - p * 0.2f);
                yield return null;
            }

            if (m_ToastObj != null)
            {
                Destroy(m_ToastObj);
                m_ToastObj = null;
            }
            m_ToastCoroutine = null;
        }

        private void ScrollToActiveLevel(GameModeType mode)
        {
            // Fully visible 1-screen map matching Concept 2 Art perfectly
        }

        private void BuildVictoryModal()
        {
            m_VictoryModal = new GameObject("VictoryModal", typeof(RectTransform));
            m_VictoryModal.transform.SetParent(m_RootCanvas, false);
            RectTransform rt = m_VictoryModal.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.sizeDelta = Vector2.zero;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            // Fullscreen backdrop matching background deep space navy
            Image bg = m_VictoryModal.AddComponent<Image>();
            bg.color = new Color(0.04f, 0.07f, 0.16f, 0.98f);
            bg.raycastTarget = true;
            Button bgBtn = m_VictoryModal.AddComponent<Button>();
            bgBtn.transition = Selectable.Transition.None;
            bgBtn.onClick.AddListener(() =>
            {
                if (m_VictoryModal != null) m_VictoryModal.SetActive(false);
            });

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

            // 2. Candy 3D Ruby Red Close (X) Button (placed directly over painted red X at X: 298, Y: 414)
            GameObject closeObj = new GameObject("VictoryCloseBtn", typeof(RectTransform));
            closeObj.transform.SetParent(sceneObj.transform, false);
            RectTransform closeRt = closeObj.GetComponent<RectTransform>();
            closeRt.anchoredPosition = new Vector2(298f, 414f);
            closeRt.sizeDelta = new Vector2(96f, 96f);

            Image closeImg = closeObj.AddComponent<Image>();
            closeImg.color = Color.clear;
            closeImg.raycastTarget = true;

            Button closeBtn = closeObj.AddComponent<Button>();
            closeBtn.onClick.AddListener(() =>
            {
                StartCoroutine(AnimateButtonPress(closeRt, () =>
                {
                    if (m_VictoryModal != null) m_VictoryModal.SetActive(false);
                }));
            });
            closeObj.transform.SetAsLastSibling();

            // 3. Subtitle Text Pill ("Earned 3 Stars! Level X Unlocked")
            GameObject subPillObj = new GameObject("SubtitlePill", typeof(RectTransform));
            subPillObj.transform.SetParent(sceneObj.transform, false);
            RectTransform spRt = subPillObj.GetComponent<RectTransform>();
            spRt.anchoredPosition = new Vector2(0, -32);
            spRt.sizeDelta = new Vector2(580, 52);
            Image spImg = subPillObj.AddComponent<Image>();
            spImg.sprite = SpriteFactory.GetPanelSprite(new Color(0.05f, 0.14f, 0.35f, 0.98f), new Color(0.35f, 0.65f, 1f, 0.90f), 2f, 16);
            spImg.type = Image.Type.Sliced;
            spImg.raycastTarget = false;

            GameObject subObj = CreateText(subPillObj.transform, Vector2.zero, new Vector2(560, 48), "", 26, FontStyle.Normal, new Color(1.0f, 0.95f, 0.82f), TextAnchor.MiddleCenter, m_CandyFont);
            m_VictorySubtitleText = subObj.GetComponent<Text>();
            MakeTextCrisp(m_VictorySubtitleText, new Color(0.02f, 0.06f, 0.20f, 0.95f), 1.0f);

            // 4. Ball Skin Reward Showcase Card (Appears when an Adventure level awards a new ball design!)
            m_VictorySkinRewardObj = new GameObject("SkinRewardCard", typeof(RectTransform));
            m_VictorySkinRewardObj.transform.SetParent(sceneObj.transform, false);
            RectTransform rwRt = m_VictorySkinRewardObj.GetComponent<RectTransform>();
            rwRt.anchoredPosition = new Vector2(0, 45);
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

            // 5. 3D Arcade Buttons Overlays (placed directly on top of the 3D buttons in the artwork)
            // Button 1: NEXT LEVEL (Green button at Y: -108)
            m_VictoryNextBtnObj = CreateTransparentButton(sceneObj.transform, new Vector2(0, -108), new Vector2(420, 84), () =>
            {
                m_VictoryModal.SetActive(false);
                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.NextLevel();
                }
                UpdateHUDVisibility();
            }, "Victory_Btn_NextLevel");

            // Button 2: LEVEL SELECT (Blue button at Y: -205)
            CreateTransparentButton(sceneObj.transform, new Vector2(0, -205), new Vector2(420, 84), () =>
            {
                m_VictoryModal.SetActive(false);
                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.StopLevel();
                    OpenLevelSelect(LevelManager.Instance.CurrentMode);
                }
            }, "Victory_Btn_LevelSelect");

            // Button 3: BALL DESIGNS (Purple button at Y: -294)
            CreateTransparentButton(sceneObj.transform, new Vector2(0, -294), new Vector2(420, 80), () =>
            {
                m_VictoryModal.SetActive(false);
                OpenSkinWardrobe();
            }, "Victory_Btn_BallDesigns");

            // Button 4: HOME (Orange button at Y: -388)
            CreateTransparentButton(sceneObj.transform, new Vector2(0, -388), new Vector2(420, 82), () =>
            {
                m_VictoryModal.SetActive(false);
                if (LevelManager.Instance != null) LevelManager.Instance.StopLevel();
                OnHomeClicked();
            }, "Victory_Btn_Home");

            m_VictoryModal.SetActive(false);
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
                    // Green 3D "EQUIPPED ✓" Button
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
            m_GameOverPanel = new GameObject("GameOverPanel");
            m_GameOverPanel.transform.SetParent(m_RootCanvas, false);
            RectTransform panelRt = m_GameOverPanel.AddComponent<RectTransform>();
            panelRt.anchorMin = Vector2.zero; panelRt.anchorMax = Vector2.one; panelRt.sizeDelta = Vector2.zero;

            Image panelImg = m_GameOverPanel.AddComponent<Image>();
            panelImg.color = new Color(0.04f, 0.06f, 0.12f, 0.95f);

            GameObject winObj = new GameObject("GameOverDialog");
            winObj.transform.SetParent(m_GameOverPanel.transform, false);
            RectTransform winRt = winObj.AddComponent<RectTransform>();
            winRt.sizeDelta = new Vector2(680, 680);
            Image winBg = winObj.AddComponent<Image>();
            winBg.sprite = SpriteFactory.GetPanelSprite(new Color(0.08f, 0.14f, 0.28f, 0.98f), new Color(0.65f, 0.25f, 0.25f, 1f), 2.5f, 20);
            winBg.type = Image.Type.Sliced;

            GameObject titleObj = CreateText(winObj.transform, new Vector2(0, 255), new Vector2(600, 70), "GAME OVER", 58, FontStyle.Bold, new Color(0.96f, 0.35f, 0.35f), TextAnchor.MiddleCenter);

            // New Best Score banner
            m_GameOverNewBestObj = CreateText(winObj.transform, new Vector2(0, 200), new Vector2(500, 36), "NEW RECORD!", 24, FontStyle.Bold, new Color(1.0f, 0.84f, 0.22f), TextAnchor.MiddleCenter);
            m_GameOverNewBestObj.SetActive(false);

            // Final Score
            GameObject scoreLabel = CreateText(winObj.transform, new Vector2(0, 145), new Vector2(400, 32), "FINAL SCORE", 20, FontStyle.Bold, new Color(0.70f, 0.82f, 1f), TextAnchor.MiddleCenter);
            GameObject scoreVal = CreateText(winObj.transform, new Vector2(0, 90), new Vector2(500, 75), "0", 64, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            m_GameOverScoreText = scoreVal.GetComponent<Text>();

            // Best Score
            GameObject bestVal = CreateText(winObj.transform, new Vector2(0, 25), new Vector2(500, 40), "BEST: 0", 26, FontStyle.Bold, new Color(1.0f, 0.82f, 0.15f), TextAnchor.MiddleCenter);
            m_GameOverBestText = bestVal.GetComponent<Text>();

            // Sparkling Gold Revive Button (Free with limited chances)
            m_GameOverReviveBtn = CreateButton(winObj.transform, new Vector2(0, -45), new Vector2(450, 72), new Color(0.92f, 0.65f, 0.12f), "REVIVE (3 CHANCES LEFT)", 28);
            m_GameOverReviveText = m_GameOverReviveBtn.GetComponentInChildren<Text>();
            m_GameOverReviveBtn.GetComponent<Button>().onClick.AddListener(() =>
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
            });

            // Big Emerald Green "PLAY AGAIN" Button
            m_GameOverRestartBtn = CreateButton(winObj.transform, new Vector2(0, -128), new Vector2(450, 68), new Color(0.12f, 0.72f, 0.35f), "PLAY AGAIN", 30);
            m_GameOverRestartBtn.GetComponent<Button>().onClick.AddListener(() =>
            {
                m_GameOverPanel.SetActive(false);
                if (GameManager.Instance != null) GameManager.Instance.RestartGame();
            });

            // Home Button
            m_GameOverHomeBtn = CreateIconButtonWithText(winObj.transform, new Vector2(0, -206), new Vector2(450, 62), new Color(0.18f, 0.42f, 0.82f), GemIconFactory.GetHomeSprite(), "HOME", 26);
            m_GameOverHomeBtn.GetComponent<Button>().onClick.AddListener(() =>
            {
                m_GameOverPanel.SetActive(false);
                OpenModeSelect();
            });

            m_GameOverPanel.SetActive(false);
        }

        // ==========================================
        // 🛠️ HELPER CREATORS
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
            if (txt.font != null && txt.font.name.IndexOf("Lilita", System.StringComparison.OrdinalIgnoreCase) >= 0 && !string.IsNullOrEmpty(txt.text))
            {
                if (txt.text.Contains("<b>") || txt.text.Contains("</b>"))
                {
                    txt.text = txt.text.Replace("<b>", "").Replace("</b>", "");
                }
                txt.fontStyle = FontStyle.Normal;
            }

            // Remove existing duplicate shadow components to avoid muddy stacking
            Shadow existingShadow = txt.gameObject.GetComponent<Shadow>();
            if (existingShadow != null && !(existingShadow is Outline))
            {
                DestroyImmediate(existingShadow);
            }

            // A tight, clean 1.0px outline gives razor-sharp definition without choking glyph holes
            Outline outline = txt.gameObject.GetComponent<Outline>();
            if (outline == null) outline = txt.gameObject.AddComponent<Outline>();
            outline.effectColor = outlineOrShadowColor;
            float d = Mathf.Clamp(distance, 0.75f, 1.0f);
            outline.effectDistance = new Vector2(d, -d);
            outline.useGraphicAlpha = true;
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
        }
    }
}
