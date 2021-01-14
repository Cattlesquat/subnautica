﻿/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 */
namespace DeathRun.Patchers
{
    using HarmonyLib;
    using Items;
    using Common;
    using UnityEngine;

    /**
     * This increases the amount of damage taken -- but only to the Player and Vehicles -- depending on type of damage.
     */
    [HarmonyPatch(typeof(DamageSystem))]
    [HarmonyPatch("CalculateDamage")]
    class DamageSystem_CalculateDamage_Patch
    {
        public static void Postfix(DamageType type, GameObject target, ref float __result)
        {
            // Heat, Pressure, Puncture, Collide, Poison, Acid, Radiation, LaserCutter, Fire, Starve

            // Only increase damage for the Player and for Vehicles.
            if (target.GetComponent<Player>() || target.GetComponent<Vehicle>())
            {
                float big;
                float little;
                if (Config.NO_WAY.Equals(DeathRun.config.damageTaken2))
                {
                    big = 3;
                    little = 2;
                }
                else if (Config.INSANITY.Equals(DeathRun.config.damageTaken2))
                {
                    big = 2;
                    little = 1.5f;
                } 
                else if (Config.HARDCORE.Equals(DeathRun.config.damageTaken2))
                {
                    big    = 1.5f;
                    little = 1.25f;
                }
                else if (Config.LOVETAPS.Equals(DeathRun.config.damageTaken2))
                {
                    big = 1.25f;
                    little = 1.10f;
                }
                else
                {
                    return;
                }

                switch (type)
                {
                    case DamageType.Starve:
                        if (__result < 1) __result = 1;
                        break;

                    case DamageType.Heat:
                        __result *= big;
                        break;

                    case DamageType.Collide:
                        __result *= big;
                        break;

                    case DamageType.Normal:
                        if ((__result < 35) || 
                            (target.GetComponent<Player>() && (__result < 70) && Player.main.HasReinforcedSuit()))
                        {
                            __result *= UnityEngine.Random.Range(little, big);
                        }
                        else 
                        {
                            __result *= little;
                        }
                        break;

                    case DamageType.Poison:
                        __result *= little;
                        break;

                    case DamageType.Acid:
                    case DamageType.Explosive:
                    case DamageType.Pressure:
                    case DamageType.Puncture:
                    default:
                        __result *= little;
                        break;
                }                
            }
        }
    }
}
