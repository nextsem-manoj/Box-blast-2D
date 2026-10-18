using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BoxBlast
{
    /// <summary>
    /// Custom UI BaseMeshEffect that arches Text along a smooth curve
    /// to match royal golden banners, and applies vertical gradient shading.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Text))]
    public class CurvedTextEffect : BaseMeshEffect
    {
        [Tooltip("The vertical peak of the arch in pixels (positive = arches up in center)")]
        public float archHeight = 20f;

        [Tooltip("How much the letters rotate along the curve tangent")]
        [Range(0f, 1f)]
        public float rotationFactor = 0.35f;

        [Tooltip("Enable 2-color vertical gradient across each character")]
        public bool useGradient = false;
        public Color topColor = new Color(1.0f, 0.98f, 0.78f, 1f);
        public Color bottomColor = new Color(0.96f, 0.58f, 0.04f, 1f);

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive() || vh.currentVertCount == 0) return;

            List<UIVertex> stream = new List<UIVertex>();
            vh.GetUIVertexStream(stream);

            if (stream.Count == 0) return;

            // 1. Calculate overall text bounds
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            for (int i = 0; i < stream.Count; i++)
            {
                float x = stream[i].position.x;
                if (x < minX) minX = x;
                if (x > maxX) maxX = x;
            }

            float width = maxX - minX;
            if (width <= 0.01f) return;
            float midX = (minX + maxX) * 0.5f;
            float halfWidth = width * 0.5f;

            // 2. Process per character (6 vertices per quad in vertex helper stream)
            for (int i = 0; i < stream.Count; i += 6)
            {
                int count = Mathf.Min(6, stream.Count - i);

                float charMinX = float.MaxValue;
                float charMaxX = float.MinValue;
                float charMinY = float.MaxValue;
                float charMaxY = float.MinValue;

                for (int j = 0; j < count; j++)
                {
                    Vector3 pos = stream[i + j].position;
                    if (pos.x < charMinX) charMinX = pos.x;
                    if (pos.x > charMaxX) charMaxX = pos.x;
                    if (pos.y < charMinY) charMinY = pos.y;
                    if (pos.y > charMaxY) charMaxY = pos.y;
                }

                float charCenterX = (charMinX + charMaxX) * 0.5f;
                float charCenterY = (charMinY + charMaxY) * 0.5f;
                float charHeight = Mathf.Max(1f, charMaxY - charMinY);

                // Normalized position: -1 (left edge) to 0 (center) to +1 (right edge)
                float normX = Mathf.Clamp((charCenterX - midX) / halfWidth, -1f, 1f);

                // Cosine arch: peak at normX = 0, zero at edges
                float yOffset = archHeight * Mathf.Cos(normX * Mathf.PI * 0.5f);

                // Derivative for tangent rotation angle
                float dy = -archHeight * (Mathf.PI * 0.5f) * Mathf.Sin(normX * Mathf.PI * 0.5f);
                float angleDeg = Mathf.Atan2(dy, halfWidth) * Mathf.Rad2Deg * rotationFactor;
                Quaternion rot = Quaternion.Euler(0f, 0f, angleDeg);

                // Transform each vertex of this character
                for (int j = 0; j < count; j++)
                {
                    UIVertex vert = stream[i + j];

                    // Optional vertical gradient
                    if (useGradient)
                    {
                        float t = Mathf.Clamp01((vert.position.y - charMinY) / charHeight);
                        Color grad = Color.Lerp(bottomColor, topColor, t);
                        // Multiply with base alpha
                        grad.a *= ((float)vert.color.a / 255f);
                        vert.color = (Color32)grad;
                    }

                    // Rotate around character center & offset along curve
                    Vector3 localPos = vert.position - new Vector3(charCenterX, charCenterY, 0f);
                    Vector3 transformed = (rot * localPos) + new Vector3(charCenterX, charCenterY + yOffset, 0f);
                    vert.position = transformed;

                    stream[i + j] = vert;
                }
            }

            vh.Clear();
            vh.AddUIVertexTriangleStream(stream);
        }
    }
}
