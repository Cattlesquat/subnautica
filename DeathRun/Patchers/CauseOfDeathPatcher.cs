/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 * To make this seem more like a "roguelike", I want to track "cause of death". Irritatingly, the engine makes this VERY
 * difficult because by the time something gets a "TakeDamage" it has no idea where the damage came from if it is just 
 * "DamageType.Normal" which is used for many kinds of damage. So these patches insert little prefixes to certain routines
 * that "might" cause damage, so that in case it proceeds to a kill of the player the cause is available.
 */
namespace DeathRun.Patchers
{
    using HarmonyLib;
    using UnityEngine;

    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("SuffocationDie")]
    internal class SuffocationPatch
    {
        [HarmonyPrefix]
        public static bool Prefix()
        {
            if (Player.main.IsSwimming())
            {
                DeathRun.setCause("Drowning");
            }
            else
            {
                DeathRun.setCause("Asphyxiation");
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(MeleeAttack))]
    [HarmonyPatch("OnTouch")]
    internal class MeleeAttackPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(MeleeAttack __instance)
        {
            DeathRun.setCause(DeathRun.CAUSE_UNKNOWN_CREATURE);
            DeathRun.setCauseObject(__instance.gameObject);
            return true;
        }
    }

    [HarmonyPatch(typeof(AttachAndSuck))]
    [HarmonyPatch("SuckBlood")]
    internal class SuckBloodPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(AttachAndSuck __instance)
        {
            DeathRun.setCause(DeathRun.CAUSE_UNKNOWN_CREATURE);
            DeathRun.setCauseObject(__instance.gameObject);
            return true;
        }
    }


    [HarmonyPatch(typeof(CrabsnakeMeleeAttack))]
    [HarmonyPatch("OnTouch")]
    internal class CrabsnakeMeleePatch
    {
        [HarmonyPrefix]
        public static bool Prefix(CrabsnakeMeleeAttack __instance)
        {
            DeathRun.setCause(DeathRun.CAUSE_UNKNOWN_CREATURE);
            DeathRun.setCauseObject(__instance.gameObject);
            return true;
        }
    }


    [HarmonyPatch(typeof(DamageOnPickup))]
    [HarmonyPatch("OnPickedUp")]
    internal class DamageOnPickupPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(DamageOnPickup __instance)
        {
            DeathRun.setCause(DeathRun.CAUSE_UNKNOWN_CREATURE);
            DeathRun.setCauseObject(__instance.gameObject);
            return true;
        }
    }


    [HarmonyPatch(typeof(DamageOverTime))]
    [HarmonyPatch("DoDamage")]
    internal class DamageOverTimePatch
    {
        [HarmonyPrefix]
        public static bool Prefix(DamageOverTime __instance)
        {
            DeathRun.setCause(DeathRun.CAUSE_UNKNOWN_CREATURE);
            DeathRun.setCauseObject(__instance.gameObject);
            return true;
        }
    }


    [HarmonyPatch(typeof(DamagePlayerInRadius))]
    [HarmonyPatch("DoDamage")]
    internal class DamagePlayerRadiusPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(DamagePlayerInRadius __instance)
        {
            DeathRun.setCause(DeathRun.CAUSE_UNKNOWN_CREATURE);
            DeathRun.setCauseObject(__instance.gameObject);
            return true;
        }
    }


    [HarmonyPatch(typeof(DamageSphere))]
    [HarmonyPatch("ApplyDamageEffects")]
    internal class DamageSpherePatch
    {
        [HarmonyPrefix]
        public static bool Prefix(DamageSphere __instance)
        {
            DeathRun.setCause(DeathRun.CAUSE_UNKNOWN_CREATURE);
            DeathRun.setCauseObject(__instance.gameObject);
            return true;
        }
    }


    [HarmonyPatch(typeof(DamageSystem))]
    [HarmonyPatch("RadiusDamage")]
    internal class RadiusDamagePatch
    {
        [HarmonyPrefix]
        public static bool Prefix(DamageOnPickup __instance)
        {
            DeathRun.setCause("Explosion");
            return true;
        }
    }


    [HarmonyPatch(typeof(Floater))]
    [HarmonyPatch("OnCollisionEnter")]
    internal class FloaterDamagePatch
    {
        [HarmonyPrefix]
        public static bool Prefix(DamageOnPickup __instance)
        {
            DeathRun.setCause("Falling Objects");
            return true;
        }
    }



    [HarmonyPatch(typeof(JuvenileEmperorMeleeAttack))]
    [HarmonyPatch("OnClawTouch")]
    internal class JuvenileEmperorPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(JuvenileEmperorMeleeAttack __instance)
        {
            DeathRun.setCause(DeathRun.CAUSE_UNKNOWN_CREATURE);
            DeathRun.setCauseObject(__instance.gameObject);
            return true;
        }
    }


    [HarmonyPatch(typeof(Lava))]
    [HarmonyPatch("OnTriggerStay")]
    internal class LavaStayPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(Lava __instance)
        {
            DeathRun.setCause("Lava");
            return true;
        }
    }


    [HarmonyPatch(typeof(MagmaBlob))]
    [HarmonyPatch("OnTriggerStay")]
    internal class MagmaStayPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(MagmaBlob __instance)
        {
            DeathRun.setCause("Chunk of Magma");
            return true;
        }
    }


    [HarmonyPatch(typeof(RangeAttacker))]
    [HarmonyPatch("Update")]
    internal class RangeAttackerPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(RangeAttacker __instance)
        {
            DeathRun.setCause(DeathRun.CAUSE_UNKNOWN_CREATURE);
            DeathRun.setCauseObject(__instance.gameObject);
            return true;
        }
    }


    [HarmonyPatch(typeof(ReaperMeleeAttack))]
    [HarmonyPatch("OnTouch")]
    internal class ReaperMeleePatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ReaperMeleeAttack __instance)
        {
            DeathRun.setCause(DeathRun.CAUSE_UNKNOWN_CREATURE);
            DeathRun.setCauseObject(__instance.gameObject);
            return true;
        }
    }



    [HarmonyPatch(typeof(Projectile))]
    [HarmonyPatch("OnCollisionEnter")]
    internal class ProjectilePatch
    {
        [HarmonyPrefix]
        public static bool Prefix(Projectile __instance)
        {
            DeathRun.setCause("Projectile");
            return true;
        }
    }

    [HarmonyPatch(typeof(SeaDragonMeleeAttack))]
    [HarmonyPatch("OnTouchFront")]
    internal class SeaDragonTouchFrontPatch
    {
        [HarmonyPrefix]
            public static bool Prefix(SeaDragonMeleeAttack __instance)
            {
                DeathRun.setCause(DeathRun.CAUSE_UNKNOWN_CREATURE);
                DeathRun.setCauseObject(__instance.gameObject);
                return true;
            }
        }

    [HarmonyPatch(typeof(SeaDragonMeleeAttack))]
    [HarmonyPatch("OnSwatAttackHit")]
    internal class SeaDragonSwatAttackPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(SeaDragonMeleeAttack __instance)
        {
            DeathRun.setCause(DeathRun.CAUSE_UNKNOWN_CREATURE);
            DeathRun.setCauseObject(__instance.gameObject);
            return true;
        }
    }


    [HarmonyPatch(typeof(SeaTreaderMeleeAttack))]
    [HarmonyPatch("OnLegTouch")]
    internal class SeaTreaderLegPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(SeaDragonMeleeAttack __instance)
        {
            DeathRun.setCause("Stomped By Sea Treader");
            return true;
        }
    }


    [HarmonyPatch(typeof(WarpBall))]
    [HarmonyPatch("Warp")]
    internal class WarpBallPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(WarpBall  __instance)
        {
            DeathRun.setCause("Warp Ball");
            return true;
        }
    }


    [HarmonyPatch(typeof(CyclopsDestructionEvent))]
    [HarmonyPatch("Update")]
    internal class CyclopsDestructionPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(CyclopsDestructionEvent __instance)
        {
            DeathRun.setCause("Went Down With The Cyclops");
            return true;
        }
    }


    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("OnConsoleCommand_kill")]
    internal class PlayerKill1Patch
    {
        [HarmonyPrefix]
        public static bool Prefix()
        {
            DeathRun.setCause("Deliberate Console Command");
            return true;
        }
    }


    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("OnConsoleCommand_takedamage")]
    internal class PlayerKill2Patch
    {
        [HarmonyPrefix]
        public static bool Prefix()
        {
            DeathRun.setCause("Unfortunate Console Command");
            return true;
        }
    }


    [HarmonyPatch(typeof(Rocket))]
    [HarmonyPatch("FixedUpdate")]
    internal class RocketKillPatch
    {
        [HarmonyPrefix]
        public static bool Prefix()
        {
            DeathRun.setCause("Elevator Accident");
            return true;
        }
    }





    [HarmonyPatch(typeof(Survival))]
    [HarmonyPatch("UpdateHunger")]
    internal class SurvivalUpdateHungerPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(Survival __instance)
        {
            if (__instance.water <= 0)
            {
                DeathRun.setCause("Dehydration");
            } else
            {
                DeathRun.setCause("Starvation");
            }            
            return true;
        }
    }
}
