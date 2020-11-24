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

            Main.podAnchored = false;
            Main.podSinking = false;
            Main.podGravity = true;

            float x;
            float z;
            switch (picker)
            {
                case 0: x = -272.8f; z = 262.9f; break; 
                case 1: x = -343.4f; z = -226.2f; break;  
                case 2: x = 103.6f; z = 257.8f; break;
                case 3: x = -272.8f; z = -262.9f; break; 
                case 4:
                default:
                    x = -649.4f; z = 92.2f; break;
            }

            //x = 131.8f; z = -399.8f; break; // Right by Delgasi cave. Very hard! But I was able to make oxygen tank, fins, scanner. Seems doable.
            //x = -658.8f; z = 398.7f; break; // Dramatic! On a Tree Mushroom! Hard but doable...
            //x = -93.8f; z = 82.2f;   break; // Shallow near a nifty cave. Interesting rather than hard.
            //x = -530.0f; z = 377.1f; break; // Quite deep, but wasn't too hard.
            //x = 104.5f; z = -317.4f; break; // Hopefully not too close to Aurora!
            //x = 151.8f; z = 592.9f; break;  // Deep and kelpy. Disorienting! Hard to find shallows.
            //x = 98.7f; z = 717.7f; break;   // Moderate kelp forest start.
            //x = 97.7f; z = 626.2f; break; // Deep and interesting
            //x = -503.0f; z = -377.1f; break; // In the grassy plains -- a different flavor of start, long way from kelp
            //x = -527.9f; z = 30.5f; break; // Crag near grass - getting to kelp a challenge
            //x = 151.2f; z = -429.5f; break; // Another crag near grass.
            //x = -649.4f; z = 92.2f; break; // Interesting ravines -- metal surprisingly scarce
            //x = 201.1f; z = -360.6f; break; // Near DelGasi
            //x = 165.2f; z = 716.9f; break; // Low-copper! Off towards alien structure I think.
            //x = 23.1f; z = 112.4f; break; // Shallow on an arch
            //x = -301.5f; z = -65.5f; break;







            ErrorMessage.AddMessage("Start Point: " + picker + " " + x + " " + z);

            __result.x = x;
            __result.y = 0;
            __result.z = z;
            return false;
        }
    }
}
