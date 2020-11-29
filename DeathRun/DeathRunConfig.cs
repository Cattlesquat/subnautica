/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 * Adapted from libraryaddict's Radiation Challenge mod -- used w/ permission.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMLHelper.V2.Json;
using SMLHelper.V2.Options.Attributes;

namespace DeathRun
{
    [Menu("Death Run")]
    public class Config : ConfigFile
    {
        public const string ALWAYS = "Always";
        public const string NEVER = "Never";
        public const string AFTER = "After Leaks Fixed";
        public const string BEFORE_AND_AFTER = "Before Radiation & After Leaks Cured";

        public const string POISONED = "Death Run (Never Breathable)";
        public const string IRRADIATED = "Hard (Pre/Post Radiation)";
        public const string BREATHABLE = "Easy (Always Breathable)";

        public const string INSANITY = "Death Run (up to x10)";
        public const string HARDCORE = "Very Hard (up to x5)";
        public const string LOVETAPS = "Love Taps (x2)";
        public const string COWARDLY = "Noob (x1)";

        public const string DEATHRUN   = "Death Run";
        public const string HARD       = "Hard";
        public const string NORMAL     = "Easy (Normal)";

        public const string AGGRESSIVE = "Death Run";

        public const string NO_VEHICLES    = "No Vehicles Challenge!";
        public const string DEATH_VEHICLES = "Death Run (exotic costs)";
        public const string HARD_VEHICLES  = "Hard (unusual costs)";

        public const string RADIATION_DEATHRUN = "Death Run (60m)";
        public const string RADIATION_HARD     = "Hard (30m)";

        public const string EXPLOSION_DEATHRUN = "Death Run (100m)";
        public const string EXPLOSION_HARD     = "Hard (50m)";

        [Choice("Damage Taken", new string[] { INSANITY, HARDCORE, LOVETAPS, COWARDLY })]
        public string damageTaken = INSANITY;

        [Choice("Surface Air", new string[] { POISONED, IRRADIATED, BREATHABLE })]
        public string surfaceAir = POISONED;

        [Choice("Radiation", new string[] { RADIATION_DEATHRUN, RADIATION_HARD, NORMAL })]
        public string radiationDepth = RADIATION_DEATHRUN;

        [Choice("Ship Explosion", new string[] { EXPLOSION_DEATHRUN, EXPLOSION_HARD, NORMAL })]
        public string explosionDepth = EXPLOSION_DEATHRUN;

        [Choice("Power Costs", new string[] { DEATHRUN, HARD, NORMAL })]
        public string powerCosts = DEATHRUN;

        [Choice("Vehicle Costs", new string[] { NO_VEHICLES, DEATH_VEHICLES, HARD_VEHICLES, NORMAL })]
        public string vehicleCosts = DEATH_VEHICLES;

        [Choice("Habitat Builder", new string[] { DEATHRUN, HARD, NORMAL })]
        public string builderCosts = DEATHRUN;

        [Choice("Creature Aggression", new string[] { AGGRESSIVE, NORMAL })]
        public string creatureAggression = AGGRESSIVE;



        [Choice("Allow Food From Island", new string[] { ALWAYS, BEFORE_AND_AFTER, AFTER, NEVER })]
        public string islandFood = "Always";

        [Toggle("Allow Specialty Air Tanks")]
        public bool enableSpecialtyTanks = false;
    }
}
