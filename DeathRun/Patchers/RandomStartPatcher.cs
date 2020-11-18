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
            ErrorMessage.AddMessage("Start Point");
            __result.x = 131.8f;
            __result.y = 0;
            __result.z = -317.4f;
            return false;
        }
    }
}
