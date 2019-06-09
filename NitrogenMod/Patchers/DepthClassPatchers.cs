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
        public static float divingCrushDepth = 200f;

        [HarmonyPostfix]
        public static void Postfix(ref Ocean.DepthClass __result)
        {
            float depth = Ocean.main.GetDepthOf(Player.main.gameObject);
            __result = Ocean.DepthClass.Safe;

            if (Player.main.IsSwimming() && depth >= divingCrushDepth)
                __result = Ocean.DepthClass.Crush;
        }
    }

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