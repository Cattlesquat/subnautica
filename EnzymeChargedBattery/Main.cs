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
    using Items;
    using Common;

    public static class Main
    {
        public const string modName = "[EnzymeChargedBattery]";

        public static void Patch()
        {
            SeraLogger.PatchStart(modName, "1.0.2");
            try
            {
                EnzymeBatteryCore.PatchBatteries();
                var harmony = HarmonyInstance.Create("seraphimrisen.enzymechargedbatteries.mod");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                SeraLogger.PatchComplete(modName);
            }
            catch (Exception ex)
            {
                SeraLogger.PatchFailed(modName, ex);
            }
        }
    }
}
