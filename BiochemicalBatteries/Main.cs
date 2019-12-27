/// <summary>
/// This mod serves as an example on how to use PrimeSonic's CustomBatteries mod in combination with other patches to create a more complex project.
/// 
/// </summary>

namespace BiochemicalBatteries
{
    using System;
    using System.Reflection;
    using Harmony;
    using Items;
    using Common;

    public static class Main
    {
        public const string modName = "[BiochemicalBattery]";

        public static void Patch()
        {
            SeraLogger.PatchStart(modName, "1.0.0");
            try
            {
                var harmony = HarmonyInstance.Create("seraphimrisen.biochemicalbattery.mod");

                BioPlasmaItems.PatchBioPlasmaItems();
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
