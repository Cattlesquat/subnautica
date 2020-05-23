namespace NitrogenMod.Patchers
{
    using Harmony;
    using UnityEngine;
    using Items;
    using NMBehaviours;
    using Common;

    [HarmonyPatch(typeof(NitrogenLevel))]
    [HarmonyPatch("Update")]
    internal class NitroDamagePatcher
    {
        private static bool lethal = true;
        private static bool _cachedActive = false;
        private static bool _cachedAnimating = false;
        private static bool decompressionVehicles = false;

        private static float damageScaler = 1f;
        private static float rpgScaler = 1f;
        
        [HarmonyPrefix]
        public static bool Prefix (ref NitrogenLevel __instance)
        {
            if (__instance.nitrogenEnabled && GameModeUtils.RequiresOxygen() && Time.timeScale > 0f)
            {
                float depthOf = Ocean.main.GetDepthOf(Player.main.gameObject);

                if (depthOf < __instance.safeNitrogenDepth - 10f && UnityEngine.Random.value < 0.0125f)
                {
                    LiveMixin component = Player.main.gameObject.GetComponent<LiveMixin>();
                    float damage = 1f + damageScaler * (__instance.safeNitrogenDepth - depthOf) / 10f;
                    if (component.health - damage > 0f)
                        component.TakeDamage(damage, default, DamageType.Normal, null);
                    else if (lethal)
                        component.TakeDamage(damage, default, DamageType.Normal, null);
                }

                bool dVFlag = false;
                if ((Player.main.GetCurrentSub() != null && Player.main.GetCurrentSub().isCyclops) || (Player.main.GetVehicle() != null && (Player.main.GetVehicle().vehicleDefaultName.Equals("seamoth", System.StringComparison.CurrentCultureIgnoreCase) || Player.main.GetVehicle().vehicleDefaultName.Equals("exosuit", System.StringComparison.CurrentCultureIgnoreCase))))
                    dVFlag = true;

                if (__instance.safeNitrogenDepth > 10f && !Player.main.IsSwimming() && UnityEngine.Random.value < 0.025f)
                {
                    if (!dVFlag || !decompressionVehicles)
                    {
                        float atmosPressure = __instance.safeNitrogenDepth - 10f;
                        if (atmosPressure < 0f)
                            atmosPressure = 0f;
                        LiveMixin component = Player.main.gameObject.GetComponent<LiveMixin>();
                        float damage = 1f + damageScaler * (__instance.safeNitrogenDepth - atmosPressure) / 10f;
                        damage *= rpgScaler;
                        if (component.health - damage > 0f)
                            component.TakeDamage(damage, default, DamageType.Normal, null);
                        else if (lethal)
                            component.TakeDamage(damage, default, DamageType.Normal, null);
                    }
                }

                float num = 1f;
                if (depthOf < __instance.safeNitrogenDepth && Player.main.IsSwimming())
                {
                    num = Mathf.Clamp(2f - __instance.GetComponent<Rigidbody>().velocity.magnitude, 0f, 2f) * 1f;
                    __instance.safeNitrogenDepth = UWE.Utils.Slerp(__instance.safeNitrogenDepth, depthOf, __instance.kDissipateScalar * num * Time.deltaTime);
                }
                else if (!Player.main.IsSwimming() && __instance.safeNitrogenDepth > 0f)
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

        private static bool DecompressionSub(Player main)
        {
            bool isInSub = false;
            return isInSub;
        }

        public static void Lethality(bool isLethal)
        {
            lethal = isLethal;
        }

        public static void AdjustScaler(float val)
        {
            damageScaler = val;
        }

        public static void SetDecomVeh(bool val)
        {
            decompressionVehicles = val;
        }

        public static void AdjustRPGScaler(float val)
        {
            rpgScaler = val; // For RPG Mod
        }

        private static void HUDController(NitrogenLevel nitrogenInstance)
        {
            if (_cachedActive)
                BendsHUDController.SetDepth(Mathf.RoundToInt(nitrogenInstance.safeNitrogenDepth));
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
            if (nitrogenInstance.safeNitrogenDepth > 25f && !_cachedAnimating)
            {
                BendsHUDController.SetFlashing(true);
                _cachedAnimating = true;
            }
            else if (nitrogenInstance.safeNitrogenDepth < 15f && _cachedAnimating)
            {
                BendsHUDController.SetFlashing(false);
                _cachedAnimating = false;
            }
        }
    }

    [HarmonyPatch(typeof(NitrogenLevel))]
    [HarmonyPatch("Start")]
    internal class NitroStartPatcher
    {
        [HarmonyPostfix]
        public static void Postfix(ref NitrogenLevel __instance)
        {
            __instance.nitrogenEnabled = Main.nitrogenEnabled;
            __instance.safeNitrogenDepth = 0f;
            
            if (Main.specialtyTanks)
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