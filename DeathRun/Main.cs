/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 * Much of the Nitrogen & Bends code from Seraphim Risen's NitrogenMod (just rebalanced and w/ more UI feedback)
 */
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
    using QModManager.API.ModLoading;

    [QModCore]
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

        //private static readonly string configPath = @"./QMods/DeathRun/config.json";

        private static void LoadConfig()
        {
            //config = new RadiationConfig();
            return;

            //if (!File.Exists(configPath))
            //{
            //    config = new RadiationConfig();
            //    return;
            //}
            //
            //var json = File.ReadAllText(configPath);
            //config = JsonConvert.DeserializeObject<RadiationConfig>(json);
        }

        public static void SaveConfig()
        {
            //var json = JsonConvert.SerializeObject(config, Formatting.Indented);
            //File.WriteAllText(configPath, json);
        }

        internal static Config config { get; } = OptionsPanelHandler.Main.RegisterModOptions<Config>();

        public static void Patch()
        {
            SeraLogger.PatchStart(modName, "1.5.1");
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

                harmony.PatchAll(Assembly.GetExecutingAssembly());

                DummySuitItems.PatchDummyItems();
                ReinforcedSuitsCore.PatchSuits();
                if(specialtyTanks)
                    O2TanksCore.PatchTanks();

                Console.WriteLine(typeof(NitroDamagePatcher).AssemblyQualifiedName);

                SeraLogger.Message(modName, "Explosion Depth");

                if (config.explosionDepth > 0)
                {
                    harmony.Patch(typeof(CrashedShipExploder).GetMethod("CreateExplosiveForce", BindingFlags.NonPublic | BindingFlags.Instance),
                         null, new HarmonyMethod(typeof(PatchExplosion).GetMethod("CreateExplosiveForce")));
                }

                SeraLogger.Message(modName, "Poisoned Air");
                if (config.poisonedAir)
                {
                    harmony.Patch(AccessTools.Method(typeof(Player), "CanBreathe"),
                        new HarmonyMethod(typeof(PatchBreathing).GetMethod("CanBreathe")), null);

                    harmony.Patch(AccessTools.Method(typeof(Player), "GetBreathPeriod"), null,
                        new HarmonyMethod(typeof(PatchBreathing).GetMethod("GetBreathPeriod")));

                    harmony.Patch(AccessTools.Method(typeof(OxygenManager), "AddOxygenAtSurface"),
                        new HarmonyMethod(typeof(PatchBreathing).GetMethod("AddOxygenAtSurface")), null);

                    harmony.Patch(AccessTools.Method(typeof(WaterAmbience), "PlayReachSurfaceSound"),
                        new HarmonyMethod(typeof(PatchBreathing).GetMethod("PlayReachSurfaceSound")), null);

                    harmony.Patch(AccessTools.Method(typeof(PipeSurfaceFloater), "GetProvidesOxygen"),
                        new HarmonyMethod(typeof(PatchBreathing).GetMethod("GetProvidesOxygen")), null);
                }

                SeraLogger.Message(modName, "Radiation Warning");
                if (config.radiationWarning)
                {
                    harmony.Patch(AccessTools.Method(typeof(uGUI_RadiationWarning), "IsRadiated"),
                        new HarmonyMethod(typeof(PatchRadiation).GetMethod("IsRadiated")), null);
                }

                SeraLogger.Message(modName, "Radiative Depth");
                if (config.radiativeDepth > 0)
                {
                    harmony.Patch(AccessTools.Method(typeof(RadiatePlayerInRange), "Radiate"),
                        new HarmonyMethod(typeof(PatchRadiation).GetMethod("Radiate")), null);

                    harmony.Patch(AccessTools.Method(typeof(DamagePlayerInRadius), "DoDamage"),
                        new HarmonyMethod(typeof(PatchRadiation).GetMethod("DoDamage")), null);

                    if (config.radiativePowerAddMultiplier > 0)
                    {
                        harmony.Patch(AccessTools.Method(typeof(PowerSystem), "AddEnergy"),
                            new HarmonyMethod(typeof(PatchPower).GetMethod("AddEnergyBase")), null);

                        harmony.Patch(AccessTools.Method(typeof(EnergyMixin), "AddEnergy"),
                            new HarmonyMethod(typeof(PatchPower).GetMethod("AddEnergyTool")), null);

                        harmony.Patch(AccessTools.Method(typeof(Vehicle), "AddEnergy", new Type[] { typeof(float) }),
                            new HarmonyMethod(typeof(PatchPower).GetMethod("AddEnergyVehicle")), null);
                    }

                    if (config.radiativePowerConsumptionMultiplier > 0)
                    {
                        harmony.Patch(AccessTools.Method(typeof(PowerSystem), "ConsumeEnergy"),
                            new HarmonyMethod(typeof(PatchPower).GetMethod("ConsumeEnergyBase")), null);

                        harmony.Patch(AccessTools.Method(typeof(EnergyMixin), "ConsumeEnergy"),
                            new HarmonyMethod(typeof(PatchPower).GetMethod("ConsumeEnergyTool")), null);

                        harmony.Patch(AccessTools.Method(typeof(Vehicle), "ConsumeEnergy", new Type[] { typeof(float) }),
                            new HarmonyMethod(typeof(PatchPower).GetMethod("ConsumeEnergyVehicle")), null);
                    }
                }

                SeraLogger.Message(modName, "Disable Fabricator Food");
                if (config.disableFabricatorFood)
                {
                    harmony.Patch(AccessTools.Method(typeof(CrafterLogic), "IsCraftRecipeFulfilled"),
                        new HarmonyMethod(typeof(PatchItems).GetMethod("IsCraftRecipeFulfilled")), null);
                }

                SeraLogger.Message(modName, "Disable Food Pickup");
                if (config.preventPreRadiativeFoodPickup || config.preventRadiativeFoodPickup)
                {
                    // Disable the hover hand, and disable ability to click
                    harmony.Patch(AccessTools.Method(typeof(PickPrefab), "OnHandHover"),
                        new HarmonyMethod(typeof(PatchItems).GetMethod("HandleItemPickup")), null);
                    harmony.Patch(AccessTools.Method(typeof(PickPrefab), "OnHandClick"),
                        new HarmonyMethod(typeof(PatchItems).GetMethod("HandleItemPickup")), null);

                    harmony.Patch(AccessTools.Method(typeof(RepulsionCannon), "ShootObject"),
                        new HarmonyMethod(typeof(PatchItems).GetMethod("ShootObject")), null);

                    harmony.Patch(AccessTools.Method(typeof(PropulsionCannon), "ValidateObject"),
                        new HarmonyMethod(typeof(PatchItems).GetMethod("ValidateObject")), null);

                    // Don't let player smash the resources for seeds
                    harmony.Patch(typeof(Knife).GetMethod("GiveResourceOnDamage", BindingFlags.NonPublic | BindingFlags.Instance),
                        new HarmonyMethod(typeof(PatchItems).GetMethod("GiveResourceOnDamage")), null);
                }

                Console.WriteLine("[DeathRun] Patched");
            }
            catch (Exception ex)
            {
                SeraLogger.PatchFailed(modName, ex);
            }
        }
    }
}
