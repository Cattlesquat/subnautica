namespace BiomeHUDIndicator.Items
{
    using System.Collections.Generic;
    using SMLHelper.V2.Crafting;

    class BiomeHUDIndicator : CompassCore
    {
        public BiomeHUDIndicator()
            : base(classID: "BiomeHUDIndicator", friendlyName: "Biome Indicator Compass", description: "An upgraded compass with uplinks to your PDA to display which biome you are currently in.")
        {
            OnFinishedPatching += SetStaticTechType;
        }

        protected override TechType BaseType { get; } = TechType.Compass;
        protected override EquipmentType Chip { get; } = EquipmentType.Chip;

        protected override TechData GetBlueprintRecipe()
        {
            return new TechData
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>(3)
                {
                    new Ingredient(TechType.Compass, 1),
                    new Ingredient(TechType.ComputerChip, 1),
                    new Ingredient(TechType.Magnetite, 1),
                }
            };
        }

        private void SetStaticTechType() => BiomeChipID = this.TechType;
    }
}
