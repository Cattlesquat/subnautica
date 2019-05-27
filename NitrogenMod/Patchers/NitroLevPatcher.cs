namespace NitrogenMod.Patchers
{
    using Harmony;
    using UnityEngine;
    using Items;
    using NMBehaviours;

    [HarmonyPatch(typeof(NitrogenLevel))]
    [HarmonyPatch("Update")]
    internal class NitroDamagePatcher
    {
        private static bool lethal = true;
        private static bool _cachedActive = false;
        private static bool _cachedAnimating = false;

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
                }
                else if (Player.main.motorMode != Player.MotorMode.Dive && __instance.safeNitrogenDepth > 0f)
                {
                    float atmosPressure = __instance.safeNitrogenDepth - 10f;
                    if (atmosPressure < 0f)
                        atmosPressure = 0f;
                    __instance.safeNitrogenDepth = UWE.Utils.Slerp(__instance.safeNitrogenDepth, atmosPressure, __instance.kDissipateScalar * 2f * Time.deltaTime);
                }
                HUDController(__instance);
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

        private static void HUDController(NitrogenLevel nitrogenInstance)
        {
            if (nitrogenInstance.safeNitrogenDepth > 10f && !_cachedActive)
            {
                BendsHUDController.SetActive(true);
                _cachedActive = true;
            }
            else if (nitrogenInstance.safeNitrogenDepth < 10f && _cachedActive)
            {
                BendsHUDController.SetActive(false);
                _cachedActive = false;
            }
            if (nitrogenInstance.safeNitrogenDepth > 50f && !_cachedAnimating)
            {
                BendsHUDController.SetFlashing(true);
                _cachedAnimating = true;
            }
            else if (nitrogenInstance.safeNitrogenDepth < 50f && _cachedAnimating)
            {
                BendsHUDController.SetFlashing(false);
                _cachedAnimating = false;
            }
        }
    }

    [HarmonyPatch(typeof(NitrogenLevel))]
    [HarmonyPatch("OnTookBreath")]
    internal class BreathPatcher
    {
        private static bool crushEnabled = false;

        [HarmonyPrefix]
        public static bool Prefix (ref NitrogenLevel __instance, Player player)
        {
            Inventory main = Inventory.main;
            int reinforcedSuit1 = main.equipment.GetCount(TechType.ReinforcedDiveSuit);
            int reinforcedSuit2 = main.equipment.GetCount(ReinforcedSuitsCore.ReinforcedSuit2ID);
            int reinforcedSuit3 = main.equipment.GetCount(ReinforcedSuitsCore.ReinforcedSuit3ID);
            int reinforcedStill = main.equipment.GetCount(ReinforcedSuitsCore.ReinforcedStillSuit);
            int reinforcedSuits = reinforcedSuit1 + reinforcedSuit2 + reinforcedSuit3 + reinforcedStill;

            if (GameModeUtils.RequiresOxygen())
            {
                if (__instance.nitrogenEnabled)
                {
                    float depthOf = Ocean.main.GetDepthOf(player.gameObject);
                    if (depthOf > 0f)
                    {
                        if (reinforcedSuit1 > 0)
                            depthOf /= 1.5f;
                        else if (reinforcedSuit2 > 0 || reinforcedStill > 0)
                            depthOf /= 2f;
                        else if (reinforcedSuit3 > 0)
                            depthOf /= 2.5f;
                    }
                    float num = __instance.depthCurve.Evaluate(depthOf / 2048f);
                    __instance.safeNitrogenDepth = UWE.Utils.Slerp(__instance.safeNitrogenDepth, depthOf, num * __instance.kBreathScalar * .75f);
                }
                if (crushEnabled && Player.main.motorMode == Player.MotorMode.Dive)
                {
                    float depthOf = Ocean.main.GetDepthOf(player.gameObject);
                    if (UnityEngine.Random.value < 0.5f)
                    {
                        if (depthOf > 500f && reinforcedSuits < 1)
                        {
                            DamagePlayer(depthOf);
                        }
                        else if (depthOf > 1000f && reinforcedSuit2 < 1 && reinforcedSuit3 < 1 && reinforcedStill < 1)
                        {
                            DamagePlayer(depthOf);
                        }
                        else if (depthOf > 1300f && reinforcedSuit3 < 1)
                            DamagePlayer(depthOf);
                    }
                }
            }
            return false;
        }

        private static void DamagePlayer(float depthOf)
        {
            LiveMixin component = Player.main.gameObject.GetComponent<LiveMixin>();
            component.TakeDamage(UnityEngine.Random.value * (depthOf - 500f) / 50f, default, DamageType.Normal, null);
        }

        public static void EnableCrush (bool isEnabled)
        {
            crushEnabled = isEnabled;
        }
    }

    [HarmonyPatch(typeof(NitrogenLevel))]
    [HarmonyPatch("Start")]
    internal class NitroStartPatcher
    {
        [HarmonyPostfix]
        public static void Postfix(ref NitrogenLevel __instance)
        {
            NitrogenOptions options = new NitrogenOptions();
            __instance.nitrogenEnabled = options.nitroEnabled;
            __instance.safeNitrogenDepth = 0f;
            NitroDamagePatcher.Lethality(options.nitroLethal);
            NitroDamagePatcher.AdjustScaler(options.damageScaler);
            BreathPatcher.EnableCrush(options.crushEnabled);

            Player.main.gameObject.AddComponent<SpecialtyTanks>();
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