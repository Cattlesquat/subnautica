namespace BiomeHUDIndicator
{
    using System;
    using System.Reflection;
    using Harmony;
    using Items;
    using UnityEngine;
    using UnityEngine.UI;
    using Common;

    public static class Main
    {
        public const string modName = "[BiomeHUDIndicator]";

        public static readonly string modFolder = "./QMods/BiomeHUDIndicator/";
        
        public static readonly string assetFolder = modFolder + "Assets/";
        public static string assetBundle = assetFolder + "biomehudchip";
        public static GameObject BiomeHUD { private set; get; }

        public static void Patch()
        {
            SeraLogger.PatchStart(modName, "1.5.2");
            try
            {
                AssetBundle ab = AssetBundle.LoadFromFile(assetBundle);
                BiomeHUD = ab.LoadAsset("biomeCanvas") as GameObject;
                CompassCore.PatchCompasses();
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
