/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 * Adapted from libraryaddict's Radiation Challenge mod -- used w/ permission.
 * 
 * Significant adjustment here:
 *   - option for surface air ALWAYS poisoned.
 */
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DeathRun.Patchers
{
    public class PatchBreathing
    {
        /**
         * True if player can't breathe the current air, because on the surface. 
         */
        private static bool isAirPoisoned(Player player)
        {
            if (Config.ALWAYS.Equals(DeathRun.config.surfaceAir)) return false;
            if (player.IsInside()) return false;
            float depth = Ocean.main.GetDepthOf(player.gameObject);
            if (depth > 0) return false;
            if (Config.NEVER.Equals(DeathRun.config.surfaceAir)) return true;
            return RadiationUtils.isInAnyRadiation(player.transform);
        }

        /**
         * Called when it wants to check about adding air to the player, we cancel if the air is bad
         */
        [HarmonyPrefix]
        public static bool AddOxygenAtSurface(OxygenManager __instance, ref float timeInterval)
        {
            Player player = Player.main;

            // If this is the wrong oxygen manager, or air isnt bad
            if (player.oxygenMgr != __instance || !isAirPoisoned(player))
            {
                return true;
            }

            // Cancel the method as they cannot breathe
            return false;
        }

        /**
         * Called when player surfaces in unbreathable air, to cancel the "breathing a lot" sounds
         */
        [HarmonyPrefix]
        public static bool PlayReachSurfaceSound(WaterAmbience __instance)
        {
            if (!isAirPoisoned(Player.main))
            {
                return true;
            }

            var time = __instance.GetType().GetField("timeReachSurfaceSoundPlayed", System.Reflection.BindingFlags.NonPublic
        | System.Reflection.BindingFlags.Instance);

            if (Time.time < (float)time.GetValue(__instance) + 1f)
            {
                return false;
            }

            // Skip other sounds as they are dependent on breathing

            time.SetValue(__instance, Time.time);
            __instance.reachSurfaceWithTank.Play(); // Different sound so no splash

            return false;
        }

        /**
         * Overrides a check in Player, this controls if the player can breathe fresh air where they are currently
         */
        [HarmonyPrefix]
        public static bool CanBreathe(Player __instance, ref bool __result)
        {
            if (isAirPoisoned(__instance))
            {
                __result = false;
                return false;
            }

            // Don't cancel
            return true;
        }

        /**
         * This is overriden as the breath period on surface would be a very large value otherwise
         */
        [HarmonyPostfix]
        public static void GetBreathPeriod(ref float __result)
        {
            if (!isAirPoisoned(Player.main))
            {
                return;
            }

            // Set their breath to the lowest value, 3 seconds is better than 9999 seconds
            __result = Math.Min(3f, __result);
        }
    }
}
