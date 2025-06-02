// PlayerSettings.cs
using UnityEngine;

public static class PlayerSettings
{
    private const string SensKey = "MouseSensitivity";
    private const float DefaultSens = 1.0f;

    public static float MouseSensitivity
    {
        get => PlayerPrefs.GetFloat(SensKey, DefaultSens);
        set
        {
            PlayerPrefs.SetFloat(SensKey, Mathf.Clamp(value, 0.1f, 10f));
            PlayerPrefs.Save();
        }
    }
}
