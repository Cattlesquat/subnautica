/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 * A "chip slot" item that allows breathing surface air (combined with compass function)
 */

using System.Collections.Generic;
using Common;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using UnityEngine;
using UWE;

namespace DeathRun.Items
{
    public class FilterChip : Equipable
    {
        public FilterChip() : base(
            "FilterChip",
            "Integrated Air Filter",
            "Makes surface air breathable and purges nitrogen from the bloodstream while indoors. Comes with a free Compass.")
        {
        }

        public override EquipmentType EquipmentType => EquipmentType.Chip;

        public override TechType RequiredForUnlock => TechType.Cyclops; //this.TechType;

        public override TechGroup GroupForPDA => TechGroup.Personal;

        public override TechCategory CategoryForPDA => TechCategory.Equipment;

        public override CraftTree.Type FabricatorType => CraftTree.Type.Fabricator;

        public override string DiscoverMessage => $"{this.FriendlyName} Unlocked!";
        //public override bool AddScannerEntry => true;

        public override string[] StepsToFabricatorTab => new string[] { "Personal", "Equipment" };

        public override QuickSlotType QuickSlotType => QuickSlotType.None;

        public override GameObject GetGameObject()
        {
            string classid = CraftData.GetClassIdForTechType(TechType.Compass);
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
                    new Ingredient(TechType.Compass, 1),
                    new Ingredient(TechType.ComputerChip, 1),
                    new Ingredient(TechType.Polyaniline, 1),
                    new Ingredient(TechType.AramidFibers, 1),
                }
            };
        }        

        protected override Atlas.Sprite GetItemSprite()
        {
            return SpriteManager.Get(TechType.ComputerChip);
        }
    }
}