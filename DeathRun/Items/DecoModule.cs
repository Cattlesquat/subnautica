/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 * A vehicle upgrade that reduces power-consumption-on-exit and nitrogen issues.
 */

using System.Collections.Generic;
using Common;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using UnityEngine;
using UWE;

namespace DeathRun.Items
{
    public class DecoModule : Equipable
    {
        public DecoModule() : base(
            "DecoModule",
            "Nano Decompression Module",
            "Eliminates nitrogen from the bloodstream of vehicle pilot. Reduces energy expended when exiting the vehicle. Stacking multiple modules increases the benefit.")
        {
        }

        public override EquipmentType EquipmentType => EquipmentType.VehicleModule;

        public override TechType RequiredForUnlock => TechType.Cyclops; 

        public override TechGroup GroupForPDA => TechGroup.VehicleUpgrades;

        public override TechCategory CategoryForPDA => TechCategory.VehicleUpgrades;

        public override CraftTree.Type FabricatorType => CraftTree.Type.SeamothUpgrades;

        public override string DiscoverMessage => $"{this.FriendlyName} Unlocked!";
        //public override bool AddScannerEntry => true;

        public override string[] StepsToFabricatorTab => new string[] { "CommonModules" };

        public override QuickSlotType QuickSlotType => QuickSlotType.Passive;

        public override GameObject GetGameObject()
        {
            string classid = CraftData.GetClassIdForTechType(TechType.VehicleArmorPlating);
            if (PrefabDatabase.TryGetPrefabFilename(classid, out string filename))
            {
                var prefab = Resources.Load<GameObject>(filename);
                var obj = GameObject.Instantiate(prefab);

                // Get the TechTags and PrefabIdentifiers
                var techTag = obj.EnsureComponent<TechTag>();
                var prefabIdentifier = obj.GetComponent<PrefabIdentifier>();

                // Change them so they fit to our requirements.
                techTag.type = TechType;
                prefabIdentifier.ClassId = ClassID;

                return obj;
            }
            return null;

            /*
            // Get the ElectricalDefense module prefab and instantiate it
            var path = "WorldEntities/Tools/Compass";
            var prefab = Resources.Load<GameObject>(path);
            var obj = GameObject.Instantiate(prefab);

            // Get the TechTags and PrefabIdentifiers
            var techTag = obj.GetComponent<TechTag>();
            var prefabIdentifier = obj.GetComponent<PrefabIdentifier>();

            // Change them so they fit to our requirements.
            techTag.type = TechType;
            prefabIdentifier.ClassId = ClassID;

            var pick = obj.GetComponent<Pickupable>();
            if (pick == null)
            {
                CattleLogger.Message("Pickupable is Null");
            } else
            {
                if (pick.AllowedToPickUp())
                {
                    CattleLogger.Message("Allowed to pick up");
                } else
                {
                    CattleLogger.Message("NOT Allowed to pick up");
                }
            }

            return obj;
            */
        }

        protected override TechData GetBlueprintRecipe()
        {
            return new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(TechType.ComputerChip, 1),
                    new Ingredient(TechType.Aerogel, 2),
                    new Ingredient(TechType.Sulphur, 2),
                    new Ingredient(TechType.Lithium, 2),
                    new Ingredient(TechType.Lead, 4),
                }
            };
        }

        protected override Atlas.Sprite GetItemSprite()
        {
            return SpriteManager.Get(TechType.PowerUpgradeModule);
        }
    }
}