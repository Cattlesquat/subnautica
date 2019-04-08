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
        //private static int frametimer = 0;

        [HarmonyPrefix]
        public static bool Prefix (ref NitrogenLevel __instance)
        {
            if (__instance.nitrogenEnabled)
            {
                float depthOf = Ocean.main.GetDepthOf(Player.main.gameObject);
                if ((__instance.GetLevelsDangerous() || Player.main.motorMode != Player.MotorMode.Dive) && UnityEngine.Random.value < 0.025f)
                {
                    global::Utils.Assert(depthOf < __instance.safeNitrogenDepth, "see log", null);
                    LiveMixin component = Player.main.gameObject.GetComponent<LiveMixin>();
                    ErrorMessage.AddMessage("Nitrogen level unsafe. Ascend slower.");
                    component.TakeDamage(1f + 1.5f * (__instance.safeNitrogenDepth - depthOf) / __instance.kDepthInterval, default(Vector3), DamageType.Normal, null);
                }
                float num = 1f;
                if (depthOf < __instance.safeNitrogenDepth && Player.main.motorMode == Player.MotorMode.Dive)
                {
                    num = Mathf.Clamp(2f - __instance.GetComponent<Rigidbody>().velocity.magnitude, 0f, 2f) * 1f;
                    __instance.safeNitrogenDepth = UWE.Utils.Slerp(__instance.safeNitrogenDepth, depthOf, __instance.kDissipateScalar * num * Time.deltaTime);
                    ErrorMessage.AddMessage("safeNitrogenDepth: " + __instance.safeNitrogenDepth.ToString());
                }
                else if (Player.main.motorMode != Player.MotorMode.Dive)
                {
                    num = Mathf.Clamp(2f - __instance.GetComponent<Rigidbody>().velocity.magnitude, 0f, 2f) * 1f;
                    __instance.safeNitrogenDepth = UWE.Utils.Slerp(__instance.safeNitrogenDepth, depthOf, __instance.kDissipateScalar * num * Time.deltaTime);
                    ErrorMessage.AddMessage("safeNitrogenDepth: " + __instance.safeNitrogenDepth.ToString());
                }
                /*
                 * Silly me. Bends aren't a think in submarines. If they were, this code would definitely let you undergo compression as you explore.
                 * This code can be repurposed, though.
                 *Scene menuCheck = SceneManager.GetActiveScene();
                 *string isInMenu = menuCheck.name;
                 *if (depthOf > __instance.safeNitrogenDepth && Player.main.motorMode != Player.MotorMode.Dive && !isInMenu.ToLower().Contains("menu"))
                 *{
                 *    if (frametimer > 1800)
                 *    {
                 *        float num = __instance.depthCurve.Evaluate(depthOf / 2048f);
                 *        __instance.safeNitrogenDepth = UWE.Utils.Slerp(__instance.safeNitrogenDepth, depthOf, num * __instance.kBreathScalar);
                 *        frametimer = 0;
                 *        ErrorMessage.AddMessage("safeNitrogenDepth: " + __instance.safeNitrogenDepth.ToString());
                 *    }
                 *    else
                 *    {
                 *        frametimer++;
                 *        //if (frametimer % 600 == 0)
                 *            //ErrorMessage.AddMessage("frametimer: " + frametimer.ToString());
                 *    }
                 *}
                 */
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
                float depthOf = Ocean.main.GetDepthOf(player.gameObject);
                float num = __instance.depthCurve.Evaluate(depthOf / 2048f);
                __instance.safeNitrogenDepth = UWE.Utils.Slerp(__instance.safeNitrogenDepth, depthOf, num * __instance.kBreathScalar * 1.25f);
                //ErrorMessage.AddMessage("safeNitrogenDepth: " + __instance.safeNitrogenDepth.ToString());
            }
            return false;
        }
    }
}