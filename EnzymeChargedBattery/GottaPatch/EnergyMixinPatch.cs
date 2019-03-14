namespace EnzymeChargedBattery.GottaPatch
{
    using System.Collections.Generic;
    using Fabricator;
    using Harmony;

    // Whoops, we could put the batteries and power cells into chargers
    // But nothing else. Did some homework, let's fix it
    // Also had to add in the models for things like Seamoths
    // Patching EnergyMixin, once again patching "Start"
    [HarmonyPatch(typeof(EnergyMixin))]
    [HarmonyPatch("Start")]
    internal class EnergyMixinPatch
    {
        [HarmonyPostfix] // Postfixes happen immediately after the code of whatever we're choosing
        public static void Postfix(ref EnergyMixin __instance)
        {
            // Tools and vehicles need to accept the batteries
            if (!__instance.allowBatteryReplacement)
                return; // No need to patch anything that doesn't let you change the batteries

            List<TechType> compatibleBatteries = __instance.compatibleBatteries;
            if(compatibleBatteries.Contains(TechType.PrecursorIonBattery) && !compatibleBatteries.Contains(EnzymeBatteryCore.BattID))
            {
                // This will only be usable in stuff that takes Precursor Ion Batteries
                compatibleBatteries.Add(EnzymeBatteryCore.BattID);
                return;
            }

            if(compatibleBatteries.Contains(TechType.PrecursorIonPowerCell) && !compatibleBatteries.Contains(EnzymeBatteryCore.PowCelID))
            {
                // If the Precursor Ion Power Cell fits, this sits
                compatibleBatteries.Add(EnzymeBatteryCore.PowCelID);
                return;
            }
        }
    }

    // Patching the EnergyMixin that makes sure a battery is present
    [HarmonyPatch(typeof(EnergyMixin))]
    [HarmonyPatch("NotifyHasBattery")]
    internal class EnergyMixin_NotifyHasBattery_Patch
    {
        [HarmonyPostfix] // Again, done after immediate code
        public static void Postfix(ref EnergyMixin __instance, InventoryItem item)
        {
            // This should allow things like subs to show inserted cells
            // But also let's check for a null just in case
            if (item?.item?.GetTechType() == EnzymeBatteryCore.PowCelID)
                __instance.batteryModels[0].model.SetActive(true);
        }
    }
}
