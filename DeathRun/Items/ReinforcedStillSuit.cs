/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 * This section taken directly from Seraphim Risen's NitrogenMod
 */
namespace DeathRun.Items
{
    using System.Collections.Generic;
    using SMLHelper.V2.Crafting;

    class ReinforcedStillSuit : ReinforcedSuitsCore
    {
        public ReinforcedStillSuit()
            : base(classID: "reinforcedstillsuit", friendlyName: "Reinforced Still Suit", description: "An upgraded still suit capable of protecting the user at depths up to 1300m and temperatures up to 70C.")
        {
            OnFinishedPatching += SetStaticTechType;
        }

        protected override TechType BaseType { get; } = TechType.WaterFiltrationSuit;
        protected override EquipmentType DiveSuit { get; } = EquipmentType.Body;

        protected override TechData GetBlueprintRecipe()
        {
            return new TechData
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>(3)
                {
                    new Ingredient(TechType.WaterFiltrationSuit, 1),
                    new Ingredient(DummySuitItems.RiverEelScaleID, 2),
                    new Ingredient(TechType.AramidFibers, 2),
                }
            };
        }

        private void SetStaticTechType() => ReinforcedStillSuit = this.TechType;
    }
}
