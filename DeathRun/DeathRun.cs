/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 * * Much of the Nitrogen & Bends code from Seraphim Risen's NitrogenMod (just rebalanced and w/ more UI feedback)
 * * Radiation Mod material from libraryaddict, used with permission. 
 * * Escape Pod unleashed material from oldark, w/ fixes provided to make pod gravity work reliably.
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

        // DeathRun's saved games are handled in DeathRunUtils
        public static DeathRunSaveData saveData = new DeathRunSaveData();
        public static DeathRunSaveListener saveListener; 

        private const string modFolder = "./QMods/DeathRun/";
        private const string assetFolder = modFolder + "Assets/";
        private const string assetBundle = assetFolder + "n2warning";

        public static GameObject N2HUD { get; set; }

        public static global::Utils.ScalarMonitor countdownMonitor { get; set; } = new global::Utils.ScalarMonitor(0f);
        public static global::Utils.ScalarMonitor playerMonitor { get; set; } = new global::Utils.ScalarMonitor(0f);

        //public static bool podGravity  = true;

        // These semaphore relate to "flavors" of energy consumption
        public static bool craftingSemaphore = false;
        public static bool chargingSemaphore = false;
        public static bool filterSemaphore   = false;
        public static bool scannerSemaphore  = false;

        public const string CAUSE_UNKNOWN = "Unknown";
        public const string CAUSE_UNKNOWN_CREATURE = "Unknown Creature";

        public const float FULL_AGGRESSION = 2400; // 40 minutes
        public const float MORE_AGGRESSION = 1200; // 20 minutes

        // Temporary storage for "cause of death"
        public static string cause = CAUSE_UNKNOWN;
        public static GameObject causeObject = null;

        internal static Config config { get; } = OptionsPanelHandler.Main.RegisterModOptions<Config>();

        public static void Patch()
        {
            SeraLogger.PatchStart(modName, "1.5.1");

            try
            {
                Harmony harmony = new Harmony("cattlesquat.deathrun.mod");

                AssetBundle ab = AssetBundle.LoadFromFile(assetBundle);
                N2HUD = ab.LoadAsset("NMHUD") as GameObject;

                harmony.PatchAll(Assembly.GetExecutingAssembly());

                DummySuitItems.PatchDummyItems();
                ReinforcedSuitsCore.PatchSuits();
                if (DeathRun.config.enableSpecialtyTanks)
                {
                    O2TanksCore.PatchTanks();
                }

                Console.WriteLine(typeof(NitroDamagePatcher).AssemblyQualifiedName);

                SeraLogger.Message(modName, "Explosion Depth");

                harmony.Patch(typeof(CrashedShipExploder).GetMethod("CreateExplosiveForce", BindingFlags.NonPublic | BindingFlags.Instance),
                     null, new HarmonyMethod(typeof(ExplosionPatcher).GetMethod("CreateExplosiveForce")));

                SeraLogger.Message(modName, "Surface Air Poisoning");
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

                SeraLogger.Message(modName, "Radiation Warning");
                //if (config.radiationWarning)
                //{
                    harmony.Patch(AccessTools.Method(typeof(uGUI_RadiationWarning), "IsRadiated"),
                        new HarmonyMethod(typeof(RadiationPatcher).GetMethod("IsRadiated")), null);

                    harmony.Patch(AccessTools.Method(typeof(uGUI_RadiationWarning), "Update"),
                        new HarmonyMethod(typeof(RadiationPatcher).GetMethod("Update")), null);

                    //harmony.Patch(AccessTools.Method(typeof(uGUI_DepthCompass), "UpdateDepth"),
                    //    new HarmonyMethod(typeof(PatchRadiation).GetMethod("UpdateDepth")), null);

                //}

                SeraLogger.Message(modName, "Radiation Depth");
                //if (config.radiativeDepth > 0)
                //{
                    harmony.Patch(AccessTools.Method(typeof(RadiatePlayerInRange), "Radiate"),
                        new HarmonyMethod(typeof(RadiationPatcher).GetMethod("Radiate")), null);

                    harmony.Patch(AccessTools.Method(typeof(DamagePlayerInRadius), "DoDamage"),
                        new HarmonyMethod(typeof(RadiationPatcher).GetMethod("DoDamage")), null);
                //}

                SeraLogger.Message(modName, "Power Consumption");

                //if (!Config.NORMAL.Equals(DeathRun.config.powerCosts)) { 
                harmony.Patch(AccessTools.Method(typeof(PowerSystem), "AddEnergy"),
                        new HarmonyMethod(typeof(PowerPatcher).GetMethod("AddEnergyBase")), null);

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



                //SeraLogger.Message(modName, "Disable Fabricator Food");
                //if (config.disableFabricatorFood)
                //{
                //    harmony.Patch(AccessTools.Method(typeof(CrafterLogic), "IsCraftRecipeFulfilled"),
                //        new HarmonyMethod(typeof(PatchItems).GetMethod("IsCraftRecipeFulfilled")), null);
                //}

                SeraLogger.Message(modName, "Food Pickup");

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


                SeraLogger.Message(modName, "Vehicle Costs");

                Dictionary<TechType, TechData> techChanges = null;

                if (Config.NO_VEHICLES.Equals(DeathRun.config.vehicleCosts))
                {                    
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
                        }

                    };
                }
                else if (Config.DEATH_VEHICLES.Equals(DeathRun.config.vehicleCosts))
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
                                    new Ingredient(TechType.TitaniumIngot, 1),
                                    new Ingredient(TechType.PowerCell, 1),
                                    new Ingredient(TechType.Glass, 2),
                                    new Ingredient(TechType.Lubricant, 1),
                                    new Ingredient(TechType.Lead, 1),
                                    new Ingredient(TechType.TreeMushroomPiece, 1),
                                    new Ingredient(TechType.KooshChunk, 1),
                                    new Ingredient(TechType.RedGreenTentacleSeed, 1),
                                    new Ingredient(TechType.EyesPlantSeed, 1)
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
                                    new Ingredient(TechType.Nickel, 3),
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
                                    new Ingredient(TechType.Aerogel, 1),
                                    new Ingredient(TechType.EnameledGlass, 1),
                                    new Ingredient(TechType.Diamond, 2),
                                    new Ingredient(TechType.Lead, 2),
                                    new Ingredient(TechType.Sulphur, 3),
                                    new Ingredient(TechType.UraniniteCrystal, 3),
                                    new Ingredient(TechType.AluminumOxide, 3) // aka Ruby
                                }
                            }
                        }
                    };
                }
                else if (Config.HARD_VEHICLES.Equals(DeathRun.config.vehicleCosts))
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
                                    new Ingredient(TechType.TitaniumIngot, 1),
                                    new Ingredient(TechType.PowerCell, 1),
                                    new Ingredient(TechType.Glass, 2),
                                    new Ingredient(TechType.Lubricant, 1),
                                    new Ingredient(TechType.Lead, 1),
                                    new Ingredient(TechType.TreeMushroomPiece, 1),
                                    new Ingredient(TechType.KooshChunk, 1)
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
                                    new Ingredient(TechType.Aerogel, 1),
                                    new Ingredient(TechType.EnameledGlass, 1),
                                    new Ingredient(TechType.Diamond, 2),
                                    new Ingredient(TechType.Lead, 2),
                                    new Ingredient(TechType.AluminumOxide, 2), //aka Ruby
                                    new Ingredient(TechType.Sulphur, 2)
                                }
                            }
                        }
                    };
                }

                if (techChanges != null)
                {
                    foreach (KeyValuePair<TechType,TechData> tech in techChanges)
                    {
                        CraftDataHandler.SetTechData(tech.Key, tech.Value);
                    }
                }

                SeraLogger.Message(modName, "Habitat Builder Costs");

                techChanges = null;
                if (Config.DEATHRUN.Equals(DeathRun.config.builderCosts))
                {
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
                                    new Ingredient(TechType.Magnetite, 2)
                                }
                            }
                        }
                    };
                } 
                else if (Config.HARD.Equals(DeathRun.config.builderCosts))
                {
                    techChanges = new Dictionary<TechType, TechData>
                    {
                        {
                            TechType.Builder,
                            new TechData
                            {
                                craftAmount = 1,
                                Ingredients = new List<Ingredient>
                                {
                                    new Ingredient(TechType.ComputerChip, 1),
                                    new Ingredient(TechType.WiringKit, 1),
                                    new Ingredient(TechType.Battery, 1),
                                    new Ingredient(TechType.Lithium, 2)
                                }
                            }
                        }
                    };
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
                SeraLogger.PatchFailed(modName, ex);
                ErrorMessage.AddMessage("DeathRun - Failed to patch. See log for details.");
            }
        }

        public static void setCause(string newCause)
        {
            cause = newCause;
        }

        public static void setCauseObject(GameObject newCause)
        {
            causeObject = newCause;
        }

    }
}
