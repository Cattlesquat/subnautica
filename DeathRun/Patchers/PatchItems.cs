/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 * Adapted from libraryaddict's Radiation Challenge mod -- used w/ permission.
 */

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DeathRun.Patchers
{
    public class PatchItems
    {
        /// <summary>
        /// If the player is in the outside air and there is radiation active
        /// </summary>
        private static bool GetPreventPickup(Transform transform)
        {
            // If player is underwater, or is in a base or escape pod. Return false
            if (transform.position.y <= -1 || Player.main.IsInsideWalkable())
            {
                return false;
            }
            else
            // If the radiation can't be checked, or the ship hasn't exploded
            if (LeakingRadiation.main == null || !CrashedShipExploder.main.IsExploded())
            {
                // Then it's before the ship exploded
                return Main.config.preventPreRadiativeFoodPickup;
            }
            else if (!Main.config.preventRadiativeFoodPickup) // If they can pickup after ship has exploded, return early
            {
                return false;
            }

            // If radiation is still active
            return RadiationUtils.GetSurfaceRadiationActive();
        }

        private static bool IsPlant(TechType techType)
        {
            switch (techType)
            {
                case TechType.PurpleVegetablePlant:
                case TechType.MelonPlant:
                case TechType.BulboTree:
                case TechType.Melon:
                case TechType.SmallMelon:
                case TechType.HangingFruit:
                case TechType.PurpleVegetable:
                case TechType.HangingFruitTree:
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Called when player knifes an object, cancel if it's a food plant
        /// </summary>
        [HarmonyPrefix]
        public static bool GiveResourceOnDamage(ref GameObject target)
        {
            if (!GetPreventPickup(target.transform)) // If island isn't radiative, return without cancelling
            {
                return true;
            }

            return !IsPlant(CraftData.GetTechType(target));
        }

        /// <summary>
        /// Disable pickups on the island
        /// </summary>
        [HarmonyPrefix]
        public static bool HandleItemPickup(ref PickPrefab __instance)
        {
            if (!GetPreventPickup(__instance.transform)) // If not radiative
            {
                return true;
            }

            return !IsPlant(__instance.pickTech);
        }

        /// <summary>
        /// Disable pickups on the island
        /// </summary>
        [HarmonyPrefix]
        public static bool ValidateObject(ref GameObject go, ref bool __result)
        {
            if (!GetPreventPickup(go.transform)) // If not radiative
            {
                return true;
            }

            PickPrefab prefab = go.GetComponent<PickPrefab>();

            if (prefab == null)
            {
                return true;
            }

            // If it returns false, then it will cancel the method and prevent them from using it
            return __result = !IsPlant(prefab.pickTech);
        }

        /// <summary>
        /// Disable pickups on the island
        /// </summary>
        [HarmonyPrefix]
        public static bool ShootObject(ref Rigidbody rb)
        {
            if (!GetPreventPickup(rb.transform)) // If not radiative
            {
                return true;
            }

            PickPrefab prefab = rb.GetComponent<PickPrefab>();

            if (prefab == null)
            {
                return true;
            }

            return !IsPlant(prefab.pickTech);
        }

        /// <summary>
        /// Controls the fabricator inside the escape pod, disables food crafting once ship has exploded
        /// </summary>
        [HarmonyPrefix]
        public static bool IsCraftRecipeFulfilled(ref TechType techType, ref bool __result)
        {
            // If they are not in the escape pod or if the ship hasn't exploded, don't do anything
            if (!Player.main.escapePod.value || !CrashedShipExploder.main.IsExploded())
            {
                return true;
            }

            // Switch between the tech types, we could use a name check but that's always prone to
            // random edge cases. Better we just stick with the list of known foods
            switch (techType)
            {
                case TechType.Bleach:
                case TechType.FilteredWater:
                case TechType.DisinfectedWater:
                case TechType.CookedPeeper:
                case TechType.CookedHoleFish:
                case TechType.CookedGarryFish:
                case TechType.CookedReginald:
                case TechType.CookedBladderfish:
                case TechType.CookedHoverfish:
                case TechType.CookedSpadefish:
                case TechType.CookedBoomerang:
                case TechType.CookedEyeye:
                case TechType.CookedOculus:
                case TechType.CookedHoopfish:
                case TechType.CookedSpinefish:
                case TechType.CookedLavaEyeye:
                case TechType.CookedLavaBoomerang:
                case TechType.CuredPeeper:
                case TechType.CuredHoleFish:
                case TechType.CuredGarryFish:
                case TechType.CuredReginald:
                case TechType.CuredBladderfish:
                case TechType.CuredHoverfish:
                case TechType.CuredSpadefish:
                case TechType.CuredBoomerang:
                case TechType.CuredEyeye:
                case TechType.CuredOculus:
                case TechType.CuredHoopfish:
                case TechType.CuredSpinefish:
                case TechType.CuredLavaEyeye:
                case TechType.CuredLavaBoomerang:
                    __result = false; // They may not craft this
                    return false; // Cancel method
                default:
                    return true; // Let method continue
            }
        }
    }
}