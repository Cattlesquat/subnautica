/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

/**
 *  Random start points -- adjust our starting spots to "more challenging places" (I personally picked these out in the Seamoth)
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
            int picker = UnityEngine.Random.Range(0, 29);

            DeathRun.podAnchored = false;
            DeathRun.podSinking = false;
            DeathRun.podGravity = true;

            float x;
            float y = 0;
            float z;
            String message;
            switch (picker)
            {
                case 0: x = -27.0f; z = 464.9f; y = -77.6f; message = "Bullseye";  break; //Bullseye
                case 1: x = -86.3f; z = 454.7f; y = -62f; message = "Cul-de-Sac";  break; //Cul de sac
                case 2: x = -86.7f; z = -111.2f; y = -20f; message = "Rolled In"; break; //Rolled in
                case 3: x = 33.4f; z = -692.2f; message = "Very Remote";  break; //Very Remote
                case 4: x = -229.0f; z = -659.4f; message = "Uh Oh"; break; //Uh Oh
                case 5: x = -694.4f; z = 397.6f; message = "Won't Be Easy";  break; //Won't Be Easy
                case 6: x = -658.8f; z = 398.7f; message = "Dramatic View!"; break; // Dramatic! On a Tree Mushroom! Hard but doable...
                case 7: x = -530.0f; z = 377.1f; message = "Quite Deep"; break; // Quite deep, but wasn't too hard.
                case 8: x = 104.5f; z = -317.4f; message = "Hopefully not TOO close..."; break; // Hopefully not too close to Aurora!
                case 9: x = 151.8f; z = 592.9f; message = "Disorienting"; break;  // Deep and kelpy. Disorienting! Hard to find shallows.
                case 10: x = 98.7f; z = 717.7f; message = "Kelp Forest"; break;   // Moderate kelp forest start.
                case 11: x = -503.0f; z = -377.1f; message = "Grassy Plains"; break; // In the grassy plains -- a different flavor of start, long way from kelp
                case 12: x = -527.9f; z = 30.5f; message = "Far From Kelp"; break; // Crag near grass - getting to kelp a challenge
                case 13: x = 151.2f; z = -429.5f; message = "Crag"; break; // Another crag near grass.
                case 14: x = -301.5f; z = -65.5f; message = "Stinger Cave"; break; // Stinger Cave
                case 15: x = 165.2f; z = 716.9f; message = "Low Copper"; break; // Low-copper! Off towards alien structure I think.
                case 16: x = -649.4f; z = 92.2f; message = "Scarcity"; break; // Interesting ravines -- metal surprisingly scarce
                case 17: x = -272.8f; z = 262.9f; message = "Buena Vista"; break; // Good view! Interesting start.
                case 18: x = -610.1f; z = 38.4f; message = "Big Wreck"; break; // Big wreck
                case 19: x = -296.6f; z = 227.1f; message = "Deep Wreck"; break; // Deep Wreck
                case 20: x = 131.8f; z = -399.8f; message = "Very Difficult!"; break; //Right by Delgasi cave. Very hard!But I was able to make oxygen tank, fins, scanner.Seems doable.
                case 21: x = -343.4f; z = -226.2f; message = "Deep Delgasi";  break; // Deep Delgasi
                case 22: x = -662.0f; z = 216.4f; message = "Dropoff"; break; // Dropoff
                case 23: x = -803.5f; z = 252.7f; message = "Precipice!"; break; // Precipice!
                case 24: x = -661f; z = 237f; y = -100.0f; message = "Hundred Below"; break; // Hundred Below
                case 25: x = 105.2f; z = 254.4f; message = "Kelpy"; break; // Kelpy Goodness
                case 26: x = 97.7f; z = 626.2f; message = "Deep and Interesting"; break; // Deep and interesting
                case 27: x = 201.1f; z = -360.6f; message = "Jellyshroom";  break; // Near DelGasi
                default: x = 23.1f; z = 112.4f; message = "Shallow Arch";  break; // Shallow on an arch
            }

            DeathRun.startName = message;

            ErrorMessage.AddMessage("\"" + message + "\"");

            __result.x = x;
            __result.y = y;
            __result.z = z;
            return false;
        }
    }
}
