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
                try
                {
                    biomeHUD = ab.LoadAsset("biomeCanvas") as GameObject;
                    SeraLogger.Message(modName, "(biomeCanvas) BiomeHUD.GetComponent<Text>().text: " + biomeHUD.GetComponent<Text>().text);
                }
                catch (Exception ex)
                {
                    SeraLogger.GenericError(modName, ex);
                    try
                    {
                        biomeHUD = ab.LoadAsset("Canvas") as GameObject;
                        SeraLogger.Message(modName, "(Canvas) BiomeHUD.GetComponent<Text>().text: " + biomeHUD.GetComponent<Text>().text);
                    }
                    catch (Exception exc)
                    {
                        SeraLogger.GenericError(modName, exc);
                    }
                }
                
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
