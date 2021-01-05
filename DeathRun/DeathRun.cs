/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 * * Much of the Nitrogen & Bends code from Seraphim Risen's NitrogenMod (but entirely new main algorithm and w/ more UI feedback)
 * * Radiation Mod material from libraryaddict, used with permission. 
 * * Escape Pod Unleashed material from oldark, w/ fixes provided to make pod gravity work reliably.
 * * Substantial increase in damage taken
 * * More aggressive creatures
 * * "Cause of Death" and "Time of Death" reporting
 * * Higher energy costs, especially to fabricate
 * * More warnings about explosion, radiation, etc.
 * * Murky water option
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
    using SMLHelper.V2.Crafting;
    using System.Collections.Generic;

    [QModCore]
    public class DeathRun
    {        
        public const string modID = "DeathRun";
        public const string modName = "[DeathRun]";
        public const string SaveFile = modID + "_" + "SavedGame.json";
        public const string StatsFile = modID + "_" + "Stats.json";

        // DeathRun's saved games are handled in DeathRunUtils
        public static DeathRunSaveData saveData = new DeathRunSaveData();
        public static DeathRunSaveListener saveListener;

        // DeathRun's "Stats" are saved and loaded in DeathRunUtils
        public static DeathRunStats statsData = new DeathRunStats();

        public const string modFolder = "./QMods/DeathRun/";
        private const string assetFolder = modFolder + "Assets/";
        private const string assetBundle = assetFolder + "n2warning";

        public static GameObject N2HUD { get; set; }

        public static global::Utils.ScalarMonitor countdownMonitor { get; set; } = new global::Utils.ScalarMonitor(0f);
        public static global::Utils.ScalarMonitor playerMonitor { get; set; } = new global::Utils.ScalarMonitor(0f);

        //public static bool podGravity  = true;
        public static float configDirty = 0;

        public static bool murkinessDirty    = false;
        public static bool encyclopediaAdded = false;

        // These semaphore relate to "flavors" of energy consumption
        public static bool craftingSemaphore = false;
        public static bool chargingSemaphore = false;
        public static bool filterSemaphore   = false;
        public static bool scannerSemaphore  = false;

        // So that the explody fish stay hidden in ambush
        public static bool crashFishSemaphore = false;

        // Don't do extra warnings during respawn process while player is already dead
        public static bool playerIsDead = false;

        // Let player know if patch didn't complete
        public static bool patchFailed = false;

        public const string CAUSE_UNKNOWN = "Unknown";
        public const string CAUSE_UNKNOWN_CREATURE = "Unknown Creature";

        public const float FULL_AGGRESSION = 2400; // 40 minutes
        public const float MORE_AGGRESSION = 1200; // 20 minutes

        // Temporary storage for "cause of death"
        public static string cause = CAUSE_UNKNOWN;
        public static GameObject causeObject = null;

        // An even more annoying code-path where a cinematic has to finish running before the player dies
        public static string cinematicCause = CAUSE_UNKNOWN;
        public static GameObject cinematicCauseObject = null;

        internal static Config config { get; } = OptionsPanelHandler.Main.RegisterModOptions<Config>();

        public static void Patch()
        {
            CattleLogger.setModName(modName);
            CattleLogger.PatchStart("1.4.1");

            try
            {
                Harmony harmony = new Harmony("cattlesquat.deathrun.mod");

                CattleLogger.Message("Asset Bundle");

                AssetBundle ab = AssetBundle.LoadFromFile(assetBundle);
                N2HUD = ab.LoadAsset("NMHUD") as GameObject;

                CattleLogger.Message("Warn-Failure Patch");

                harmony.Patch(AccessTools.Method(typeof(MainMenuController), "Start"),
                    null, new HarmonyMethod(typeof(WarnFailurePatcher).GetMethod("Postfix")));

                //harmony.Patch(AccessTools.Method(typeof(IngameMenu), "Awake"),
                //    null, new HarmonyMethod(typeof(WarnFailurePatcher).GetMethod("Postfix")));

                CattleLogger.Message("Main Patch");

                harmony.PatchAll(Assembly.GetExecutingAssembly());

                CattleLogger.Message("Items");

                DummySuitItems.PatchDummyItems();
                ReinforcedSuitsCore.PatchSuits();
                if (DeathRun.config.enableSpecialtyTanks)
                {
                    O2TanksCore.PatchTanks();
                }

                //Console.WriteLine(typeof(NitroDamagePatcher).AssemblyQualifiedName);

                CattleLogger.Message("Explosion Depth");

                harmony.Patch(typeof(CrashedShipExploder).GetMethod("CreateExplosiveForce", BindingFlags.NonPublic | BindingFlags.Instance),
                     null, new HarmonyMethod(typeof(ExplosionPatcher).GetMethod("CreateExplosiveForce")));

                CattleLogger.Message("Surface Air Poisoning");
                //if (config.poisonedAir)
                //{
                    harmony.Patch(AccessTools.Method(typeof(Player), "CanBreathe"),
                        new HarmonyMethod(typeof(BreathingPatcher).GetMethod("CanBreathe")), null);

                    harmony.Patch(AccessTools.Method(typeof(Player), "GetBreathPeriod"), null,
                        new HarmonyMethod(typeof(BreathingPatcher).GetMethod("GetBreathPeriod")));

                    harmony.Patch(AccessTools.Method(typeof(OxygenManager), "AddOxygenAtSurface"),
                        new HarmonyMethod(typeof(BreathingPatcher).GetMethod("AddOxygenAtSurface")), null);

                    harmony.Patch(AccessTools.Method(typeof(WaterAmbience), "PlayReachSurfaceSound"),
                        new HarmonyMethod(typeof(BreathingPatcher).GetMethod("PlayReachSurfaceSound")), null);

                    //harmony.Patch(AccessTools.Method(typeof(PipeSurfaceFloater), "GetProvidesOxygen"),
                    //    new HarmonyMethod(typeof(PatchBreathing).GetMethod("GetProvidesOxygen")), null);
                //}

                CattleLogger.Message("Radiation Warning");
                //if (config.radiationWarning)
                //{
                    harmony.Patch(AccessTools.Method(typeof(uGUI_RadiationWarning), "IsRadiated"),
                        new HarmonyMethod(typeof(RadiationPatcher).GetMethod("IsRadiated")), null);

                    harmony.Patch(AccessTools.Method(typeof(uGUI_RadiationWarning), "Update"),
                        new HarmonyMethod(typeof(RadiationPatcher).GetMethod("Update")), null);

                    //harmony.Patch(AccessTools.Method(typeof(uGUI_DepthCompass), "UpdateDepth"),
                    //    new HarmonyMethod(typeof(PatchRadiation).GetMethod("UpdateDepth")), null);

                //}

                CattleLogger.Message("Radiation Depth");
                //if (config.radiativeDepth > 0)
                //{
                    harmony.Patch(AccessTools.Method(typeof(RadiatePlayerInRange), "Radiate"),
                        new HarmonyMethod(typeof(RadiationPatcher).GetMethod("Radiate")), null);
                 
                    harmony.Patch(AccessTools.Method(typeof(DamagePlayerInRadius), "DoDamage"),
                        new HarmonyMethod(typeof(RadiationPatcher).GetMethod("DoDamage")), null);
                //}

                CattleLogger.Message("Power Consumption");

                //if (!Config.NORMAL.Equals(DeathRun.config.powerCosts)) { 
                harmony.Patch(AccessTools.Method(typeof(PowerSystem), "AddEnergy"),
                        new HarmonyMethod(typeof(PowerPatcher).GetMethod("AddEnergyBase")), null);

                harmony.Patch(AccessTools.Method(typeof(SolarPanel), "Update"),
                        new HarmonyMethod(typeof(PowerPatcher).GetMethod("SolarPanelUpdate")), null);

                    harmony.Patch(AccessTools.Method(typeof(EnergyMixin), "AddEnergy"),
                        new HarmonyMethod(typeof(PowerPatcher).GetMethod("AddEnergyTool")), null);

                    harmony.Patch(AccessTools.Method(typeof(Vehicle), "AddEnergy", new Type[] { typeof(float) }),
                        new HarmonyMethod(typeof(PowerPatcher).GetMethod("AddEnergyVehicle")), null);


                    harmony.Patch(AccessTools.Method(typeof(PowerSystem), "ConsumeEnergy"),
                        new HarmonyMethod(typeof(PowerPatcher).GetMethod("ConsumeEnergyBase")), null);

                    harmony.Patch(AccessTools.Method(typeof(EnergyMixin), "ConsumeEnergy"),
                        new HarmonyMethod(typeof(PowerPatcher).GetMethod("ConsumeEnergyTool")), null);

                    harmony.Patch(AccessTools.Method(typeof(Vehicle), "ConsumeEnergy", new Type[] { typeof(float) }),
                        new HarmonyMethod(typeof(PowerPatcher).GetMethod("ConsumeEnergyVehicle")), null);


                    harmony.Patch(AccessTools.Method(typeof(CrafterLogic), "ConsumeEnergy"), 
                        new HarmonyMethod(typeof(PowerPatcher).GetMethod("ConsumeEnergyFabricatorPrefix")), null);

                    harmony.Patch(AccessTools.Method(typeof(CrafterLogic), "ConsumeEnergy"), null,
                        new HarmonyMethod(typeof(PowerPatcher).GetMethod("ConsumeEnergyFabricatorPostfix")), null);

                    harmony.Patch(AccessTools.Method(typeof(FiltrationMachine), "UpdateFiltering"),
                        new HarmonyMethod(typeof(PowerPatcher).GetMethod("ConsumeEnergyFiltrationPrefix")), null);

                    harmony.Patch(AccessTools.Method(typeof(FiltrationMachine), "UpdateFiltering"), null, 
                        new HarmonyMethod(typeof(PowerPatcher).GetMethod("ConsumeEnergyFiltrationPostfix")), null);

                    harmony.Patch(AccessTools.Method(typeof(MapRoomFunctionality), "UpdateScanning"),
                        new HarmonyMethod(typeof(PowerPatcher).GetMethod("ConsumeEnergyScanningPrefix")), null);

                    harmony.Patch(AccessTools.Method(typeof(MapRoomFunctionality), "UpdateScanning"), null, 
                        new HarmonyMethod(typeof(PowerPatcher).GetMethod("ConsumeEnergyScanningPostfix")), null);

                    harmony.Patch(AccessTools.Method(typeof(Charger), "Update"),
                        new HarmonyMethod(typeof(PowerPatcher).GetMethod("ConsumeEnergyChargingPrefix")), null);

                    harmony.Patch(AccessTools.Method(typeof(Charger), "Update"), null, 
                        new HarmonyMethod(typeof(PowerPatcher).GetMethod("ConsumeEnergyChargingPostfix")), null);



                    //harmony.Patch(AccessTools.Method(typeof(RegeneratePowerSource), "Start"), null,
                    //    new HarmonyMethod(typeof(PowerPatcher).GetMethod("RegeneratePowerStart")), null);



                //CattleLogger.Message("Disable Fabricator Food");
                //if (config.disableFabricatorFood)
                //{
                //    harmony.Patch(AccessTools.Method(typeof(CrafterLogic), "IsCraftRecipeFulfilled"),
                //        new HarmonyMethod(typeof(PatchItems).GetMethod("IsCraftRecipeFulfilled")), null);
                //}

                CattleLogger.Message("Food Pickup");

                // Disable the hover hand, and disable ability to click
                harmony.Patch(AccessTools.Method(typeof(PickPrefab), "OnHandHover"),
                    new HarmonyMethod(typeof(ItemPatcher).GetMethod("HandleItemPickup")), null);
                harmony.Patch(AccessTools.Method(typeof(PickPrefab), "OnHandClick"),
                    new HarmonyMethod(typeof(ItemPatcher).GetMethod("HandleItemPickup")), null);

                harmony.Patch(AccessTools.Method(typeof(RepulsionCannon), "ShootObject"),
                    new HarmonyMethod(typeof(ItemPatcher).GetMethod("ShootObject")), null);

                harmony.Patch(AccessTools.Method(typeof(PropulsionCannon), "ValidateObject"),
                    new HarmonyMethod(typeof(ItemPatcher).GetMethod("ValidateObject")), null);

                // Don't let player smash the resources for seeds
                harmony.Patch(typeof(Knife).GetMethod("GiveResourceOnDamage", BindingFlags.NonPublic | BindingFlags.Instance),
                    new HarmonyMethod(typeof(ItemPatcher).GetMethod("GiveResourceOnDamage")), null);


                CattleLogger.Message("Vehicle Costs: " + DeathRun.config.vehicleCosts);

                Dictionary<TechType, TechData> techChanges = null;

                if (Config.NO_VEHICLES.Equals(DeathRun.config.vehicleCosts))
                {
                    CattleLogger.Message("No Vehicles");
                    techChanges = new Dictionary<TechType, TechData>
                    {
                        {
                            TechType.HatchingEnzymes,
                            new TechData
                            {
                                craftAmount = 5, // Gives you extra copies of Hatching Enzymes so that vehicles are then unlocked
                                Ingredients = new List<Ingredient>
                                {
                                    new Ingredient(TechType.EyesPlantSeed, 1),
                                    new Ingredient(TechType.SeaCrownSeed, 1),
                                    new Ingredient(TechType.TreeMushroomPiece, 1),
                                    new Ingredient(TechType.RedGreenTentacleSeed, 1),
                                    new Ingredient(TechType.KooshChunk, 1)
                                }
                            }
                        },
                        {
                            TechType.Seamoth,
                            new TechData
                            {
                                craftAmount = 1,
                                Ingredients = new List<Ingredient>
                                {
                                    new Ingredient(TechType.TitaniumIngot, 1),
                                    new Ingredient(TechType.PowerCell, 1),
                                    new Ingredient(TechType.Glass, 2),
                                    new Ingredient(TechType.Lubricant, 1),
                                    new Ingredient(TechType.Lead, 1),
                                    new Ingredient(TechType.HatchingEnzymes, 1)
                                }
                            }
                        },
                        {
                            TechType.Cyclops,
                            new TechData
                            {
                                craftAmount = 1,
                                Ingredients = new List<Ingredient>
                                {
                                    new Ingredient(TechType.PlasteelIngot, 3),
                                    new Ingredient(TechType.EnameledGlass, 3),
                                    new Ingredient(TechType.Lubricant, 1),
                                    new Ingredient(TechType.AdvancedWiringKit, 1),
                                    new Ingredient(TechType.Lead, 3),
                                    new Ingredient(TechType.HatchingEnzymes, 1)
                                }
                            }
                        },
                        {
                            TechType.Exosuit,
                            new TechData
                            {
                                craftAmount = 1,
                                Ingredients = new List<Ingredient>
                                {
                                    new Ingredient(TechType.PlasteelIngot, 2),
                                    new Ingredient(TechType.Aerogel, 1),
                                    new Ingredient(TechType.EnameledGlass, 1),
                                    new Ingredient(TechType.Diamond, 2),
                                    new Ingredient(TechType.Lead, 2),
                                    new Ingredient(TechType.HatchingEnzymes, 1)
                                }
                            }
                        },
                        {
                            TechType.Seaglide,
                            new TechData
                            {
                                craftAmount = 1,
                                Ingredients = new List<Ingredient>
                                {
                                    new Ingredient(TechType.Battery, 1),
                                    new Ingredient(TechType.Lubricant, 2),
                                    new Ingredient(TechType.CopperWire, 1),
                                    new Ingredient(TechType.Lead, 2),
                                    new Ingredient(TechType.Lithium, 2),
                                }
                            }
                        }
                    };
                }
                else if (Config.DEATH_VEHICLES.Equals(DeathRun.config.vehicleCosts) || Config.DEATH_VEHICLES_2.Equals(DeathRun.config.vehicleCosts))
                {
                    CattleLogger.Message("Death Vehicles");

                    if (Config.DEATH_VEHICLES.Equals(DeathRun.config.vehicleCosts))
                    {
                        techChanges = new Dictionary<TechType, TechData>
                        {
                            {
                                TechType.Seamoth,
                                new TechData
                                {
                                    craftAmount = 1,
                                    Ingredients = new List<Ingredient>
                                    {
                                        new Ingredient(TechType.PlasteelIngot, 1),
                                        new Ingredient(TechType.PowerCell, 1),
                                        new Ingredient(TechType.EnameledGlass, 2),
                                        new Ingredient(TechType.Lubricant, 2),
                                        new Ingredient(TechType.Lead, 4),
                                        new Ingredient(TechType.TreeMushroomPiece, 1),
                                        new Ingredient(TechType.KooshChunk, 1),
                                        new Ingredient(TechType.RedGreenTentacleSeed, 1),
                                        new Ingredient(TechType.EyesPlantSeed, 1)
                                    }
                                }
                            },
                            {
                                TechType.VehicleHullModule1,
                                new TechData
                                {
                                    craftAmount = 1,
                                    Ingredients = new List<Ingredient>
                                    {
                                        new Ingredient(TechType.TitaniumIngot, 1),
                                        new Ingredient(TechType.Magnetite, 2),
                                        new Ingredient(TechType.EnameledGlass, 1)
                                    }
                                }
                            },
                            {
                                TechType.VehicleHullModule2,
                                new TechData
                                {
                                    craftAmount = 1,
                                    Ingredients = new List<Ingredient>
                                    {
                                        new Ingredient(TechType.VehicleHullModule1, 1),
                                        new Ingredient(TechType.PlasteelIngot, 1),
                                        new Ingredient(TechType.AluminumOxide, 3),
                                        new Ingredient(TechType.EnameledGlass, 1)
                                    }
                                }
                            },
                            {
                                TechType.VehicleHullModule3,
                                new TechData
                                {
                                    craftAmount = 1,
                                    Ingredients = new List<Ingredient>
                                    {
                                        new Ingredient(TechType.VehicleHullModule2, 1),
                                        new Ingredient(TechType.PlasteelIngot, 1),
                                        new Ingredient(TechType.Sulphur, 2),
                                        new Ingredient(TechType.UraniniteCrystal, 2)
                                    }
                                }
                            },
                            {
                                TechType.Cyclops,
                                new TechData
                                {
                                    craftAmount = 1,
                                    Ingredients = new List<Ingredient>
                                    {
                                        new Ingredient(TechType.PlasteelIngot, 3),
                                        new Ingredient(TechType.EnameledGlass, 3),
                                        new Ingredient(TechType.Lubricant, 4),
                                        new Ingredient(TechType.AdvancedWiringKit, 1),
                                        new Ingredient(TechType.Lead, 3),
                                        new Ingredient(TechType.Nickel, 2),
                                        new Ingredient(TechType.Kyanite, 2)
                                    }
                                }
                            },
                            {
                                TechType.Exosuit,
                                new TechData
                                {
                                    craftAmount = 1,
                                    Ingredients = new List<Ingredient>
                                    {
                                        new Ingredient(TechType.PlasteelIngot, 2),
                                        new Ingredient(TechType.Aerogel, 3),
                                        new Ingredient(TechType.EnameledGlass, 2),
                                        new Ingredient(TechType.Diamond, 2),
                                        new Ingredient(TechType.Lead, 2),
                                        new Ingredient(TechType.Sulphur, 3),
                                        new Ingredient(TechType.UraniniteCrystal, 3),
                                        new Ingredient(TechType.Lubricant, 3)
                                    }
                                }
                            },
                            {
                                TechType.ExoHullModule1,
                                new TechData
                                {
                                    craftAmount = 1,
                                    Ingredients = new List<Ingredient>
                                    {
                                        new Ingredient(TechType.PlasteelIngot, 1),
                                        new Ingredient(TechType.Nickel, 3),
                                        new Ingredient(TechType.Kyanite, 1)
                                    }
                                }
                            },
                            {
                                TechType.Seaglide,
                                new TechData
                                {
                                    craftAmount = 1,
                                    Ingredients = new List<Ingredient>
                                    {
                                        new Ingredient(TechType.Battery, 1),
                                        new Ingredient(TechType.Lubricant, 2),
                                        new Ingredient(TechType.CopperWire, 1),
                                        new Ingredient(TechType.Lead, 2),
                                        new Ingredient(TechType.Gold, 2)
                                    }
                                }
                            }
                        };
                    } 
                    else
                    {
                        techChanges = new Dictionary<TechType, TechData>
                        {
                            {
                                TechType.Seamoth,
                                new TechData
                                {
                                    craftAmount = 1,
                                    Ingredients = new List<Ingredient>
                                    {
                                        new Ingredient(TechType.PlasteelIngot, 1),
                                        new Ingredient(TechType.PowerCell, 1),
                                        new Ingredient(TechType.EnameledGlass, 2),
                                        new Ingredient(TechType.Lubricant, 2),
                                        new Ingredient(TechType.Lead, 4),
                                        new Ingredient(TechType.HydrochloricAcid, 3),
                                        new Ingredient(TechType.Aerogel, 1)
                                    }
                                }
                            },
                            {
                                TechType.Cyclops,
                                new TechData
                                {
                                    craftAmount = 1,
                                    Ingredients = new List<Ingredient>
                                    {
                                        new Ingredient(TechType.PlasteelIngot, 3),
                                        new Ingredient(TechType.EnameledGlass, 3),
                                        new Ingredient(TechType.Lubricant, 4),
                                        new Ingredient(TechType.AdvancedWiringKit, 1),
                                        new Ingredient(TechType.Lead, 3),
                                        new Ingredient(TechType.Nickel, 2),
                                        new Ingredient(TechType.Kyanite, 2)
                                    }
                                }
                            },
                            {
                                TechType.Exosuit,
                                new TechData
                                {
                                    craftAmount = 1,
                                    Ingredients = new List<Ingredient>
                                    {
                                        new Ingredient(TechType.PlasteelIngot, 2),
                                        new Ingredient(TechType.Aerogel, 3),
                                        new Ingredient(TechType.EnameledGlass, 2),
                                        new Ingredient(TechType.Diamond, 2),
                                        new Ingredient(TechType.Lead, 2),
                                        new Ingredient(TechType.Sulphur, 3),
                                        new Ingredient(TechType.UraniniteCrystal, 3),
                                        new Ingredient(TechType.Lubricant, 3)
                                    }
                                }
                            },
                            {
                                TechType.Seaglide,
                                new TechData
                                {
                                    craftAmount = 1,
                                    Ingredients = new List<Ingredient>
                                    {
                                        new Ingredient(TechType.Battery, 1),
                                        new Ingredient(TechType.Lubricant, 2),
                                        new Ingredient(TechType.CopperWire, 1),
                                        new Ingredient(TechType.Lead, 2),
                                        new Ingredient(TechType.Gold, 2)
                                    }
                                }
                            }
                        };
                    }

                    PDAHandler.EditFragmentsToScan(TechType.Seamoth, 15);
                    PDAHandler.EditFragmentsToScan(TechType.ExosuitFragment, 7);
                    PDAHandler.EditFragmentsToScan(TechType.Seaglide, 4);
                }
                else if (Config.HARD_VEHICLES.Equals(DeathRun.config.vehicleCosts))
                {
                    CattleLogger.Message("Hard Vehicles");
                    techChanges = new Dictionary<TechType, TechData>
                    {
                        {
                            TechType.Seamoth,
                            new TechData
                            {
                                craftAmount = 1,
                                Ingredients = new List<Ingredient>
                                {
                                    new Ingredient(TechType.PlasteelIngot, 1),
                                    new Ingredient(TechType.PowerCell, 1),
                                    new Ingredient(TechType.EnameledGlass, 2),
                                    new Ingredient(TechType.Lubricant, 3),
                                    new Ingredient(TechType.Lead, 4)
                                }
                            }
                        },
                        {
                            TechType.Cyclops,
                            new TechData
                            {
                                craftAmount = 1,
                                Ingredients = new List<Ingredient>
                                {
                                    new Ingredient(TechType.PlasteelIngot, 3),
                                    new Ingredient(TechType.EnameledGlass, 3),
                                    new Ingredient(TechType.Lubricant, 4),
                                    new Ingredient(TechType.AdvancedWiringKit, 1),
                                    new Ingredient(TechType.Lead, 4),
                                    new Ingredient(TechType.Nickel, 1)
                                }
                            }
                        },
                        {
                            TechType.Exosuit,
                            new TechData
                            {
                                craftAmount = 1,
                                Ingredients = new List<Ingredient>
                                {
                                    new Ingredient(TechType.PlasteelIngot, 2),
                                    new Ingredient(TechType.Aerogel, 2),
                                    new Ingredient(TechType.EnameledGlass, 1),
                                    new Ingredient(TechType.Diamond, 2),
                                    new Ingredient(TechType.Lead, 4),
                                    new Ingredient(TechType.Sulphur, 2),
                                    new Ingredient(TechType.Lubricant, 2)
                                }
                            }
                        },
                        {
                        TechType.Seaglide,
                            new TechData
                            {
                                craftAmount = 1,
                                Ingredients = new List<Ingredient>
                                {
                                    new Ingredient(TechType.Battery, 1),
                                    new Ingredient(TechType.Lubricant, 2),
                                    new Ingredient(TechType.CopperWire, 1),
                                    new Ingredient(TechType.Lead, 1),
                                    new Ingredient(TechType.Gold, 1)
                                }
                            }
                        }

                    };
                    PDAHandler.EditFragmentsToScan(TechType.Seamoth, 10);
                    PDAHandler.EditFragmentsToScan(TechType.ExosuitFragment, 6);
                    PDAHandler.EditFragmentsToScan(TechType.Seaglide, 3);
                }

                if (techChanges != null)
                {
                    foreach (KeyValuePair<TechType,TechData> tech in techChanges)
                    {
                        CraftDataHandler.SetTechData(tech.Key, tech.Value);
                    }
                }

                CattleLogger.Message("Habitat Builder Costs");

                techChanges = null;
                if (Config.DEATHRUN.Equals(DeathRun.config.builderCosts))
                {
                    CattleLogger.Message("Death Habitat");
                    techChanges = new Dictionary<TechType, TechData>
                    {
                        {
                            TechType.Builder,
                            new TechData
                            {
                                craftAmount = 1, 
                                Ingredients = new List<Ingredient>
                                {
                                    new Ingredient(TechType.ComputerChip, 2),
                                    new Ingredient(TechType.WiringKit, 2),
                                    new Ingredient(TechType.Battery, 1),
                                    new Ingredient(TechType.Lithium, 2),
                                    new Ingredient(TechType.Magnetite, 1)
                                }
                            }
                        },
                        {
                        TechType.MedicalCabinet,
                            new TechData
                            {
                                craftAmount = 1,
                                Ingredients = new List<Ingredient>
                                {
                                    new Ingredient(TechType.ComputerChip, 1),
                                    new Ingredient(TechType.FiberMesh, 4),
                                    new Ingredient(TechType.Silver, 1),
                                    new Ingredient(TechType.Titanium, 1)
                                }
                            }
                        },
                        //{
                        //TechType.Fabricator,
                        //    new TechData
                        //    {
                        //        craftAmount = 1,
                        //        Ingredients = new List<Ingredient>
                        //        {
                        //            new Ingredient(TechType.Titanium, 1),
                        //            new Ingredient(TechType.Gold, 1),
                        //            new Ingredient(TechType.JeweledDiskPiece, 1)
                        //        }
                        //    }
                        //}
                    };


                    PDAHandler.EditFragmentsToScan(TechType.Beacon, 4);
                    PDAHandler.EditFragmentsToScan(TechType.Gravsphere, 4);
                    PDAHandler.EditFragmentsToScan(TechType.StasisRifle, 4);
                    PDAHandler.EditFragmentsToScan(TechType.PropulsionCannon, 4);
                    PDAHandler.EditFragmentsToScan(TechType.LaserCutter, 6);
                    PDAHandler.EditFragmentsToScan(TechType.LaserCutterFragment, 6);
                    PDAHandler.EditFragmentsToScan(TechType.BatteryCharger, 6);
                    PDAHandler.EditFragmentsToScan(TechType.PowerCellCharger, 6);
                    PDAHandler.EditFragmentsToScan(TechType.Constructor, 10);
                    PDAHandler.EditFragmentsToScan(TechType.BaseBioReactor, 5);
                    PDAHandler.EditFragmentsToScan(TechType.BaseNuclearReactor, 5);
                    PDAHandler.EditFragmentsToScan(TechType.ThermalPlant, 5);
                    PDAHandler.EditFragmentsToScan(TechType.BaseMoonpool, 5);
                    PDAHandler.EditFragmentsToScan(TechType.PowerTransmitter, 2);
                    PDAHandler.EditFragmentsToScan(TechType.BaseMapRoom, 5);
                    //PDAHandler.EditFragmentsToScan(TechType.BaseWaterPark, 4);
                    //PDAHandler.EditFragmentsToScan(TechType.Spotlight, 3);
                }
                else if (Config.HARD.Equals(DeathRun.config.builderCosts))
                {
                    CattleLogger.Message("Hard Habitat");
                    techChanges = new Dictionary<TechType, TechData>
                    {
                        {
                            TechType.Builder,
                            new TechData
                            {
                                craftAmount = 1,
                                Ingredients = new List<Ingredient>
                                {
                                    new Ingredient(TechType.ComputerChip, 2),
                                    new Ingredient(TechType.WiringKit, 2),
                                    new Ingredient(TechType.Battery, 1),
                                    new Ingredient(TechType.Lithium, 1),
                                }
                            }
                        },
                        {
                            TechType.MedicalCabinet,
                            new TechData
                            {
                                craftAmount = 1,
                                Ingredients = new List<Ingredient>
                                {
                                    new Ingredient(TechType.ComputerChip, 1),
                                    new Ingredient(TechType.FiberMesh, 3),
                                    new Ingredient(TechType.Silver, 1),
                                    new Ingredient(TechType.Titanium, 1)
                                }
                            }
                        }
                    };

                    PDAHandler.EditFragmentsToScan(TechType.Beacon, 3);
                    PDAHandler.EditFragmentsToScan(TechType.Gravsphere, 3);
                    PDAHandler.EditFragmentsToScan(TechType.StasisRifle, 3);
                    PDAHandler.EditFragmentsToScan(TechType.PropulsionCannon, 3);
                    PDAHandler.EditFragmentsToScan(TechType.RepulsionCannon, 3);
                    PDAHandler.EditFragmentsToScan(TechType.LaserCutter, 4);
                    PDAHandler.EditFragmentsToScan(TechType.LaserCutterFragment, 4);
                    PDAHandler.EditFragmentsToScan(TechType.Welder, 4);
                    PDAHandler.EditFragmentsToScan(TechType.BatteryCharger, 4);
                    PDAHandler.EditFragmentsToScan(TechType.PowerCellCharger, 4);
                    PDAHandler.EditFragmentsToScan(TechType.Constructor, 6);
                    PDAHandler.EditFragmentsToScan(TechType.BaseBioReactor, 4);
                    PDAHandler.EditFragmentsToScan(TechType.BaseNuclearReactor, 4);
                    PDAHandler.EditFragmentsToScan(TechType.ThermalPlant, 4);
                    PDAHandler.EditFragmentsToScan(TechType.BaseMoonpool, 4);
                    PDAHandler.EditFragmentsToScan(TechType.PowerTransmitter, 2);
                    PDAHandler.EditFragmentsToScan(TechType.BaseMapRoom, 4);
                    //PDAHandler.EditFragmentsToScan(TechType.BaseWaterPark, 3);
                    //PDAHandler.EditFragmentsToScan(TechType.Spotlight, 2);
                }

                if (techChanges != null)
                {
                    foreach (KeyValuePair<TechType, TechData> tech in techChanges)
                    {
                        CraftDataHandler.SetTechData(tech.Key, tech.Value);
                    }
                }

                Console.WriteLine("[DeathRun] Patched");

            }
            catch (Exception ex)
            {
                CattleLogger.PatchFailed(ex);
                patchFailed = true;
            }

            statsData.LoadStats();

            Console.WriteLine("[DeathRun] Stats Loaded");
        }

        public static void setCause(string newCause)
        {
            cause = newCause;
        }

        public static void setCauseObject(GameObject newCause)
        {
            causeObject = newCause;
        }


        /**
         * This gets called when we detect a brand-new-game being started from the main menu.
         */
        public static void StartNewGame()
        {
            CattleLogger.Message("Start New Game -- clearing all mod-specific player data");

            saveData = new DeathRunSaveData();
            countdownMonitor = new global::Utils.ScalarMonitor(0f);
            playerMonitor = new global::Utils.ScalarMonitor(0f);

            configDirty = 0;

            murkinessDirty = false;

            craftingSemaphore = false;
            chargingSemaphore = false;
            filterSemaphore = false;
            scannerSemaphore = false;

            cause = CAUSE_UNKNOWN;
            causeObject = null;
            cinematicCause = CAUSE_UNKNOWN;
            cinematicCauseObject = null;

            playerIsDead = false;
        }
    }


    internal class WarnFailurePatcher
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            if (DeathRun.patchFailed)
            {
                ErrorMessage.AddMessage("PATCH FAILED - Death Run patch failed to complete. See errorlog (Logoutput.Log) for details.");
                DeathRunUtils.CenterMessage("PATCH FAILED", 10, 4);
            }

            DeathRunUtils.ShowHighScores(true);
        }
    }
}
