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
        /**
         * How deep into the ocean the radiation can EVER penetrate (1 to X)
         */

        public static float getRadiationMaxDepth()
        {
            if (Config.RADIATION_WORSE.Equals(DeathRun.config.radiationDepth))
            {
                return 200;
            }
            else if (Config.RADIATION_DEATHRUN.Equals(DeathRun.config.radiationDepth))
            {
                return 60;
            }
            else if (Config.RADIATION_HARD.Equals(DeathRun.config.radiationDepth))
            {
                return 30;
            }
            else
            {
                return 0;
            }
        }


        public static bool isRadiationFixed()
        {
            if (LeakingRadiation.main == null)
            {
                return false;
            }

            return (LeakingRadiation.main.GetNumLeaks() == 0);
        }

        /**
         * How deep into the ocean the radiation can currently penetrate (1 to X)
        */
        public static float getRadiationDepth()
        {
            // A % of how strong the radiation is compared to max radiation
            float radiationStrengthPerc = LeakingRadiation.main.currentRadius / LeakingRadiation.main.kMaxRadius;

            // If leaks are repaired, accelerate the depth phase-out
            if (isRadiationFixed())
            {
                radiationStrengthPerc *= radiationStrengthPerc;
                //radiationStrengthPerc = (float)Math.Sqrt((double)radiationStrengthPerc * radiationStrengthPerc * radiationStrengthPerc);

                //ErrorMessage.AddMessage("base: " + (LeakingRadiation.main.currentRadius / LeakingRadiation.main.kMaxRadius) +  "  percent: " + radiationStrengthPerc + "   Depth: " + (getRadiationMaxDepth() * radiationStrengthPerc));
            }

            // How deep the radiation can reach
            float depth = getRadiationMaxDepth() * radiationStrengthPerc;

            return depth;
        }

        /**
         *  @return true if world is in irradiated state (ship has exploded and there is still active radiation)
         */
        public static bool isRadiationActive()
        {
            // If LeakingRadiation isn't null, ship has exploded and radiation is enabled
            return LeakingRadiation.main != null && CrashedShipExploder.main.IsExploded() && GameModeUtils.HasRadiation() && LeakingRadiation.main.currentRadius > 1;
        }

        /**
         * @return true if the whole surface is irradiated
         */
        public static bool isSurfaceRadiationActive()
        {
            return isRadiationActive() && getRadiationDepth() > 1;
        }

        /**
         * @return true if location is inside ship's radiation radius.
         */
        public static bool isInShipsRadiation(UnityEngine.Transform transform)
        {
            return isRadiationActive() && (transform.position - LeakingRadiation.main.transform.position).magnitude <= LeakingRadiation.main.currentRadius;
        }

        /**
         * @return true if location is irradiated
         */
        public static bool isInAnyRadiation(UnityEngine.Transform transform)
        {
            if (!isRadiationActive())
            {
                return false;
            }

            float depth = getRadiationDepth();

            if (depth > 1 && transform.position.y >= -depth)
            {
                return true;
            }

            return (transform.position - LeakingRadiation.main.transform.position).magnitude <= LeakingRadiation.main.currentRadius;
        }

        /**
         * @return true if location is both (a) irradiated and (b) on the surface
         */
        public static bool isInSurfaceRadiation(UnityEngine.Transform transform)
        {
            if (!isRadiationActive())
            {
                return false;
            }

            float depth = getRadiationDepth();

            return depth > 1 && transform.position.y >= -depth;
        }

        public static string getPlayerBiome()
        {
            string s;

            AtmosphereDirector atmosphereDirector = AtmosphereDirector.main;
            if (atmosphereDirector)
            {
                string biomeOverride = atmosphereDirector.GetBiomeOverride();
                if (!string.IsNullOrEmpty(biomeOverride))
                {
                    return biomeOverride;
                }
            }
            LargeWorld largeWorld = LargeWorld.main;
            if (largeWorld && (Player.main != null))
            {
                s = largeWorld.GetBiome(Player.main.transform.position);
                if (s == null)
                {
                    return "<none>";
                } else
                {
                    return s;
                }
            }
            return "<unknown>";
        }
    }
}