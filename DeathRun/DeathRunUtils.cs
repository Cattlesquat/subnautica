/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 * Utilities
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Oculus.Newtonsoft.Json;
using SMLHelper.V2.Utility;
using Common;

namespace DeathRun
{
    using global::DeathRun.Patchers;
    using UnityEngine;
    using UnityEngine.UI;

    public class DeathRunUtils
    {
        public const string VERSION = "2.2.2";

        public static CenterText[] centerMessages = new CenterText[] {
            new CenterText(250f, true),
            new CenterText(210f, true),
            new CenterText(-210f),
            new CenterText(-250f),
            new CenterText(100f)
        };

        public class CenterText : BasicText
        {
            public CenterText(float set_y) : base()
            {
                setLoc(0, set_y);
            }

            public CenterText(float set_y, bool over) : base(over)
            {
                setLoc(0, set_y);
            }
        }


        /**
         *  CenterMessage - hijacks the "intro" text item (the "press any key to begin" message) to display general "large text messages".
         */
        public static void CenterMessage(String s, float seconds)
        {
            CenterMessage(s, seconds, 0);
        }

        /**
         *  CenterMessage - hijacks the "intro" text item (the "press any key to begin" message) to display general "large text messages".
         */
        public static void CenterMessage(String s, float seconds, int index)
        {
            centerMessages[index].ShowMessage(s, seconds);
        }


        /**
         *  When just loaded a game, don't restore some previous mid-screen giant message
         */
        public static void JustLoadedGame()
        {
            foreach (CenterText ct in centerMessages)
            {
                ct.Hide();
            }
        }


        /**
         *  isIntroStillGoing() -- returns true if we're still on "press any key to continue" or during intro cinematic
         */
        public static bool isIntroStillGoing ()
        {
            // This checks if we're holding on the "press any key to continue" screen
            if (Player.main != null)
            {
                Survival surv = Player.main.GetComponent<Survival>();
                if ((surv != null) && surv.freezeStats)
                {
                    return true;
                }
            }

            // Checks if opening animation is running
            if ((EscapePod.main == null) || ((EscapePod.main.IsPlayingIntroCinematic() && EscapePod.main.IsNewBorn())))
            {
                return true;
            }
            return false;
        }

        public static bool isExplosionClockRunning()
        {
            if (DayNightCycle.main == null) return false;
            if (CrashedShipExploder.main == null) return false;

            // These are the internal parameters for the Aurora story events (see AuroraWarnings for time thresholds)
            float timeToStartWarning = CrashedShipExploder.main.GetTimeToStartWarning();
            float timeToStartCountdown = CrashedShipExploder.main.GetTimeToStartCountdown();
            float timeNow = DayNightCycle.main.timePassedAsFloat;
            
            return ((timeNow >= Mathf.Lerp(timeToStartWarning, timeToStartCountdown, 0.2f)) && (timeNow <= timeToStartCountdown + 24f));
        }


        /**
         * Returns the tech type of a Subnautica game object
         */
        public static TechType getTechType (GameObject go)
        {
            return CraftData.GetTechType(go, out go);
        }

        /**
         * Returns the "friendly name" of a Subnautica game object
         */
        public static string getFriendlyName (GameObject go)
        {
            TechType t = CraftData.GetTechType(go, out go);
            return Language.main.Get(t.AsString(false));
        }

        public static string sayTime(TimeSpan time)
        {
            string result = "";
            bool any = false;

            if (time.Days > 0)
            {
                result += time.Days + " Days";
                any = true;
            }

            if (any || (time.Hours > 0))
            {
                if (any) result += ", ";
                result += time.Hours + " Hours";
                any = true;
            }

            if (any || (time.Minutes > 0))
            {
                if (any) result += ", ";
                result += time.Minutes + " Minutes";
                any = true;
            }

            if ((time.Days == 0) && (time.Hours == 0))
            {
                if (any) result += ", ";
                result += time.Seconds + " Seconds";
            }

            return result;
        }

        public static string sayBriefTime(TimeSpan time, bool noDays)
        {
            string result = "";
            bool any = false;

            if ((time.Days > 0) && !noDays)
            {
                result += time.Days;
                any = true;
            }

            int hours = time.Hours + (noDays ? time.Days * 24 : 0);

            if (any || (hours > 0))
            {
                if (any) result += ":";
                if (any && (hours < 10)) result += "0";
                result += hours;
                any = true;
            }

            if (any) result += ":";
            if (any && (time.Minutes < 10)) result += "0";
            result += time.Minutes;
            any = true;

            if (any) result += ":";
            if (any && (time.Seconds < 10)) result += "0";
            result += time.Seconds;

            return result;
        }

        public static int HIGH_SCORE_ROWS = DeathRunStats.MAX_HIGH_SCORES + 2;

        public static BasicText highScoreLabel = new BasicText(-544,  150, 30);
        public static BasicText highScoreTag   = new BasicText(-544, -200, 20);

        public static HighScoreText[] highScoreNumbers  = new HighScoreText[HIGH_SCORE_ROWS];
        public static HighScoreText[] highScoreStarts   = new HighScoreText[HIGH_SCORE_ROWS];
        public static HighScoreText[] highScoreCauses   = new HighScoreText[HIGH_SCORE_ROWS];
        public static HighScoreText[] highScoreTimes    = new HighScoreText[HIGH_SCORE_ROWS];
        public static HighScoreText[] highScoreDeaths   = new HighScoreText[HIGH_SCORE_ROWS];
        public static HighScoreText[] highScorePercents = new HighScoreText[HIGH_SCORE_ROWS];
        public static HighScoreText[] highScoreScores   = new HighScoreText[HIGH_SCORE_ROWS];

        public static bool wouldBeShowing = false; // If true, then only reason High Scores aren't currently showing is that they are hidden by the preference

        public static List<String> tips = new List<String>() { "How long can YOU survive?",
                                                               "Did you know eating raw bladderfish yields oxygen?",
                                                               "First aid kits also purge nitrogen.",
                                                               "Eating certain kinds of native fish purges nitrogen.",
                                                               "Speak softly, but carry a floating pump.",
                                                               "Never stop moving - those things bite!",
                                                               "Keep your food and water topped up: it slowly heals you!",
                                                               "The more Death Run settings you use - the higher your score!",
                                                               "Survive longer? Higher score. Win fastest? Highest Score.",
                                                               "Hold a fish in your hand: many enemies will bite it instead of you!",
                                                               "Famous Last Words: I'll just leave my pump here for a minute.",
                                                               "You swim faster when you aren't holding something.",
                                                               "Low on air? Top off at a friendly brain coral!",
                                                               "While breathing at a pipe, your nitrogen disperses more quickly.",
                                                               "One pump, one length of pipe: Portable Breathing!",
                                                               "You can carry an extra air tank, but remember to fill it!",
                                                               "Thirsty? Cut fresh seaweed and eat immediately!",
                                                               "Repair your Escape Pod for better power generation!",
                                                               "You swim faster when on the surface.",
                                                               "Enemies only chase you if they can SEE you.",
                                                               "Hugging the ground helps hide from enemies (and Crash Fish blasts)",
                                                               "Aurora is easier to explore AFTER you repair its radiation leaks."
                                                             };

        public static void InitHighScores ()
        {
            for (int row = 0; row < HIGH_SCORE_ROWS; row++)
            {
                highScoreNumbers[row] = new HighScoreText(TextAnchor.MiddleCenter);
                highScoreStarts[row] = new HighScoreText(TextAnchor.MiddleCenter);
                highScoreCauses[row] = new HighScoreText(TextAnchor.MiddleCenter);
                highScoreTimes[row] = new HighScoreText(TextAnchor.MiddleCenter);
                highScoreDeaths[row] = new HighScoreText(TextAnchor.MiddleCenter);
                highScorePercents[row] = new HighScoreText(TextAnchor.MiddleCenter);
                highScoreScores[row] = new HighScoreText(TextAnchor.MiddleCenter);
            }
        }


        public static void ShowHighScores(bool should)
        {
            if (highScoreNumbers[0] == null)
            {
                InitHighScores();
            }

            // When "should" is false, it means we're being called because of an Options/Preference change, not a menu state change
            if (!should && !wouldBeShowing) {
                return;
            }

            wouldBeShowing = true;

            if (!DeathRun.config.showHighScores)
            {
                return;
            }

            float y = 75;
            for (int row = 0; row < HIGH_SCORE_ROWS; row++)
            {
                if (row == HIGH_SCORE_ROWS - 1)
                {
                    y = 100;
                }

                highScoreNumbers[row].setLoc(-930, y);
                highScoreStarts[row].setLoc(-825, y);
                highScoreCauses[row].setLoc(-620, y);
                highScoreTimes[row].setLoc(-465, y);
                highScoreDeaths[row].setLoc(-380, y);
                highScorePercents[row].setLoc(-280, y);
                highScoreScores[row].setLoc(-180, y);
                y -= 25;
            }

            highScoreLabel.setAlign(TextAnchor.MiddleCenter);
            highScoreLabel.ShowMessage("Death Run " + VERSION + " - Best Scores");
            highScoreTag.setAlign(TextAnchor.MiddleCenter);

            int pick;
            if (DeathRun.statsData.VeryFirstTime || !DeathRun.config.showTips)
            {
                DeathRun.statsData.VeryFirstTime = false;
                pick = 0;
            } else
            {
                pick = UnityEngine.Random.Range(0, tips.Count);
            }

            highScoreTag.ShowMessage(tips[pick]); // "How long can YOU survive?"

            int index = 0;
            foreach (RunData score in DeathRun.statsData.HighScores)
            {
                highScoreNumbers[index].ShowMessage("" + (index + 1));
                highScoreStarts[index].ShowMessage(score.Start);
                highScoreCauses[index].ShowMessage(score.Cause);

                TimeSpan t = TimeSpan.FromSeconds(score.RunTime);
                highScoreTimes[index].ShowMessage(sayBriefTime(t, true));

                highScoreDeaths[index].ShowMessage("" + score.Deaths);

                string msg = "" + (int)(((float)score.DeathRunSettingCount / (float)RunData.MAX_DEATHRUN_SETTING_COUNT) * 100) + "%";

                if (score.DeathRunSettingBonus > 0)
                {
                    msg += " +" + score.DeathRunSettingBonus;
                }

                highScorePercents[index].ShowMessage(msg);

                highScoreScores[index].ShowMessage("" + score.Score);

                if (index == DeathRun.statsData.RecentIndex)
                {
                    Color c = (index == DeathRunStats.MAX_HIGH_SCORES) ? Color.red : Color.green;
                    highScoreNumbers[index].setColor(c);
                    highScoreStarts[index].setColor(c);
                    highScoreCauses[index].setColor(c);
                    highScoreTimes[index].setColor(c);
                    highScoreDeaths[index].setColor(c);
                    highScorePercents[index].setColor(c);
                    highScoreScores[index].setColor(c);
                }

                index++;
                if (index == HIGH_SCORE_ROWS - 1)
                {
                    break;
                }
            }

            while (index < DeathRunStats.MAX_HIGH_SCORES)
            {
                highScoreNumbers[index].ShowMessage("" + (index + 1));
                index++;
            }

            highScoreNumbers[HIGH_SCORE_ROWS - 1].ShowMessage("#");
            highScoreStarts[HIGH_SCORE_ROWS - 1].ShowMessage("START");
            highScoreCauses[HIGH_SCORE_ROWS - 1].ShowMessage("CAUSE OF DEATH");
            highScoreTimes[HIGH_SCORE_ROWS - 1].ShowMessage("TIME");
            highScoreDeaths[HIGH_SCORE_ROWS - 1].ShowMessage("DEATHS");
            highScorePercents[HIGH_SCORE_ROWS - 1].ShowMessage("DIFFICULTY");
            highScoreScores[HIGH_SCORE_ROWS - 1].ShowMessage("SCORE");
        }

        public static void HideHighScores(bool should)
        {
            if (highScoreNumbers[0] == null)
            {
                InitHighScores();
            }

            // When "should" is false, it means we're being called because of an Options/Preference change, not a menu state change
            // When "should" is true, it means the actual menu state has changed
            if (should)
            {
                wouldBeShowing = false;
            }

            highScoreLabel.Hide();
            highScoreTag.Hide();
            for (int row = 0; row < HIGH_SCORE_ROWS; row++)
            {
                highScoreNumbers[row].Hide();
                highScoreStarts[row].Hide();
                highScoreCauses[row].Hide();
                highScoreTimes[row].Hide();
                highScoreDeaths[row].Hide();
                highScorePercents[row].Hide();
                highScoreScores[row].Hide();
            }
        }


        static Dictionary<string, DeathRunSaveData> saveList = new Dictionary<string, DeathRunSaveData>();

        public static void RegisterSave(string slotName, DeathRunSaveData saveData)
        {
            DeathRunSaveData already = new DeathRunSaveData();
            if (saveList.TryGetValue(slotName, out already))
            {
                saveList.Remove(slotName);
            }

            try
            {
                saveList.Add(slotName, saveData);
            }
            catch (Exception ex) 
            {
                CattleLogger.Message("Failed to add to dictionary");
                CattleLogger.GenericError(ex);
            }
        }

        public static DeathRunSaveData FindSave(string slotName)
        {
            DeathRunSaveData saveData;
            if (saveList.TryGetValue(slotName, out saveData))
            {
                return saveData;
            }
            return null;
        }
    }

    public class RunData
    {
        public int ID { get; set; }
        public string Start { get; set; }
        public string Cause { get; set; }
        public float RunTime { get; set; }
        public float Deepest { get; set; }
        public int Deaths { get; set; }
        public int Score { get; set; }
        public int BestVehicle { get; set; }
        public int VehicleFlags { get; set; }
        public int DeathRunSettingCount { get; set; }
        public int DeathRunSettingBonus { get; set; }
        public bool Victory { get; set; }
        public string Version { get; set; }

        public const int MAX_DEATHRUN_SETTING_COUNT = 28;

        // I coded these casually and left some space between them in case there's desire to hack something else in later
        public const int BEST_SEAGLIDE = 10;
        public const int BEST_SEAMOTH  = 20;
        public const int BEST_EXOSUIT  = 30;
        public const int BEST_CYCLOPS  = 40;

        public const int FLAG_SEAGLIDE = 0x01;
        public const int FLAG_SEAMOTH  = 0x02;
        public const int FLAG_EXOSUIT  = 0x04;
        public const int FLAG_CYCLOPS  = 0x08;
        public const int FLAG_HABITAT  = 0x10;
        public const int FLAG_CURE     = 0x20;
        public const int FLAG_BEACON   = 0x40;
        public const int FLAG_DIVEREEL = 0x80;
        public const int FLAG_REINFORCED = 0x100;
        public const int FLAG_RADIATION = 0x200;
        public const int FLAG_LASERCUTTER = 0x400;
        public const int FLAG_ULTRAGLIDE = 0x800;
        public const int FLAG_DOUBLETANK = 0x1000;
        public const int FLAG_PLASTEEL_TANK = 0x2000;

        public RunData()
        {
            ID = -1;
            Start = "";

            Cause = "";
            RunTime = 0;
            Deaths = 0;

            Score = 0;
            BestVehicle = 0;
            VehicleFlags = 0;
            Deepest = 0;

            DeathRunSettingCount = -1;
            DeathRunSettingBonus = -1;

            Victory = false;

            Version = "";
        }

        public void startNewRun()
        {
            ID    = DeathRun.statsData.getNewRunID();
            Start = DeathRun.saveData.startSave.message;
            Version = DeathRunUtils.VERSION;

            CattleLogger.Message("Starting new run - " + ID + " " + Start);

            countSettings();            
        }


        /**
         * Checks our "difficulty score" based on our settings (and ensures that changing settings can only lower the score)
         */
        public void countSettings()
        {
            int count   = DeathRun.config.countDeathRunSettings();
            int bonuses = DeathRun.config.countDeathRunBonuses();

            if ("".Equals(Version))
            {
                Version = DeathRunUtils.VERSION;
                DeathRunSettingCount = count;
                DeathRunSettingBonus = bonuses;
            }
            else
            {
                if ((DeathRunSettingCount < 0) || (count < DeathRunSettingCount))
                {
                    DeathRunSettingCount = count;
                }

                if ((DeathRunSettingBonus < 0) || (bonuses < DeathRunSettingBonus))
                {
                    DeathRunSettingBonus = bonuses;
                }
            }
        }


        public void updateFromSave(DeathRunSaveData saveData)
        {
            RunTime = saveData.playerSave.allLives;
            Deaths = saveData.playerSave.numDeaths + 1;
            calcScore();
        }


        public void updateVitals (bool victory)
        {
            Cause = DeathRun.cause;
            RunTime = DeathRun.saveData.playerSave.allLives;
            Deaths = DeathRun.saveData.playerSave.numDeaths;
            Victory = victory;

            countSettings();
            calcScore();

            DeathRun.statsData.addRun(this);
        }


        public void updateVehicle (int vehicle, int flag)
        {
            if (((VehicleFlags & FLAG_CURE) != FLAG_CURE) || ((flag != FLAG_SEAMOTH) && (flag != FLAG_EXOSUIT) && (flag != FLAG_CYCLOPS)))
            {
                VehicleFlags |= flag;
            }            

            if (vehicle > BestVehicle)
            {
                BestVehicle = vehicle;
            }
        }


        public int calcScore()
        {
            CattleLogger.Message("=== CALCULATING SCORE ===");

            float timeVal = 0;
            float timeLeft = RunTime;
            while (timeLeft > 0) 
            {
                if (timeLeft <= 3600)
                {
                    timeVal += timeLeft;
                    break;
                }
                else
                {
                    timeVal += 3600;
                    timeLeft = (timeLeft - 3600) / 2;
                }
            };

            CattleLogger.Message("Survival Points: " + timeVal);

            float vehicleVal = 0;
            if ((VehicleFlags & FLAG_SEAGLIDE) != 0)
            {
                vehicleVal += 1000; 
            }
            if ((VehicleFlags & FLAG_SEAMOTH) != 0)
            {
                vehicleVal += 5000;
            }
            if ((VehicleFlags & FLAG_EXOSUIT) != 0)
            {
                vehicleVal += 7500;
            }
            if ((VehicleFlags & FLAG_CYCLOPS) != 0)
            {
                vehicleVal += 15000;
            }
            if ((VehicleFlags & FLAG_HABITAT) != 0)
            {
                vehicleVal += 4000;
            }
            if ((VehicleFlags & FLAG_CURE) != 0)
            {
                vehicleVal += 25000;
            }

            if ((VehicleFlags & FLAG_BEACON) != 0)
            {
                vehicleVal += 200;
            }

            if ((VehicleFlags & FLAG_DIVEREEL) != 0)
            {
                vehicleVal += 400;
            }

            if ((VehicleFlags & FLAG_REINFORCED) != 0)
            {
                vehicleVal += 1500;
            }

            if ((VehicleFlags & FLAG_RADIATION) != 0)
            {
                vehicleVal += 300;
            }

            if ((VehicleFlags & FLAG_ULTRAGLIDE) != 0)
            {
                vehicleVal += 500;
            }

            if ((VehicleFlags & FLAG_DOUBLETANK) != 0)
            {
                vehicleVal += 250;
            }

            if ((VehicleFlags & FLAG_PLASTEEL_TANK) != 0)
            {
                vehicleVal += 1000;
            }

            CattleLogger.Message("Vehicle/Tool Points: " + vehicleVal);


            float victoryVal = 0;
            if (Victory)
            {
                victoryVal = 35000 - timeVal*3/2; // SHORTER victory runs are better

                timeVal = 0;
                if (victoryVal < 20000) victoryVal = 20000;                

                if ((VehicleFlags & (FLAG_SEAMOTH + FLAG_EXOSUIT + FLAG_CYCLOPS)) == 0)
                {
                    victoryVal += 30000;
                }
            }

            CattleLogger.Message("Victory Points: " + victoryVal);

            float totalVal = timeVal + vehicleVal + victoryVal;

            CattleLogger.Message("SUBTOTAL: " + totalVal);

            if ((Deaths > 1) || ((Deaths > 0) && Victory))
            {
                totalVal = totalVal / (Deaths + (Victory ? 1 : 0));
                CattleLogger.Message("ADJUSTED FOR " + Deaths + " DEATHS: " + totalVal);
            }

            if (DeathRunSettingCount >= 0)
            {
                Score = (int)(totalVal * (DeathRunSettingCount + DeathRunSettingBonus / 2) / MAX_DEATHRUN_SETTING_COUNT);
                CattleLogger.Message("ADJUSTED FOR DEATHRUN SETTINGS: " + Score);
            }
            else
            {
                Score = (int)totalVal;
            }

            if (Score > 99999) Score = 99999;

            CattleLogger.Message("FINAL SCORE: " + Score);

            return Score;
        }


        public bool betterThan (RunData other)
        {
            // Mostly at this point use score
            if (Score > other.Score) return true;
            if (other.Score > Score) return false;

            // Victory beats defeat
            if (Victory && !other.Victory) return true;
            if (other.Victory) return false;

            // Compares two victories
            if (Victory)
            {
                if (Deaths < other.Deaths) return true; // Fewer deaths is definitive comparison between victories
                if (other.Deaths <= Deaths) return false;

                return RunTime <= other.RunTime; // In victory a SHORTER time is best
            }

            // Otherwise we are comparing two defeats. In death, longer survival is best.
            if ((Deaths > 0) && other.Deaths > 0)
            {
                return RunTime / Deaths >= other.RunTime / other.Deaths;
            }

            return RunTime >= other.RunTime;
        }
    }


    /**
     * DeathRunSaveData - saves and restores data we want saved with the saved game.
     */
    public class DeathRunSaveData
    {
        public PlayerSaveData playerSave { get; set; }   // Player (lives, duration) save data
        public NitroSaveData nitroSave { get; set; }     // Nitrogen/Bends save data
        public PodSaveData podSave { get; set; }         // Escape Pod save data
        public StartSpot startSave { get; set; }         // Escape Pod start spot save data
        public CountdownSaveData countSave { get; set; } // Countdown save data
        public RunData runData { get; set; }             // Stats about our run

        public DeathRunSaveData()
        {
            playerSave = new PlayerSaveData();
            nitroSave  = new NitroSaveData();
            podSave    = new PodSaveData();
            startSave  = new StartSpot();
            countSave  = new CountdownSaveData();
            runData    = new RunData();

            setDefaults();
        }

        public void setDefaults()
        {
            playerSave.setDefaults();
            nitroSave.setDefaults();
            podSave.setDefaults();
            countSave.setDefaults();
        }

        public void Save()
        {
            string saveDirectory = SaveUtils.GetCurrentSaveDataDir();

            DeathRun.saveData.countSave.AboutToSaveGame();
            DeathRun.saveData.playerSave.AboutToSaveGame();

            try
            {
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore, // Keeps our Vector3's etc from generating infinite references
                    //PreserveReferencesHandling = PreserveReferencesHandling.Objects
                };

                var saveDataJson = JsonConvert.SerializeObject(this, Formatting.Indented, settings);

                if (!Directory.Exists(saveDirectory))
                {
                    Directory.CreateDirectory(saveDirectory);
                }

                File.WriteAllText(Path.Combine(saveDirectory, DeathRun.SaveFile), saveDataJson);
            }
            catch (Exception e)
            {
                CattleLogger.GenericError(e);
                CattleLogger.Message("Failed");
            }
        }

        public void Load() 
        {
            var path = Path.Combine(SaveUtils.GetCurrentSaveDataDir(), DeathRun.SaveFile);

            if (!File.Exists(path))
            {
                CattleLogger.Message("Death Run data not found - using defaults");
                setDefaults();
                return;
            }

            try
            {
                var save = File.ReadAllText(path);

                var jsonSerializerSettings = new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore,
                    //PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                };
               
                //var json = JsonConvert.DeserializeObject<DeathRunSaveData>(save, jsonSerializerSettings);
                //this.exampleString = json.exampleString;
                //this.exampleData = json.exampleData;

                // This deserializes the whole saveData object all at once.
                DeathRun.saveData = JsonConvert.DeserializeObject<DeathRunSaveData>(save, jsonSerializerSettings);

                DeathRun.saveData.countSave.JustLoadedGame();
                DeathRun.saveData.playerSave.JustLoadedGame();

                // Special escape-pod re-adjustments
                EscapePod_FixedUpdate_Patch.JustLoadedGame();

                DeathRunUtils.JustLoadedGame();
            }
            catch (Exception e)
            {
                CattleLogger.GenericError(e);
                CattleLogger.Message("Death Run data not found - using defaults");
                CattleLogger.Message(e.StackTrace);
                setDefaults();
            }
        }


        /**
         * This deserializes a byte stream we've retrieved from other sources (used on the main menu)
         */
        public static bool LoadFromBytes(byte[] bytes, out DeathRunSaveData target)
        {
            try
            {
                string @jsonData = Encoding.UTF8.GetString(bytes);

                var jsonSerializerSettings = new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore,
                };
                

                // This deserializes the whole saveData object all at once.
                target = JsonConvert.DeserializeObject<DeathRunSaveData>(jsonData, jsonSerializerSettings);
            }
            catch (Exception e)
            {
                CattleLogger.GenericError(e);
                CattleLogger.Message("Death Run thumbnail data not found - using defaults");
                CattleLogger.Message(e.StackTrace);

                target = null;
                return false;
            }

            return true;
        }
    }


    /**
     * DeathRunSaveListener - we add one of these to the Escape Pod (just to have a game object), and it listens for when the game is
     * saved and loaded so that we can do our save/load stuff too.
     */
    public class DeathRunSaveListener : MonoBehaviour, IProtoEventListener
    {
        public void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            DeathRun.saveData.Load();
        }

        public void OnProtoSerialize(ProtobufSerializer serializer)
        {
            DeathRun.saveData.Save();
        }
    }



   /**
    * DeathRunStats - saves and restores "stats" data across all games
    */
    public class DeathRunStats
    {
        public bool VeryFirstTime { get; set; }
        public int RunCounter { get; set; }
        public int RecentIndex { get; set; }
        public List<RunData> HighScores { get; set; }
        public string Version { get; set; }

        public const int MAX_HIGH_SCORES = 10;

        public DeathRunStats()
        {
            HighScores = new List<RunData>();
            setDefaults();
        }

        public void setDefaults()
        {
            VeryFirstTime = true;
            RunCounter = 0;
            RecentIndex = -1;
            Version = "";
        }

        /**
         * Gets a unique Run ID for a newly-starting run, so that we'll know which other scores in the list potentially came from our same run.
         */
        public int getNewRunID()
        {
            if (DeathRun.patchFailed) return -1;

            RunCounter++;

            SaveStats();

            return RunCounter;
        }

        /**
         * Clear any "extra scores" from the end of the list (in case we added a recent run as an "temporary 11th place")
         */
        private void ClearExtraScores()
        {
            // Remove anything more than the max number of items.
            while (HighScores.Count > MAX_HIGH_SCORES)
            {
                HighScores.RemoveAt(MAX_HIGH_SCORES);
            }
        }


        /**
         * When we've just loaded the stats file for the first time this session
         */
        public void JustLoadedStats()
        {
            ClearExtraScores();

            foreach (RunData run in HighScores)
            {
                run.calcScore();
            }

            RecentIndex = -1; // When reloading, we clear the "my most recent run" pointer.
        }


        /**
         * Adds a completed run to our high score list at the appropriate place. If 
         */
        public int addRun(RunData run)
        {            
            ClearExtraScores();

            if (run.ID < 0) return 0;

            List<RunData> toRemove = new List<RunData>();

            // Remove any earlier lower-scoring entries from this same run ID
            foreach (RunData existing in HighScores)
            {
                if (existing.ID == run.ID)
                {
                    if (run.betterThan(existing))
                    {
                        toRemove.Add(existing);
                    }
                }
            }

            foreach (RunData remove in toRemove)
            {
                HighScores.Remove(remove);
            }

            int place;
            bool added = false;
            for (place = 0; place < HighScores.Count; place++)
            {
                if (!run.betterThan(HighScores[place])) continue;

                // Add our score to the list
                HighScores.Insert(place, run);
                added = true;

                // If we bomped some other run down to 11th place then remove it
                ClearExtraScores(); 
                break;
            }

            // Add to bottom of the list if we didn't displace anyone. 
            // This will also result in a temporary "11th place" for the current run if it didn't beat anyone
            if (!added && (place == HighScores.Count))
            {
                HighScores.Insert(place, run);
            }

            RecentIndex = place;

            SaveStats();

            return place;
        }


        public void SaveStats()
        {
            if (DeathRun.patchFailed) return;

            Version = DeathRunUtils.VERSION;

            try
            {
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore, // Keeps our Vector3's etc from generating infinite references
                    //PreserveReferencesHandling = PreserveReferencesHandling.Objects
                };

                var statsDataJson = JsonConvert.SerializeObject(this, Formatting.Indented, settings);

                File.WriteAllText(Path.Combine(DeathRun.modFolder, DeathRun.StatsFile), statsDataJson);
            }
            catch (Exception e)
            {
                CattleLogger.GenericError(e);
                CattleLogger.Message("Failed");
            }
        }

        public void LoadStats()
        {
            var path = Path.Combine(DeathRun.modFolder, DeathRun.StatsFile);

            if (!File.Exists(path))
            {
                CattleLogger.Message("Death Run `Stats` data not found - starting new Stats");
                setDefaults();
                SaveStats();
                return;
            }

            try
            {
                var save = File.ReadAllText(path);

                var jsonSerializerSettings = new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore,
                    //PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                };

                // This deserializes the whole statsData object all at once.
                DeathRun.statsData = JsonConvert.DeserializeObject<DeathRunStats>(save, jsonSerializerSettings);

                DeathRun.statsData.JustLoadedStats();
            }
            catch (Exception e)
            {
                CattleLogger.GenericError(e);
                CattleLogger.Message("Death Run Stats not found - starting new Stats");
                CattleLogger.Message(e.StackTrace);
                setDefaults();
                SaveStats();
            }
        }
    }




    public class HighScoreText : BasicText
    {
        public HighScoreText() : base(20)
        {            
        }

        public HighScoreText(TextAnchor useAlign) : base(20, useAlign)
        {
        }
    }



    /**
     * Transformations that can be inexpensively copied (since the = operator in c# copies the reference not the object, and
     * copying a "real" Transform requires instantiating a game object: blarg)
     */
    public class Trans
    {
        public Vector3 position { get; set; }
        public Quaternion rotation { get; set; }
        public Vector3 localScale { get; set; }
        public bool initialized;

        public Trans(Vector3 newPosition, Quaternion newRotation, Vector3 newLocalScale)
        {
            position = newPosition;
            rotation = newRotation;
            localScale = newLocalScale;
            initialized = true;
        }

        public Trans()
        {
            position = Vector3.zero;
            rotation = Quaternion.identity;
            localScale = Vector3.one;
            initialized = false;
        }

        public Trans(Transform transform)
        {
            copyFrom(transform);
            initialized = true;
        }

        public bool isInitialized()
        {
            return initialized;
        }

        public void copyFrom(Transform transform)
        {
            position = transform.position;
            rotation = transform.rotation;
            localScale = transform.localScale;
            initialized = true;
        }

        public void copyFrom(Trans trans)
        {
            position = trans.position;
            rotation = trans.rotation;
            localScale = trans.localScale;
            initialized = true;
        }

        public void copyTo(Transform transform)
        {
            transform.position = position;
            transform.rotation = rotation;
            transform.localScale = localScale;
        }

        public void copyTo(Trans trans)
        {
            trans.position = position;
            trans.rotation = rotation;
            trans.localScale = localScale;
            trans.initialized = true;
        }
    }
}