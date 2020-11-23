/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 * Adapted from Seraphim Risen's NitrogenMod, but here is where I have "made substantial changes", the Stuff I Came Here To Do:
 * 1 - All Nitrogen/Bends code moved here into this class (and mainly the Update method), so that it was easier to balance it all (e.g. all happening in the same timescale)
 * 2 - I've made some effort to make the nitrogen aspect feel a little more like my experiences in actually going SCUBA diving
 *   a - added a "velocity penalty" for ascending at fast speed (but let you come up fast enough it isn't *boring* as it can be IRL)
 *   b - the nitrogen issues follow you down into the depths, and indeed increase even more quickly there. (But obviously sparing the real-life hours-and-hours it would be)
 * 3 - Bends damage is "Damage.Starve", to bypass the "suit protection". (The suits DO help you shed the nitrogen faster and accumulate it slower, but if you get the bends, OUCH!)
 * 4 - Bends damage has some protection against "one-shotting" the player, but it's serious stuff.
 * 5 - Tried to increase the "player feedback" of both the positive and negative variety, again in ways that are at least "reminiscent" to those who have used a real dive computer.
 *   a - Nitrogen initially has a "%" value "grace period" as your dive begins, before rolling into "deco mode". 
 *   b - Warning for "ascending too fast", before the velocity penalty kicks in
 *   c - Introduction message when nitrogen first starts to accumulate
 *   d - Messages when taking Bends damage, to make clear what's happening.
 *   
 * I haven't tried to make this a "scuba diving simulator" by any means, but definitely am reaching for some "SCUBA verisimilitude" while adding challenge to the game.
 */
namespace DeathRun.Patchers
{
    using HarmonyLib;
    using UnityEngine;
    using Items;
    using NMBehaviours;
    using Common;
    using UnityStandardAssets.ImageEffects;
    using FMOD.Studio;

    [HarmonyPatch(typeof(NitrogenLevel))]
    [HarmonyPatch("Update")]
    internal class NitroDamagePatcher
    {
        private static bool lethal = true;
        private static bool cachedActive = false;
        private static bool cachedAnimating = false;
        //private static bool wasSwimming = false;
        private static int tookDamageTicks = 0;
        private static bool decompressionVehicles = false;

        private static float damageScaler = 1f;
        private static float rpgScaler = 1f;

        private static float ascentRate = 0f;

        //private static float lastAscent;

        private static int ascentWarning = 0;

        private static int oldTicks = 0;
        private static int cachedTicks = 0;

        //private static float lastNum = 0;
        
        [HarmonyPrefix]
        public static bool Prefix (ref NitrogenLevel __instance)
        {
            // Nitrogen tracking doesn't start until player leaves the pod (for underwater starts)
            if (!EscapePod.main.topHatchUsed && !EscapePod.main.bottomHatchUsed)
            {
                return false;
            }

            float depthOf = Ocean.main.GetDepthOf(Player.main.gameObject);

            //
            // NITROGEN controller
            //
            if (__instance.nitrogenEnabled && Time.timeScale > 0f)
            {                
                int  ticks = (int)(Time.time * 2);
                bool tick  = (ticks != oldTicks) && (oldTicks > 0);
                oldTicks = ticks;

                Inventory main = Inventory.main;
                TechType bodySlot = Inventory.main.equipment.GetTechTypeInSlot("Body");
                TechType headSlot = Inventory.main.equipment.GetTechTypeInSlot("Head");

                bool isSwimming = Player.main.IsSwimming();

                bool isInBase = !isSwimming && (depthOf > 10) && !GameModeUtils.RequiresOxygen();

                bool isInVehicle = Player.main.GetCurrentSub()?.isCyclops == true ||
                                   Player.main.GetVehicle() is SeaMoth ||
                                   Player.main.GetVehicle() is Exosuit;

                float ascent = __instance.GetComponent<Rigidbody>().velocity.y;  // Player's current positive Y velocity is the ascent rate (fastest seems somewhat above 6)
                ascentRate = ((ascentRate * 29) + ascent) / 30;                  // Average based on 30 frames-per-second                

                //
                // NITROGEN - Main Nitrogen adjustment calculations - run twice a second.
                //
                if (tick)
                {
                    float modifier;
                    if (isSwimming)
                    {
                        modifier = 1f;
                        if (depthOf > 0f)
                        {
                            // Increasingly better suits help lower rate of nitrogen accumulation
                            if (bodySlot == ReinforcedSuitsCore.ReinforcedSuit3ID)
                                modifier = 0.55f;
                            else if ((bodySlot == ReinforcedSuitsCore.ReinforcedSuit2ID || bodySlot == ReinforcedSuitsCore.ReinforcedStillSuit))
                                modifier = 0.75f;
                            else if (bodySlot == TechType.ReinforcedDiveSuit)
                                modifier = 0.85f;
                            else if ((bodySlot == TechType.RadiationSuit || bodySlot == TechType.Stillsuit))
                                modifier = 0.95f;
                            if (headSlot == TechType.Rebreather)
                                modifier -= 0.05f;
                        }
                    }
                    else
                    {
                        modifier = 0.5f;
                    }

                    float num = __instance.depthCurve.Evaluate(depthOf / 2048f) * 2;

                    float baselineSafe = (depthOf < 0) ? 0 : depthOf * 3 / 4; // At any given depth our safe equilibrium gradually approaches 3/4 of current depth

                    if ((baselineSafe < __instance.safeNitrogenDepth) || (__instance.safeNitrogenDepth < 10))
                    {
                        modifier = 1 / modifier; // If we're dissipating N2, don't have our high quality suit slow it down
                        if (__instance.safeNitrogenDepth < 100)
                        {
                            num = 0.05f;
                        }
                        else
                        {
                            num = 0.075f; // Intentionally more forgiving when deeper
                        }
                    }

                    __instance.safeNitrogenDepth = UWE.Utils.Slerp(__instance.safeNitrogenDepth, baselineSafe, num * __instance.kBreathScalar * modifier);

                    // This little % buffer helps introduce the concept of N2 (both initially and as a positive feedback reminder)
                    float target = ((depthOf < 10) && (__instance.safeNitrogenDepth <= 10f)) ? 0 : 100;
                    float rate;
                    if (target > 0)
                    {
                        rate = 1 + depthOf / 50f;
                    } 
                    else
                    {
                        rate = (depthOf <= 1) ? 4 : 2;
                    }

                    __instance.nitrogenLevel = UWE.Utils.Slerp(__instance.nitrogenLevel, target, rate * modifier);
                    __instance.nitrogenLevel = Mathf.Clamp(__instance.nitrogenLevel, 0, 100);

                    if (__instance.nitrogenLevel >= 100)
                    {
                        if (__instance.safeNitrogenDepth <= 10)
                        {
                            __instance.safeNitrogenDepth = 10.01f;
                        }
                    }
                }

                //
                // DAMAGE - Check if we need to take damage
                //
                if ((__instance.nitrogenLevel >= 100) && (__instance.safeNitrogenDepth >= 10f) && ((int)depthOf < (int)__instance.safeNitrogenDepth))
                {                    
                    if ((!isInVehicle && !isInBase) || !decompressionVehicles)
                    {
                        if (UnityEngine.Random.value < (isSwimming ? 0.0125f : 0.025f))
                        {
                            if ((tookDamageTicks == 0) || (ticks - tookDamageTicks > 10))
                            {
                                DecoDamage(ref __instance, depthOf);
                                tookDamageTicks = ticks;
                            }
                        }
                    }
                }
                else
                {
                    if ((__instance.nitrogenLevel <= 90) || ((depthOf <= 1) && (ascentRate < 4) && (__instance.safeNitrogenDepth < 10f)) || ((depthOf >= __instance.safeNitrogenDepth + 10) && isSwimming))
                    {
                        tookDamageTicks = 0;
                    }
                }

                //
                // ASCENT RATE - Check for ascending too quickly while swimming
                //
                if (isSwimming)
                {
                    if (ascentRate > 2)
                    {
                        if (ascentRate > 4)
                        {
                            ascentWarning++;
                            if (ascentWarning == 1)
                            {
                                DeathRunUtils.CenterMessage("Ascending too quickly!", 4);
                                ErrorMessage.AddMessage("Ascending too quickly!");
                            }
                            else if (ascentRate >= 5)
                            {
                                if (__instance.nitrogenLevel < 100)
                                {
                                    if (ascentWarning % 2 == 0)
                                    {
                                        __instance.nitrogenLevel++;
                                    }
                                }
                                else
                                {
                                    if (ascentWarning >= 60) // After about 2 seconds of too fast
                                    {
                                        if (ascentWarning % 10 == 0)
                                        {
                                            if (__instance.safeNitrogenDepth < depthOf * 1.25f)
                                            {
                                                __instance.safeNitrogenDepth += 1;
                                            }
                                        }
                                    }

                                    if ((ascentWarning % 120) == 0)
                                    {
                                        ErrorMessage.AddMessage("Ascending too quickly!");
                                        DeathRunUtils.CenterMessage("Ascending too quickly!", 4);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // if returned to slow speed then increase our buffer
                        if (ascentWarning > 0)
                        {
                            ascentWarning--;
                        }

                        // Once we've basically stopped, can do a text warning again if we get too fast
                        if (ascentRate <= 0.5f)
                        {
                            ascentWarning = 0;
                        }
                    }
                }

                //wasSwimming = isSwimming;
                //lastAscent = ascentRate;

                HUDController(__instance, (ascentRate >= 5) && (ascentWarning >= 30));
            }

            return false;
        }

        /**
         * DecoDamage - this actually applies the decompression damage from getting the bends. Includes "anti-one-shotting" protection. Also
         * resets the "safe depth" higher after each shot of damage. 
         */ 
        private static void DecoDamage(ref NitrogenLevel __instance, float depthOf)
        {
            LiveMixin component = Player.main.gameObject.GetComponent<LiveMixin>();

            float damage = 20f + UnityEngine.Random.value * 20 + (__instance.safeNitrogenDepth - depthOf);

            if (damage >= component.health)
            {
                if (!lethal || (component.health > 0.1f))
                {
                    damage = component.health - 0.05f;
                    if (damage <= 0)
                    {
                        return;
                    }
                }
            }

            if (component.health - damage > 0f)
            {
                ErrorMessage.AddMessage("You have the bends from ascending too quickly!");
                DeathRunUtils.CenterMessage("You have the bends!", 4);
            }
            else 
            {
                ErrorMessage.AddMessage("You died of the bends!");
                DeathRunUtils.CenterMessage("You died of the bends!", 5);
            }

            component.TakeDamage(damage, default, DamageType.Starve, null);

            __instance.safeNitrogenDepth = Mathf.Clamp(__instance.safeNitrogenDepth * 3 / 4, depthOf > 10 ? depthOf : 10, __instance.safeNitrogenDepth);
        }

        private static bool DecompressionSub(Player main)
        {
            bool isInSub = false;
            return isInSub;
        }

        public static void Lethality(bool isLethal)
        {
            lethal = isLethal;
        }

        public static void AdjustScaler(float val)
        {
            damageScaler = val;
        }

        public static void SetDecomVeh(bool val)
        {
            decompressionVehicles = val;
        }

        public static void AdjustRPGScaler(float val)
        {
            rpgScaler = val; // For RPG Mod
        }

        /**
         * HUDController - HUD now uses its stages in order to give a gradual feedback:
         * 
         * (1) "Off" when no Nitrogen at all
         * (2) "Percent" when accumulating Nitrogen but still within "grace period"
         * (3) "Safe Depth" showing when we have deco obligations but are within safe depth
         * (4) "Flashing" when we are violating safe depth (and eligible to take damage imminently)
         */ 
        private static void HUDController(NitrogenLevel nitrogenInstance, bool forceFlash)
        {
            if (cachedActive)
            {
                BendsHUDController.SetDepth(Mathf.RoundToInt(nitrogenInstance.safeNitrogenDepth), nitrogenInstance.nitrogenLevel);
            }

            float depthOf = Ocean.main.GetDepthOf(Player.main.gameObject);

            if (nitrogenInstance.nitrogenLevel >= 1)
            {
                BendsHUDController.SetActive(true, (nitrogenInstance.nitrogenLevel >= 100) && (nitrogenInstance.safeNitrogenDepth >= 10f));

                // If we're just starting N2 accumulation, and haven't had a warning in at least a minute, display the "intro to nitrogen" message
                if (!cachedActive && ((cachedTicks == 0) || (oldTicks - cachedTicks > 120)))
                {
                    ErrorMessage.AddMessage("The deeper you go, the faster nitrogen accumulates in your bloodstream!");
                    cachedTicks = oldTicks;
                }

                // If any nitrogen at all, turn on the display
                cachedActive = true;
            }
            else if ((nitrogenInstance.nitrogenLevel < 1) && cachedActive)
            {
                // If NO nitrogen, turn the display off
                BendsHUDController.SetActive(false, false);
                cachedActive = false;
            }

            // Flashing Red is when we're about to take damage -- either violating Safe Depth or else in "dangerously fast ascent".
            bool uhoh = ((depthOf < nitrogenInstance.safeNitrogenDepth) || forceFlash) && (nitrogenInstance.safeNitrogenDepth >= 10f) && (nitrogenInstance.nitrogenLevel >= 100);
            if (uhoh && !cachedAnimating)
            {
                BendsHUDController.SetFlashing(true);
                cachedAnimating = true;
            }
            else if (!uhoh && cachedAnimating)
            {
                BendsHUDController.SetFlashing(false);
                cachedAnimating = false;
            }
        }
    }

    [HarmonyPatch(typeof(NitrogenLevel))]
    [HarmonyPatch("Start")]
    internal class NitroStartPatcher
    {
        [HarmonyPostfix]
        public static void Postfix(ref NitrogenLevel __instance)
        {
            __instance.nitrogenEnabled = Main.nitrogenEnabled;
            __instance.safeNitrogenDepth = 0f;
            __instance.nitrogenLevel = 0f;
            
            if (Main.specialtyTanks)
                Player.main.gameObject.AddComponent<SpecialtyTanks>();
        }
    }

    [HarmonyPatch(typeof(NitrogenLevel))]
    [HarmonyPatch("OnRespawn")]
    internal class RespawnPatcher
    {
        [HarmonyPostfix]
        public static void Postfix(ref NitrogenLevel __instance)
        {
            __instance.safeNitrogenDepth = 0f;
            __instance.nitrogenLevel = 0f;
        }
    }
}