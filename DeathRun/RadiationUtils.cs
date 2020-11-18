/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 * Adapted from libraryaddict's Radiation Challenge mod -- used w/ permission.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeathRun
{
    public class RadiationUtils
    {
        /// <summary>
        /// How deep into the ocean the radiation can penetrate.
        ///
        /// From 1 to X
        /// </summary>
        public static float GetRadiationDepth()
        {
            return 30;

            // A % of how strong the radiation is compared to max radiation
            float radiationStrengthPerc = LeakingRadiation.main.currentRadius / LeakingRadiation.main.kMaxRadius;

            // How deep the radiation can reach
            return Main.config.radiativeDepth * radiationStrengthPerc;
        }

        public static bool GetRadiationActive()
        {
            return true;

            // If LeakingRadiation isn't null, ship has exploded and radiation is enabled
            return LeakingRadiation.main != null && CrashedShipExploder.main.IsExploded() && GameModeUtils.HasRadiation() && LeakingRadiation.main.currentRadius > 1;
        }

        public static bool GetSurfaceRadiationActive()
        {
            return true;

            return GetRadiationActive() && GetRadiationDepth() > 1;
        }

        public static bool GetInShipsRadiation(UnityEngine.Transform transform)
        {
            return GetRadiationActive() && (transform.position - LeakingRadiation.main.transform.position).magnitude <= LeakingRadiation.main.currentRadius;
        }

        public static bool GetInAnyRadiation(UnityEngine.Transform transform)
        {
            if (!GetRadiationActive())
            {
                return false;
            }

            float depth = GetRadiationDepth();

            if (depth > 1 && transform.position.y >= -depth)
            {
                return true;
            }

            return (transform.position - LeakingRadiation.main.transform.position).magnitude <= LeakingRadiation.main.currentRadius;
        }

        public static bool GetInSurfaceRadiation(UnityEngine.Transform transform)
        {
            if (!GetRadiationActive())
            {
                return false;
            }

            float depth = GetRadiationDepth();

            return depth > 1 && transform.position.y >= -depth;
        }
    }
}