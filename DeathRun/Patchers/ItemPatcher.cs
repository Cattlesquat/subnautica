/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 * Adapted from libraryaddict's Radiation Challenge mod -- used w/ permission.
 * 
 * Main changes:
 * - Since our escape pod now sinks, it doesn't really make sense to turn off its fabricator food functions
 * - Combined multiple island-food-rules choices into a single preference setting
 */

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DeathRun.Patchers
{
    public class ItemPatcher
    {
        /**
         * Checks if player is currently prevented from picking up food items on the island. 
         */
        private static bool GetPreventPickup(Transform transform)
        {
            // If we're always allowed island food, no need to trouble ourselves
            if (Config.ALWAYS.Equals(DeathRunPlugin.config.islandFood))
            {
                return false;
            }

            // If player is underwater, or is in a base or escape pod. Return false
            if (transform.position.y <= -1 || Player.main.IsInsideWalkable())
            {
                return false;
            }

            // If we're never allowed island food, then we're just never allowed it!
            if (Config.NEVER.Equals(DeathRunPlugin.config.islandFood))
            {
                return true;
            }

            // If the radiation can't be checked, or the ship hasn't exploded
            if (LeakingRadiation.main == null || !CrashedShipExploder.main.IsExploded())
            {
                // Then it's before the ship exploded
                return Config.AFTER.Equals(DeathRunPlugin.config.islandFood);
            }

            // If radiation is still active
            return RadiationUtils.isSurfaceRadiationActive();
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

        /**
         * Player uses knife on an object -- cancel if it's a food plant
         */
        [HarmonyPrefix]
        public static bool GiveResourceOnDamage(ref GameObject target)
        {
            if (!GetPreventPickup(target.transform)) 
            {
                return true;
            }

            return !IsPlant(CraftData.GetTechType(target));
        }

        /**
         * Attempts to pick up an item on island -- cancel if it's a food plant
         */
        [HarmonyPrefix]
        public static bool HandleItemPickup(ref PickPrefab __instance)
        {
            if (!GetPreventPickup(__instance.transform)) 
            {
                return true;
            }

            return !IsPlant(__instance.pickTech);
        }

        /**
         * Attempts to pick up an item on island -- cancel if it's a food plant
         */
        [HarmonyPrefix]
        public static bool ValidateObject(ref GameObject go, ref bool __result)
        {
            if (!GetPreventPickup(go.transform)) // If not radiation
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

        /**
         * Attempts to pick up an item on island -- cancel if it's a food plant
         */
        [HarmonyPrefix]
        public static bool ShootObject(ref Rigidbody rb)
        {
            if (!GetPreventPickup(rb.transform)) 
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
    }
}