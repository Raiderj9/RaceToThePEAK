using HarmonyLib;
using PeakRace.Patch;
using PeakRace.src;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PeakRace.Core;

//TODO look at handkerchief rack, maybe on table next to tiki torch
//maybe really fancy would be a rotating Neckerchief display rack
public class TeamSelectorHandler
{
    public static GameObject TeamKiosk;
    public static GameObject[] TeamSelectors = new GameObject[12];
    public static Vector3[] TeamSelectorsPos;
    static AssetBundle kioskBundle;
    //public static Shader shader;

    public static void Initialize()
    {
        Debug.Log("[RaceToThePeak] TeamSelectorHandler was started");

        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private static void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        if (newScene.name == "Airport")
        {
            TeamHandler.findShader();

            Vector3[] TeamSelectorsPos = [
                new Vector3(-5.37f, 1.5f, 111.15f),
                new Vector3(-4.07f, 1.5f, 111.15f),
                new Vector3(-2.77f, 1.5f, 111.15f),
                new Vector3(-1.47f, 1.5f, 111.15f),
                new Vector3(-0.17f, 1.5f, 111.15f),
                new Vector3(1.27f,  1.5f, 111.15f),
                new Vector3(-5.37f, 1.5f, 112.45f),
                new Vector3(-4.07f, 1.5f, 112.45f),
                new Vector3(-2.77f, 1.5f, 112.45f),
                new Vector3(-1.47f, 1.5f, 112.45f),
                new Vector3(-0.17f, 1.5f, 112.45f),
                new Vector3(1.27f,  1.5f, 112.45f),
            ];

            Debug.Log("[RaceToThePeak] Team Selectors are being loaded");

            //Loads Asset Bundle
            Stream path = Assembly.GetExecutingAssembly().GetManifestResourceStream("PeakRace.Resources.teamkiosk");
            if (path == null)
            {
                Debug.Log($"[RaceToThePeak] Stream Path was null");
                return;
            }

            //Loads Kiosk bundle
            if (kioskBundle == null)
            {
                kioskBundle = AssetBundle.LoadFromStream(path);
            }
            if (kioskBundle == null)
            { 
                Debug.Log("[RaceToThePeak] Asset bundle was null"); 
                return;
            }

            //Searches for Game Shader
            if (Plugin.shader == null)
            {
                findShader();
            }

            //foreach (string asset in kioskBundle.GetAllAssetNames())
            //{
            //    Debug.Log($"[RaceToThePeak] found asset: {asset}");
            //}
            GameObject baseTeamSelector = kioskBundle.LoadAsset<GameObject>("assets/bundledassets/racetothepeak/prefabs/armbandcollision.prefab");

            //Builds Airport Team Kiosk
            TeamKiosk = new GameObject("TeamKiosk");

            Material baseColorMaterial = new Material(Plugin.shader);
            TeamSelector selector;
            // loop through all teams and build selector
            for (int i = 0; i< TeamSelectors.Length; i++)
            {
                TeamSelectors[i] = Object.Instantiate(baseTeamSelector, TeamSelectorsPos[i], new Quaternion());
                TeamSelectors[i].transform.SetParent(TeamKiosk.transform, worldPositionStays: false);
                TeamSelectors[i].transform.localScale = new Vector3(1f, .5f, 1f);
                TeamSelectors[i].GetComponent<Renderer>().material = TeamHandler.TroopMat[i];
                //TeamSelectors[i].GetComponent<Renderer>().material.shader = TeamHandler.CharShader;
                //TeamSelectors[i].GetComponent<Renderer>().material.color = Plugin.teamList[i].Item2;
                TeamSelectors[i].transform.localEulerAngles = new Vector3(270f, 270f, 0f);
                TeamSelectors[i].transform.localScale = new Vector3(1f, 1f, 1f);
                TeamSelectors[i].name = $"Team Selector {i}";
                //TeamSelectors[i].GetComponent<MeshRenderer>().
                selector = TeamSelectors[i].AddComponent<TeamSelector>();
                TeamSelectors[i].SetActive(true);
                selector.setTeam(i);
            }

            Debug.Log("[RaceToThePeak] Closed Stream Path");
            path.Dispose();
            kioskBundle.Unload(false); // Unloads the bundle but keeps assets in memory

        }
        else
        {
            TeamSelectors = new GameObject[12];
        }
    }
    static void findShader()
    {
        //Searches for Game Shader
        UnityEngine.Object[] shaderList = Resources.FindObjectsOfTypeAll(typeof(Shader));
        foreach (Shader shader in shaderList)
        {
            //Debug.Log($"Found Material: {shade.name} ID({shade.GetInstanceID()})");
            if (shader.name == "Universal Render Pipeline/Lit")
            {
                Plugin.shader = shader;
                Debug.Log("[RaceToThePeak] Successfully found Universal Render Pipeline/Lit shader");
            }
        }
    }
}
