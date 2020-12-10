/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 * This section inspired by Seraphim Risen's Nitrogen Mod, but I have simplified the use of First Aid Kits (always works for both
 * nitrogen and health, to save player having to try to understand which will be affected). Have also adapted the effects to be
 * appropriate for current Nitrogen balance settings. Added some player feedback.
 * 
 * Also stopped it from giving major food & water bonuses for getting killed.
 * 
 * Eating raw Boomerangs helps with nitrogen (the way eating raw bladderfish helps with oxygen in the vanilla game)
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

                    if (DeathRun.saveData.nitroSave.safeDepth > 10f)
                    {
                        DeathRun.saveData.nitroSave.safeDepth /= 2;

                        if (Time.time - ticksNotice > 60)
                        {
                            ticksNotice = Time.time;
                            ErrorMessage.AddMessage("First Aid Kit helps purge Nitrogen from your bloodstream.");
                        }

                        if (DeathRun.saveData.nitroSave.safeDepth < 10f)
                        {
                            nitrogenLevel.nitrogenLevel = DeathRun.saveData.nitroSave.safeDepth * 10;
                        }
                    }
                    else
                    {
                        nitrogenLevel.nitrogenLevel = 0;
                    }
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Survival))]
    [HarmonyPatch("Eat")]
    internal class SurvivalEatPatcher
    {
        static float ticksNotice = 0;

        [HarmonyPostfix]
        public static void Postfix(Survival __instance, GameObject useObj, bool __result)
        {
            if (__result && (useObj != null))
            {
                TechType techType = CraftData.GetTechType(useObj);
                if ((techType == TechType.Boomerang) || (techType == TechType.LavaBoomerang))
                {
                    NitrogenLevel nitrogenLevel = Player.main.gameObject.GetComponent<NitrogenLevel>();

                    if (DeathRun.saveData.nitroSave.safeDepth > 10f)
                    {
                        DeathRun.saveData.nitroSave.safeDepth /= 2;

                        if (Time.time - ticksNotice > 60)
                        {
                            ticksNotice = Time.time;
                            ErrorMessage.AddMessage("The tasty raw Boomerang helps purge Nitrogen from your bloodstream!");
                        }

                        if (DeathRun.saveData.nitroSave.safeDepth < 10f)
                        {
                            nitrogenLevel.nitrogenLevel = DeathRun.saveData.nitroSave.safeDepth * 10;
                        }
                    }
                    else
                    {
                        nitrogenLevel.nitrogenLevel = 0;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Survival))]
    [HarmonyPatch("Reset")]
    internal class SurvivalResetPatcher
    {
        /**
         * After very beginning of the game, don't give a full food & water reset on death.
         */
        [HarmonyPrefix]
        public static bool Prefix(ref Survival __instance)
        {
            if (DayNightCycle.main.timePassedAsFloat >= 5 * 60)
            {
                __instance.food  = Mathf.Clamp(__instance.food * .9f,  12f, 90.5f);
                __instance.water = Mathf.Clamp(__instance.water * .9f, 12f, 90.5f);
                return false;
            }
            return true;
        }
    }
}
