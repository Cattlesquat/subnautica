namespace NitrogenMod.Items
{
    using System.Collections.Generic;
    using SMLHelper.V2.Crafting;

    class PhotosynthesisSmallTank : O2TanksCore
    {
        public PhotosynthesisSmallTank()
            : base(classID: "photosynthesissmalltank", friendlyName: "Small Photosynthesis Tank", description: "An O2 tank that houses microorganisms that produce oxygen in sunlight.")
        {
            OnFinishedPatching += SetStaticTechType;
        }

        protected override TechType BaseType { get; } = TechType.Tank;
        protected override EquipmentType SpecialtyO2Tank { get; } = EquipmentType.Tank;

        protected override TechData GetBlueprintRecipe()
        {
            return new TechData
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>(3)
                {
                    new Ingredient(TechType.Tank, 1),
                    new Ingredient(TechType.PurpleBrainCoralPiece, 1),
                    new Ingredient(TechType.Glass, 1),
                }
            };
        }

        private void SetStaticTechType() => PhotosynthesisSmallID = this.TechType;

        public override TechType RequiredForUnlock { get; } = TechType.Rebreather;
    }
}
