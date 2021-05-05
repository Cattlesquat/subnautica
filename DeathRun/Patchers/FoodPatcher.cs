/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 * Food challenges
 */
namespace DeathRun.Patchers
{
    using HarmonyLib;
    using Items;
    using Common;
    using UnityEngine;

    [HarmonyPatch(typeof(Eatable))]
    [HarmonyPatch("GetFoodValue")]
    internal class FoodValuePatcher
    {
        [HarmonyPostfix]
        public static void Postfix(Eatable __instance, ref float __result)
        {
            if (Config.FOOD_OMNIVORE.Equals(DeathRun.config.foodChallenge))
            {
                return;
            }

            TechType t = CraftData.GetTechType(__instance.gameObject);

            if (Config.FOOD_VEGAN.Equals(DeathRun.config.foodChallenge) || Config.FOOD_PESCATARIAN.Equals(DeathRun.config.foodChallenge))
            {
                if (t == TechType.NutrientBlock)
                {
                    __result = -25f;
                    return;
                }

                if (t == TechType.Snack1) 
                {
                    __result = -25f;
                    return;
                }

                if (Config.FOOD_PESCATARIAN.Equals(DeathRun.config.foodChallenge))
                {
                    if ((t == TechType.Snack2) || (t == TechType.Snack3))
                    {
                        __result = -25f;
                        return;
                    }
                }
            }

            if ((t == TechType.CookedPeeper) || (t == TechType.Peeper) || (t == TechType.CuredPeeper) ||
                (t == TechType.CookedHoleFish) || (t == TechType.HoleFish) || (t == TechType.CuredHoleFish) ||
                (t == TechType.CookedGarryFish) || (t == TechType.GarryFish) || (t == TechType.CuredGarryFish) ||
                (t == TechType.CookedReginald) || (t == TechType.Reginald) || (t == TechType.CuredReginald) ||
                (t == TechType.CookedBladderfish) || (t == TechType.Bladderfish) || (t == TechType.CuredBladderfish) ||
                (t == TechType.CookedHoverfish) || (t == TechType.Hoverfish) || (t == TechType.CuredHoverfish) ||
                (t == TechType.CookedSpadefish) || (t == TechType.Spadefish) || (t == TechType.CuredSpadefish) ||
                (t == TechType.CookedBoomerang) || (t == TechType.Boomerang) || (t == TechType.CuredBoomerang) ||
                (t == TechType.CookedLavaBoomerang) || (t == TechType.LavaBoomerang) || (t == TechType.CuredLavaBoomerang) ||
                (t == TechType.CookedEyeye) || (t == TechType.Eyeye) || (t == TechType.CuredEyeye) ||
                (t == TechType.CookedLavaEyeye) || (t == TechType.LavaEyeye) || (t == TechType.CuredLavaEyeye) ||
                (t == TechType.CookedOculus) || (t == TechType.Oculus) || (t == TechType.CuredOculus) ||
                (t == TechType.CookedHoopfish) || (t == TechType.Hoopfish) || (t == TechType.CuredHoopfish) ||
                (t == TechType.CookedSpinefish) || (t == TechType.Spinefish) || (t == TechType.CuredSpinefish))
            {
                if (!Config.FOOD_PESCATARIAN.Equals(DeathRun.config.foodChallenge))
                {
                    __result = -25f;
                }                
            }
            else
            {
                if (Config.FOOD_PESCATARIAN.Equals(DeathRun.config.foodChallenge) && (__result > 0))
                {
                    __result = -25f;
                }
            }
        }
    }
}
