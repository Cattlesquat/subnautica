/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 * Adapted from Mr. Purple's "Extra Options"
 * 
 * Ref - https://forums.unknownworlds.com/discussion/154099/mod-pc-murky-waters-v2-with-dll-patcher-wip
 */
namespace DeathRun.Patchers
{
    using HarmonyLib;
    using Items;
    using Common;
    using UnityEngine;

    [HarmonyPatch(typeof(WaterscapeVolume.Settings))]
    [HarmonyPatch("GetExtinctionAndScatteringCoefficients")]
    internal class MurkPatcher
    {        
        [HarmonyPrefix]
        public static bool Prefix (WaterscapeVolume.Settings __instance, ref Vector4 __result)
        {            
            var t = __instance;

            if (Config.MURK_NORMAL.Equals(DeathRun.config.murkiness)) 
            {
                return true;
            }

            float murkerizer;
            if (Config.MURK_DARK.Equals(DeathRun.config.murkiness))
            {
                murkerizer = 1.5f;   
            }
            else if (Config.MURK_DARKER.Equals(DeathRun.config.murkiness))
            {
                murkerizer = 2f;
            }
            else if(Config.MURK_DARKEST.Equals(DeathRun.config.murkiness))
            {
                murkerizer = 5f; // These people just want pain.
            } else
            {
                murkerizer = .5f; // Noticeable clarity without totally screwing the view up
            }
            
            float d = t.murkiness * murkerizer / 100f;
            Vector3 vector = t.absorption + t.scattering * Vector3.one;
            __result = new Vector4(vector.x, vector.y, vector.z, t.scattering) * d;
            return false;

            // This is what was going on in Purple's mod w/ sliding scale to 400
            //var m = 1.0f - Mathf.Clamp(murkerizer / 400.0f, 0.0f, 1.0f);
            //var mv = m * 180.0f + 10.0f;
            //float d = t.murkiness / mv;

            // This is what the vanilla code of the method looks like
            //float d = this.murkiness / 100f;
            //Vector3 vector = this.absorption + this.scattering * Vector3.one;
            //return new Vector4(vector.x, vector.y, vector.z, this.scattering) * d;
        }
    }
}
