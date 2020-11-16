using HarmonyLib;
using System;
using UnityEngine;

namespace DeathRun   // This is usually going to be the name of your mod. Just make it consistent for each file in a specific mod.
{
    [HarmonyPatch(typeof(EscapePod))]   // We're going to patch the EscapePod class.
    [HarmonyPatch("StartAtPosition")]   // The 'StartAtPosition' method of the EscapePod class.
    internal class EscapePod_StartAtPosition_Patch
    {
        [HarmonyPrefix]                 // Run our patch before the game's default code.
        public static bool Prefix(ref Vector3 position, EscapePod __instance)   // EscapePod __instance gives us the equivalent of 'this.'
        {
            //position.x += 250;
            position.y += 10;
            __instance.transform.position = position;
            __instance.anchorPosition = position;
            __instance.RespawnPlayer();   // So if you compare this with the original EscapePod.StartAtPosition method using dnSpy, you'll see this line as this.RespawnPlayer()
            return false;               // returning false means the original method with Subnautica will not run. We replace it entirely with this one.
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
