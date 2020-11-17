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
            ErrorMessage.AddMessage("Check Position");

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
        static Transform podOriginalTransform;
        static bool anchored = false;
        static float lastDepth = -10;


        [HarmonyPrefix]
        public static bool Prefix(EscapePod __instance)
        {
            podOriginalTransform = __instance.transform;
            WorldForces wf = __instance.GetComponent<WorldForces>();
            float depth = Ocean.main.GetDepthOf(__instance.gameObject);
            if (wf.aboveWaterGravity == 50f && depth > 0)
            {
                wf.aboveWaterGravity = 9.81f;
            }

            Vector3 here = __instance.transform.position;
            Vector3 down = here;
            down.y -= 2;

            if (Physics.Raycast(here, down, 10, Voxeland.GetTerrainLayerMask(), QueryTriggerInteraction.Ignore))
            {
                if (!anchored) ErrorMessage.AddMessage("The Escape Pod has struck bottom.");
                anchored = true; // Protection from weirdly falling through the earth                
            }

            lastDepth = depth;
            return true;
        }

        [HarmonyPostfix]
        public static void Postfix(EscapePod __instance)
        {
            if (anchored)
            {
                __instance.transform.position = podOriginalTransform.position;
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
        }
    }
}

