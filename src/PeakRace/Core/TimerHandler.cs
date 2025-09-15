using PeakRace.Patch;
using PeakRace.src;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PeakRace.Core;

internal class TimerHandler
{
    private static GameObject timerUI;

    public static void Initialize()
    {
        Debug.Log("[RaceToThePeak] TimerHandler was started");
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Airport" || scene.name.ToLower().StartsWith("level_"))
        {
            Debug.Log("[RaceToThePeak] Starting Timer");

            timerUI = new GameObject("TimerUI");
            RectTransform rectTransform = timerUI.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.pivot = new Vector2(0f, 1f);

            timerUI.AddComponent<TimerUI>();

        }
        else
        {
            timerUI = new GameObject();
        }
    }
}
