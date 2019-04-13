namespace EnzymeChargedBattery.Patchers
{
    using System.Collections.Generic;
    using System.Reflection;
    using Harmony;
    using Items;

    [HarmonyPatch(typeof(BatteryCharger))]
    [HarmonyPatch("Start")]
    internal class BatteryChargerPatch
    {
        [HarmonyPrefix]
        public static void Prefix(ref BatteryCharger __instance)
        {
            FieldInfo compatTechField = typeof(BatteryCharger).GetField("compatibleTech", BindingFlags.Static | BindingFlags.NonPublic);
            var compatTech = (HashSet<TechType>)compatTechField.GetValue(null);
            if (!compatTech.Contains(EnzymeBatteryCore.BattID))
                compatTech.Add(EnzymeBatteryCore.BattID);
        }
    }

    [HarmonyPatch(typeof(PowerCellCharger))]
    [HarmonyPatch("Start")]
    internal class PowerCellChargerPatch
    {
        [HarmonyPrefix]
        public static void Prefix(ref PowerCellCharger __instance)
        {
            FieldInfo compatTechField = typeof(PowerCellCharger).GetField("compatibleTech", BindingFlags.Static | BindingFlags.NonPublic);
            var compatTech = (HashSet<TechType>)compatTechField.GetValue(null);
            if (!compatTech.Contains(EnzymeBatteryCore.PowCelID))
                compatTech.Add(EnzymeBatteryCore.PowCelID);
        }
    }
}
