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
            GameObject BiomeHUDObject = Instantiate(Main.biomeHUD);
            SeraLogger.Message(Main.modName, "Awake()");
        }

        private void Update()
        {
            if (BiomeHUDObject != null)
            {
                if (Time.time >= t + 5f && BiomeHUDObject.activeSelf == true)
                {
                    BiomeHUDObject.SetActive(false);
                }
            }
        }

        public static void DisplayBiome(string message)
        {
            if (BiomeHUDObject != null)
            {
                BiomeHUDObject.GetComponent<Text>().text = message;
                t = Time.time;
                BiomeHUDObject.SetActive(true);
            }
        }
    }
}
