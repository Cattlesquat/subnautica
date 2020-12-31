/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 * This is a hacky way of detecting when we're actually "starting a new game" (rather than loading one), so that our various
 * mod-specific game state things get properly initialized when starting a new game from main menu (after playing another one).
 */
namespace DeathRun.Patchers
{
    using HarmonyLib;
    using Items;
    using Common;
    using UnityEngine;
    using UnityEngine.UI;
    using System.IO;
    using System.Collections.Generic;

    [HarmonyPatch(typeof(uGUI_MainMenu))]
    [HarmonyPatch("OnButtonSurvival")]
    internal class SurvivalButtonPatcher
    {
        [HarmonyPrefix]
        public static bool Prefix()
        {
            DeathRunUtils.HideHighScores(true);
            DeathRun.StartNewGame();
            return true;
        }
    }


    [HarmonyPatch(typeof(uGUI_MainMenu))]
    [HarmonyPatch("OnButtonHardcore")]
    internal class HardcoreButtonPatcher
    {
        [HarmonyPrefix]
        public static bool Prefix()
        {
            DeathRunUtils.HideHighScores(true);
            DeathRun.StartNewGame();
            return true;
        }
    }


    [HarmonyPatch(typeof(uGUI_MainMenu))]
    [HarmonyPatch("OnButtonFreedom")]
    internal class FreedomButtonPatcher
    {
        [HarmonyPrefix]
        public static bool Prefix()
        {
            DeathRunUtils.HideHighScores(true);
            DeathRun.StartNewGame();
            return true;
        }
    }


    [HarmonyPatch(typeof(uGUI_MainMenu))]
    [HarmonyPatch("OnButtonCreative")]
    internal class CreativeButtonPatcher
    {
        [HarmonyPrefix]
        public static bool Prefix()
        {
            DeathRunUtils.HideHighScores(true);
            DeathRun.StartNewGame();
            return true;
        }
    }


    [HarmonyPatch(typeof(uGUI_MainMenu))]
    [HarmonyPatch("ShowPrimaryOptions")]
    internal class ShowPrimaryOptionsPatcher
    {
        [HarmonyPrefix]
        public static bool Prefix(bool show)
        {
            if (show)
            {
                DeathRunUtils.ShowHighScores(true);
            }
            else
            {
                DeathRunUtils.HideHighScores(true);
            }            
            return true;
        }
    }

    [HarmonyPatch(typeof(MainMenuLoadButton))]
    [HarmonyPatch("Load")]
    internal class LoadButtonPatcher
    {
        [HarmonyPrefix]
        public static bool Prefix(MainMenuLoadButton __instance)
        {
            if (!__instance.IsEmpty()) 
            {
                DeathRunUtils.HideHighScores(true);
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(SceneCleaner))]
    [HarmonyPatch("Open")]
    internal class SceneCleanerPatcher
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            DeathRunUtils.ShowHighScores(true);
        }
    }


    /**
     * UpdateLoadButtonState -- this lets us annotate the load buttons for individual games.
     */
    [HarmonyPatch(typeof(MainMenuLoadPanel))]
    [HarmonyPatch("UpdateLoadButtonState")]
    internal class MainMenuLoadPanel_UpdateLoadButtonState_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(MainMenuLoadPanel __instance, MainMenuLoadButton lb)
        {
            SaveLoadManager.GameInfo gameInfo = SaveLoadManager.main.GetGameInfo(lb.saveGame);

            if ((gameInfo == null) || !gameInfo.IsValid() || gameInfo.isFallback)
            {
                return;
            }

            DeathRunSaveData slotData = DeathRunUtils.FindSave(lb.saveGame);
            if (slotData == null)
            {
                return;
            }

            if (!"".Equals(slotData.startSave.message))
            {
                lb.load.FindChild("SaveGameMode").GetComponent<Text>().text = slotData.startSave.message;

                slotData.runData.updateFromSave(slotData);

                string duration = Utils.PrettifyTime((int)slotData.playerSave.allLives);

                if (slotData.playerSave.allLives >= 60 * 60 * 24)
                {
                    int total = (int)slotData.playerSave.allLives;
                    int days = total / (60 * 60 * 24);
                    total -= (days * 60 * 60 * 24);

                    int hours = total / (60 * 60);
                    total -= hours * 60 * 60;

                    int minutes = total / 60;
                    total -= minutes;

                    duration = "" + days + " " + ((days == 1) ? "day" : "days") + ", " + hours + ":" + ((minutes < 10) ? "0" : "") + minutes;
                }
                    
                duration += ". Score: " + slotData.runData.Score;

                lb.load.FindChild("SaveGameLength").GetComponent<Text>().text = duration;
            }
        }
    }

    /**
     * LoadSlotsAsync - in Unity's insane user files system, we add the name of our JSON file to the list of files for it to retrieve from the
     * "save slot". This is entirely so we can have access to it when the list of saved games is being created on the main menu.
     */
    [HarmonyPatch(typeof(UserStoragePC))]
    [HarmonyPatch("LoadSlotsAsync")]
    internal class LoadSlots_Patch
    {
        [HarmonyPrefix]
        private static bool Prefix(ref List<string> fileNames)
        {
            fileNames.Add(DeathRun.SaveFile);
            return true;
        }
    }

    /**
     * RegisterSaveGame - as saved games get "registered" to each slot on the load menu, we record a copy of the DeathRun JSON, so that we
     * will have access to it when the individual load slots are being drawn. Did I mention that Unity's user file system is insane?
     */
    [HarmonyPatch(typeof(SaveLoadManager))]
    [HarmonyPatch("RegisterSaveGame")]
    internal class RegisterSaveGame_Patch
    {
        [HarmonyPostfix]
        private static void Postfix(string slotName, UserStorageUtils.LoadOperation loadOperation)
        {
            byte[] bytes;

            if (loadOperation.GetSuccessful()) {
                if (loadOperation.files.TryGetValue(DeathRun.SaveFile, out bytes))
                {
                    DeathRunSaveData saveData = new DeathRunSaveData();
                    if (DeathRunSaveData.LoadFromBytes(bytes, out saveData)) 
                    {
                        DeathRunUtils.RegisterSave(slotName, saveData);
                    }
                }
            }
        }
    }
}
