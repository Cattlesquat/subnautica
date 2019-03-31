namespace NitrogenMod
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Linq;
    using Harmony;
    using UnityEngine;

    public class Main
    {
        public static void Patch()
        {
            UnityEngine.Debug.Log("[NitrogenMod] Start patching. Version: 1.0.0.0");
            try
            {
                var harmony = HarmonyInstance.Create("seraphimrisen.nitrogenmod.mod");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                UnityEngine.Debug.Log("[NitrogenMod] Patching complete.");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log($"[NitrogenMod] ERROR: {e.ToString()}");
            }
        }
    }
}
