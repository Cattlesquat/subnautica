/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 * Escape Pod sinking -- inspired by and adapted from oldark's "Escape Pod Unleashed" -- basically the idea is to have your escape pod
 * "sink to the bottom". It's different, and dramatic, and "maybe just a tad more challenging", especially combined with the N2 changes,
 * etc.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using HarmonyLib;
using UnityEngine;

namespace DeathRun.Patchers   
{
    /**
     * Data that is saved/restored with saved games is here (DeathRun.saveData.podSave)
     */
    public class PodSaveData
    {
        public Trans podTransform { get; set; }   // Pod's correct transform
        public Trans podPrev { get; set; }        // Pod's previous transform
        public Trans podTipped { get; set; }      // Pod when tipped over
        public Trans podStraight { get; set; }    // Pod when upright
        public bool podAnchored { get; set; } // True if pod has reached "final resting position" on bottom and shouldn't move again
        public bool podSinking { get; set; }  // True if "pod sinking" has initiated (after cinematics, etc)
        public bool podGravity { get; set; }  // True if pod *should* sink (as opposed to float on surface)
        public float lastDepth { get; set; }  // Most recent depth of pod
        public int prevSecs { get; set; }     // Previous integer "secs" time.
        public bool spotPicked { get; set; }  // Spot picked?

        public PodSaveData()
        {
            podTransform = new Trans();
            podPrev = new Trans();
            podTipped = new Trans();
            podStraight = new Trans();
            setDefaults();
        }

        public void setDefaults()
        {
            podGravity = true;
            podAnchored = false;
            podSinking = false;
            lastDepth = 0;
            prevSecs = 0;
            spotPicked = false;
        }
    }


    [HarmonyPatch(typeof(EscapePod))]   
    [HarmonyPatch("StartAtPosition")]   
    internal class EscapePod_StartAtPosition_Patch
    {
        [HarmonyPrefix]            
        public static bool Prefix(ref Vector3 position, EscapePod __instance) 
        {
            if (Config.BASIC_GAME.Equals(DeathRun.config.startLocation))
            {
                return true;
            }

            __instance.transform.position = position;
            __instance.anchorPosition = position;
            __instance.RespawnPlayer();

            return false;                 
        }
    }


    /**
     * RespawnPlayer -- recharge the Escape Pod power supply when player dies, but the amount depends on difficulty level settings.
     */
    [HarmonyPatch(typeof(EscapePod))]
    [HarmonyPatch("RespawnPlayer")]
    internal class EscapePod_RespawnPlayer_Patch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            float restore;
            if (Config.DEATHRUN.Equals(DeathRun.config.powerCosts))
            {
                restore = 5;   // Recharge each cell by 5 (total of 15)
            } 
            else if (Config.HARDCORE.Equals(DeathRun.config.powerCosts))
            {
                restore = 10;  // Recharge each cell by 10 (total of 30)
            } 
            else
            {
                restore = 25;  // Recharges each cell to full power
            }

            RegeneratePowerSource[] cells = EscapePod.main.gameObject.GetAllComponentsInChildren<RegeneratePowerSource>();
            if (cells != null)
            {
                foreach (RegeneratePowerSource cell in cells)
                {
                    float chargeable = cell.powerSource.GetMaxPower() - cell.powerSource.GetPower();

                    restore = Mathf.Clamp(restore, 0, chargeable);
                    cell.powerSource.ModifyPower(restore, out _);
                }
            }
        }
    }

    [HarmonyPatch(typeof(EscapePod))]
    [HarmonyPatch("FixedUpdate")]
    internal class EscapePod_FixedUpdate_Patch
    {
        static bool frozen = false;
        static int frozenSecs = 0;

        [HarmonyPrefix]
        public static bool Prefix(EscapePod __instance)
        {
            if (Config.BASIC_GAME.Equals(DeathRun.config.startLocation))
            {
                return true;
            }

            WorldForces wf = __instance.GetComponent<WorldForces>();
            float depth = Ocean.main.GetDepthOf(__instance.gameObject);

            // Copy our current transform
            if (!DeathRun.saveData.podSave.podAnchored)
            {
                DeathRun.saveData.podSave.podTransform.copyFrom(__instance.transform);
            }

            // This block makes sure the pod doesn't sink *during* the opening cinematic etc.
            if (!DeathRun.saveData.podSave.podSinking)
            {
                frozen = false;

                // This checks if we're holding on the "press any key to continue" screen or intro cinematic
                if (DeathRunUtils.isIntroStillGoing())
                {
                    return true;
                }

                // Otherwise, "turn on gravity" for the pod
                if (DeathRun.saveData.podSave.podGravity)
                {
                    DeathRun.saveData.podSave.podSinking = true;
                    wf.underwaterGravity = 9.81f;
                    if (depth <= 0)
                    {
                        wf.aboveWaterGravity = 50f;
                    }
                    else
                    {
                        wf.aboveWaterGravity = 9.81f;
                    }
                }
            }

            // Once we're below the surface, return gravity to normal
            if (wf.aboveWaterGravity == 50f && depth > 0)
            {
                wf.aboveWaterGravity = 9.81f;
            }

            // Give player some early feedback the lifepod is sinking
            if (DeathRun.saveData.podSave.podGravity && (depth > 6) && (DeathRun.saveData.podSave.lastDepth < 6))
            {
                ErrorMessage.AddMessage("The Life Pod is sinking!");
                DeathRunUtils.CenterMessage("The Life Pod is sinking!", 6);
            }

            int secs = (int)DayNightCycle.main.timePassedAsFloat;
            if (secs != DeathRun.saveData.podSave.prevSecs)
            {
                // Note if we've recently been frozen in place by cut scene or player distance
                if (frozen || __instance.rigidbodyComponent.isKinematic) 
                {
                    frozenSecs = secs;
                }

                // Check when pod hits bottom so we can stop processing it.
                if ((DeathRun.saveData.podSave.prevSecs > 0) && (secs - frozenSecs > 2) && (depth > 20))
                {
                    float dist = Vector3.Distance(DeathRun.saveData.podSave.podPrev.position, DeathRun.saveData.podSave.podTransform.position);
                    if (!DeathRun.saveData.podSave.podAnchored && (dist < 0.5))
                    {
                        ErrorMessage.AddMessage("The Escape Pod has struck bottom!");
                        DeathRunUtils.CenterMessage("The Escape Pod has struck bottom!", 4);
                        DeathRun.saveData.podSave.podAnchored = true;
                        float random = UnityEngine.Random.value;
                        float angle;
                        float up = 2;

                        if (random < .10f)
                        {
                            angle = 30;
                        } else if (random < .20f)
                        {
                            angle = 45;
                        } else if (random < .30f)
                        {
                            angle = 60;
                        } else if (random < .40f)
                        {
                            angle = 120;
                        } else if (random < .50f)
                        {
                            angle = 135;
                            up    = 3;
                        } else if (random < .60f)
                        {
                            angle = 150;
                            up    = 4;
                        } else if (random < .70f)
                        {
                            angle = 170;
                            up    = 4;
                        } else if (random < .80f)
                        {
                            angle = 300;
                        } else if (random < .90f)
                        {
                            angle = 315;
                        } else
                        {
                            angle = 330;
                        }
                        
                        __instance.transform.Translate(0, up, 0);

                        if (Player.main.IsInside())
                        {
                            Player.main.transform.Translate(0, 2, 0);
                        }

                        // Make a copy of our "upright state"
                        DeathRun.saveData.podSave.podStraight.copyFrom(__instance.transform);

                        // Now tip the pod over and copy that
                        __instance.transform.Rotate(Vector3.forward, angle); // Jolt at bottom!
                        DeathRun.saveData.podSave.podTipped.copyFrom(__instance.transform);

                        // If we're supposed to (based on preferences) stay straight up, copy THAT back
                        if (DeathRun.config.podStayUpright)
                        {
                            DeathRun.saveData.podSave.podStraight.copyTo(__instance.transform);
                        }

                        // Finally, store the stable transform we're supposed to use.
                        DeathRun.saveData.podSave.podTransform.copyFrom(__instance.transform);
                    }
                }
                DeathRun.saveData.podSave.prevSecs = secs;
                DeathRun.saveData.podSave.podPrev.copyFrom(DeathRun.saveData.podSave.podTransform);
            }            

            // If player is away from the pod, stop gravity so that it doesn't fall through the world when the geometry unloads
            frozen = DeathRun.saveData.podSave.podAnchored || (Vector3.Distance(__instance.transform.position, Player.main.transform.position) > 20);
            if (frozen)
            {
                wf.underwaterGravity = 0.0f;
            }
            else
            {
                wf.underwaterGravity = 9.81f;
            }

            DeathRun.saveData.podSave.lastDepth = depth;
            return true;
        }

        static bool wasDead = false;

        [HarmonyPostfix]
        public static void Postfix(EscapePod __instance)
        {
            if (Config.BASIC_GAME.Equals(DeathRun.config.startLocation))
            {
                return;
            }

            if (DeathRun.saveData.podSave.podAnchored || !DeathRun.saveData.podSave.podGravity) 
            {
                __instance.rigidbodyComponent.isKinematic = true; // Make sure pod stays in place (turns off physics effects)
            }
            else if (DeathRun.playerIsDead)
            {
                __instance.rigidbodyComponent.isKinematic = true; // Make sure pod stays in place (turns off physics effects)
                wasDead = true;
            }
            else
            {
                if (wasDead && !DeathRun.playerIsDead)
                {
                    wasDead = false;
                    if (!DeathRun.saveData.podSave.podAnchored && DeathRun.saveData.podSave.podGravity)
                    {
                        __instance.rigidbodyComponent.isKinematic = false;
                    }
                }

                if (frozen)
                {
                    DeathRun.saveData.podSave.podTransform.copyTo(__instance.transform); // Teleport pod back to where it was at beginning of frame (temporary "frozen" behavior)
                }
            }
        }

        /**
         * Sets the interval at which the each of the Escape Pod's power cells regenerate an energy. Vanilla is 20.
         */
        public static void setRegenerateInterval(float secs)
        {
            RegeneratePowerSource[] cells = EscapePod.main.gameObject.GetAllComponentsInChildren<RegeneratePowerSource>();
            if (cells != null)
            {
                foreach (RegeneratePowerSource cell in cells)
                {
                    cell.regenerationInterval = secs;
                }
            }
        }

        public static void CheckSolarCellRate ()
        {
            if (EscapePod.main.damageEffectsShowing)
            {
                setRegenerateInterval(20);
            } else
            {
                setRegenerateInterval(15);
            }
        }

        /**
         * When loading game, put escape pod back in any weird position, and restore its anchor.
         */
        public static void JustLoadedGame()
        {
            if (!Config.BASIC_GAME.Equals(DeathRun.config.startLocation))
            {
                WorldForces wf = EscapePod.main.GetComponent<WorldForces>();
                float depth = Ocean.main.GetDepthOf(EscapePod.main.gameObject);

                if (DeathRun.saveData.podSave.podSinking)
                {
                    wf.aboveWaterGravity = 9.81f;
                }

                if (DeathRun.saveData.podSave.podAnchored)
                {
                    DeathRun.saveData.podSave.podTransform.copyTo(EscapePod.main.transform);
                    EscapePod.main.rigidbodyComponent.isKinematic = true;
                    wf.underwaterGravity = 0.0f;
                }
                else
                {
                    wf.underwaterGravity = 9.81f;
                }
            }

            if (EscapePod.main.liveMixin.GetHealthFraction() < 0.99f)
            {
                CattleLogger.Message("Loading Damaged Pod - effects showing: " + EscapePod.main.damageEffectsShowing);
                EscapePod.main.damageEffectsShowing = false;
                EscapePod.main.ShowDamagedEffects();
                EscapePod.main.lightingController.SnapToState(2);
                uGUI_EscapePod.main.SetHeader(Language.main.Get("IntroEscapePod3Header"), new Color32(243, 201, 63, byte.MaxValue), 2f);
                uGUI_EscapePod.main.SetContent(Language.main.Get("IntroEscapePod3Content"), new Color32(233, 63, 27, byte.MaxValue));
                uGUI_EscapePod.main.SetPower(Language.main.Get("IntroEscapePod3Power"), new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
                EscapePod.main.introCinematic.interpolationTimeOut = 0f;
            } else
            {
                CattleLogger.Message("Loading Repaired Pod - effects showing: " + EscapePod.main.damageEffectsShowing);
                EscapePod.main.lightingController.SnapToState(0);   //159,243,63
                uGUI_EscapePod.main.SetHeader(Language.main.Get("IntroEscapePod4Header"), new Color32(243, 243, 63, byte.MaxValue), 2f);
                uGUI_EscapePod.main.SetContent(Language.main.Get("IntroEscapePod4Content"), new Color32(243, 243, 63, byte.MaxValue));
                uGUI_EscapePod.main.SetPower(Language.main.Get("IntroEscapePod4Power"), new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
            }
            EscapePod.main.UpdateDamagedEffects();
            CheckSolarCellRate();
        }
    }

    /**
     * In UpdateDamageEffects we handle two main things:
     * (1) When the secondary systems are first repaired, we give the Escape Pod some extra power
     * (2) We keep the Escape Pod's "status display" updated.
     */
    [HarmonyPatch(typeof(EscapePod))]
    [HarmonyPatch("UpdateDamagedEffects")]
    internal class EscapePod_UpdateDamagedEffects_Patch
    {
        static bool damaged = false;
        static bool blinkOn = false;
        static float lastBlink = 0;

        [HarmonyPrefix]
        public static bool Prefix()
        {
            damaged = EscapePod.main.damageEffectsShowing;
            return true;
        }

        [HarmonyPostfix]
        public static void Postfix()
        {
            if (damaged != EscapePod.main.damageEffectsShowing)
            {
                EscapePod_FixedUpdate_Patch.CheckSolarCellRate();

                if (!EscapePod.main.damageEffectsShowing)
                {
                    // At repair, give a one-time "full recharge"
                    RegeneratePowerSource[] cells = EscapePod.main.gameObject.GetAllComponentsInChildren<RegeneratePowerSource>();
                    if (cells != null)
                    {
                        foreach (RegeneratePowerSource cell in cells)
                        {
                            float chargeable = cell.powerSource.GetMaxPower() - cell.powerSource.GetPower();
                           
                            cell.powerSource.ModifyPower(chargeable, out _);
                        }
                    }
                }
            } else {
                if ((lastBlink == 0) || (lastBlink > Time.time))
                {
                    lastBlink = Time.time;
                }
                else if (Time.time >= lastBlink + 0.5)
                {
                    lastBlink = Time.time;
                    blinkOn = !blinkOn;
                }

                if (damaged)
                {
                    string content = Language.main.Get("IntroEscapePod3Content");
                    string bonus;

                    if (DeathRun.saveData.podSave.podAnchored || Config.BASIC_GAME.Equals(DeathRun.config.startLocation))
                    {
                        if (!Config.BASIC_GAME.Equals(DeathRun.config.startLocation))
                        {
                            content = content.Replace("Flotation Devices: DEPLOYED", "Flotation Devices: FAILED");

                            if (DeathRun.config.podStayUpright)
                            {
                                content = content.Replace("Hull Integrity: OK", "Inertial Stabilizers: DEPLOYED");
                            }
                            else
                            {
                                content = content.Replace("Hull Integrity: OK", "Inertial Stabilizers: " + (blinkOn ? "FAILED" : ""));
                            }
                        }
                    }
                    else
                    {
                        content = content.Replace("Flotation Devices: DEPLOYED", "Flotation Devices: " + (blinkOn ? "FAILED" : ""));
                    }

                    if (!Config.NORMAL.Equals(DeathRun.config.creatureAggression))
                    {
                        content = content.Replace("Uncharted ocean planet 4546B", "Planet 4546B: HOSTILE FAUNA");
                    }

                    if (BreathingPatcher.isSurfaceAirPoisoned())
                    {
                        content = content.Replace("Oxygen/nitrogen atmosphere", "Atmosphere: requires filtration");
                    }

                    bonus = "";

                    if (BreathingPatcher.isSurfaceAirPoisoned())
                    {
                        bonus += "\n\n- Atmosphere: " + (blinkOn ? "NOT BREATHABLE" : "") + "\n- Recommend Air Pumps to Filter Oxygen";
                    }
                        
                    if (DeathRunUtils.isExplosionClockRunning())
                    {
                        bonus += "\n\n" + (blinkOn ? "- QUANTUM EXPLOSION WARNING" : "");
                    }

                    if (RadiationUtils.isRadiationActive())
                    {
                        bonus += "\n\n" + (blinkOn ? "- EXTREME RADIATION HAZARD" : "");
                    }

                    uGUI_EscapePod.main.SetHeader(Language.main.Get("IntroEscapePod3Header"), new Color32(243, 201, 63, byte.MaxValue), 2f);
                    uGUI_EscapePod.main.SetContent(content + bonus, new Color32(233, 63, 27, byte.MaxValue));
                }
                else
                {
                    string content = Language.main.Get("IntroEscapePod4Content");

                    if (!Config.BASIC_GAME.Equals(DeathRun.config.startLocation))
                    {
                        content = content.Replace("Flotation Devices: DEPLOYED", "Flotation Devices: FAILED");

                        if (DeathRun.config.podStayUpright)
                        {
                            content = content.Replace("Hull Integrity: OK", "Inertial Stabilizers: DEPLOYED");
                        } else
                        {
                            content = content.Replace("Hull Integrity: OK", "Inertial Stabilizers: FAILED");
                        }
                    }

                    string bonus = "";

                    if (DeathRunUtils.isExplosionClockRunning())
                    {
                        bonus += "\n    - QUANTUM EXPLOSION WARNING";
                    }

                    if (RadiationUtils.isRadiationActive())
                    {
                        bonus += "\n    - EXTREME RADIATION HAZARD";
                    }

                    uGUI_EscapePod.main.SetHeader(Language.main.Get("IntroEscapePod4Header"), new Color32((byte)(blinkOn ? 243 : 228), (byte)(blinkOn ? 243 : 228), 63, byte.MaxValue)); //, 2f);
                    uGUI_EscapePod.main.SetContent(content + bonus, new Color32((byte)(blinkOn ? 243 : 228), (byte)(blinkOn ? 243 : 228), 63, byte.MaxValue));
                }
            }
        }
    }

    [HarmonyPatch(typeof(EscapePod))]
    [HarmonyPatch("Awake")]
    internal class EscapePod_Awake_Patch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            // This adds a listener to the Escape Pod's game object, entirely for the purpose of letting the whole mod know
            // when the player is saving/loading the game so that we will save and load our part.

            DeathRun.saveListener = EscapePod.main.gameObject.AddComponent<DeathRunSaveListener>();
        }
    }
}

