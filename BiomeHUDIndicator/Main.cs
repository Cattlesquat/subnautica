namespace BiomeHUDIndicator
{
    using System;
    using System.Reflection;
    using Harmony;
    using BiomeHUDIndicator.Fabricator;

    public static class Main
    {
        public static void Patch()
        {
            UnityEngine.Debug.Log("[BiomeHUDIndicator] Start patching. Version: 1.0.0.0");
            try
            {
                CompassCore.PatchIt();
                var harmony = HarmonyInstance.Create("com.biomehudindicator.mod");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                UnityEngine.Debug.Log("[BiomeHUDIndicator] Patching complete.");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log($"[BiomeHUDIndicator] ERROR: {e.ToString()}");
            }
        }
    }
}
