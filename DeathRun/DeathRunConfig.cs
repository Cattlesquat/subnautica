/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 * Goals for this configuration screen:
 * (1) Very clear recommended settings (e.g. DeathRun Means DeathRun!) 
 * (2) Mod designed around the recommended settings
 * (3) Very clear names for settings/effects - no "wall of numbers"
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMLHelper.V2.Json;
using SMLHelper.V2.Options.Attributes;
using DeathRun.Patchers;
using SMLHelper.V2.Options;
using UnityEngine;

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

        public const string NO_WAY = "No Way! (up to 200% more)";
        public const string INSANITY = "Death Run (up to 100% more)";
        public const string HARDCORE = "Very Hard (up to 50% more)";
        public const string LOVETAPS = "Love Taps (up to 25% more)";
        public const string COWARDLY = "Noob (normal)";

        public const string UNPATCHED = "UNPATCHED";

        public const string EXORBITANT = "WORSE than Death Run!";

        public const string DEATHRUN = "Death Run";
        public const string HARD = "Hard";
        public const string NORMAL = "Easy";

        public const string RANDOM = "RANDOM";
        public const string BASIC_GAME = "BASIC GAME";

        public const string AGGRESSIVE = "Death Run";

        public const string NO_VEHICLES = "No Vehicles Challenge!";
        public const string DEATH_VEHICLES = "Death Run (exotic costs)";
        public const string DEATH_VEHICLES_2 = "Death Run (alternate costs)";
        public const string HARD_VEHICLES = "Hard (unusual costs)";

        public const string RADIATION_WORSE = "Apocalypse (200m)";
        public const string RADIATION_DEATHRUN = "Death Run (60m)";
        public const string RADIATION_HARD = "Hard (30m)";

        public const string FILTER_RADIATION = "Challenge - Not NEAR Aurora";
        public const string FILTER_AURORA = "Challenge - Not INSIDE Aurora";
        public const string FILTER_ANYWHERE = "Works Anywhere";

        public const string FOOD_VEGAN = "Vegan (no fish or nutrient blocks)";
        public const string FOOD_VEGETARIAN = "Vegetarian (no fish)";
        public const string FOOD_PESCATARIAN = "Radical Pescatarian (fish ONLY)";
        public const string FOOD_OMNIVORE = "Disabled";

        public const string FARMING_VERY_SLOW = "Very Slow (1/6 speed)";
        public const string FARMING_SLOW = "Very Slow (1/3 speed)";
        public const string FARMING_NORMAL = "Disabled";

        public const string NONVIOLENT = "Nonviolent (no knife damage)";
        public const string VIOLENT = "Disabled";

        public const string EXPLOSION_DEATHRUN = "Death Run (100m)";
        public const string EXPLOSION_HARD = "Hard (50m)";

        public const string TIME_RANDOM = "RANDOM";
        public const string TIME_SHORT = "Short (45 min)";
        public const string TIME_MEDIUM = "Medium (60 min)";
        public const string TIME_LONG = "Long (90 min)";

        public const string MURK_NORMAL = "Normal";
        public const string MURK_DARK = "Dark";
        public const string MURK_DARKER = "Darker";
        public const string MURK_DARKEST = "Darkest";
        public const string MURK_CLEAR = "Crazy Clear";

        public const string FX_CHERNOBYL = "Aurora-as-Chernobyl";
        public const string FX_REMINDER = "Mild Reminder";
        public const string FX_NORMAL = "Disable";

        public const string WHENEVER = "Whenever Encountered";
        public const string OCCASIONAL = "Occasional Reminders (> 5 min)";
        public const string INTRODUCTORY = "Introducing Concept Only";

        [Choice("Damage Taken", new string[] { NO_WAY, INSANITY, HARDCORE, LOVETAPS, COWARDLY }), OnChange(nameof(ChangedChoice))]
        public string damageTaken2 = INSANITY;

        [Choice("Surface Air", new string[] { POISONED, IRRADIATED, BREATHABLE }), OnChange(nameof(ChangedChoice))]
        public string surfaceAir = POISONED;

        [Choice("Radiation", new string[] { RADIATION_WORSE, RADIATION_DEATHRUN, RADIATION_HARD, NORMAL }), OnChange(nameof(ChangedChoice))]
        public string radiationDepth = RADIATION_DEATHRUN;

        [Choice("Nitrogen and the Bends", new string[] { DEATHRUN, HARD, NORMAL }), OnChange(nameof(ChangedChoice))]
        public string nitrogenBends = DEATHRUN;

        [Choice("Personal Diving Depth", new string[] { DEATHRUN, HARD, NORMAL }), OnChange(nameof(ChangedChoice))]
        public string personalCrushDepth = DEATHRUN;

        [Choice("Depth of Explosion", new string[] { EXPLOSION_DEATHRUN, EXPLOSION_HARD, NORMAL }), OnChange(nameof(ChangedChoice))]
        public string explodeDepth = EXPLOSION_DEATHRUN;

        [Choice("Explosion Time", new string[] { TIME_RANDOM, TIME_SHORT, TIME_MEDIUM, TIME_LONG }), OnChange(nameof(ChangedChoice))]
        public string explosionTime = TIME_RANDOM;

        [Choice("Power Costs", new string[] { EXORBITANT, DEATHRUN, HARD, NORMAL }), OnChange(nameof(ChangedChoice))]
        public string powerCosts = DEATHRUN;

        [Choice("Batteries", new string[] { EXORBITANT, DEATHRUN, HARD, NORMAL }), OnChange(nameof(ChangedChoice))]
        public string batteryCosts = DEATHRUN;

        [Choice("Power to Exit Vehicles", new string[] { EXORBITANT, DEATHRUN, HARD, NORMAL }), OnChange(nameof(ChangedChoice))]
        public string powerExitVehicles = DEATHRUN;

        [Choice("Vehicle Costs", new string[] { NO_VEHICLES, DEATH_VEHICLES, DEATH_VEHICLES_2, HARD_VEHICLES, NORMAL }), OnChange(nameof(ChangedChoice))]
        public string vehicleCosts = DEATH_VEHICLES;

        [Choice("Habitat Builder", new string[] { DEATHRUN, HARD, NORMAL }), OnChange(nameof(ChangedChoice))]
        public string builderCosts = DEATHRUN;

        [Choice("Scans Required", new string[] { EXORBITANT, DEATHRUN, HARD, NORMAL }), OnChange(nameof(ChangedChoice))]
        public string scansRequired = DEATHRUN;

        [Choice("Creature Aggression", new string[] { EXORBITANT, DEATHRUN, HARD, NORMAL }), OnChange(nameof(ChangedChoice))]
        public string creatureAggression = DEATHRUN;

        //FIXME - ideally this should use the values from RandomStartPatcher's "spots" List, but if I did that I wouldn't be able to
        //use this easy-to-use "[Choice(...)]" annotation, so I've just hacked this horrible thing in (where the strings need to correspond
        //precisely with the ones in the other list). If some kind soul, Knowledgeable in the Ways Of SMLHelper, were to push a PR that did this,
        //I would gratefully merge it.
        [Choice("Start Location", new string[] { RANDOM, BASIC_GAME,
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
            "Barren: Very Hard",
            "Far From Kelp",
            "Crag",
            "Stinger Cave",
            "Low Copper",
            "Scarcity",
            "Buena Vista",
            "Big Wreck",
            "Deep Wreck",
            "Very Difficult!",
            "Deep Degasi",
            "Precipice!",
            "Kelpy",
            "Deep and Unusual",
            "Jellyshroom",
            "Shallow Arch"
         })]
        public string startLocation = RANDOM;

        [Choice("Water Murkiness (Optional)", new string[] { MURK_NORMAL, MURK_DARK, MURK_DARKER, MURK_DARKEST, MURK_CLEAR }), OnChange(nameof(ChangedMurkiness))]
        public string murkiness = MURK_NORMAL;

        [Choice("Radiation FX if Immune (Optional)", new string[] { FX_CHERNOBYL, FX_REMINDER, FX_NORMAL }), OnChange(nameof(ChangedChoice))]
        public string radiationFX = FX_CHERNOBYL;

        [Choice("Filter Pump (Optional)", new string[] { FILTER_RADIATION, FILTER_AURORA, FILTER_ANYWHERE }), OnChange(nameof(ChangedChoice))]
        public string filterPumpRules = FILTER_ANYWHERE;

        [Choice("Nonviolent Challenge (Optional)", new string[] { NONVIOLENT, VIOLENT }), OnChange(nameof(ChangedChoice))]
        public string nonViolent = VIOLENT;

        [Choice("Farming Challenge (Optional)", new string[] { FARMING_VERY_SLOW, FARMING_SLOW, FARMING_NORMAL }), OnChange(nameof(ChangedChoice))]
        public string farmingChallenge = FARMING_NORMAL;

        [Choice("Food Challenge (Optional)", new string[] { FOOD_VEGAN, FOOD_VEGETARIAN, FOOD_PESCATARIAN, FOOD_OMNIVORE }), OnChange(nameof(ChangedChoice))]
        public string foodChallenge = FOOD_OMNIVORE;

        [Choice("Food From Island (Optional)", new string[] { ALWAYS, BEFORE_AND_AFTER, AFTER, NEVER })]
        public string islandFood = ALWAYS;

        [Choice("Show Bends and Air Warnings", new string[] { WHENEVER, OCCASIONAL, INTRODUCTORY, NEVER })]
        public string showWarnings = WHENEVER;

        [Toggle("Show High Score List"), OnChange(nameof(ChangedHighScores))]
        public bool showHighScores = true;


        [Toggle("Show Tips w/ High Scores"), OnChange(nameof(ChangedTips))]
        public bool showTips = true;

        [Toggle("Allow First Aid in Quickslots")]
        public bool firstAidQuickSlot = true;

        [Toggle("Don't Tip Escape Pod Over"), OnChange(nameof(ChangedTipOver))]
        public bool podStayUpright = false;

        [Toggle("Allow Specialty Air Tanks")]
        public bool enableSpecialtyTanks = false;


        private void ChangedChoice(ChoiceChangedEventArgs e)
        {
            DeathRun.configDirty = Time.time;
        }


        private void ChangedMurkiness(ChoiceChangedEventArgs e)
        {
            DeathRun.murkinessDirty = true;
            DeathRun.configDirty = Time.time;
        }


        private void ChangedHighScores (ToggleChangedEventArgs e)
        {
            if (showHighScores)
            {
                DeathRunUtils.ShowHighScores(false);
            }
            else
            {
                DeathRunUtils.HideHighScores(false);
            }
        }


        private void ChangedTips(ToggleChangedEventArgs e)
        {
            if (showHighScores)
            {
                DeathRunUtils.ShowHighScores(false);
            }
            else
            {
                DeathRunUtils.HideHighScores(false);
            }
        }


        private void ChangedTipOverX(ToggleChangedEventArgs e)
        {

        }

        private void ChangedTipOver(ToggleChangedEventArgs e)
        {
            if (DeathRun.saveData == null) return;
            if (DeathRun.saveData.podSave == null) return;
            if (DeathRun.saveData.podSave.podStraight == null) return;
            if (DeathRun.saveData.podSave.podTipped == null) return;
            if (EscapePod.main == null) return;

            if (Config.BASIC_GAME.Equals(DeathRun.config.startLocation))
            {
                return;
            }

            if (podStayUpright)
            {
                if (DeathRun.saveData.podSave.podStraight.isInitialized())
                {
                    DeathRun.saveData.podSave.podStraight.copyTo(DeathRun.saveData.podSave.podTransform);
                    DeathRun.saveData.podSave.podStraight.copyTo(EscapePod.main.transform);
                }
            } 
            else
            {
                if (DeathRun.saveData.podSave.podTipped.isInitialized())
                {
                    DeathRun.saveData.podSave.podTipped.copyTo(DeathRun.saveData.podSave.podTransform);
                    DeathRun.saveData.podSave.podTipped.copyTo(EscapePod.main.transform);
                }
            }
        }


        private int quickCheck (string setting)
        {
            if (DEATHRUN.Equals(setting) || EXORBITANT.Equals(setting))
            {
                return 2;
            } else if (HARD.Equals(setting))
            {
                return 1;
            }
            return 0;
        }

        public int countDeathRunSettings ()
        {
            int count = 0;

            if (INSANITY.Equals(damageTaken2) || NO_WAY.Equals(damageTaken2))
            {
                count += 2;
            } else if (HARDCORE.Equals(damageTaken2))
            {
                count += 1;
            }

            if (POISONED.Equals(surfaceAir))
            {
                count += 2;
            } else if (IRRADIATED.Equals(surfaceAir))
            {
                count += 1;
            }

            if (RADIATION_DEATHRUN.Equals(radiationDepth) || RADIATION_WORSE.Equals(radiationDepth))
            {
                count += 2;
            } else if (RADIATION_HARD.Equals(radiationDepth))
            {
                count += 1;
            }

            if (EXPLOSION_DEATHRUN.Equals(explodeDepth))
            {
                count += 2;
            }
            else if (EXPLOSION_HARD.Equals(explodeDepth))
            {
                count += 1;
            }


            if (DEATH_VEHICLES.Equals(vehicleCosts) || DEATH_VEHICLES_2.Equals(vehicleCosts) || NO_VEHICLES.Equals(vehicleCosts))
            {
                count += 2;
            }
            else if (HARD_VEHICLES.Equals(vehicleCosts))
            {
                count += 1;
            }

            count += quickCheck(nitrogenBends);
            count += quickCheck(personalCrushDepth);
            count += quickCheck(powerCosts);
            count += quickCheck(batteryCosts);
            count += quickCheck(scansRequired);
            count += quickCheck(powerExitVehicles);
            count += quickCheck(builderCosts);
            count += quickCheck(creatureAggression);

            if (!BASIC_GAME.Equals(startLocation))
            {
                count += 2;
            }

            return count; // of 28
        }


        public int countDeathRunBonuses()
        {
            int bonuses = 0;

            if (NO_WAY.Equals(damageTaken2))
            {
                bonuses += 2;
            }

            if (NO_VEHICLES.Equals(vehicleCosts))
            {
                bonuses += 3;
            } else if (EXORBITANT.Equals(powerExitVehicles))
            {
                bonuses++;
            }

            if (EXORBITANT.Equals(creatureAggression))
            {
                bonuses += 2;
            }

            if (EXORBITANT.Equals(scansRequired))
            {
                bonuses++;
            }

            if (EXORBITANT.Equals(batteryCosts))
            {
                bonuses++;
            }

            if (FX_CHERNOBYL.Equals(radiationFX))
            {
                bonuses++;
            }

            if (RADIATION_WORSE.Equals(radiationDepth))
            {
                bonuses++;
            }

            if (FILTER_RADIATION.Equals(filterPumpRules))
            {
                bonuses += 2;
            } else if (FILTER_AURORA.Equals(filterPumpRules))
            {
                bonuses++;
            }

            if (FOOD_VEGAN.Equals(foodChallenge))
            {
                bonuses += 1;
            } else if (FOOD_VEGETARIAN.Equals(foodChallenge))
            {
                bonuses += 1;
            } else if (FOOD_PESCATARIAN.Equals(foodChallenge))
            {
                bonuses += 1;
            }

            if (FARMING_VERY_SLOW.Equals(farmingChallenge))
            {
                bonuses += 1;
            } else if (FARMING_SLOW.Equals(farmingChallenge))
            {
                bonuses += 1;
            }

            if (MURK_DARK.Equals(murkiness))
            {
                bonuses++;
            } else if (MURK_DARKER.Equals(murkiness))
            {
                bonuses += 2;
            } else if (MURK_DARKEST.Equals(murkiness))
            {
                bonuses += 3;
            }

            return bonuses;
        }
    }
}
