﻿using AK;
using LevelGeneration;
using System;
using System.Reflection;
using TheArchive.Core.Core;
using TheArchive.Utilities;
using static HackingTool;
using static TheArchive.Core.ArchivePatcher;
using static TheArchive.Utilities.Utils;

namespace TheArchive.HarmonyPatches.Patches
{
    [BindPatchToSetting(nameof(ArchiveSettings.EnableQualityOfLifeImprovements), "QOL")]
    public class QualityOfLifePatches
    {
        // add R4 / R5 to the beginning of the header text
        [ArchivePatch(typeof(PlayerGuiLayer), "UpdateObjectiveHeader")]
        internal static class PlayerGuiLayer_UpdateObjectiveHeaderPatch
        {
            public static void Prefix(ref string header)
            {
                header = $"R{ArchiveMod.CurrentRundown.GetIntValue()}{header}";
            }
        }

        // Change hacking minigame to be more in line with newest version of the game -> minigame finishes and hack disappears instantly
        [ArchivePatch(typeof(HackingTool), "UpdateHackSequence", RundownFlags.RundownFour)]
        internal static class HackingTool_UpdateHackSequencePatch
        {
            private static MethodInfo HackingTool_ClearScreen = typeof(HackingTool).GetMethod("ClearScreen", AnyBindingFlags);
            private static MethodInfo HackingTool_OnStopHacking = typeof(HackingTool).GetMethod("OnStopHacking", AnyBindingFlags);

            public static bool Prefix(ref HackingTool __instance)
            {
                try
                {

                    switch (__instance.m_state)
                    {
                        case HackSequenceState.DoneWait:
                            __instance.m_activeMinigame.EndGame();
                            __instance.m_holoSourceGFX.SetActive(value: false);
                            __instance.Sound.Post(EVENTS.BUTTONGENERICBLIPTHREE);
                            __instance.m_stateTimer = 1f;
                            if (__instance.m_currentHackable != null)
                            {
                                LG_LevelInteractionManager.WantToSetHackableStatus(__instance.m_currentHackable, eHackableStatus.Success, __instance.Owner);
                            }
                            __instance.m_state = HackSequenceState.Done;
                            return false;
                        case HackSequenceState.Done:
                            if (__instance.m_stateTimer < 0f)
                            {
                                HackingTool_ClearScreen.Invoke(__instance, null);
                                HackingTool_OnStopHacking.Invoke(__instance, null);
                                __instance.Sound.Post(EVENTS.BUTTONGENERICSEQUENCEFINISHED);
                                __instance.m_state = HackSequenceState.Idle;
                            }
                            else
                            {
                                __instance.m_stateTimer -= Clock.Delta;
                            }
                            return false;
                    }
                }
                catch (Exception ex)
                {
                    ArchiveLogger.Exception(ex);
                }
                return true;
            }
        }

        // Remove the character restriction in chat, this also results in the user being able to use TMP tags but whatever
        [ArchivePatch(typeof(PlayerChatManager), nameof(PlayerChatManager.Setup))]
        internal static class PlayerChatManager_SetupPatch
        {
            public static void Postfix(ref PlayerChatManager __instance)
            {
                try
                {
                    typeof(PlayerChatManager).GetProperty(nameof(PlayerChatManager.m_forbiddenChars)).SetValue(__instance, new UnhollowerBaseLib.Il2CppStructArray<int>(0));
                }
                catch(Exception ex)
                {
                    ArchiveLogger.Exception(ex);
                }
                
            }
        }

    }
}
