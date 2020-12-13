/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 *  Random start points -- adjust our starting spots to "more challenging places" (I personally picked these out in the Seamoth)
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
    public class StartSpot
    {
        public float x, y, z;
        public string message;

        public StartSpot()
        {
            x = 0;
            y = 0;
            z = 0;
            message = "";
        }

        public StartSpot(float x, float y, float z, string message)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.message = message;
        }

        public StartSpot(float x, float z, string message)
        {
            y = 0;

            this.x = x;
            this.z = z;
            this.message = message;
        }
    }

    [HarmonyPatch(typeof(RandomStart))]
    [HarmonyPatch("GetRandomStartPoint")]
    internal class RandomStartPatcher
    {
        // Adjust our starting spots to "more challenging places" (I personally picked these out in the Seamoth)
        public static List<StartSpot> spots = new List<StartSpot>()
        {
            // These start the pod underwater so that it can get to places with "overhead environments"
            new StartSpot (-28.6f, -75.6f, 466.9f, "Bullseye"),  //-27.0, -77.6, 494.9
            new StartSpot (-86.3f, -62f, 454.7f, "Cul-de-Sac"),
            new StartSpot (-86.0f, -20f, -114.2f, "Rolled In"),
            new StartSpot (-661f, -100.0f, 237f, "Hundred Below"), 

            // These start the pod on the surface and it sinks
            new StartSpot (33.4f, -692.2f, "Very Remote"),
            new StartSpot (-232.1f, -662.7f, "Uh Oh"),         // -229.0f, -659.4f
            new StartSpot (-694.4f, 397.6f, "Won't Be Easy"),
            new StartSpot (-658.8f, 398.7f, "Dramatic View!"), // On a tree mushroom! Hard but doable...
            new StartSpot (-530.0f, 377.1f, "Quite Deep"),     // Quite deep, but wasn't too hard.
            new StartSpot (104.5f, -317.4f, "TOO close...?"),  // Hopefully not too close to Aurora!
            new StartSpot (151.8f, 592.9f, "Disorienting"),    // Deep and kelpy. Disorienting! Hard to find shallows.
            new StartSpot (98.7f, 717.7f, "Kelp Forest"),      // Moderate kelp forest start.
            new StartSpot (-503.0f, -377.1f, "Barren: Very Hard"), // In the grassy plains -- a different flavor of start, long way from kelp
            new StartSpot (-527.9f, 30.5f, "Far From Kelp"),   // Crag near grass - getting to kelp a challenge
            new StartSpot (151.2f, -429.5f, "Crag"),           // Another crag near grass.
            new StartSpot (-301.5f, -65.5f, "Stinger Cave"),   
            new StartSpot (165.2f, 716.9f, "Low Copper"),      // Low-copper! Off towards alien structure I think.
            new StartSpot (-649.4f, 92.2f, "Scarcity"),        // Interesting ravines -- metal surprisingly scarce
            new StartSpot (-272.8f, 262.9f, "Buena Vista"),    // Good view! Interesting start.
            new StartSpot (-610.1f, 38.4f, "Big Wreck"),       
            new StartSpot (-296.6f, 227.1f, "Deep Wreck"),     
            new StartSpot (131.8f, -399.8f, "Very Difficult!"),// Right by Degasi cave. Very hard! But I was able to make oxygen tank, fins, scanner. Seems doable.
            new StartSpot (-343.4f, -226.2f, "Deep Degasi"),  
            new StartSpot (-803.5f, 252.7f, "Precipice!"),
            new StartSpot (105.2f, 254.4f, "Kelpy"),
            new StartSpot (97.7f, 626.2f, "Deep and Unusual"), // Deep and interesting
            new StartSpot (201.1f, -360.6f, "Jellyshroom"),    // Near Degasi
            new StartSpot (23.1f, 112.4f, "Shallow Arch")      // Shallow on an arch
        };


        [HarmonyPrefix]
        public static bool Prefix(EscapePod __instance, ref Vector3 __result)
        {
            int picker = UnityEngine.Random.Range(0, spots.Count);            
            StartSpot spot = spots[picker];

            // If a specific spot was specified in the config, use that instead.
            foreach (StartSpot s in spots) {
                if (s.message.Equals(DeathRun.config.startLocation))
                {
                    spot = s;
                    break;
                }
            }

            DeathRun.saveData.podSave.podGravity  = true;  // Pod should sink
            DeathRun.saveData.podSave.podSinking  = false; // ... but isn't sinking yet
            DeathRun.saveData.podSave.podAnchored = false; // ... and hasn't come to rest on the bottom
            DeathRun.saveData.startSave           = spot;  // Here's where we started

            //ErrorMessage.AddMessage("\"" + spot.message + "\"");
            DeathRunUtils.CenterMessage("DEATH RUN", 10, 2);
            DeathRunUtils.CenterMessage("Start: \"" + spot.message + "\"", 10, 3);

            __result.x = spot.x;
            __result.y = spot.y;
            __result.z = spot.z;
            return false;
        }
    }
}
