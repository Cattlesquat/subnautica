namespace NitrogenMod
{
    using System;
    using System.Reflection;
    using HarmonyLib;
    using SMLHelper.V2.Handlers;
    using Common;
    using Items;
    using UnityEngine;
    using Patchers;

    public class Main
    {
        public const string modName = "[NitrogenMod]";

        private const string modFolder = "./QMods/DeathRun/";
        private const string assetFolder = modFolder + "Assets/";
        private const string assetBundle = assetFolder + "n2warning";
        public static GameObject N2HUD { get; set; }

        public static bool specialtyTanks = true;
        public static bool nitrogenEnabled = true;
        public static bool decompressionVehicles = false;

        public static void Patch()
        {
            SeraLogger.PatchStart(modName, "1.5.1");
            try
            {
                Harmony harmony = new Harmony("seraphimrisen.nitrogenmod.mod");

                AssetBundle ab = AssetBundle.LoadFromFile(assetBundle);
                N2HUD = ab.LoadAsset("NMHUD") as GameObject;

                NitrogenOptions savedSettings = new NitrogenOptions();
                OptionsPanelHandler.RegisterModOptions(savedSettings);

                nitrogenEnabled = savedSettings.nitroEnabled;
                decompressionVehicles = savedSettings.decompressionVehicles;
                NitroDamagePatcher.Lethality(savedSettings.nitroLethal);
                NitroDamagePatcher.AdjustScaler(savedSettings.damageScaler);
                NitroDamagePatcher.SetDecomVeh(decompressionVehicles);
                BreathPatcher.EnableCrush(savedSettings.crushEnabled);

                harmony.PatchAll(Assembly.GetExecutingAssembly());

                DummySuitItems.PatchDummyItems();
                ReinforcedSuitsCore.PatchSuits();
                if(specialtyTanks)
                    O2TanksCore.PatchTanks();

                Console.WriteLine(typeof(NitroDamagePatcher).AssemblyQualifiedName);
                SeraLogger.PatchComplete(modName);
            }
            catch (Exception ex)
            {
                SeraLogger.PatchFailed(modName, ex);
            }
        }
    }
}