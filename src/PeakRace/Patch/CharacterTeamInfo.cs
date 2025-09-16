using HarmonyLib;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.TextCore.Text;

namespace PeakRace.Patch;

[HarmonyPatch]
internal class CharacterTeamInfo : MonoBehaviourPunCallbacks
{
    public bool teamOn;
    public bool teamGUIOn;
    public bool timeOn;
    public int teamInt;
    public float time;
    public string timeString;
    public float checkpointRadius;
    private GameObject GameMap;
    private GameObject EndFlag;
    private List<Campfire> campfireList;
    public Character myChar;

    [HarmonyPatch(typeof(Character), nameof(Character.Awake))]
    [HarmonyPostfix]
    static void PosPatch(Character __instance)
    {
        __instance.gameObject.AddComponent<CharacterTeamInfo>();
        Debug.Log($"[RaceToThePeak] TeamInfo Object Created for {__instance.name}");
    }

    // Adds 5 minutes to player death timer
    [HarmonyPatch(typeof(Character), nameof(Character.RPCA_Die))]
    [HarmonyPostfix]
    private static void deathTimer(Character __instance)
    {
        CharacterTeamInfo teamHandler = __instance.GetComponent<CharacterTeamInfo>();
        teamHandler.time += 300;

        // Need to count alive and climbing players
        int aliveCount = 0;
        int climbingCount = 0;
        foreach (Character character in Character.AllCharacters)
        {
            if (!character.data.dead)
            {
                aliveCount++;
                if (character.GetComponent<CharacterTeamInfo>().timeOn)
                {
                    climbingCount++;
                }
            }
        }

        // if all alive players are done climbing respawn dead players
        if (climbingCount == 0 && aliveCount > 0)
        {
            Campfire spawnCamp = teamHandler.campfireList[0];
            teamHandler.timeOn = false;
            Debug.Log($"[RaceToThePeak] Respawning all players due to last climber dying");
            respawnPlayers(spawnCamp);
        }
    }

    void Awake()
    {
        string scene = SceneManager.GetActiveScene().name;
        if (scene == "Title")
        {
            return;
        }

        teamOn = true;
        teamGUIOn = true;

        checkpointRadius = 15;

        //links to Character
        myChar = this.gameObject.GetComponent<Character>();

        //Checks if timer should start
        if (scene == "Airport")
        { timeOn = false; }
        else
        { timeOn = true; }

        //TODO Maybe set an 'onsceneload' for the timer
        //Sets starting timer to 0
        time = 0;
        //Sets initial 0 time clock
        timeString = "00:00:00";

        //Initializes Campfire Checkpoints
        if (scene != "Airport")
        {
            GameMap = GameObject.Find("Map");
            campfireList = GameMap.GetComponentsInChildren<Campfire>(true).Cast<Campfire>().ToList();
            //Removes spawn campfire
            campfireList.RemoveAt(0);
            EndFlag = GameObject.Find("Map/Biome_4/Volcano/Peak/Flag_planted_seagull");
        }

        //Debug.Log("[RaceToThePeak] TeamInfo Initialized");
    }

    //This is a call to make initial changeTeam call after the Armband Awake so it doesnt throw an error
    public void InitializeTeam()
    {
        changeTeam(TeamHandler.getPlayerTeam(myChar.name));
    }

    private void FixedUpdate()
    {

        String scene = SceneManager.GetActiveScene().name;
        
        //if (scene == "Airport")
        //{
        //    if (Keyboard.current.pKey.wasPressedThisFrame)
        //    {
        //        Debug.Log("[RaceToThePeak] p key pressed");
        //        teamInt++;
        //        if (teamInt > 11)
        //        { teamInt = 0; }
        //        //Called to update Armband
        //        changeTeam(teamInt);
        //        //PlayerPrefs.SetInt("Team", teamInt);
        //    }
        //}
        //
        //// Start/Stop timer
        //if (Keyboard.current.iKey.wasPressedThisFrame)
        //{
        //    Debug.Log("[RaceToThePeak] i key pressed");
        //    timeOn = !timeOn;
        //    //Debug.Log($"[RaceToThePeak] timeOn:{timeOn} \n update Speed: {Time.fixedDeltaTime}");
        //}
        //
        //// Disable/Enable Teams
        //if (Keyboard.current.oKey.wasPressedThisFrame)
        //{
        //    Debug.Log("[RaceToThePeak] o key pressed");
        //    teamOn = !teamOn;
        //    teamGUIOn = !teamGUIOn;
        //}
        //
        //// Reset Timer
        //if (Keyboard.current.rKey.wasPressedThisFrame)
        //{
        //    Debug.Log("[RaceToThePeak] r key pressed");
        //    resetTimer();
        //}

        if (timeOn)
        {
            time += Time.fixedDeltaTime;
            timeToString();
        }
        //Debug.Log("[RaceToThePeak] TeamInfo Updated");
        if (scene != "Airport")
        {
            checkpointHandler();
        }
    }

    // Converts time float to timer string
    private void timeToString()
    {
        int hourTime = (int)(time / 3600);
        int minTime = (int)(time % 3600 / 60);
        int secTime = (int)(time % 60);

        timeString =  $"{needZero(hourTime)}:{needZero(minTime)}:{needZero(secTime)}";
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

    private void resetTimer()
    {
        time = 0;
    }

    private void checkpointHandler()
    {
        // Checks if in range of one of the campfires
        int idx = 0;
        foreach (Campfire campfire in campfireList)
        {
            if (Vector3.Distance(campfire.transform.position, myChar.Center) <= checkpointRadius)
            {
                campfireList.RemoveAt(idx);
                timeOn = false;
                idx--;
                Debug.Log($"[RaceToThePeak] Timer was turned off for {myChar.name} due to campfire");
                respawnPlayers(campfire);
                return;
            }
            idx++;
        }

        // Checks if in range of the end flag
        if (Vector3.Distance(EndFlag.transform.position, myChar.Center) <= checkpointRadius)
        {
            timeOn = false;
            Debug.Log($"[RaceToThePeak] Timer was turned off for {myChar.name} due to reaching the PEAK");
            return;
        }
    }

    // Checks if its time to respawn all players by the fire
    private static void respawnPlayers(Campfire campfire)
    {
        bool allPlayersStopped = true;
        int aliveCount = 0;
        foreach (Character character in Character.AllCharacters)
        {
            if (!character.data.dead || !character.data.fullyPassedOut)
            {
                if(character.GetComponent<CharacterTeamInfo>().timeOn)
                {
                    allPlayersStopped = false; 
                    break;
                }
                aliveCount++;
            }
        }

        // if all alive players made it to the top, respawn everyone
        // dont want to respawn them if no one is alive, want to end game
        if (allPlayersStopped && aliveCount > 0)
        {
            foreach (Character character in Character.AllCharacters)
            {
                if (character.data.dead || character.data.fullyPassedOut)
                {
                    //respawn them around the campfire
                    Vector3 adjustLocation = new Vector3(10f,0f,8f);
                    character.photonView.RPC("RPCA_ReviveAtPosition", RpcTarget.All, campfire.transform.position + adjustLocation, true);
                }
            }
        }

    }

    // sends new team data to all clients
    public void changeTeam(int newTeam)
    {
        if(!photonView.IsMine)
        { return; }
        photonView.RPC("RPCA_ChangeTeam", RpcTarget.AllBuffered, newTeam);
    }

    [PunRPC]
    public void RPCA_ChangeTeam(int newTeam)
    {
        teamInt = newTeam;
        TeamHandler.addCharacter(myChar.name, newTeam);
        if(myChar.TryGetComponent<Armband>(out Armband armband))
        {
            armband.changeArmband(newTeam);
        }
    }
}
