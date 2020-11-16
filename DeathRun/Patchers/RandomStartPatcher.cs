using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

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
