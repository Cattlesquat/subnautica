/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 * From libraryaddict's Radiation Challenge mod -- used w/ permission.
 * 
 * I've added code to make the radiation warning appear in the upper right corner, and more subdued, when the player
 * is actually fully immune to radiation, but is still in the radiated area.
 */

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
}
