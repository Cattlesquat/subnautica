namespace NitrogenMod.Patchers
{
    using Harmony;
    using Items;
    using NMBehaviours;

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
        public static void Postfix(ref Player __instance, ref Ocean.DepthClass __result)
        {
            Inventory main = Inventory.main;
            int reinforcedSuit1 = main.equipment.GetCount(TechType.ReinforcedDiveSuit);
            int reinforcedSuit2 = main.equipment.GetCount(ReinforcedSuitsCore.ReinforcedSuit2ID);
            int reinforcedSuit3 = main.equipment.GetCount(ReinforcedSuitsCore.ReinforcedSuit3ID);
            int reinforcedStill = main.equipment.GetCount(ReinforcedSuitsCore.ReinforcedStillSuit);
            int reinforcedSuits = reinforcedSuit1 + reinforcedSuit2 + reinforcedSuit3 + reinforcedStill;
            float depth = Ocean.main.GetDepthOf(Player.main.gameObject);

            __result = Ocean.DepthClass.Safe;
            if ((reinforcedSuit3 > 0))
                return;
            if ((reinforcedSuit2 > 0 || reinforcedStill > 0) && depth <= 1300f)
                return;
            else if (reinforcedSuit1 > 0 && depth <= 800f)
                return;
            else if (depth <= 500f && reinforcedSuits == 0)
                return;
            else
                __result = Ocean.DepthClass.Crush;
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