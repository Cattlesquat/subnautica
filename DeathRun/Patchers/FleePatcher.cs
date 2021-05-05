/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 * Stops the "combat creatures" from running away
 */
namespace DeathRun.Patchers
{
    using HarmonyLib;
    using Items;
    using Common;
    using UnityEngine;

    //[HarmonyPatch(typeof(FleeOnDamage))]
    //[HarmonyPatch("OnTakeDamage")]
    internal class FleePatcher
    {
        //[HarmonyPrefix]
        public static bool OnTakeDamage(FleeOnDamage __instance, DamageInfo damageInfo)
        {
            return true;

            /* //Disable this for now
            if (damageInfo.type == DamageType.Electrical) return true;

            TechType t = DeathRunUtils.getTechType(__instance.gameObject);

            switch (t)
            {
                // These guys don't flee on damage anymore.
                case TechType.Stalker:
                case TechType.Crabsnake:
                case TechType.CrabSquid:
                case TechType.Sandshark:
                case TechType.BoneShark:
                case TechType.SpineEel:
                case TechType.Shocker:
                case TechType.LavaLizard:
                case TechType.Warper:
                case TechType.ReaperLeviathan:
                case TechType.GhostLeviathan:
                case TechType.GhostLeviathanJuvenile:
                case TechType.SeaDragon:
                    return false;

                default: 
                    return true;
            }
            */
        }
    }
}
