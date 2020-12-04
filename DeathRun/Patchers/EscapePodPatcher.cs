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
using HarmonyLib;
using UnityEngine;

namespace DeathRun.Patchers   
{
    /**
     * Data that is saved/restored with saved games is here (DeathRun.saveData.podSave)
     */
    public class PodSaveData
    {
        public Trans podTransform { get; set; } = new Trans(); // Pod's correct transform
        public Trans podPrev { get; set; } = new Trans();      // Pod's previous transform
        public bool podAnchored { get; set; } // True if pod has reached "final resting position" on bottom and shouldn't move again
        public bool podSinking { get; set; }  // True if "pod sinking" has initiated (after cinematics, etc)
        public bool podGravity { get; set; }  // True if pod *should* sink (as opposed to float on surface)
        public float lastDepth { get; set; }  // Most recent depth of pod
        public int prevSecs { get; set; }     // Previous integer "secs" time.

        public PodSaveData()
        {
            setDefaults();
        }

        public void setDefaults()
        {
            podGravity = true;
            podAnchored = false;
            podSinking = false;
            lastDepth = 0;
            prevSecs = 0;
        }
    }


    [HarmonyPatch(typeof(EscapePod))]   
    [HarmonyPatch("StartAtPosition")]   
    internal class EscapePod_StartAtPosition_Patch
    {
        [HarmonyPrefix]            
        public static bool Prefix(ref Vector3 position, EscapePod __instance) 
        {
            __instance.transform.position = position;
            __instance.anchorPosition = position;
            __instance.RespawnPlayer();   
            return false;                 
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
                        if (random < .20f)
                        {
                            angle = 30;
                        } else if (random < .40f)
                        {
                            angle = 45;
                        } else if (random < .60f)
                        {
                            angle = 60;
                        } else if (random < .80f)
                        {
                            angle = 120;
                        } else
                        {
                            angle = 135;
                        }
                        __instance.transform.Rotate(Vector3.forward, angle); // Jolt at bottom!
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

        [HarmonyPostfix]
        public static void Postfix(EscapePod __instance)
        {
            if (DeathRun.saveData.podSave.podAnchored || !DeathRun.saveData.podSave.podGravity)
            {
                __instance.rigidbodyComponent.isKinematic = true; // Make sure pod stays in place (turns off physics effects)
            }
            else if (frozen)
            {
                DeathRun.saveData.podSave.podTransform.copyTo(__instance.transform); // Teleport pod back to where it was at beginning of frame (temporary "frozen" behavior)
            }
        }

        /**
         * When loading game, put escape pod back in any weird position, and restore its anchor.
         */
        public static void JustLoadedGame()
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
            } else
            {
                wf.underwaterGravity = 9.81f;
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

            Common.SeraLogger.Message(DeathRun.modName, "Escape Pod Awake");
            DeathRun.saveListener = EscapePod.main.gameObject.AddComponent<DeathRunSaveListener>();
        }
    }
}

