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
            if (compassID > 0 || biomeChip > 0)
            {
                return true;
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(uGUI_DepthCompass))]
    [HarmonyPatch("Start")]
    internal class UGUIPatcher
    {
        [HarmonyPostfix]
        public static void Postfix(ref uGUI_DepthCompass __instance)
        {
            __instance.gameObject.AddComponent<BiomeDisplay>();
            Transform currentTransform = __instance.transform;
            Transform getParent = currentTransform.parent;
            while (getParent != null)
            {
                SeraLogger.Message(Main.modName, "CurrentTransform: " + currentTransform.name + " Parent: " + getParent.name);
                currentTransform = getParent;
                getParent = currentTransform.parent;
            }
        }
    }
}