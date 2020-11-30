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
        static Trans podOriginalTransform = new Trans();
        static Trans podAtPrevSec = new Trans();
        static bool frozen = false;
        static int prevSecs = 0;
        static int frozenSecs = 0;
        static float lastDepth = 0;

        [HarmonyPrefix]
        public static bool Prefix(EscapePod __instance)
        {
            WorldForces wf = __instance.GetComponent<WorldForces>();
            float depth = Ocean.main.GetDepthOf(__instance.gameObject);

            // Copy our current transform
            if (!DeathRun.podAnchored)
            {
                podOriginalTransform.copyFrom(__instance.transform);
            }

            // This block makes sure the pod doesn't sink *during* the opening cinematic etc.
            if (!DeathRun.podSinking)
            {
                frozen = false;

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
                if ((__instance.IsPlayingIntroCinematic() && __instance.IsNewBorn()))
                {
                    return true;
                }

                // Otherwise, "turn on gravity" for the pod
                if (DeathRun.podGravity)
                {
                    DeathRun.podSinking = true;
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
            if (DeathRun.podGravity && (depth > 6) && (lastDepth < 6))
            {
                ErrorMessage.AddMessage("The Life Pod is sinking!");
                DeathRunUtils.CenterMessage("The Life Pod is sinking!", 6);
            }

            int secs = (int)Time.time;
            if (secs != prevSecs)
            {
                // Note if we've recently been frozen in place by cut scene or player distance
                if (frozen || __instance.rigidbodyComponent.isKinematic) 
                {
                    frozenSecs = secs;
                }

                // Check when pod hits bottom so we can stop processing it.
                if ((prevSecs > 0) && (secs - frozenSecs > 2) && (depth > 20))
                {
                    float dist = Vector3.Distance(podAtPrevSec.position, podOriginalTransform.position);
                    if (!DeathRun.podAnchored && (dist < 0.5))
                    {
                        ErrorMessage.AddMessage("The Escape Pod has struck bottom!");
                        DeathRunUtils.CenterMessage("The Escape Pod has struck bottom!", 4);
                        DeathRun.podAnchored = true;
                        float random = UnityEngine.Random.value;
                        float angle;
                        if (random < .25f)
                        {
                            angle = 30;
                        } else if (random < .5f)
                        {
                            angle = 45;
                        } else if (random < .75f)
                        {
                            angle = 60;
                        } else if (random < 85f)
                        {
                            angle = 85;
                        } else if (random < 95f)
                        {
                            angle = 120;
                        } else
                        {
                            angle = 160;
                        }
                        __instance.transform.Rotate(Vector3.forward, angle); // Jolt at bottom!
                        podOriginalTransform.copyFrom(__instance.transform);
                    }
                }
                prevSecs = (int)Time.time;
                podAtPrevSec.copyFrom(podOriginalTransform);
            }            

            // If player is away from the pod, stop gravity so that it doesn't fall through the world when the geometry unloads
            frozen = DeathRun.podAnchored || (Vector3.Distance(__instance.transform.position, Player.main.transform.position) > 20);
            if (frozen)
            {
                wf.underwaterGravity = 0.0f;
            }
            else
            {
                wf.underwaterGravity = 9.81f;
            }

            lastDepth = depth;
            return true;
        }

        [HarmonyPostfix]
        public static void Postfix(EscapePod __instance)
        {
            if (DeathRun.podAnchored || !DeathRun.podGravity)
            {
                __instance.rigidbodyComponent.isKinematic = true; // Make sure pod stays in place (turns off physics effects)
            }
            else if (frozen)
            {
                podOriginalTransform.copyTo(__instance.transform); // Teleport pod back to where it was at beginning of frame (temporary "frozen" behavior)
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

