namespace NitrogenMod.Patchers
{
    using Harmony;
    using UnityEngine;
    using UnityEngine.UI;

    [HarmonyPatch(typeof(NitrogenLevel))]
    [HarmonyPatch("Start")]
    internal class NitroLevPatcher
    {
        [HarmonyPostfix]
        public static void Postfix (ref NitrogenLevel __instance)
        {
            __instance.nitrogenEnabled = NitrogenOptions.enabled;
            UnityEngine.Debug.Log("[NitrogenMod] Postfix method ran correctly. nitogrenEnabled is " + __instance.nitrogenEnabled);
        }
    }
}