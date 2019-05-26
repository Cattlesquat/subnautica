namespace NitrogenMod.Items
{
    using SMLHelper.V2.Assets;
    using SMLHelper.V2.Handlers;
    using SMLHelper.V2.Utility;
    using UnityEngine;
    using Common;

    internal abstract class O2TanksCore : Craftable
    {
        private const string craftTab = "SpecialtyTanks";
        private const string Assets = @"NitrogenMod/Assets";
        private static readonly string[] craftPath = new[] { craftTab };

        public static TechType PhotosynthesisSmallID { get; protected set; }
        public static TechType PhotosynthesisTankID { get; protected set; }
        public static TechType ChemosynthesisTankID { get; protected set; }

        internal static void PatchTanks()
        {
            var tabIcon = ImageUtils.LoadSpriteFromFile(@"./Qmods/" + Assets + @"/TankTabIcon.png");
            CraftTreeHandler.AddTabNode(CraftTree.Type.Workbench, craftTab, "Specialty O2 Tanks", tabIcon);
            SeraLogger.Message(Main.modName, "Creating new O2 tank crafting tab");

            var smallPhotoTank = new PhotosynthesisSmallTank();
            var photoTank = new PhotosynthesisTank();
            var chemoTank = new ChemosynthesisTank();

            smallPhotoTank.Patch();
            photoTank.Patch();
            chemoTank.Patch();

            CraftDataHandler.SetItemSize(PhotosynthesisSmallID, new Vector2int(2, 3));
            CraftDataHandler.SetItemSize(PhotosynthesisTankID, new Vector2int(2, 3));
            CraftDataHandler.SetItemSize(ChemosynthesisTankID, new Vector2int(2, 3));
        }

        protected abstract TechType BaseType { get; }
        protected abstract EquipmentType SpecialtyO2Tank { get; }

        protected O2TanksCore(string classID, string friendlyName, string description) : base(classID, friendlyName, description)
        {
            OnFinishedPatching += SetEquipmentType;
        }

        public override CraftTree.Type FabricatorType { get; } = CraftTree.Type.Workbench;
        public override TechGroup GroupForPDA { get; } = TechGroup.Workbench;
        public override TechCategory CategoryForPDA { get; } = TechCategory.Workbench;
        public override string AssetsFolder { get; } = Assets;
        public override string[] StepsToFabricatorTab { get; } = craftPath;
        public override TechType RequiredForUnlock { get; } = TechType.HatchingEnzymes;

        public override GameObject GetGameObject()
        {
            GameObject prefab = CraftData.GetPrefabForTechType(this.BaseType);
            var obj = GameObject.Instantiate(prefab);

            return obj;
        }

        private void SetEquipmentType()
        {
            CraftDataHandler.SetEquipmentType(this.TechType, this.SpecialtyO2Tank);
        }
    }
}