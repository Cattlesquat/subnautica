namespace TimeCapsuleLogger
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Linq;
    using Harmony;
    using UnityEngine;
    
    // Just for the purpose of logging time capsule code
    public class Main
    {
        public static void Patch()
        {
            UnityEngine.Debug.Log("[TimeCapsuleLogger] Start patching. Version: 1.0.0.0");
            try
            {
                var harmony = HarmonyInstance.Create("seraphimrisen.timecapsulelogger.mod");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                UnityEngine.Debug.Log("[NitrogenMod] Patching complete.");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log($"[TimeCapsuleLogger] ERROR: {e.ToString()}");
            }
        }
    }

    [HarmonyPatch(typeof(TimeCapsule))]
    [HarmonyPatch("Spawn")]
    internal class SpawnLogger
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            ErrorMessage.AddMessage("TimeCapsule.Spawn() has run!");
        }
    }
}