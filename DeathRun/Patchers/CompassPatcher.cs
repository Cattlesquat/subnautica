/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 * Allows FilterChip to also function as a compass
 */
namespace DeathRun.Patchers
{
    using HarmonyLib;
    using Items;
    using Common;
    using UnityEngine;

    [HarmonyPatch(typeof(Equipment))]
    [HarmonyPatch("GetCount")]
    internal class CompassGetCountPatcher
    {
        /**
         * When a count of equipped Compasses is requested, if we have a Filter Chip equipped, consider that to be a compass as well.
         */
        [HarmonyPrefix]
        public static bool Prefix(ref Equipment __instance, TechType techType, ref int __result)
        {
            if (techType == TechType.Compass)
            {
                int count;
                __instance.equippedCount.TryGetValue(DeathRun.filterChip.TechType, out count);
                if (count > 0)
                {
                    __result = count;
                    return false;
                }
            }
            return true;
        }
    }
}