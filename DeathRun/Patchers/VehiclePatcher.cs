/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 * Keep track of when vehicles get built
 */
namespace DeathRun.Patchers
{
    using HarmonyLib;
    using Items;
    using Common;
    using UnityEngine;

    [HarmonyPatch(typeof(CyclopsLightingPanel))]
    [HarmonyPatch("SubConstructionComplete")]
    internal class CyclopsDonePatcher
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            DeathRun.saveData.runData.updateVehicle(RunData.BEST_CYCLOPS, RunData.FLAG_CYCLOPS);
        }
    }


    [HarmonyPatch(typeof(Exosuit))]
    [HarmonyPatch("SubConstructionComplete")]
    internal class ExosuitDonePatcher
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            DeathRun.saveData.runData.updateVehicle(RunData.BEST_EXOSUIT, RunData.FLAG_EXOSUIT);
        }
    }

    [HarmonyPatch(typeof(SeaMoth))]
    [HarmonyPatch("SubConstructionComplete")]
    internal class SeamothDonePatcher
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            DeathRun.saveData.runData.updateVehicle(RunData.BEST_SEAMOTH, RunData.FLAG_SEAMOTH);
        }
    }

    [HarmonyPatch(typeof(GhostCrafter))]
    [HarmonyPatch("OnCraftingEnd")]
    internal class FabricatorDonePatcher
    {
        [HarmonyPostfix]
        public static void Postfix(GhostCrafter __instance)
        {
            if (__instance.logic.craftingTechType == TechType.Seaglide)
            {
                DeathRun.saveData.runData.updateVehicle(RunData.BEST_SEAGLIDE, RunData.FLAG_SEAGLIDE);
            }

            if (__instance.logic.craftingTechType == TechType.Builder)
            {
                DeathRun.saveData.runData.updateVehicle(0, RunData.FLAG_HABITAT);
            }

            if (__instance.logic.craftingTechType == TechType.HatchingEnzymes)
            {
                DeathRun.saveData.runData.updateVehicle(0, RunData.FLAG_CURE);
            }

        }
    }

}
