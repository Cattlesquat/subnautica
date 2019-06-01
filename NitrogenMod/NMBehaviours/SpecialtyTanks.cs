namespace NitrogenMod.NMBehaviours
{
    using UnityEngine;
    using Common;
    using Items;

    class SpecialtyTanks : MonoBehaviour
    {
        private const float SolarMaxDepth = 200f;

        private OxygenManager cachedOxygenManager;
        private DayNightCycle cachedDayNight;
        private WaterTemperatureSimulation cachedTemp;

        private void Awake()
        {
            cachedOxygenManager = Player.main.oxygenMgr;
            cachedDayNight = DayNightCycle.main;
            cachedTemp = WaterTemperatureSimulation.main;
            SeraLogger.Message(Main.modName, "SpecialtyTanks is Awake() and running!");
        }

        private void Update()
        {
            TechType tankSlot = Inventory.main.equipment.GetTechTypeInSlot("Tank");
            if (Player.main.IsSwimming() && GameModeUtils.RequiresOxygen())
            {
                float playerDepth = Ocean.main.GetDepthOf(Player.main.gameObject);
                if ((tankSlot == O2TanksCore.PhotosynthesisSmallID || tankSlot == O2TanksCore.PhotosynthesisTankID) && playerDepth < 200f)
                {
                    if (cachedDayNight == null) // Safety check
                    {
                        cachedDayNight = DayNightCycle.main;
                        return;
                    }
                    float lightScalar = cachedDayNight.GetLocalLightScalar();
                    if (lightScalar > 0.9f)
                        lightScalar = 0.9f;
                    float percentage = (200f - playerDepth) / 200f;
                    cachedOxygenManager.AddOxygen(Time.deltaTime * lightScalar * percentage);
                }

                if (tankSlot == O2TanksCore.ChemosynthesisTankID)
                {
                    if (cachedTemp == null) // Safety check
                    {
                        cachedTemp = WaterTemperatureSimulation.main;
                        return;
                    }
                    else
                    {
                        float waterTemp = cachedTemp.GetTemperature(Player.main.transform.position);
                        if (waterTemp > 30f)
                        {
                            float oxygenAdded = waterTemp *Time.deltaTime * .01f;
                            cachedOxygenManager.AddOxygen(oxygenAdded);
                        }
                    }
                }
            }
        }
    }
}
