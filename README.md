# Puls SDK Unity Plugin

This repository contains Puls SDK Unity plugin. This plugin allows Unity developers to easily integrate Puls events to build.

## Documentation

Documentation could be found at the [official website][https://hello.puls.games/docs/puls-events]

## Quick start

1. To use Puls SDK in your project download PulsSDK_vX.X.unitypackage

2. Open your project in the Unity editor

3. Select `Assets > Import Package > Custom Package` and find the `PulsSDK_vX.X.unitypackage` file.

4. Make sure all of the files are selected and click Import.

5. Ready!

## API

```CSharp
// Check platform
if (PulsBridge.IsMobile) {
    Debug.Log("Running on mobile device");
}

// Get user language
string userLanguage = PulsBridge.Language;

// Save data
PulsBridge.SaveData("playerName", "John Doe");

// Load data
string playerName = PulsBridge.LoadData("playerName");

// Remove data
PulsBridge.RemoveData("playerName");

// Clear all data
PulsBridge.ClearAllData();
```



