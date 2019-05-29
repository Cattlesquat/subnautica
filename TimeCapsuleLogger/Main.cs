namespace TimeCapsuleLogger
{
    using System;
    using Common;
    using System.Reflection;
    using Harmony;

    // Just for the purpose of logging time capsule code
    public class Main
    {
        public static void Patch()
        {
            string modName = "[TimeCapsuleLogger]";
            SeraLogger.PatchStart(modName, "1.0.2");
            try
            {
                var harmony = HarmonyInstance.Create("seraphimrisen.timecapsulelogger.mod");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                SeraLogger.PatchComplete(modName);
            }
            catch (Exception ex)
            {
                SeraLogger.PatchFailed(modName, ex);
            }
        }
    }

    [HarmonyPatch(typeof(TimeCapsule))]
    [HarmonyPatch("Spawn")]
    internal class SpawnLogger
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            ErrorMessage.AddMessage("Time Capsule detected!");
        }
    }
}





