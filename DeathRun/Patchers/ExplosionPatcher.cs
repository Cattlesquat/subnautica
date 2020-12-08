using HarmonyLib;
/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 * Adapted from libraryaddict's Radiation Challenge mod -- used w/ permission.
 * 
 * This version simulates more of a damage shockwave down from the surface (so the deeper you are, the better), rather than
 * a hard line of dead vs. no-damage-at-all.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeathRun.Patchers
{
    public class ExplosionPatcher
    {        
        /**
         * Damage or kill the player if above a certain depth
         * 
         * Presently this is called at the moment Aurora explodes. That's not *terrible* (and it does coincide with
         * the SOUND), but it bothers me that the "visual shockwave" doesn't get there until later. We could in theory
         * intercept the player's WorldForces component, but that would both require a complex transpiler patch AND 
         * would disjoin the effect from the sound. So for now I have left this synched with the sound.
         */
        [HarmonyPostfix]
        public static void CreateExplosiveForce() // Should only be called when Aurora explodes
        {
            float depth = Ocean.main.GetDepthOf(Player.main.gameObject);
            LiveMixin component = Player.main.gameObject.GetComponent<LiveMixin>();
            if (component == null)
            {
                return;
            }

            float damage;
            if (Config.EXPLOSION_DEATHRUN.Equals(DeathRun.config.explodeDepth))
            {
                damage = 500f;
            }
            else if (Config.EXPLOSION_HARD.Equals(DeathRun.config.explodeDepth))
            {
                damage = 300f;
            }
            else
            {
                return;
            }

            if (depth > 10)
            {
                if (depth > 100) {
                    damage = 0;
                } 
                else
                {
                    damage /= (depth + 5)/10;
                }

                if (!Player.main.IsSwimming() && (depth > 0))
                {
                    damage /= 2;
                }

                if (component.TakeDamage(damage, default, DamageType.Explosive, null))
                {
                    ErrorMessage.AddMessage("You were killed by the shockwave from the Aurora's explosion.");
                }
                else
                {
                    ErrorMessage.AddMessage("You have been injured by the shockwave from the Aurora's explosion.");
                }
            }
        }
    }
}