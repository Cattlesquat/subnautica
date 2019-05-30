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

        public static bool specialtyTanks = true;

        public static void Patch()
        {
            SeraLogger.PatchStart(modName, "1.2.2");
            try
            {
                var harmony = HarmonyInstance.Create("seraphimrisen.nitrogenmod.mod");

                AssetBundle ab = AssetBundle.LoadFromFile(assetBundle);
                N2HUD = ab.LoadAsset("NMHUD") as GameObject;

                harmony.PatchAll(Assembly.GetExecutingAssembly());
                OptionsPanelHandler.RegisterModOptions(new NitrogenOptions());

                DummySuitItems.PatchDummyItems();
                ReinforcedSuitsCore.PatchSuits();
                if(specialtyTanks)
                    O2TanksCore.PatchTanks();
                
                SeraLogger.PatchComplete(modName);
            }
            catch (Exception ex)
            {
                SeraLogger.PatchFailed(modName, ex);
            }
        }
    }
}