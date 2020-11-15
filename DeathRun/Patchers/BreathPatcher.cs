namespace DeathRun.Patchers
{
    using HarmonyLib;
    using Items;
    using Common;
    using UnityEngine;

    //
    // Much of Nitrogen/Bends and Crush-Depth code adapted from SeraphimRisen's NitrogenMod
    //

    [HarmonyPatch(typeof(NitrogenLevel))]
    [HarmonyPatch("OnTookBreath")]
    internal class BreathPatcher
    {
        private static bool crushEnabled = false;
        private static bool crushed = false;

        [HarmonyPrefix]
        public static bool Prefix(ref NitrogenLevel __instance, Player player)
        {
            if (GameModeUtils.RequiresOxygen())
            {
                float depthOf = Ocean.main.GetDepthOf(player.gameObject);

                // Player's personal crush depth
                if (crushEnabled) {
                    if (Player.main.GetDepthClass() == Ocean.DepthClass.Crush)
                    {
                        if (!crushed)
                        {
                            ErrorMessage.AddMessage("Personal crush depth exceeded. Return to safe depth!");
                            crushed = true;
                        }                        
                        if (UnityEngine.Random.value < 0.5f)
                        {
                            float crushDepth = PlayerGetDepthClassPatcher.divingCrushDepth;
                            if (depthOf > crushDepth)
                            {
                                float crush = depthOf - crushDepth;
                                if (crush < 50)
                                {
                                    DamagePlayer(4);
                                }
                                else if (crush < 100)
                                {
                                    DamagePlayer(8);
                                }
                                else
                                {
                                    DamagePlayer(16);
                                }
                            }
                        }
                    } else
                    {
                        crushed = false;
                    }
                }
            }
            return false;
        }

        private static void DamagePlayer(float ouch)
        {
            LiveMixin component = Player.main.gameObject.GetComponent<LiveMixin>();
            component.TakeDamage(UnityEngine.Random.value * ouch/2 + ouch/2, default, DamageType.Normal, null);
        }

        public static void EnableCrush(bool isEnabled)
        {
            crushEnabled = isEnabled;
        }
    }
}