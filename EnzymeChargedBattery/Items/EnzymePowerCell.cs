namespace EnzymeChargedBattery.Items
{
    using System.Collections.Generic;
    using SMLHelper.V2.Crafting;

    internal class EnzymePowerCell : SeraphimBatteryCore
    {
        internal const int BattPerPC = 2;

        public EnzymePowerCell(EnzymeBattery enzBatt)
            : base(classID: "EnzymePowerCell", friendlyName: "Enzyme-Charged Ion Power Cell", description: "A new power cell based on the discovery of a chemical interaction between hatching enzymes, radiation, and ion crystals.")
        {
            
            if (!enzBatt.IsPatched)
                enzBatt.Patch();

            OnFinishedPatching += SetStaticTechType;
        }

        protected override TechType BaseType { get; } = TechType.PrecursorIonPowerCell;
        protected override float PowerCapacity { get; } = EnzymeBattery.BattCap * BattPerPC;
        protected override EquipmentType ChargerType { get; } = EquipmentType.PowerCellCharger;

        protected override TechData GetBlueprintRecipe()
        {
            return new TechData
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>(2)
                {
                    new Ingredient(SeraphimBatteryCore.EnzBattID, BattPerPC),
                    new Ingredient(TechType.Silicone, 1),
                }
            };
        }

        private void SetStaticTechType() => EnzPowCelID = this.TechType;
    }
}
