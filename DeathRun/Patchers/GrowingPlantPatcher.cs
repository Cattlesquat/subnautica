/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 * Farming challenge
 */
namespace DeathRun.Patchers
{
    using HarmonyLib;
    using Items;
    using Common;
    using UnityEngine;

    [HarmonyPatch(typeof(GrowingPlant))]
    [HarmonyPatch("GetGrowthDuration")]
    internal class GrowingPlantPatcher
    {
        [HarmonyPostfix]
        public static void Postfix(GrowingPlant __instance, ref float __result)
        {
            if (Config.FARMING_VERY_SLOW.Equals(DeathRunPlugin.config.farmingChallenge))
            {
                __result *= 6;
            } 
            else if (Config.FARMING_SLOW.Equals(DeathRunPlugin.config.farmingChallenge))
            {
                __result *= 3;
            }
        }
    }
}
