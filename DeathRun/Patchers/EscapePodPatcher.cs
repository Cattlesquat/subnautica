using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

/**
 * Escape Pod sinking -- inspired by and adapted from oldark's "Escape Pod Unleashed" -- basically the idea is to have your escape pod
 * "sink to the bottom". It's different, and dramatic, and "maybe just a tad more challenging".
 */ 
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
            if (!Main.podAnchored)
            {
                podOriginalTransform.copyFrom(__instance.transform);
            }
            WorldForces wf = __instance.GetComponent<WorldForces>();
            float depth = Ocean.main.GetDepthOf(__instance.gameObject);
            if (wf.aboveWaterGravity == 50f && depth > 0)
            {
                wf.aboveWaterGravity = 9.81f;
            }

            if (depth > 10 && (lastDepth < 10))
            {
                ErrorMessage.AddMessage("The Life Pod is sinking!");
            }

            Vector3 here = __instance.transform.position;
            Vector3 down = here;
            down.y -= 2;

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
                    if (!Main.podAnchored && (dist < 0.5))
                    {
                        ErrorMessage.AddMessage("The Escape Pod has struck bottom!");
                        Main.podAnchored = true;
                        __instance.transform.Rotate(Vector3.forward, 30); // Jolt at bottom!
                        podOriginalTransform.copyFrom(__instance.transform);
                    }
                    //ErrorMessage.AddMessage(" Dist:" + dist + " Diff:" + (podAtPrevSec.position.y - podOriginalTransform.position.y));
                }
                prevSecs = (int)Time.time;
                podAtPrevSec.copyFrom(podOriginalTransform);
            }            

            //if (Physics.Raycast(here, down, 10, Voxeland.GetTerrainLayerMask(), QueryTriggerInteraction.Ignore))
            //{
            //    if (!anchored) ErrorMessage.AddMessage("The Escape Pod has struck bottom.");
            //    anchored = true;
            //}

            //Bounds podBottom = new Bounds(here, down);
            //frozen = !LargeWorldStreamer.main.IsRangeActiveAndBuilt(podBottom);

            frozen = Main.podAnchored || (Vector3.Distance(here, Player.main.transform.position) > 20);
            if (frozen)
            {
                wf.underwaterGravity = 0.0f;
            }
            else
            {
                wf.underwaterGravity = 9.81f;
            }

            //__instance.transform.Rotate(Vector3.forward, 30);

            //RaycastHit raycastHit;
            //if (UWE.Utils.TraceForTerrain(new Ray(here, down), 2, out raycastHit)) {
            //    if (!anchored) ErrorMessage.AddMessage("WHOMP! WHOMP! WHOMP! WHOMP!");
            //    anchored = true; // Protection from weirdly falling through the earth                
            //}

            lastDepth = depth;
            return true;
        }

        [HarmonyPostfix]
        public static void Postfix(EscapePod __instance)
        {
            if (Main.podAnchored)
            {
                __instance.rigidbodyComponent.isKinematic = true;
            }
            else if (frozen)
            {
                podOriginalTransform.copyTo(__instance.transform);                
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
            EscapePod pod = EscapePod.main;

            WorldForces wf = pod.GetComponent<WorldForces>();
            //wf.underwaterGravity = 0.0f;

            wf.underwaterGravity = 9.81f;
            wf.aboveWaterGravity = 50f;

            //LargeWorld.main.streamer.cellManager.RegisterEntity(EscapePod.main.gameObject);
        }
    }
}

