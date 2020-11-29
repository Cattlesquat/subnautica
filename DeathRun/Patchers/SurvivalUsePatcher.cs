/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 * This section inspired by Seraphim Risen's Nitrogen Mod, but I have simplified the use of First Aid Kits (always works for both
 * nitrogen and health, to save player having to try to understand which will be affected). Have also adapted the effects to be
 * appropriate for current Nitrogen balance settings. Added some player feedback.
 */
namespace DeathRun.Patchers
{
    using HarmonyLib;
    using UnityEngine;

    [HarmonyPatch(typeof(Survival))]
    [HarmonyPatch("Use")]
    internal class SurvivalUsePatcher
    {
        static float ticksNotice = 0;

        [HarmonyPrefix]
        public static bool Prefix(ref Survival __instance, ref bool __result, GameObject useObj)
        {
            if (useObj != null)
            {
                TechType techType = CraftData.GetTechType(useObj);

                if (techType == TechType.FirstAidKit)
                {
                    NitrogenLevel nitrogenLevel = Player.main.gameObject.GetComponent<NitrogenLevel>();

                    if (nitrogenLevel.safeNitrogenDepth > 10f)
                    {
                        nitrogenLevel.safeNitrogenDepth /= 2;

                        if (Time.time - ticksNotice > 60)
                        {
                            ticksNotice = Time.time;
                            ErrorMessage.AddMessage("First Aid Kit helps purge Nitrogen from your bloodstream.");
                        }

                        if (nitrogenLevel.safeNitrogenDepth < 10f)
                        {
                            nitrogenLevel.nitrogenLevel = nitrogenLevel.safeNitrogenDepth * 10;
                        }
                    } else
                    {
                        nitrogenLevel.nitrogenLevel = 0;
                    }
                }
            }
            return true;
        }
    }
}
