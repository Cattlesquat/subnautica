/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 * This patch ups the aggression level of creatures, based on difficulty level setting.
 */
namespace DeathRun.Patchers
{
    using HarmonyLib;
    using Items;
    using Common;
    using UnityEngine;
    using System;
    using UWE;

    [HarmonyPatch(typeof(AggressiveWhenSeeTarget))]
    [HarmonyPatch("GetAggressionTarget")]
    internal class Aggression_GetAggressionTarget_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(AggressiveWhenSeeTarget __instance)
        {
            return false;
        }

        [HarmonyPostfix]
        public static void Postfix(AggressiveWhenSeeTarget __instance, ref GameObject __result)
        {
            int maxSearchRings = __instance.maxSearchRings;

            // Not the exploding fish, those are actually more dangerous hiding in ambush
            if (CraftData.GetTechType(__instance.gameObject) == TechType.Crash)
            {
                DeathRun.crashFishSemaphore = true;
            }
            else if (Config.DEATHRUN.Equals(DeathRun.config.creatureAggression) || Config.EXORBITANT.Equals(DeathRun.config.creatureAggression))
            {
                if (DayNightCycle.main.timePassedAsFloat >= DeathRun.FULL_AGGRESSION)
                {
                    maxSearchRings *= 3; //BR// Triple aggression search after 40 minutes
                }
                else if (DayNightCycle.main.timePassedAsFloat >= DeathRun.MORE_AGGRESSION)
                {
                    maxSearchRings *= 2; //BR// Double aggression search after 20 minutes
                }
            }
            else if (Config.HARD.Equals(DeathRun.config.creatureAggression))
            {
                if (DayNightCycle.main.timePassedAsFloat > DeathRun.MORE_AGGRESSION)
                {
                    maxSearchRings += 1; //BR// One extra aggression ring after 20 minutes
                }
            }

            if (Ocean.main.GetDepthOf(Player.main.gameObject) <= 5)
            {
                if (maxSearchRings > __instance.maxSearchRings + 1) maxSearchRings = __instance.maxSearchRings + 1;
            }

            IEcoTarget ecoTarget = EcoRegionManager.main.FindNearestTarget(__instance.targetType, __instance.transform.position, __instance.isTargetValidFilter, maxSearchRings);
            if (ecoTarget == null)
            {
                __result = null;
            }
            else
            {
                __result = ecoTarget.GetGameObject();
            }

            DeathRun.crashFishSemaphore = false;
        }
    }


    [HarmonyPatch(typeof(AggressiveWhenSeeTarget), "IsTargetValid", new Type[] { typeof(GameObject) })]
    internal class Aggression_IsTargetValid_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(GameObject target, AggressiveWhenSeeTarget __instance)
        {
            return false;
        }

        [HarmonyPostfix]
        public static void Postfix(GameObject target, AggressiveWhenSeeTarget __instance, ref bool __result)
        {
            if (target == null)
            {
                __result = false;
                return;
            }
            if (target == __instance.creature.friend)
            {
                __result = false;
                return;
            }
            if (target == Player.main.gameObject && !Player.main.CanBeAttacked())
            {
                __result = false;
                return;
            }
            if (__instance.ignoreSameKind && CraftData.GetTechType(target) == __instance.myTechType)
            {
                __result = false;
                return;
            }
            if (__instance.targetShouldBeInfected)
            {
                InfectedMixin component = target.GetComponent<InfectedMixin>();
                if (component == null || component.GetInfectedAmount() < 0.33f)
                {
                    __result = false;
                    return;
                }
            }

            float dist = Vector3.Distance(target.transform.position, __instance.transform.position);
            if (dist > __instance.maxRangeScalar)
            {
                if (((target != Player.main.gameObject) && !target.GetComponent<Vehicle>()) || (dist > __instance.maxRangeScalar * 4))
                {
                    __result = false;
                    return;
                }

                if (target == Player.main.gameObject)
                {
                    if (Player.main.precursorOutOfWater || PrecursorMoonPoolTrigger.inMoonpool)
                    {
                        __result = false;
                        return;
                    }
                }

                if (target.GetComponent<Vehicle>() != null)
                {
                    if ((target.GetComponent<Vehicle>() != Player.main.currentMountedVehicle) || target.GetComponent<Vehicle>().precursorOutOfWater || PrecursorMoonPoolTrigger.inMoonpool)
                    {
                        __result = false;
                        return;
                    }
                }

            }
            if (!Mathf.Approximately(__instance.minimumVelocity, 0f))
            {
                Rigidbody componentInChildren = target.GetComponentInChildren<Rigidbody>();
                if (componentInChildren != null && componentInChildren.velocity.magnitude <= __instance.minimumVelocity)
                {
                    __result = false;
                    return;
                }
            }

            if ((((target != Player.main.gameObject) || Player.main.IsInside() || Player.main.precursorOutOfWater || PrecursorMoonPoolTrigger.inMoonpool) && (!target.GetComponent<Vehicle>() || (target.GetComponent<Vehicle>() != Player.main.currentMountedVehicle) || target.GetComponent<Vehicle>().precursorOutOfWater)) ||  // Must be player or vehicle
                (Ocean.main.GetDepthOf(target) <= 5) ||                                     // Keeps reapers from eating us up on land
                (!Config.EXORBITANT.Equals(DeathRun.config.creatureAggression) && !Config.DEATHRUN.Equals(DeathRun.config.creatureAggression)) ||            // Only on maximum aggression mode
                (DayNightCycle.main.timePassedAsFloat < DeathRun.MORE_AGGRESSION) ||        // Not at very beginning of game
                (CraftData.GetTechType(__instance.gameObject) == TechType.Crash))           // Not the explody fish (more fun from ambush!)
            {
                __result = __instance.creature.GetCanSeeObject(target);
            }
            else
            {
                if (!Config.EXORBITANT.Equals(DeathRun.config.creatureAggression)) {
                    __result = true; //BR// Can definitely see player "even through terrain"
                } else
                {
                    //BR// For DeathRun setting, we don't check "field of view", only that line-of-sight doesn't pass through terrain.
                    __result = !Physics.Linecast(__instance.transform.position, target.transform.position, Voxeland.GetTerrainLayerMask());
                }
            }
        }
    }


    [HarmonyPatch(typeof(EcoRegion))]
    [HarmonyPatch("FindNearestTarget")]
    internal class EcoRegion_FindNearestTarget_Patch
    {
        [HarmonyPrefix]
        public static bool PreFix(EcoTargetType type, Vector3 wsPos, EcoRegion.TargetFilter isTargetValid, ref float bestDist, ref IEcoTarget best)
        {
            // Explody fish runs regular code - everybody else runs postfix
            return DeathRun.crashFishSemaphore;            
        }

        [HarmonyPostfix]
        public static void PostFix(EcoRegion __instance, EcoTargetType type, Vector3 wsPos, EcoRegion.TargetFilter isTargetValid, ref float bestDist, ref IEcoTarget best)
        {
            if (DeathRun.crashFishSemaphore)
            {
                return;
            }

            ProfilingUtils.BeginSample("EcoRegion.FindNearestTarget");
            __instance.timeStamp = Time.time;
            System.Collections.Generic.HashSet<IEcoTarget> hashSet;
            if (!__instance.ecoTargets.TryGetValue((int)type, out hashSet))
            {
                ProfilingUtils.EndSample(null);
                return;
            }
            float num = float.MaxValue;
            foreach (IEcoTarget ecoTarget in hashSet)
            {
                if (ecoTarget != null && !ecoTarget.Equals(null))
                {
                    float sqrMagnitude = (wsPos - ecoTarget.GetPosition()).sqrMagnitude;

                    if (((ecoTarget.GetGameObject() == Player.main.gameObject) && !Player.main.IsInside() && Player.main.IsUnderwater() && !Player.main.precursorOutOfWater) || 
                        (ecoTarget.GetGameObject().GetComponent<Vehicle>() && (ecoTarget.GetGameObject().GetComponent<Vehicle>() == Player.main.currentMountedVehicle)) && !Player.main.currentMountedVehicle.precursorOutOfWater)
                    {
                        bool feeding = false;
                        if (ecoTarget.GetGameObject() == Player.main.gameObject)
                        {
                            Pickupable held = Inventory.main.GetHeld();
                            if (held != null && (held.GetTechType() == TechType.Peeper))
                            {
                                feeding = true;
                            }
                        }

                        float depth = Ocean.main.GetDepthOf(ecoTarget.GetGameObject());
                        if ((depth > 5) && !feeding)
                        {
                            if (Config.DEATHRUN.Equals(DeathRun.config.creatureAggression) || Config.EXORBITANT.Equals(DeathRun.config.creatureAggression))
                            {
                                if (DayNightCycle.main.timePassedAsFloat >= DeathRun.FULL_AGGRESSION)
                                {
                                    sqrMagnitude = 1; //BR// Player appears very close! (i.e. attractive target)
                                }
                                else if (DayNightCycle.main.timePassedAsFloat >= DeathRun.MORE_AGGRESSION)
                                {
                                    sqrMagnitude /= 4;
                                }
                                else
                                {
                                    sqrMagnitude /= 2;
                                }
                            }
                            else if (Config.HARD.Equals(DeathRun.config.creatureAggression))
                            {
                                if (DayNightCycle.main.timePassedAsFloat >= DeathRun.FULL_AGGRESSION)
                                {
                                    sqrMagnitude /= 3; //BR// Player appears closer! (i.e. attractive target)
                                }
                                else if (DayNightCycle.main.timePassedAsFloat >= DeathRun.MORE_AGGRESSION)
                                {
                                    sqrMagnitude /= 2;
                                }
                            }
                        }
                    }

                    if (sqrMagnitude < num && (isTargetValid == null || isTargetValid(ecoTarget)))
                    {
                        best = ecoTarget;
                        num = sqrMagnitude;
                    }
                }
            }
            if (best != null)
            {
                bestDist = Mathf.Sqrt(num);
            }
            ProfilingUtils.EndSample(null);
        }
    }

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

            if (CraftData.GetTechType(__instance.gameObject) == TechType.Crash)
            {
                return true; // Explody ambush fish just runs normal method
            }

            if (Player.main.precursorOutOfWater || !Player.main.IsUnderwater() || PrecursorMoonPoolTrigger.inMoonpool || (Ocean.main.GetDepthOf(Player.main.gameObject) < 5))
            {
                return true;
            }

            Vehicle veh = Player.main.currentMountedVehicle;
            if (veh != null)
            {
                if (veh.precursorOutOfWater) return true;                
            }

            //BR// Adjust aggression levels            
            if (Config.DEATHRUN.Equals(DeathRun.config.creatureAggression) || Config.EXORBITANT.Equals(DeathRun.config.creatureAggression))
            {
                if (DayNightCycle.main.timePassedAsFloat > DeathRun.FULL_AGGRESSION)
                {
                    aggressionMultiplier = 4;
                    aggressionRadius = 6;
                }
                else if (DayNightCycle.main.timePassedAsFloat > DeathRun.MORE_AGGRESSION)
                {
                    aggressionMultiplier = 2;
                    aggressionRadius = 3;
                } 
                else
                {
                    return true;
                }
            }
            else if (Config.HARD.Equals(DeathRun.config.creatureAggression) && (DayNightCycle.main.timePassedAsFloat > DeathRun.MORE_AGGRESSION))
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


    //[HarmonyPatch(typeof(OutOfBoundsWarp))]
    //[HarmonyPatch("Warp")]
    //internal class OutOfBoundsWarp_Warp_Patch
    //{
    //    [HarmonyPrefix]
    //    public static bool Prefix(OutOfBoundsWarp __instance)
    //    {
    //        CattleLogger.Message("***** Object out of bounds: " + __instance.gameObject + "  " + __instance.gameObject.name);
    //        return true;
    //    }
    //}

}


