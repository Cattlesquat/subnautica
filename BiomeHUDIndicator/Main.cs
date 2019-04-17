namespace BiomeHUDIndicator
{
    using System;
    using System.Reflection;
    using Harmony;
    using Items;
    using UnityEngine;
    using Common;

    public static class Main
    {
        public const string modName = "[BiomeHUDIndicator]";

        public static readonly string modFolder = "./QMods/BiomeHUDIndicator/";
        public static GameObject biomeHUD { private set; get; }
        
        public static void Patch()
        {
            SeraLogger.PatchStart(modName, "1.0.0");
            try
            {
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
