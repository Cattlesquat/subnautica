namespace BiomeHUDIndicator.GottaPatch
{
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using System.Text;
using Harmony;
using UnityEngine;
using Fabricator;

    // So I did a dumb and the compass's checks are all in DepthCompass. I just overlooked ALL OF THEM!
    [HarmonyPatch(typeof(uGUI_DepthCompass))]
    [HarmonyPatch("IsCompassEnabled")]
    internal class uGUI_DepthCompassPatcher
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
        * So this is what we wanna replace.
        */
        [HarmonyPrefix] // We're attempting to cancel the entire method
        public static bool Prefix(ref uGUI_DepthCompass __instance)
        {
            if (__instance._initialized)
            {
                return false;
            }
            if (!uGUI.isMainLevel)
            {
                return false;
            }
            if (LaunchRocket.isLaunching)
            {
                return false;
            }
            if (uGUI.isIntro)
            {
                return false;
            }
            Player main = Player.main;
            if (main == null)
            {
                return false;
            }
            PDA pda = main.GetPDA();
            if (pda != null && pda.isInUse)
            {
                return false;
            }
            Player.Mode mode = main.GetMode();
            if (mode == Player.Mode.Piloting)
            {
                return false;
            }
            Inventory main2 = Inventory.main;
            if (main2 != null && main2.equipment != null && (main2.equipment.GetCount(TechType.Compass) > 0 || main2.equipment.GetCount(CompassCore.BiomeChipID) > 0))
            {
                return true;
            }
            uGUI_CameraDrone main3 = uGUI_CameraDrone.main;
            return main3 != null && main3.GetCamera() != null;
        }
    }
}
