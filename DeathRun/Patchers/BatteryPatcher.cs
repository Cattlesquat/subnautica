/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 * Models (aka mostly copies) PrimeSonic's pre-Custom-Batteries code for Midgame Batteries. 
 * 
 * I've adapted it to allow non-recharging Copper batteries, and for the capacity to depend on difficulty settings. And my own recipes of course.
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
    [HarmonyPatch(typeof(EnergyMixin))] 
    [HarmonyPatch(nameof(EnergyMixin.Start))]
    internal class EnergyMixin_Initialize_Patcher
    {
        [HarmonyPostfix] 
        public static void Postfix(ref EnergyMixin __instance)
        {
            // This is necessary to allow the new batteries to be compatible with tools and vehicles

            if (!__instance.allowBatteryReplacement)
                return; // Battery replacement not allowed - No need to make changes

            List<TechType> compatibleBatteries = __instance.compatibleBatteries;

            if (compatibleBatteries.Contains(TechType.Battery) &&
                !compatibleBatteries.Contains(AcidBatteryCellBase.BatteryID))
            {
                // If the regular Battery is compatible with this item,
                // the Deep Lithium Battery should also be compatible
                compatibleBatteries.Add(AcidBatteryCellBase.BatteryID);
                return;
            }

            if (compatibleBatteries.Contains(TechType.PowerCell) &&
                !compatibleBatteries.Contains(AcidBatteryCellBase.PowerCellID))
            {
                // If the regular Power Cell is compatible with this item,
                // the Deep Lithium Power Cell should also be compatible
                compatibleBatteries.Add(AcidBatteryCellBase.PowerCellID);
                return;
            }
        }
    }

    [HarmonyPatch(typeof(EnergyMixin))] 
    [HarmonyPatch(nameof(EnergyMixin.NotifyHasBattery))] 
    internal class EnergyMixin_NotifyHasBattery_Patcher
    {
        [HarmonyPostfix] 
        public static void Postfix(ref EnergyMixin __instance, InventoryItem item)
        {
            // For vehicles that show a battery model when one is equipped,
            // this will replicate the model for the normal Power Cell so it doesn't look empty

            if (item?.item?.GetTechType() == AcidBatteryCellBase.PowerCellID)
                __instance.batteryModels[0].model.SetActive(true);
        }
    }


    [HarmonyPatch(typeof(BatteryCharger))] 
    [HarmonyPatch(nameof(BatteryCharger.Initialize))]
    internal class BatteryCharger_Patcher
    {
        [HarmonyPrefix] 
        public static void Prefix(ref BatteryCharger __instance)
        {
            HashSet<TechType> compatibleTech = BatteryCharger.compatibleTech;

            // Make sure the Acid Battery is NOT allowed in the battery charger
            if (compatibleTech.Contains(AcidBatteryCellBase.BatteryID))
                compatibleTech.Remove(AcidBatteryCellBase.BatteryID);

        }
    }

    [HarmonyPatch(typeof(PowerCellCharger))] 
    [HarmonyPatch(nameof(PowerCellCharger.Initialize))]
    internal class PowerCellCharger_Patcher
    {
        [HarmonyPrefix] 
        public static void Prefix(ref PowerCellCharger __instance)
        {
            HashSet<TechType> compatibleTech = PowerCellCharger.compatibleTech;

            // Make sure the Acid Power Cell is allowed in the power cell charger
            if (!compatibleTech.Contains(AcidBatteryCellBase.PowerCellID))
                compatibleTech.Add(AcidBatteryCellBase.PowerCellID);
        }
    }


    internal abstract class AcidBatteryCellBase : Craftable
    {
        private const string BatteryPowerCraftingTab = "BatteryPower";
        private const string BasicMaterialsCraftingTab = "BasicMaterials";
        private const string ElectronicsCraftingTab = "Electronics";
        private const string ResourcesCraftingTab = "Resources";
        private const string MgBatteryAssets = @"DeathRun\Assets";

        private static readonly string[] PathToNewTab = new[] { ResourcesCraftingTab, ElectronicsCraftingTab, BatteryPowerCraftingTab };
        private static readonly string[] PathToBasicTab = new[] { ResourcesCraftingTab, BasicMaterialsCraftingTab };

        // Class level elements

        public static TechType BatteryID { get; protected set; }
        public static TechType PowerCellID { get; protected set; }

        internal static void PatchAll()
        {
            if (Config.NORMAL.Equals(DeathRunPlugin.config.batteryCosts))
            {
                // If we're leaving normal batteries alone, don't patch in our alternates.
                return;
            }

            string mainDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // Create a new crafting tree tab for batteries and power cells
            string assetsFolder = Path.Combine(mainDirectory, "Assets");
            string pathToIcon = Path.Combine(assetsFolder, @"CraftingTabIcon.png");

            Atlas.Sprite tabIcon = ImageUtils.LoadSpriteFromFile(pathToIcon);
            CraftTreeHandler.AddTabNode(CraftTree.Type.Fabricator, BatteryPowerCraftingTab, "Batteries and Power Cells", tabIcon, ResourcesCraftingTab, ElectronicsCraftingTab);

            // Remove the original batteries from the Electronics tab
            CraftTreeHandler.RemoveNode(CraftTree.Type.Fabricator, ResourcesCraftingTab, ElectronicsCraftingTab, TechType.Battery.ToString());
            CraftTreeHandler.RemoveNode(CraftTree.Type.Fabricator, ResourcesCraftingTab, ElectronicsCraftingTab, TechType.PowerCell.ToString());
            CraftTreeHandler.RemoveNode(CraftTree.Type.Fabricator, ResourcesCraftingTab, ElectronicsCraftingTab, TechType.PrecursorIonBattery.ToString());
            CraftTreeHandler.RemoveNode(CraftTree.Type.Fabricator, ResourcesCraftingTab, ElectronicsCraftingTab, TechType.PrecursorIonPowerCell.ToString());

            //var config = new DeepConfig();
            //config.ReadConfigFile(mainDirectory);
            //QuickLogger.Info($"Selected PowerStyle in config: {config.SelectedPowerStyle} - (Battery Capacity:{Mathf.RoundToInt(config.BatteryCapacity)})");

            CattleLogger.Message("Patching AcidBattery");

            int battSize;
            int powerSize;
            if (Config.EXORBITANT.Equals(DeathRunPlugin.config.batteryCosts))
            {
                battSize = 25;
                powerSize = 75;
            }
            else if (Config.DEATHRUN.Equals(DeathRunPlugin.config.batteryCosts))
            {
                battSize = 50;
                powerSize = 125;
            }
            else if (Config.HARD.Equals(DeathRunPlugin.config.batteryCosts))
            {
                battSize = 75;
                powerSize = 150;
            } else
            {
                battSize = 100;
                powerSize = 200;
            }

            var acidBattery = new AcidBattery(battSize);
            acidBattery.Patch();

            CattleLogger.Message("Patching AcidPowerCell");
            var lithiumPowerCell = new AcidPowerCell(acidBattery, powerSize);
            lithiumPowerCell.Patch();

            // And "regular" batteries (now Lithium) back in on the new Batteries and PowerCells tab
            CraftTreeHandler.AddCraftingNode(CraftTree.Type.Fabricator, TechType.Battery, PathToNewTab);
            CraftTreeHandler.AddCraftingNode(CraftTree.Type.Fabricator, TechType.PowerCell, PathToNewTab);

            // Add the Ion Batteries after the Lithium Batteries
            CraftTreeHandler.AddCraftingNode(CraftTree.Type.Fabricator, TechType.PrecursorIonBattery, PathToNewTab);
            CraftTreeHandler.AddCraftingNode(CraftTree.Type.Fabricator, TechType.PrecursorIonPowerCell, PathToNewTab);

            CattleLogger.Message("Patching Copper Recycling");
            // Add recycling of batteries for copper
            CraftTreeHandler.AddCraftingNode(CraftTree.Type.Fabricator, TechType.Copper, PathToBasicTab);
            CattleLogger.Message("Patching Copper Recycling Done");
        }

        protected abstract TechType BaseType { get; } // Should only ever be Battery or PowerCell
        protected abstract float PowerCapacity { get; }
        protected abstract EquipmentType ChargerType { get; } // Should only ever be BatteryCharger or PowerCellCharger

        // Instance level elements

        protected AcidBatteryCellBase(string classId, string friendlyName, string description)
            : base(classId, friendlyName, description)
        {
            // This event will be invoked after all patching done by the Craftable class is complete
            OnFinishedPatching += SetEquipmentType;
        }

        public override CraftTree.Type FabricatorType { get; } = CraftTree.Type.Fabricator;
        public override TechGroup GroupForPDA { get; } = TechGroup.Resources;
        public override TechCategory CategoryForPDA { get; } = TechCategory.Electronics;
        public override string AssetsFolder { get; } = MgBatteryAssets;
        public override string[] StepsToFabricatorTab { get; } = PathToNewTab;
        public override TechType RequiredForUnlock { get; } = TechType.AcidMushroom; // These will unlock once the player acquires an Acid Mushroom

        public override GameObject GetGameObject()
        {
            GameObject prefab = CraftData.GetPrefabForTechTypeAsync(this.BaseType).GetResult();
            var obj = GameObject.Instantiate(prefab);

            Battery battery = obj.GetComponent<Battery>();
            battery._capacity = this.PowerCapacity;
            battery.name = $"{this.ClassID}BatteryCell";

            return obj;
        }

        private void SetEquipmentType()
        {
            // This is necessary to allow the new battery and power cell to be added to their respective charging stations
            CraftDataHandler.SetEquipmentType(this.TechType, this.ChargerType);
        }
    }


    internal class AcidBattery : AcidBatteryCellBase
    {
        // This battery provides 2.5x the power of a normal battery
        internal static float BatteryCapacity { get; private set; } = 50f;

        public AcidBattery(float capacity = 50f)
            : base(classId: "AcidBattery",
                   friendlyName: "Copper/Zinc Battery",
                   description: "A very basic mobile power source, and NOT rechargeable. Please dispose of safely.")
        {
            BatteryCapacity = capacity;
            // This event will be invoked after all patching done by the Craftable class is complete
            OnFinishedPatching += SetStaticTechType;
        }

        protected override TechType BaseType { get; } = TechType.LithiumIonBattery;
        protected override float PowerCapacity => BatteryCapacity;
        protected override EquipmentType ChargerType { get; } = EquipmentType.None; //EquipmentType.BatteryCharger;

        protected override TechData GetBlueprintRecipe()
        {
            return new TechData
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>(4)
                {
                    new Ingredient(TechType.Copper, 1),
                    new Ingredient(TechType.AcidMushroom, 2)
                }
            };
        }

        private void SetStaticTechType()
        {
            BatteryID = this.TechType;
        }
    }


    internal class AcidPowerCell : AcidBatteryCellBase
    {
        internal static float PowerCellCapacity { get; private set; } = 150f;

        public AcidPowerCell(AcidBattery acidBattery, float capacity = 150f)
            : base(classId: "AcidPowerCell",
                   friendlyName: "Lead Acid Power Cell",
                   description: "A basic lead/acid vehicle power source - not super powerful, but it IS rechargeable. Keep fully charged during winter months!")
        {
            PowerCellCapacity = capacity;

            // Because we're dependent on battery regarding blueprint,
            // we'll go ahead and add this little safety check here
            if (!acidBattery.IsPatched)
                acidBattery.Patch();

            // This event will be invoked after all patching done by the Craftable class is complete
            OnFinishedPatching += SetStaticTechType;
        }

        protected override TechType BaseType { get; } = TechType.PowerCell;
        protected override float PowerCapacity => PowerCellCapacity;
        protected override EquipmentType ChargerType { get; } = EquipmentType.PowerCellCharger;

        protected override TechData GetBlueprintRecipe()
        {
            return new TechData
            {                
                craftAmount = 1,
                Ingredients = new List<Ingredient>(2)
                {
                    new Ingredient(TechType.Lead, 2),
                    new Ingredient(TechType.AcidMushroom, 4),
                    new Ingredient(TechType.Silicone, 1),
                }
            };
        }

        private void SetStaticTechType()
        {
            PowerCellID = this.TechType;
        }
    }

    /**
     * Normally Subnautica doesn't display tooltip text for batteries in the inventory. But since we have introduced more battery differences, this
     * re-adds the tooltip text.
     */
    [HarmonyPatch(typeof(TooltipFactory))]
    [HarmonyPatch("ItemCommons")]
    internal class TooltipFactoryPatcher
    {
        [HarmonyPostfix]
        private static void ItemCommonsPrefix(StringBuilder sb, TechType techType, GameObject obj)
        {
            IBattery component4 = obj.GetComponent<IBattery>();
            if (component4 != null)
            {
                TooltipFactory.WriteDescription(sb, Language.main.Get(TooltipFactory.techTypeTooltipStrings.Get(techType)));
            }
        }
    }

    /**
     * This stops new tools and vehicles from being spawned with batteries and power cells
     */
    [HarmonyPatch(typeof(EnergyMixin))]
    [HarmonyPatch("OnCraftEnd")]
    internal class EnergyMixin_OnCraftEnd_Patcher
    {
        [HarmonyPrefix]
        private static void OnCraftEndPrefix(EnergyMixin __instance, TechType techType)
        {
            if (!Config.NORMAL.Equals(DeathRunPlugin.config.batteryCosts) && !GameModeUtils.IsOptionActive(GameModeOption.Creative))
            {
                if (techType != TechType.MapRoomCamera)
                {
                    __instance.defaultBattery = 0;
                }
            }
        }
    }
}