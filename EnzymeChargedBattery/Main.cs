/// <summary>
/// This class and project were only made possible with the help and guidance of PrimeSonic and his midgamebatteries mod.
/// Without that mod and its source code, this mod would not have been completed or working.
/// midgamebatteries repo: https://github.com/PrimeSonic/PrimeSonicSubnauticaMods/tree/master/MidGameBatteries
/// </summary>

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
            UnityEngine.Debug.Log("[EnzymeChargedBattery] Start patching. Version: 1.0.1");
            try
            {
                EnzymeBatteryCore.PatchIt();
                var harmony = HarmonyInstance.Create("seraphimrisen.enzymechargedbatteries.mod");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                UnityEngine.Debug.Log("[EnzymeChargedBattery] Patching complete.");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log($"[EnzymeChargedBattery] ERROR: {e.ToString()}");
            }
        }
    }
}
