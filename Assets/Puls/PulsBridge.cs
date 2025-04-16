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
    
    [DllImport("__Internal")]
    private static extern string Puls_GetUserId();

    // Local Storage (user-specific)
    [DllImport("__Internal")]
    private static extern void Puls_SaveToLocalStorage(string userId, string key, string value);
    
    [DllImport("__Internal")]
    private static extern IntPtr Puls_LoadFromLocalStorage(string userId, string key);
    
    [DllImport("__Internal")]
    private static extern void Puls_RemoveFromLocalStorage(string userId, string key);
    
    [DllImport("__Internal")]
    private static extern void Puls_ClearUserLocalStorage(string userId);

    // Puls Cloud Storage
    [DllImport("__Internal")]
    private static extern void Puls_SaveToCloud(string data, Action<string> successCallback, Action<string> errorCallback);
    
    [DllImport("__Internal")]
    private static extern void Puls_LoadFromCloud(string syncToken, Action<string, string> successCallback, Action<string> errorCallback);
    
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
            return Puls_GetLanguage();
#else
            return Application.systemLanguage.ToString();
#endif
        }
    }

    public static string UserId
    {
        get
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            return Puls_GetUserId();
#else
            return "editor-user"; 
#endif
        }
    }

    // Local Storage 
    public static void SaveLocalData(string key, string value)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Puls_SaveToLocalStorage(UserId, key, value);
#else
        PlayerPrefs.SetString($"{UserId}_{key}", value);
#endif
    }

    public static string LoadLocalData(string key)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        IntPtr ptr = Puls_LoadFromLocalStorage(UserId, key);
        if (ptr == IntPtr.Zero) return null;
        string result = Marshal.PtrToStringAuto(ptr);
        Puls_FreeMemory(ptr);
        return result;
#else
        return PlayerPrefs.GetString($"{UserId}_{key}", null);
#endif
    }

    public static void RemoveLocalData(string key)
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        Puls_RemoveFromLocalStorage(UserId, key);
#else
        PlayerPrefs.DeleteKey($"{UserId}_{key}");
#endif
    }

    public static void ClearUserData()
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        Puls_ClearUserLocalStorage(UserId);
#else
        PlayerPrefs.DeleteAll();
#endif
    }

    // Puls Cloud Storage
    public static void SaveToCloud(string data, Action<string> onSuccess, Action<string> onError)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        CallbackHelper.onSaveSuccess = onSuccess;
        CallbackHelper.onSaveError = onError;
        
        Puls_SaveToCloud(data, CallbackHelper.OnSaveSuccess, CallbackHelper.OnSaveError);
#else
        Debug.LogWarning("PulsStorage not available in editor");
        onError?.Invoke("UNKNOWN");
#endif
    }

    public static void LoadFromCloud(string syncToken, Action<string, string> onSuccess, Action<string> onError)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        CallbackHelper.onLoadSuccess = onSuccess;
        CallbackHelper.onLoadError = onError;
        
        Puls_LoadFromCloud(syncToken, CallbackHelper.OnLoadSuccess, CallbackHelper.OnLoadError);
#else
        Debug.LogWarning("PulsStorage not available in editor");
        onError?.Invoke("UNKNOWN");
#endif
    }
    
    private static class CallbackHelper
    {
        public static Action<string> onSaveSuccess;
        public static Action<string> onSaveError;
        public static Action<string, string> onLoadSuccess;
        public static Action<string> onLoadError;

        [AOT.MonoPInvokeCallback(typeof(Action<string>))]
        public static void OnSaveSuccess(string syncToken)
        {
            onSaveSuccess?.Invoke(syncToken);
        }

        [AOT.MonoPInvokeCallback(typeof(Action<string>))]
        public static void OnSaveError(string error)
        {
            onSaveError?.Invoke(error);
        }

        [AOT.MonoPInvokeCallback(typeof(Action<string, string>))]
        public static void OnLoadSuccess(string data, string syncToken)
        {
            onLoadSuccess?.Invoke(data, syncToken);
        }

        [AOT.MonoPInvokeCallback(typeof(Action<string>))]
        public static void OnLoadError(string error)
        {
            onLoadError?.Invoke(error);
        }
    }
}

