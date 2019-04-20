namespace BrineDamageFix
{
    using System;
    using Common;
    using System.Reflection;
    using Harmony;

    // For now we'll bludgeon this method to death
    public class Main
    {
        public static void Patch()
        {
            string modName = "[BrineDamageFix]";
            SeraLogger.PatchStart(modName, "1.0.0");
            try
            {
                var harmony = HarmonyInstance.Create("seraphimrisen.brinedamagefix.mod");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                SeraLogger.PatchComplete(modName);
            }
            catch (Exception ex)
            {
                SeraLogger.PatchFailed(modName, ex);
            }
        }
    }

    [HarmonyPatch(typeof(AcidicBrineDamage))]
    [HarmonyPatch("ApplyDamage")]
    internal class AcidicBrineDamageBrutePatch
    {
        [HarmonyPrefix]
        public static bool Prefix()
        {
            if (Player.main.motorMode != Player.MotorMode.Dive)
                return false;
            else
                return true;
        }
    }
}





