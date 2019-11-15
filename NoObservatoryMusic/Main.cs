namespace NoObservatoryMusic
{
    using System;
    using System.Reflection;
    using Harmony;
    using SMLHelper.V2.Handlers;
    using Common;

    public static class Main
    {
        public const string modName = "[NoObservatoryMusic]";

        public static void Patch()
        {
            SeraLogger.PatchStart(modName, "1.0.3");
            try
            {
                var harmony = HarmonyInstance.Create("seraphimrisen.noobservatorymusic.mod");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                OptionsPanelHandler.RegisterModOptions(new ObservatoryOptions());
                SeraLogger.PatchComplete(modName);
            }
            catch (Exception ex)
            {
                SeraLogger.PatchFailed(modName, ex);
            }
        }
    }
}
