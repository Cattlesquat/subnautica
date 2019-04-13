namespace EnzymeChargedBattery.Patchers
{
    using System.Collections.Generic;
    using Fabricator;
    using Harmony;

    [HarmonyPatch(typeof(EnergyMixin))]
    [HarmonyPatch("Start")]
    internal class EnergyMixinPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ref EnergyMixin __instance)
        {
            if (!__instance.allowBatteryReplacement)
                return;

            List<TechType> compatibleBatteries = __instance.compatibleBatteries;
            if(compatibleBatteries.Contains(TechType.PrecursorIonBattery) && !compatibleBatteries.Contains(EnzymeBatteryCore.BattID))
            {
                compatibleBatteries.Add(EnzymeBatteryCore.BattID);
                return;
            }

            if(compatibleBatteries.Contains(TechType.PrecursorIonPowerCell) && !compatibleBatteries.Contains(EnzymeBatteryCore.PowCelID))
            {
                compatibleBatteries.Add(EnzymeBatteryCore.PowCelID);
                return;
            }
        }
    }

    [HarmonyPatch(typeof(EnergyMixin))]
    [HarmonyPatch("NotifyHasBattery")]
    internal class EnergyMixin_NotifyHasBattery_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(ref EnergyMixin __instance, InventoryItem item)
        {
            if (item?.item?.GetTechType() == EnzymeBatteryCore.PowCelID)
                __instance.batteryModels[0].model.SetActive(true);
        }
    }
}
