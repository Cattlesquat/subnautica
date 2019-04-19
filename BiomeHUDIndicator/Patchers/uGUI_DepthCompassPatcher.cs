namespace BiomeHUDIndicator.Patchers
{
    using System.Collections.Generic;
    using Harmony;
    using UnityEngine;
    using Items;
    using Common;

    [HarmonyPatch(typeof(uGUI_DepthCompass))]
    [HarmonyPatch("IsCompassEnabled")]
    internal class DepthCompass_IsCompassEnabledPatcher
    {
        private static string _cachedBiome = "unassigned";
        private static string _cachedBiomeFriendly = "Unassigned";

        private static float updateTimer = 0f;

        private static bool addedComponent = false;

        [HarmonyPrefix]
        public static bool Prefix(ref uGUI_DepthCompass __instance, ref bool __result)
        {
            if (__instance == null)
            {
                __result = false;
                return false;
            }
            if (!__instance._initialized)
            {
                __result = false;
                return false;
            }
            if (!uGUI.isMainLevel)
            {
                __result = false;
                return false;
            }
            if (LaunchRocket.isLaunching)
            {
                __result = false;
                return false;
            }
            if (uGUI.isIntro)
            {
                __result = false;
                return false;
            }
            Player main = Player.main;
            if (main == null)
            {
                __result = false;
                return false;
            }
            PDA pda = main.GetPDA();
            if (pda != null && pda.isInUse)
            {
                __result = false;
                return false;
            }
            Player.Mode mode = main.GetMode();
            if (mode == Player.Mode.Piloting)
            {
                __result = false;
                return false;
            }
            Inventory main2 = Inventory.main;
            if (main2 != null && main2.equipment != null && TechTypeCheck(main2))
            {
                if (Time.time >= updateTimer + 7.5f || updateTimer == 0f)
                {
                    BiomeCheck();
                    updateTimer = Time.time;
                }
                __result = true;
                return false;
            }
            uGUI_CameraDrone main3 = uGUI_CameraDrone.main;
            __result = main3 != null && main3.GetCamera() != null;
            return false;
        }

        private static bool TechTypeCheck(Inventory inv)
        {
            int compassID = inv.equipment.GetCount(TechType.Compass);
            int biomeChip = inv.equipment.GetCount(CompassCore.BiomeChipID);
            if(compassID > 0 || biomeChip > 0)
            {
                return true;
            }
            return false;
        }

        private static void BiomeCheck()
        {
            if (!addedComponent)
            {
                uGUI.main.gameObject.AddComponent<BiomeDisplay>();
                addedComponent = true;
                SeraLogger.Message(Main.modName, "Added BiomeDisplay component");
            }
            string curBiome = Player.main.GetBiomeString().ToLower();
            int biomeChip = Inventory.main.equipment.GetCount(CompassCore.BiomeChipID);
            if (biomeChip > 0 && curBiome != null)
            {
                int index = curBiome.IndexOf('_');
                if (index > 0)
                {
                    curBiome = curBiome.Substring(0, index);
                }
                if (curBiome != _cachedBiome)
                {
                    _cachedBiome = curBiome;
                    foreach (var biome in biomeList)
                    {
                        if (curBiome.Contains(biome.Key))
                        {
                            _cachedBiomeFriendly = biome.Value;
                            ErrorMessage.AddMessage("ENTERING: " + _cachedBiomeFriendly);
                            BiomeDisplay.DisplayBiome(_cachedBiomeFriendly);
                        }
                    }
                }
            }
        }

        private static readonly Dictionary<string, string> biomeList = new Dictionary<string, string>()
        {
            { "safe", "Safe Shallows" },
            { "kelpforest", "Kelp Forest" },
            { "grassy", "Grassy Plateaus" },
            { "mushroomforest", "Mushroom Forest" },
            { "jellyshroomcaves", "Jellyshroom Caves" },
            { "sparse", "Sparse Reef" },
            { "underwaterislands" , "Underwater Islands" },
            { "bloodkelp" , "Blood Kelp Zone" },
            { "dunes" , "Sand Dunes" },
            { "crashzone" , "Crash Zone" },
            { "grandreef" , "Grand Reef" },
            { "mountains" , "Mountains" },
            { "lostriver" , "Lost River" },
            { "ilz" , "Inactive Lava Zone" },
            { "lava" , "Lava Lake" },
            { "floatingisland" , "Floating Island" },
            { "koosh" , "Bulb Zone" },
            { "seatreader" , "Sea Treader's Path" },
            { "crag" , "Crag Field" },
            { "unassigned" , "Unassigned" },
            { "void" , "Ecological Dead Zone" },
            // Special-case biomes, may remove
            { "precursor" , "Precursor Facility" },
            { "prison" , "Primary Containment Facility" },
            { "shipspecial" , "Aurora" },
            { "shipinterior", "Aurora" },
            { "crashhome" , "Aurora" },
            { "aurora" , "Aurora" },
            { "mesas" , "Mesas" },
            { "observatory" , "Observatory" },
            { "generatorroom" , "Generator Room" },
            { "crashedship" , "Aurora" },
        };
    }
}