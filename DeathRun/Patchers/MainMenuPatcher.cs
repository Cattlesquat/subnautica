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

    [HarmonyPatch(typeof(uGUI_MainMenu))]
    [HarmonyPatch("OnButtonSurvival")]
    internal class SurvivalButtonPatcher
    {
        [HarmonyPrefix]
        public static bool Prefix()
        {
            DeathRunUtils.HideHighScores();
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
            DeathRunUtils.HideHighScores();
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
            DeathRunUtils.HideHighScores();
            DeathRun.StartNewGame();
            return true;
        }
    }


    [HarmonyPatch(typeof(uGUI_MainMenu))]
    [HarmonyPatch("OnButtonFreedom")]
    internal class CreativeButtonPatcher
    {
        [HarmonyPrefix]
        public static bool Prefix()
        {
            DeathRunUtils.HideHighScores();
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
                DeathRunUtils.ShowHighScores();
            }
            else
            {
                DeathRunUtils.HideHighScores();
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
                DeathRunUtils.HideHighScores();
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
            DeathRunUtils.ShowHighScores();
        }
    }
}
