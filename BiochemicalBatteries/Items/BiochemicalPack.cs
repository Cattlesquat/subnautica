namespace BiochemicalBatteries.Items
{
    using System.Reflection;
    using System.Collections.Generic;
    using System.IO;
    using CustomBatteries.API;
    using SMLHelper.V2.Utility;

    internal class BiochemicalPack : IModPluginPack
    {
        /*
         * This is where you load the file that will be the icon for your battery and power cell.
         * ImageUtils.LoadSpriteFromFile(String) is from SMLHelper and loads a png from a file as a Sprite
         */
        public Atlas.Sprite BatteryIcon { get; } = ImageUtils.LoadSpriteFromFile(IOUtilities.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Assets", "BiochemBattery.png"));
        public Atlas.Sprite PowerCellIcon { get; } = ImageUtils.LoadSpriteFromFile(IOUtilities.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Assets", "BiochemPowerCell.png"));
        // The name of your custom battery pack
        public string PluginPackName { get; } = "Biochemical Batteries";
        // The capacity of your battery - power cells are always double this
        public int BatteryCapacity { get; } = 2500;
        // How to unlock your batteries
        public TechType UnlocksWith { get; } = TechType.Warper;
        // The techtype for the battery
        public string BatteryID { get; } = "BiochemBattery";
        // The user-facing name for the battery
        public string BatteryName { get; } = "Biochemical Battery";
        // The in-game description for the battery
        public string BatteryFlavorText { get; } = "A usable version of a Warper's internal power cell.";
        // List of ingredients for the battery - if it uses an ingredient more than once, be sure to list it multiple times
        public IList<TechType> BatteryParts { get; } = new List<TechType> { BioPlasmaItems.BioPlasmaID, TechType.Magnetite, TechType.Magnetite };
        // The techtype for the power cell
        public string PowerCellID { get; } = "BiochemPowerCell";
        // The user-facing name for the power cell
        public string PowerCellName { get; } = "Biochemical Power Cell";
        // The in-game description for the power cell
        public string PowerCellFlavorText { get; } = "Wait. You managed to kill TWO warpers?!";
        // If you want the power cell to take more ingredients than 2x battery + silicone, list it here
        public IList<TechType> PowerCellAdditionalParts { get; } = new List<TechType>();
    }
}