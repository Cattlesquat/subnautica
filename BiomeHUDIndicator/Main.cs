namespace BiomeHUDIndicator
{
    using System;
    using System.Reflection;
    using Harmony;
    using Items;
    using UnityEngine;
    using Common;
    using SMLHelper.V2.Handlers;

    public static class Main
    {
        public const string modName = "[BiomeHUDIndicator]";

        private const string modFolder = "./QMods/BiomeHUDIndicator/";
        private const string assetFolder = modFolder + "Assets/";
        private const string assetBundle = assetFolder + "biomehudchip";

        public static GameObject BiomeHUD { get; set; }

        public static bool animationsEnabled = true;
        public static bool imagesEnabled = true;
        public static byte imageAlpha = 255;

        public static void Patch()
        {
            SeraLogger.PatchStart(modName, "2.0.3");
            try
            {
                AssetBundle ab = AssetBundle.LoadFromFile(assetBundle);
                BiomeHUD = ab.LoadAsset("biomeCanvas") as GameObject;

                CompassCore.PatchCompasses();

                BiomeDisplayOptions savedSettings = new BiomeDisplayOptions();
                OptionsPanelHandler.RegisterModOptions(savedSettings);
                animationsEnabled = savedSettings.animationEnabled;
                imagesEnabled = savedSettings.imageEnabled;
                imageAlpha = savedSettings.alphaValue;

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