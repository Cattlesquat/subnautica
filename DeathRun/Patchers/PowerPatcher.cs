/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 * Adapted (w/ substantial changes) from libraryaddict's Radiation Challenge mod -- used w/ permission.
 * 
 * General ideas:
 * * Crafting/charging/scanning/filtering is expensive everywhere (very expensive in radiation)
 * * Power consumption in general is very expensive in radiation, but apart from above tools/vehicles function normally outside radiation.
 * * Gaining/Regaining energy goes slowly in general (very slowly in radiation)
 * 
 * I had to reorganize a bunch of stuff in order to make the new features possible/flexible.
 */

using Common;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

// Power details: 
// - Bases use BaseRoot while the Cyclops is a "true" SubRoot. 
// - Bases have a special PowerRelay type (BasePowerRelay)

namespace DeathRun.Patchers
{
    public class PowerPatcher
    {
        /**
         * Gets the transform/location of a power interface.
         */
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

        /**
         * @return true if Transform is currently in radiation
         */
        private static bool isTransformInRadiation(Transform transform)
        {
            if (transform == null) return false;
            return RadiationUtils.isInAnyRadiation(transform);
        }

        /**
         * @return true if the power interface is currently in radiation
         */
        private static bool isPowerInRadiation(IPowerInterface powerInterface)
        {
            return isTransformInRadiation(GetTransform(powerInterface));
        }

        /**
         * AdjustAddEnergy - when gaining energy from e.g. solar panel, thermal power station, etc.
         * @return adjusted value of energy to Add back into power grid, based on radiation and difficulty settings
         */
        private static float AdjustAddEnergy(float amount, bool radiation)
        {
            if (radiation)
            {
                if (Config.DEATHRUN.Equals(DeathRunPlugin.config.powerCosts) || Config.EXORBITANT.Equals(DeathRunPlugin.config.powerCosts))
                {
                    amount /= 10;
                }
                else if (Config.HARD.Equals(DeathRunPlugin.config.powerCosts))
                {
                    amount /= 6;
                } 
            }
            else if (Config.DEATHRUN.Equals(DeathRunPlugin.config.powerCosts) || Config.EXORBITANT.Equals(DeathRunPlugin.config.powerCosts))
            {
                amount /= 6;
            }
            else if (Config.HARD.Equals(DeathRunPlugin.config.powerCosts))
            {
                amount /= 3;
            }

            return amount;
        }

        /**
         * AdjustAddConsuming - when spending energy to do anything
         * @return adjusted amount of energy to consume, based on radiation, type-of-use, and difficulty settings
         */
        private static float AdjustConsumeEnergy(float amount, bool radiation, bool isBase)
        {
            if (radiation)
            {
                if (Config.DEATHRUN.Equals(DeathRunPlugin.config.powerCosts) || Config.EXORBITANT.Equals(DeathRunPlugin.config.powerCosts))
                {
                    amount *= 5;
                }
                else if (Config.HARD.Equals(DeathRunPlugin.config.powerCosts))
                {
                    amount *= 3;
                }
            }
            else if ((DeathRunPlugin.chargingSemaphore || DeathRunPlugin.craftingSemaphore || DeathRunPlugin.scannerSemaphore /* || DeathRun.filterSemaphore */))
            {
                if (Config.DEATHRUN.Equals(DeathRunPlugin.config.powerCosts) || Config.EXORBITANT.Equals(DeathRunPlugin.config.powerCosts))
                {
                    amount *= 3;
                }
                else if (Config.HARD.Equals(DeathRunPlugin.config.powerCosts))
                {
                    amount *= 2;
                }
            }

            //if (DeathRun.filterSemaphore)
            //{
            //    ErrorMessage.AddMessage("Filtering " + amount + "  isBase=" + isBase + "   charging=" + DeathRun.chargingSemaphore + "  Crafting=" + DeathRun.craftingSemaphore + "   Scanner="+DeathRun.scannerSemaphore);
            //}

            return amount;
        }

        /**
         * ConsumeEnergyBase -- adjusts the amount of power spent at a base
         * @return true if there was enough energy to perform the action
         */
        [HarmonyPrefix]
        public static bool ConsumeEnergyBase(ref IPowerInterface powerInterface, ref float amount, bool __result)
        {
            amount = AdjustConsumeEnergy(amount, isPowerInRadiation(powerInterface), powerInterface is BasePowerRelay);

            // In vanilla if you try to use 5 power from your Fabricator but you only have 4 power, then you not only
            // fail but also lose your 4 power. That was already a little bit irritating, but it becomes grotesque and
            // feels unfair when power requirements are e.g. 15. This next block prevents the not-actually-enough power
            // from being lost, merely doesn't produce the item.
            if (DeathRunPlugin.craftingSemaphore && (powerInterface.GetPower() < amount))
            {
                ErrorMessage.AddMessage("Not Enough Power"); 
                __result = false;
                return false;
            } 

            return true;
        }


        /**
         * SolarPanelUpdate - solar panels have a completely different way of adding power
         */
        [HarmonyPrefix]
        public static bool SolarPanelUpdate(SolarPanel __instance)
        {
            if (__instance.gameObject.GetComponent<Constructable>().constructed)
            {
                float amount = __instance.GetRechargeScalar() * DayNightCycle.main.deltaTime * 0.25f * 5f;
                amount = AdjustAddEnergy(amount, isTransformInRadiation(__instance.gameObject.transform));
                __instance.powerSource.power = Mathf.Clamp(__instance.powerSource.power + amount, 0f, __instance.powerSource.maxPower);
            }
            return false;
        }


        /**
         * AddEnergyBase -- adjusts the amount of power added to a base
         */
        [HarmonyPrefix]
        public static bool AddEnergyBase(ref IPowerInterface powerInterface, ref float amount)
        {
            amount = AdjustAddEnergy(amount, isPowerInRadiation(powerInterface));
            return true;
        }

        /**
         * ConsumeEnergyTool - adjust the amount of power consumed by a handheld tool
         */
        [HarmonyPrefix]
        public static void ConsumeEnergyTool(ref float amount)
        {
            amount = AdjustConsumeEnergy(amount, isTransformInRadiation(Player.main.transform), false);
        }

        /**
         * AddEnergyTool - adjust the amount of power added to a handheld tool
         */
        [HarmonyPrefix]
        public static void AddEnergyTool(EnergyMixin __instance, ref float amount)
        {
            // Acid Battery is not chargeable by any method (e.g. Swim Charge Fins)
            /* if (!Config.NORMAL.Equals(DeathRunPlugin.config.batteryCosts))
            {
                var batt = __instance.GetBattery();
                if (batt != null) 
                {
                    TechType t = CraftData.GetTechType(batt);
                    if (t == AcidBatteryCellBase.BatteryID) 
                    {
                        amount = 0;
                        return;
                    }
                }
            } */

            if (isTransformInRadiation(Player.main.transform))
            {
                if (Config.DEATHRUN.Equals(DeathRunPlugin.config.powerCosts) || Config.EXORBITANT.Equals(DeathRunPlugin.config.powerCosts))
                {
                    amount /= 4;
                }
                else if (Config.HARD.Equals(DeathRunPlugin.config.powerCosts))
                {
                    amount /= 2;
                }

            }
            else if (Config.DEATHRUN.Equals(DeathRunPlugin.config.powerCosts) || Config.EXORBITANT.Equals(DeathRunPlugin.config.powerCosts))
            {
                amount /= 2;
            }
        }

        /**
         * ConsumeEnergyVehicle - adjust the amount of power consumed by a vehicle
         */
        [HarmonyPrefix]
        public static void ConsumeEnergyVehicle(Vehicle __instance, ref float amount)
        {            
            amount = AdjustConsumeEnergy(amount, isTransformInRadiation(__instance.transform), false);
        }

        /**
         * AddEnergyVehicle - adjust the amount of power added to a vehicle
         */
        [HarmonyPrefix]
        public static void AddEnergyVehicle(Vehicle __instance, ref float amount)
        {
            amount = AdjustAddEnergy(amount, isTransformInRadiation(__instance.transform));
        }



        [HarmonyPrefix]
        public static bool ConsumeEnergyFabricatorPrefix(PowerRelay powerRelay, ref float amount, ref bool __result)
        {
            DeathRunPlugin.craftingSemaphore = true; // Raises our crafting semaphore before consuming energy at a fabricator
            return true;
        }

        [HarmonyPostfix]
        public static void ConsumeEnergyFabricatorPostfix(PowerRelay powerRelay, ref float amount, ref bool __result)
        {
            DeathRunPlugin.craftingSemaphore = false; // Lowers our crafting semaphore after consuming energy at a fabricator
        }

        [HarmonyPrefix]
        public static bool ConsumeEnergyFiltrationPrefix()
        {
            DeathRunPlugin.filterSemaphore = true  ; // Raises our filter semaphore before consuming energy at a filtration machine
            return true;
        }

        [HarmonyPostfix]
        public static void ConsumeEnergyFiltrationPostfix()
        {
            DeathRunPlugin.filterSemaphore = false; // Lowers our filter semaphore after consuming energy at a filtration machine
        }

        [HarmonyPrefix]
        public static bool ConsumeEnergyScanningPrefix()
        {
            DeathRunPlugin.scannerSemaphore = true; // Raises our scanner semaphore before consuming energy at a scanning room
            return true;
        }

        [HarmonyPostfix]
        public static void ConsumeEnergyScanningPostfix()
        {
            DeathRunPlugin.scannerSemaphore = false; // Lowers our scanner semaphore after consuming energy at a scanning room
        }

        [HarmonyPrefix]
        public static bool ConsumeEnergyChargingPrefix()
        {
            DeathRunPlugin.chargingSemaphore = true; // Raises our charging semaphore before consuming energy at a charger
            return true;
        }

        [HarmonyPostfix]
        public static void ConsumeEnergyChargingPostfix()
        {
            DeathRunPlugin.chargingSemaphore = false; // Lowers our charging semaphore after consuming energy at a charger
        }


        /**
         * The regenerating "Solar Cells" in the Escape Pod use an irritating infinite loop IEnumerator, so this is a
         * "pass-through postfix" to double the rate of energy recharge once the secondary systems are repaired.
         * 
         * ... Did this a different way (changed the regenerationThreshold at the appropriate time)
         */
        //[HarmonyPostfix]
        //public static void RegeneratePowerStart(RegeneratePowerSource __instance)
        //{
        //    if (!EscapePod.main.damageEffectsShowing)
        //    {
        //        if (__instance.powerSource.GetPower() < __instance.regenerationThreshhold)
        //        {
        //            __instance.powerSource.SetPower(Mathf.Min(__instance.regenerationThreshhold, __instance.powerSource.GetPower() + __instance.regenerationAmount));
        //        }
        //    }
        //}
    }
}