using System.IO;
using UnityEngine;
using UnityEditor;

namespace BoxBlast.Visuals3D
{
    public static class ModelGeneratorEditor
    {
        [MenuItem("Tools/Box Blast 3D/Generate All 3D Models & Prefabs")]
        public static void GenerateAll()
        {
            string modelsDir = "Assets/Models";
            string prefabsDir = "Assets/Models/Prefabs";
            string materialsDir = "Assets/Models/Materials";

            if (!AssetDatabase.IsValidFolder("Assets/Models"))
                AssetDatabase.CreateFolder("Assets", "Models");
            if (!AssetDatabase.IsValidFolder(prefabsDir))
                AssetDatabase.CreateFolder("Assets/Models", "Prefabs");
            if (!AssetDatabase.IsValidFolder(materialsDir))
                AssetDatabase.CreateFolder("Assets/Models", "Materials");

            // 1. Generate & Save OBJ files
            GenerateAndSaveObj(ModelGenerator3D.CreateRoundedCubeMesh(), "Cube_Rounded_Beveled.obj", modelsDir);
            GenerateAndSaveObj(ModelGenerator3D.CreateCrystalSphereMesh(), "Sphere_Crystal_Smooth.obj", modelsDir);
            GenerateAndSaveObj(ModelGenerator3D.CreateDiamondGemMesh(), "Gem_Diamond_Faceted.obj", modelsDir);
            GenerateAndSaveObj(ModelGenerator3D.CreateBoardPedestalMesh(), "Board_Pedestal_8x8.obj", modelsDir);

            AssetDatabase.Refresh();

            // 2. Generate PBR Materials matching Box-blast-2D palette
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null) litShader = Shader.Find("Standard");

            Material[] gemMaterials = new Material[ModelGenerator3D.PaletteColors.Length];

            for (int i = 0; i < ModelGenerator3D.PaletteColors.Length; i++)
            {
                Color color = ModelGenerator3D.PaletteColors[i];
                string name = ModelGenerator3D.ColorNames[i];
                string matPath = $"{materialsDir}/Mat_{name}.mat";

                Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                if (mat == null)
                {
                    mat = new Material(litShader);
                    AssetDatabase.CreateAsset(mat, matPath);
                }

                mat.color = color;
                // High gloss PBR crystal jewel finish
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.92f);
                if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 0.92f);
                if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.15f);

                EditorUtility.SetDirty(mat);
                gemMaterials[i] = mat;
            }

            // Pedestal Material (Dark brushed metallic)
            string pedMatPath = $"{materialsDir}/Mat_Pedestal_Metallic.mat";
            Material pedMat = AssetDatabase.LoadAssetAtPath<Material>(pedMatPath);
            if (pedMat == null)
            {
                pedMat = new Material(litShader);
                AssetDatabase.CreateAsset(pedMat, pedMatPath);
            }
            pedMat.color = new Color(0.12f, 0.14f, 0.20f);
            if (pedMat.HasProperty("_Smoothness")) pedMat.SetFloat("_Smoothness", 0.75f);
            if (pedMat.HasProperty("_Metallic")) pedMat.SetFloat("_Metallic", 0.85f);
            EditorUtility.SetDirty(pedMat);

            // 3. Generate Prefabs
            CreateBlockPrefab("Block_3D_Cube", $"{modelsDir}/Cube_Rounded_Beveled.obj", gemMaterials[0], prefabsDir);
            CreateBlockPrefab("Block_3D_Sphere", $"{modelsDir}/Sphere_Crystal_Smooth.obj", gemMaterials[1], prefabsDir);
            CreateBlockPrefab("Board_3D_Pedestal", $"{modelsDir}/Board_Pedestal_8x8.obj", pedMat, prefabsDir);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("<color=#00FF88><b>[BoxBlast3D]</b> Successfully generated all 3D Models, PBR Materials, and Prefabs!</color>");
        }

        private static void GenerateAndSaveObj(Mesh mesh, string fileName, string folder)
        {
            string fullPath = Path.Combine(Application.dataPath, folder.Replace("Assets/", ""), fileName);
            string objString = ModelGenerator3D.MeshToObjString(mesh);
            File.WriteAllText(fullPath, objString);
            Debug.Log($"[BoxBlast3D] Saved 3D OBJ: {fullPath}");
        }

        private static void CreateBlockPrefab(string prefabName, string objAssetPath, Material defaultMat, string prefabFolder)
        {
            string prefabPath = $"{prefabFolder}/{prefabName}.prefab";
            GameObject go = new GameObject(prefabName);

            MeshFilter mf = go.AddComponent<MeshFilter>();
            MeshRenderer mr = go.AddComponent<MeshRenderer>();

            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(objAssetPath);
            if (mesh != null)
            {
                mf.sharedMesh = mesh;
            }
            if (defaultMat != null)
            {
                mr.sharedMaterial = defaultMat;
            }

            // Shadow casting
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            mr.receiveShadows = true;

            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            GameObject.DestroyImmediate(go);
            Debug.Log($"[BoxBlast3D] Created Prefab: {prefabPath}");
        }
    }
}
