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
            //JustLoadedGame();
        }
    }



    /**
     * Player.Awake -- our patch changes the "tooltip" entries for existing items, in order to enhance them with our own
     * mod-related tooltip info.
     */
    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("Awake")]
    internal class PlayerAwakePatcher
    {
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
            updated = original + " Renders surface air breathable. Supplies 'trimix' or 'nitrox' as appropriate to help clean nitrogen from the bloodstream. ";
            LanguageHandler.Main.SetTechTypeTooltip(TechType.PipeSurfaceFloater, updated);

            // Base Air Pump
            original = Language.main.Get("Tooltip_PipeSurfaceFloater");
            updated = original + " Supplies 'trimix' or 'nitrox' as appropriate to help clean nitrogen from the bloodstream. ";
            LanguageHandler.Main.SetTechTypeTooltip(TechType.BasePipeConnector, updated);

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
            updated = updated.Replace("Integrity: OK", "Stabilizers: FAILED");
            LanguageHandler.SetLanguageLine("IntroEscapePod3Content", updated);

            // Update Escape pod "status screen text" for new situation
            // ... post-secondary-system fix
            original = Language.main.Get("IntroEscapePod4Content");
            updated = original.Replace("DEPLOYED", "FAILED");
            updated = updated.Replace("Integrity: OK", "Stabilizers: FAILED");
            updated = updated.Replace("Oxygen / nitrogen atmosphere", "Atmosphere: requires filtration");
            LanguageHandler.SetLanguageLine("IntroEscapePod4Content", updated);

            // Forces the language handler to restart with our updates
            Language.main.SetCurrentLanguage(Language.main.GetCurrentLanguage());
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
                    "GOOD LUCK..."
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
