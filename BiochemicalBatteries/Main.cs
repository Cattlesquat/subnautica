/// <summary>
/// This mod serves as an example on how to use PrimeSonic's CustomBatteries mod in combination with other patches to create a more complex project.
/// It also gives an example of how to add a custom item and harvest type to source for that mod.
/// Happy Warper hunting.
/// https://github.com/PrimeSonic/PrimeSonicSubnauticaMods/blob/CustomBatteries/CustomBatteries/
/// </summary>

namespace BiochemicalBatteries
{
    using System;
    using System.Reflection;
    using Harmony;
    using Items;
    using Common;
    using CustomBatteries.API;

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

                // First, you instantiate PrimeSonic's service class
                var cbservice = new CustomBatteriesService();
                // Create a new instance for your custom pack
                var bcpack = new Items.BiochemicalPack();
                // Use CustomBatteries' API to add it to the game
                cbservice.AddPluginPackFromMod(bcpack);


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
