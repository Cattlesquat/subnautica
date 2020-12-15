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
            DeathRun.StartNewGame();
            return true;
        }
    }
}
