namespace EnzymeChargedBattery.Items
{
    using System.Collections.Generic;
    using SMLHelper.V2.Crafting;

    internal class KharaaBattery : SeraphimBatteryCore
    {
        internal static float BattCap { get; private set; } = 2500f;

        public KharaaBattery(float cap = 2500f)
            : base(classID: "BiochemBattery", friendlyName: "Biochemical Battery", description: "Based on the power units that supply nearly perpetual power to the Warpers. Does not receive power from their power source, however.")
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
                Ingredients = new List<Ingredient>(2)
                {
                    new Ingredient(BioPlasmaItems.BioPlasmaID, 1),
                    new Ingredient(TechType.Magnetite, 2),
                }
            };
        }

        private void SetStaticTechType() => KhaBattID = this.TechType;
        public override TechType RequiredForUnlock { get; } = TechType.Warper;
    }
}
