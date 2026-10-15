# Box-blast-2D: Permanent Project Map & Guidelines

Yeh file AI assistant ke permanent memory ke roop me kaam karti hai. Iski wajah se AI ko kisi bhi kaam ke liye files search nahi karni padti aur direct command par instant kaam hota hai.

---

## 🗺️ Project Architecture Map (Kaunsi Cheez Kahan Hai)

### 1. User Interface & Menus (`Assets/Scripts/UI/`)
* **`UIManager.cs` (Master UI Controller)**:
  - **Home Screen / Mode Select**: `BuildModeSelectScreen()` (L:2257+). Uses 1152x2048 AspectRatioFitter with Concept 3 Master 3D artwork, Adventure button, Classic button.
  - **Game Over Screen**: `BuildClassicGameOverModal()` (L:3870+) & `ShowGameOver()` (L:1820+). Displays Score, Best Score, Revive button, Home button, Play Again button.
  - **Adventure Map**: `BuildAdventureLevelMapModal()` - Galaxy Road with levels 1-100+, star progress, scrolling.
  - **HUDs**: `BuildClassicHUD()` (Score, Best, Boosters dock) and `BuildAdventureHUD()` (Moves left, Target gems).
  - **Text Crispness Helper**: `MakeTextCrisp()` with downward 3D drop shadow and `dynamicPixelsPerUnit = 4.0f`.
* **`FloatingText.cs`**: In-game floating popup scores (+10, +100, COMBO x2, SHUFFLED!).

### 2. Gameplay Core & State (`Assets/Scripts/Core/`)
* **`GameManager.cs` (State & Game Loop)**:
  - `TriggerGameOver()`: Hits when board is jammed. Plays sound, triggers haptics, calls `UIManager.ShowGameOver()`.
  - `ReviveGame()`: Clears blocks from center/board and gives up to 3 revive chances.
  - Score & Coins: `AddScore()`, `AddCoins()`, `BestScore` management via PlayerPrefs.
  - Boosters: Cannon (clears row/col), Bomb (clears 3x3), Arrow (clears single block), Shuffle (refreshes 3 pieces).
* **`AchievementManager.cs`**: Achievements, quest badges, milestone rewards.
* **`HapticManager.cs`**: Mobile vibration feedback (Light, Medium, Heavy, Success, Warning).

### 3. Board & Grid System (`Assets/Scripts/Grid/`)
* **`GridManager.cs`**:
  - 8x8 Grid management (`m_Cells[8,8]`).
  - Placement validation: `CanShapeFit(shape, originX, originY)`.
  - Availability check: `CanAnyShapeFit(availableShapes)`.
  - Line clears: `CheckAndClearLines()` - detects complete rows, columns, combos, and triggers line blast effects.
  - Screen Shake: `ShakeBoard()` on combos and multi-clears.
* **`GridCell.cs`**:
  - Individual grid cell rendering, occupied state, preview highlights, and line clear glowing.

### 4. Shapes & Dock (`Assets/Scripts/Shapes/`)
* **`ShapeSpawner.cs`**:
  - Bottom Dock: 3 slots for draggable shapes (`m_SlotShapes[3]`).
  - `SpawnBatch()`: Spawns batch of 3 pieces.
  - `CheckMoveAvailability()`: Runs after every piece placement. In Classic Mode, if `!CanAnyShapeFit()`, immediately triggers `GameManager.Instance.TriggerGameOver()`.
  - `ShuffleShapes()`: Shuffles/refreshes the current 3 pieces.
* **`DraggableShape.cs`**:
  - Touch/Mouse drag & drop logic, lift offset, snap preview on grid, release to place.
* **`ShapeData.cs` & `ShapeCatalog.cs`**:
  - Matrix data for all shapes (Dots, 2x2, 3x3, I-lines, L-shapes, T-shapes, Z-shapes).

### 5. Adventure & Level Modes (`Assets/Scripts/Modes/`)
* **`LevelManager.cs`**:
  - Tracks `CurrentLevel`, `MovesRemaining`, and `TargetGems`.
  - Win condition: All target gems collected before moves run out.
  - Loss condition: Moves reach 0 or board full.
* **`GameMode.cs` & `GemType.cs`**:
  - Mode definitions (Classic vs Adventure) and Gem Types (Ruby, Sapphire, Emerald, Topaz, Amethyst, Diamond).

### 6. Visuals & Audio (`Assets/Scripts/Visuals/` & `Audio/`)
* **`SpriteFactory.cs` & `GemIconFactory.cs`**:
  - Generates pixel-perfect procedural sprites, 3D spheres, buttons, crowns, and booster icons.
* **`ParticleFXManager.cs` & `LineBlastAnimator.cs`**:
  - Particle bursts, shockwaves, star showers, and floating combo banners.
* **`SoundManager.cs`**:
  - SFX for block place, line blast, combo arpeggio, game over, button clicks.

---

## ⚡ Direct Execution Rules for AI (User Preferences)

1. **Direct Action First**: Jab user koi change bole, bina lambe explanation ya redundant searches ke direct target file me change execute karo.
2. **Language**: Hamesha clear, polite Hindi/Hinglish me baat karo.
3. **Zero Errors**: Har script edit ke baad Unity compile logs verify karo ki 0 compilation errors hain.
4. **Preserve Code Integrity**: Purane comments, structure aur unrelated functions ko kabhi disturb mat karo.
5. **Clickable Links**: Har file aur function ka clickable markdown link (`file:///...`) provide karo.
