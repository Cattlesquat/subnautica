namespace BiomeHUDIndicator.Fabricator
{
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMLHelper.V2.Crafting;

    class BiomeHUDIndicator : CompassCore
    {
        public BiomeHUDIndicator() 
            : base(classID: "BiomeHUDIndicator", friendlyName: "Biome Indicator Compass", description: "An upgraded compass with uplinks to your PDA to display which biome you are currently in.")
        {
            OnFinishedPatching += SetStaticTechType;
        }

        // Setting up the base attributes
        protected override TechType BaseType { get; } = TechType.Compass;
    }
}
