using MoreMountains.NiceVibrations;
using UnityEngine;

/// <summary>
/// HapticManager
/// - Wrapper tiện dụng quanh MMVibrationManager
/// - Hỗ trợ: Presets (Selection/Success/Warning/Failure/Light/Medium/Heavy), Transient, Continuous, Vibrate
/// - Có method để Stop continuous haptic
/// </summary>
public static class HapticManager
{
    // Friendly enum để dùng trong code (map tới HapticTypes của NiceVibrations khi cần)
    public enum Preset
    {
        None,
        LightImpact,
        MediumImpact,
        HeavyImpact,
        Selection,
        Success,
        Warning,
        Failure,
        VibrateDefault // generic vibrate (Android fallback / generic)
    }

    /// <summary>
    /// Play a preset haptic (Selection, Success, Warning, Failure, Light/Medium/Heavy)
    /// </summary>
    public static void PlayPreset(Preset preset)
    {
        switch (preset)
        {
            case Preset.LightImpact:
                MMVibrationManager.Haptic(HapticTypes.LightImpact);
                break;
            case Preset.MediumImpact:
                MMVibrationManager.Haptic(HapticTypes.MediumImpact);
                break;
            case Preset.HeavyImpact:
                MMVibrationManager.Haptic(HapticTypes.HeavyImpact);
                break;
            case Preset.Selection:
                MMVibrationManager.Haptic(HapticTypes.Selection);
                break;
            case Preset.Success:
                MMVibrationManager.Haptic(HapticTypes.Success);
                break;
            case Preset.Warning:
                MMVibrationManager.Haptic(HapticTypes.Warning);
                break;
            case Preset.Failure:
                MMVibrationManager.Haptic(HapticTypes.Failure);
                break;
            case Preset.VibrateDefault:
                // Simple vibrate fallback (medium)
                MMVibrationManager.Vibrate();
                break;
            case Preset.None:
            default:
                break;
        }
    }

    /// <summary>
    /// Transient haptic: short burst with intensity and sharpness (values 0..1)
    /// Example: PlayTransient(0.5f, 0.7f)
    /// </summary>
    public static void PlayTransient(float intensity = 0.5f, float sharpness = 0.5f)
    {
        // clamp just in case
        intensity = Mathf.Clamp01(intensity);
        sharpness = Mathf.Clamp01(sharpness);
        MMVibrationManager.TransientHaptic(intensity, sharpness);
    }

    /// <summary>
    /// Continuous haptic: duration in seconds. Provide a MonoBehaviour to let NiceVibrations control lifetime (pass 'this' from a MonoBehaviour).
    /// fallbackPreset: used if continuous is not supported on the device.
    /// </summary>
    public static void PlayContinuous(float intensity, float sharpness, float durationSeconds, HapticTypes fallback = HapticTypes.None, MonoBehaviour coroutineOwner = null)
    {
        intensity = Mathf.Clamp01(intensity);
        sharpness = Mathf.Clamp01(sharpness);

        // If no owner provided, we still call ContinuousHaptic with null (works but some features may require owner for control)
        MMVibrationManager.ContinuousHaptic(intensity, sharpness, durationSeconds, fallback, coroutineOwner);
    }

    /// <summary>
    /// Stop any ongoing continuous haptic.
    /// </summary>
    public static void StopContinuous()
    {
        MMVibrationManager.StopContinuousHaptic();
    }

    /// <summary>
    /// Convenience: play a short "tick" (light selection-like feedback)
    /// </summary>
    public static void Tick()
    {
        PlayPreset(Preset.Selection);
    }

    /// <summary>
    /// Convenience: play success feedback
    /// </summary>
    public static void Success()
    {
        PlayPreset(Preset.Success);
    }

    /// <summary>
    /// Convenience: play failure feedback
    /// </summary>
    public static void Failure()
    {
        PlayPreset(Preset.Failure);
    }
}


