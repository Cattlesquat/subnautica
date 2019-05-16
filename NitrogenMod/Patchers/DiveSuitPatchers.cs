namespace NitrogenMod.Patchers
{
    using Harmony;
    using UnityEngine;
    using Items;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

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
            if (__instance.HasReinforcedSuit())
            {
                TechType bodySlot = Inventory.main.equipment.GetTechTypeInSlot("Body");
                if (bodySlot == TechType.ReinforcedDiveSuit || bodySlot == ReinforcedSuitsCore.ReinforcedStillSuit)
                    __instance.temperatureDamage.minDamageTemperature += 15f;
                else if (bodySlot == ReinforcedSuitsCore.ReinforcedSuit2ID)
                    __instance.temperatureDamage.minDamageTemperature += 20f;
                else if (bodySlot == ReinforcedSuitsCore.ReinforcedSuit3ID)
                    __instance.temperatureDamage.minDamageTemperature += 35f;
                /*if (bodySlot == ReinforcedSuitsCore.ReinforcedSuit2ID || bodySlot == ReinforcedSuitsCore.ReinforcedSuit3ID)
                {

                }
                else if (bodySlot == ReinforcedSuitsCore.ReinforcedStillSuit)
                {

                }*/
            }
            if (__instance.HasReinforcedGloves())
            {
                __instance.temperatureDamage.minDamageTemperature += 6f;
            }
            return false;
        }
    }
}
