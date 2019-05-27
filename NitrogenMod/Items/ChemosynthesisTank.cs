namespace NitrogenMod.Items
{
    using System.Collections.Generic;
    using SMLHelper.V2.Crafting;

    class ChemosynthesisTank : O2TanksCore
    {
        public ChemosynthesisTank()
            : base(classID: "chemosynthesistank", friendlyName: "Chemosynthesis Tank", description: "A lightweight O2 tank that houses microorganisms that produce oxygen under high temperatures.")
        {
            OnFinishedPatching += SetStaticTechType;
        }

        protected override TechType BaseType { get; } = TechType.PlasteelTank;
        protected override EquipmentType SpecialtyO2Tank { get; } = EquipmentType.Tank;

        protected override TechData GetBlueprintRecipe()
        {
            return new TechData
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>(3)
                {
                    new Ingredient(TechType.PlasteelTank, 1),
                    new Ingredient(DummySuitItems.ThermoBacteriaID, 4),
                    new Ingredient(TechType.Kyanite, 1),
                }
            };
        }

        private void SetStaticTechType() => ChemosynthesisTankID = this.TechType;
    }
}
