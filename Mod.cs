using BepInEx;
using BepInEx.IL2CPP;
using UnityEngine;
using HarmonyLib;

namespace SRXDModifiers
{
    [BepInPlugin("useredge.srxdmodifiers", "SRXD Modifiers Mod", "0.1.0")]
    public class Mod : BasePlugin
    {
        private static bool isPlayingTrack = false;
        private static bool canToggle = true;
        private static bool isInBaseGame = false;

        private static double[] speeds = new double[] { 0.8, 1.0, 1.25, 1.5 };
        private static int index = 1;
        private static int lowestAllowed = 1;

        public static BepInEx.Logging.ManualLogSource Logger;
        public override void Load()
        {
            Logger = Log;
            Harmony.CreateAndPatchAll(typeof(Mod));
        }

        [HarmonyPatch(typeof(Game), nameof(Game.Update)), HarmonyPostfix]
        private static void Game_Update_Postfix()
        {
            //Logger.LogMessage($"isPlayingTrack: {isPlayingTrack}");
            //Logger.LogMessage($"canToggle: {canToggle}");

            if (Input.GetKeyDown(KeyCode.F6))
            {
                if (canToggle && !isPlayingTrack)
                {
                    if (index < speeds.Length - 1)
                    {
                        index++;
                        Track.Instance.ChangePitch(speeds[index]);
                        Logger.LogMessage("---------------------------------PITCH UP---------------------------------");
                        Logger.LogMessage($"Pitch: {Track.Instance.basePitch}");
                        Logger.LogMessage("---------------------------------PITCH UP---------------------------------");
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.F5))
            {
                if (canToggle && !isPlayingTrack)
                {
                    if (index >= lowestAllowed)
                    {
                        index--;
                        Track.Instance.ChangePitch(speeds[index]);
                        Logger.LogMessage("---------------------------------PITCH DOWN---------------------------------");
                        Logger.LogMessage($"Pitch: {Track.Instance.basePitch}");
                        Logger.LogMessage("---------------------------------PITCH DOWN---------------------------------");
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Track), nameof(Track.RestartTrack)), HarmonyPostfix]
        private static void Track_RestartTrack_Postfix()
        {
            Logger.LogMessage("---------------------------------RESTARTED---------------------------------");
            Logger.LogMessage($"Pitch: {Track.Instance.basePitch}");
            Logger.LogMessage("---------------------------------RESTARTED---------------------------------");
            canToggle = false;
            isPlayingTrack = true;
        }

        [HarmonyPatch(typeof(Track), nameof(Track.ReturnToPickTrack)), HarmonyPostfix]
        private static void Track_ReturnToPickTrack_Postfix()
        {
            index = 1;
            Logger.LogMessage("---------------------------------RETURNED TO SELECTION---------------------------------");
            canToggle = false;
            isPlayingTrack = true;
        }

        [HarmonyPatch(typeof(Track), nameof(Track.PlayTrack)), HarmonyPostfix]
        private static void Track_PlayTrack_Postfix()
        {
            Track.Instance.ChangePitch(speeds[index]);
            Logger.LogMessage("---------------------------------STARTED PLAYING---------------------------------");
            Logger.LogMessage($"Pitch: {Track.Instance.basePitch}");
            Logger.LogMessage("---------------------------------STARTED PLAYING---------------------------------");
            canToggle = false;
            isPlayingTrack = true;
        }

            [HarmonyPatch(typeof(Track), nameof(Track.CompleteSong)), HarmonyPostfix]
        private static void Track_CompleteSong_Postfix()
        {
            Logger.LogMessage($"Song cleared at pitch: {Track.Instance.basePitch}");
            canToggle = true;
            isPlayingTrack = false;
        }

        [HarmonyPatch(typeof(Track), nameof(Track.FailSong)), HarmonyPostfix]
        private static void Track_FailSong_Postfix()
        {
            Logger.LogMessage($"Failed! Pitch was: {Track.Instance.basePitch}");
            canToggle = true;
            isPlayingTrack = false;
        }

        [HarmonyPatch(typeof(Track), nameof(Track.Update)), HarmonyPostfix]
        private static void Track_Update_Postfix()
        {
            if (Track.IsPlaying)
            { 
                canToggle = false;
                isPlayingTrack = true;
            }

            if(Track.IsInWorldMenu)
            {
                canToggle = true;
                isPlayingTrack = false;
            }

            if (isInBaseGame)
            {
                lowestAllowed = 2;
            }
            else
            {
                lowestAllowed = 1;
            }

        }

        [HarmonyPatch(typeof(XDMainMenu), nameof(XDMainMenu.OpenLevelSelect)), HarmonyPostfix]
        private static void XDMainMenu_OpenLevelSelect_Postfix()
        {
            Track.Instance.ChangePitch(speeds[index]);
            canToggle = false;
            isInBaseGame = true;
        }

        [HarmonyPatch(typeof(XDMainMenu), nameof(XDMainMenu.OpenCustomTrackSelect)), HarmonyPostfix]
        private static void XDMainMenu_OpenCustomTrackSelect_Postfix()
        {
            Track.Instance.ChangePitch(speeds[index]);
            canToggle = true;
            isInBaseGame = false;
        }
    }
}
