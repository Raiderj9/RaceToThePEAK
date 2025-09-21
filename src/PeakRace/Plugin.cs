using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using PeakRace.Core;
using PeakRace.Patch;
using Steamworks;
using System.Collections.Generic;
using TMPro;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.Localization.SmartFormat.Core.Output;

namespace PeakRace;

[BepInAutoPlugin]
public partial class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource Log { get; private set; } = null!;
    private readonly Harmony harmony = new(Name);
    public static List<(string, Color)> teamList;
    public static TMP_FontAsset FontAsset; //DarumaDropOne-Regular SDF
    //public static int shadowMaterialID; //DarumaDropOne-Regular SDF Shadow (Instance)
    public static Shader shader;
    public static Color Color = new Color(0.8745f, 0.8549f, 0.7608f, 1f); //Standard Color

    private void Awake()
    {
        Log = base.Logger;
        Log.LogInfo("Plugin RaceToThePeak Loaded");

        // Sets up team Names and colors
        teamList = new List<(string, Color)>
        {
            ("Troop BingBong"   ,new Color(0.5f, 1f, 0.42f, 1f)),
            ("Troop Antlion"    ,new Color(1f, 0.6f, 0.34f, 1f)),
            ("Troop Scorpion"   ,new Color(0.56f, 0.25f, 1f, 1f)),
            ("Troop Cheetah"    ,new Color(1f, 1f, 0f, 1f)),
            ("Troop Lovebird"   ,new Color(1f, 0.5f, 1f, 1f)),
            ("Troop Narwhal"    ,new Color(.25f, .5f, 1f, 1f)),
            ("Troop AntEater"   ,new Color(0.3f, 0.82f, 0.28f, 1f)),
            ("Troop Condor"     ,new Color(0.36f, 1f, 1f, 1)),
            ("Troop Crab"       ,new Color(1f, 0.24f, 0.28f, 1f)),
            ("Troop Salamander" ,new Color(0.9f, 0.29f, 1f, 1f)),
            ("Troop Capybara"   ,new Color(0.64f, 0.44f, 0.25f, 1f)),
            ("Troop Mushroom"   ,new Color(0.6f, 0.57f, 0.79f, 1f))
        };

        //Initializing Team Handler
        TeamHandler.Initialize();

        //Initializing Timer Handler
        TimerHandler.Initialize();

        //Initializing Team Selector Handler
        TeamSelectorHandler.Initialize();

        //Character Team Handler
        harmony.PatchAll(typeof(CharacterTeamInfo));
        Log.LogInfo("Character Team Handler Successful");

        //Map Patches
        harmony.PatchAll(typeof(MapPatch));
        Log.LogInfo("Map Patches Successful");

        //ArmBand
        harmony.PatchAll(typeof(Armband));
        Log.LogInfo("Armband Successful");


        Log.LogInfo($"Plugin {Name} is loaded!");
    }
}