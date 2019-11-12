namespace EnzymeChargedBattery.Items
{
    using System.Collections.Generic;
    using SMLHelper.V2.Crafting;

    internal class EnzymeBattery : SeraphimBatteryCore
    {
        internal static float BattCap { get; private set; } = 1000f;

        public EnzymeBattery()
            : base(classID: "EnzymeBattery", friendlyName: "Enzyme-Charged Ion Battery", description: "A new battery based on the discovery of a chemical interaction between hatching enzymes, radiation, and ion crystals.")
        {
            OnFinishedPatching += SetStaticTechType;
        }

        protected override TechType BaseType { get; } = TechType.PrecursorIonBattery;
        protected override float PowerCapacity => BattCap;
        protected override EquipmentType ChargerType { get; } = EquipmentType.BatteryCharger;

        protected override TechData GetBlueprintRecipe()
        {
            return new TechData
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>(4)
                {
                    new Ingredient(TechType.PrecursorIonBattery, 1),
                    new Ingredient(TechType.HatchingEnzymes, 2),
                    new Ingredient(TechType.Lead, 1),
                    new Ingredient (TechType.UraniniteCrystal, 2),
                }
            };
        }

        private void SetStaticTechType() => EnzBattID = this.TechType;
    }
}
