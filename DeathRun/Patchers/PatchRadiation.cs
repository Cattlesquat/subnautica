/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 * From libraryaddict's Radiation Challenge mod -- used w/ permission.
 */

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DeathRun.Patchers
{
    public class PatchRadiation
    {
        private static bool radiated;

        [HarmonyPrefix]
        public static bool DoDamage(DamagePlayerInRadius __instance)
        {
            // If this isn't radiation damage, don't handle
            if (__instance.damageType != DamageType.Radiation)
            {
                return true;
            }

            if (__instance.enabled && __instance.gameObject.activeInHierarchy && __instance.damageRadius > 0f)
            {
                PlayerDistanceTracker tracker = (PlayerDistanceTracker)AccessTools.Field(typeof(DamagePlayerInRadius), "tracker").GetValue(__instance);
                float distanceToPlayer = GetDistance(tracker);

                if (distanceToPlayer <= __instance.damageRadius)
                {
                    float radiationAmount = Player.main.radiationAmount;
                    if (radiationAmount == 0f)
                    {
                        return false;
                    }
                    Player.main.GetComponent<LiveMixin>().TakeDamage(__instance.damageAmount, __instance.transform.position, __instance.damageType, null);
                }
            }

            return false;
        }

        /**
         * Readjust the radiation values dependent on where the player is
         */
        [HarmonyPrefix]
        public static bool Radiate(RadiatePlayerInRange __instance)
        {
            bool flag = GameModeUtils.HasRadiation() && (NoDamageConsoleCommand.main == null || !NoDamageConsoleCommand.main.GetNoDamageCheat());

            PlayerDistanceTracker tracker = (PlayerDistanceTracker)AccessTools.Field(typeof(RadiatePlayerInRange), "tracker").GetValue(__instance);
            float distanceToPlayer = GetDistance(tracker);

            if (radiated = distanceToPlayer <= __instance.radiateRadius && flag && __instance.radiateRadius > 0f)
            {
                float num = Mathf.Clamp01(1f - distanceToPlayer / __instance.radiateRadius);
                float num2 = num;
                if (Inventory.main.equipment.GetCount(TechType.RadiationSuit) > 0)
                {
                    num -= num2 * 0.5f;
                }
                if (Inventory.main.equipment.GetCount(TechType.RadiationHelmet) > 0)
                {
                    num -= num2 * 0.23f * 2f;
                }
                if (Inventory.main.equipment.GetCount(TechType.RadiationGloves) > 0)
                {
                    num -= num2 * 0.23f;
                }

                if (Player.main.IsInBase())
                {
                    num = num / 4;
                }
                else if (Player.main.IsInSubmarine())
                {
                    num = num / 2;
                }

                num = Mathf.Clamp01(num);
                Player.main.SetRadiationAmount(num);
            }
            else
            {
                Player.main.SetRadiationAmount(0f);
            }

            return false;
        }

        /**
         * Forces the radiated symbol to appear on the player's screen, even if they're fully immune
         */
        [HarmonyPrefix]
        public static bool IsRadiated(uGUI_RadiationWarning __instance, ref bool __result)
        {
            Player main = Player.main;

            if (main == null || !radiated)
            {
                __result = false;
            }
            else
            {
                PDA pda = main.GetPDA();

                // Display if pda is null or isn't in use

                __result = pda == null || !pda.isInUse;
            }

            return false; // We're doing the same thing as the base method, just more.
        }

        private static float GetDistance(PlayerDistanceTracker tracker)
        {
            // If the object is null, ship hasn't exploded yet or radius is too small
            if (!RadiationUtils.isSurfaceRadiationActive())
            {
                return tracker.distanceToPlayer;
            }

            // How deep the player is
            float playerDepth = Math.Max(0, -Player.main.transform.position.y);
            float radiationDepth = RadiationUtils.getRadiationDepth();

            // If they are deeper than the radiation, return
            if (playerDepth > radiationDepth)
            {
                return tracker.distanceToPlayer;
            }

            // A % of how close they are to getting out of radiation
            float playerRadiationStrength = playerDepth / radiationDepth;

            return Math.Min(tracker.distanceToPlayer, tracker.maxDistance * playerRadiationStrength);
        }
    }
}
