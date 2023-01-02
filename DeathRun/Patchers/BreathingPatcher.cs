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
using UnityEngine;

namespace DeathRun.Patchers
{
    public class BreathingPatcher
    {
        public static bool warnedNotBreathable = false;
        public static float warningTime = 0;

        public static bool isSurfaceAirPoisoned ()
        {
            if (Config.BREATHABLE.Equals(DeathRunPlugin.config.surfaceAir)) return false;
            if (Config.POISONED.Equals(DeathRunPlugin.config.surfaceAir))
            {
                Inventory main2 = Inventory.main;
                if (main2 != null && main2.equipment != null && main2.equipment.GetCount(DeathRunPlugin.filterChip.TechType) > 0)
                {
                    return false;
                }
                return true;
            }
            return RadiationUtils.isRadiationActive();
        }

        /**
         * True if player can't breathe the current air, because on the surface. 
         */
        private static bool isAirPoisoned(Player player)
        {
            if (!isSurfaceAirPoisoned()) return false;
            if (player.IsInside() || player.precursorOutOfWater) return false;
            float depth = Ocean.GetDepthOf(player.gameObject);
            if (depth > 5) return false;

            // After repairing radiation leaks, when inside the Aurora.
            if (LeakingRadiation.main != null)
            {
                if (LeakingRadiation.main.GetNumLeaks() == 0)
                {
                    if (Config.IRRADIATED.Equals(DeathRunPlugin.config.surfaceAir)) return false;
                    string LDBiome = RadiationUtils.getPlayerBiome();
                    if (LDBiome.Contains("CrashedShip_Interior"))
                    {
                        return false;
                    }
                }
            }

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
                if (!isSurfaceAirPoisoned() && !Player.main.IsInside() && (Ocean.GetDepthOf(Player.main.gameObject) < 5))
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

            // If we're at a pipe so we can actually breathe, treat it normally.
            if (DeathRunPlugin.saveData.nitroSave.atPipe)
            {
                return true;
            }

            if (Player.main.currentMountedVehicle != null)
            {
                return true;
            }

            // Smoke choke sounds in unbreathable atmosphere
            PlayerDamageSounds s = Player.main.gameObject.GetComponent<PlayerDamageSounds>();
            if (s != null)
            {
                s.painSmoke.Play();
            }

            if (!Config.NEVER.Equals(DeathRunPlugin.config.showWarnings))
            {
                if (!warnedNotBreathable ||
                    Config.WHENEVER.Equals(DeathRunPlugin.config.showWarnings) ||
                    (Config.OCCASIONAL.Equals(DeathRunPlugin.config.showWarnings) && (Time.time > warningTime + 300)))
                {

                    if (!warnedNotBreathable || (Time.time > warningTime + 30f))
                    {
                        warnedNotBreathable = true;
                        warningTime = Time.time;
                        if (Config.POISONED.Equals(DeathRunPlugin.config.surfaceAir))
                        {
                            DeathRunUtils.CenterMessage("WARNING! Surface air not breathable!", 5);
                            DeathRunUtils.CenterMessage("A Floating Pump could filter it.", 5, 1);

                            ErrorMessage.AddMessage("WARNING! The surface air on this planet is not breathable!");
                            ErrorMessage.AddMessage("Use of a Floating Pump could filter it however.");
                        }
                        else
                        {
                            DeathRunUtils.CenterMessage("WARNING! Surface air now too irradiated to breathe!", 5);
                            DeathRunUtils.CenterMessage("A Floating Pump could filter it.", 5, 1);

                            ErrorMessage.AddMessage("The surface air is now too irradiated to breathe!");
                        }
                    }
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
         * This is overridden as the breath period on surface would be a very large value otherwise
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
                if (BreathingPatcher.isSurfaceAirPoisoned() || (Ocean.GetDepthOf(Player.main.gameObject) > 100))
                {
                    message = "Out of Air!";
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

    /**
     * I find it endlessly irritating that it won't let you deploy the pump when you're on the surface. 
     * It's not "challenging gameplay" it's just a bad interface. So this fixes it.
     * 
     * ... except apparently the current Qmod version of Harmony doesn't support [HarmonyReversePatch] yet.
     */
    //[HarmonyPatch(typeof(PipeSurfaceFloater))]
    //[HarmonyPatch("OnRightHandDown")]
    //internal class FloatingPumpPatcher
    //{
    //    [HarmonyPostfix]
    //    public static void OnRightHandDown(PipeSurfaceFloater __instance, bool __result)
    //    {
    //        __result = PatchDropTool.OnRightHandDown(__instance) && Player.main.IsSwimming();            
    //    }
    //}

    //[HarmonyPatch]
    //class PatchDropTool
    //{
    //    [HarmonyReversePatch]        
    //    [HarmonyPatch(typeof(DropTool), "OnRightHandDown")]
    //    public static bool OnRightHandDown(DropTool instance)
    //    {
    //        throw new NotImplementedException("Stub - should actually end up running DropTool.OnRightHandDown");
    //    }
    //}
}
