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
 * Data that is saved/restored with saved games is here (DeathRun.saveData.nitroSave)
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
        public global::Utils.ScalarMonitor timeMonitor { get; set; } = new global::Utils.ScalarMonitor(0f);

        public PlayerSaveData()
        {
            setDefaults();
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
            timeMonitor.Update(0);
            timeMonitor.Update(0);
        }
    }



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
            updated = original + " Supplies 'trimix' or 'nitrox' as appropriate to help clean nitrogen from the bloodstream. ";
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

            Language.main.SetCurrentLanguage(Language.main.GetCurrentLanguage());
        }
    }



    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("Update")]
    internal class PlayerUpdatePatcher
    {
        static List<string> messages = null;

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
                DeathRun.saveData.playerSave.timeMonitor.Update(DayNightCycle.main.timePassedAsFloat);
                return;
            }

            DeathRun.saveData.playerSave.timeMonitor.Update(DayNightCycle.main.timePassedAsFloat);
            float interval = DeathRun.saveData.playerSave.timeMonitor.currValue - DeathRun.saveData.playerSave.timeMonitor.prevValue;

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
            if (DeathRun.saveData.playerSave.timeMonitor.JustWentAbove(DeathRun.saveData.playerSave.startedGame + delta))
            {
                DeathRunUtils.CenterMessage(message, 5);
            }
        }



        private static void doRespawnMessages()
        {
            if (DeathRun.saveData.playerSave.numDeaths > 1)
            {
                if (DeathRun.saveData.playerSave.timeMonitor.JustWentAbove(DeathRun.saveData.playerSave.timeOfDeath + 15))
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
        [HarmonyPrefix]
        public static void Prefix()
        {
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
            //ErrorMessage.AddMessage(text);            

            DeathRun.saveData.playerSave.killOpening = true;            
        }
    }

    [HarmonyPatch(typeof(uGUI_HardcoreGameOver))]
    [HarmonyPatch("OnSelect")]
    internal class HardcoreDeathPatcher
    {
        [HarmonyPostfix]
        public static void Postfix(uGUI_HardcoreGameOver __instance)
        {
            // Use the time-of-death message we will have just queued as our "Game Over" message.
            __instance.text.text = DeathRunUtils.centerMessages[0].textText.text + " Game Over.";
        }
    }
}
