namespace NoObservatoryMusic
{
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Harmony;
using UnityEngine;

    // If all goes as planned this should basically instantly nullify the observatory player biome
    public static class Main
    {
        public static void Patch()
        {
            UnityEngine.Debug.Log("[NoObservatoryMusic] Start patching. Version: 1.0.0.0");
            try
            {
                var harmony = HarmonyInstance.Create("com.noobservatorymusic.mod");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                UnityEngine.Debug.Log("[NoObservatoryMusic] Patching complete.");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log($"[NoObservatoryMusic] ERROR: {e.ToString()}");
            }
        }
    }
}
