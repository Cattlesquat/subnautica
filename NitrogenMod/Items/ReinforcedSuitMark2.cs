namespace NitrogenMod.Items
{
    using System.Collections.Generic;
    using SMLHelper.V2.Crafting;

    class ReinforcedSuitMark2 : ReinforcedSuitsCore
    {
        public ReinforcedSuitMark2()
            : base(classID: "reinforcedsuit2", friendlyName: "Reinforced Dive Suit Mark 2", description: "An upgraded dive suit capable of protecting the user past 1000m and up to 1300m.")
        {
            OnFinishedPatching += SetStaticTechType;
        }

        protected override TechType BaseType { get; } = TechType.ReinforcedDiveSuit;
        protected override EquipmentType DiveSuit { get; } = EquipmentType.Body;

        protected override TechData GetBlueprintRecipe()
        {
            return new TechData
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>(3)
                {
                    new Ingredient(TechType.ReinforcedDiveSuit, 1),
                    new Ingredient(TechType.AluminumOxide, 2),
                    new Ingredient(TechType.Nickel, 2),
                }
            };
        }

        private void SetStaticTechType() => ReinforcedSuit2ID = this.TechType;
    }
}
