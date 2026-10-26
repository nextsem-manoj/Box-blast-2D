using System.Collections;
using UnityEngine;

namespace BoxBlast
{
    /// <summary>
    /// Represents a single slot in the 9x9 Grid.
    /// Manages circular socket, 3D sphere state, squash-and-stretch placement bounce, 
    /// idle breathing pulse, and bubble blast pop animations.
    /// </summary>
    public class GridCell : MonoBehaviour
    {
        public int GridX { get; private set; }
        public int GridY { get; private set; }
        public bool IsOccupied { get; private set; }
        public Color CurrentColor { get; private set; }

        private SpriteRenderer m_BgRenderer;
        private SpriteRenderer m_BlockRenderer;
        private SpriteRenderer m_PreviewRenderer;
        private SpriteRenderer m_LineGlowRenderer;

        public const float BlockScale = 0.88f;
        public const float GemScale = 0.72f;

        private GameObject m_BlockObj;
        private GameObject m_PreviewObj;
        private GameObject m_LineGlowObj;
        private float m_CellSize;

        private Coroutine m_IdleCoroutine;
        private Coroutine m_LineGlowCoroutine;

        public void Initialize(int x, int y, float cellSize)
        {
            GridX = x;
            GridY = y;
            m_CellSize = cellSize;
            IsOccupied = false;

            // Background Circular Socket Slot
            m_BgRenderer = gameObject.AddComponent<SpriteRenderer>();
            m_BgRenderer.sprite = SpriteFactory.GetEmptyCellSprite();
            m_BgRenderer.sortingOrder = 1;
            Sprite frameCheck = Resources.Load<Sprite>("Gameplay_BoardFrame_Concept1");
            if (frameCheck != null)
            {
                m_BgRenderer.enabled = false;
            }
            transform.localScale = Vector3.one * (cellSize * 0.98f);

            // Preview Ghost Sphere
            m_PreviewObj = new GameObject($"Preview_{x}_{y}");
            m_PreviewObj.transform.SetParent(transform, false);
            m_PreviewRenderer = m_PreviewObj.AddComponent<SpriteRenderer>();
            m_PreviewRenderer.sortingOrder = 3;
            m_PreviewObj.transform.localScale = Vector3.one * BlockScale;
            m_PreviewObj.SetActive(false);

            // Line Clear Anticipation Highlight
            m_LineGlowObj = new GameObject($"LineGlow_{x}_{y}");
            m_LineGlowObj.transform.SetParent(transform, false);
            m_LineGlowRenderer = m_LineGlowObj.AddComponent<SpriteRenderer>();
            m_LineGlowRenderer.sprite = SpriteFactory.GetLineHighlightSprite();
            m_LineGlowRenderer.sortingOrder = 5;
            m_LineGlowObj.SetActive(false);

            // Filled 3D Sphere Ball
            m_BlockObj = new GameObject($"Sphere_{x}_{y}");
            m_BlockObj.transform.SetParent(transform, false);
            m_BlockRenderer = m_BlockObj.AddComponent<SpriteRenderer>();
            m_BlockRenderer.sortingOrder = 4;
            m_BlockObj.transform.localScale = Vector3.one * BlockScale;
            m_BlockObj.SetActive(false);

            // Embedded Sparkling Gem centered inside Sphere
            m_GemObj = new GameObject($"Gem_{x}_{y}");
            m_GemObj.transform.SetParent(m_BlockObj.transform, false);
            m_GemRenderer = m_GemObj.AddComponent<SpriteRenderer>();
            m_GemRenderer.sortingOrder = 6; // Above sphere body (4)
            m_GemObj.transform.localScale = Vector3.one * GemScale;
            m_GemObj.transform.localPosition = new Vector3(0f, 0.02f, 0f); // Calibrated visual centroid alignment
            m_GemObj.SetActive(false);
        }

        public GemType EmbeddedGem { get; private set; } = GemType.None;
        private GameObject m_GemObj;
        private SpriteRenderer m_GemRenderer;

        public void SetLineClearHighlight(bool active)
        {
            if (m_LineGlowObj == null) return;

            if (active)
            {
                m_LineGlowObj.SetActive(true);
                if (m_LineGlowCoroutine == null && gameObject.activeInHierarchy)
                {
                    m_LineGlowCoroutine = StartCoroutine(AnimateLineGlowPulse());
                }
            }
            else
            {
                if (m_LineGlowCoroutine != null)
                {
                    StopCoroutine(m_LineGlowCoroutine);
                    m_LineGlowCoroutine = null;
                }
                m_LineGlowObj.SetActive(false);
                m_LineGlowObj.transform.localScale = Vector3.one;

                if (IsOccupied && m_BlockObj != null)
                {
                    m_BlockObj.transform.localScale = Vector3.one * BlockScale;
                }
            }
        }

        private IEnumerator AnimateLineGlowPulse()
        {
            float elapsed = 0f;
            while (true)
            {
                elapsed += Time.unscaledDeltaTime * 6.5f;
                float pulse = 1.0f + Mathf.Sin(elapsed) * 0.08f;
                if (m_LineGlowObj != null) m_LineGlowObj.transform.localScale = Vector3.one * pulse;
                yield return null;
            }
        }

        public void SetPreview(bool show, Color color = default)
        {
            if (IsOccupied) return;

            if (show)
            {
                m_PreviewRenderer.sprite = SpriteFactory.GetPreviewCellSprite(color);
                m_PreviewRenderer.color = new Color(1f, 1f, 1f, 0.52f);
                m_PreviewObj.SetActive(true);
            }
            else
            {
                m_PreviewObj.SetActive(false);
            }
        }

        public void Fill(Color color, bool animate = true)
        {
            Fill(color, GemType.None, animate);
        }

        public void Fill(Color color, GemType gem, bool animate = true)
        {
            IsOccupied = true;
            CurrentColor = color;
            EmbeddedGem = gem;
            SetPreview(false);

            if (gem != GemType.None)
            {
                // Gem blocks keep the active skin's base color/gradient, but strip the center emblem (e.g. Electric bolt)
                m_BlockRenderer.sprite = SpriteFactory.GetGemCarrierBlockSprite(color);
            }
            else
            {
                m_BlockRenderer.sprite = SpriteFactory.GetBlockSprite(color);
            }
            m_BlockObj.SetActive(true);

            if (gem != GemType.None && m_GemRenderer != null)
            {
                m_GemRenderer.sprite = GemIconFactory.GetGemSprite(gem);
                m_GemObj.SetActive(true);
            }
            else if (m_GemObj != null)
            {
                m_GemObj.SetActive(false);
            }

            StopAllCoroutines();

            if (animate)
            {
                StartCoroutine(AnimatePlaceSquash());
            }
            else
            {
                m_BlockObj.transform.localScale = Vector3.one * BlockScale;
                m_IdleCoroutine = StartCoroutine(AnimateIdleBreathing());
            }
        }

        public void RefreshBlockSprite()
        {
            if (IsOccupied && m_BlockRenderer != null)
            {
                if (EmbeddedGem != GemType.None)
                {
                    m_BlockRenderer.sprite = SpriteFactory.GetGemCarrierBlockSprite(CurrentColor);
                    if (m_GemRenderer != null)
                    {
                        m_GemRenderer.sprite = GemIconFactory.GetGemSprite(EmbeddedGem);
                    }
                }
                else
                {
                    m_BlockRenderer.sprite = SpriteFactory.GetBlockSprite(CurrentColor);
                }
            }
        }

        /// <summary>
        /// Instantly frees the logical occupancy of this cell on the grid.
        /// This ensures move availability checks (Game Over) and batch spawning
        /// recognize the cell as open immediately on the same frame, while the visual
        /// laser sweep or bubble blast continues to animate.
        /// </summary>
        public void FreeLogicOccupancy()
        {
            IsOccupied = false;
        }

        public void Clear(bool animate = true, float delay = 0f, bool harvestGem = true)
        {
            if (!IsOccupied && (m_BlockObj == null || !m_BlockObj.activeSelf)) return;
            IsOccupied = false;

            // Harvest embedded gemstone towards adventure level objectives ONLY on real gameplay line clears!
            if (harvestGem && animate && EmbeddedGem != GemType.None)
            {
                if (LevelManager.Instance != null && LevelManager.Instance.IsLevelActive)
                {
                    LevelManager.Instance.OnGemCollected(EmbeddedGem, transform.position);
                }
            }

            EmbeddedGem = GemType.None;
            if (m_GemObj != null) m_GemObj.SetActive(false);

            StopAllCoroutines();

            if (animate)
            {
                StartCoroutine(AnimateBubbleBlast(delay));
            }
            else
            {
                m_BlockObj.SetActive(false);
            }
        }

        /// <summary>
        /// Crisp, snappy placement drop animation for spheres.
        /// Stays strictly within the golden circular socket boundary.
        /// </summary>
        private IEnumerator AnimatePlaceSquash()
        {
            Transform t = m_BlockObj.transform;
            t.localScale = Vector3.zero;

            float elapsed = 0f;
            float popDuration = 0.08f;

            // 1. Pop In swiftly to BlockScale
            while (elapsed < popDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / popDuration;
                float s = Mathf.Sin(progress * Mathf.PI * 0.5f);
                t.localScale = Vector3.one * (s * BlockScale);
                yield return null;
            }

            // 2. Subtle micro-settle bounce (strictly within socket rim)
            elapsed = 0f;
            float settleDuration = 0.06f;
            while (elapsed < settleDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / settleDuration;
                float bounce = 1.0f + Mathf.Sin(progress * Mathf.PI) * 0.02f;
                t.localScale = Vector3.one * (bounce * BlockScale);
                yield return null;
            }

            t.localScale = Vector3.one * BlockScale;
        }

        /// <summary>
        /// Exploding bubble / energy pop blast animation.
        /// </summary>
        private IEnumerator AnimateBubbleBlast(float delay)
        {
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            Transform t = m_BlockObj.transform;
            Color origColor = m_BlockRenderer.color;

            // Flash sphere white/bright
            m_BlockRenderer.color = Color.white;

            float elapsed = 0f;
            float duration = 0.24f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float p = elapsed / duration;

                // Expand outward like an exploding bubble, then pop away
                float scale;
                if (p < 0.28f)
                {
                    scale = Mathf.Lerp(1.0f, 1.35f, p / 0.28f);
                }
                else
                {
                    scale = Mathf.Lerp(1.35f, 0f, (p - 0.28f) / 0.72f);
                }

                t.localScale = Vector3.one * (scale * BlockScale);

                Color c = Color.Lerp(Color.white, origColor, p);
                c.a = Mathf.Clamp01(1f - p * 1.2f);
                m_BlockRenderer.color = c;

                yield return null;
            }

            m_BlockRenderer.color = origColor;
            t.localScale = Vector3.one * BlockScale;
            m_BlockObj.SetActive(false);
        }

        /// <summary>
        /// Sphere stays permanently locked, stable, and perfectly seated inside the circular socket.
        /// No scale oscillation so balls never bulge out or shrink away.
        /// </summary>
        private IEnumerator AnimateIdleBreathing()
        {
            if (m_BlockObj != null)
            {
                m_BlockObj.transform.localScale = Vector3.one * BlockScale;
            }
            yield break;
        }
    }
}
