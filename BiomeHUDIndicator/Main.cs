namespace BiomeHUDIndicator
{
    using System;
    using System.Reflection;
    using Harmony;
    using BiomeHUDIndicator.Fabricator;
    using UnityEngine;

    public static class Main
    {
        public static readonly string modFolder = "./QMods/BiomeHUDIndicator/";
        public static readonly string assetsFolder = modFolder + "Assets/";
        public static readonly string assetsBundle = assetsFolder + "biomehudchip";
        public static GameObject biomeHUD { private set; get; }
        // Just to be clear, this is the entry method
        public static void Patch()
        {
            UnityEngine.Debug.Log("[BiomeHUDIndicator] Start patching. Version: 1.0.0.0");
            try
            {
                AssetBundle ab = AssetBundle.LoadFromFile(assetsBundle);
                biomeHUD = ab.LoadAsset("Canvas") as GameObject;
                CompassCore.PatchIt();
                var harmony = HarmonyInstance.Create("seraphimrisen.biomehudindicator.mod");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                UnityEngine.Debug.Log("[BiomeHUDIndicator] Patching complete.");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log($"[BiomeHUDIndicator] ERROR: {e.ToString()}");
            }
        }
    }
}
