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
using DeathRun.Patchers;

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
        public const string NORMAL     = "Easy";

        public const string RANDOM     = "RANDOM";

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

        [Choice("Nitrogen and the Bends", new string[] { DEATHRUN, HARD, NORMAL })]
        public string nitrogenBends = DEATHRUN;

        [Choice("Personal Diving Depth", new string[] { DEATHRUN, HARD, NORMAL })]
        public string personalCrushDepth = DEATHRUN;

        [Choice("Ship Explosion", new string[] { EXPLOSION_DEATHRUN, EXPLOSION_HARD, NORMAL })]
        public string explosionDepth = EXPLOSION_DEATHRUN;

        [Choice("Power Costs", new string[] { DEATHRUN, HARD, NORMAL })]
        public string powerCosts = DEATHRUN;

        [Choice("Vehicle Costs", new string[] { NO_VEHICLES, DEATH_VEHICLES, HARD_VEHICLES, NORMAL })]
        public string vehicleCosts = DEATH_VEHICLES;

        [Choice("Habitat Builder", new string[] { DEATHRUN, HARD, NORMAL })]
        public string builderCosts = DEATHRUN;

        [Choice("Creature Aggression", new string[] { DEATHRUN, HARD, NORMAL })]
        public string creatureAggression = DEATHRUN;

        //FIXME - ideally this should use the values from RandomStartPatcher's "spots" List, but if I did that I wouldn't be able to
        //use this easy-to-use "[Choice(...)]" annotation, so I've just hacked this horrible thing in (where the strings need to correspond
        //precisely with the ones in the other list). If some kind soul knowledgeable in the Ways Of SMLHelper were to push a PR that did this,
        //I would gratefully merge it.
        [Choice("Start Location", new string[] { RANDOM,
            "Bullseye",
            "Cul-de-Sac",
            "Rolled In",
            "Hundred Below",
            "Very Remote",
            "Uh Oh",
            "Won't Be Easy",
            "Dramatic View!",
            "Quite Deep",
            "TOO close...?",
            "Disorienting",
            "Kelp Forest",
            "Grassy Plains",
            "Far From Kelp",
            "Crag",
            "Stinger Cave",
            "Low Copper",
            "Scarcity",
            "Buena Vista",
            "Big Wreck",
            "Deep Wreck",
            "Very Difficult!",
            "Deep Delgasi",
            "Precipice!",
            "Kelpy",
            "Deep and Unusual",
            "Jellyshroom",
            "Shallow Arch"
         })]
        public string startLocation = RANDOM;

        [Choice("Allow Food From Island", new string[] { ALWAYS, BEFORE_AND_AFTER, AFTER, NEVER })]
        public string islandFood = ALWAYS;

        [Toggle("Allow Specialty Air Tanks")]
        public bool enableSpecialtyTanks = false;
    }
}
