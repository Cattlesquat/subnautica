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
        public GameObject BiomeHUDObject { private get; set; }
        private static float t = 0f;
        private static BiomeDisplay main;

        
        private void Awake()
        {
            SeraLogger.Message(Main.modName, "BiomeDisplay.Awake()");
            GameObject BiomeHUDObject = Instantiate(Main.BiomeHUD);
            SeraLogger.Message(Main.modName, "BiomeHUDObject instantiated");
            BiomeHUDObject.transform.Find("BiomeHUDChip").gameObject.GetComponent<Text>().enabled = false;
            SeraLogger.Message(Main.modName, "BiomeHUDObject.BiomeHUDChip.Text.enabled: " + BiomeHUDObject.transform.Find("BiomeHUDChip").gameObject.GetComponent<Text>().enabled.ToString());
            t = Time.time;
            BiomeDisplay.main = this;
        }

        private void Update()
        {
            SeraLogger.Message(Main.modName, "Time.time: " + Time.time.ToString());
            SeraLogger.Message(Main.modName, "t: " + t.ToString());
            try
            {
                SeraLogger.Message(Main.modName, "BiomeHUDObject.BiomeHUDChip.Text.enabled: " + BiomeHUDObject.transform.Find("BiomeHUDChip").gameObject.GetComponent<Text>().enabled.ToString());
            }
            catch (Exception ex)
            {
                SeraLogger.GenericError(Main.modName, ex);
            }
            if (Time.time >= t + 5f && BiomeHUDObject.transform.Find("BiomeHUDChip").gameObject.GetComponent<Text>().enabled == true)
            {
                SeraLogger.Message(Main.modName, "Entered Time check in Update");
                BiomeHUDObject.transform.Find("BiomeHUDChip").gameObject.GetComponent<Text>().enabled = false;
                SeraLogger.Message(Main.modName, "BiomeHUDObject.BiomeHUDChip.Text.enabled: " + BiomeHUDObject.transform.Find("BiomeHUDChip").gameObject.GetComponent<Text>().enabled.ToString());
            }
        }

        private void ChangeText(string message)
        {
            try
            {
                BiomeHUDObject.transform.Find("BiomeHUDChip").gameObject.GetComponent<Text>().text = message;
            }
            catch (Exception ex)
            {
                SeraLogger.GenericError(Main.modName, ex);
            }
            try
            {
                BiomeHUDObject.transform.Find("BiomeHUDChip").gameObject.GetComponent<Text>().enabled = true;
            }
            catch (Exception ex)
            {
                SeraLogger.GenericError(Main.modName, ex);
            }
            
            t = Time.time;
            
            SeraLogger.Message(Main.modName, "BiomeHUDObject.BiomeHUDChip.Text.enabled: " + BiomeHUDObject.transform.Find("BiomeHUDChip").gameObject.GetComponent<Text>().enabled.ToString());
        }

        public static void DisplayBiome(string message)
        {
            BiomeDisplay.main.ChangeText(message);
        }
    }
}
