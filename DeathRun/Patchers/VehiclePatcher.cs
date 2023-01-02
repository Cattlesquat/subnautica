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
            DeathRunPlugin.saveData.runData.updateVehicle(RunData.BEST_CYCLOPS, RunData.FLAG_CYCLOPS);
        }
    }


    /* [HarmonyPatch(typeof(Exosuit))]
    [HarmonyPatch("SubConstructionComplete")]
    internal class ExosuitDonePatcher
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            DeathRunPlugin.saveData.runData.updateVehicle(RunData.BEST_EXOSUIT, RunData.FLAG_EXOSUIT);
        }
    } */

    [HarmonyPatch(typeof(SeaMoth))]
    [HarmonyPatch("SubConstructionComplete")]
    internal class SeamothDonePatcher
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            DeathRunPlugin.saveData.runData.updateVehicle(RunData.BEST_SEAMOTH, RunData.FLAG_SEAMOTH);
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
                DeathRunPlugin.saveData.runData.updateVehicle(RunData.BEST_SEAGLIDE, RunData.FLAG_SEAGLIDE);
            }

            if (__instance.logic.craftingTechType == TechType.Builder)
            {
                DeathRunPlugin.saveData.runData.updateVehicle(0, RunData.FLAG_HABITAT);
            }

            if (__instance.logic.craftingTechType == TechType.HatchingEnzymes)
            {
                DeathRunPlugin.saveData.runData.updateVehicle(0, RunData.FLAG_CURE);
            }

            if (__instance.logic.craftingTechType == TechType.Beacon)
            {
                DeathRunPlugin.saveData.runData.updateVehicle(0, RunData.FLAG_BEACON);
            }

            if (__instance.logic.craftingTechType == TechType.DiveReel)
            {
                DeathRunPlugin.saveData.runData.updateVehicle(0, RunData.FLAG_DIVEREEL);
            }

            if (__instance.logic.craftingTechType == TechType.RadiationSuit)
            {
                DeathRunPlugin.saveData.runData.updateVehicle(0, RunData.FLAG_RADIATION);
            }

            if (__instance.logic.craftingTechType == TechType.ReinforcedDiveSuit)
            {
                DeathRunPlugin.saveData.runData.updateVehicle(0, RunData.FLAG_REINFORCED);
            }

            if (__instance.logic.craftingTechType == TechType.LaserCutter)
            {
                DeathRunPlugin.saveData.runData.updateVehicle(0, RunData.FLAG_LASERCUTTER);
            }

            if ((__instance.logic.craftingTechType == TechType.UltraGlideFins) || (__instance.logic.craftingTechType == TechType.SwimChargeFins))
            {
                DeathRunPlugin.saveData.runData.updateVehicle(0, RunData.FLAG_ULTRAGLIDE);
            }


            if (__instance.logic.craftingTechType == TechType.DoubleTank)
            {
                DeathRunPlugin.saveData.runData.updateVehicle(0, RunData.FLAG_DOUBLETANK);
            }

            if ((__instance.logic.craftingTechType == TechType.HighCapacityTank) || (__instance.logic.craftingTechType == TechType.PlasteelTank))
            {
                DeathRunPlugin.saveData.runData.updateVehicle(0, RunData.FLAG_PLASTEEL_TANK);
            }



        }
    }

}
