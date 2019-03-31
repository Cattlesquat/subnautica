namespace NitrogenMod.Patchers
{
    using Harmony;

    [HarmonyPatch(typeof(NitrogenLevel))]
    [HarmonyPatch("Start")]
    internal class NitroLevPatcher
    {
        [HarmonyPostfix]
        public static void Postfix(ref NitrogenLevel __instance)
        {
            __instance.nitrogenEnabled = true; // This should enable Nitrogen!
        }
    }
}
