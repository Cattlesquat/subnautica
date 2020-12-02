/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 * Adapted (w/ substantial changes) from libraryaddict's Radiation Challenge mod -- used w/ permission.
 */

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Power details: 
// - Bases use BaseRoot while the Cyclops is a "true" SubRoot. 
// - Bases have a special PowerRelay type (BasePowerRelay)

namespace DeathRun.Patchers
{
    public class PatchPower
    {
        private static UnityEngine.Transform GetTransform(IPowerInterface powerInterface)
        {
            if (powerInterface is BatterySource)
            {
                return ((BatterySource)powerInterface).transform;
            }
            else if (powerInterface is PowerSource)
            {
                return ((PowerSource)powerInterface).transform;
            }
            else if (powerInterface is PowerRelay)
            {
                return ((PowerRelay)powerInterface).transform;
            }

            return null;
        }

        private static void AddEnergy(ref float amount)
        {
            if (Config.DEATHRUN.Equals(DeathRun.config.powerCosts))
            {
                amount /= 3;
            }
            else if (Config.HARD.Equals(DeathRun.config.powerCosts))
            {
                amount /= 2;
            }
        }

        private static void ConsumeEnergy(IPowerInterface powerInterface, ref float amount)
        {
            if ((powerInterface is BasePowerRelay) ||
                (DeathRun.chargingSemaphore || DeathRun.craftingSemaphore || DeathRun.scannerSemaphore || DeathRun.filterSemaphore))
            {
                if (Config.DEATHRUN.Equals(DeathRun.config.powerCosts))
                {
                    amount *= 3;
                }
                else if (Config.HARD.Equals(DeathRun.config.powerCosts))
                {
                    amount *= 2;
                }
            }
        }

        [HarmonyPrefix]
        public static void ConsumeEnergyBase(ref IPowerInterface powerInterface, ref float amount)
        {
            ConsumeEnergy(powerInterface, ref amount);
        }

        [HarmonyPrefix]
        public static void AddEnergyBase(ref IPowerInterface powerInterface, ref float amount)
        {
            AddEnergy(ref amount);
        }

        [HarmonyPrefix]
        public static void ConsumeEnergyTool(ref float amount)
        {
            //ConsumeEnergy(Player.main.transform, ref amount);
        }

        [HarmonyPrefix]
        public static void AddEnergyTool(ref float amount)
        {
            //AddEnergy(Player.main.transform, ref amount);
        }

        [HarmonyPrefix]
        public static void ConsumeEnergyVehicle(Vehicle __instance, ref float amount)
        {
            ErrorMessage.AddMessage("ConsumeVehicle");
            //ConsumeEnergy(__instance.transform, ref amount);
        }

        [HarmonyPrefix]
        public static void AddEnergyVehicle(Vehicle __instance, ref float amount)
        {
            AddEnergy(ref amount);
        }



        [HarmonyPrefix]
        public static bool ConsumeEnergyFabricatorPrefix(PowerRelay powerRelay, ref float amount, ref bool __result)
        {
            DeathRun.craftingSemaphore = true; // Raises our crafting semaphore before consuming energy at a fabricator
            return true;
        }

        [HarmonyPostfix]
        public static void ConsumeEnergyFabricatorPostfix(PowerRelay powerRelay, ref float amount, ref bool __result)
        {
            DeathRun.craftingSemaphore = false; // Lowers our crafting semaphore after consuming energy at a fabricator
        }

        [HarmonyPrefix]
        public static bool ConsumeEnergyFiltrationPrefix()
        {
            DeathRun.filterSemaphore = true  ; // Raises our filter semaphore before consuming energy at a filtration machine
            return true;
        }

        [HarmonyPostfix]
        public static void ConsumeEnergyFiltrationPostfix()
        {
            DeathRun.filterSemaphore = false; // Lowers our filter semaphore after consuming energy at a filtration machine
        }

        [HarmonyPrefix]
        public static bool ConsumeEnergyScanningPrefix()
        {
            DeathRun.scannerSemaphore = true; // Raises our scanner semaphore before consuming energy at a scanning room
            return true;
        }

        [HarmonyPostfix]
        public static void ConsumeEnergyScanningPostfix()
        {
            DeathRun.scannerSemaphore = false; // Lowers our scanner semaphore after consuming energy at a scanning room
        }

        [HarmonyPrefix]
        public static bool ConsumeEnergyChargingPrefix()
        {
            DeathRun.chargingSemaphore = true; // Raises our charging semaphore before consuming energy at a charger
            return true;
        }

        [HarmonyPostfix]
        public static void ConsumeEnergyChargingPostfix()
        {
            DeathRun.chargingSemaphore = false; // Lowers our charging semaphore after consuming energy at a charger
        }
    }
}