#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class ForceReloadDomain
{
    static ForceReloadDomain()
    {
        // 1. Force Domain Reload to be ENABLED so code changes always apply in Play Mode
        if (EditorSettings.enterPlayModeOptionsEnabled)
        {
            EditorSettings.enterPlayModeOptionsEnabled = false;
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.None;
            Debug.Log("[BoxBlast] EnterPlayModeOptions disabled! Full Domain Reload is now active.");
        }
    }

    [MenuItem("Box Blast/Force Recompile and Reload Scripts %#r")]
    public static void ForceReload()
    {
        EditorSettings.enterPlayModeOptionsEnabled = false;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.None;
        EditorUtility.RequestScriptReload();
        Debug.Log("[BoxBlast] Requested Script Reload!");
    }
}
#endif
