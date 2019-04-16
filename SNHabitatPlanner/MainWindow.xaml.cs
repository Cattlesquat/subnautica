namespace SNHabitatPlanner
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // mainGrid.MouseUp += new MouseButtonEventHandler(ExteriorGrowbedsPlus_MouseUp);
        }

        private int foundations = 0;

        private int iCompartments = 0;

        private int lCompartments = 0;

        private int tCompartments = 0;

        private int xCompartments = 0;

        private int glassICompartments = 0;

        private int glassLCompartments = 0;

        private int windows = 0;

        private int hatches = 0;

        private int verticalConnectors = 0;

        private int ladders = 0;

        private int reinforcements = 0;

        private int bulkheads = 0;

        private int baseRooms = 0;

        private int moonpools = 0;

        private int mapRooms = 0;

        private int observatorys = 0;

        private int nuclearReactors = 0;

        private int bioreactors = 0;

        private int alienContainments = 0;

        private int upgradeConsoles = 0;

        private int filtrationMachines = 0;

        private int solarPanels = 0;

        private int thermalPlants = 0;
        private void TPlantsMinus_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (thermalPlants > 0)
                thermalPlants--;
        }
        private void TPlantsPlus_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (thermalPlants < 100)
                thermalPlants++;
        }

        private int fabricators = 0;
        private void FabricatorsMinus_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (fabricators > 0)
                fabricators--;
        }
        private void FabricatorsPlus_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (fabricators < 100)
                fabricators++;
        }

        private int powerTransmitters = 0;
        private void PTransmittersMinus_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (powerTransmitters > 0)
                powerTransmitters--;
        }
        private void PTransmittersPlus_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (powerTransmitters < 100)
                powerTransmitters++;
        }

        private int radios = 0;
        private void RadiosMinus_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (radios > 0)
                radios--;
        }
        private void RadiosPlus_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (radios < 100)
                radios++;
        }

        private int medicalCabinets = 0;
        private void MCabinetsMinus_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (medicalCabinets > 0)
                medicalCabinets--;
        }
        private void MCabinetsPlus_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (medicalCabinets < 100)
                medicalCabinets++;
        }

        private int batteryChargers = 0;
        private void BChargersMinus_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (batteryChargers > 0)
                batteryChargers--;
        }
        private void BChargersPlus_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (batteryChargers < 100)
                batteryChargers++;
        }

        private int powerCellChargers = 0;
        private void PCChargersMinus_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (powerCellChargers > 0)
                powerCellChargers--;
        }
        private void PCChargersPlus_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (powerCellChargers < 100)
                powerCellChargers++;
        }

        private int lockers = 0;
        private void LockersMinus_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (lockers > 0)
                lockers--;
        }
        private void LockersPlus_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (lockers < 100)
                lockers++;
        }

        private int wallLockers = 0;
        private void WLockersMinus_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (wallLockers > 0)
                wallLockers--;
        }
        private void WLockersPlus_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (wallLockers < 100)
                wallLockers++;
        }

        private int spotlights = 0;
        private void SLightsMinus_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (spotlights > 0)
                spotlights--;
        }
        private void SLightPlus_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (spotlights < 100)
            {
                spotlights++;
            }
        }

        private int floodlights = 0;
        private void FLightsMinus_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (floodlights > 0)
                floodlights--;
        }
        private void FLightsPlus_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (floodlights < 100)
                floodlights++;
        }

        private int pots = 0;
        private void PotsMinus_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (pots > 0)
                pots--;
        }
        private void PotsPlus_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (pots < 100)
                pots++;
        }

        private int interiorGrowbeds = 0;
        private void IGrowbedsMinus_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (interiorGrowbeds > 0)
                interiorGrowbeds--;
        }
        private void IGrowbedsPlus_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (interiorGrowbeds < 100)
                interiorGrowbeds++;
        }

        private int exteriorGrowbeds = 0;
        private void EGrowbedsMinus_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (exteriorGrowbeds > 0)
                exteriorGrowbeds--;
        }
        private void EGrowbedsPlus_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (exteriorGrowbeds < 100)
                exteriorGrowbeds++;
        }
    }
}
