namespace EnzymeChargedBattery.Patchers
{
    using System.Collections.Generic;
    using System.Reflection;
    using Harmony;
    using Items;

    [HarmonyPatch(typeof(BatteryCharger))]
    [HarmonyPatch("Initialize")]
    internal class BatteryChargerPatch
    {
        [HarmonyPrefix]
        public static void Prefix(ref BatteryCharger __instance)
        {
            FieldInfo compatTechField = typeof(BatteryCharger).GetField("compatibleTech", BindingFlags.Static | BindingFlags.NonPublic);
            var compatTech = (HashSet<TechType>)compatTechField.GetValue(null);
            if (!compatTech.Contains(SeraphimBatteryCore.BattID))
            {
                compatTech.Add(EnzymeBattery.BattID);
                compatTech.Add(KharaaBattery.BattID);
            }
                
        }
    }

    [HarmonyPatch(typeof(PowerCellCharger))]
    [HarmonyPatch("Initialize")]
    internal class PowerCellChargerPatch
    {
        [HarmonyPrefix]
        public static void Prefix(ref PowerCellCharger __instance)
        {
            FieldInfo compatTechField = typeof(PowerCellCharger).GetField("compatibleTech", BindingFlags.Static | BindingFlags.NonPublic);
            var compatTech = (HashSet<TechType>)compatTechField.GetValue(null);
            if (!compatTech.Contains(SeraphimBatteryCore.PowCelID))
            {
                compatTech.Add(EnzymePowerCell.PowCelID);
                compatTech.Add(KharaaPowerCell.PowCelID);
            }
        }
    }
}
