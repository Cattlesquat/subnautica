namespace NitrogenMod
{
    using System;
    using System.Reflection;
    using Harmony;
    using SMLHelper.V2.Handlers;
    using Common;
    using Items;
    using UnityEngine;

    public class Main
    {
        public const string modName = "[NitrogenMod]";

        public const string modFolder = "./QMods/NitrogenMod/";
        private const string assetFolder = modFolder + "Assets/";
        private const string assetBundle = assetFolder + "n2warning";
        public static GameObject N2HUD { get; set; }

        public static void Patch()
        {
            SeraLogger.PatchStart(modName, "1.0.0");
            try
            {
                var harmony = HarmonyInstance.Create("seraphimrisen.nitrogenmod.mod");

                AssetBundle ab = AssetBundle.LoadFromFile(assetBundle);
                N2HUD = ab.LoadAsset("NMHUD") as GameObject;

                DummySuitItems.PatchDummyItems();
                ReinforcedSuitsCore.PatchSuits();
                O2TanksCore.PatchTanks();

                harmony.PatchAll(Assembly.GetExecutingAssembly());
                OptionsPanelHandler.RegisterModOptions(new NitrogenOptions());

                SeraLogger.PatchComplete(modName);
            }
            catch (Exception ex)
            {
                SeraLogger.PatchFailed(modName, ex);
            }
        }
    }
}