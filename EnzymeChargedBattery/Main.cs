namespace EnzymeChargedBattery
{
    using System;
    using System.Reflection;
    using Harmony;
    using EnzymeChargedBattery.Fabricator;

    public static class Main
    {
        public static void Patch()
        {
            UnityEngine.Debug.Log("[EnzymeChargedBattery] Start patching. Version: 1.0.0.1");
            try
            {
                EnzymeBatteryCore.PatchIt();
                var harmony = HarmonyInstance.Create("com.enzymechargedbatteries.mod");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                UnityEngine.Debug.Log("Patching complete.");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log($"[EnzymeChargedBattery] ERROR: {e.ToString()}");
            }
        }
    }
}
