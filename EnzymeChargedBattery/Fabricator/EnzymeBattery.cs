namespace EnzymeChargedBattery.Fabricator
{
    using System.Collections.Generic;
    using SMLHelper.V2.Crafting;

    internal class EnzymeBattery : EnzymeBatteryCore
    {
        // This provides 2x the power of a Precursor Battery
        internal static float BattCap { get; private set; } = 1000f;

        public EnzymeBattery(float cap = 1000f)
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
                    new Ingredient(TechType.PrecursorIonBattery, 1), // We're supercharging a battery
                    new Ingredient(TechType.HatchingEnzymes, 2), // It's post-endgame, so need this
                    new Ingredient(TechType.Lead, 1), // Gotta have radiation shielding
                    new Ingredient (TechType.UraniniteCrystal, 2), // The magic of sci-fi game radiation
                }
            };
        }

        private void SetStaticTechType() => BattID = this.TechType;
    }
}
