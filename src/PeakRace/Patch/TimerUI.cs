using BepInEx;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using pworld.Scripts.Extensions;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PeakRace.Patch;

public class TimerUI : MonoBehaviour
{
    private int leaderboardPosX = 23;
    private int leaderboardPosY = -60;
    private int troopPosX = 270;
    private int troopPosY = 10;
    private int clockPosX = 20;
    private int clockPosY = 5;
    private Canvas canvas;
    private TextMeshProUGUI leaderboard;
    private TextMeshProUGUI troop;
    private TextMeshProUGUI clock;
    static int shadowMaterialID;

    void Awake()
    {
        GameObject timerUI = this.gameObject;
        canvas = timerUI.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scalar = canvas.gameObject.AddComponent<CanvasScaler>();
        scalar.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scalar.referenceResolution = new Vector2(1920f, 1080f);

        //Finds the shadow material
        UnityEngine.Object[] materialList = Resources.FindObjectsOfTypeAll(typeof(Material));
        foreach (var material in materialList)
        {
            //Debug.Log($"Found Material: {material.name} ID({material.GetInstanceID()})");
            if (material.name == "DarumaDropOne-Regular SDF Shadow")
            {
                shadowMaterialID = material.GetInstanceID();
                Debug.Log("[RaceToThePeak] Successfully found Daruma Shadow Material");
            }
        }

        //Finds the TMP_Font
        if (Plugin.FontAsset == null)
        {
            UnityEngine.Object[] fontList = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            foreach (TMP_FontAsset font in fontList)
            {
                //Debug.Log($"[RaceToThePeak] Found Font: {font.name}");
                if (font.name.Equals("DarumaDropOne-Regular SDF"))
                {
                    Plugin.FontAsset = font;
                    Debug.Log("[RaceToThePeak] Successfully found Daruma Font");
                }
            }
        }

        InitializeUI();
    }

    void InitializeUI()
    {
        //Configures leaderboard
        GameObject leaderboardUI = new GameObject("LeaderboardUI");
        leaderboardUI.transform.SetParent(canvas.transform, worldPositionStays: false);
        leaderboard = leaderboardUI.AddComponent<TextMeshProUGUI>();
        Vector3 leaderboardPos = default(Vector3);
        leaderboardPos.x = leaderboardPosX;
        leaderboardPos.y = leaderboardPosY;
        leaderboard.text = "";
        SetupText(leaderboard, leaderboardPos);
        leaderboard.font = Plugin.FontAsset;
        leaderboard.fontMaterial = (Material)Resources.InstanceIDToObject(shadowMaterialID);
        leaderboard.color = Plugin.Color;
        leaderboard.fontSize = 18;

        //Configures Troop
        GameObject troopUI = new GameObject("TroopUI");
        troopUI.transform.SetParent(canvas.transform, worldPositionStays: false);
        troop = troopUI.AddComponent<TextMeshProUGUI>();
        Vector3 troopPos = default(Vector3);
        troopPos.x = troopPosX;
        troopPos.y = troopPosY;
        troop.text = "Troop Null";
        troop.color = Plugin.Color;
        SetupText(troop, troopPos);
        troop.font = Plugin.FontAsset;
        troop.fontMaterial = (Material)Resources.InstanceIDToObject(shadowMaterialID);
        troop.fontSize = 50;

        //Configures Timer
        GameObject clockUI = new GameObject("ClockUI");
        clockUI.transform.SetParent(canvas.transform, worldPositionStays: false);
        clock = clockUI.AddComponent<TextMeshProUGUI>();
        Vector3 clockPos = default(Vector3);
        clockPos.x = clockPosX;
        clockPos.y = clockPosY;
        clock.text = "00:00:00";
        SetupText(clock, clockPos);
        clock.font = Plugin.FontAsset;
        clock.fontMaterial = (Material)Resources.InstanceIDToObject(shadowMaterialID);
        clock.color = Plugin.Color;
        clock.fontSize = 46;

        Debug.Log("[RaceToThePeak] GUI Initialized");
    }

    public static void SetupText(TextMeshProUGUI text, Vector3 anchoredPos)
    {
        RectTransform rectTransform = text.rectTransform;
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.position += anchoredPos;
        rectTransform.sizeDelta = new Vector2(500f, 500f);
        text.alignment = TextAlignmentOptions.TopLeft;
    }

    private void LateUpdate()
    {
        if (Character.localCharacter == null)
        {
            Debug.Log($"[RaceToThePeak] the Local Character has not been loaded yet");
            return;
        }

        CharacterTeamInfo TeamInfo = Character.localCharacter.GetComponent<CharacterTeamInfo>();
        if (TeamInfo == null)
        {
            Debug.Log($"[RaceToThePeak] the {Character.localCharacter.name} CharacterTeamHandler couldnt be found");
            return;
        }

        troop.text = Plugin.teamList[TeamInfo.teamInt].Item1;
        troop.color = Plugin.teamList[TeamInfo.teamInt].Item2;
    
        // Disable/Enable Teams
        if (leaderboard.enabled != TeamInfo.teamOn)
        { 
            leaderboard.enabled = TeamInfo.teamOn;
            troop.enabled = TeamInfo.teamOn;
            clock.enabled = TeamInfo.teamOn;
        }

        clock.text = TeamInfo.timeString;

        String scene = SceneManager.GetActiveScene().name;
        if (scene != "Airport" && TeamInfo.teamGUIOn)
        {
            updateScoreboard();
        }
        
    }

    private void updateScoreboard()
    {
        CharacterTeamInfo yourTeamInfo = Character.localCharacter.GetComponent<CharacterTeamInfo>();
        int yourPlace = 0;

        //Initializes 
        Dictionary<int, List<CharacterTeamInfo>> teamTracker = new Dictionary<int, List<CharacterTeamInfo>>();
        foreach (Character allChar in Character.AllCharacters)
        {
            CharacterTeamInfo TeamInfo = allChar.GetComponent<CharacterTeamInfo>();
            if (teamTracker.ContainsKey(TeamInfo.teamInt))
            {
                teamTracker[TeamInfo.teamInt].Add(TeamInfo);
            }
            else
            {
                teamTracker.Add(TeamInfo.teamInt, new List<CharacterTeamInfo>());
                teamTracker[TeamInfo.teamInt].Add(TeamInfo);
            }
        }

        //Debug.Log($"teamTracker Counter:{teamTracker.Count}");

        //Loop through teamTracker to calculate total team scores
        List<(int team, float time)> teamTimer = new List<(int, float)>();
        foreach (KeyValuePair <int, List<CharacterTeamInfo>> team in teamTracker)
        {
            float timer = 0;
            foreach (CharacterTeamInfo teamHandler in team.Value)
            {
                timer += teamHandler.time;
            }
            //Debug.Log($"team:{team.Key}\ntime:{timer / team.Value.Count}");
            teamTimer.Add((team.Key, timer / team.Value.Count));
        }
        //Add debug teams here
        //teamTimer.Add((0, 10));
        //teamTimer.Add((1, 15));
        //teamTimer.Add((2, 20));
        //teamTimer.Add((3, 30));
        //teamTimer.Add((4, 45));
        //teamTimer.Add((5, 60));

        // Determines teams placement
        (int team, float time)[] trophyPlaces = [(-1, 0), (-1, 0), (-1, 0), (-1, 0), (-1, 0), (-1, 0),
                                                 (-1, 0), (-1, 0), (-1, 0), (-1, 0), (-1, 0), (-1, 0)];
        int idx = 0;
        while(teamTimer.Count>0)
        {
            //should rewrite to do just a straight insertion sort algorithm
            int nextPlace = findNextPole(teamTimer); 
            
            trophyPlaces[idx].team = teamTimer[nextPlace].team;
            trophyPlaces[idx].time = teamTimer[nextPlace].time;
            if (yourTeamInfo.teamInt == teamTimer[nextPlace].team)
            {
                yourPlace = idx;
            }
            teamTimer.RemoveAt(nextPlace);
            idx++;
        }

        // If player is top 5, writes top five score
        string[] teams = ["","","","",""];
        if(yourPlace<5)
        {
            teams[0] = "1st " + timeToString(trophyPlaces[0].time);
            if (trophyPlaces[1].time > 0)
                { teams[1] = "\n2nd " + timeToString(trophyPlaces[1].time); }
            if (trophyPlaces[2].time > 0)
                { teams[2] = "\n3rd " + timeToString(trophyPlaces[2].time); }
            if (trophyPlaces[3].time > 0)
                { teams[3] = "\n4th " + timeToString(trophyPlaces[3].time); }
            if (trophyPlaces[4].time > 0)
                { teams[4] = "\n5th " + timeToString(trophyPlaces[4].time); }

            teams[yourPlace] += " (You)";
            //Here is where we would calculate team color on leaderboard
            leaderboard.text = teams[0]+teams[1]+teams[2]+teams[3]+teams[4];
        }
        // Otherwise break after 3 and write player position
        else
        {
            teams[0] = "1st " + timeToString(trophyPlaces[0].time);
            teams[1] = "\n2nd " + timeToString(trophyPlaces[1].time);
            teams[2] = "\n3rd " + timeToString(trophyPlaces[2].time);
            teams[3] = "\n- - - - - - - - - -";
            teams[4] = "\n"+ (yourPlace+1) + "th " + timeToString(trophyPlaces[yourPlace].time) + " (You)";

            //Here is where we would calculate team color on leaderboard
            leaderboard.text = teams[0] + teams[1] + teams[2] + teams[3] + teams[4];
        }

    }

    // Calculates the next pole position
    private int findNextPole(List<(int team, float time)> teamTimer)
    {
        int scoreIdx = 0;
        float lowestScore = teamTimer[0].time;
        for (int i = 1; i < teamTimer.Count; i++)
        {
            if(teamTimer[i].time< lowestScore)
            {
                scoreIdx = i;
                lowestScore = teamTimer[i].time;
            }
        }
        return scoreIdx;
    }

    // Converts float to timer string
    private string timeToString(float time)
    {
        int hourTime = (int)(time / 3600);
        int minTime = (int)(time % 3600 / 60);
        int secTime = (int)(time % 60);

        return $"{needZero(hourTime)}:{needZero(minTime)}:{needZero(secTime)}";
    }

    // Determines whether string needs extra 0 for timer format
    private string needZero(int time)
    {
        if (time / 10 < 1)
        {
            return $"0{time}";
        }
        return $"{time}";
    }

}
