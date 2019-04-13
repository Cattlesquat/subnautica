namespace EnzymeChargedBattery.Items
{
    using SMLHelper.V2.Assets;
    using SMLHelper.V2.Handlers;
    using SMLHelper.V2.Utility;
    using UnityEngine;
    using Common;

    internal abstract class EnzymeBatteryCore : Craftable
    {
        private const string BatCraftTab = "BatteryPower";
        private const string ElecCraftTab = "Electronics";
        private const string ResCraftTab = "Resources";
        private const string Assets = @"EnzymeChargedBattery/Assets";
        private static readonly string[] CraftPath = new[] { ResCraftTab, ElecCraftTab, BatCraftTab };

        // Setting up stuff that's used at the class level
        public static TechType BattID { get; protected set; }
        public static TechType PowCelID { get; protected set; }

        internal static void PatchBatteries()
        {
            if (!TechTypeHandler.ModdedTechTypeExists("DeepPowerCell"))
            {
                var tabIcon = ImageUtils.LoadSpriteFromFile(@"./Qmods/" + Assets + @"/TabIcon.png");
                CraftTreeHandler.AddTabNode(CraftTree.Type.Fabricator, BatCraftTab, "Batteries and Power Cells", tabIcon, ResCraftTab, ElecCraftTab);
                SeraLogger.Message(Main.modName, "MidGameBatteries not installed, creating new crafting tab");

                CraftTreeHandler.RemoveNode(CraftTree.Type.Fabricator, ResCraftTab, ElecCraftTab, TechType.Battery.ToString());
                CraftTreeHandler.RemoveNode(CraftTree.Type.Fabricator, ResCraftTab, ElecCraftTab, TechType.PrecursorIonBattery.ToString());
                CraftTreeHandler.RemoveNode(CraftTree.Type.Fabricator, ResCraftTab, ElecCraftTab, TechType.PowerCell.ToString());
                CraftTreeHandler.RemoveNode(CraftTree.Type.Fabricator, ResCraftTab, ElecCraftTab, TechType.PrecursorIonPowerCell.ToString());

                CraftTreeHandler.AddCraftingNode(CraftTree.Type.Fabricator, TechType.Battery, CraftPath);
                CraftTreeHandler.AddCraftingNode(CraftTree.Type.Fabricator, TechType.PowerCell, CraftPath);
                CraftTreeHandler.AddCraftingNode(CraftTree.Type.Fabricator, TechType.PrecursorIonBattery, CraftPath);
                CraftTreeHandler.AddCraftingNode(CraftTree.Type.Fabricator, TechType.PrecursorIonPowerCell, CraftPath);
            }
            else
            {
                // Skip creating the tab
                SeraLogger.Message(Main.modName, "MidGameBatteries installed, adding to crafting tab");
            }
            var enzBatt = new EnzymeBattery(1000f);
            enzBatt.Patch();
            var enzPowerCell = new EnzymePowerCell(enzBatt);
            enzPowerCell.Patch();
        }

        protected abstract TechType BaseType { get; } // Let's borrow the precursor batteries
        protected abstract float PowerCapacity { get; }
        protected abstract EquipmentType ChargerType { get; } // Should ALWAYS be BatteryCharger or PowerCellCharger.

        // Stuff that exists at instances
        protected EnzymeBatteryCore(string classID, string friendlyName, string description)
            : base(classID, friendlyName, description)
        {
            // Once all Fabricator classes are done, invoke
            OnFinishedPatching += SetEquipmentType;
        }

        public override CraftTree.Type FabricatorType { get; } = CraftTree.Type.Fabricator;
        public override TechGroup GroupForPDA { get; } = TechGroup.Resources;
        public override TechCategory CategoryForPDA { get; } = TechCategory.Electronics;
        public override string AssetsFolder { get; } = Assets;
        public override string[] StepsToFabricatorTab { get; } = CraftPath;
        public override TechType RequiredForUnlock { get; } = TechType.HatchingEnzymes; // These will unlock once you've got hatching enzymes

        public override GameObject GetGameObject()
        {
            GameObject prefab = CraftData.GetPrefabForTechType(this.BaseType);
            var obj = GameObject.Instantiate(prefab);

            Battery battery = obj.GetComponent<Battery>();
            battery._capacity = this.PowerCapacity;
            battery.name = $"{this.ClassID}BatteryCell";

            return obj;
        }

        private void SetEquipmentType()
        {
            CraftDataHandler.SetEquipmentType(this.TechType, this.ChargerType);
        }
    }
}
