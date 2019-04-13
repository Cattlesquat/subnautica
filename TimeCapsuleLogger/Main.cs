namespace TimeCapsuleLogger
{
    using System;
    using Common;
    using System.Reflection;
    using Harmony;
    using UnityEngine;

    // Just for the purpose of logging time capsule code
    public class Main
    {
        public static void Patch()
        {
            string modName = "[TimeCapsuleLogger]";
            SeraLogger.PatchStart(modName, "1.0.0");
            UnityEngine.Debug.Log("[TimeCapsuleLogger] Start patching. Version: 1.0.0.0");
            try
            {
                var harmony = HarmonyInstance.Create("seraphimrisen.timecapsulelogger.mod");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                UnityEngine.Debug.Log("[TimeCapsuleLogger] Patching complete.");
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
            ErrorMessage.AddMessage("Time Capsule detected!");
        }
    }
}





