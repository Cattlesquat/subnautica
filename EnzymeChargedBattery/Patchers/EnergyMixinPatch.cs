namespace EnzymeChargedBattery.Patchers
{
    using System.Collections.Generic;
    using Items;
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
            if(compatibleBatteries.Contains(TechType.PrecursorIonBattery) && !compatibleBatteries.Contains(SeraphimBatteryCore.BattID))
            {
                compatibleBatteries.Add(SeraphimBatteryCore.BattID);
                return;
            }

            if(compatibleBatteries.Contains(TechType.PrecursorIonPowerCell) && !compatibleBatteries.Contains(SeraphimBatteryCore.PowCelID))
            {
                compatibleBatteries.Add(SeraphimBatteryCore.PowCelID);
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
            if (item?.item?.GetTechType() == SeraphimBatteryCore.PowCelID)
                __instance.batteryModels[0].model.SetActive(true);
        }
    }
}
