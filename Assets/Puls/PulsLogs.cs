using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PulsLogs : MonoBehaviour
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
}