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
            BiomeDisplay.main = this;
            BiomeHUDObject = Instantiate(Main.BiomeHUD);
            BiomeHUDObject.transform.Find("BiomeHUDChip").gameObject.GetComponent<Text>().enabled = false;
            t = Time.time;
            SeraLogger.Message(Main.modName, "BiomeDisplay.Awake() has run. BiomeDisplay is awake and running!");
        }

        private void Update()
        {
            if (Time.time >= t + 5f && BiomeHUDObject.transform.Find("BiomeHUDChip").gameObject.GetComponent<Text>().enabled == true)
            {
                SeraLogger.Message(Main.modName, "Entered Time check in Update");
                BiomeHUDObject.transform.Find("BiomeHUDChip").gameObject.GetComponent<Text>().enabled = false;
                SeraLogger.Message(Main.modName, "BiomeHUDObject.BiomeHUDChip.Text.enabled: " + BiomeHUDObject.transform.Find("BiomeHUDChip").gameObject.GetComponent<Text>().enabled.ToString());
            }
        }

        private void ChangeText(string message)
        {
            SeraLogger.Message(Main.modName, "message: " + message);
            BiomeHUDObject.transform.Find("BiomeHUDChip").gameObject.GetComponent<Text>().text = message;
            BiomeHUDObject.transform.Find("BiomeHUDChip").gameObject.GetComponent<Text>().enabled = true;
            t = Time.time;
            SeraLogger.Message(Main.modName, "BiomeHUDObject.BiomeHUDChip.Text.text: " + BiomeHUDObject.transform.Find("BiomeHUDChip").gameObject.GetComponent<Text>().text);
            SeraLogger.Message(Main.modName, "BiomeHUDObject.BiomeHUDChip.Text.enabled: " + BiomeHUDObject.transform.Find("BiomeHUDChip").gameObject.GetComponent<Text>().enabled.ToString());
        }

        public static void DisplayBiome(string message)
        {
            BiomeDisplay.main.ChangeText(message);
        }
    }
}
