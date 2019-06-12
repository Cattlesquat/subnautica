namespace NitrogenMod.Patchers
{
    using Harmony;
    using UnityEngine;

    [HarmonyPatch(typeof(Survival))]
    [HarmonyPatch("Use")]
    internal class SurvivalUsePatcher
    {
        [HarmonyPrefix]
        public static bool Prefix(ref Survival __instance, ref bool __result, GameObject useObj)
        {
            bool prefixFlag = true;
            if (useObj != null)
            {
                TechType techType = CraftData.GetTechType(useObj);
                NitrogenLevel nitrogenLevel = null;
                bool nFlag = false;

                if (Player.main.gameObject.GetComponent<NitrogenLevel>() != null)
                {
                    nitrogenLevel = Player.main.gameObject.GetComponent<NitrogenLevel>();
                    if (nitrogenLevel.safeNitrogenDepth >= 10f)
                        nFlag = true;
                }

                if (techType == TechType.FirstAidKit)
                {
                    if (Player.main.GetComponent<LiveMixin>().AddHealth(50f) > 0.1f)
                    {
                        prefixFlag = false;
                        __result = true;
                    }
                        
                    if (nFlag)
                    {
                        if (nitrogenLevel.safeNitrogenDepth > 10f)
                            nitrogenLevel.safeNitrogenDepth -= 10f;
                        else
                            nitrogenLevel.safeNitrogenDepth = 0f;

                        prefixFlag = false;
                        __result = true;
                    }
                    
                    if (__result)
                    {
                        string useEatSound = CraftData.GetUseEatSound(techType);
                        FMODUWE.PlayOneShot(useEatSound, Player.main.transform.position, 1f);
                    }
                }
            }
            return prefixFlag;
        }
    }
}
