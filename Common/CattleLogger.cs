/**
 * From Seraphim's SeraLogger, but with a single registered modname so I don't have to keep passing it.
 */
namespace Common
{
    using System;    

    public class CattleLogger
    {
        static string modName = "Unregistered";

        public static void setModName(string name)
        {
            modName = name;
        }

        /*
         * General messages or generic errors
         */
        public static void Message(string modName, string message)
        {
            Console.WriteLine(modName + " " + message);
        }

        public static void Message(string message)
        {
            Message(modName, message);
        }


        public static void GenericError(string modName, Exception ex)
        {
            Console.WriteLine(modName + " ERROR: " + ex);
        }

        public static void GenericError(Exception ex)
        {
            GenericError(modName, ex);
        }


        /*
         * Patch() errors
         */
        public static void PatchStart(string modName, string version)
        {
            Console.WriteLine(modName + " Start patching. Version: " + version);
        }

        public static void PatchStart(string version)
        {
            PatchStart(modName, version);
        }


        public static void PatchComplete(string modName)
        {
            Console.WriteLine(modName + " Patching complete.");
        }

        public static void PatchComplete()
        {
            PatchComplete(modName);
        }


        public static void PatchFailed(string modName, Exception ex)
        {
            Console.WriteLine(modName + " Patching failed. Exception: " + ex);
        }

        public static void PatchFailed (Exception ex)
        {
            PatchFailed(modName, ex);
        }

    }
}
