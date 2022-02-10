using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using HarmonyLib;
using TMPro;
using SpinCore.Handlers;

namespace SRXDModifiers
{

    [BepInPlugin("SRXDModifiers", "SRXD Modifiers", "0.4.1")]
    public class SRXDModifiers : BaseUnityPlugin
    {

        public static bool isPlayingTrack = false;
        public static bool canToggle = true;
        public static bool isInBaseGame = false;

        public static new BepInEx.Logging.ManualLogSource Logger;
        private void Awake()
        {

            ContextMenu.Setup();
            Logger = base.Logger;
            Harmony.CreateAndPatchAll(typeof(PlaybackSpeed));
            Harmony.CreateAndPatchAll(typeof(AutoPlay));
            Harmony.CreateAndPatchAll(typeof(SurvivalMode));
            Harmony.CreateAndPatchAll(typeof(ModifierText));
            Harmony.CreateAndPatchAll(typeof(ContextMenuCall));

        }

        public class ContextMenuCall
        {
            [HarmonyPatch(typeof(Game), nameof(Game.Update)), HarmonyPostfix]
            private static void Game_Update_Postfix()
            {

                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    ContextMenu.openMenu();
                }

            }
        }

        public class ModifierText
        {

            public static TMP_Text modifierString;

            [HarmonyPatch(typeof(XDLevelCompleteMenu), nameof(XDLevelCompleteMenu.Setup)), HarmonyPostfix]
            private static void Setup_Postfix(XDLevelCompleteMenu __instance)
            {

                var modifiersLabel = GameObject.Instantiate(__instance.scoreValueText.gameObject, __instance.scoreValueText.transform.parent, true).GetComponent<TMP_Text>();
                modifiersLabel.SetText("MODIFIERS");
                modifiersLabel.font = __instance.extendedStats.translatedRhythmHeader.text.font;
                modifiersLabel.fontSize = 14f;
                modifiersLabel.characterSpacing = 1f;
                modifiersLabel.fontStyle = FontStyles.Italic;
                modifiersLabel.fontStyle ^= FontStyles.Bold;
                modifiersLabel.transform.localPosition = new Vector3(300f, 11f, 0f);
                modifiersLabel.gameObject.SetActive(true);

                if (modifierString != null) UnityEngine.Object.Destroy(modifierString);

                modifierString = GameObject.Instantiate(__instance.scoreValueText.gameObject, __instance.scoreValueText.transform.parent, true).GetComponent<TMP_Text>();
                modifierString.font = __instance.scoreValueText.font;
                modifierString.fontSize = 13.5f;
                modifierString.fontStyle = FontStyles.Bold;
                modifierString.transform.localPosition = new Vector3(300f, -11f, 0f);
                modifierString.enableAutoSizing = true;
                modifierString.alignment = TextAlignmentOptions.Right;

                var text = "";

                if (Track.Instance.basePitch == 1f && !AutoPlay.enabled && !SurvivalMode.enabled) text = " - ";

                if (PlaybackSpeed.current != 1f) text += $"x{PlaybackSpeed.current}";
                if (AutoPlay.enabled) text += " AP";
                if (SurvivalMode.enabled) text += " SM";

                modifierString.SetText(text);
                modifierString.gameObject.SetActive(true);

            }

        }

        public class PlaybackSpeed
        {
            public static double current = 1.0f;

            public static void updatePlaybackSpeed(double value)
            {
                if (Track.Instance == null) return;

                Track.Instance.ChangePitch(value);
                current = value;
            }

        }

        public class AutoPlay
        {
            public static MainCamera cameraInstance;
            public static bool enabled = false;
            public static bool bgViewMode = false;

            [HarmonyPatch(typeof(MainCamera), nameof(MainCamera.UpdateCameraTransform)), HarmonyPostfix]
            private static void CameraUpdate_Postfix(MainCamera __instance)
            {
                if (bgViewMode && enabled)
                {
                    __instance.cameraSplineOffset = -0.75f - PlayerSettingsData.Instance.TrackSpeedOverride / 2f;
                }
                else
                {
                    __instance.cameraSplineOffset = 0f;
                }
            }

            [HarmonyPatch(typeof(Wheel), nameof(Wheel.UpdateWheel)), HarmonyPostfix]
            private static void Wheel_UpdateWheel_Postfix(Wheel __instance)
            {
                if (enabled && !isInBaseGame)
                {
                    __instance.MakeCpuControlledThisFrame();
                }

            }

            [HarmonyPatch(typeof(Game), nameof(Game.Update)), HarmonyPostfix]
            private static void Game_Update_Postfix()
            {
                if (Input.GetKeyDown(KeyCode.B))
                {
                    bgViewMode = !bgViewMode;
                }
            }

            private static void resetScore()
            {
                if (enabled || PlaybackSpeed.current == 0.8f)
                {
                    Track.Instance.playStateFirst.scoreState.ClearScore();
                    Track.Instance.playStateFirst.scoreState.maxCombo = 0;
                }
            }


            [HarmonyPatch(typeof(InGameTrackDetails), "Update"), HarmonyPostfix]
            private static void UpdateHUD_Postfix(InGameTrackDetails __instance)
            {
                if (bgViewMode && __instance != null) __instance.canvasGroup.alpha = 0f;
            }

            [HarmonyPatch(typeof(Track), nameof(Track.CompleteSong)), HarmonyPrefix]
            private static void Track_CompleteSong_Prefix()
            {
                resetScore();
            }

            [HarmonyPatch(typeof(Track), nameof(Track.FailSong)), HarmonyPrefix]
            private static void Track_FailSong_Prefix()
            {
                resetScore();
            }
        }

        public class SurvivalMode
        {

            public static bool enabled = false;

            [HarmonyPatch(typeof(PlayState), nameof(PlayState.ReviveSpeed), MethodType.Getter), HarmonyPostfix]
            private static void PlayableTrackData_AddNotes_Postfix(ref float __result)
            {
                var defaultReviveSpeed = __result;

                if (enabled)
                {
                    __result = 0f;
                }
                else
                {
                    __result = defaultReviveSpeed;
                }
            }

        }
    }
}