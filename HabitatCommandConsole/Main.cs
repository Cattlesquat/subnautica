namespace HabitatManager
{
    using System;
    using System.Reflection;
    using System.Linq;
    using Harmony;
    using SMLHelper;
    using SMLHelper.V2.Handlers;
    using UnityEngine;

    public class Main
    {
        // Just an output on whether this worked
        private static bool wasSuccessful = true;

        // This class exists only to try the patch. If it doesn't work, then RIP
        public static void Patch()
        {
            try
            {
                Logging.logMessage("[SeraphimHabitatManager] Patching started for HabitatManager");
                HabitatManager.Patch();
            }
            catch (Exception ex)
            {
                Logging.logMessage("[SeraphimHabitatManager] You have failed! Exception: " + ex.ToString());
                wasSuccessful = false;
            }
            Logging.logMessage("[SeraphimHabitatManager] " + (!wasSuccessful ? "was not successful!" : "was successful."));
        }
    }
}
