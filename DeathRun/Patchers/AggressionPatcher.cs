/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 * This patch ups the aggression level of creatures.
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
            maxSearchRings *= 3; //BR// Triple aggression search

            IEcoTarget ecoTarget = EcoRegionManager.main.FindNearestTarget(__instance.targetType, __instance.transform.position, __instance.isTargetValidFilter, maxSearchRings);
            if (ecoTarget == null)
            {
                __result = null;
            }
            else
            {
                __result = ecoTarget.GetGameObject();
            }
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

            if ((target != Player.main.gameObject) && !target.GetComponent<Vehicle>())
            {
                __result = __instance.creature.GetCanSeeObject(target);
            }
            else
            {
                __result = true; //BR// Can definitely see player
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
            return false;
        }

        [HarmonyPostfix]
        public static void PostFix(EcoRegion __instance, EcoTargetType type, Vector3 wsPos, EcoRegion.TargetFilter isTargetValid, ref float bestDist, ref IEcoTarget best)
        {
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
                    if ((ecoTarget.GetGameObject() == Player.main.gameObject) || (ecoTarget.GetGameObject().GetComponent<Vehicle>()))
                    {
                        sqrMagnitude = 9; //BR// Player appears close! (i.e. attractive target)
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
}

