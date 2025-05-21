# Puls SDK Unity Plugin

This repository contains Puls SDK Unity plugin. This plugin allows Unity developers to easily integrate Puls events to build.

## Documentation

Documentation could be found at the [official website][https://hello.puls.games/docs/puls-events]

## Quick start

1. To use Puls SDK in your project download PulsBridgeSDK.X.X.X.unitypackage

2. Open your project in the Unity editor

3. Select `Assets > Import Package > Custom Package` and find the `PulsBridgeSDK.X.X.X.unitypackage` file.

4. Make sure all of the files are selected and click Import.

5. Ready!

## Saves

At the first boot, you need to initialize the save system using the PulsPlayerPrefs.Initialize(() => {}) method. After the callback, the save system will load the latest saves and they can be used. 
The saving system in Puls completely copies the standard PlayerPrefs methods in Unity, with usage examples listed below in the API.
IMPORTANT: do not set the refresh interval in the initialization method to less than 10 seconds.

## API

```CSharp
// Check platform
if (PulsBridge.IsMobile) {
    Debug.Log("Running on mobile device");
}

// Get user language
string userLanguage = PulsBridge.Language;

// Get user ID
string userId = PulsBridge.UserId;

// Storage
PulsPlayerPrefs.SetString(string key, string value)
PulsPlayerPrefs.GetString(string key, string defaultValue = null)

PulsPlayerPrefs.SetInt(string key, int value)
PulsPlayerPrefs.GetInt(string key, int defaultValue = 0)

PulsPlayerPrefs.SetFloat(string key, float value)
PulsPlayerPrefs.GetFloat(string key, float defaultValue = 0f)

PulsPlayerPrefs.HasKey(string key)

PulsPlayerPrefs.DeleteKey(string key)
PulsPlayerPrefs.DeleteAll()

// Storage Example
public static void Init(Action onFinish)
{
    PulsPlayerPrefs.Initialize(() =>
   {
    var jsonData = PulsPlayerPrefs.GetString(PrefsKey);
    loadedData = JsonUtility.FromJson<GameData>(jsonData);

    onFinish?.Invoke();
   });
}

public void Save()
{
    var jsonData = JsonUtility.ToJson(GameData);
    PulsPlayerPrefs.SetString(PrefsKey, jsonData);
#if UNITY_EDITOR
    PlayerPrefs.Save();
#endif
}
```



