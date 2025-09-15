using PeakRace.src;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace PeakRace.Patch;

[RequireComponent(typeof(PhotonView))]
internal class TeamHandler
{

    public static List<(string charName, int team)> charTeam;
    public static Material[] TroopMat = new Material[12];
    public static GameObject baseArmband;
    public static Material troopBaseMat;
    public static Shader CharShader;

    public static void Initialize()
    {
        Debug.Log("[RaceToThePeak] TeamHandler was started");

        charTeam = new List<(string charName, int team)>();

        Debug.Log($"[RaceToThePeak] Finished Loading Armband Assets");

    }

    public static void addCharacter(string character, int newTeam)
    {
        // first check to see if the character is already in the list
        if (character == "Bot")
        {
            return;
        }

        for (int i = 0; i < charTeam.Count; i++)
        {
            if (charTeam[i].charName == character)
            {
                charTeam[i] = (character, newTeam);
                Debug.Log($"[RaceToThePeak] changed {character} to team {newTeam}");
                return;
            }
        }

        charTeam.Add((character, newTeam));
        Debug.Log($"[RaceToThePeak] added {character} to team {newTeam}");
    }

    public static int getPlayerTeam(string name)
    {
        foreach ((string charName, int team) in charTeam)
        {
            if (charName == name)
            {
                Debug.Log($"[RaceToThePeak] Loading Player {charName} to team {team}");
                return team;
            }

        }
        Debug.Log($"[RaceToThePeak] {name} was not found");
        return 0;
    }

    static void setupArmbandPrefabs()
    {

        Debug.Log($"[RaceToThePeak] Loading Armband Resources");
        //Loads Asset Bundle
        Stream path = Assembly.GetExecutingAssembly().GetManifestResourceStream("PeakRace.Resources.armband");
        if (path == null)
        {
            Debug.Log($"[RaceToThePeak] Stream Path was null");
            return;
        }

        //Loads bundle
        AssetBundle assetBundle = AssetBundle.LoadFromStream(path);
        if (assetBundle == null)
        {
            Debug.Log("[RaceToThePeak] Asset bundle was null");
            return;
        }

        //foreach (string asset in assetBundle.GetAllAssetNames())
        //{
        //    Debug.Log($"[RaceToThePeak] found asset: {asset}");
        //}
        baseArmband = assetBundle.LoadAsset<GameObject>("assets/bundledassets/racetothepeak/prefabs/armband.prefab");
        troopBaseMat = assetBundle.LoadAsset<Material>("assets/bundledassets/racetothepeak/mats/basearmband.mat");
        configureShader(troopBaseMat);

        //TODO could probably do this cleaner with a loop and better planning of my Asset bundle...
        TroopMat[0] = assetBundle.LoadAsset<Material>("assets/bundledassets/racetothepeak/mats/0troopbing.mat");
        configureShader(TroopMat[0]);
        TroopMat[1] = assetBundle.LoadAsset<Material>("assets/bundledassets/racetothepeak/mats/1troopantlion.mat");
        configureShader(TroopMat[1]);
        TroopMat[2] = assetBundle.LoadAsset<Material>("assets/bundledassets/racetothepeak/mats/2troopscorpion.mat");
        configureShader(TroopMat[2]);
        TroopMat[3] = assetBundle.LoadAsset<Material>("assets/bundledassets/racetothepeak/mats/3troopcheetah.mat");
        configureShader(TroopMat[3]);
        TroopMat[4] = assetBundle.LoadAsset<Material>("assets/bundledassets/racetothepeak/mats/4trooplovebird.mat");
        configureShader(TroopMat[4]);
        TroopMat[5] = assetBundle.LoadAsset<Material>("assets/bundledassets/racetothepeak/mats/5troopnarwhal.mat");
        configureShader(TroopMat[5]);
        TroopMat[6] = assetBundle.LoadAsset<Material>("assets/bundledassets/racetothepeak/mats/7troopanteater.mat");
        configureShader(TroopMat[6]);
        TroopMat[7] = assetBundle.LoadAsset<Material>("assets/bundledassets/racetothepeak/mats/8troopcondor.mat");
        configureShader(TroopMat[7]);
        TroopMat[8] = assetBundle.LoadAsset<Material>("assets/bundledassets/racetothepeak/mats/9troopcrab.mat");
        configureShader(TroopMat[8]);
        TroopMat[9] = assetBundle.LoadAsset<Material>("assets/bundledassets/racetothepeak/mats/10troopsalamander.mat");
        configureShader(TroopMat[9]);
        TroopMat[10] = assetBundle.LoadAsset<Material>("assets/bundledassets/racetothepeak/mats/6troopcapybara.mat");
        configureShader(TroopMat[10]);
        TroopMat[11] = assetBundle.LoadAsset<Material>("assets/bundledassets/racetothepeak/mats/11troopmushroom.mat");
        configureShader(TroopMat[11]);

        path.Dispose();
        assetBundle.Unload(false);
    }

    static void configureShader(Material mat)
    {
        mat.shader = CharShader;
        mat.SetFloat("_UseTextureAlpha", 1);
    }

    public static void findShader()
    {
        if(CharShader != null)
        {
            return;
        }

        //Searches for Game Shader
        UnityEngine.Object[] shaderList = Resources.FindObjectsOfTypeAll(typeof(Shader));
        foreach (Shader shade in shaderList)
        {
            //Debug.Log($"Found Material: {shade.name} ID({shade.GetInstanceID()})");
            if (shade.name == "W/Character")
            {
                CharShader = shade;
                Debug.Log("[RaceToThePeak] Successfully found W/Character");

                setupArmbandPrefabs();
                return;
            }
        }

        Debug.Log("[RaceToThePeak] Could not find W/Character");
    }
}