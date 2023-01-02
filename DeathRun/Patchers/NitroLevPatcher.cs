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
 * 6 - Moved "safeNitrogenDepth" into my save/load serialized class, because the game's one doesn't seem to save/restore from saved game files.
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
     * Data that is saved/restored with saved games is here (DeathRunPlugin.saveData.nitroSave)
     */
    public class NitroSaveData
    {
        public float safeDepth { get; set; }      // Safe depth for Nitrogen/Bends. Replaces the one in game code that isn't saved w/ saved game.
        public int oldTicks { get; set; }         // Ticks marker for our half-second tick pulse
        public int n2IntroTicks { get; set; }     // Marks tick of when we were introduced to n2 concept
        public int tookDamageTicks { get; set; }  // Marks tick of last time we took damage
        public int reallyTookDamageTicks { get; set; }  // Marks tick of last time we took damage
        public int n2WarningTicks { get; set; }   // Marks tick of last time we got N2 warning
        public int ascentWarningTicks { get; set; } // Marks tick of when an Ascent Rate warning was given

        public int ascentWarning { get; set; }    // Ascent warning accumulator
        public float ascentRate { get; set; }     // Current ascent rate
        public float pipeTime { get; set; }       // Time last got air from a pipe
        public float bubbleTime { get; set; }     // Time last got air from a bubble
        public bool atPipe { get; set; }          // True if currently considered "breathing from a pipe"

        public NitroSaveData()
        {
            setDefaults();
        }

        public void setDefaults()
        {
            oldTicks = 0;
            n2IntroTicks = 0;
            tookDamageTicks = 0;
            reallyTookDamageTicks = 0;
            n2WarningTicks = 0;
            ascentWarningTicks = 0;
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
        private static bool cachedActive = false;
        private static bool cachedAnimating = false;
        private static int  cachedTicks = 0;

        [HarmonyPrefix]
        public static bool Prefix (ref NitrogenLevel __instance)
        {
            if (DeathRunPlugin.murkinessDirty)
            {
                if (WaterBiomeManager.main != null)
                {
                    WaterBiomeManager.main.Rebuild();
                    DeathRunPlugin.murkinessDirty = false;
                }
            }

            // Nitrogen tracking doesn't start until player leaves the pod (for underwater starts)
            if ((EscapePod.main == null) || (!EscapePod.main.topHatchUsed && !EscapePod.main.bottomHatchUsed))
            {
                return false;
            }

            if (DeathRunPlugin.playerIsDead || (Player.main == null) || (Ocean.main == null) || (Inventory.main == null))
            {
                return false;
            }

            float depth = Ocean.GetDepthOf(Player.main.gameObject);

            // Update our deepest depth for stats
            DeathRunPlugin.saveData.runData.Deepest = Mathf.Max(DeathRunPlugin.saveData.runData.Deepest, depth);

            //
            // NITROGEN controller
            //
            if (!Config.NORMAL.Equals(DeathRunPlugin.config.nitrogenBends) && Time.timeScale > 0f)
            {                
                int  ticks = (int)(DayNightCycle.main.timePassedAsFloat * 2);
                bool tick  = (ticks != DeathRunPlugin.saveData.nitroSave.oldTicks) && (DeathRunPlugin.saveData.nitroSave.oldTicks > 0);
                DeathRunPlugin.saveData.nitroSave.oldTicks = ticks;

                Inventory main = Inventory.main;
                TechType bodySlot = Inventory.main.equipment.GetTechTypeInSlot("Body");
                TechType headSlot = Inventory.main.equipment.GetTechTypeInSlot("Head");

                bool isSwimming = Player.main.IsSwimming();

                Vehicle vehicle = Player.main.GetVehicle();

                bool isInVehicle = Player.main.GetCurrentSub()?.isCyclops == true ||
                                   vehicle is SeaMoth ||
                                   vehicle is Exosuit;

                bool isInBase = Player.main.IsInside() && !isInVehicle; //!isSwimming && (depth > 10) && !GameModeUtils.RequiresOxygen();

                bool isSeaglide = (Player.main.motorMode == Player.MotorMode.Seaglide);

                if (!isSeaglide)
                {
                    Pickupable held = Inventory.main.GetHeld();
                    if (held != null && held.gameObject.GetComponent<Seaglide>() != null)
                    {
                        isSeaglide = true;
                    }
                }

                float ascent = __instance.GetComponent<Rigidbody>().velocity.y;  // Player's current positive Y velocity is the ascent rate (fastest seems somewhat above 6)
                DeathRunPlugin.saveData.nitroSave.ascentRate = ((DeathRunPlugin.saveData.nitroSave.ascentRate * 29) + ascent) / 30;                  // Average based on 30 frames-per-second                

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
                            else if ((bodySlot == TechType.RadiationSuit || bodySlot == TechType.WaterFiltrationSuit))
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
                    if (Config.DEATHRUN.Equals(DeathRunPlugin.config.nitrogenBends))
                    {
                        baselineSafe = (depth < 0) ? 0 : depth * 3 / 4; // At any given depth our safe equilibrium gradually approaches 3/4 of current depth
                    } 
                    else
                    {
                        baselineSafe = ((depth < 0) || !isSwimming) ? 0 : depth * 3 / 4; // At any given depth our safe equilibrium gradually approaches 3/4 of current depth
                    }

                    // "Deco Module" vehicle upgrade
                    if (vehicle != null)
                    {
                        int deco = vehicle.modules.GetCount(DeathRunPlugin.decoModule.TechType);
                        if (deco > 0)
                        {
                            baselineSafe = ((deco < 2) && (DeathRunPlugin.saveData.nitroSave.safeDepth >= 10)) ? 1 : 0;
                            //CattleLogger.Message("baselineSafe = " + baselineSafe + "    deco=" + deco);
                        }
                    }

                    if (isInBase || Player.main.GetCurrentSub()?.isCyclops == true)
                    {
                        if (Inventory.main.equipment.GetCount(DeathRunPlugin.filterChip.TechType) > 0) {
                            baselineSafe = 0;
                        }
                    }

                    // Better dissipation when we're breathing through a pipe, or in a vehicle/base, or riding Seaglide, or wearing Rebreather
                    if ((baselineSafe > 0) && (DeathRunPlugin.saveData.nitroSave.atPipe || !isSwimming || isSeaglide || (headSlot == TechType.Rebreather))) 
                    {
                        float adjustment = depth * (2 + (!isSwimming ? 1 : 0) + (isSeaglide ? 1 : 0)) / 8;

                        if ((baselineSafe - adjustment <= DeathRunPlugin.saveData.nitroSave.safeDepth) && (baselineSafe > 1)) { 
                            baselineSafe = baselineSafe - adjustment;
                            if (baselineSafe < 1)
                            {
                                baselineSafe = 1;
                            }
                        }

                        if (DeathRunPlugin.saveData.nitroSave.atPipe)
                        {
                            float now = DayNightCycle.main.timePassedAsFloat;
                            if ((now > DeathRunPlugin.saveData.nitroSave.pipeTime + 1) && (now > DeathRunPlugin.saveData.nitroSave.bubbleTime + 3))
                            {
                                DeathRunPlugin.saveData.nitroSave.atPipe = false;
                            }
                        }
                    }

                    if ((baselineSafe < DeathRunPlugin.saveData.nitroSave.safeDepth) || (DeathRunPlugin.saveData.nitroSave.safeDepth < 10))
                    {
                        modifier = 1 / modifier; // If we're dissipating N2, don't have our high quality suit slow it down

                        // Intentionally more forgiving when deeper
                        num = 0.05f + (0.01f * (int)(DeathRunPlugin.saveData.nitroSave.safeDepth / 100));
                    }

                    DeathRunPlugin.saveData.nitroSave.safeDepth = UWE.Utils.Slerp(DeathRunPlugin.saveData.nitroSave.safeDepth, baselineSafe, num * __instance.kBreathScalar * modifier);

                    // This little % buffer helps introduce the concept of N2 (both initially and as a positive feedback reminder)
                    float target;
                    if (Player.main.precursorOutOfWater || (baselineSafe <= 0))
                    {
                        target = 0;
                    } else if ((DeathRunPlugin.saveData.nitroSave.safeDepth > 10f) || (depth >= 20)) {
                        target = 100;
                    }
                    else if (depth <= 10)
                    {
                        target = 0;
                    } else
                    {
                        target = (depth - 10) * 10; // Transition zone between 10m and 20m
                    }

                    float rate;
                    if (target > 0)
                    {
                        rate = 1 + depth / 50f;
                    } 
                    else
                    {
                        rate = (depth <= 1) ? 6 : (DeathRunPlugin.saveData.nitroSave.atPipe || !isSwimming) ? 4 : 2;
                    }

                    __instance.nitrogenLevel = UWE.Utils.Slerp(__instance.nitrogenLevel, target, rate * modifier);
                    __instance.nitrogenLevel = Mathf.Clamp(__instance.nitrogenLevel, 0, 100);

                    if (__instance.nitrogenLevel >= 100)
                    {
                        if (DeathRunPlugin.saveData.nitroSave.safeDepth <= 10)
                        {
                            DeathRunPlugin.saveData.nitroSave.safeDepth = 10.01f;
                        }
                    }
                }

                //
                // DAMAGE - Check if we need to take damage
                //
                if ((__instance.nitrogenLevel >= 100) && (DeathRunPlugin.saveData.nitroSave.safeDepth >= 10f) && ((int)depth < (int)DeathRunPlugin.saveData.nitroSave.safeDepth))
                {                    
                    if (!isInVehicle && !isInBase)
                    {
                        if (DeathRunPlugin.saveData.nitroSave.n2WarningTicks == 0)
                        {
                            // If we've NEVER had an N2 warning, institute a hard delay before we can take damage
                            DeathRunPlugin.saveData.nitroSave.tookDamageTicks = ticks;
                        }

                        if ((DeathRunPlugin.saveData.nitroSave.n2WarningTicks == 0) || (ticks - DeathRunPlugin.saveData.nitroSave.n2WarningTicks > 60))
                        {
                            if ((DeathRunPlugin.saveData.nitroSave.safeDepth >= 13f) || ((int)depth + 1) < (int)DeathRunPlugin.saveData.nitroSave.safeDepth) // Avoid spurious warnings right at surface
                            {
                                if (!Config.NEVER.Equals(DeathRunPlugin.config.showWarnings))
                                {
                                    if ((DeathRunPlugin.saveData.nitroSave.n2WarningTicks == 0) ||
                                        Config.WHENEVER.Equals(DeathRunPlugin.config.showWarnings) ||
                                        (Config.OCCASIONAL.Equals(DeathRunPlugin.config.showWarnings) && (ticks - DeathRunPlugin.saveData.nitroSave.n2WarningTicks > 600)))
                                    {
                                        DeathRunUtils.CenterMessage("Decompression Warning", 5);
                                        DeathRunUtils.CenterMessage("Dive to Safe Depth!", 5, 1);
                                    }
                                }
                                DeathRunPlugin.saveData.nitroSave.n2WarningTicks = ticks;
                            }
                        }

                        int danger = (int)DeathRunPlugin.saveData.nitroSave.safeDepth - (int)depth;
                        float deco = (isSwimming ? 0.0125f : 0.025f);

                        if (danger < 5) deco /= 2;
                        if (danger < 2) deco /= 2;

                        if (UnityEngine.Random.value < deco)
                        {
                            if ((DeathRunPlugin.saveData.nitroSave.tookDamageTicks == 0) || (ticks - DeathRunPlugin.saveData.nitroSave.tookDamageTicks > 10))
                            {
                                DecoDamage(ref __instance, depth, ticks);
                                DeathRunPlugin.saveData.nitroSave.tookDamageTicks = ticks;
                                DeathRunPlugin.saveData.nitroSave.reallyTookDamageTicks = ticks;
                            }
                        }
                    }
                }
                else
                {
                    if ((__instance.nitrogenLevel <= 90) || ((depth <= 1) && (DeathRunPlugin.saveData.nitroSave.ascentRate < 4) && (DeathRunPlugin.saveData.nitroSave.safeDepth < 10f)) || ((depth >= DeathRunPlugin.saveData.nitroSave.safeDepth + 10) && isSwimming))
                    {
                        DeathRunPlugin.saveData.nitroSave.tookDamageTicks = 0;
                    }
                }

                //
                // ASCENT RATE - Check for ascending too quickly while swimming
                //
                if (isSwimming)
                {
                    if (DeathRunPlugin.saveData.nitroSave.ascentRate > (isSeaglide ? 3 : 2))
                    {
                        if (DeathRunPlugin.saveData.nitroSave.ascentRate > 4)
                        {
                            DeathRunPlugin.saveData.nitroSave.ascentWarning++;
                            if (DeathRunPlugin.saveData.nitroSave.ascentWarning == 1)
                            {
                                doAscentWarning(ticks);
                            }
                            else if (DeathRunPlugin.saveData.nitroSave.ascentRate >= (isSeaglide ? 5.5 : 5))
                            {
                                if (__instance.nitrogenLevel < 100)
                                {
                                    if ((DeathRunPlugin.saveData.nitroSave.ascentWarning % (isSeaglide ? 4 : 2)) == 0)
                                    {
                                        if (((DeathRunPlugin.saveData.nitroSave.ascentWarning % 8) == 0) || Config.DEATHRUN.Equals(DeathRunPlugin.config.nitrogenBends))
                                        {
                                            __instance.nitrogenLevel++;
                                        }
                                    }
                                }
                                else
                                {
                                    if (DeathRunPlugin.saveData.nitroSave.ascentWarning >= (isSeaglide ? 90 : 60)) // After about 2 seconds of too fast
                                    {
                                        int tickrate;
                                        if (Config.DEATHRUN.Equals(DeathRunPlugin.config.nitrogenBends))
                                        {
                                            tickrate = 10;
                                        } else
                                        {
                                            tickrate = 20;
                                        }

                                        if (isSeaglide) tickrate = tickrate * 3 / 2;

                                        if (DeathRunPlugin.saveData.nitroSave.ascentWarning % tickrate == 0)
                                        {
                                            if (DeathRunPlugin.saveData.nitroSave.safeDepth < depth * 1.25f)
                                            {
                                                DeathRunPlugin.saveData.nitroSave.safeDepth += 1;                                                
                                            }
                                        }
                                    }

                                    if ((DeathRunPlugin.saveData.nitroSave.ascentWarning % 120) == 0)
                                    {
                                        doAscentWarning(ticks);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // if returned to slow speed then increase our buffer
                        if (DeathRunPlugin.saveData.nitroSave.ascentWarning > 0)
                        {
                            DeathRunPlugin.saveData.nitroSave.ascentWarning--;
                        }

                        // Once we've basically stopped, can do a text warning again if we get too fast
                        if (DeathRunPlugin.saveData.nitroSave.ascentRate <= 0.5f)
                        {
                            DeathRunPlugin.saveData.nitroSave.ascentWarning = 0;
                        }
                    }
                }

                HUDController(__instance, false); // (DeathRunPlugin.saveData.nitroSave.ascentRate >= 5) && (DeathRunPlugin.saveData.nitroSave.ascentWarning >= 30));
            } else
            {
                if (Time.timeScale > 0f)
                {
                    __instance.nitrogenLevel = 0;
                    DeathRunPlugin.saveData.nitroSave.safeDepth = 0;
                    BendsHUDController.SetActive(false, false);
                }
            }

            return false;
        }


        private static void doAscentWarning (int ticks)
        {
            if (!Config.NEVER.Equals(DeathRunPlugin.config.showWarnings))
            {
                if ((DeathRunPlugin.saveData.nitroSave.ascentWarningTicks == 0) ||
                    Config.WHENEVER.Equals(DeathRunPlugin.config.showWarnings) ||
                    (Config.OCCASIONAL.Equals(DeathRunPlugin.config.showWarnings) && (ticks - DeathRunPlugin.saveData.nitroSave.ascentWarningTicks > 600)))
                {
                    ErrorMessage.AddMessage("Ascending too quickly!");
                    DeathRunUtils.CenterMessage("Ascending too quickly!", 5);
                }
            }
            DeathRunPlugin.saveData.nitroSave.ascentWarningTicks = ticks;
        }

        /**
         * DecoDamage - this actually applies the decompression damage from getting the bends. Includes "anti-one-shotting" protection. Also
         * resets the "safe depth" higher after each shot of damage. 
         */
        private static void DecoDamage(ref NitrogenLevel __instance, float depthOf, int ticks)
        {
            LiveMixin component = Player.main.gameObject.GetComponent<LiveMixin>();

            float damageBase = (Config.DEATHRUN.Equals(DeathRunPlugin.config.nitrogenBends)) ? 20f : 10f;

            if ((DeathRunPlugin.saveData.nitroSave.safeDepth - depthOf) < 5)
            {
                damageBase /= 2;
            }

            if ((DeathRunPlugin.saveData.nitroSave.safeDepth - depthOf) < 2)
            {
                damageBase /= 2;
            }

            float damage = damageBase + UnityEngine.Random.value * damageBase + (DeathRunPlugin.saveData.nitroSave.safeDepth - depthOf);

            if (damage >= component.health)
            {
                if (component.health > 0.1f)
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
                if (!Config.NEVER.Equals(DeathRunPlugin.config.showWarnings))
                {
                    if ((DeathRunPlugin.saveData.nitroSave.reallyTookDamageTicks == 0) ||
                        Config.WHENEVER.Equals(DeathRunPlugin.config.showWarnings) ||
                        (Config.OCCASIONAL.Equals(DeathRunPlugin.config.showWarnings) && (ticks - DeathRunPlugin.saveData.nitroSave.reallyTookDamageTicks > 600)))
                    {
                        ErrorMessage.AddMessage("You have the bends from ascending too quickly!");
                        DeathRunUtils.CenterMessage("You have the bends!", 6);
                        DeathRunUtils.CenterMessage("Slow your ascent!", 6, 1);
                    }
                }
            }
            //else 
            //{
            //    ErrorMessage.AddMessage("You died of the bends!");
            //    DeathRunUtils.CenterMessage("You died of the bends!", 5);
            //}

            DeathRunPlugin.setCause("The Bends");
            component.TakeDamage(damage, default, DamageType.Starve, null);

            DeathRunPlugin.saveData.nitroSave.safeDepth = Mathf.Clamp(DeathRunPlugin.saveData.nitroSave.safeDepth * 3 / 4, depthOf > 10 ? depthOf : 10, DeathRunPlugin.saveData.nitroSave.safeDepth);
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
                BendsHUDController.SetDepth(Mathf.RoundToInt(DeathRunPlugin.saveData.nitroSave.safeDepth), nitrogenInstance.nitrogenLevel);
            }

            float depthOf = Ocean.GetDepthOf(Player.main.gameObject);

            if (nitrogenInstance.nitrogenLevel >= 1)
            {
                BendsHUDController.SetActive(true, (nitrogenInstance.nitrogenLevel >= 100) && (DeathRunPlugin.saveData.nitroSave.safeDepth >= 10f));

                // If we're just starting N2 accumulation, and haven't had a warning in at least a minute, display the "intro to nitrogen" message
                if (!cachedActive && ((cachedTicks == 0) || (DeathRunPlugin.saveData.nitroSave.oldTicks - cachedTicks > 120)))
                {
                    if (!Config.NEVER.Equals(DeathRunPlugin.config.showWarnings))
                    {
                        if ((DeathRunPlugin.saveData.nitroSave.n2IntroTicks == 0) ||
                            Config.WHENEVER.Equals(DeathRunPlugin.config.showWarnings) ||
                            (Config.OCCASIONAL.Equals(DeathRunPlugin.config.showWarnings) && (cachedTicks - DeathRunPlugin.saveData.nitroSave.n2IntroTicks > 600)))
                        {
                            ErrorMessage.AddMessage("The deeper you go, the faster nitrogen accumulates in your bloodstream!");
                            DeathRunPlugin.saveData.nitroSave.n2IntroTicks = cachedTicks;
                        }
                    }
                    
                    cachedTicks = DeathRunPlugin.saveData.nitroSave.oldTicks;
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
            bool uhoh = ((depthOf < DeathRunPlugin.saveData.nitroSave.safeDepth) || forceFlash) && (DeathRunPlugin.saveData.nitroSave.safeDepth >= 10f) && (nitrogenInstance.nitrogenLevel >= 100);
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
            __instance.nitrogenLevel = 0f;
            DeathRunPlugin.saveData.nitroSave.safeDepth = 0f;

            if (DeathRunPlugin.config.enableSpecialtyTanks)
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
            __instance.nitrogenLevel = 0f;
            DeathRunPlugin.saveData.nitroSave.safeDepth = 0f;
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
                DeathRunPlugin.saveData.nitroSave.atPipe = true;
                DeathRunPlugin.saveData.nitroSave.pipeTime = DayNightCycle.main.timePassedAsFloat;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Bubble))]
    [HarmonyPatch("OnCollisionEnter")]
    internal class BubblePatcher
    {
        [HarmonyPrefix]
        public static bool Prefix(ref Bubble __instance, Collision collisionInfo)
        {
            if (__instance.hasPopped || ((Time.time < __instance.dontPopTime) && collisionInfo.gameObject.layer != LayerMask.NameToLayer("Player")))
            {
                return true;
            }
            if ((Player.main == null) || (collisionInfo.gameObject != Player.main.gameObject)) return true;
            DeathRunPlugin.saveData.nitroSave.atPipe = true;
            DeathRunPlugin.saveData.nitroSave.bubbleTime = DayNightCycle.main.timePassedAsFloat;
            return true;
        }
    }


    /**
     * When player uses the funky elevator in the Precursor base, don't give him bends.
     */
    /*
    [HarmonyPatch(typeof(PrecursorElevator))]
    [HarmonyPatch("Update")]
    internal class PrecursorElevatorPatcher
    {
        [HarmonyPrefix]
        public static bool Prefix(ref PrecursorElevator __instance)
        {
            ErrorMessage.AddMessage("Elevator Update");
            if (__instance.elevatorPointIndex != -1)
            {
                ErrorMessage.AddMessage("Elevator Update Made Safe");
                DeathRunPlugin.saveData.nitroSave.safeDepth = 10;
            }
            return true;
        }
    }
    */


    /**
     * When player teleports, don't give him bends.
     */
    /*
    [HarmonyPatch(typeof(PrecursorElevator))]
    [HarmonyPatch("ActivateElevator")]
    internal class PrecursorElevatorActivatePatcher
    {
        [HarmonyPrefix]
        public static bool Prefix(ref PrecursorElevator __instance)
        {
            ErrorMessage.AddMessage("Activate");
            if (__instance.elevatorPointIndex == -1)
            {
                ErrorMessage.AddMessage("Activate Made Safe");
                DeathRunPlugin.saveData.nitroSave.safeDepth = 10;
            }
            return true;
        }
    }
    */



    /**
     * When player teleports, don't give him bends.
     */
    /*
    [HarmonyPatch(typeof(PrecursorElevatorTrigger))]
    [HarmonyPatch("OnTriggerEnter")]
    internal class PrecursorElevatorTriggerPatcher
    {
        [HarmonyPrefix]
        public static bool Prefix(ref PrecursorElevatorTrigger __instance, Collider col)
        {
            ErrorMessage.AddMessage("Elevator Trigger");
            if (col.gameObject.Equals(Player.main.gameObject))
            {
                ErrorMessage.AddMessage("Elevator Trigger Made Safe");
                DeathRunPlugin.saveData.nitroSave.safeDepth = 10;
            }
            return true;
        }
    }
    */


    /**
     * When player teleports, don't give him bends.
     */
    [HarmonyPatch(typeof(PrecursorTeleporter))]
    [HarmonyPatch("BeginTeleportPlayer")]
    internal class PrecursorTeleporterPatcher
    {
        [HarmonyPrefix]
        public static bool Prefix(ref PrecursorTeleporter __instance)
        {
            if (DeathRunPlugin.saveData.nitroSave.safeDepth > 10)
            {
                DeathRunPlugin.saveData.nitroSave.safeDepth = 10;
            }
            return true;
        }
    }
}