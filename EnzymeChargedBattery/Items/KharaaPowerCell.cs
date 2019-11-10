namespace EnzymeChargedBattery.Items
{
    using System.Collections.Generic;
    using SMLHelper.V2.Crafting;

    internal class KharaaPowerCell : SeraphimBatteryCore
    {
        internal const int BattPerPC = 2;

        public KharaaPowerCell(KharaaBattery khaBatt)
            : base(classID: "BiochemPowerCell", friendlyName: "Biochemical Power Cell", description: "Based on the power units that supply nearly perpetual power to the Warpers. Does not receive power from their power source, however.")
        {

            if (!khaBatt.IsPatched)
                khaBatt.Patch();

            OnFinishedPatching += SetStaticTechType;
        }

        protected override TechType BaseType { get; } = TechType.PrecursorIonPowerCell;
        protected override float PowerCapacity { get; } = KharaaBattery.BattCap * BattPerPC;
        protected override EquipmentType ChargerType { get; } = EquipmentType.PowerCellCharger;

        protected override TechData GetBlueprintRecipe()
        {
            return new TechData
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>(2)
                {
                    new Ingredient(KharaaBattery.BattID, BattPerPC),
                    new Ingredient(TechType.Silicone, 1),
                }
            };
        }

        private void SetStaticTechType() => PowCelID = this.TechType;
        public override TechType RequiredForUnlock { get; } = TechType.Warper;
    }
}
