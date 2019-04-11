namespace NitrogenMod.Patchers
{
    using Harmony;
    using UnityEngine;
    using UWE;
    using UnityEngine.UI;
    using UnityEngine.SceneManagement;

    [HarmonyPatch(typeof(NitrogenLevel))]
    [HarmonyPatch("Update")]
    internal class NitroDamagePatcher
    {
        private static bool lethal = true;

        private static float damageScaler = 0f;
        
        [HarmonyPrefix]
        public static bool Prefix (ref NitrogenLevel __instance)
        {
            if (__instance.nitrogenEnabled && GameModeUtils.RequiresOxygen() && Time.timeScale > 0f)
            {
                float depthOf = Ocean.main.GetDepthOf(Player.main.gameObject);
                if (depthOf < __instance.safeNitrogenDepth - 10f && UnityEngine.Random.value < 0.0125f)
                {
                    global::Utils.Assert(depthOf < __instance.safeNitrogenDepth, "see log", null);
                    LiveMixin component = Player.main.gameObject.GetComponent<LiveMixin>();
                    ErrorMessage.AddMessage("WARNING: Experiencing unsafe decompression. Ascend slower.");
                    float damage = 1f + damageScaler * (__instance.safeNitrogenDepth - depthOf) / 10f;
                    if (component.health - damage > 0f)
                        component.TakeDamage(damage, default, DamageType.Normal, null);
                    else if (lethal)
                        component.TakeDamage(damage, default, DamageType.Normal, null);
                }
                if (__instance.safeNitrogenDepth > 10f && Player.main.motorMode != Player.MotorMode.Dive && UnityEngine.Random.value < 0.025f)
                {
                    float atmosPressure = __instance.safeNitrogenDepth - 10f;
                    if (atmosPressure < 0f)
                        atmosPressure = 0f;
                    LiveMixin component = Player.main.gameObject.GetComponent<LiveMixin>();
                    ErrorMessage.AddMessage("WARNING: Experiencing unsafe decompression");
                    float damage = 1f + damageScaler * (__instance.safeNitrogenDepth - atmosPressure) / 10f;
                    if (component.health - damage > 0f)
                        component.TakeDamage(damage, default, DamageType.Normal, null);
                    else if (lethal)
                        component.TakeDamage(damage, default, DamageType.Normal, null);
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
                    __instance.safeNitrogenDepth = UWE.Utils.Slerp(__instance.safeNitrogenDepth, atmosPressure, __instance.kDissipateScalar * 2f * Time.deltaTime);
                    //ErrorMessage.AddMessage("safeNitrogenDepth: " + __instance.safeNitrogenDepth.ToString());
                }
            }
            return false;
        }

        public static void Lethality(bool isLethal)
        {
            lethal = isLethal;
        }

        public static void AdjustScaler(float val)
        {
            damageScaler = val;
        }
    }

    [HarmonyPatch(typeof(NitrogenLevel))]
    [HarmonyPatch("OnTookBreath")]
    internal class NitroBreathPatcher
    {
        private static bool crushEnabled = false;

        private static float crushDepth = 500f;
        
        [HarmonyPrefix]
        public static bool Prefix (ref NitrogenLevel __instance, Player player)
        {
            int reinforcedSuit = Inventory.main.equipment.GetCount(TechType.ReinforcedDiveSuit);
            if (GameModeUtils.RequiresOxygen())
            {
                if (__instance.nitrogenEnabled && reinforcedSuit < 1)
                {
                    float depthOf = Ocean.main.GetDepthOf(player.gameObject);
                    float num = __instance.depthCurve.Evaluate(depthOf / 2048f);
                    __instance.safeNitrogenDepth = UWE.Utils.Slerp(__instance.safeNitrogenDepth, depthOf, num * __instance.kBreathScalar * .75f);
                    //ErrorMessage.AddMessage("safeNitrogenDepth: " + __instance.safeNitrogenDepth.ToString());
                }
                if (crushEnabled && reinforcedSuit < 1 && Player.main.motorMode == Player.MotorMode.Dive)
                {
                    float depthOf = Ocean.main.GetDepthOf(player.gameObject);
                    LiveMixin component = Player.main.gameObject.GetComponent<LiveMixin>();
                    ErrorMessage.AddMessage("WARNING: Water pressure exceeding safe level");
                    if (depthOf > crushDepth)
                    {
                        float damage = UnityEngine.Random.value * depthOf / 100f;
                    }
                }
            }
            return false;
        }

        public static void EnableCrush (bool isEnabled)
        {
            crushEnabled = isEnabled;
        }

        public static void AdjustCrush (float newDepth)
        {
            crushDepth = newDepth;
        }
    }

    [HarmonyPatch(typeof(NitrogenLevel))]
    [HarmonyPatch("Start")]
    internal class NitroLevPatcher
    {
        [HarmonyPostfix]
        public static void Postfix(ref NitrogenLevel __instance)
        {
            NitrogenOptions isEnabled = new NitrogenOptions();
            __instance.nitrogenEnabled = isEnabled.nitroEnabled;
            __instance.safeNitrogenDepth = 0f;
        }
    }

    [HarmonyPatch(typeof(NitrogenLevel))]
    [HarmonyPatch("OnRespawn")]
    internal class RespawnPatcher
    {
        [HarmonyPostfix]
        public static void Postfix(ref NitrogenLevel __instance)
        {
            __instance.safeNitrogenDepth = 0f;
        }
    }
}