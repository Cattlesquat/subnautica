namespace Common
{
    using System;

    public class SeraLogger
    {
        /*
         * General messages or generic errors
         */
        public static void Message(string modName, string message)
        {
            UnityEngine.Debug.Log(modName + " " + message);
        }

        public static void GenericError(string modName, Exception ex)
        {
            UnityEngine.Debug.Log(modName + " ERROR: " + ex.ToString());
        }

        /*
         * File I/O related errors
         */
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

        public static void SeralizerFailed(string file, Exception ex)
        {
            UnityEngine.Debug.Log("File I/O Error: " + file + " Exception: " + ex.ToString());
        }

        /*
         * Patch() errors
         */
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
    }
}
