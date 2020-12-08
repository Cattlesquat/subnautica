/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 * Adapted from libraryaddict's Radiation Challenge mod -- used w/ permission.
 * 
 * Main adjustments here:
 *   - options for surface air BREATHABLE (always breathable) and POISONED (never breathable)
 *   - Clearer feedback/messages when air isn't breathable.
 */
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DeathRun.Patchers
{
    public class BreathingPatcher
    {
        public static bool warnedNotBreathable = false;
        public static float warningTime = 0;

        public static bool isSurfaceAirPoisoned ()
        {
            if (Config.BREATHABLE.Equals(DeathRun.config.surfaceAir)) return false;
            if (Config.POISONED.Equals(DeathRun.config.surfaceAir)) return true;
            return RadiationUtils.isRadiationActive();
        }

        /**
         * True if player can't breathe the current air, because on the surface. 
         */
        private static bool isAirPoisoned(Player player)
        {
            if (!isSurfaceAirPoisoned()) return false;
            if (player.IsInside()) return false;
            float depth = Ocean.main.GetDepthOf(player.gameObject);
            if (depth > 5) return false;
            return true;
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
                if (!isSurfaceAirPoisoned() && !Player.main.IsInside() && (Ocean.main.GetDepthOf(Player.main.gameObject) < 5))
                {
                    if (warnedNotBreathable)
                    {
                        warnedNotBreathable = true;
                        warningTime = Time.time;
                        ErrorMessage.AddMessage("You find the surface air is now breathable!");
                    }
                }

                return true;
            }

            // Smoke choke sounds in unbreathable atmosphere
            PlayerDamageSounds s = Player.main.gameObject.GetComponent<PlayerDamageSounds>();
            if (s != null) {
                s.painSmoke.Play();
            }

            if (!warnedNotBreathable || (Time.time > warningTime + 30f))
            {
                warnedNotBreathable = true;
                warningTime = Time.time;
                if (Config.POISONED.Equals(DeathRun.config.surfaceAir))
                {
                    ErrorMessage.AddMessage("WARNING! The surface air on this planet is not breathable!");
                }
                else
                {
                    ErrorMessage.AddMessage("The surface air is now too irradiated to breathe!");
                }
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

    /**
     * "Swim to Surface" message obviously inappropriate when surface air is poisoned, plus it always seemed "odd" at 300m anyway.
     */
    [HarmonyPatch(typeof(uGUI_PopupMessage))]
    [HarmonyPatch("SetText")]
    internal class SwimToSurfacePatcher
    {
        static HintSwimToSurface hinter = null;

        public static void setHinter (HintSwimToSurface hinter)
        {
            SwimToSurfacePatcher.hinter = hinter;
        }

        [HarmonyPrefix]
        public static bool Prefix(ref string message)
        {
            if (Language.main.Get("SwimToSurface").Equals(message))
            {
                if (BreathingPatcher.isSurfaceAirPoisoned() || (Ocean.main.GetDepthOf(Player.main.gameObject) > 100))
                {
                    message = "Out of Air!";
                    if (hinter != null)
                    {
                        hinter.messageHash = message.GetHashCode(); // This is so the hinter will clear the message eventually
                    }
                } else
                {
                    if (hinter != null)
                    {
                        hinter.messageHash = hinter.message.GetHashCode(); // Put hinter back to its own hashcode.
                    }
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(HintSwimToSurface))]
    [HarmonyPatch("OnLanguageChanged")]
    internal class HintSwimPatcher
    {
        /**
         * This just grabs the handle to the HintSwimToSurface object "so we can fuck with it later"
         */
        [HarmonyPostfix]
        public static void PostFix (HintSwimToSurface __instance)
        {
            SwimToSurfacePatcher.setHinter(__instance);
        }
    }
}
