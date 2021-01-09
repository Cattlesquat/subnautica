/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 */
using HarmonyLib;
using System;
using UnityEngine;
using System.IO;
using System.Reflection;
using Common;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using System.Collections.Generic;
using SMLHelper.V2.Crafting;
using System.Text;

namespace DeathRun.Patchers
{ 
    [HarmonyPatch(typeof(Seaglide))] 
    [HarmonyPatch("OnRightHandHeld")]
    internal class Seaglide_RightHandHeld_Patcher
    {
        [HarmonyPrefix] 
        public static bool Prefix(ref Seaglide __instance, bool __result)
        {
            if (CraftData.GetTechType(__instance.gameObject) == TechType.PowerGlide && __instance.energyMixin.charge > 0f &&
                (__instance.energyMixin.charge >= 1f || !DeathRun.saveData.playerSave.seaGlideExpended))
            {
                __instance.powerGlideActive = true;
            }

            __result = false;
            return false;
        }
    }


    [HarmonyPatch(typeof(Seaglide))]
    [HarmonyPatch("UpdateActiveState")]
    internal class Seaglide_UpdateActiveState_Patcher
    {
        static float charge = 0;

        [HarmonyPrefix]
        public static bool Prefix(ref Seaglide __instance)
        {
            charge = __instance.energyMixin.charge;
            
            if ((__instance.energyMixin.charge < 1f) && DeathRun.saveData.playerSave.seaGlideExpended)
            {
                var batt = __instance.energyMixin.battery;
                if (batt != null)
                {
                    batt.charge = 0;
                }
            } else
            {
                DeathRun.saveData.playerSave.seaGlideExpended = false;
            }
            
            return true;
        }

        [HarmonyPostfix]
        public static void Postfix(ref Seaglide __instance)
        {
            var batt = __instance.energyMixin.battery;
            if (batt != null)
            {
                batt.charge = charge;
            }
        }
    }

}