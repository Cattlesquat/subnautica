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
 * I haven't tried to make this a "scuba diving simulator" by any means (for the same reason Unknown Worlds didn't, I'm sure!), 
 * but definitely am reaching for some "SCUBA verisimilitude" from my own diving experience, while adding extra challenge to the game.
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

    /**
     * Data that is saved/restored with saved games is here (DeathRun.saveData.nitroSave)
     */
    public class NitroSaveData
    {
        public int oldTicks { get; set; }
        public int tookDamageTicks { get; set; }
        public int ascentWarning { get; set; }
        public float ascentRate { get; set; }
        public float safeDepth { get; set; }
        public float pipeTime { get; set; }
        public float bubbleTime { get; set; }
        public bool atPipe { get; set; }

        public NitroSaveData()
        {
            setDefaults();
        }

        public void setDefaults()
        {
            oldTicks = 0;
            tookDamageTicks = 0;
            ascentWarning = 0;
            ascentRate = 0;
            atPipe = false;
            pipeTime = 0;
            bubbleTime = 0;
            safeDepth = 0;
        }
    }


    [HarmonyPatch(typeof(NitrogenLevel))]
    [HarmonyPatch("Update")]
    internal class NitroDamagePatcher
    {
        private static bool lethal = true;                 // true means bends can be lethal
        private static bool decompressionVehicles = false; // if true, getting into a vehicle incurs the bends

        private static bool cachedActive = false;
        private static bool cachedAnimating = false;
        private static int  cachedTicks = 0;

        [HarmonyPrefix]
        public static bool Prefix (ref NitrogenLevel __instance)
        {
            // Nitrogen tracking doesn't start until player leaves the pod (for underwater starts)
            if (!EscapePod.main.topHatchUsed && !EscapePod.main.bottomHatchUsed)
            {
                return false;
            }

            float depth = Ocean.main.GetDepthOf(Player.main.gameObject);

            //
            // NITROGEN controller
            //
            if (!Config.NORMAL.Equals(DeathRun.config.nitrogenBends) && Time.timeScale > 0f)
            {                
                int  ticks = (int)(DayNightCycle.main.timePassedAsFloat * 2);
                bool tick  = (ticks != DeathRun.saveData.nitroSave.oldTicks) && (DeathRun.saveData.nitroSave.oldTicks > 0);
                DeathRun.saveData.nitroSave.oldTicks = ticks;

                Inventory main = Inventory.main;
                TechType bodySlot = Inventory.main.equipment.GetTechTypeInSlot("Body");
                TechType headSlot = Inventory.main.equipment.GetTechTypeInSlot("Head");

                bool isSwimming = Player.main.IsSwimming();

                bool isInBase = !isSwimming && (depth > 10) && !GameModeUtils.RequiresOxygen();

                bool isInVehicle = Player.main.GetCurrentSub()?.isCyclops == true ||
                                   Player.main.GetVehicle() is SeaMoth ||
                                   Player.main.GetVehicle() is Exosuit;

                float ascent = __instance.GetComponent<Rigidbody>().velocity.y;  // Player's current positive Y velocity is the ascent rate (fastest seems somewhat above 6)
                DeathRun.saveData.nitroSave.ascentRate = ((DeathRun.saveData.nitroSave.ascentRate * 29) + ascent) / 30;                  // Average based on 30 frames-per-second                

                //
                // NITROGEN - Main Nitrogen adjustment calculations - run twice a second.
                //
                if (tick)
                {
                    float modifier;
                    if (isSwimming)
                    {
                        modifier = 1f;
                        if (depth > 0f)
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

                    float num = __instance.depthCurve.Evaluate(depth / 2048f) * 2;

                    float baselineSafe;
                    if (Config.DEATHRUN.Equals(DeathRun.config.nitrogenBends))
                    {
                        baselineSafe = (depth < 0) ? 0 : depth * 3 / 4; // At any given depth our safe equilibrium gradually approaches 3/4 of current depth
                    } else
                    {
                        baselineSafe = (depth < 0) ? 0 : depth / 2; // At any given depth our safe equilibrium gradually approaches 1/2 of current depth
                    }

                    // Better dissipation when we're breathing through a pipe, or in a vehicle/base
                    if (DeathRun.saveData.nitroSave.atPipe || !isSwimming)
                    {
                        if (baselineSafe - (depth / 4) <= __instance.safeNitrogenDepth) { 
                            baselineSafe = baselineSafe - (depth / 4);
                        }

                        if (DeathRun.saveData.nitroSave.atPipe)
                        {
                            float now = DayNightCycle.main.timePassedAsFloat;
                            if ((now > DeathRun.saveData.nitroSave.pipeTime + 1) && (now > DeathRun.saveData.nitroSave.pipeTime + 1))
                            {
                                DeathRun.saveData.nitroSave.atPipe = false;
                            }
                        }
                    }

                    if ((baselineSafe < __instance.safeNitrogenDepth) || (__instance.safeNitrogenDepth < 10))
                    {
                        modifier = 1 / modifier; // If we're dissipating N2, don't have our high quality suit slow it down

                        // Intentionally more forgiving when deeper
                        num = 0.05f + (0.01f * (int)(__instance.safeNitrogenDepth / 100));

                        //if (__instance.safeNitrogenDepth < 100)
                        //{
                        //    num = 0.05f;
                        //}
                        //else
                        //{
                        //    num = 0.075f; 
                        //}
                    }

                    __instance.safeNitrogenDepth = UWE.Utils.Slerp(__instance.safeNitrogenDepth, baselineSafe, num * __instance.kBreathScalar * modifier);

                    // This little % buffer helps introduce the concept of N2 (both initially and as a positive feedback reminder)
                    float target = ((depth < 10) && (__instance.safeNitrogenDepth <= 10f)) ? 0 : 100;
                    float rate;
                    if (target > 0)
                    {
                        rate = 1 + depth / 50f;
                    } 
                    else
                    {
                        rate = (depth <= 1) ? 4 : (DeathRun.saveData.nitroSave.atPipe || !isSwimming) ? 3 : 2;
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

                    DeathRun.saveData.nitroSave.safeDepth = __instance.safeNitrogenDepth;
                }

                //
                // DAMAGE - Check if we need to take damage
                //
                if ((__instance.nitrogenLevel >= 100) && (__instance.safeNitrogenDepth >= 10f) && ((int)depth < (int)__instance.safeNitrogenDepth))
                {                    
                    if ((!isInVehicle && !isInBase) || !decompressionVehicles)
                    {
                        if (UnityEngine.Random.value < (isSwimming ? 0.0125f : 0.025f))
                        {
                            if ((DeathRun.saveData.nitroSave.tookDamageTicks == 0) || (ticks - DeathRun.saveData.nitroSave.tookDamageTicks > 10))
                            {
                                DecoDamage(ref __instance, depth);
                                DeathRun.saveData.nitroSave.tookDamageTicks = ticks;
                            }
                        }
                    }
                }
                else
                {
                    if ((__instance.nitrogenLevel <= 90) || ((depth <= 1) && (DeathRun.saveData.nitroSave.ascentRate < 4) && (__instance.safeNitrogenDepth < 10f)) || ((depth >= __instance.safeNitrogenDepth + 10) && isSwimming))
                    {
                        DeathRun.saveData.nitroSave.tookDamageTicks = 0;
                    }
                }

                //
                // ASCENT RATE - Check for ascending too quickly while swimming
                //
                if (isSwimming)
                {
                    if (DeathRun.saveData.nitroSave.ascentRate > 2)
                    {
                        if (DeathRun.saveData.nitroSave.ascentRate > 4)
                        {
                            DeathRun.saveData.nitroSave.ascentWarning++;
                            if (DeathRun.saveData.nitroSave.ascentWarning == 1)
                            {
                                DeathRunUtils.CenterMessage("Ascending too quickly!", 4);
                                ErrorMessage.AddMessage("Ascending too quickly!");
                            }
                            else if (DeathRun.saveData.nitroSave.ascentRate >= 5)
                            {
                                if (__instance.nitrogenLevel < 100)
                                {
                                    if (DeathRun.saveData.nitroSave.ascentWarning % 2 == 0)
                                    {
                                        if (((DeathRun.saveData.nitroSave.ascentWarning % 4) == 0) || Config.DEATHRUN.Equals(DeathRun.config.nitrogenBends))
                                        {
                                            __instance.nitrogenLevel++;
                                        }
                                    }
                                }
                                else
                                {
                                    if (DeathRun.saveData.nitroSave.ascentWarning >= 60) // After about 2 seconds of too fast
                                    {
                                        int tickrate;
                                        if (Config.DEATHRUN.Equals(DeathRun.config.nitrogenBends))
                                        {
                                            tickrate = 10;
                                        } else
                                        {
                                            tickrate = 20;
                                        }

                                        if (DeathRun.saveData.nitroSave.ascentWarning % tickrate == 0)
                                        {
                                            if (__instance.safeNitrogenDepth < depth * 1.25f)
                                            {
                                                __instance.safeNitrogenDepth += 1;
                                                DeathRun.saveData.nitroSave.safeDepth = __instance.safeNitrogenDepth;
                                            }
                                        }
                                    }

                                    if ((DeathRun.saveData.nitroSave.ascentWarning % 120) == 0)
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
                        if (DeathRun.saveData.nitroSave.ascentWarning > 0)
                        {
                            DeathRun.saveData.nitroSave.ascentWarning--;
                        }

                        // Once we've basically stopped, can do a text warning again if we get too fast
                        if (DeathRun.saveData.nitroSave.ascentRate <= 0.5f)
                        {
                            DeathRun.saveData.nitroSave.ascentWarning = 0;
                        }
                    }
                }

                HUDController(__instance, (DeathRun.saveData.nitroSave.ascentRate >= 5) && (DeathRun.saveData.nitroSave.ascentWarning >= 30));
            }

            // This helps us tell if the player is breathing from a pipe.             
            OxygenManager oxy = Player.main.gameObject.GetComponent<OxygenManager>();
            if (oxy != null)
            {
                //DeathRun.saveData.nitroSave.bubbleTime = DayNightCycle.main.timePassedAsFloat;
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

            float damageBase = (Config.DEATHRUN.Equals(DeathRun.config.nitrogenBends)) ? 20f : 10f;

            float damage = damageBase + UnityEngine.Random.value * damageBase + (__instance.safeNitrogenDepth - depthOf);

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
                if (!cachedActive && ((cachedTicks == 0) || (DeathRun.saveData.nitroSave.oldTicks - cachedTicks > 120)))
                {
                    ErrorMessage.AddMessage("The deeper you go, the faster nitrogen accumulates in your bloodstream!");
                    cachedTicks = DeathRun.saveData.nitroSave.oldTicks;
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
            __instance.nitrogenEnabled = true; 
            __instance.safeNitrogenDepth = 0f;
            __instance.nitrogenLevel = 0f;
            DeathRun.saveData.nitroSave.safeDepth = 0f;

            if (DeathRun.config.enableSpecialtyTanks)
            {
                Player.main.gameObject.AddComponent<SpecialtyTanks>();
            }
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
            DeathRun.saveData.nitroSave.safeDepth = 0f;
        }
    }


    [HarmonyPatch(typeof(OxygenArea))]
    [HarmonyPatch("OnTriggerStay")]
    internal class PipePatcher
    {
        [HarmonyPrefix]
        public static bool Prefix (Collider other)
        {
            if (other.gameObject.FindAncestor<Player>() == Utils.GetLocalPlayerComp())
            {
                DeathRun.saveData.nitroSave.atPipe = true;
                DeathRun.saveData.nitroSave.pipeTime = DayNightCycle.main.timePassedAsFloat;
            }

            return true;
        }
    }

    //[HarmonyPatch(typeof(Bubble))]
    //[HarmonyPatch("OnCollisionEnter")]
    //internal class BubblePatcher
    //{
    //    [HarmonyPrefix]
    //    public static bool Prefix(ref Bubble __instance, Collision collisionInfo)
    //    {
    //        if (__instance.hasPopped || ((Time.time < __instance.dontPopTime) && collisionInfo.gameObject.layer != LayerMask.NameToLayer("Player")))
    //        {
    //            return true;
    //        }
    //        if ((Player.main == null) || (collisionInfo.gameObject != Player.main.gameObject)) return true;
    //        DeathRun.saveData.nitroSave.atPipe = true;
    //        DeathRun.saveData.nitroSave.bubbleTime = DayNightCycle.main.timePassedAsFloat;
    //        return true;
    //    }
    //}
}