# Box-blast-2D Fast Memory & Direct Action Rules

- User Language: Hindi / Hinglish.
- Action: Direct execution on user commands without redundant search steps.
- Architecture:
  - UI, Screens, Buttons: `Assets/Scripts/UI/UIManager.cs`
  - Game State, Score, Game Over: `Assets/Scripts/Core/GameManager.cs`
  - Grid 8x8, Placements, Lines Clear: `Assets/Scripts/Grid/GridManager.cs`
  - Dock Shapes, Spawning, Availability: `Assets/Scripts/Shapes/ShapeSpawner.cs`
  - Levels, Adventure Mode: `Assets/Scripts/Modes/LevelManager.cs`
  - Sprites, Icons, Themes: `Assets/Scripts/Visuals/SpriteFactory.cs` & `GemIconFactory.cs`
- Verification: Always verify compilation in Unity Editor log after code edits.
