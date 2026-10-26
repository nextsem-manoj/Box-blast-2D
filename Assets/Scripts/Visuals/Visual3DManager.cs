using System.Collections.Generic;
using UnityEngine;

namespace BoxBlast.Visuals3D
{
    /// <summary>
    /// Master 3D Visual Controller for Box Blast 3D.
    /// Manages 3D rounded cube meshes, crystal spheres, faceted gems, 
    /// PBR glossy materials, directional lighting, and shadows.
    /// Works seamlessly with the Box-blast-2D color design.
    /// </summary>
    public class Visual3DManager : MonoBehaviour
    {
        private static Visual3DManager s_Instance;
        public static Visual3DManager Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    GameObject go = new GameObject("Visual3DManager");
                    s_Instance = go.AddComponent<Visual3DManager>();
                    DontDestroyOnLoad(go);
                }
                return s_Instance;
            }
        }

        public bool Is3DActive { get; set; } = true;
        public bool UseRoundedCube { get; set; } = true; // true = rounded beveled cube, false = smooth crystal sphere

        private Mesh m_RoundedCubeMesh;
        private Mesh m_SphereMesh;
        private Mesh m_DiamondMesh;
        private Mesh m_PedestalMesh;

        private Material m_BaseLitMaterial;
        private Material m_PedestalMaterial;
        private readonly Dictionary<string, Material> m_ColorMaterialCache = new Dictionary<string, Material>();

        private GameObject m_PedestalInstance;
        private Light m_DirectionalLight;

        private void Awake()
        {
            if (s_Instance != null && s_Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            s_Instance = this;
            Initialize3DAssets();
        }

        public void Initialize3DAssets()
        {
            // 1. Load or Generate 3D Meshes
            if (m_RoundedCubeMesh == null)
            {
                m_RoundedCubeMesh = Resources.Load<Mesh>("Models/Cube_Rounded_Beveled");
                if (m_RoundedCubeMesh == null)
                {
                    m_RoundedCubeMesh = ModelGenerator3D.CreateRoundedCubeMesh(size: 1f, bevelRadius: 0.18f, subdivisions: 6);
                }
            }

            if (m_SphereMesh == null)
            {
                m_SphereMesh = Resources.Load<Mesh>("Models/Sphere_Crystal_Smooth");
                if (m_SphereMesh == null)
                {
                    m_SphereMesh = ModelGenerator3D.CreateCrystalSphereMesh(radius: 0.5f, latSegments: 20, lonSegments: 24);
                }
            }

            if (m_DiamondMesh == null)
            {
                m_DiamondMesh = Resources.Load<Mesh>("Models/Gem_Diamond_Faceted");
                if (m_DiamondMesh == null)
                {
                    m_DiamondMesh = ModelGenerator3D.CreateDiamondGemMesh(radius: 0.45f);
                }
            }

            if (m_PedestalMesh == null)
            {
                m_PedestalMesh = Resources.Load<Mesh>("Models/Board_Pedestal_8x8");
                if (m_PedestalMesh == null)
                {
                    m_PedestalMesh = ModelGenerator3D.CreateBoardPedestalMesh(width: 6.4f, height: 0.5f);
                }
            }

            // 2. Base PBR Shader
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null) litShader = Shader.Find("Standard");
            if (litShader == null) litShader = Shader.Find("Mobile/Diffuse");

            if (litShader != null)
            {
                m_BaseLitMaterial = new Material(litShader);
            }
        }

        /// <summary>
        /// Returns a high-gloss PBR crystal material matching the exact Color of the block.
        /// </summary>
        public Material GetPBRMaterial(Color color)
        {
            string hex = ColorUtility.ToHtmlStringRGBA(color);
            if (m_ColorMaterialCache.TryGetValue(hex, out Material cachedMat) && cachedMat != null)
            {
                return cachedMat;
            }

            Material mat = (m_BaseLitMaterial != null) ? new Material(m_BaseLitMaterial) : new Material(Shader.Find("Standard"));
            mat.name = $"Mat_Gem_{hex}";
            mat.color = color;

            // Ultra-glossy gemstone finish
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.92f);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 0.92f);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.15f);

            m_ColorMaterialCache[hex] = mat;
            return mat;
        }

        /// <summary>
        /// Returns dark brushed metallic pedestal material.
        /// </summary>
        public Material GetPedestalMaterial()
        {
            if (m_PedestalMaterial == null)
            {
                m_PedestalMaterial = (m_BaseLitMaterial != null) ? new Material(m_BaseLitMaterial) : new Material(Shader.Find("Standard"));
                m_PedestalMaterial.name = "Mat_Pedestal_DarkMetallic";
                m_PedestalMaterial.color = new Color(0.12f, 0.14f, 0.20f);
                if (m_PedestalMaterial.HasProperty("_Smoothness")) m_PedestalMaterial.SetFloat("_Smoothness", 0.75f);
                if (m_PedestalMaterial.HasProperty("_Metallic")) m_PedestalMaterial.SetFloat("_Metallic", 0.85f);
            }
            return m_PedestalMaterial;
        }

        /// <summary>
        /// Attaches or updates 3D MeshFilter and MeshRenderer on a dedicated child GameObject.
        /// This completely avoids Unity's conflict with the existing SpriteRenderer on targetObj.
        /// </summary>
        public void Apply3DBlockVisual(GameObject targetObj, Color color)
        {
            if (targetObj == null) return;
            if (m_RoundedCubeMesh == null || m_SphereMesh == null) Initialize3DAssets();

            Transform child = targetObj.transform.Find("Mesh3D_Block");
            GameObject meshChild;
            if (child == null)
            {
                meshChild = new GameObject("Mesh3D_Block");
                meshChild.transform.SetParent(targetObj.transform, false);
                meshChild.transform.localPosition = Vector3.zero;
                meshChild.transform.localRotation = Quaternion.identity;
                meshChild.transform.localScale = Vector3.one;
            }
            else
            {
                meshChild = child.gameObject;
            }

            MeshFilter mf = meshChild.GetComponent<MeshFilter>();
            if (mf == null) mf = meshChild.AddComponent<MeshFilter>();

            MeshRenderer mr = meshChild.GetComponent<MeshRenderer>();
            if (mr == null) mr = meshChild.AddComponent<MeshRenderer>();

            mf.sharedMesh = UseRoundedCube ? m_RoundedCubeMesh : m_SphereMesh;
            mr.sharedMaterial = GetPBRMaterial(color);
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            mr.receiveShadows = true;
            mr.enabled = true;
            meshChild.SetActive(true);

            // Hide 2D SpriteRenderer if present so 3D mesh renders cleanly
            SpriteRenderer sr = targetObj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.enabled = false;
            }
        }

        /// <summary>
        /// Applies 3D faceted diamond gem visual on a dedicated child GameObject.
        /// </summary>
        public void Apply3DGemVisual(GameObject gemObj, GemType gem)
        {
            if (gemObj == null || gem == GemType.None) return;
            if (m_DiamondMesh == null) Initialize3DAssets();

            Transform child = gemObj.transform.Find("Mesh3D_Gem");
            GameObject meshChild;
            if (child == null)
            {
                meshChild = new GameObject("Mesh3D_Gem");
                meshChild.transform.SetParent(gemObj.transform, false);
                meshChild.transform.localPosition = Vector3.zero;
                meshChild.transform.localRotation = Quaternion.identity;
                meshChild.transform.localScale = Vector3.one;
            }
            else
            {
                meshChild = child.gameObject;
            }

            MeshFilter mf = meshChild.GetComponent<MeshFilter>();
            if (mf == null) mf = meshChild.AddComponent<MeshFilter>();

            MeshRenderer mr = meshChild.GetComponent<MeshRenderer>();
            if (mr == null) mr = meshChild.AddComponent<MeshRenderer>();

            mf.sharedMesh = m_DiamondMesh;
            Color gemColor = GetGemColor(gem);
            mr.sharedMaterial = GetPBRMaterial(gemColor);
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            mr.receiveShadows = true;
            mr.enabled = true;
            meshChild.SetActive(true);

            SpriteRenderer sr = gemObj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.enabled = false;
            }
        }

        private Color GetGemColor(GemType gem)
        {
            switch (gem)
            {
                case GemType.RedRuby: return new Color(1.0f, 0.12f, 0.28f);
                case GemType.BlueDiamond: return new Color(0.35f, 0.85f, 1.0f);
                case GemType.OrangePentagon: return new Color(1.0f, 0.58f, 0.06f);
                case GemType.YellowStar: return new Color(1.0f, 0.84f, 0.00f);
                default: return Color.white;
            }
        }

        /// <summary>
        /// Ensures directional lighting and shadows are present in the scene for 3D depth.
        /// </summary>
        public void EnsureLighting(Camera mainCam)
        {
            if (m_DirectionalLight == null)
            {
                Light[] existingLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
                foreach (var l in existingLights)
                {
                    if (l.type == LightType.Directional)
                    {
                        m_DirectionalLight = l;
                        break;
                    }
                }

                if (m_DirectionalLight == null)
                {
                    GameObject lightGo = new GameObject("3D_Directional_Light");
                    m_DirectionalLight = lightGo.AddComponent<Light>();
                    m_DirectionalLight.type = LightType.Directional;
                    m_DirectionalLight.color = new Color(1.0f, 0.96f, 0.90f);
                    m_DirectionalLight.intensity = 1.25f;
                    m_DirectionalLight.shadows = LightShadows.Soft;
                    lightGo.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
                }
            }
        }

        /// <summary>
        /// Manages board background platform. Keeps board clean and lets the royal frame and 3D blocks shine.
        /// </summary>
        public void SpawnPedestal(Vector3 boardCenter, float boardSize)
        {
            // Do not render the raw edge-on mesh that cuts across the 8x8 grid.
            if (m_PedestalInstance != null)
            {
                m_PedestalInstance.SetActive(false);
            }
        }
    }
}
