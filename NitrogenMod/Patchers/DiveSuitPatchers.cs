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
            if (__instance.HasReinforcedSuit())
            {
                if (bodySlot == TechType.ReinforcedDiveSuit || bodySlot == ReinforcedSuitsCore.ReinforcedStillSuit)
                    __instance.temperatureDamage.minDamageTemperature += 15f;
                else if (bodySlot == ReinforcedSuitsCore.ReinforcedSuit2ID)
                    __instance.temperatureDamage.minDamageTemperature += 20f;
                else if (bodySlot == ReinforcedSuitsCore.ReinforcedSuit3ID)
                    __instance.temperatureDamage.minDamageTemperature += 35f;
            }
            if (__instance.HasReinforcedGloves())
            {
                __instance.temperatureDamage.minDamageTemperature += 6f;
            }
            if (bodySlot == TechType.RadiationSuit)
                ErrorMessage.AddMessage("Safe diving depth now 500m.");
            else if (bodySlot == TechType.Stillsuit || bodySlot == TechType.ReinforcedDiveSuit)
                ErrorMessage.AddMessage("Safe diving depth now 800m.");
            else if (bodySlot == ReinforcedSuitsCore.ReinforcedStillSuit || bodySlot == ReinforcedSuitsCore.ReinforcedSuit2ID)
                ErrorMessage.AddMessage("Safe diving depth now 1300m.");
            else if (bodySlot == ReinforcedSuitsCore.ReinforcedSuit3ID)
                ErrorMessage.AddMessage("Safe diving depth now unlimited.");
            else
                ErrorMessage.AddMessage("Safe diving depth now 200m.");
            return false;
        }
    }
}
