namespace BiochemicalBatteries.Items
{
    using SMLHelper.V2.Assets;
    using SMLHelper.V2.Handlers;
    using UnityEngine;

    internal abstract class BioPlasmaItems : Spawnable
    {
        public static TechType BioPlasmaID { get; protected set; }

        internal static void PatchBioPlasmaItems()
        {
            var bioPlasma = new BioPlasmaCore();

            bioPlasma.Patch();

            CraftDataHandler.SetHarvestOutput(TechType.Warper, BioPlasmaID);
            CraftDataHandler.SetHarvestType(TechType.Warper, HarvestType.DamageDead);
        }

        protected abstract TechType BaseType { get; }

        protected BioPlasmaItems(string classID, string friendlyName, string description) : base(classID, friendlyName, description)
        {

        }

        public override GameObject GetGameObject()
        {
            GameObject prefab = CraftData.GetPrefabForTechType(this.BaseType);
            var obj = GameObject.Instantiate(prefab);

            return obj;
        }
    }

    class BioPlasmaCore : BioPlasmaItems
    {
        public BioPlasmaCore()
            : base(classID: "bioplasmacore", friendlyName: "Bioplasma Core", description: "The power core from a dead Warper.")
        {
            OnFinishedPatching += SetStaticTechType;
        }

        protected override TechType BaseType { get; } = TechType.PrecursorIonCrystal;

        public override string AssetsFolder { get; } = @"BiochemicalBatteries/Assets";

        private void SetStaticTechType() => BioPlasmaID = this.TechType;
    }
}
