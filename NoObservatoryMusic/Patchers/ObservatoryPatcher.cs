namespace NoObservatoryMusic.Patchers
{
    using Harmony;

    // This should patch the Observatory for us. Let's do it.
    [HarmonyPatch(typeof(ObservatoryAmbientSound))]
    [HarmonyPatch("IsPlayerInObservatory")]
    internal class InObservatoryPatcher
    {
        public static bool disabled = true;

        [HarmonyPrefix] // We're attempting to cancel the entire method
        public static bool Prefix()
        {
            return !disabled;
        }
    }

    [HarmonyPatch(typeof(ObservatoryAmbientSound))]
    [HarmonyPatch("OnTriggerEnter")]
    internal class OnEnterPatcher
    {
        public static bool disabled = true;

        [HarmonyPrefix] // We're attempting to cancel the entire method
        public static bool Prefix()
        {
            return !disabled;
        }
    }

    [HarmonyPatch(typeof(ObservatoryAmbientSound))]
    [HarmonyPatch("OnTriggerExit")]
    internal class OnExitPatcher
    {
        public static bool disabled = true;

        [HarmonyPrefix] // We're attempting to cancel the entire method
        public static bool Prefix()
        {
            return !disabled;
        }
    }

}
