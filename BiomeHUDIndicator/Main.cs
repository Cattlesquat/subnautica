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

        public const string modFolder = "./QMods/BiomeHUDIndicator/";
        private const string assetFolder = modFolder + "Assets/";
        private const string assetBundle = assetFolder + "biomehudchip";
        public static GameObject BiomeHUD { get; set; }

        public static void Patch()
        {
            SeraLogger.PatchStart(modName, "2.0.1");
            try
            {
                AssetBundle ab = AssetBundle.LoadFromFile(assetBundle);
                BiomeHUD = ab.LoadAsset("biomeCanvas") as GameObject;

                CompassCore.PatchCompasses();

                var harmony = HarmonyInstance.Create("seraphimrisen.biomehudindicator.mod");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                OptionsPanelHandler.RegisterModOptions(new BiomeDisplayOptions());

                SeraLogger.PatchComplete(modName);
            }
            catch (Exception ex)
            {
                SeraLogger.PatchFailed(modName, ex);
            }
        }
    }
}