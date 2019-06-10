namespace NitrogenMod.Patchers
{
    using Harmony;
    using Items;

    [HarmonyPatch(typeof(NitrogenLevel))]
    [HarmonyPatch("OnTookBreath")]
    internal class BreathPatcher
    {
        private static bool crushEnabled = false;

        [HarmonyPrefix]
        public static bool Prefix(ref NitrogenLevel __instance, Player player)
        {
            Inventory main = Inventory.main;
            TechType bodySlot = Inventory.main.equipment.GetTechTypeInSlot("Body");
            TechType headSlot = Inventory.main.equipment.GetTechTypeInSlot("Head");

            if (GameModeUtils.RequiresOxygen())
            {
                float depthOf = Ocean.main.GetDepthOf(player.gameObject);
                if (__instance.nitrogenEnabled)
                {
                    float modifier = 1f;
                    if (depthOf > 0f)
                    {
                        if (bodySlot == ReinforcedSuitsCore.ReinforcedSuit3ID)
                            modifier = 0.55f;
                        else if ((bodySlot == ReinforcedSuitsCore.ReinforcedSuit2ID || bodySlot == ReinforcedSuitsCore.ReinforcedStillSuit) && depthOf <= 1300f)
                            modifier = 0.75f;
                        else if (bodySlot == TechType.ReinforcedDiveSuit && depthOf <= 800f)
                            modifier = 0.85f;
                        else if ((bodySlot == TechType.RadiationSuit || bodySlot == TechType.Stillsuit) && depthOf <= 500f)
                            modifier = 0.95f;
                        if (headSlot == TechType.Rebreather)
                            modifier -= 0.05f;
                    }
                    float num = __instance.depthCurve.Evaluate(depthOf / 2048f);
                    __instance.safeNitrogenDepth = UWE.Utils.Slerp(__instance.safeNitrogenDepth, depthOf, num * __instance.kBreathScalar * modifier);
                }

                if (crushEnabled && Player.main.GetDepthClass() == Ocean.DepthClass.Crush)
                {
                    if (UnityEngine.Random.value < 0.5f)
                    {
                        float crushDepth = PlayerGetDepthClassPatcher.divingCrushDepth;
                        if (depthOf > crushDepth)
                            DamagePlayer(depthOf - crushDepth);
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

        public static void EnableCrush(bool isEnabled)
        {
            crushEnabled = isEnabled;
        }
    }
}