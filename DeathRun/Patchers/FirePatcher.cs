/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 * Food challenges
 */
namespace DeathRun.Patchers
{
    using HarmonyLib;
    using Items;
    using Common;
    using UnityEngine;

    [HarmonyPatch(typeof(SubFire))]
    [HarmonyPatch("CreateFireChance")]
    internal class FirePatcher
    {
        [HarmonyPostfix]
        public static void Postfix(ref SubFire __instance, ref bool __result, float chance)
        {
            float fire;
            if (Config.NO_WAY.Equals(DeathRunPlugin.config.damageTaken2))
            {
                fire = .5f;
            }
            else if (Config.INSANITY.Equals(DeathRunPlugin.config.damageTaken2))
            {
                fire = .25f;
            }
            else if (Config.HARDCORE.Equals(DeathRunPlugin.config.damageTaken2))
            {
                fire = .125f;
            } 
            else if (Config.LOVETAPS.Equals(DeathRunPlugin.config.damageTaken2))
            {
                fire = .0625f;
            } else
            {
                return;
            }
            if (chance != 2)
            {
                fire /= 2;
            }

            if (!__result && UnityEngine.Random.value < fire)
            {
                __result = true;
            }
        }
    }
}
