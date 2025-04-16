using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class PulsExample : MonoBehaviour
{
    [SerializeField] private TMP_Text _langText;
    [SerializeField] private TMP_Text _platformText;
    [SerializeField] private TMP_Text _userID;
    [SerializeField] private TMP_Text _result;

    private void Start()
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        _langText.text = $"Lang: {PulsBridge.Language}";
        _userID.text = $"UserID: {PulsBridge.UserId}";

        string currentDevice = "";

        if (PulsBridge.IsMobile)
            currentDevice = "mobile";
        else if (PulsBridge.IsDesktop)
            currentDevice = "desktop";
        
        _platformText.text = $"Device: {currentDevice}";
        
        SyncWithCloud();
#endif
    }

    public void SaveGameDataToCloud()
    {
        // 1. Your game data
        var gameData = new
        {
            score = Random.Range(50, 1000),
            level = 5,
            inventory = new[] { "sword", "shield", "potion" },
            settings = new
            {
                musicVolume = 0.8f,
                effectsVolume = 0.6f
            }
        };

        Debug.Log($"ready data");
        
        // 2. Serialize to JSON
        string jsonData = JsonUtility.ToJson(gameData);

        Debug.Log($"ready json");
        
        // 3. Save to cloud
        PulsBridge.SaveToCloud(
            jsonData,
            // success
            (syncToken) =>
            {
                Debug.Log($"sync finished");
                
                Debug.Log($"Saved! SyncToken: {syncToken}");

                // Save syncToken to local storage
                PulsBridge.SaveLocalData("lastSyncToken", syncToken);
            },
            // Errors
            (error) =>
            {
                Debug.Log($"error");
                
                switch (error)
                {
                    case "NO_DATA":
                        Debug.LogError("NO_DATA");
                        break;
                    case "MAX_SIZE":
                        Debug.LogError("MAX_SIZE (1MB)");
                        break;
                    case "SESSION_EXPIRED":
                        Debug.LogError("SESSION_EXPIRED");
                        break;
                    default:
                        Debug.LogError($"Save error: {error}");
                        break;
                }
            }
        );
    }

    public void LoadGameDataFromCloud()
    {
        // 1. Get Last syncToken from local storage
        string lastSyncToken = PulsBridge.LoadLocalData("lastSyncToken");

        // 2. Load from cloud
        PulsBridge.LoadFromCloud(
            lastSyncToken,
            // success
            (data, newSyncToken) =>
            {
                if (string.IsNullOrEmpty(data))
                {
                    Debug.Log("Local data is newer");
                    return;
                }

                // Deserialize
                var gameData = JsonUtility.FromJson<GameData>(data);

                // Save new syncToken to local storage
                PulsBridge.SaveLocalData("lastSyncToken", newSyncToken);

                // Update game state
                ApplyData(gameData);
            },
            // Errors
            (error) =>
            {
                switch (error)
                {
                    case "NO_DATA":
                        Debug.Log("NO_DATA");
                        break;
                    case "INVALID_DATA":
                        Debug.LogError("INVALID_DATA");
                        break;
                    case "SESSION_EXPIRED":
                        Debug.LogError("SESSION_EXPIRED");
                        break;
                    default:
                        Debug.LogError($"Loading error: {error}");
                        break;
                }
            }
        );
    }

    public void SyncWithCloud()
    {
        // 1. Load
        PulsBridge.LoadFromCloud(
            PulsBridge.LoadLocalData("lastSyncToken"),
            (cloudData, newSyncToken) =>
            {
                // 2. Compare
                string localData = PulsBridge.LoadLocalData("gameState");

                if (ShouldUseCloudData(localData, cloudData))
                {
                    // 3. Use Cloud Data
                    Debug.Log("Use Cloud Data");
                    // Deserialize
                    var gameData = JsonUtility.FromJson<GameData>(cloudData);
                    ApplyData(gameData);
                    PulsBridge.SaveLocalData("gameState", cloudData);
                }
                else
                {
                    // 4. If Local data is newer then save it to cloud
                    Debug.Log("If Local data is newer then save it to cloud");
                    PulsBridge.SaveToCloud(
                        localData,
                        (syncToken) => { PulsBridge.SaveLocalData("lastSyncToken", syncToken); },
                        (error) => { Debug.LogError($"Sync error: {error}"); }
                    );
                }

                // Update token
                PulsBridge.SaveLocalData("lastSyncToken", newSyncToken);
            },
            (error) => { Debug.LogError($"Loading error: {error}"); }
        );
    }

    private bool ShouldUseCloudData(string localData, string cloudData)
    {
        // Example for compare
        return string.IsNullOrEmpty(localData) ||
               cloudData.Length > localData.Length;
    }

    private void ApplyData(GameData gameData)
    {
        // Apply to game
        
        Debug.Log($"loaded data: {gameData.score}");
        _result.text = $"loaded data: {gameData.score}";
    }

    [System.Serializable]
    private class GameData
    {
        public int score;
        public int level;
        public string[] inventory;
        public SettingsData settings;
    }

    [System.Serializable]
    private class SettingsData
    {
        public float musicVolume;
        public float effectsVolume;
    }
}