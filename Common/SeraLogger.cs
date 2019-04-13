namespace Common
{
    using System;

    public class SeraLogger
    {
        public static void ConfigNotFound(string modName)
        {
            UnityEngine.Debug.Log(modName + " Config file not found. Creating default value.");
        }

        public static void ConfigReadSuccess(string modName)
        {
            UnityEngine.Debug.Log(modName + " Config file found and read successfully");
        }

        public static void ConfigReadError(string modName, Exception ex)
        {
            UnityEngine.Debug.Log(modName + " Error reading file. Setting defaults. Exception: " + ex.ToString());
        }

        public static void PatchStart(string modName, string version)
        {
            UnityEngine.Debug.Log(modName + " Start patching. Version: " + version);
        }

        public static void PatchComplete(string modName)
        {
            UnityEngine.Debug.Log(modName + " Patching complete.");
        }

        public static void PatchFailed(string modName, Exception ex)
        {
            UnityEngine.Debug.Log(modName + " Patching failed. Exception: " + ex.ToString());
        }

        public static void SeralizerFailed(string file, Exception ex)
        {
            UnityEngine.Debug.Log("File I/O Error: " + file + " Exception: " + ex.ToString());
        }
    }
}
