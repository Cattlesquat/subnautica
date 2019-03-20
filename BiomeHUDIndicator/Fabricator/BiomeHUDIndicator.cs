namespace BiomeHUDIndicator.Fabricator
{
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMLHelper.V2.Crafting;
using UnityEngine;

    class BiomeHUDIndicator : CompassCore
    {
        public BiomeHUDIndicator()
            : base(classID: "BiomeHUDIndicator", friendlyName: "Biome Indicator Compass", description: "An upgraded compass with uplinks to your PDA to display which biome you are currently in.")
        {
            OnFinishedPatching += SetStaticTechType;
        }

        // Setting up the base attributes
        protected override TechType BaseType { get; } = TechType.Compass;
        protected override EquipmentType Chip { get; } = EquipmentType.Chip;

        protected override TechData GetBlueprintRecipe()
        {
            // UnityEngine.Debug.Log("[BiomeHUDIndicator] Establishing Crafting Recipe (comment out when done)");
            return new TechData
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>(3)
                {
                    new Ingredient(TechType.Compass, 1), // Need us an old compass
                    new Ingredient(TechType.ComputerChip, 1), // And a computer to tell us where we are
                    new Ingredient(TechType.Magnetite, 1), // And some magnetite for techy goodness
                }
            };
        }

        private void SetStaticTechType() => BiomeChipID = this.TechType;
    }
}
