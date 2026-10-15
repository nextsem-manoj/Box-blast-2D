using UnityEngine;

namespace BoxBlast
{
    /// <summary>
    /// Controls mobile haptic vibrations (light, medium, heavy) for touch feedback
    /// when placing blocks, blasting lines, triggering combos, and using boosters.
    /// Includes user preference toggle stored in PlayerPrefs.
    /// </summary>
    public static class HapticManager
    {
        private const string PrefsHaptics = "BoxBlast_HapticsEnabled";

        public static bool IsHapticsEnabled { get; private set; } = true;

        static HapticManager()
        {
            IsHapticsEnabled = PlayerPrefs.GetInt(PrefsHaptics, 1) == 1;
        }

        public static void ToggleHaptics()
        {
            IsHapticsEnabled = !IsHapticsEnabled;
            PlayerPrefs.SetInt(PrefsHaptics, IsHapticsEnabled ? 1 : 0);
            PlayerPrefs.Save();

            if (IsHapticsEnabled)
            {
                TriggerLight();
            }
        }

        public static void TriggerLight()
        {
            if (!IsHapticsEnabled || Application.isEditor) return;

#if UNITY_ANDROID && !UNITY_EDITOR
            VibrateAndroid(20, 90);
#elif UNITY_IOS || UNITY_ANDROID
            Handheld.Vibrate();
#endif
        }

        public static void TriggerMedium()
        {
            if (!IsHapticsEnabled || Application.isEditor) return;

#if UNITY_ANDROID && !UNITY_EDITOR
            VibrateAndroid(40, 170);
#elif UNITY_IOS || UNITY_ANDROID
            Handheld.Vibrate();
#endif
        }

        public static void TriggerHeavy()
        {
            if (!IsHapticsEnabled || Application.isEditor) return;

#if UNITY_ANDROID && !UNITY_EDITOR
            VibrateAndroid(85, 255);
#elif UNITY_IOS || UNITY_ANDROID
            Handheld.Vibrate();
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private static AndroidJavaObject s_Vibrator;
        private static bool s_VibratorChecked = false;

        private static void InitVibrator()
        {
            if (s_VibratorChecked) return;
            s_VibratorChecked = true;
            try
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    s_Vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                }
            }
            catch {}
        }

        private static void VibrateAndroid(long milliseconds, int amplitude)
        {
            InitVibrator();
            if (s_Vibrator != null)
            {
                try
                {
                    using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
                    {
                        int sdkInt = version.GetStatic<int>("SDK_INT");
                        if (sdkInt >= 26) // Android 8.0 Oreo+
                        {
                            using (var effectClass = new AndroidJavaClass("android.os.VibrationEffect"))
                            using (var effect = effectClass.CallStatic<AndroidJavaObject>("createOneShot", milliseconds, amplitude))
                            {
                                s_Vibrator.Call("vibrate", effect);
                                return;
                            }
                        }
                        else
                        {
                            s_Vibrator.Call("vibrate", milliseconds);
                            return;
                        }
                    }
                }
                catch {}
            }
            Handheld.Vibrate();
        }
#endif
    }
}
