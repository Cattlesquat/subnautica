/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 * This introduces a countdown timer for the ship explosion, hijacking the Sunbeam's rescue arrival timer UI (but staying out
 * of its way). Also allows the countdown length to be left random, or configured long/medium/short.
 * 
 * Provides some messages to the player along the way, that also explain the "radiation rules".
 */
namespace DeathRun.Patchers
{
    using HarmonyLib;
    using Items;
    using Common;
    using UnityEngine;
    using System;
    using UWE;

    /**
     * Our time monitor is saved with the saved game, to maintain message continuity across saves.
     */
    public class CountdownSaveData
    {
        public float currTime { get; set; }
        public float prevTime { get; set; }

        public CountdownSaveData()
        {
            setDefaults();
        }

        public void AboutToSaveGame()
        {
            currTime = DeathRun.countdownMonitor.currValue; // TimeMonitor doesn't seem to serialize well, so we do this.
            prevTime = DeathRun.countdownMonitor.prevValue;
        }

        public void JustLoadedGame()
        {
            DeathRun.countdownMonitor.currValue = currTime; // TimeMonitor doesn't seem to serialize well, so we do this.
            DeathRun.countdownMonitor.prevValue = prevTime;
        }

        public void setDefaults()
        {
            currTime = 0;
            prevTime = 0;
        }
    }


    [HarmonyPatch(typeof(CrashedShipExploder))]
    [HarmonyPatch("SetExplodeTime")]
    internal class ExplodeTime_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(ref CrashedShipExploder __instance)
        {
            float num;
            if (Config.TIME_LONG.Equals(DeathRun.config.explosionTime))
            {
                num = 4.5f;   // 90 minutes from game start  (80 minutes is the longest "random" time available)
            }
            else if (Config.TIME_SHORT.Equals(DeathRun.config.explosionTime))
            {
                num = 2.25f; // Just less than shortest possible time of the original random numbber range: 45 minutes from game start
                // (actual shortest random value is 2.3f, but 46 minutes just... irritates me)
            }
            else if (Config.TIME_MEDIUM.Equals(DeathRun.config.explosionTime))
            {
                num = 3f;   // One hour from game start
            } else
            {
                return; // Just leave the original random number in place
            }

            __instance.timeToStartCountdown = __instance.timeToStartWarning + num * 1200f;
        }
    }

    [HarmonyPatch(typeof(uGUI_SunbeamCountdown))]
    [HarmonyPatch("UpdateInterface")]
    internal class Countdown_Patch
    {
        static bool showingShip = false;        

        /**
         * Returns true if we're currently showing the Sunbeam Arrival countdown
         */
        static bool isSunbeamShowing (uGUI_SunbeamCountdown __instance)
        {
            StoryGoalCustomEventHandler main = StoryGoalCustomEventHandler.main;
            return main && main.countdownActive;            
        }

        [HarmonyPostfix]
        public static void Postfix(ref uGUI_SunbeamCountdown __instance)
        {
            if (DayNightCycle.main == null) return;
            if (CrashedShipExploder.main == null) return;

            // These are the internal parameters for the Aurora story events (see AuroraWarnings for time thresholds)
            float timeToStartWarning = CrashedShipExploder.main.GetTimeToStartWarning();
            float timeToStartCountdown = CrashedShipExploder.main.GetTimeToStartCountdown();
            float timeNow = DayNightCycle.main.timePassedAsFloat;

            DeathRun.countdownMonitor.Update(timeNow);

            int deep;
            if (Config.EXPLOSION_DEATHRUN.Equals(DeathRun.config.explodeDepth))
            {
                deep = 100;
            }
            else if (Config.EXPLOSION_HARD.Equals(DeathRun.config.explodeDepth))
            {
                deep = 50;
            } 
            else
            {
                deep = 0;
            }

            if (deep > 0) { 
                // At time of second Aurora warning
                if (DeathRun.countdownMonitor.JustWentAbove(Mathf.Lerp(timeToStartWarning, timeToStartCountdown, 0.5f))) {
                    DeathRunUtils.CenterMessage("Explosion Shockwave Will Be", 5);
                    DeathRunUtils.CenterMessage("Over " + deep + "m Deep!", 5, 1);                    
                }

                if (DeathRun.countdownMonitor.JustWentAbove(Mathf.Lerp(timeToStartWarning, timeToStartCountdown, 0.5f) + 5))
                {
                    ErrorMessage.AddMessage("WARNING: Explosion will produce shockwave over " + deep + "m deep!");
                }

                // At time of third Aurora warning
                if (DeathRun.countdownMonitor.JustWentAbove(Mathf.Lerp(timeToStartWarning, timeToStartCountdown, 0.8f)))
                {
                    DeathRunUtils.CenterMessage("Prepare To Evacuate At Least", 5);
                    DeathRunUtils.CenterMessage("" + deep + "m Deep. Preferably Inside.", 5, 1);                    
                }
                
                if (DeathRun.countdownMonitor.JustWentAbove(Mathf.Lerp(timeToStartWarning, timeToStartCountdown, 0.8f) + 5))
                {
                    ErrorMessage.AddMessage("Prepare to evacuate at least " + deep + "m deep, preferably inside!");
                }

                // At time of final countdown
                if (DeathRun.countdownMonitor.JustWentAbove(timeToStartCountdown))
                {
                    DeathRunUtils.CenterMessage("Seek Safe Depth Immediately!", 5);
                    DeathRunUtils.CenterMessage("Preferably Inside!", 5, 1);
                }

                if (DeathRun.countdownMonitor.JustWentAbove(timeToStartCountdown + 5))
                {
                    ErrorMessage.AddMessage("Seek safe depth immediately! Preferably inside!");
                }
            }


            if (Config.RADIATION_DEATHRUN.Equals(DeathRun.config.radiationDepth))
            {
                deep = 60;
            }
            else if (Config.RADIATION_HARD.Equals(DeathRun.config.radiationDepth))
            {
                deep = 30;
            }
            else
            {
                deep = 0;
            }
            if (deep > 0)
            {
                if (DeathRun.countdownMonitor.JustWentAbove(timeToStartCountdown + 100f))
                {
                    DeathRunUtils.CenterMessage("Radiation Will Gradually Permeate", 5);
                    DeathRunUtils.CenterMessage("Ocean As Deep As " + deep + "m.", 5, 1);
                    
                }
                if (DeathRun.countdownMonitor.JustWentAbove(timeToStartCountdown + 105f))
                {
                    ErrorMessage.AddMessage("Radiation will gradually permeate the sea, as deep as " + deep + "m.");
                }

                if (DeathRun.countdownMonitor.JustWentAbove(timeToStartCountdown + 110))
                {
                    DeathRunUtils.CenterMessage("All devices will consume power more rapidly", 5);
                    DeathRunUtils.CenterMessage("while exposed to RADIATION, and recharge slowly.", 5, 1);                    
                }

                if (DeathRun.countdownMonitor.JustWentAbove(timeToStartCountdown + 115))
                {
                    ErrorMessage.AddMessage("All devices will consume power more rapidly while exposed to RADIATION and recharge slowly.");
                }                    
            }

            // If the Sunbeam rescue timer is showing, give that precedence over our countdown timer.
            if (isSunbeamShowing(__instance))
            {
                showingShip = false;
                return;
            }

            // This is the time of the very first warning about the Aurora
            if (timeNow >= Mathf.Lerp(timeToStartWarning, timeToStartCountdown, 0.2f) && // Time of first Aurora warning
                (timeNow < timeToStartCountdown + 24f))                                  // Actual explosion time (24 sec after countdown)
            {
                float timeLeft = (timeToStartCountdown + 24f) - timeNow;
                if (timeLeft < 0) timeLeft *= -1;
                showShip(ref __instance);
                updateShip(ref __instance, timeLeft);
            } else
            {
                hideShip(ref __instance);
            }
        }

        /**
         * Show the ship countdown, if not already showing
         */
        static void showShip(ref uGUI_SunbeamCountdown __instance)
        {
            if (showingShip) return;

            __instance.countdownTitle.text = "Drive Core Explosion"; 
            __instance.countdownHolder.SetActive(true);

            showingShip = true;
        }

        /**
         * Update the ship countdown display text
         */
        static void updateShip(ref uGUI_SunbeamCountdown __instance, float timeLeft)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds((double)timeLeft);
            string text = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
            __instance.countdownText.text = text;
        }

        /**
         * Hide the ship countdown if we're currently showing it
         */
        static void hideShip(ref uGUI_SunbeamCountdown __instance)
        {
            if (!showingShip) return;
            __instance.countdownHolder.SetActive(false);
            showingShip = false;
        }
    }
}
