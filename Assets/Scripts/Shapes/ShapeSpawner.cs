using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoxBlast
{
    /// <summary>
    /// Controls the 3 square puzzle slot boxes (vertical in Landscape, horizontal in Portrait).
    /// Each puzzle piece sits inside its own square box. When a puzzle is used, that box
    /// becomes empty. When all 3 puzzles are used, a brand new batch of 3 puzzles spawns.
    /// </summary>
    public class ShapeSpawner : MonoBehaviour
    {
        public static ShapeSpawner Instance { get; private set; }

        private Vector3[] m_LandscapePositions = new Vector3[]
        {
            new Vector3(4.05f,  2.25f, 0f),
            new Vector3(4.05f,  0.0f,  0f),
            new Vector3(4.05f, -2.25f, 0f)
        };

        private Vector3[] m_PortraitPositionsClassic = new Vector3[]
        {
            new Vector3(-2.25f, -4.30f, 0f),
            new Vector3( 0.00f, -4.30f, 0f),
            new Vector3( 2.25f, -4.30f, 0f)
        };

        private Vector3[] m_PortraitPositionsAdventure = new Vector3[]
        {
            new Vector3(-2.25f, -4.65f, 0f),
            new Vector3( 0.00f, -4.65f, 0f),
            new Vector3( 2.25f, -4.65f, 0f)
        };

        private Vector3[] GetPortraitPositions()
        {
            bool isLevel = (LevelManager.Instance != null && LevelManager.Instance.IsLevelActive);
            return isLevel ? m_PortraitPositionsAdventure : m_PortraitPositionsClassic;
        }

        private float GetBowlScale()
        {
            return 1.0f;
        }

        private Vector3[] m_CurrentSlotPositions;
        private readonly GameObject[] m_SlotBoxObjects = new GameObject[3];
        private readonly DraggableShape[] m_SlotShapes = new DraggableShape[3];
        private bool m_IsLandscape = true;

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

            m_IsLandscape = Screen.width > Screen.height;
            m_CurrentSlotPositions = m_IsLandscape ? m_LandscapePositions : GetPortraitPositions();

            CreateSlotBoxes();
        }

        private void OnEnable()
        {
            SkinManager.OnSkinChanged += HandleSkinChanged;
        }

        private void OnDisable()
        {
            SkinManager.OnSkinChanged -= HandleSkinChanged;
        }

        private void HandleSkinChanged(BallSkinType newSkin)
        {
            RefreshAllSlotShapesSprites();
        }

        public void RefreshAllSlotShapesSprites()
        {
            for (int i = 0; i < m_SlotShapes.Length; i++)
            {
                if (m_SlotShapes[i] != null)
                {
                    m_SlotShapes[i].RefreshSprites();
                }
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private GameObject m_DockBackdropObj;

        private void CreateSlotBoxes()
        {
            m_CurrentSlotPositions = m_IsLandscape ? m_LandscapePositions : GetPortraitPositions();
            float bowlScale = GetBowlScale();

            // Sleek dark glass dock backdrop to frame the 3 royal pedestals and soften background clutter
            if (m_DockBackdropObj == null)
            {
                m_DockBackdropObj = new GameObject("PedestalDockBackdrop");
                m_DockBackdropObj.transform.SetParent(transform, false);
                SpriteRenderer sr = m_DockBackdropObj.AddComponent<SpriteRenderer>();
                sr.sprite = SpriteFactory.GetPillSprite(new Color(0.02f, 0.05f, 0.14f, 0.82f));
                sr.sortingOrder = 1; // Behind pedestals (sortingOrder 2)
            }

            if (!m_IsLandscape)
            {
                m_DockBackdropObj.SetActive(true);
                m_DockBackdropObj.transform.position = new Vector3(0f, m_CurrentSlotPositions[1].y - 0.15f, 0f);
                m_DockBackdropObj.transform.localScale = new Vector3(7.5f, 2.5f, 1f);
            }
            else
            {
                m_DockBackdropObj.SetActive(false);
            }

            for (int i = 0; i < 3; i++)
            {
                if (m_SlotBoxObjects[i] == null)
                {
                    GameObject boxObj = new GameObject($"SlotBox_{i}");
                    boxObj.transform.SetParent(transform, false);

                    SpriteRenderer sr = boxObj.AddComponent<SpriteRenderer>();
                    sr.sprite = SpriteFactory.GetConcept1PedestalSprite();
                    sr.drawMode = SpriteDrawMode.Simple;
                    boxObj.transform.localScale = new Vector3(bowlScale, bowlScale, 1f);
                    sr.sortingOrder = 2; // Behind shape blocks (sortingOrder 10)

                    m_SlotBoxObjects[i] = boxObj;
                }
                else
                {
                    m_SlotBoxObjects[i].transform.localScale = new Vector3(bowlScale, bowlScale, 1f);
                }

                // 3D Royal Box is symmetric and centered directly under the shape slot
                Vector3 pedPos = m_CurrentSlotPositions[i];
                m_SlotBoxObjects[i].transform.position = pedPos;
            }
        }

        public void UpdateLayout(bool isLandscape)
        {
            m_IsLandscape = false;
            m_CurrentSlotPositions = GetPortraitPositions();
            float bowlScale = GetBowlScale();

            CreateSlotBoxes();

            for (int i = 0; i < 3; i++)
            {
                if (m_SlotBoxObjects[i] != null)
                {
                    m_SlotBoxObjects[i].transform.localScale = new Vector3(bowlScale, bowlScale, 1f);
                    Vector3 pedPos = m_CurrentSlotPositions[i];
                    m_SlotBoxObjects[i].transform.position = pedPos;
                }

                if (m_SlotShapes[i] != null)
                {
                    m_SlotShapes[i].UpdateSlotPosition(m_CurrentSlotPositions[i]);
                }
            }
        }

        public void SpawnBatch()
        {
            ClearAll();

            if (m_CurrentSlotPositions == null)
            {
                m_CurrentSlotPositions = m_IsLandscape ? m_LandscapePositions : GetPortraitPositions();
            }

            CreateSlotBoxes();

            List<ShapeData> allShapes = ShapeCatalog.GetAllShapes();
            List<ShapeData> batchShapes = GeneratePlayableBatch(allShapes);

            for (int i = 0; i < 3; i++)
            {
                ShapeData chosenData = (i < batchShapes.Count) ? batchShapes[i] : allShapes[Random.Range(0, allShapes.Count)];
                SpawnShapeAtSlot(chosenData, m_CurrentSlotPositions[i], i);
            }

            CheckMoveAvailability();
        }

        private List<ShapeData> GeneratePlayableBatch(List<ShapeData> allShapes)
        {
            List<ShapeData> batch = new List<ShapeData>();
            if (GridManager.Instance == null)
            {
                for (int i = 0; i < 3; i++) batch.Add(allShapes[Random.Range(0, allShapes.Count)]);
                return batch;
            }

            int occupied = GridManager.Instance.GetOccupiedCount();
            bool[,] baseGrid = GridManager.Instance.GetOccupancyGrid();

            // Try simulated sequential batch generation (ensuring all 3 shapes can fit sequentially)
            for (int attempt = 0; attempt < 8; attempt++)
            {
                bool[,] simGrid = (bool[,])baseGrid.Clone();
                List<ShapeData> candidateBatch = new List<ShapeData>();
                bool success = true;

                for (int slot = 0; slot < 3; slot++)
                {
                    List<ShapeData> fitting = GetFittingShapesForGrid(simGrid, allShapes);
                    if (fitting.Count == 0)
                    {
                        success = false;
                        break;
                    }

                    ShapeData chosen = SelectWeightedShape(fitting, occupied, slot);
                    candidateBatch.Add(chosen);

                    if (!SimulatePlacementAndClear(simGrid, chosen))
                    {
                        success = false;
                        break;
                    }
                }

                if (success && candidateBatch.Count == 3)
                {
                    return candidateBatch;
                }
            }

            // Fallback: Pick directly from shapes that fit on current board, favoring compact shapes
            List<ShapeData> currentFitting = GetFittingShapes(allShapes);
            if (currentFitting.Count > 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    batch.Add(SelectWeightedShape(currentFitting, occupied, i));
                }
                return batch;
            }

            for (int i = 0; i < 3; i++)
            {
                batch.Add(allShapes[Random.Range(0, allShapes.Count)]);
            }
            return batch;
        }

        private ShapeData SelectWeightedShape(List<ShapeData> candidates, int occupiedCells, int slotIndex)
        {
            if (candidates == null || candidates.Count == 0) return null;

            // Extreme crowdedness (>= 38 occupied of 64) -> strictly prefer small pieces (<= 2 or 3 blocks)
            if (occupiedCells >= 38)
            {
                List<ShapeData> small = new List<ShapeData>();
                for (int i = 0; i < candidates.Count; i++)
                {
                    if (candidates[i].Blocks.Length <= 3) small.Add(candidates[i]);
                }
                if (small.Count > 0) return small[Random.Range(0, small.Count)];
            }

            // Moderately crowded (>= 22 occupied) -> favor manageable shapes (<= 4 blocks)
            if (occupiedCells >= 22)
            {
                List<ShapeData> manageable = new List<ShapeData>();
                for (int i = 0; i < candidates.Count; i++)
                {
                    if (candidates[i].Blocks.Length <= 4) manageable.Add(candidates[i]);
                }
                if (manageable.Count > 0) return manageable[Random.Range(0, manageable.Count)];
            }

            return candidates[Random.Range(0, candidates.Count)];
        }

        private bool SimulatePlacementAndClear(bool[,] grid, ShapeData shape)
        {
            int w = GridManager.BoardWidth;
            int h = GridManager.BoardHeight;

            List<Vector2Int> validPositions = new List<Vector2Int>();
            for (int x = 0; x <= w - shape.Width; x++)
            {
                for (int y = 0; y <= h - shape.Height; y++)
                {
                    if (SimCanFit(grid, shape, x, y))
                    {
                        validPositions.Add(new Vector2Int(x, y));
                    }
                }
            }

            if (validPositions.Count == 0) return false;

            // Prioritize positions that clear lines
            Vector2Int bestPos = validPositions[0];
            int maxClears = -1;

            int checkLimit = Mathf.Min(validPositions.Count, 6);
            for (int i = 0; i < checkLimit; i++)
            {
                Vector2Int pos = validPositions[i];
                int clears = CountSimLineClears(grid, shape, pos.x, pos.y);
                if (clears > maxClears)
                {
                    maxClears = clears;
                    bestPos = pos;
                }
            }

            // Place shape on sim grid
            for (int b = 0; b < shape.Blocks.Length; b++)
            {
                grid[bestPos.x + shape.Blocks[b].x, bestPos.y + shape.Blocks[b].y] = true;
            }

            // Simulate clearing lines
            List<int> fullRows = new List<int>();
            List<int> fullCols = new List<int>();

            for (int y = 0; y < h; y++)
            {
                bool rowFull = true;
                for (int x = 0; x < w; x++)
                {
                    if (!grid[x, y]) { rowFull = false; break; }
                }
                if (rowFull) fullRows.Add(y);
            }

            for (int x = 0; x < w; x++)
            {
                bool colFull = true;
                for (int y = 0; y < h; y++)
                {
                    if (!grid[x, y]) { colFull = false; break; }
                }
                if (colFull) fullCols.Add(x);
            }

            foreach (int y in fullRows)
            {
                for (int x = 0; x < w; x++) grid[x, y] = false;
            }

            foreach (int x in fullCols)
            {
                for (int y = 0; y < h; y++) grid[x, y] = false;
            }

            return true;
        }

        private bool SimCanFit(bool[,] grid, ShapeData shape, int originX, int originY)
        {
            for (int i = 0; i < shape.Blocks.Length; i++)
            {
                int gx = originX + shape.Blocks[i].x;
                int gy = originY + shape.Blocks[i].y;
                if (gx < 0 || gx >= GridManager.BoardWidth || gy < 0 || gy >= GridManager.BoardHeight) return false;
                if (grid[gx, gy]) return false;
            }
            return true;
        }

        private int CountSimLineClears(bool[,] grid, ShapeData shape, int originX, int originY)
        {
            int w = GridManager.BoardWidth;
            int h = GridManager.BoardHeight;
            int clears = 0;

            for (int y = 0; y < h; y++)
            {
                bool full = true;
                for (int x = 0; x < w; x++)
                {
                    bool isCovered = grid[x, y];
                    if (!isCovered)
                    {
                        for (int b = 0; b < shape.Blocks.Length; b++)
                        {
                            if (originX + shape.Blocks[b].x == x && originY + shape.Blocks[b].y == y)
                            {
                                isCovered = true;
                                break;
                            }
                        }
                    }
                    if (!isCovered) { full = false; break; }
                }
                if (full) clears++;
            }

            for (int x = 0; x < w; x++)
            {
                bool full = true;
                for (int y = 0; y < h; y++)
                {
                    bool isCovered = grid[x, y];
                    if (!isCovered)
                    {
                        for (int b = 0; b < shape.Blocks.Length; b++)
                        {
                            if (originX + shape.Blocks[b].x == x && originY + shape.Blocks[b].y == y)
                            {
                                isCovered = true;
                                break;
                            }
                        }
                    }
                    if (!isCovered) { full = false; break; }
                }
                if (full) clears++;
            }

            return clears;
        }

        private List<ShapeData> GetFittingShapesForGrid(bool[,] grid, List<ShapeData> allShapes)
        {
            List<ShapeData> fitting = new List<ShapeData>();
            int w = GridManager.BoardWidth;
            int h = GridManager.BoardHeight;

            for (int i = 0; i < allShapes.Count; i++)
            {
                ShapeData s = allShapes[i];
                if (s == null) continue;

                bool canFit = false;
                for (int x = 0; x <= w - s.Width; x++)
                {
                    for (int y = 0; y <= h - s.Height; y++)
                    {
                        if (SimCanFit(grid, s, x, y))
                        {
                            canFit = true;
                            break;
                        }
                    }
                    if (canFit) break;
                }
                if (canFit) fitting.Add(s);
            }
            return fitting;
        }

        public bool CanShapeFitAnywhere(ShapeData shape)
        {
            if (GridManager.Instance == null || shape == null) return false;
            for (int x = 0; x <= GridManager.BoardWidth - shape.Width; x++)
            {
                for (int y = 0; y <= GridManager.BoardHeight - shape.Height; y++)
                {
                    if (GridManager.Instance.CanShapeFit(shape, x, y))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private List<ShapeData> GetFittingShapes(List<ShapeData> allShapes)
        {
            List<ShapeData> fitting = new List<ShapeData>();
            if (GridManager.Instance == null) return allShapes;

            for (int i = 0; i < allShapes.Count; i++)
            {
                ShapeData s = allShapes[i];
                if (s != null && CanShapeFitAnywhere(s))
                {
                    fitting.Add(s);
                }
            }
            return fitting;
        }

        private ShapeData PickBestRescueShape(List<ShapeData> fittingCatalog)
        {
            if (fittingCatalog == null || fittingCatalog.Count == 0) return ShapeCatalog.GetAllShapes()[0];

            List<ShapeData> ultraSmall = new List<ShapeData>();
            for (int i = 0; i < fittingCatalog.Count; i++)
            {
                if (fittingCatalog[i].Blocks.Length <= 2) ultraSmall.Add(fittingCatalog[i]);
            }
            if (ultraSmall.Count > 0) return ultraSmall[Random.Range(0, ultraSmall.Count)];

            List<ShapeData> small = new List<ShapeData>();
            for (int i = 0; i < fittingCatalog.Count; i++)
            {
                if (fittingCatalog[i].Blocks.Length <= 3) small.Add(fittingCatalog[i]);
            }
            if (small.Count > 0) return small[Random.Range(0, small.Count)];

            return fittingCatalog[Random.Range(0, fittingCatalog.Count)];
        }

        public void ReplaceSlotShape(int slotIndex, ShapeData newData)
        {
            if (slotIndex < 0 || slotIndex >= 3 || newData == null) return;

            List<GemType> existingGems = null;
            if (m_SlotShapes[slotIndex] != null)
            {
                existingGems = m_SlotShapes[slotIndex].GetBlockGems();
                Destroy(m_SlotShapes[slotIndex].gameObject);
                m_SlotShapes[slotIndex] = null;
            }

            Vector3 slotPos = (m_CurrentSlotPositions != null && slotIndex < m_CurrentSlotPositions.Length)
                ? m_CurrentSlotPositions[slotIndex]
                : Vector3.zero;

            SpawnShapeAtSlot(newData, slotPos, slotIndex, existingGems);
        }

        public void ShuffleShapes()
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlayShuffle();
            SpawnBatch();
            FloatingText.Create(new Vector3(m_IsLandscape ? 3.55f : 0f, 0f, 0f), "SHUFFLED!", new Color(1.0f, 0.45f, 0.85f), 6f);
        }

        private void SpawnShapeAtSlot(ShapeData data, Vector3 slotPos, int slotIndex, List<GemType> overrideGems = null)
        {
            GameObject shapeObj = new GameObject($"Shape_Slot{slotIndex}_{data.ShapeName}");
            shapeObj.transform.SetParent(transform, false);

            DraggableShape draggable = shapeObj.AddComponent<DraggableShape>();
            float cellSize = (GridManager.Instance != null) ? GridManager.Instance.TotalCellStep : 0.72f;

            List<GemType> gems = overrideGems;
            if (gems == null && LevelManager.Instance != null && LevelManager.Instance.IsLevelActive && LevelManager.Instance.CurrentLevel != null && LevelManager.Instance.CurrentLevel.TargetGems != null && LevelManager.Instance.CurrentLevel.TargetGems.Count > 0)
            {
                // Find all gems that are still missing towards completing the level objective!
                List<GemType> missingGems = new List<GemType>();
                foreach (var kvp in LevelManager.Instance.CurrentLevel.TargetGems)
                {
                    int have = (LevelManager.Instance.CollectedGems != null && LevelManager.Instance.CollectedGems.ContainsKey(kvp.Key))
                        ? LevelManager.Instance.CollectedGems[kvp.Key] : 0;
                    int missing = kvp.Value - have;
                    // Add weighted by how many are still missing
                    for (int m = 0; m < missing; m++)
                    {
                        missingGems.Add(kvp.Key);
                    }
                }

                // Check how many dock shapes currently have a missing gem
                int activeShapesWithMissingGem = 0;
                for (int s = 0; s < 3; s++)
                {
                    if (s != slotIndex && m_SlotShapes[s] != null)
                    {
                        List<GemType> slotGems = m_SlotShapes[s].GetBlockGems();
                        if (slotGems != null)
                        {
                            for (int g = 0; g < slotGems.Count; g++)
                            {
                                if (missingGems.Contains(slotGems[g]))
                                {
                                    activeShapesWithMissingGem++;
                                    break;
                                }
                            }
                        }
                    }
                }

                // If player still needs gems, guarantee or heavily favor spawning missing gems!
                bool shouldSpawnGem = false;
                if (missingGems.Count > 0)
                {
                    // If no other dock slot currently has a needed gem, guarantee this shape gets one!
                    // Otherwise, provide a generous 75% spawn chance
                    shouldSpawnGem = (activeShapesWithMissingGem == 0) || (UnityEngine.Random.value < 0.75f);
                }
                else
                {
                    // All objectives already collected: small decorative chance
                    shouldSpawnGem = UnityEngine.Random.value < 0.25f;
                }

                if (shouldSpawnGem && data.Blocks.Length > 0)
                {
                    gems = new List<GemType>();
                    for (int b = 0; b < data.Blocks.Length; b++) gems.Add(GemType.None);

                    GemType chosenGem = GemType.None;
                    if (missingGems.Count > 0)
                    {
                        chosenGem = missingGems[UnityEngine.Random.Range(0, missingGems.Count)];
                    }
                    else
                    {
                        List<GemType> allTargetGems = new List<GemType>(LevelManager.Instance.CurrentLevel.TargetGems.Keys);
                        if (allTargetGems.Count > 0)
                        {
                            chosenGem = allTargetGems[UnityEngine.Random.Range(0, allTargetGems.Count)];
                        }
                    }

                    if (chosenGem != GemType.None)
                    {
                        int gemBlockIdx = UnityEngine.Random.Range(0, data.Blocks.Length);
                        gems[gemBlockIdx] = chosenGem;
                    }
                }
            }
            else if (gems != null)
            {
                if (gems.Count < data.Blocks.Length)
                {
                    while (gems.Count < data.Blocks.Length) gems.Add(GemType.None);
                }
                else if (gems.Count > data.Blocks.Length)
                {
                    GemType preserved = GemType.None;
                    for (int g = 0; g < gems.Count; g++)
                    {
                        if (gems[g] != GemType.None) { preserved = gems[g]; break; }
                    }
                    gems = new List<GemType>();
                    for (int b = 0; b < data.Blocks.Length; b++) gems.Add(GemType.None);
                    if (preserved != GemType.None && data.Blocks.Length > 0)
                    {
                        gems[0] = preserved;
                    }
                }
            }

            draggable.Initialize(data, slotPos, cellSize, slotIndex, gems);

            m_SlotShapes[slotIndex] = draggable;
            StartCoroutine(AnimateSpawn(shapeObj.transform));
        }

        private IEnumerator AnimateSpawn(Transform t)
        {
            Vector3 targetScale = t.localScale;
            t.localScale = Vector3.zero;

            float elapsed = 0f;
            float duration = 0.22f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                float scaleMul = Mathf.Sin(progress * Mathf.PI * 0.5f);
                t.localScale = targetScale * scaleMul;
                yield return null;
            }

            t.localScale = targetScale;
        }

        public void OnShapePlaced(DraggableShape placedShape)
        {
            int slotIdx = placedShape.SlotIndex;
            if (slotIdx >= 0 && slotIdx < 3 && m_SlotShapes[slotIdx] == placedShape)
            {
                m_SlotShapes[slotIdx] = null;
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    if (m_SlotShapes[i] == placedShape)
                    {
                        m_SlotShapes[i] = null;
                        break;
                    }
                }
            }

            // Check how many shapes remain in the 3 square boxes
            int remainingCount = 0;
            for (int i = 0; i < 3; i++)
            {
                if (m_SlotShapes[i] != null) remainingCount++;
            }

            // If all 3 puzzles have been used -> Spawn 3 fresh new puzzles into all boxes!
            if (remainingCount == 0)
            {
                SpawnBatch();
            }
            else
            {
                CheckMoveAvailability();
            }
        }

        public void CheckMoveAvailability()
        {
            if (GridManager.Instance == null) return;

            List<ShapeData> remainingData = new List<ShapeData>();
            for (int i = 0; i < 3; i++)
            {
                if (m_SlotShapes[i] != null && m_SlotShapes[i].Data != null)
                {
                    remainingData.Add(m_SlotShapes[i].Data);
                    bool canFitThis = CanShapeFitAnywhere(m_SlotShapes[i].Data);
                    m_SlotShapes[i].SetDimmed(!canFitThis);
                }
            }

            if (!GridManager.Instance.CanAnyShapeFit(remainingData))
            {
                if (LevelManager.Instance != null && LevelManager.Instance.IsLevelActive)
                {
                    // Guard: Never fail during fresh level setup when player has not made a move yet!
                    if (LevelManager.Instance.CurrentLevel != null && LevelManager.Instance.MovesRemaining >= LevelManager.Instance.CurrentLevel.MovesLimit)
                    {
                        List<ShapeData> all = ShapeCatalog.GetAllShapes();
                        List<ShapeData> fitting = GetFittingShapes(all);
                        for (int i = 0; i < 3; i++)
                        {
                            if (m_SlotShapes[i] != null && m_SlotShapes[i].Data != null)
                            {
                                if (!CanShapeFitAnywhere(m_SlotShapes[i].Data))
                                {
                                    ShapeData rescue = (fitting.Count > 0) ? PickBestRescueShape(fitting) : new ShapeData("Dot", new[] { new Vector2Int(0, 0) }, SpriteFactory.BlockColors[2]);
                                    ReplaceSlotShape(i, rescue);
                                }
                            }
                        }
                        return;
                    }

                    if (LevelManager.Instance.MovesRemaining <= 0)
                    {
                        LevelManager.Instance.FailLevel(DefeatReason.OutOfMoves);
                        return;
                    }

                    List<ShapeData> allShapes = ShapeCatalog.GetAllShapes();
                    List<ShapeData> fittingCatalog = GetFittingShapes(allShapes);

                    if (fittingCatalog.Count > 0)
                    {
                        // Quietly adapt only the unplaceable slot(s) into fitting pieces
                        for (int i = 0; i < 3; i++)
                        {
                            if (m_SlotShapes[i] != null && m_SlotShapes[i].Data != null)
                            {
                                if (!CanShapeFitAnywhere(m_SlotShapes[i].Data))
                                {
                                    ShapeData rescueShape = PickBestRescueShape(fittingCatalog);
                                    ReplaceSlotShape(i, rescueShape);
                                }
                            }
                        }

                        // Re-check dimming after rescue
                        for (int i = 0; i < 3; i++)
                        {
                            if (m_SlotShapes[i] != null && m_SlotShapes[i].Data != null)
                            {
                                bool canFitThis = CanShapeFitAnywhere(m_SlotShapes[i].Data);
                                m_SlotShapes[i].SetDimmed(!canFitThis);
                            }
                        }
                        return;
                    }

                    // Board is 100% full (no shape in catalog can fit anywhere)
                    if (GridManager.Instance.GetOccupiedCount() >= 55)
                    {
                        LevelManager.Instance.FailLevel(DefeatReason.BoardFull);
                    }
                    else
                    {
                        // Fallback: spawn 1x1 dot rescue to prevent premature loss on playable boards
                        ShapeData dotShape = new ShapeData("Dot", new[] { new Vector2Int(0, 0) }, SpriteFactory.BlockColors[2]);
                        for (int i = 0; i < 3; i++)
                        {
                            if (m_SlotShapes[i] != null && m_SlotShapes[i].Data != null && !CanShapeFitAnywhere(m_SlotShapes[i].Data))
                            {
                                ReplaceSlotShape(i, dotShape);
                            }
                        }
                    }
                }
                else if (GameManager.Instance != null)
                {
                    GameManager.Instance.TriggerGameOver();
                }
            }
        }

        public void ClearAll()
        {
            StopAllCoroutines();
            for (int i = 0; i < 3; i++)
            {
                if (m_SlotShapes[i] != null)
                {
                    Destroy(m_SlotShapes[i].gameObject);
                    m_SlotShapes[i] = null;
                }
            }
        }

        public void RefreshActiveShapeSprites()
        {
            for (int i = 0; i < 3; i++)
            {
                if (m_SlotShapes[i] != null)
                {
                    m_SlotShapes[i].RefreshSprites();
                }
            }
        }
    }
}
