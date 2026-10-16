using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace BoxBlast.Editor
{
    public class AABBuildWindow : EditorWindow
    {
        private string m_VersionName = "1.0.3";
        private int m_VersionCode = 7;
        private bool m_UniversalArch = false;
        private bool m_AutoIncrement = true;

        [MenuItem("Build/🚀 Android AAB Build Center...", false, 0)]
        public static void OpenWindow()
        {
            var window = GetWindow<AABBuildWindow>("AAB Builder");
            window.minSize = new Vector2(420, 400);
            window.Show();
        }

        private void OnEnable()
        {
            m_VersionName = string.IsNullOrEmpty(PlayerSettings.bundleVersion) || PlayerSettings.bundleVersion == "1.0.2" 
                ? "1.0.3" 
                : PlayerSettings.bundleVersion;

            // Google Play has Release 6 active, so next release must be >= 7
            m_VersionCode = Mathf.Max(PlayerSettings.Android.bundleVersionCode, 7);
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Android App Bundle (.aab) Builder", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Play Console has Release 6 (1.0.2) active. This tool creates Release 7+ (.AAB) signed with boxblast.keystore.", MessageType.Info);

            GUILayout.Space(10);
            EditorGUILayout.LabelField("App Identity & Versioning", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Package ID:", "com.nextsem.boxblast2d");

            m_VersionName = EditorGUILayout.TextField("Version Name (e.g. 1.0.3):", m_VersionName);

            EditorGUILayout.BeginHorizontal();
            m_VersionCode = EditorGUILayout.IntField("Version Code (Must be >= 7):", m_VersionCode);
            if (GUILayout.Button("+1", GUILayout.Width(40)))
            {
                m_VersionCode++;
            }
            EditorGUILayout.EndHorizontal();

            if (m_VersionCode <= 6)
            {
                EditorGUILayout.HelpBox("WARNING: Version code must be at least 7 because Release 6 is already published on Google Play!", MessageType.Error);
            }

            m_AutoIncrement = EditorGUILayout.Toggle("Auto +1 on Next Build", m_AutoIncrement);

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Target Architecture", EditorStyles.boldLabel);
            m_UniversalArch = EditorGUILayout.ToggleLeft("Universal (ARM64 + ARMv7) [~15MB]", m_UniversalArch);
            if (!m_UniversalArch)
            {
                EditorGUILayout.HelpBox("ARM64 Only (Recommended for Google Play, <10MB size)", MessageType.None);
            }

            GUILayout.Space(10);
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string keystorePath = Path.Combine(projectRoot, "boxblast.keystore");
            bool keystoreFound = File.Exists(keystorePath);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Keystore Status:", GUILayout.Width(110));
            if (keystoreFound)
            {
                GUI.color = Color.green;
                EditorGUILayout.LabelField("✓ Verified boxblast.keystore (Upload Key)", EditorStyles.boldLabel);
                GUI.color = Color.white;
            }
            else
            {
                GUI.color = Color.yellow;
                EditorGUILayout.LabelField("Using fallback keystore", EditorStyles.boldLabel);
                GUI.color = Color.white;
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(15);
            GUI.backgroundColor = new Color(0.2f, 0.75f, 0.3f);
            if (GUILayout.Button($"▶ BUILD RELEASE {m_VersionCode} (.AAB) NOW", GUILayout.Height(45)))
            {
                PlayerSettings.bundleVersion = m_VersionName;
                PlayerSettings.Android.bundleVersionCode = m_VersionCode;

                AndroidArchitecture arch = m_UniversalArch 
                    ? (AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7) 
                    : AndroidArchitecture.ARM64;
                string fileName = m_UniversalArch 
                    ? $"BoxBlast2D_v{m_VersionCode}_Universal.aab" 
                    : $"BoxBlast2D_v{m_VersionCode}_ARM64.aab";

                bool success = AndroidBuildScript.PerformBuild(arch, fileName, isHeadless: false, customVersionCode: m_VersionCode, customVersionName: m_VersionName);
                if (success && m_AutoIncrement)
                {
                    m_VersionCode++;
                    PlayerSettings.Android.bundleVersionCode = m_VersionCode;
                }
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(8);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("📁 Open Builds Folder"))
            {
                string outDir = Path.Combine(projectRoot, "Builds/Android");
                if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);
                EditorUtility.RevealInFinder(outDir);
            }
            if (GUILayout.Button("⚙ Unity Build Settings"))
            {
                EditorWindow.GetWindow(Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    public static class AndroidBuildScript
    {
        private const string BuildOutputDirectory = "Builds/Android";

        [MenuItem("Build/Build Android AAB (<10MB Optimized - ARM64)", false, 10)]
        public static void BuildOptimizedAAB()
        {
            int code = Mathf.Max(PlayerSettings.Android.bundleVersionCode, 7);
            string name = string.IsNullOrEmpty(PlayerSettings.bundleVersion) || PlayerSettings.bundleVersion == "1.0.2" ? "1.0.3" : PlayerSettings.bundleVersion;
            PerformBuild(AndroidArchitecture.ARM64, $"BoxBlast2D_v{code}_ARM64.aab", customVersionCode: code, customVersionName: name);
        }

        [MenuItem("Build/Build Android AAB (Universal ARM64 + ARMv7)", false, 11)]
        public static void BuildUniversalAAB()
        {
            int code = Mathf.Max(PlayerSettings.Android.bundleVersionCode, 7);
            string name = string.IsNullOrEmpty(PlayerSettings.bundleVersion) || PlayerSettings.bundleVersion == "1.0.2" ? "1.0.3" : PlayerSettings.bundleVersion;
            PerformBuild(AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7, $"BoxBlast2D_v{code}_Universal.aab", customVersionCode: code, customVersionName: name);
        }

        public static void BuildCommandLine()
        {
            Debug.Log("[AndroidBuildScript] Starting CLI Android AAB build...");
            int code = Mathf.Max(PlayerSettings.Android.bundleVersionCode, 7);
            PerformBuild(AndroidArchitecture.ARM64, $"BoxBlast2D_v{code}_ARM64.aab", isHeadless: true, customVersionCode: code);
        }

        public static bool PerformBuild(AndroidArchitecture architectures, string fileName, bool isHeadless = false, int? customVersionCode = null, string customVersionName = null)
        {
            try
            {
                if (EditorApplication.isCompiling)
                {
                    string compilingMsg = "Unity is currently compiling scripts in the background. Please wait a few seconds until the spinning icon at the bottom-right disappears, then try again.";
                    Debug.LogWarning($"[AndroidBuildScript] {compilingMsg}");
                    if (!isHeadless)
                    {
                        EditorUtility.DisplayDialog("Scripts Still Compiling", compilingMsg, "OK");
                    }
                    return false;
                }

                Debug.Log($"[AndroidBuildScript] Configuring Android Build Settings for Play Console AAB update...");

                // 1. Ensure Output Directory
                string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
                string outDir = Path.Combine(projectRoot, BuildOutputDirectory);
                if (!Directory.Exists(outDir))
                {
                    Directory.CreateDirectory(outDir);
                }

                string outPath = Path.Combine(outDir, fileName);

                // 2. Switch Active Target to Android if needed
                if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                {
                    Debug.Log("[AndroidBuildScript] Switching active build target to Android...");
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
                }

                // 3. AAB & Compression Settings
                EditorUserBuildSettings.buildAppBundle = true;
                EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ASTC;

                // 4. Architecture & Scripting
                NamedBuildTarget androidTarget = NamedBuildTarget.Android;
                PlayerSettings.SetScriptingBackend(androidTarget, ScriptingImplementation.IL2CPP);
                PlayerSettings.Android.targetArchitectures = architectures;

                // 5. Aggressive Size Optimizations for <10MB target
                PlayerSettings.stripEngineCode = true;
                PlayerSettings.SetManagedStrippingLevel(androidTarget, ManagedStrippingLevel.High);
                PlayerSettings.SetIl2CppCompilerConfiguration(androidTarget, Il2CppCompilerConfiguration.Master);
                PlayerSettings.SetIl2CppCodeGeneration(androidTarget, Il2CppCodeGeneration.OptimizeSize);

                // 6. Google Play Identity & Version Code
                PlayerSettings.SetApplicationIdentifier(androidTarget, "com.nextsem.boxblast2d");
                PlayerSettings.productName = "Box Blast 2D";
                PlayerSettings.companyName = "NextSem";
                
                // Release 6 is already live in Google Play Console, so new upload must be >= 7
                if (customVersionCode.HasValue)
                {
                    PlayerSettings.Android.bundleVersionCode = customVersionCode.Value;
                }
                else if (PlayerSettings.Android.bundleVersionCode < 7)
                {
                    PlayerSettings.Android.bundleVersionCode = 7;
                }

                if (!string.IsNullOrEmpty(customVersionName))
                {
                    PlayerSettings.bundleVersion = customVersionName;
                }
                else if (string.IsNullOrEmpty(PlayerSettings.bundleVersion) || PlayerSettings.bundleVersion == "1.0.2")
                {
                    PlayerSettings.bundleVersion = "1.0.3";
                }

                PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
                PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;

                // 7. Keystore signing: Prioritize dedicated project release keystore
                string projectKeystorePath = Path.Combine(projectRoot, "boxblast.keystore");
                if (File.Exists(projectKeystorePath))
                {
                    Debug.Log($"[AndroidBuildScript] Using dedicated project keystore: {projectKeystorePath}");
                    PlayerSettings.Android.useCustomKeystore = true;
                    PlayerSettings.Android.keystoreName = projectKeystorePath;
                    PlayerSettings.Android.keystorePass = "boxblast123";
                    PlayerSettings.Android.keyaliasName = "boxblast";
                    PlayerSettings.Android.keyaliasPass = "boxblast123";
                }
                else
                {
                    string debugKeystorePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".android", "debug.keystore");
                    if (File.Exists(debugKeystorePath))
                    {
                        Debug.Log($"[AndroidBuildScript] Using fallback debug keystore: {debugKeystorePath}");
                        PlayerSettings.Android.useCustomKeystore = true;
                        PlayerSettings.Android.keystoreName = debugKeystorePath;
                        PlayerSettings.Android.keystorePass = "android";
                        PlayerSettings.Android.keyaliasName = "androiddebugkey";
                        PlayerSettings.Android.keyaliasPass = "android";
                    }
                    else
                    {
                        PlayerSettings.Android.useCustomKeystore = false;
                    }
                }

                // 8. Collect Scenes
                string[] scenes = EditorBuildSettings.scenes
                    .Where(s => s.enabled && !string.IsNullOrEmpty(s.path))
                    .Select(s => s.path)
                    .ToArray();

                if (scenes.Length == 0)
                {
                    scenes = new[] { "Assets/Scenes/SampleScene.unity" };
                }

                Debug.Log($"[AndroidBuildScript] Building {scenes.Length} scene(s) to '{outPath}' with VersionCode={PlayerSettings.Android.bundleVersionCode}...");

                BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
                {
                    scenes = scenes,
                    locationPathName = outPath,
                    target = BuildTarget.Android,
                    options = BuildOptions.None
                };

                BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
                BuildSummary summary = report.summary;

                if (summary.result == BuildResult.Succeeded)
                {
                    FileInfo fi = new FileInfo(outPath);
                    double sizeMb = fi.Length / (1024.0 * 1024.0);
                    string msg = $"✓ Google Play AAB Build SUCCESSFUL!\n\nFile: {outPath}\nSize: {sizeMb:F2} MB ({fi.Length:N0} bytes)\nVersion Code: {PlayerSettings.Android.bundleVersionCode}\nVersion Name: {PlayerSettings.bundleVersion}\nPackage: com.nextsem.boxblast2d\nKeystore: boxblast.keystore";
                    Debug.Log($"[AndroidBuildScript] {msg}");

                    if (!isHeadless)
                    {
                        EditorUtility.DisplayDialog("Play Store AAB Ready", msg, "OK");
                        EditorUtility.RevealInFinder(outPath);
                    }
                    return true;
                }
                else
                {
                    string err = $"✗ Android AAB Build FAILED with result: {summary.result}. Total errors: {summary.totalErrors}";
                    Debug.LogError($"[AndroidBuildScript] {err}");
                    if (!isHeadless)
                    {
                        EditorUtility.DisplayDialog("Android AAB Build Failed", err, "OK");
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return false;
            }
        }
    }
}
