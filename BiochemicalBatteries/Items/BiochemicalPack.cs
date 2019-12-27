namespace BiochemicalBatteries.Items
{
    using System.Collections.Generic;
    using CustomBatteries.API;
    using SMLHelper.V2.Utility;

    internal class BiochemicalPack : IModPluginPack
    {
        public Atlas.Sprite BatteryIcon { get; } = ImageUtils.LoadSpriteFromFile("Assets/biochemicalbattery.png");
        public Atlas.Sprite PowerCellIcon { get; } = ImageUtils.LoadSpriteFromFile("Assets/biochemicalpowercell.png");
        public string PluginPackName { get; } = "Biochemical Batteries";
        public int BatteryCapacity { get; } = 2500;
        public TechType UnlocksWith { get; } = TechType.Warper;
        public string BatteryID { get; } = "BiochemBattery";
        public string BatteryName { get; } = "Biochemical Battery";
        public string BatteryFlavorText { get; } = "A usable version of a Warper's internal power cell.";
        public IList<TechType> BatteryParts { get; } = new List<TechType> { BioPlasmaItems.BioPlasmaID, TechType.Magnetite, TechType.Magnetite };
        public string PowerCellID { get; } = "BiochemPowerCell";
        public string PowerCellName { get; } = "Biochemical Power Cell";
        public string PowerCellFlavorText { get; } = "Wait. You managed to kill TWO warpers?!";
        public IList<TechType> PowerCellAdditionalParts { get; } = new List<TechType>();
    }
}
