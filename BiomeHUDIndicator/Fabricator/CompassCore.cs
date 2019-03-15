namespace BiomeHUDIndicator.Fabricator
{
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using UnityEngine;

// We're not going to modify the base compass so much as create a successor or upgrade to it
    class CompassCore : Craftable
    {
        // Set up all the names and paths and stuff
        private const string craftTab = "HUDChips";
        private const string Assets = @"BiomeHUDIndicator/Assets";
        private static readonly string[] craftPath = new[] { craftTab };

        // Setting up class-level stuff
        public static TechType BiomeChipID { get; protected set; }

        internal static void PatchIt()
        {
            // Nothing else at this time upgrades HUD chips so we'll just go ahead and make the tab. May update at later date
            var tabIcon = ImageUtils.LoadSpriteFromFile(@"./Qmods/" + Assets + @"/TabIcon.png");
            CraftTreeHandler.AddTabNode(CraftTree.Type.Workbench, craftTab, "HUD Chip Upgrades", tabIcon);
            UnityEngine.Debug.Log("Crafting tab HUD Chip Upgrades created.");
        }

        protected abstract TechType BaseType { get; } // Gonna use base type Compass
        
        // Let's get us some hot constructor action
        protected CompassCore(string classID, string friendlyName, string description) : base(classID, friendlyName, description)
        {
            // Invoke when done
            OnFinishedPatching += SetEquipmentType;
        }


    }
}
