namespace BiomeHUDIndicator.Patchers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Gendarme;
    using System.Text;
    using UnityEngine;
    using UnityEngine.UI;
    using Common;

    class BiomeDisplay : MonoBehaviour
    {
        public static GameObject BiomeHUDObject { private set; get; }
        private static float t = 0f;

        

        private void Awake()
        {
            BiomeHUDObject = Main.biomeHUD;
            SeraLogger.Message(Main.modName, "Awake()");
            try
            {
                SeraLogger.Message(Main.modName, "BiomeHUDObject.GetComponent<Text>().text: " + BiomeHUDObject.GetComponent<Text>().text);
            }
            catch (Exception ex)
            {
                SeraLogger.GenericError(Main.modName, ex);
            }
        }

        private void Update()
        {
            if (Time.time >= t + 5f && BiomeHUDObject.activeSelf == true)
            {
                try
                {
                    BiomeHUDObject.SetActive(false);
                }
                catch (Exception ex)
                {
                    SeraLogger.GenericError(Main.modName, ex);
                }
            }
            
        }

        public static void DisplayBiome(string message)
        {
            try
            {
                BiomeHUDObject.GetComponent<Text>().text = message;
                t = Time.time;
                BiomeHUDObject.SetActive(true);
            }
            catch (Exception ex)
            {
                SeraLogger.GenericError(Main.modName, ex);
            }
        }
    }
}
