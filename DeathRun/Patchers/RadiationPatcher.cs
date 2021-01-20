/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 * From libraryaddict's Radiation Challenge mod -- used w/ permission.
 * 
 * I've added code to make the radiation warning appear in the upper right corner, and more subdued, when the player
 * is actually fully immune to radiation, but is still in the radiated area.
 */

using Common;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace DeathRun.Patchers
{
    public class RadiationPatcher
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

            radiated = distanceToPlayer <= __instance.radiateRadius;
            if (radiated && flag && __instance.radiateRadius > 0f)
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


        static string immuneMessage = "Radiation (Immune)";

        /**
         * RadiationWarning update -- If we're fully immune to the radiation, display the warning in the upper right corner
         * and without animation. If we're taking damage then show the normal "center screen, pulsing, bright" message.
         */
        [HarmonyPostfix]
        public static void Update(uGUI_RadiationWarning __instance)
        {
            if (Player.main == null) return;

            // Get the background gameObject for the radiation warning
            var background = GameObject.Find("RadiationWarning").FindChild("Background");

            // Get its animation component
            Animation a = background?.GetComponent<Animation>();

            // Check if we're currently immune to the radiation
            if (Player.main.radiationAmount <= 0)
            {
                if (!immuneMessage.Equals(__instance.text.text))
                {
                    __instance.text.text = immuneMessage; // Use alternate text
                    __instance.transform.localPosition = new Vector3(720f, 550f, 0f); // Put in upper right
                    if (a != null)
                    {
                        //a.Rewind();        
                        AnimationState s = a.GetState(0);
                        s.normalizedTime = 0.25f;            // Pick out a not-too-bright, not-too-dull frame of the animation
                        a.Play();
                        a.Sample();          // Forces the pose to be calculated
                        a.Stop();            // Actually commits the pose without waiting until end of frame

                        a.enabled = false;   // Now cease looping the animation
                    }
                }
            }
            else
            {
                if (immuneMessage.Equals(__instance.text.text))
                {
                    __instance.OnLanguageChanged(); // Reset to regular all-caps message
                    __instance.transform.localPosition = new Vector3(0f, 420f, 0f); // Reset to its default position
                    if (a != null)
                    {
                        a.enabled = true;  // Resume looping the animation
                        a.Play();
                    }
                }
            }
        }

        [HarmonyPostfix]
        public static void UpdateDepth(uGUI_DepthCompass __instance)
        {
            //ErrorMessage.AddMessage("Y = " + __instance.gameObject.transform.localPosition.y);
            //ErrorMessage.AddMessage("Y = " + __instance.gameObject.transform.position.y);
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


    /**
     * RadiationFX -- shows at least some of the radiation effects even when player is immune.
     */
    [HarmonyPatch(typeof(RadiationsScreenFXController))]
    [HarmonyPatch("Update")]

    public class RadiationFXPatcher
    {
        const float MINIMUM_AMOUNT_NORMAL    = 0.1f;
        const float MINIMUM_AMOUNT_SHIP      = 0.2f;

        const float FIX_PERIOD = 5;

        [HarmonyPrefix]
        private static bool Update(RadiationsScreenFXController __instance)
        {
            if ((Player.main == null) || (LeakingRadiation.main == null))
            {
                DeathRun.saveData.playerSave.backgroundRads = 0;
                return true;
            }

            Vector3 reactorRoom = new Vector3(873.7f, 2.9f, -1.7f);
            float distance = (Player.main.transform.position - reactorRoom).magnitude; //LeakingRadiation.main.transform.position
            float backgroundRads;

            string LDBiome     = TerrainDebugGUI.main.CalculateRawBiome(Player.main);
            string PlayerBiome = Player.main.GetBiomeString();

            //ErrorMessage.AddMessage("LDBiome=" + LDBiome + "  PlayerBiome=" + PlayerBiome);


            if (RadiationUtils.isInShipsRadiation(Player.main.transform))
            {
                backgroundRads = MINIMUM_AMOUNT_SHIP;
            }
            else if (RadiationUtils.isInAnyRadiation(Player.main.transform))
            {
                backgroundRads = MINIMUM_AMOUNT_NORMAL;
            }
            else
            {
                backgroundRads = 0;
            }
            DeathRun.saveData.playerSave.backgroundRads = backgroundRads;

            // In the moments right after we fix the leaks, the visible radiation fades back a bit.
            float fixFactor = 1.0f;
            if (LeakingRadiation.main.GetNumLeaks() == 0)
            {
                if (DeathRun.saveData.playerSave.fixedRadiation == 0)
                {
                    DeathRun.saveData.playerSave.fixedRadiation = DayNightCycle.main.timePassedAsFloat;
                } 
                else if (DayNightCycle.main.timePassedAsFloat > DeathRun.saveData.playerSave.fixedRadiation + FIX_PERIOD)
                {
                    fixFactor = 0.0f;
                } 
                else
                {
                    fixFactor = 1 - ((DayNightCycle.main.timePassedAsFloat - DeathRun.saveData.playerSave.fixedRadiation) / FIX_PERIOD);
                }                
            }

            // If we're inside the ship (or near it), and radiation leaks aren't fixed yet, we show quite a bit more radiation effects
            if (fixFactor > 0)
            {
                if (LDBiome.Contains("CrashedShip")) {
                    if (LDBiome.Contains("Interior_Power") && !LDBiome.Contains("Corridor"))
                    {
                        if (Player.main.IsSwimming())
                        {
                            backgroundRads = 2.0f;
                        }
                        else
                        {
                            backgroundRads = 1.6f;
                        }
                    }
                    else if (LDBiome.Contains("PowerCorridor")) 
                    {
                        if (distance <= 32)
                        {
                            backgroundRads = 1.4f;
                        }
                        else
                        {
                            backgroundRads = 1.2f;
                        }
                    } 
                    else if (LDBiome.Contains("Elevator") || LDBiome.Contains("Locker") || LDBiome.Contains("Seamoth"))
                    {
                        backgroundRads = 1.0f;
                    }
                    else if (LDBiome.Contains("Exo") || LDBiome.Contains("Living") || LDBiome.Contains("Cargo"))
                    {
                        backgroundRads = 0.8f;
                    }
                    else if (LDBiome.Contains("Entrance_03") || LDBiome.Contains("Entrance_01_01"))
                    {
                        backgroundRads = 0.7f;
                    }
                    else if (LDBiome.Contains("THallway_Lower") || LDBiome.Contains("Entrance_01"))
                    {
                        backgroundRads = 0.6f;
                    }
                    else if (LDBiome.Contains("THallway") || LDBiome.Contains("Entrance"))
                    {
                        backgroundRads = 0.5f;
                    }
                    else if (PlayerBiome.Contains("crashedShip") || PlayerBiome.Contains("generatorRoom"))
                    {
                        backgroundRads = 0.4f;
                    }
                    else if (PlayerBiome.Contains("CrashZone") || PlayerBiome.Contains("crashZone"))
                    {
                        backgroundRads = 0.3f;
                    }
                }

                backgroundRads = backgroundRads * fixFactor;
                if (backgroundRads > DeathRun.saveData.playerSave.backgroundRads)
                {
                    DeathRun.saveData.playerSave.backgroundRads = backgroundRads;
                }
            }            

            //ErrorMessage.AddMessage("Dist=" + distance + "  Rads=" + backgroundRads + "  Leaks="+ LeakingRadiation.main.GetNumLeaks());

            //CattleLogger.Message("start = " + LeakingRadiation.main.kStartRadius);
            //CattleLogger.Message("max = " + LeakingRadiation.main.kMaxRadius);

            float rads = Mathf.Max(Player.main.radiationAmount, backgroundRads);

            // If Player is naturally in at least our minimum display amount, just run normal method.
            if (Player.main.radiationAmount >= rads)
            {
                return true; 
            }

            if (rads >= __instance.prevRadiationAmount && rads > 0f)
            {
                __instance.animTime += Time.deltaTime / __instance.fadeDuration;
            }
            else
            {
                __instance.animTime -= Time.deltaTime / __instance.fadeDuration;
            }
            __instance.animTime = Mathf.Clamp01(__instance.animTime);
            __instance.fx.noiseFactor = rads * __instance.radiationMultiplier + __instance.minRadiation * __instance.animTime;
            if (__instance.fx.noiseFactor > 0f && !__instance.fx.enabled)
            {
                __instance.fx.enabled = true;
            }
            __instance.prevRadiationAmount = rads;

            return false;
        }
    }
}
