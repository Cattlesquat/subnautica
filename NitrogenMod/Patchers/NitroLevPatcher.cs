namespace NitrogenMod.Patchers
{
    using Harmony;
    using UnityEngine;
    using UWE;
    using UnityEngine.UI;

    [HarmonyPatch(typeof(NitrogenLevel))]
    [HarmonyPatch("Start")]
    internal class NitroLevPatcher
    {
        [HarmonyPostfix]
        public static void Postfix (ref NitrogenLevel __instance)
        {
            NitrogenOptions isEnabled = new NitrogenOptions();
            __instance.nitrogenEnabled = isEnabled.enabled;
        }
    }

    [HarmonyPatch(typeof(NitrogenLevel))]
    [HarmonyPatch("Update")]
    internal class NitroDamagePatcher
    {
        [HarmonyPrefix]
        public static bool Prefix (ref NitrogenLevel __instance)
        {
            if (__instance.nitrogenEnabled)
            {
                float depthOf = Ocean.main.GetDepthOf(Player.main.gameObject);
                if (__instance.GetLevelsDangerous() && UnityEngine.Random.value < 0.025f)
                {
                    global::Utils.Assert(depthOf < __instance.safeNitrogenDepth, "see log", null);
                    LiveMixin component = Player.main.gameObject.GetComponent<LiveMixin>();
                    ErrorMessage.AddMessage("Unsafe depth. Ascend slower.");
                    component.TakeDamage(1f + (__instance.safeNitrogenDepth - depthOf) / __instance.kDepthInterval, default(Vector3), DamageType.Normal, null);
                }
                if (depthOf < __instance.safeNitrogenDepth)
                {
                    float num = 1f;
                    if (Player.main.motorMode == Player.MotorMode.Dive)
                    {
                        num = Mathf.Clamp(2f - __instance.GetComponent<Rigidbody>().velocity.magnitude, 0f, 2f) * 1f;
                    }
                    else
                    {
                        num *= 2f;
                    }
                    __instance.safeNitrogenDepth = UWE.Utils.Slerp(__instance.safeNitrogenDepth, depthOf, __instance.kDissipateScalar * num * Time.deltaTime);
                    }
            }
            return false;
        }
    }
}