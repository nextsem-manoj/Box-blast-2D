#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BoxBlast.Editor
{
    public static class AssetProcessor3D
    {
        private const string ArtifactsDir = @"C:\Users\nextsem\.gemini\antigravity-ide\brain\866b4f20-441d-497d-8e9d-ef2614e38358";

        [MenuItem("Box Blast/Process 3D Royal Assets", false, 30)]
        public static void ProcessAllAssets()
        {
            string bgSource = Path.Combine(ArtifactsDir, "gameplay_bg_clean_1791709474292.jpg");
            string headerSource = Path.Combine(ArtifactsDir, "gameplay_header_3d_1791709505745.jpg");
            string boardFrameSource = Path.Combine(ArtifactsDir, "gameplay_board_single_frame_1791711257485.jpg");
            string pedestalSource = Path.Combine(ArtifactsDir, "gameplay_pedestal_3d_1791709562398.jpg");
            string objectivesSource = Path.Combine(ArtifactsDir, "gameplay_objectives_3d_1791709591030.jpg");

            string resourcesDir = Path.Combine(Application.dataPath, "Resources");

            // 1. Process Background
            if (File.Exists(bgSource))
            {
                string targetPath = Path.Combine(resourcesDir, "GameplayBackground_Concept1.png");
                Texture2D bgTex = LoadTextureFromFile(bgSource);
                if (bgTex != null)
                {
                    File.WriteAllBytes(targetPath, bgTex.EncodeToPNG());
                    ConfigureSprite(targetPath, 4096);
                    Debug.Log("Processed 3D Background: " + targetPath);
                }
            }

            // 2. Process Header (Flood-fill outer black to transparent)
            if (File.Exists(headerSource))
            {
                string targetPath = Path.Combine(resourcesDir, "Gameplay_Header_Concept1.png");
                Texture2D headerTex = LoadTextureFromFile(headerSource);
                if (headerTex != null)
                {
                    Texture2D croppedHeader = CropAndKeyOuterBlack(headerTex, 32);
                    File.WriteAllBytes(targetPath, croppedHeader.EncodeToPNG());
                    ConfigureSprite(targetPath, 2048);
                    Debug.Log("Processed 3D Header: " + targetPath);
                }
            }

            // 3. Process Objectives Bar
            if (File.Exists(objectivesSource))
            {
                string targetPath = Path.Combine(resourcesDir, "Gameplay_Objectives_Concept1.png");
                Texture2D objTex = LoadTextureFromFile(objectivesSource);
                if (objTex != null)
                {
                    Texture2D croppedObj = CropAndKeyOuterBlack(objTex, 32);
                    File.WriteAllBytes(targetPath, croppedObj.EncodeToPNG());
                    ConfigureSprite(targetPath, 2048);
                    Debug.Log("Processed 3D Objectives: " + targetPath);
                }
            }

            // 4. Process Pedestal
            if (File.Exists(pedestalSource))
            {
                string targetPath = Path.Combine(resourcesDir, "Gameplay_Pedestal_Concept1.png");
                Texture2D pedTex = LoadTextureFromFile(pedestalSource);
                if (pedTex != null)
                {
                    Texture2D croppedPed = CropAndKeyOuterBlack(pedTex, 32);
                    File.WriteAllBytes(targetPath, croppedPed.EncodeToPNG());
                    ConfigureSprite(targetPath, 1024);
                    Debug.Log("Processed 3D Pedestal: " + targetPath);
                }
            }

            // 5. Process Board Frame
            if (File.Exists(boardFrameSource))
            {
                string targetPath = Path.Combine(resourcesDir, "Gameplay_BoardFrame_Concept1.png");
                Texture2D frameTex = LoadTextureFromFile(boardFrameSource);
                if (frameTex != null)
                {
                    Texture2D croppedFrame = CropAndKeyOuterBlack(frameTex, 32);
                    File.WriteAllBytes(targetPath, croppedFrame.EncodeToPNG());
                    ConfigureSprite(targetPath, 2048);
                    Debug.Log("Processed 3D Board Frame: " + targetPath);
                }
            }

            AssetDatabase.Refresh();
            Debug.Log("✅ All 3D Royal Gameplay Assets successfully processed and configured in Resources!");
        }

        private static Texture2D LoadTextureFromFile(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (tex.LoadImage(bytes))
            {
                return tex;
            }
            return null;
        }

        private static Texture2D CropAndKeyOuterBlack(Texture2D source, byte blackThreshold)
        {
            int w = source.width;
            int h = source.height;
            Color32[] pixels = source.GetPixels32();

            // Flood-fill outer black pixels using BFS
            bool[,] visited = new bool[w, h];
            System.Collections.Generic.Queue<Vector2Int> queue = new System.Collections.Generic.Queue<Vector2Int>();

            // Push all border pixels that are dark
            for (int x = 0; x < w; x++)
            {
                int idx0 = x; // y = 0
                int idxTop = (h - 1) * w + x; // y = h - 1

                if (IsDark(pixels[idx0], blackThreshold)) { visited[x, 0] = true; queue.Enqueue(new Vector2Int(x, 0)); }
                if (IsDark(pixels[idxTop], blackThreshold)) { visited[x, h - 1] = true; queue.Enqueue(new Vector2Int(x, h - 1)); }
            }
            for (int y = 0; y < h; y++)
            {
                int idx0 = y * w; // x = 0
                int idxRight = y * w + (w - 1); // x = w - 1

                if (!visited[0, y] && IsDark(pixels[idx0], blackThreshold)) { visited[0, y] = true; queue.Enqueue(new Vector2Int(0, y)); }
                if (!visited[w - 1, y] && IsDark(pixels[idxRight], blackThreshold)) { visited[w - 1, y] = true; queue.Enqueue(new Vector2Int(w - 1, y)); }
            }

            Vector2Int[] dirs = new Vector2Int[]
            {
                new Vector2Int(-1, 0), new Vector2Int(1, 0),
                new Vector2Int(0, -1), new Vector2Int(0, 1)
            };

            while (queue.Count > 0)
            {
                Vector2Int pt = queue.Dequeue();
                int idx = pt.y * w + pt.x;
                pixels[idx] = new Color32(0, 0, 0, 0);

                for (int d = 0; d < 4; d++)
                {
                    int nx = pt.x + dirs[d].x;
                    int ny = pt.y + dirs[d].y;

                    if (nx >= 0 && nx < w && ny >= 0 && ny < h && !visited[nx, ny])
                    {
                        int nIdx = ny * w + nx;
                        Color32 nc = pixels[nIdx];
                        int maxVal = Mathf.Max(nc.r, Mathf.Max(nc.g, nc.b));

                        if (maxVal < blackThreshold)
                        {
                            visited[nx, ny] = true;
                            queue.Enqueue(new Vector2Int(nx, ny));
                        }
                        else if (maxVal < blackThreshold + 40)
                        {
                            // Soft anti-aliased edge
                            visited[nx, ny] = true;
                            byte alpha = (byte)Mathf.Clamp((maxVal - blackThreshold) * (255f / 40f), 0, 255);
                            pixels[nIdx] = new Color32(nc.r, nc.g, nc.b, alpha);
                        }
                    }
                }
            }

            // Find tight bounding box of remaining non-transparent pixels
            int minX = w, maxX = 0, minY = h, maxY = 0;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (pixels[y * w + x].a > 0)
                    {
                        if (x < minX) minX = x;
                        if (x > maxX) maxX = x;
                        if (y < minY) minY = y;
                        if (y > maxY) maxY = y;
                    }
                }
            }

            if (minX > maxX || minY > maxY)
            {
                minX = 0; maxX = w - 1; minY = 0; maxY = h - 1;
            }

            // Add slight 2px margin
            minX = Mathf.Max(0, minX - 2);
            maxX = Mathf.Min(w - 1, maxX + 2);
            minY = Mathf.Max(0, minY - 2);
            maxY = Mathf.Min(h - 1, maxY + 2);

            int cropW = maxX - minX + 1;
            int cropH = maxY - minY + 1;

            Texture2D result = new Texture2D(cropW, cropH, TextureFormat.RGBA32, false);
            Color32[] croppedPixels = new Color32[cropW * cropH];

            for (int y = 0; y < cropH; y++)
            {
                for (int x = 0; x < cropW; x++)
                {
                    croppedPixels[y * cropW + x] = pixels[(minY + y) * w + (minX + x)];
                }
            }

            result.SetPixels32(croppedPixels);
            result.Apply(false, false);
            return result;
        }

        private static bool IsDark(Color32 c, byte thresh)
        {
            return c.r < thresh && c.g < thresh && c.b < thresh;
        }

        private static void ConfigureSprite(string absPath, int maxSize)
        {
            string relPath = "Assets" + absPath.Substring(Application.dataPath.Length).Replace('\\', '/');
            AssetDatabase.ImportAsset(relPath, ImportAssetOptions.ForceUpdate);

            TextureImporter importer = AssetImporter.GetAtPath(relPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.anisoLevel = 16;
                importer.alphaIsTransparency = true;
                importer.maxTextureSize = maxSize;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }
    }
}
#endif
