/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 * Adapted from libraryaddict's Radiation Challenge mod -- used w/ permission.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

/**
 *  Random start points -- adjust our starting spots to "more challenging places" (picked these out in the Seamoth)
 */
namespace DeathRun.Patchers
{
    [HarmonyPatch(typeof(RandomStart))]
    [HarmonyPatch("GetRandomStartPoint")]
    internal class RandomStartPatcher
    {
        [HarmonyPrefix]
        public static bool Prefix(EscapePod __instance, ref Vector3 __result)
        {
            int picker = UnityEngine.Random.Range(0, 4);
            ErrorMessage.AddMessage("Start Point: " + picker);

            Main.podAnchored = false;

            float x;
            float z;
            switch (picker)
            {
                case 0: x = 104.5f; z = -317.4f; break; 
                case 1: x = -530.0f; z = 377.1f; break; ///
                case 2: x = 98.7f; z = 717.7f;   break;
                case 3: x = 97.7f; z = 626.2f;   break;
                case 4:
                default:
                    x = -649.4f; z = 92.2f; break;
            }

            //x = 131.8f; z = -399.8f; break; // Right by Delgasi cave. Very hard! But I was able to make oxygen tank, fins, scanner. Seems doable.
            //x = -658.8f; z = 398.7f; // On a Tree Mushroom! Hard but doable...
            //x = -93.8f; z = 82.2f;   // Shallow near a cool cave
            //x = -530.0f; z = 377.1f; // Quite deep, but wasn't too hard.

            __result.x = x;
            __result.y = 0;
            __result.z = z;
            return false;
        }
    }
}
