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
}
