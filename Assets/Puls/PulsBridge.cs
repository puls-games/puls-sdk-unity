using System;
using System.Runtime.InteropServices;
using UnityEngine;

public static class PulsBridge
{
    // Platform detection
    [DllImport("__Internal")]
    private static extern int Puls_IsMobile();
    
    [DllImport("__Internal")]
    private static extern int Puls_IsDesktop();
    
    [DllImport("__Internal")]
    private static extern string Puls_GetLanguage();
    
    // Storage methods
    [DllImport("__Internal")]
    private static extern void Puls_SaveToStorage(string key, string value);
    
    [DllImport("__Internal")]
    private static extern IntPtr Puls_LoadFromStorage(string key);
    
    [DllImport("__Internal")]
    private static extern void Puls_RemoveFromStorage(string key);
    
    [DllImport("__Internal")]
    private static extern void Puls_ClearStorage();

    // Методы Puls Storage
    [DllImport("__Internal")]
    private static extern void Puls_SaveToPulsStorage(string key, string value);
    
    [DllImport("__Internal")]
    private static extern int Puls_LoadFromPulsStorage(string key, Action<IntPtr> callback);
    
    [DllImport("__Internal")]
    private static extern void Puls_FreeMemory(IntPtr ptr);

    public static bool IsMobile
    {
        get
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            return Puls_IsMobile() == 1;
#else
            return Application.isMobilePlatform;
#endif
        }
    }

    public static bool IsDesktop
    {
        get
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            return Puls_IsDesktop() == 1;
#else
            return !Application.isMobilePlatform;
#endif
        }
    }

    public static string Language
    {
        get
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            //IntPtr ptr = Puls_GetLanguage();
            //string result = Marshal.PtrToStringAuto(ptr);
            //Puls_FreeMemory(ptr);
            return Puls_GetLanguage();
#else
            return Application.systemLanguage.ToString();
#endif
        }
    }

    // Local Storage
    public static void SaveData(string key, string value) {
#if UNITY_WEBGL && !UNITY_EDITOR
        Puls_SaveToStorage(key, value);
#else
        PlayerPrefs.SetString(key, value);
#endif
    }

    public static string LoadData(string key) {
#if UNITY_WEBGL && !UNITY_EDITOR
        IntPtr ptr = Puls_LoadFromStorage(key);
        if (ptr == IntPtr.Zero) return null;
        string result = Marshal.PtrToStringAuto(ptr);
        Puls_FreeMemory(ptr);
        return result;
#else
        return PlayerPrefs.GetString(key, null);
#endif
    }

    // Puls Storage
    public static void SaveToPulsStorage(string key, string value) {
#if UNITY_WEBGL && !UNITY_EDITOR
        Puls_SaveToPulsStorage(key, value);
#else
        Debug.LogWarning("PulsStorage not available in editor");
#endif
    }

    public static void LoadFromPulsStorage(string key, Action<string> callback) {
#if UNITY_WEBGL && !UNITY_EDITOR
        Puls_LoadFromPulsStorage(key, (ptr) => {
            string result = ptr != IntPtr.Zero ? Marshal.PtrToStringAuto(ptr) : null;
            if (ptr != IntPtr.Zero) Puls_FreeMemory(ptr);
            callback?.Invoke(result);
        });
#else
        callback?.Invoke(null);
        Debug.LogWarning("PulsStorage not available in editor");
#endif
    }

    public static void RemoveData(string key)
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        Puls_RemoveFromStorage(key);
#else
        PlayerPrefs.DeleteKey(key);
#endif
    }

    public static void ClearAllData()
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        Puls_ClearStorage();
#else
        PlayerPrefs.DeleteAll();
#endif
    }
}