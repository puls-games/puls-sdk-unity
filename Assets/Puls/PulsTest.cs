using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class PulsTest : MonoBehaviour
{
    [SerializeField] private TMP_Text _langText;
    [SerializeField] private TMP_Text _platformText;

    private void Start()
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        _langText.text = $"Lang: {PulsBridge.Language}";

        string currentDevice = "";

        if (PulsBridge.IsMobile)
            currentDevice = "mobile";
        else if (PulsBridge.IsDesktop)
            currentDevice = "desktop";
        
        _platformText.text = $"Device: {currentDevice}";
#endif
    }

    public void SyncWithCloud()
    {
        // 1. Load
        PulsBridge.LoadFromCloud(
            PulsBridge.LoadLocalData("lastSyncToken"),
            (cloudData, newSyncToken) => {
                // 2. Compare
                string localData = PulsBridge.LoadLocalData("gameState");
                
                if (ShouldUseCloudData(localData, cloudData))
                {
                    // 3. Use Cloud Data
                    Debug.Log("Use Cloud Data");
                    ApplyData(cloudData);
                    PulsBridge.SaveLocalData("gameState", cloudData);
                }
                else
                {
                    // 4. If Local data is newer then save it to cloud
                    Debug.Log("If Local data is newer then save it to cloud");
                    PulsBridge.SaveToCloud(
                        localData,
                        (syncToken) => {
                            PulsBridge.SaveLocalData("lastSyncToken", syncToken);
                        },
                        (error) => {
                            Debug.LogError($"Sync error: {error}");
                        }
                    );
                }
                
                // Update token
                PulsBridge.SaveLocalData("lastSyncToken", newSyncToken);
            },
            (error) => {
                Debug.LogError($"Loading error: {error}");
            }
        );
    }

    bool ShouldUseCloudData(string localData, string cloudData)
    {
        // Example for compare
        return string.IsNullOrEmpty(localData) || 
               cloudData.Length > localData.Length;
    }

    void ApplyData(string jsonData)
    {
        // Apply to game
    }
}