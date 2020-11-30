/**
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
        public static void Postfix(DamageType type, GameObject target, float __result)
        {
            // Heat, Pressure, Puncture, Collide, Poison, Acid, Radiation, LaserCutter, Fire, Starve

            // Only increase damage for the Player and for Vehicles.
            if (target.GetComponent<Player>() || target.GetComponent<Vehicle>())
            {
                float big;
                float little;
                if (Config.INSANITY.Equals(DeathRun.config.damageTaken))
                {
                    big = 10;
                    little = 5;
                } 
                else if (Config.HARDCORE.Equals(DeathRun.config.damageTaken))
                {
                    big    = 5;
                    little = 3;
                }
                else if (Config.LOVETAPS.Equals(DeathRun.config.damageTaken))
                {
                    big = 2;
                    little = 2;
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
                        __result *= big;
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
