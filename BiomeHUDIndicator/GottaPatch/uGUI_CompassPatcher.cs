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

    // So I did a dumb and the compass's checks are all in DepthCompass. I just overlooked ALL OF THEM!
    [HarmonyPatch(typeof(uGUI_DepthCompass))]
    [HarmonyPatch("IsCompassEnabled")]
    internal class DepthCompass_IsCompassEnabledPatcher
    {
        /*
        * IL_0075: ldsfld class Inventory Inventory::main
        * IL_007A: stloc.3
		* IL_007B: ldloc.3
		* IL_007C: ldnull
        * IL_007D: call bool[UnityEngine] UnityEngine.Object::op_Inequality(class [UnityEngine] UnityEngine.Object, class [UnityEngine] UnityEngine.Object)
		* IL_0082: brfalse IL_00AA
        * IL_0087: ldloc.3
		* IL_0088: callvirt instance class Equipment Inventory::get_equipment()
        * IL_008D: brfalse IL_00AA
        * IL_0092: ldloc.3
		* IL_0093: callvirt instance class Equipment Inventory::get_equipment()
        * IL_0098: ldc.i4    512
		* IL_009D: callvirt instance int32 Equipment::GetCount(valuetype TechType)
        * IL_00A2: ldc.i4.0
		* IL_00A3: ble IL_00AA
        * IL_00A8: ldc.i4.1
		* IL_00A9: ret
        * So this is what we wanna replace. Eventually.
        */
        [HarmonyPrefix] // We're attempting to replace the entire method
        public static bool Prefix(ref uGUI_DepthCompass __instance, ref bool __result)
        {
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
                __result = true;
                return false;
            }
            uGUI_CameraDrone main3 = uGUI_CameraDrone.main;
            __result = main3 != null && main3.GetCamera() != null;
            return false;
        }

        private static bool TechTypeCheck(Inventory inv)
        {
            // Let's try this instead
            int compassID = 0;
            int biomeChip = 0;
            bool chips = false;
            compassID = inv.equipment.GetCount(TechType.Compass);
            biomeChip = inv.equipment.GetCount(CompassCore.BiomeChipID);
            // UnityEngine.Debug.Log("[BiomeHUDIndicator] compassID is " + compassID + " and biomeCHIP is " + biomeChip);
            if(compassID > 0 || biomeChip > 0)
            {
                chips = true;
                return chips;
            }
            // UnityEngine.Debug.Log("[BiomeHUDIndicator] chips is " + chips);
            return chips;
        }
    }

    [HarmonyPatch(typeof(uGUI_DepthCompass))]
    [HarmonyPatch("UpdateDepth")]
    internal class DepthCompass_UpdateDepth
    {
        [HarmonyPostfix] // Gonna run this after existing UpdateDepth
        public static void Postfix (ref uGUI_DepthCompass __instance)
        {
            if (BiomeChipCheck(Inventory.main))
            {
                
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
    }

    // Logging this is no longer necessary.
    /* [HarmonyPatch(typeof(uGUI_DepthCompass))]
    * [HarmonyPatch("UpdateCompass")]
    * internal class DepthCompass_UpdateCompassLogger
    * {
    *     [HarmonyPostfix] // Just need to log this
    *     public static void Postfix (ref uGUI_DepthCompass __instance)
    *     {
    *         UnityEngine.Debug.Log("[BiomeHUDIndicator] result of flag is " + __instance.IsCompassEnabled());
    *     }
    * }
    */
}