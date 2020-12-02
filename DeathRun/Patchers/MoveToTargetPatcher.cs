/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 * This patch increases the aggression level of creatures.
 */
namespace DeathRun.Patchers
{
    using HarmonyLib;
    using Items;
    using Common;
    using UnityEngine;

    /**
     * Okay this runs ALMOST the same code as the original, but searches a much wider radius and doubles the aggression level
     * 
     * DeathRun.aggressionMultiplier -- how much more aggressive than baseline creatures should be
     * DeathRun.aggressionRadius     -- how much wider a target area than baseline creatures should search for targets
     */
    [HarmonyPatch(typeof(MoveTowardsTarget))]
    [HarmonyPatch("UpdateCurrentTarget")]
    internal class MoveTowardsTarget_UpdateCurrentTarget_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(MoveTowardsTarget __instance)
        {
            float aggressionMultiplier;
            int aggressionRadius;

            //BR// Adjust aggression levels
            if (Config.DEATHRUN.Equals(DeathRun.config.creatureAggression))
            {
                aggressionMultiplier = 4;
                aggressionRadius = 6;
            }
            else if (Config.HARD.Equals(DeathRun.config.creatureAggression))
            {
                aggressionMultiplier = 2;
                aggressionRadius = 3;
            }
            else
            {
                return true; // Just run normal method
            }

            ProfilingUtils.BeginSample("UpdateCurrentTarget");
            if (EcoRegionManager.main != null && (Mathf.Approximately(__instance.requiredAggression, 0f) || __instance.creature.Aggression.Value * aggressionMultiplier >= __instance.requiredAggression))
            {
                IEcoTarget ecoTarget = EcoRegionManager.main.FindNearestTarget(__instance.targetType, __instance.transform.position, __instance.isTargetValidFilter, aggressionRadius);

                if (ecoTarget != null)
                {
                    __instance.currentTarget = ecoTarget;
                }
                else
                {
                    __instance.currentTarget = null;
                }
            }
            ProfilingUtils.EndSample(null);
            return false;
        }
    }
}

