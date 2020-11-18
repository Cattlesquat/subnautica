using HarmonyLib;
/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 * Adapted from libraryaddict's Radiation Challenge mod -- used w/ permission.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeathRun.Patchers
{
    public class PatchExplosion
    {
        /// <summary>
        /// Patch explosion to kill below a certain depth
        /// </summary>
        [HarmonyPostfix]
        public static void CreateExplosiveForce() // Should only be called when aurora explodes
        {
            if (Player.main.transform.position.y <= -Main.config.explosionDepth) // If they are below a depth of X
            {
                return;
            }

            Player.main.liveMixin.Kill(DamageType.Explosive); // They died from explosion
        }
    }
}