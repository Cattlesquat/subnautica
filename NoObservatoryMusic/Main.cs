namespace NoObservatoryMusic
{
    using System;
    using System.Reflection;
    using Harmony;

    // If all goes as planned this should basically instantly nullify the observatory player biome
    public static class Main
    {
        public static void Patch()
        {
            UnityEngine.Debug.Log("[NoObservatoryMusic] Start patching. Version: 1.0.0.0");
            try
            {
                var harmony = HarmonyInstance.Create("seraphimrisen.noobservatorymusic.mod");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                UnityEngine.Debug.Log("[NoObservatoryMusic] Patching complete.");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log($"[NoObservatoryMusic] ERROR: {e.ToString()}");
            }
        }
    }
}
