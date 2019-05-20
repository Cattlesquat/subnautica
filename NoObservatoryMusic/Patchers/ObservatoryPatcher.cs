namespace NoObservatoryMusic.Patchers
{
    using Harmony;

    [HarmonyPatch(typeof(ObservatoryAmbientSound))]
    [HarmonyPatch("IsPlayerInObservatory")]
    internal class InObservatoryPatcher
    {
        public static bool disabled = true;

        [HarmonyPrefix]
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

        [HarmonyPrefix]
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

        [HarmonyPrefix]
        public static bool Prefix()
        {
            return !disabled;
        }
    }

}
