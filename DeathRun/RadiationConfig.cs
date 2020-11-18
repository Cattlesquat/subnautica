/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 * Adapted from libraryaddict's Radiation Challenge mod -- used w/ permission.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMLHelper.V2.Json;
using SMLHelper.V2.Options.Attributes;

namespace DeathRun
{
    [Menu("RadiationChallenge")]
    public class Config : ConfigFile
    {
        [Slider("radiativeDepth", 1f, 1000f, DefaultValue = 30F, Format = "{0:R0}")]
        public float radiativeDepth = 30;
        [Slider("explosionDepth", 1f, 1000f, DefaultValue = 50F, Format = "{0:R0}")]
        public float explosionDepth = 50;
        [Slider("radiativePowerConsumptionMultiplier", 1f, 1000f, DefaultValue = 5F, Format = "{0:R0}")]
        public float radiativePowerConsumptionMultiplier = 5;
        [Slider("radiativePowerAddMultiplier", 0.1f, 1000f, DefaultValue = 0.2F, Format = "{0:R0}")]
        public float radiativePowerAddMultiplier = 0.2f;
        [Toggle("disableFabricatorFood")]
        public bool disableFabricatorFood = true;
        [Toggle("preventPreRadiativeFoodPickup")]
        public bool preventPreRadiativeFoodPickup = true;
        [Toggle("preventRadiativeFoodPickup")]
        public bool preventRadiativeFoodPickup = true;
        [Toggle("poisonedAir")]
        public bool poisonedAir = true;
        [Toggle("radiationWarning")]
        public bool radiationWarning = true;
    }
}
