namespace NitrogenMod.Patchers
{
    using Harmony;
    using UnityEngine;
    using UWE;
    using UnityEngine.UI;
    using UnityEngine.SceneManagement;

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
                if (depthOf < __instance.safeNitrogenDepth - 10f && UnityEngine.Random.value < 0.025f)
                {
                    global::Utils.Assert(depthOf < __instance.safeNitrogenDepth, "see log", null);
                    LiveMixin component = Player.main.gameObject.GetComponent<LiveMixin>();
                    ErrorMessage.AddMessage("WARNING: Experiencing unsafe decompression. Ascend slower.");
                    component.TakeDamage(1f + (__instance.safeNitrogenDepth - depthOf) / 10f, default(Vector3), DamageType.Normal, null);
                }
                if (__instance.safeNitrogenDepth > 0f && Player.main.motorMode != Player.MotorMode.Dive && UnityEngine.Random.value < 0.025f)
                {
                    float atmosPressure = __instance.safeNitrogenDepth - 10f;
                    if (atmosPressure < 0f)
                        atmosPressure = 0f;
                    LiveMixin component = Player.main.gameObject.GetComponent<LiveMixin>();
                    ErrorMessage.AddMessage("WARNING: Experiencing unsafe decompression");
                    component.TakeDamage(1f + (__instance.safeNitrogenDepth - atmosPressure) /10f, default(Vector3), DamageType.Normal, null);
                }
                float num = 1f;
                if (depthOf < __instance.safeNitrogenDepth && Player.main.motorMode == Player.MotorMode.Dive)
                {
                    num = Mathf.Clamp(2f - __instance.GetComponent<Rigidbody>().velocity.magnitude, 0f, 2f) * 1f;
                    __instance.safeNitrogenDepth = UWE.Utils.Slerp(__instance.safeNitrogenDepth, depthOf, __instance.kDissipateScalar * num * Time.deltaTime);
                    //ErrorMessage.AddMessage("safeNitrogenDepth: " + __instance.safeNitrogenDepth.ToString());
                }
                else if (Player.main.motorMode != Player.MotorMode.Dive && __instance.safeNitrogenDepth > 0f)
                {
                    float atmosPressure = __instance.safeNitrogenDepth - 10f;
                    if (atmosPressure < 0f)
                        atmosPressure = 0f;
                    __instance.safeNitrogenDepth = UWE.Utils.Slerp(__instance.safeNitrogenDepth, atmosPressure, __instance.kDissipateScalar * num * Time.deltaTime);
                    //ErrorMessage.AddMessage("safeNitrogenDepth: " + __instance.safeNitrogenDepth.ToString());
                }
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(NitrogenLevel))]
    [HarmonyPatch("OnTookBreath")]
    internal class NitroBreathPatcher
    {
        [HarmonyPrefix]
        public static bool Prefix (ref NitrogenLevel __instance, Player player)
        {
            if (__instance.nitrogenEnabled)
            {
                int reinforcedSuit = 0;
                Inventory inv = Inventory.main;
                reinforcedSuit = inv.equipment.GetCount(TechType.ReinforcedDiveSuit);
                if (reinforcedSuit < 1)
                {
                    float depthOf = Ocean.main.GetDepthOf(player.gameObject);
                    float num = __instance.depthCurve.Evaluate(depthOf / 2048f);
                    __instance.safeNitrogenDepth = UWE.Utils.Slerp(__instance.safeNitrogenDepth, depthOf, num * __instance.kBreathScalar * 1.10f);
                }
                //ErrorMessage.AddMessage("safeNitrogenDepth: " + __instance.safeNitrogenDepth.ToString());
            }
            return false;
        }
    }
}