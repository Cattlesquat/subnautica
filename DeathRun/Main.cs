namespace DeathRun
{
    using System;
    using System.Reflection;
    using HarmonyLib;
    using SMLHelper.V2.Handlers;
    using Common;
    using Items;
    using UnityEngine;
    using Patchers;

    //
    // Much of Nitrogen/Bends and Crush-Depth code adapted from SeraphimRisen's NitrogenMod
    //

    public class Main
    {
        public const string modName = "[DeathRun]";

        private const string modFolder = "./QMods/DeathRun/";
        private const string assetFolder = modFolder + "Assets/";
        private const string assetBundle = assetFolder + "n2warning";
        public static GameObject N2HUD { get; set; }

        public static bool specialtyTanks = true;
        public static bool nitrogenEnabled = true;
        public static bool decompressionVehicles = false;

        public static void Patch()
        {
            SeraLogger.PatchStart(modName, "1.1.1.1");
            try
            {
                Harmony harmony = new Harmony("cattlesquat.deathrun.mod");

                AssetBundle ab = AssetBundle.LoadFromFile(assetBundle);
                N2HUD = ab.LoadAsset("NMHUD") as GameObject;

                DeathRunOptions savedSettings = new DeathRunOptions();
                OptionsPanelHandler.RegisterModOptions(savedSettings);

                nitrogenEnabled = savedSettings.nitroEnabled;
                decompressionVehicles = savedSettings.decompressionVehicles;
                NitroDamagePatcher.Lethality(savedSettings.nitroLethal);
                NitroDamagePatcher.AdjustScaler(savedSettings.damageScaler);
                NitroDamagePatcher.SetDecomVeh(decompressionVehicles);
                BreathPatcher.EnableCrush(savedSettings.crushEnabled);

                //harmony.PatchAll(Assembly.GetExecutingAssembly());
                harmony.PatchAll();

                //NitrogenLevel poop = new NitrogenLevel();
                //NitroDamagePatcher.Prefix(ref poop);

                DummySuitItems.PatchDummyItems();
                ReinforcedSuitsCore.PatchSuits();
                if(specialtyTanks)
                    O2TanksCore.PatchTanks();

                SeraLogger.Message(modName, "Whee!");

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