namespace NitrogenMod.Patchers
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
            float depthOf = Ocean.main.GetDepthOf(Player.main.gameObject);

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

                //float num = Mathf.Clamp(2f - __instance.GetComponent<Rigidbody>().velocity.magnitude, 0f, 2f) * 1f;

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
                            //if ((int)ascentRate != (int)lastAscent)
                            //{
                            //    ErrorMessage.AddMessage("Ascent " + (int)ascentRate);
                            //}
                            //lastAscent = ascentRate;

                            ascentWarning++;
                            if (ascentWarning == 1)
                            {
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

                        // Once we've stopped, can do a text warning again
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
            }
            else 
            {
                ErrorMessage.AddMessage("You died of the bends!");
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
                if (!cachedActive && ((cachedTicks == 0) || (oldTicks - cachedTicks > 120)))
                {
                    ErrorMessage.AddMessage("The deeper you go, the faster nitrogen accumulates in your bloodstream!");
                    cachedTicks = oldTicks;
                }
                cachedActive = true;
            }
            else if ((nitrogenInstance.nitrogenLevel < 1) && cachedActive)
            {
                BendsHUDController.SetActive(false, false);
                cachedActive = false;
            }

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