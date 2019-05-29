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
            TechType bodySlot = Inventory.main.equipment.GetTechTypeInSlot("Body");
            TechType headSlot = Inventory.main.equipment.GetTechTypeInSlot("Head");
            
            if (GameModeUtils.RequiresOxygen())
            {
                if (__instance.nitrogenEnabled)
                {
                    float depthOf = Ocean.main.GetDepthOf(player.gameObject);
                    float depthOfModified = depthOf;
                    if (depthOf > 0f)
                    {
                        if (headSlot == TechType.Rebreather)
                            depthOfModified *= 0.95f;
                        if (bodySlot == ReinforcedSuitsCore.ReinforcedSuit3ID)
                            depthOfModified *= 0.3f;
                        else if ((bodySlot == ReinforcedSuitsCore.ReinforcedSuit2ID || bodySlot == ReinforcedSuitsCore.ReinforcedStillSuit) && depthOf <= 1300f)
                            depthOfModified *= 0.5f;
                        else if (bodySlot == TechType.ReinforcedDiveSuit && depthOf <= 800f)
                            depthOfModified *= 0.75f;
                        else if ((bodySlot == TechType.RadiationSuit || bodySlot == TechType.Stillsuit) && depthOf <= 500f)
                            depthOfModified *= 0.9f;
                    }
                    float num = __instance.depthCurve.Evaluate(depthOf / 2048f);
                    __instance.safeNitrogenDepth = UWE.Utils.Slerp(__instance.safeNitrogenDepth, depthOf, num * __instance.kBreathScalar * .75f);
                }

                if (crushEnabled && Player.main.motorMode == Player.MotorMode.Dive)
                {
                    float depthOf = Ocean.main.GetDepthOf(player.gameObject);
                    if (UnityEngine.Random.value < 0.5f)
                    {
                        if (bodySlot == ReinforcedSuitsCore.ReinforcedSuit3ID)
                            return false;
                        else if ((bodySlot == ReinforcedSuitsCore.ReinforcedSuit2ID || bodySlot == ReinforcedSuitsCore.ReinforcedStillSuit) && depthOf >= 1300f)
                            DamagePlayer(depthOf - 1300f);
                        else if ((bodySlot == TechType.ReinforcedDiveSuit || bodySlot == TechType.Stillsuit) && depthOf >= 800f)
                            DamagePlayer(depthOf - 800f);
                        else if (bodySlot == TechType.RadiationSuit && depthOf >= 500f)
                            DamagePlayer(depthOf - 500f);
                        else if (depthOf >= 200f)
                            DamagePlayer(depthOf - 200f);
                    }
                }
            }
            return false;
        }

        private static void DamagePlayer(float depthOf)
        {
            LiveMixin component = Player.main.gameObject.GetComponent<LiveMixin>();
            component.TakeDamage(UnityEngine.Random.value * depthOf / 50f, default, DamageType.Normal, null);
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