namespace EnzymeChargedBattery.GottaPatch
{
    using System.Collections.Generic;
    using System.Reflection;
    using Harmony;
    using Fabricator;

    // We're gonna patch the BatteryCharger and the Start method
    [HarmonyPatch(typeof(BatteryCharger))]
    [HarmonyPatch("Start")]
    internal class BatteryChargerPatch
    {
        [HarmonyPrefix] // Make sure it runs before the chosen method
        public static void Prefix(ref BatteryCharger __instance)
        {
            // Have to collect info from a private field
            FieldInfo compatTechField = typeof(BatteryCharger).GetField("compatibleTech", BindingFlags.Static | BindingFlags.NonPublic);
            var compatTech = (HashSet<TechType>)compatTechField.GetValue(null);
            // Check to see if Enzyme Batteries are in the charger, if not, add them
            if (!compatTech.Contains(EnzymeBatteryCore.BattID))
                compatTech.Add(EnzymeBatteryCore.BattID);
        }
    }

    // Going to patch the PowerCellCharger and its start method
    [HarmonyPatch(typeof(PowerCellCharger))]
    [HarmonyPatch("Start")]
    internal class PowerCellChargerPatch
    {
        [HarmonyPrefix] // As above, run before chosen method
        public static void Prefix(ref PowerCellCharger __instance)
        {
            // Grab that private HashSet
            FieldInfo compatTechField = typeof(PowerCellCharger).GetField("compatibleTech", BindingFlags.Static | BindingFlags.NonPublic);
            var compatTech = (HashSet<TechType>)compatTechField.GetValue(null);
            // If Enzyme Power Cells aren't in the list, add them
            if (!compatTech.Contains(EnzymeBatteryCore.PowCelID))
                compatTech.Add(EnzymeBatteryCore.PowCelID);
        }
    }
}
