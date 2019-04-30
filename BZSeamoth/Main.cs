namespace BZSeamoth
{
    using System;
    using System.Reflection;
    using Harmony;
    using UnityEngine;
    using Common;

    public class Main
    {
        public static string modName = "[BZSeamoth]";

        public static void Patch()
        {
            SeraLogger.PatchStart(modName, "1.1.0");
            try
            {
                var harmony = HarmonyInstance.Create("seraphimrisen.bzseamoth.mod");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                SeraLogger.PatchComplete(modName);
            }
            catch (Exception ex)
            {
                SeraLogger.PatchFailed(modName, ex);
            }
        }
    }

    [HarmonyPatch(typeof(SpawnConsoleCommand))]
    [HarmonyPatch("OnConsoleCommand_spawn")]
    internal class BZSeamothSpawnPatcher
    {
        [HarmonyPrefix]
        public static bool Prefix(ref SpawnConsoleCommand __instance, NotificationCenter.Notification n)
        {
            if (n != null && n.data != null && n.data.Count > 0)
            {
                string text = (string)n.data[0];
                TechType techType;
                if (UWE.Utils.TryParseEnum<TechType>(text, out techType))
                {
                    if (techType == TechType.Seamoth && n.data.Count <= 2)
                    {
                        GameObject prefabForTechType = CraftData.GetPrefabForTechType(techType, true);
                        if (prefabForTechType != null)
                        {
                            int num = 1;
                            int num2;
                            if (n.data.Count > 1 && int.TryParse((string)n.data[1], out num2))
                            {
                                num = num2;
                            }
                            float maxDist = 12f;
                            if (n.data.Count > 2)
                            {
                                maxDist = float.Parse((string)n.data[2]);
                            }
                            Debug.LogFormat("Spawning {0} {1}", new object[]
                            {
                            num,
                            techType
                            });
                            for (int i = 0; i < num; i++)
                            {
                                GameObject gameObject = global::Utils.CreatePrefab(prefabForTechType, maxDist, i > 0);
                                LargeWorldEntity.Register(gameObject);
                                CrafterLogic.NotifyCraftEnd(gameObject, techType);
                                gameObject.SendMessage("StartConstruction", SendMessageOptions.DontRequireReceiver);
                            }
                        }
                        return false;
                    }
                    return true;
                }
                else
                {
                    return true;
                }
            }
            return true;
        }
    }
    /*private void OnConsoleCommand_spawn(NotificationCenter.Notification n)
    {
        
    }*/
}
