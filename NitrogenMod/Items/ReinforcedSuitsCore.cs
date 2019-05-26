namespace NitrogenMod.Items
{
    using SMLHelper.V2.Assets;
    using SMLHelper.V2.Handlers;
    using SMLHelper.V2.Utility;
    using UnityEngine;
    using Common;

    internal abstract class ReinforcedSuitsCore : Craftable
    {
        private const string craftTab = "ReinforcedSuits";
        private const string Assets = @"NitrogenMod/Assets";
        private static readonly string[] craftPath = new[] { craftTab };

        public static TechType ReinforcedSuit2ID { get; protected set; }
        public static TechType ReinforcedSuit3ID { get; protected set; }
        public static TechType ReinforcedStillSuit { get; protected set; }

        internal static void PatchSuits()
        {
            var tabIcon = ImageUtils.LoadSpriteFromFile(@"./Qmods/" + Assets + @"/SuitTabIcon.png");
            CraftTreeHandler.AddTabNode(CraftTree.Type.Workbench, craftTab, "Dive Suit Upgrades", tabIcon);
            SeraLogger.Message(Main.modName, "Creating new dive suit crafting tab");

            var DiveSuitMk2 = new ReinforcedSuitMark2();
            var DiveSuitMk3 = new ReinforcedSuitMark3();
            var StillSuitMk2 = new ReinforcedStillSuit();

            DiveSuitMk2.Patch();
            DiveSuitMk3.Patch();
            StillSuitMk2.Patch();

            CraftDataHandler.SetItemSize(ReinforcedSuit2ID, new Vector2int(2, 2));
            CraftDataHandler.SetItemSize(ReinforcedSuit3ID, new Vector2int(2, 2));
            CraftDataHandler.SetItemSize(ReinforcedStillSuit, new Vector2int(2, 2));
        }

        protected abstract TechType BaseType { get; }
        protected abstract EquipmentType DiveSuit { get; }

        protected ReinforcedSuitsCore(string classID, string friendlyName, string description) : base(classID, friendlyName, description)
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
            CraftDataHandler.SetEquipmentType(this.TechType, this.DiveSuit);
        }
    }
}
