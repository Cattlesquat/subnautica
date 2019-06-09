namespace NitrogenMod.Patchers
{
    using Harmony;
    using Items;

    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("HasReinforcedSuit")]
    internal class HasReinforcedSuitPatcher
    {
        [HarmonyPrefix]
        public static bool Prefix (ref bool __result)
        {
            Inventory main = Inventory.main;
            int reinforcedCount = main.equipment.GetCount(TechType.ReinforcedDiveSuit) + main.equipment.GetCount(ReinforcedSuitsCore.ReinforcedStillSuit) + main.equipment.GetCount(ReinforcedSuitsCore.ReinforcedSuit2ID) + main.equipment.GetCount(ReinforcedSuitsCore.ReinforcedSuit3ID);
            if (reinforcedCount > 0)
                __result = true;
            else
                __result = false;
            return false;
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
                crushDepth = 500f;
            else if (bodySlot == TechType.ReinforcedDiveSuit)
            {
                __instance.temperatureDamage.minDamageTemperature += 15f;
                crushDepth = 800f;
            }
            else if (bodySlot == TechType.Stillsuit)
                crushDepth = 800f;
            else if (bodySlot == ReinforcedSuitsCore.ReinforcedStillSuit)
            {
                __instance.temperatureDamage.minDamageTemperature += 15f;
                crushDepth = 1300f;
            }
            else if (bodySlot == ReinforcedSuitsCore.ReinforcedSuit2ID)
            {
                __instance.temperatureDamage.minDamageTemperature += 20f;
                crushDepth = 1300f;
            }
            else if (bodySlot == ReinforcedSuitsCore.ReinforcedSuit3ID)
            {
                __instance.temperatureDamage.minDamageTemperature += 35f;
                crushDepth = 8000f;
            }
            if (__instance.HasReinforcedGloves())
            {
                __instance.temperatureDamage.minDamageTemperature += 6f;
            }
            PlayerGetDepthClassPatcher.divingCrushDepth = crushDepth;

            if (crushDepth < 8000f)
                ErrorMessage.AddMessage("Safe diving depth now " + crushDepth.ToString() + ".");
            else
                ErrorMessage.AddMessage("Safe diving depth now unlimited.");
            
            return false;
        }
    }
}
