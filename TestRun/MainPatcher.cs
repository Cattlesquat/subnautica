using QModManager.API.ModLoading;
using HarmonyLib;

namespace DeathRun   
{
    [QModCore]
    public static class MainPatcher        

    {
        [QModPatch]
        public static void Patch()
        {
            Harmony harmony = new Harmony("com.cattlesquat.subnautica.deathrun.mod"); 
            harmony.PatchAll();
        }
    }
}
