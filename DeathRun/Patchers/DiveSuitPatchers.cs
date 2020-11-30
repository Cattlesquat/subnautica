/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 * This section taken with minor adjustments from Seraphim Risen's NitrogenMod
 */
namespace DeathRun.Patchers
{
    using HarmonyLib;
    using Items;

    // Code provided by AlexejheroYTB to remove a destructive prefix
    [HarmonyPatch(typeof(Player), nameof(Player.HasReinforcedSuit))]
    internal static class HasReinforcedSuitPatcher
    {
        [HarmonyPostfix]
        public static void Postfix(ref bool __result)
        {
            var main = Inventory.main.equipment;
            __result = __result || main.GetCount(ReinforcedSuitsCore.ReinforcedStillSuit) + main.GetCount(ReinforcedSuitsCore.ReinforcedSuit2ID) + main.GetCount(ReinforcedSuitsCore.ReinforcedSuit3ID) > 0;
        }
    }

    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("UpdateReinforcedSuit")]
    internal class UpdateReinforcedSuitPatcher
    {
        [HarmonyPrefix]
        public static bool Prefix (ref Player __instance)
        {
            __instance.temperatureDamage.minDamageTemperature = 49f;
            TechType bodySlot = Inventory.main.equipment.GetTechTypeInSlot("Body");

            float crushDepth = 200f;

            if (bodySlot == TechType.RadiationSuit)
            {
                crushDepth = 500f;
            }
            else if (bodySlot == TechType.ReinforcedDiveSuit)
            {
                __instance.temperatureDamage.minDamageTemperature += 15f;

                if (Config.DEATHRUN.Equals(DeathRun.config.personalCrushDepth))
                {
                    crushDepth = 800f;
                } else
                {
                    crushDepth = 8000f;
                }
            }
            else if (bodySlot == TechType.Stillsuit)
            {
                if (Config.DEATHRUN.Equals(DeathRun.config.personalCrushDepth))
                {
                    crushDepth = 800f;
                }
                else
                {
                    crushDepth = 1300f;
                }
            }
            else if (bodySlot == ReinforcedSuitsCore.ReinforcedStillSuit)
            {
                __instance.temperatureDamage.minDamageTemperature += 15f;

                if (Config.DEATHRUN.Equals(DeathRun.config.personalCrushDepth))
                {
                    crushDepth = 1300f;
                }
                else
                {
                    crushDepth = 8000f;
                }
            }
            else if (bodySlot == ReinforcedSuitsCore.ReinforcedSuit2ID)
            {
                __instance.temperatureDamage.minDamageTemperature += 20f;
                if (Config.DEATHRUN.Equals(DeathRun.config.personalCrushDepth))
                {
                    crushDepth = 1300f;
                }
                else
                {
                    crushDepth = 8000f;
                }
            }
            else if (bodySlot == ReinforcedSuitsCore.ReinforcedSuit3ID)
            {
                __instance.temperatureDamage.minDamageTemperature += 35f;
                crushDepth = 8000f;
            }

            if (Config.NORMAL.Equals(DeathRun.config.personalCrushDepth))
            {
                crushDepth = 8001f;
            }

            if (__instance.HasReinforcedGloves())
            {
                __instance.temperatureDamage.minDamageTemperature += 6f;
            }
            PlayerGetDepthClassPatcher.divingCrushDepth = crushDepth;

            if (crushDepth < 8000f)
                ErrorMessage.AddMessage("Safe diving depth now " + crushDepth.ToString() + ".");            
            else if (crushDepth == 8000f)
                ErrorMessage.AddMessage("Safe diving depth now unlimited.");
            
            return false;
        }
    }
}
