using UnityEngine;
using UnityEngine.UI;

namespace BoxBlast
{
    /// <summary>
    /// Procedurally renders the luminous glowing celestial highway ribbon
    /// connecting level spheres on the Adventure Galaxy Trail map.
    /// Creates a smooth, anti-aliased winding ribbon with outer neon glow,
    /// vibrant aquamarine body, and crisp white center stripe.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public class GalaxyRoadGraphic : MaskableGraphic
    {
        [SerializeField] private int m_TotalLevels = 40;
        [SerializeField] private float m_StepY = 92f;
        [SerializeField] private float m_StartY = 130f;
        [SerializeField] private float m_Amplitude = 175f;
        [SerializeField] private float m_Frequency = 0.7853982f;
        [SerializeField] private float m_PhaseOffset = 1.5707963f;

        public float TotalContentHeight => m_StartY * 2f + (m_TotalLevels - 1) * m_StepY;

        public Vector2 GetLevelPosition(int level)
        {
            float lvlFrac = level;
            float wave = (lvlFrac - 1f) * m_Frequency + m_PhaseOffset;
            float px = Mathf.Sin(wave) * m_Amplitude;
            float py = m_StartY + (lvlFrac - 1f) * m_StepY;
            return new Vector2(px, py);
        }

        public Vector2 GetLevelNormal(int level)
        {
            float lvlFrac = level;
            float wave = (lvlFrac - 1f) * m_Frequency + m_PhaseOffset;
            float dx = Mathf.Cos(wave) * m_Frequency * m_Amplitude;
            float dy = m_StepY;
            return new Vector2(-dy, dx).normalized;
        }

        public void Setup(int totalLevels, float stepY, float startY, float amplitude, float frequency, float phaseOffset)
        {
            m_TotalLevels = totalLevels;
            m_StepY = stepY;
            m_StartY = startY;
            m_Amplitude = amplitude;
            m_Frequency = frequency;
            m_PhaseOffset = phaseOffset;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (m_TotalLevels <= 1) return;

            int samples = (m_TotalLevels - 1) * 20;
            if (samples < 20) samples = 20;

            // Rich warm 3D Golden Road Highway Palette
            Color glowOuter = new Color(0.95f, 0.65f, 0.10f, 0f);
            Color glowInner = new Color(1.0f, 0.78f, 0.20f, 0.45f);
            Color bodyColor = new Color(0.96f, 0.80f, 0.25f, 0.98f);
            Color coreColor = new Color(1.0f, 0.98f, 0.85f, 1.0f);

            float wGlow = 46f;
            float wOuter = 30f;
            float wBody = 20f;
            float wCore = 7f;

            for (int i = 0; i <= samples; i++)
            {
                float progress = (float)i / samples;
                // Extend 2 full levels below Level 1 and 2 full levels above Level N for seamless ends
                float lvlFrac = -1.5f + progress * (m_TotalLevels + 2.5f);
                float wave = (lvlFrac - 1f) * m_Frequency + m_PhaseOffset;

                float px = Mathf.Sin(wave) * m_Amplitude;
                float py = m_StartY + (lvlFrac - 1f) * m_StepY;
                Vector2 center = new Vector2(px, py);

                // Tangent vector
                float dx = Mathf.Cos(wave) * m_Frequency * m_Amplitude;
                float dy = m_StepY;
                Vector2 normal = new Vector2(-dy, dx).normalized;

                // 8 Vertices across the ribbon cross-section:
                // 0: Outer Left Glow (alpha 0)
                // 1: Inner Left Glow
                // 2: Body Left (cyan)
                // 3: Core Left (white)
                // 4: Core Right (white)
                // 5: Body Right (cyan)
                // 6: Inner Right Glow
                // 7: Outer Right Glow (alpha 0)
                vh.AddVert(center - normal * wGlow, glowOuter, Vector2.zero);
                vh.AddVert(center - normal * wOuter, glowInner, Vector2.zero);
                vh.AddVert(center - normal * wBody, bodyColor, Vector2.zero);
                vh.AddVert(center - normal * wCore, coreColor, Vector2.zero);
                vh.AddVert(center + normal * wCore, coreColor, Vector2.zero);
                vh.AddVert(center + normal * wBody, bodyColor, Vector2.zero);
                vh.AddVert(center + normal * wOuter, glowInner, Vector2.zero);
                vh.AddVert(center + normal * wGlow, glowOuter, Vector2.zero);

                if (i > 0)
                {
                    int prev = (i - 1) * 8;
                    int curr = i * 8;

                    for (int q = 0; q < 7; q++)
                    {
                        AddRibbonQuad(vh, prev + q, prev + q + 1, curr + q + 1, curr + q);
                    }
                }
            }
        }

        private static void AddRibbonQuad(VertexHelper vh, int i0, int i1, int i2, int i3)
        {
            vh.AddTriangle(i0, i1, i2);
            vh.AddTriangle(i0, i2, i3);
        }
    }
}
