/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 * Changes item descriptions of existing items
 */
namespace DeathRun.Patchers
{
    using HarmonyLib;
    using Items;
    using Common;
    using UnityEngine;
    using SMLHelper.V2.Handlers;
    using System.Collections.Generic;
    using System;
    using UnityEngine.UI;

     /**
     * Data that is saved/restored with saved games is here (DeathRun.saveData.playerSave)
     */
    public class PlayerSaveData
    {
        public float startedGame { get; set; }
        public float timeOfDeath { get; set; }
        public float spanAtDeath { get; set; }
        public float currentLife { get; set; }
        public float allLives { get; set; }
        public int numDeaths { get; set; }
        public bool killOpening { get; set; }
        public float currTime { get; set; }
        public float prevTime { get; set; }
        public string causeOfDeath { get; set; }
        public float crushDepth { get; set; }


        public PlayerSaveData()
        {
            setDefaults();
        }

        public void AboutToSaveGame()
        {
            currTime = DeathRun.playerMonitor.currValue; // TimeMonitor doesn't seem to serialize well, so we do this.
            prevTime = DeathRun.playerMonitor.prevValue;
        }

        public void JustLoadedGame()
        {
            DeathRun.playerMonitor.currValue = currTime; // TimeMonitor doesn't seem to serialize well, so we do this.
            DeathRun.playerMonitor.prevValue = prevTime;
        }

        public void setDefaults()
        {
            startedGame = 0;
            timeOfDeath = 0;
            spanAtDeath = 0;
            currentLife = 0;
            allLives = 0;
            numDeaths = 0;
            killOpening = false;
            currTime = 0;
            prevTime = 0;
            causeOfDeath = "Unknown";
            crushDepth = 200f;
        }
    }


    /**
     * Player now respawns with less "free health" when killed.
     * 
     * Although we're patching "SuffocationReset" here, that's only because it's a convenient location that we know is called
     * AFTER the player's health has been reset. 
     */
    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("SuffocationReset")]
    internal class PlayerRespawnPatcher
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            if (Config.INSANITY.Equals(DeathRun.config.damageTaken))
            {
                Player.main.liveMixin.health = Player.main.liveMixin.maxHealth * .25f;
            }
            else if (Config.HARDCORE.Equals(DeathRun.config.damageTaken))
            {
                Player.main.liveMixin.health = Player.main.liveMixin.maxHealth * .50f;
            }
            else if (Config.LOVETAPS.Equals(DeathRun.config.damageTaken))
            {
                Player.main.liveMixin.health = Player.main.liveMixin.maxHealth * .75f;
            }
        }
    }


    /**
     * Player.Awake -- our patch changes the "tooltip" and "encyclopedia" entries for existing items, in order to enhance them with our own
     * mod-related tooltip info.
     */
    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("Awake")]
    internal class PlayerAwakePatcher
    {
        /**
         * Encyclopedia entries need to go in the prefix, because SMLHelper's EncyclopediaHandler runs in a Player postfix too.
         */
        [HarmonyPrefix]
        public static bool Prefix ()
        {
            PDAEncyclopedia.EntryData[] entryData = new PDAEncyclopedia.EntryData[] {
                new PDAEncyclopedia.EntryData
                {
                    key = "Nitrogen", nodes = new string[] { "Death" }
                },
                new PDAEncyclopedia.EntryData
                {                 
                    key = "Atmosphere", nodes = new string[] { "Death" }
                },
                new PDAEncyclopedia.EntryData
                {                 
                    key = "Radiation", nodes = new string[] { "Death" }
                },
                new PDAEncyclopedia.EntryData
                {
                    key = "Explosion", nodes = new string[] { "Death" }
                },
                new PDAEncyclopedia.EntryData
                {
                    key = "Aggression", nodes = new string[] { "Death" }
                },
                new PDAEncyclopedia.EntryData
                {
                    key = "CrushDepth", nodes = new string[] { "Death" }
                },
                new PDAEncyclopedia.EntryData
                {
                    key = "DeathRun", nodes = new string[] { "Death" }
                },
                new PDAEncyclopedia.EntryData
                {
                    key = "PowerCosts", nodes = new string[] { "Death" }
                },
                new PDAEncyclopedia.EntryData
                {
                    key = "LifePodSank", nodes = new string[] { "Death" }
                }
            };

            foreach (PDAEncyclopedia.EntryData entry in entryData)
            {
                PDAEncyclopediaHandler.AddCustomEntry(entry);
            }            

            LanguageHandler.SetLanguageLine($"EncyPath_Death", $"A DEATH RUN Primer");

            LanguageHandler.SetLanguageLine($"Ency_DeathRun", $"A Death Run Primer");
            LanguageHandler.SetLanguageLine($"EncyDesc_DeathRun", "Think you're a jaded Planet 4546B veteran? Well then try a Death Run! You will need all the skills you've learned to handle the onslaught of aggressive creatures, increased damage, the unbreathable atmosphere, nitrogen and `the bends`, the engulfing radiation, the expensive fabrication costs, and many other hazards!\n\nDeath Run is less about winning than about HOW LONG you can survive! That's why we keep a convenient `Death Timer` running at all times. Good luck!");

            LanguageHandler.SetLanguageLine($"Ency_Nitrogen", $"Nitrogen and 'The Bends'");
            LanguageHandler.SetLanguageLine($"EncyDesc_Nitrogen", "The deeper you go, the higher your blood nitrogen level will be. In Death Run your 'safe depth' will tend to settle out at about 3/4 of your current depth, so you'll want to get close to that depth without going above it, and then wait for your safe depth to improve. You must also avoid ascending too *quickly* (i.e. you can no longer just hold down the ascent button at all times). First Aid kits and floating air pumps can help with making ascents -- eating certain kinds of native life may help as well. \n\nIn real life, `The Bends`, or decompression sickness, results from nitrogen bubbles forming in the bloodstream. The deeper a diver goes, the faster nitrogen accumulates in their bloodstream. If they ascend slowly and make appropriate 'deco stops' and 'safety stops', the nitrogen is removed as they exhale. But if they ascend too quickly, the nitrogen forms bubbles which can block important blood vessels and cause death. \n\n1. Ascend slowly\n2. Watch your `Safe Depth`.");

            LanguageHandler.SetLanguageLine($"Ency_Atmosphere", $"Unbreathable Atmosphere");
            LanguageHandler.SetLanguageLine($"EncyDesc_Atmosphere", "The atmosphere of Planet 4546B is not breathable without filtering. You can use a floating air pump with pipes to filter the air sufficiently that you can breathe it.\n\n1. Surface air unbreathable\n2. A floating air pump can filter it!\n");

            LanguageHandler.SetLanguageLine($"Ency_Radiation", $"Extreme Radiation Danger");
            LanguageHandler.SetLanguageLine($"EncyDesc_Radiation", "In the event of a quantum explosion in your vicinity, be advised that all surface locations will become irradiated and will require the use of radiation suits in order to safely travel on the surface. Likewise the surface of the ocean, down to a depth of up to 60 meters, will become irradiated -- at least until any local radiation leaks are repaired.\n\n1. Surface will become irradiated\n2. Radiation as deep as 60m.\n3. Bring a Radiation Suit!");

            LanguageHandler.SetLanguageLine($"Ency_Explosion", $"Quantum Explosion Danger");
            LanguageHandler.SetLanguageLine($"EncyDesc_Explosion", "Quantum Explosions have been known to produce shockwaves up to 100m below the ocean surface. In the event of a quantum explosion in your vicinity, seek shelter immediately: as deep as possible, and preferably inside a reinforced structure of some kind.\n\n1. Explosion shockwave to 100m\n2. Take shelter as deep as possible\n3. Preferably inside a structure.");

            LanguageHandler.SetLanguageLine($"Ency_Aggression", $"Aggressive Creatures");
            LanguageHandler.SetLanguageLine($"EncyDesc_Aggression", "The native life of Planet 4546B has been reported to be EXTREMELY AGGRESSIVE and HOSTILE. While exploring the local oceans, it is best not to `hover in one place` for too long.");

            LanguageHandler.SetLanguageLine($"Ency_CrushDepth", $"Crush Depth");
            LanguageHandler.SetLanguageLine($"EncyDesc_CrushDepth", "An unaided diver on Planet 4546B should venture no deeper than 200m without either an improved diving suit or submersible support. While many types of diving suit will extend this range, reinforced suits, and in particular the Reinforced Dive Suit Mark 3, offer the best protection.\n\n1. Personal safe depth 200m.\n2. Radiation or Stillsuit 500m.\n3. Reinforced Dive Suit 800m.\n4. Scan Very Deep Creatures");

            LanguageHandler.SetLanguageLine($"Ency_PowerCosts", $"Increased Power Costs");
            LanguageHandler.SetLanguageLine($"EncyDesc_PowerCosts", "The costs of fabrication (as well as battery charging, scanning, and water filtration) have gone up dramatically following the imposition of the Galactic Fair Robotic Labor Standards Act. You may find that fabricators cost as much as 15 Standard Imperial Power Units per use. Power likewise recharges rather slowly.\n\nIn radiated areas, power usage will be even greater, as much as five times what you'd normally expect.");

            LanguageHandler.SetLanguageLine($"Ency_LifePodSank", $"Life Pod Flotation Failure");
            LanguageHandler.SetLanguageLine($"EncyDesc_LifePodSank", "Your life pod's flotation function has failed. We are sorry for the inconvenience. A refund can be requested by returning the lifepod to the nearest Alterra regional office within 48 hours of the incident.");

            string original;
            string updated;

            original = Language.main.Get("EncyDesc_Boomerang");
            updated = original + " NITROGEN PURGATIVE EFFECTS.";
            LanguageHandler.Main.SetLanguageLine("EncyDesc_Boomerang", updated);

            original = Language.main.Get("EncyDesc_LavaBoomerang");
            updated = original + " NITROGEN PURGATIVE EFFECTS.";
            LanguageHandler.Main.SetLanguageLine("EncyDesc_LavaBoomerang", updated);

            original = Language.main.Get("EncyDesc_Airpumps");
            updated = original + "\n- FILTERS CONTAMINATED SURFACE AIR";
            LanguageHandler.Main.SetLanguageLine("EncyDesc_Airpumps", updated);

            original = Language.main.Get("EncyDesc_ReinforcedSuit");
            updated = original.Replace("wearing this suit", "wearing this suit\n- Mark 2 and Mark 3 suits available\n- Scan DEEP life forms to make Mark 2 and Mark 3 suits available.");
            LanguageHandler.Main.SetLanguageLine("EncyDesc_ReinforcedSuit", updated);

            return true;
        }


        [HarmonyPostfix]
        public static void Postfix()
        {
            SeraLogger.Message(DeathRun.modName, "Item descriptions");

            // First aid kit -- Nitrogen effects
            string original = Language.main.Get("Tooltip_FirstAidKit");
            string updated = original + " Also cleans nitrogen from the bloodstream to prevent 'The Bends'.";
            LanguageHandler.Main.SetTechTypeTooltip(TechType.FirstAidKit, updated);

            // Pipe! 
            original = Language.main.Get("Tooltip_Pipe");
            updated = original + " Supplies 'trimix' or 'nitrox' as appropriate to help clean nitrogen from the bloodstream. Thus, 'Safe Depth' will decrease more quickly when breathing at a pipe.";
            LanguageHandler.Main.SetTechTypeTooltip(TechType.Pipe, updated);

            // Floating Air Pump
            original = Language.main.Get("Tooltip_PipeSurfaceFloater");
            updated = original + " Renders surface air BREATHABLE. Supplies 'trimix' or 'nitrox' as appropriate to help clean nitrogen from the bloodstream. ";
            LanguageHandler.Main.SetTechTypeTooltip(TechType.PipeSurfaceFloater, updated);

            // Base Air Pump
            original = Language.main.Get("Tooltip_PipeSurfaceFloater");
            updated = original + " Supplies 'trimix' or 'nitrox' as appropriate to help clean nitrogen from the bloodstream. ";
            LanguageHandler.Main.SetTechTypeTooltip(TechType.BasePipeConnector, updated);

            // Boomerang
            original = Language.main.Get("Tooltip_Boomerang");
            updated = original + " Seems to have unusual nitrogen-filtering blood chemistry.";
            LanguageHandler.Main.SetTechTypeTooltip(TechType.Boomerang, updated);

            // Lava Boomerang
            original = Language.main.Get("Tooltip_LavaBoomerang");
            updated = original + " Seems to have unusual nitrogen-filtering blood chemistry.";
            LanguageHandler.Main.SetTechTypeTooltip(TechType.LavaBoomerang, updated);

            // Reinforced Dive Suit - personal depth limit
            original = Language.main.Get("Tooltip_ReinforcedDiveSuit");
            if (Config.DEATHRUN.Equals(DeathRun.config.personalCrushDepth))
            {
                updated = original + " Personal depth limit 800m.";
            }
            else if (Config.HARD.Equals(DeathRun.config.personalCrushDepth))
            {
                updated = original + " Personal diving depth unlimited.";
            }
            LanguageHandler.Main.SetTechTypeTooltip(TechType.ReinforcedDiveSuit, updated);

            // Radiation Suit - personal depth limit
            original = Language.main.Get("Tooltip_RadiationSuit");
            if (!Config.NORMAL.Equals(DeathRun.config.personalCrushDepth))
            {
                updated = original + " Personal depth limit 500m.";
            }
            LanguageHandler.Main.SetTechTypeTooltip(TechType.RadiationSuit, updated);

            // StillSuit - personal depth limit
            original = Language.main.Get("Tooltip_Stillsuit");
            if (Config.DEATHRUN.Equals(DeathRun.config.personalCrushDepth))
            {
                updated = original + " Personal depth limit 800m.";
            }
            else if (Config.HARD.Equals(DeathRun.config.personalCrushDepth))
            {
                updated = original + " Personal depth limit 1300m.";
            }
            LanguageHandler.Main.SetTechTypeTooltip(TechType.Stillsuit, updated);

            // Habitat Builder if we're doing underwater explosions
            if (!Config.NORMAL.Equals(DeathRun.config.explodeDepth) || !Config.NORMAL.Equals(DeathRun.config.radiationDepth))
            {
                original = Language.main.Get("Tooltip_Builder");
                updated = original + " Build DEEP if you're expecting any big explosions or radiation!";
                LanguageHandler.Main.SetTechTypeTooltip(TechType.Builder, updated);
            }

            // Update Escape pod "status screen text" for new situation
            // ... pre-secondary-system fix
            original = Language.main.Get("IntroEscapePod3Content");
            updated = original.Replace("DEPLOYED", "FAILED");
            //updated = updated.Replace("Integrity: OK", "Stabilizers: FAILED");
            LanguageHandler.SetLanguageLine("IntroEscapePod3Content", updated);

            // Update Escape pod "status screen text" for new situation
            // ... post-secondary-system fix
            LanguageHandler.SetLanguageLine("IntroEscapePod4Header", "CONDITION YELLOW");
            original = Language.main.Get("IntroEscapePod4Content");
            updated = original.Replace("DEPLOYED", "FAILED");
            updated = updated.Replace("Hull Integrity: OK", "Inertial Stabilizers: FAILED");
            updated = updated.Replace("Uncharted ocean planet 4546B", "Planet 4546B: HOSTILE FAUNA");
            updated = updated.Replace("Oxygen/nitrogen atmosphere", "Atmosphere: requires filtration");
            LanguageHandler.SetLanguageLine("IntroEscapePod4Content", updated);

            // Forces the language handler to restart with our updates
            Language.main.SetCurrentLanguage(Language.main.GetCurrentLanguage());

            DeathRun.encyclopediaAdded = false; // Have not yet added the DeathRun encyclopedia entries
        }
    }



    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("Update")]
    internal class PlayerUpdatePatcher
    {
        static List<string> messages = null;

        /**
         * Player.Update -- Handles time-based messages (module intro messages, and death respawn messages)
         */
        [HarmonyPostfix]
        public static void Postfix()
        {
            // Don't do anything if we're holding on the "press any key to continue" screen or intro cinematic
            if (DeathRunUtils.isIntroStillGoing())
            {
                return;
            }

            if (!DeathRun.encyclopediaAdded && (DeathRun.saveData.playerSave.startedGame > 0)) 
            {
                DeathRun.encyclopediaAdded = true;
                PDAEncyclopedia.Add("DeathRun", false);
                PDAEncyclopedia.Add("Aggression", false);
                PDAEncyclopedia.Add("Atmosphere", false);
                PDAEncyclopedia.Add("CrushDepth", false);
                PDAEncyclopedia.Add("Explosion", false);
                PDAEncyclopedia.Add("Nitrogen", false);
                PDAEncyclopedia.Add("Radiation", false);
                PDAEncyclopedia.Add("PowerCosts", false);
                PDAEncyclopedia.Add("LifePodSank", false);
            }

            if (DeathRun.saveData.playerSave.startedGame == 0)
            {
                DeathRun.saveData.playerSave.startedGame = DayNightCycle.main.timePassedAsFloat;
                DeathRun.playerMonitor.Update(DayNightCycle.main.timePassedAsFloat);                
                return;
            }

            DeathRun.playerMonitor.Update(DayNightCycle.main.timePassedAsFloat);
            float interval = DeathRun.playerMonitor.currValue - DeathRun.playerMonitor.prevValue;

            // Update our "time alive"
            DeathRun.saveData.playerSave.currentLife += interval;
            DeathRun.saveData.playerSave.allLives += interval;

            // Roll the "Mod Intro" messages
            if (!DeathRun.saveData.playerSave.killOpening && (DayNightCycle.main.timePassedAsFloat - DeathRun.saveData.playerSave.startedGame < 200))
            {
                doIntroMessages();
            } else
            {
                DeathRun.saveData.playerSave.killOpening = true;
            }

            // Respawn messages
            if ((DeathRun.saveData.playerSave.timeOfDeath > 0) && ((DayNightCycle.main.timePassedAsFloat - DeathRun.saveData.playerSave.timeOfDeath < 200))) 
            {
                doRespawnMessages();
            }
        }


        private static void timedMessage(string message, float delta)
        {
            if (DeathRun.playerMonitor.JustWentAbove(DeathRun.saveData.playerSave.startedGame + delta))
            {
                DeathRunUtils.CenterMessage(message, 5);
            }
        }


        private static void doRespawnMessages()
        {
            if (DeathRun.playerMonitor.JustWentAbove(DeathRun.saveData.playerSave.timeOfDeath + 15))
            {
                ErrorMessage.AddMessage(DeathRunUtils.centerMessages[0].textText.text);

                if (DeathRun.saveData.playerSave.numDeaths > 1)
                {
                    TimeSpan timeSpan2 = TimeSpan.FromSeconds((double)DeathRun.saveData.playerSave.spanAtDeath);
                    string text;
                    if (DeathRun.saveData.playerSave.numDeaths == 2)
                    {
                        text = "Both Lives: ";
                    } 
                    else
                    {
                        text = "All " + DeathRun.saveData.playerSave.numDeaths + " Lives: ";
                    }
                    text +=  DeathRunUtils.sayTime(timeSpan2);
                    DeathRunUtils.CenterMessage(text, 10);
                }
            }

            if (DeathRun.playerMonitor.JustWentAbove(DeathRun.saveData.playerSave.timeOfDeath + 25))
            {
                TimeSpan timeSpan2 = TimeSpan.FromSeconds((double)DeathRun.saveData.playerSave.spanAtDeath);
                string text;
                if (DeathRun.saveData.playerSave.numDeaths == 2)
                {
                    text = "Both Lives: ";
                }
                else
                {
                    text = "All " + DeathRun.saveData.playerSave.numDeaths + " Lives: ";
                }
                text += DeathRunUtils.sayTime(timeSpan2);
                ErrorMessage.AddMessage(text);
            }
        }


        private static void doIntroMessages()
        {
            if (messages == null)
            {
                messages = new List<string>
                {
                    "Welcome to Death Run",
                    "Subnautica 'Roguelike' Difficulty Mod",
                    "How long can YOU Survive?",
                    "Mod by Cattlesquat",
                    "With Special Thanks To",
                    "Unknown Worlds Entertainment!",
                    "Seraphim Risen (Nitrogen Mod)",
                    "oldark (Escape Pod Unleashed)",
                    "libraryAddict (Radiation Challenge)",
                    "PrimeSonic (SML Helper, coding advice)",
                    "MrPurple6411 (SML Helper, coding advice)",
                    "AndreaDev3d (Unity help & advice)",                    
                    "Your Start Location: \"" + DeathRun.saveData.startSave.message + "\"",
                    "GOOD LUCK...",
                    "",
                    "",
                    "For Tay and Ian"
                };
            }

            float time = 10;
            foreach (string message in messages)
            {
                timedMessage(message, time);
                time += 8;
            }
        }
    }


    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("OnKill")]
    internal class PlayerKillPatcher
    {
        /**
         * Player.OnKill -- Display our "Time of Death" message to make it feel a bit more roguelike :)
         */
        [HarmonyPrefix]
        public static void Prefix(DamageType damageType)
        {
            setCauseOfDeath(damageType);

            DeathRun.saveData.playerSave.numDeaths++;

            DeathRun.saveData.playerSave.timeOfDeath = DayNightCycle.main.timePassedAsFloat;
            DeathRun.saveData.playerSave.spanAtDeath = DeathRun.saveData.playerSave.allLives;

            TimeSpan timeSpan = TimeSpan.FromSeconds((double)DeathRun.saveData.playerSave.currentLife);

            string text = "Time of Death";
            if (DeathRun.saveData.playerSave.numDeaths > 1)
            {
                text += " #" + DeathRun.saveData.playerSave.numDeaths;
            }
            text += ": ";

            text += DeathRunUtils.sayTime(timeSpan);

            DeathRunUtils.CenterMessage(text, 10);
            SeraLogger.Message(DeathRun.modName, text);

            text = "Cause of Death: " + DeathRun.cause;
            DeathRunUtils.CenterMessage(text, 10, 1);

            //ErrorMessage.AddMessage(text);            

            DeathRun.saveData.playerSave.killOpening = true;            
        }

        static void setCauseOfDeath (DamageType damageType)
        {
            switch (damageType)
            {
                case DamageType.Acid:
                    DeathRun.setCause("Acid");
                    break;

                case DamageType.Collide:
                    DeathRun.setCause("Collision");
                    break;

                case DamageType.Electrical:
                    DeathRun.setCause("Electrocution");
                    break;

                case DamageType.Explosive:
                    DeathRun.setCause("Explosion");
                    break;

                case DamageType.Fire:
                    DeathRun.setCause("Burned to Death");
                    break;

                case DamageType.Heat:
                    DeathRun.setCause("Extreme Heat");
                    break;

                case DamageType.Poison:
                    DeathRun.setCause("Poison");
                    break;

                case DamageType.Pressure:
                    DeathRun.setCause("Pressure");
                    break;

                case DamageType.Puncture:
                    DeathRun.setCause("Puncture Wounds");
                    break;

                case DamageType.Radiation:
                    DeathRun.setCause("Radiation");
                    break;

                case DamageType.Smoke:
                    DeathRun.setCause("Smoke Asphyxiation");
                    break;

                //case DamageType.Starve:
                //case DamageType.Normal:
                default:
                    if (DeathRun.CAUSE_UNKNOWN_CREATURE.Equals(DeathRun.cause))
                    {
                        if (DeathRun.causeObject != null)
                        {
                            SeraLogger.Message(DeathRun.modName, "Has an object: " + DeathRun.causeObject.name);
                            GameObject go;
                            TechType t = CraftData.GetTechType(DeathRun.causeObject, out go);
                            SeraLogger.Message(DeathRun.modName, "  TechType: " + t + " " + t.AsString());
                            
                            //PrefabIdentifier prefab = go.transform.GetComponent<PrefabIdentifier>();
                            //if (prefab != null)
                            //{
                            //    ErrorMessage.AddMessage("  Prefab: " + prefab.name + "   ClassId: " + prefab.ClassId);
                            //    t = CraftData.entClassTechTable.GetOrDefault(prefab.ClassId, TechType.None);
                            //    ErrorMessage.AddMessage("    TechType: " + t + " " + t.AsString());
                            //}

                            DeathRun.setCause(Language.main.Get(t.AsString(false)));

                            SeraLogger.Message(DeathRun.modName, "Cause of Death: " + DeathRun.cause);
                        }
                    }
                    break;

            }
        }
    }

    [HarmonyPatch(typeof(uGUI_HardcoreGameOver))]
    [HarmonyPatch("OnSelect")]
    internal class HardcoreDeathPatcher
    {
        /**
         * This makes sure the player sees the "Time of Death" when Permadeath is active.
         */
        [HarmonyPostfix]
        public static void Postfix(uGUI_HardcoreGameOver __instance)
        {
            // Use the time-of-death message we will have just queued as our "Game Over" message.
            __instance.text.text = DeathRunUtils.centerMessages[0].textText.text + " Game Over.";
        }
    }


    [HarmonyPatch(typeof(LaunchRocket))]
    [HarmonyPatch("StartEndCinematic")]
    internal class LaunchRocketPatcher
    {
        /**
         * This makes sure the player sees the "Victory" message showing length of run when the rocket launches
         */
        [HarmonyPrefix]
        public static void Prefix(uGUI_HardcoreGameOver __instance)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds((double)DeathRun.saveData.playerSave.allLives);

            string text = "Victory! In " + DeathRunUtils.sayTime(timeSpan) + " (" + (DeathRun.saveData.playerSave.numDeaths + 1) + " ";
            if (DeathRun.saveData.playerSave.numDeaths == 0)
            {
                text += "life";
            } else
            {
                text += "lives";
            }

            text += ")";

            DeathRunUtils.CenterMessage(text, 10);
            SeraLogger.Message(DeathRun.modName, text);
            //ErrorMessage.AddMessage(text);            
        }
    }
}
