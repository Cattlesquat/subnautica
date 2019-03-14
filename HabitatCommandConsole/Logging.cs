namespace HabitatManager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;

    class Logging
    {
        // Shortens the method name for writing a log file
        public static void logMessage(string msg)
        {
            UnityEngine.Debug.Log(string.Format(msg));
        }
    }
}
