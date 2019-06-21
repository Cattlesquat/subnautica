namespace CyclopsThermodynamics
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Linq;
    using Harmony;
    using UnityEngine;
    using Common;

    public class Main
    {
        public static void Patch()
        {
            string modName = "[CyclopsThermodynamics]";
            SeraLogger.PatchStart(modName, "1.0.0");
            try
            {
                var harmony = HarmonyInstance.Create("seraphimrisen.cyclopsthermodynamics.mod");
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
