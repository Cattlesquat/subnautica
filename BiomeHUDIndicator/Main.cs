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
        public static readonly string assetBundle = assetFolder + "biomehudchip";
        public static GameObject biomeHUD { private set; get; }

        public static void Patch()
        {
            SeraLogger.PatchStart(modName, "1.0.0");
            try
            {
                SeraLogger.Message(modName, "AssetBundle path: " + assetBundle);
                AssetBundle ab = AssetBundle.LoadFromFile(assetBundle);
                if (ab == null)
                    SeraLogger.Message(modName, "(biomeCanvas) ab is NULL");
                biomeHUD = ab.LoadAsset("biomeCanvas") as GameObject;
                if (biomeHUD == null)
                    SeraLogger.Message(modName, "(biomeCanvas) biomeHUD is NULL");
                else
                {
                    biomeHUD = ab.LoadAsset("Canvas") as GameObject;
                    if (biomeHUD == null)
                    {
                        SeraLogger.Message(modName, "(Canvas) BiomeHUD is NULL");
                    }
                }
                if (biomeHUD.GetComponent<Text>().text == null)
                    SeraLogger.Message(modName, "biomeHUD.Text.text is NULL");
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
