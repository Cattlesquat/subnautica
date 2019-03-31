namespace BiomeHUDIndicator.GottaPatch
{
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using System.Text;
using Harmony;
using SMLHelper.V2.Handlers;
using UnityEngine;
using Fabricator;

    /*[HarmonyPatch(typeof(uGUI_DepthCompass))]
    [HarmonyPatch("UpdateCompass()")]
    internal class DepthCompass_UpdateCompassPatcher
    {
        public static void Postfix (ref uGUI_DepthCompass __instance)
        {
            Player main = Player.main;
            Inventory main2 = Inventory.main;
            if (BiomeChipCheck(main2))
            {
                Main.biomeHUD.SetActive(true);
            }
            else
            {
                Main.biomeHUD.SetActive(false);
            }
        }

        private static bool BiomeChipCheck(Inventory inv)
        {
            bool chip = false;
            int biomeChip = 0;
            biomeChip = inv.equipment.GetCount(CompassCore.BiomeChipID);
            if (biomeChip > 0)
            {
                chip = true;
            }
            return chip;
        }
    }*/ // Gonna just get the biome display working before I actually try to implement this

    // So I did a dumb and the compass's checks are all in DepthCompass. I just overlooked ALL OF THEM!
    [HarmonyPatch(typeof(uGUI_DepthCompass))]
    [HarmonyPatch("IsCompassEnabled")]
    internal class DepthCompass_IsCompassEnabledPatcher
    {
        public static uGUI_BiomeIndicator biomeIndi;
        [HarmonyPrefix] // We're replacing the entire method
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
                int biomeChip = 0;
                biomeChip = main2.equipment.GetCount(CompassCore.BiomeChipID);
                __result = true;
                return false;
            }
            uGUI_CameraDrone main3 = uGUI_CameraDrone.main;
            __result = main3 != null && main3.GetCamera() != null;
            return false;
        }

        // This checks and returns whether or not the compass and/or biome chip are present.
        private static bool TechTypeCheck(Inventory inv)
        {
            int compassID = 0;
            int biomeChip = 0;
            bool chips = false;
            compassID = inv.equipment.GetCount(TechType.Compass);
            biomeChip = inv.equipment.GetCount(CompassCore.BiomeChipID);
            if(compassID > 0 || biomeChip > 0)
            {
                chips = true;
                return chips;
            }
            return chips;
        }
    }
}