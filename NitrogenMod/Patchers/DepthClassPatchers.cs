namespace NitrogenMod.Patchers
{
    using Harmony;
    using Items;
    using NMBehaviours;
    using Common;

    [HarmonyPatch(typeof(uGUI_DepthCompass))]
    [HarmonyPatch("OnDepthClassChanged")]
    internal class DepthCompassDepthClassPatcher
    {
        [HarmonyPrefix]
        public static bool Prefix(ref uGUI_DepthCompass __instance)
        {
            Ocean.DepthClass depthClass = Player.main.GetDepthClass();

            if (__instance._cachedDepthClass != depthClass)
            {
                __instance._cachedDepthClass = depthClass;
                if (__instance._depthMode == uGUI_DepthCompass.DepthMode.Player)
                {
                    __instance.UpdateHalfMoonSprite();
                }
            }
            switch (depthClass)
            {
                case Ocean.DepthClass.Unsafe:
                case Ocean.DepthClass.Crush:
                    __instance.shadow.sprite = __instance.shadowDanger;
                    MaterialExtensions.SetBlending(__instance.shadow.material, Blending.AlphaBlend, false);
                    __instance.depthText.color = __instance.textColorDanger;
                    __instance.suffixText.color = __instance.textColorDanger;
                    return false;
            }
            __instance.shadow.sprite = __instance.shadowNormal;
            MaterialExtensions.SetBlending(__instance.shadow.material, Blending.Multiplicative, true);
            __instance.depthText.color = __instance.textColorNormal;
            __instance.suffixText.color = __instance.textColorNormal;
            return false;
        }
    }

    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("GetDepthClass")]
    internal class PlayerGetDepthClassPatcher
    {
        [HarmonyPostfix]
        public static void Postfix(ref Ocean.DepthClass __result)
        {
            float depth = Ocean.main.GetDepthOf(Player.main.gameObject);
            TechType bodySlot = Inventory.main.equipment.GetTechTypeInSlot("Body");

            if (Player.main.motorMode == Player.MotorMode.Dive)
            {
                if (bodySlot == ReinforcedSuitsCore.ReinforcedSuit3ID)
                    __result = Ocean.DepthClass.Safe;
                else
                {
                    if ((bodySlot == ReinforcedSuitsCore.ReinforcedSuit2ID || bodySlot == ReinforcedSuitsCore.ReinforcedStillSuit) && depth <= 1300f)
                        __result = Ocean.DepthClass.Safe;
                    else
                    {
                        if ((bodySlot == TechType.ReinforcedDiveSuit || bodySlot == TechType.Stillsuit) && depth <= 800f)
                            __result = Ocean.DepthClass.Safe;
                        else
                        {
                            if (bodySlot == TechType.RadiationSuit && depth <= 500f)
                                __result = Ocean.DepthClass.Safe;
                            else
                            {
                                if (depth >= 200f)
                                    __result = Ocean.DepthClass.Crush;
                            }
                        }
                    }
                }
            }
            else
                __result = Ocean.DepthClass.Safe;
        }
    }

    // Throwing this patch in here since this is all essentially a HUD patch
    [HarmonyPatch(typeof(uGUI_DepthCompass))]
    [HarmonyPatch("Start")]
    internal class UGUIPatcher
    {
        [HarmonyPostfix]
        public static void Postfix(ref uGUI_DepthCompass __instance)
        {
            __instance.gameObject.AddComponent<BendsHUDController>();
        }
    }
}