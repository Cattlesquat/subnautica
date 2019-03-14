namespace EnzymeChargedBattery.Fabricator
{
    using System.Collections.Generic;
    using SMLHelper.V2.Crafting;

    internal class EnzymePowerCell : EnzymeBatteryCore
    {
        // Identical to any other power cell, made from 2 batts
        internal const int BattPerPC = 2;

        public EnzymePowerCell(EnzymeBattery enzBatt)
            : base(classID: "EnzymePowerCell", friendlyName: "Enzyme-Charged Ion Power Cell", description: "A new power cell based on the discovery of a chemical interaction between hatching enzymes, radiation, and ion crystals.")
        {
            // Soooooooooo in theory we should always have the EnzymeBattery unlocked, but let's make sure
            if (!enzBatt.IsPatched)
                enzBatt.Patch();

            // Make sure it's invoke
            OnFinishedPatching += SetStaticTechType;
        }

        protected override TechType BaseType { get; } = TechType.PrecursorIonPowerCell;
        protected override float PowerCapacity { get; } = EnzymeBattery.BattCap * BattPerPC;
        protected override EquipmentType ChargerType { get; } = EquipmentType.PowerCellCharger;

        protected override TechData GetBlueprintRecipe()
        {
            return new TechData
            {
                // Just like any other power cell
                craftAmount = 1,
                Ingredients = new List<Ingredient>(2)
                {
                    new Ingredient(BattID, BattPerPC),
                    new Ingredient(TechType.Silicone, 1),
                }
            };
        }

        private void SetStaticTechType() => PowCelID = this.TechType;
    }
}
