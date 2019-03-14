namespace HabitatManager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using SMLHelper.V2.Assets;
    using SMLHelper.V2.Crafting;
    using SMLHelper.V2.Handlers;
    using SMLHelper.V2.Utility;
    using UnityEngine;

    public class HabitatManager : Buildable
    {
        // Setting up various names and text fields, some may be removed later
        public const string techName = "HabitatManager";
        public const string lookingAt = "UseHabitatManager";
        public static TechType TechTypeID { get; private set; } = TechType.UnusedOld;

        // Constructor that establishes names and tooltips and such
        private HabitatManager() : base(techName, "Habitat Manager", "Helps you manage various modules aboard your habitat.")
        {
        }
        
        // Set up the overrides that define where this appears in the PDA
        public override TechGroup GroupForPDA { get; } = TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.InteriorModule;
        public override string AssetsFolder { get; } = "HabitatManager/Assets";
        public override TechType RequiredForUnlock { get; } = TechType.BaseFiltrationMachine;

        // Time to get the game object
        public override GameObject GetGameObject()
        {
            var prefab = GameObject.Instantiate(CraftData.GetPrefabForTechType(TechType.PictureFrame)); // Gonna borrow the pictureframe model for now
            GameObject model = prefab.FindChild("model");

            const float modelScaling = 0.30f;
            model.transform.localScale -= new Vector3(modelScaling, modelScaling, modelScaling);
            return prefab;
        }

        // Time to define the recipe
        protected override TechData GetBlueprintRecipe()
        {
            return new TechData
            {
                Ingredients =
                {
                    new Ingredient(TechType.Titanium, 2),
                    new Ingredient(TechType.WiringKit, 1),
                    new Ingredient(TechType.ComputerChip, 1),
                    new Ingredient(TechType.Glass, 2),
                }
            };
        }

        // Time to run patch
        public void Patch()
        {
            this.TechType = TechTypeHandler.AddTechType(this.ClassID, this.FriendlyName, this.Description, false);
            TechTypeID = this.TechType;
        }
    }
}
