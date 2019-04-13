namespace BiomeHUDIndicator
{
    using System;
    using System.Reflection;
    using Harmony;
    using BiomeHUDIndicator.Fabricator;
    using UnityEngine;
    using Common;

    public static class Main
    {
        public const string modName = "[BiomeHUDIndicator]";

        public static readonly string modFolder = "./QMods/BiomeHUDIndicator/";
        public static readonly string assetsFolder = modFolder + "Assets/";
        public static readonly string assetsBundle = assetsFolder + "biomehudchip";
        public static GameObject biomeHUD { private set; get; }
        // Just to be clear, this is the entry method
        public static void Patch()
        {
            SeraLogger.PatchStart(modName, "1.0.0");
            try
            {
                AssetBundle ab = AssetBundle.LoadFromFile(assetsBundle);
                biomeHUD = ab.LoadAsset("Canvas") as GameObject;
                CompassCore.PatchIt();
                var harmony = HarmonyInstance.Create("seraphimrisen.biomehudindicator.mod");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                SeraLogger.PatchComplete(modName);
            }
            catch (Exception ex)
            {
                SeraLogger.PatchFailed(modName, ex);
            }
        }
    }
}
