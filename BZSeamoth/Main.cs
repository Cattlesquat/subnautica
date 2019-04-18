namespace BZSeamoth
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Linq;
    using Harmony;
    using UnityEngine;
    using Common;

    public class Main
    {
        
    }


    /*private void OnConsoleCommand_spawn(NotificationCenter.Notification n)
    {
        if (n != null && n.data != null && n.data.Count > 0)
        {
            string text = (string)n.data[0];
            TechType techType;
            if (UWE.Utils.TryParseEnum<TechType>(text, out techType))
            {
                if (techType == TechType.Seamoth && n.data.Count <= 2)
                {
                    techType = TechType.SeaTruck;
                }
                if (CraftData.IsAllowed(techType))
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
                    else
                    {
                        ErrorMessage.AddDebug("Could not find prefab for TechType = " + techType);
                    }
                }
            }
            else
            {
                ErrorMessage.AddDebug("Could not parse " + text + " as TechType");
            }
        }
    }*/
}
