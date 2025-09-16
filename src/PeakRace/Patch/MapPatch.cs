using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using static PositionSyncer;

namespace PeakRace.Patch;

[HarmonyPatch]
internal class MapPatch
{

    //Function to restart everyones timers and respawn players
    [HarmonyPatch(typeof(Campfire), nameof(Campfire.Light_Rpc))]
    [HarmonyPostfix]
    private static void restartTimer()
    {
        //Finds all Characters
        foreach (Character allChar in Character.AllCharacters)
        {
            Debug.Log("[RaceToThePeak] Timer was turned back on due to map transition");
            CharacterTeamInfo TeamInfo = allChar.GetComponentInChildren<CharacterTeamInfo>();
            TeamInfo.timeOn = true;
        }
    }

    //Functions to Change Interaction Text
    [HarmonyPatch(typeof(RespawnChest), nameof(RespawnChest.GetInteractionText))]
    [HarmonyPostfix]
    private static void disableRespawnStatueText(ref string __result)
    {
        //Forces Interaction Text
        __result = LocalizedText.GetText("TOUCH");
    }

    //Functions to force only Items to spawn
    [HarmonyPatch(typeof(RespawnChest), nameof(RespawnChest.SpawnItems))]
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        List <CodeInstruction> codes = instructions.ToList();

        // We want to remove the lines of code from SpawnItems that respawns players so we can add it to Campfires later
        // This isnt really clear, and mostly hardcoded... Whoopsie 0_0

        for (int i = 6; i <= 18; i++)
        {
            codes[i].opcode = OpCodes.Nop;
        }
        Debug.Log("[RaceToThePeak] Successfully set Respawn Statue to Only Items");
        return codes;
    }

    //TODO Function to wait for all remaining players before ending game

    //TODO disable respawn ability on statues if in teammode

    //TODO change list of survivors to winning team

    //TODO change game time to winning team time

}

