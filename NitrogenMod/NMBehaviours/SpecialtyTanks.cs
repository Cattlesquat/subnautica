namespace NitrogenMod.NMBehaviours
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using Common;
    using Items;

    class SpecialtyTanks : MonoBehaviour
    {
        private const float SolarMaxDepth = 200f;

        private float solarStored = 0f;

        private Oxygen cachedOxygen;
        private DayNightCycle cachedDayNight;
        private WaterTemperatureSimulation cachedTemp;

        private void Awake()
        {
            solarStored = 0f;
            cachedOxygen = Player.main.gameObject.GetComponent<Oxygen>();
            cachedDayNight = DayNightCycle.main;
            cachedTemp = WaterTemperatureSimulation.main;
            SeraLogger.Message(Main.modName, "SpecialtyTanks is Awake() and running!");
        }

        private void Start()
        {
            
        }

        private void Update()
        {
            TechType tankSlot = Inventory.main.equipment.GetTechTypeInSlot("Tank");

            if (tankSlot == O2TanksCore.PhotosynthesisSmallID || tankSlot == O2TanksCore.PhotosynthesisTankID)
            {
                if (cachedDayNight == null)
                    return;
                else
                {
                    if (solarStored <= 20f)
                    {
                        float lightScalar = cachedDayNight.GetLocalLightScalar();
                        if (lightScalar > 0.8f)
                            lightScalar = 0.8f;
                        if (Ocean.main.GetDepthOf(Player.main.gameObject) < SolarMaxDepth)
                            solarStored += (Time.deltaTime * lightScalar / Ocean.main.GetDepthOf(Player.main.gameObject));
                    }
                    else
                    {
                        solarStored = 0f;
                        ErrorMessage.AddMessage("solarStored has replenished O2!");
                        cachedOxygen.oxygenAvailable += 2f;
                    }
                }
            }

            if (tankSlot == O2TanksCore.ChemosynthesisTankID)
            {
                if (cachedTemp == null)
                    return;
                else
                {
                    float waterTemp = cachedTemp.GetTemperature(Player.main.transform.position);
                    if (waterTemp > 30f)
                    {

                    }
                }
            }
        }
    }
}
