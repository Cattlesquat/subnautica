/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 */
namespace DeathRun.Patchers
{
    using HarmonyLib;
    using Items;
    using Common;
    using UnityEngine;

    [HarmonyPatch(typeof(DamageSystem))]
    [HarmonyPatch("CalculateDamage")]
    class DamageSystem_CalculateDamage_Patch
    {
        public static void Postfix(DamageType type, GameObject target, float __result)
        {
            // Heat, Pressure, Puncture, Collide, Poison, Acid, Radiation, LaserCutter, Fire, Starve
            if (target.GetComponent<Player>() || target.GetComponent<Vehicle>())
            {
                switch (type)
                {
                    case DamageType.Starve:
                        if (__result < 1) __result = 1;
                        break;

                    case DamageType.Heat:
                    case DamageType.Collide:
                        __result *= 10;
                        break;

                    //case DamageType.Pressure:

                    default:
                        __result *= 5;
                        break;
                }                
            }
        }
    }
}
