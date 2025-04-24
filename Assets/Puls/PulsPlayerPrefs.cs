
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public static class PulsPlayerPrefs
{
    private const string SyncTokenKey = "Puls.SyncToken";
    private const string DataKey = "Puls.Data";

    [Serializable]
    private class PrefsKVP
    {
        public string Key;
        public string Value;
    }

    [Serializable]
    private class PrefsData
    {
        public List<PrefsKVP> PrefsKVPs = new List<PrefsKVP>();
        public long Saves;

        public void SaveValue(string key, string value)
        {
            var kvp = GetKVP(key, out var _);
            if (kvp == null)
            {
                kvp = new PrefsKVP()
                {
                    Key = key
                };
                PrefsKVPs.Add(kvp);
            }
            kvp.Value = value;
        }

        public string LoadValue(string key)
        {
            var kvp = GetKVP(key, out var _);
            if (kvp == null)
            {
                return null;
            }
            return kvp.Value;
        }

        public bool HasKey(string key)
        {
            var kvp = GetKVP(key, out var _);
            return kvp != null;
        }

        public void DeleteKey(string key)
        {
            GetKVP(key, out var index);
            if (index >= 0)
            {
                PrefsKVPs.RemoveAt(index);
            }
        }

        public void DeleteAll()
        {
            PrefsKVPs.Clear();
        }

        private PrefsKVP GetKVP(string key, out int index)
        {
            index = -1;

            var count = PrefsKVPs.Count;
            for (var i = 0; i < count; i++)
            {
                var kvp = PrefsKVPs[i];
                if (kvp.Key == key)
                {
                    index = i;
                    return kvp;
                }
            }

            return null;
        }
    }

    private static PrefsData data = new PrefsData();
    private static long lastSyncedSaves;

    public static void Initialize(Action onFinish, Action<string> onError = null, int refreshInterval = 5)
    {
        RunSyncLoop(refreshInterval);

        var syncToken = GetSyncToken();
        if (string.IsNullOrEmpty(syncToken))
        {
            syncToken = null;
        }

        PulsBridge.LoadFromCloud(syncToken,
            (cloudJsonData, newSyncToken) =>
            {
                var localJsonData = PulsBridge.LoadLocalData(DataKey);

                var localData = GetData(localJsonData);
                var cloudData = GetData(cloudJsonData);

                data = GetNewerData(localData, cloudData);
                if (data == null)
                {
                    data = new PrefsData();
                }

                SetSyncToken(newSyncToken);
                onFinish?.Invoke();
            },

            (error) =>
            {
                Debug.LogError($"[PulsPlayerPrefs] Load from cloud error: {error}");

                onError?.Invoke(error);

                var jsonData = PulsBridge.LoadLocalData(DataKey);
                data = GetData(jsonData);
                if (data == null)
                {
                    data = new PrefsData();
                }
                onFinish?.Invoke();
            });
    }

    public static void SetString(string key, string value)
    {
        SaveValue(key, value);
    }

    public static string GetString(string key, string defaultValue = null)
    {
        var value = LoadValue(key);
        if (value == null)
        {
            if (defaultValue != null)
            {
                value = defaultValue;
            }
            else
            {
                value = string.Empty;
            }
        }
        return value;
    }

    public static void SetInt(string key, int value)
    {
        SaveValue(key, value.ToString());
    }

    public static int GetInt(string key, int defaultValue = 0)
    {
        var stringValue = LoadValue(key);
        if (stringValue == null)
        {
            return defaultValue;
        }
        if (int.TryParse(stringValue, out var result))
        {
            return result;
        }
        return defaultValue;
    }

    public static void SetFloat(string key, float value)
    {
        SaveValue(key, value.ToString("R", CultureInfo.InvariantCulture));
    }

    public static float GetFloat(string key, float defaultValue = 0f)
    {
        var stringValue = LoadValue(key);
        if (stringValue == null)
        {
            return defaultValue;
        }
        if (float.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }
        return defaultValue;
    }

    public static bool HasKey(string key)
    {
        return data.HasKey(key);
    }

    public static void DeleteKey(string key)
    {
        data.DeleteKey(key);
    }

    public static void DeleteAll()
    {
        data.DeleteAll();
    }

    private static void SaveValue(string key, string value)
    {
        data.SaveValue(key, value);
        SaveToLocalStorage();
    }

    private static string LoadValue(string key)
    {
        return data.LoadValue(key);
    }

    private static void SetSyncToken(string syncToken)
    {
        PulsBridge.SaveLocalData(SyncTokenKey, syncToken);
    }

    private static string GetSyncToken()
    {
        return PulsBridge.LoadLocalData(SyncTokenKey);
    }

    private static PrefsData GetData(string jsonData)
    {
        if (string.IsNullOrEmpty(jsonData))
        {
            return null;
        }

        return JsonUtility.FromJson<PrefsData>(jsonData);
    }

    private static PrefsData GetNewerData(PrefsData data1, PrefsData data2)
    {
        if (data1 == null)
        {
            return data2;
        }

        if (data2 == null)
        {
            return data1;
        }

        return data1.Saves > data2.Saves ? data1 : data2;
    }

    private static void SaveToLocalStorage()
    {
        data.Saves++;
        var jsonData = JsonUtility.ToJson(data);
        PulsBridge.SaveLocalData(DataKey, jsonData);
    }

    private static void RunSyncLoop(int seconds)
    {
        var gameObject = new GameObject("[PulsCoroutineHolder]");
        UnityEngine.Object.DontDestroyOnLoad(gameObject);
        var pulsCoroutineHolder = gameObject.AddComponent<PulsCoroutineHolder>();
        pulsCoroutineHolder.StartCoroutine(SyncLoop(seconds));
    }

    private static IEnumerator SyncLoop(float interval)
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(interval);
            Sync();
        }
    }

    private static void Sync()
    {
        if (data.Saves <= lastSyncedSaves)
        {
            return;
        }

        var dataSaves = data.Saves;

        string jsonData = JsonUtility.ToJson(data);
        PulsBridge.SaveToCloud(jsonData,
            (syncToken) =>
            {
                lastSyncedSaves = dataSaves;
                SetSyncToken(syncToken);
            },

            (error) =>
            {
                Debug.LogError($"[PulsPlayerPrefs] Save to cloud error: {error}");
            });
    }
}
