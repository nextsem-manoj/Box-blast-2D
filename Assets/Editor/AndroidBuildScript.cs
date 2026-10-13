using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace BoxBlast.Editor
{
    public static class AndroidBuildScript
    {
        private const string BuildOutputDirectory = "Builds/Android";

        [MenuItem("Build/Build Android AAB (<10MB Optimized - ARM64)", false, 10)]
        public static void BuildOptimizedAAB()
        {
            PerformBuild(AndroidArchitecture.ARM64, "BoxBlast2D_v2_ARM64.aab");
        }

        [MenuItem("Build/Build Android AAB (Universal ARM64 + ARMv7)", false, 11)]
        public static void BuildUniversalAAB()
        {
            PerformBuild(AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7, "BoxBlast2D_v2_Universal.aab");
        }

        public static void BuildCommandLine()
        {
            Debug.Log("[AndroidBuildScript] Starting CLI Android AAB build...");
            PerformBuild(AndroidArchitecture.ARM64, "BoxBlast2D_v2_ARM64.aab", isHeadless: true);
        }

        public static bool PerformBuild(AndroidArchitecture architectures, string fileName, bool isHeadless = false)
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

                // 6. Google Play Identity & Version Code Increment
                PlayerSettings.SetApplicationIdentifier(androidTarget, "com.nextsem.boxblast2d");
                PlayerSettings.productName = "Box Blast 2D";
                PlayerSettings.companyName = "NextSem";
                
                // Increment versionCode so Play Console accepts the update
                int currentVersionCode = PlayerSettings.Android.bundleVersionCode;
                if (currentVersionCode < 2)
                {
                    PlayerSettings.Android.bundleVersionCode = 2;
                }
                PlayerSettings.bundleVersion = "1.1";

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
                    string msg = $" Google Play AAB Build SUCCESSFUL!\n\nFile: {outPath}\nSize: {sizeMb:F2} MB ({fi.Length:N0} bytes)\nVersion Code: {PlayerSettings.Android.bundleVersionCode}\nVersion Name: {PlayerSettings.bundleVersion}\nPackage: com.nextsem.boxblast2d\nKeystore: boxblast.keystore";
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
                    string err = $" Android AAB Build FAILED with result: {summary.result}. Total errors: {summary.totalErrors}";
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
