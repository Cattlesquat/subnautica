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
        public GameObject _BiomeHUDObject { private get; set; }
        private static float t = 0f;
        private static BiomeDisplay main;

        
        private void Awake()
        {
            BiomeHUDObject = Main.BiomeHUD;
            _BiomeHUDObject = Instantiate(BiomeHUDObject);
            _BiomeHUDObject.SetActive(false);
            //BiomeHUDObject.transform.Find("BiomeHUDChip").gameObject.GetComponent<Text>().enabled = false;
            t = Time.time;
            SeraLogger.Message(Main.modName, "BiomeDisplay.Awake() has run. BiomeDisplay is awake and running!");
            BiomeDisplay.main = this;
        }

        private void Update()
        {
            float f = Time.time;
            if (f >= t + 5f && _BiomeHUDObject.transform.Find("BiomeHUDChip").gameObject.GetComponent<Text>().enabled == true)
            {
                SeraLogger.Message(Main.modName, "Vanish check is running");
                _BiomeHUDObject.SetActive(false);
                //BiomeHUDObject.transform.Find("BiomeHUDChip").gameObject.GetComponent<Text>().enabled = false;
                t = Time.time;
            }
        }

        private void ChangeText(string message)
        {
            SeraLogger.Message(Main.modName, "message: " + message);
            _BiomeHUDObject.transform.Find("BiomeHUDChip").gameObject.GetComponent<Text>().text = message;
            _BiomeHUDObject.SetActive(true);
            //BiomeHUDObject.transform.Find("BiomeHUDChip").gameObject.GetComponent<Text>().enabled = true;
            SeraLogger.Message(Main.modName, "ChangeText() BiomeHUDObject.BiomeHUDChip.Text.text: " + BiomeHUDObject.transform.Find("BiomeHUDChip").gameObject.GetComponent<Text>().text);
            SeraLogger.Message(Main.modName, "ChangeText() BiomeHUDObject.BiomeHUDChip.Text.enabled: " + BiomeHUDObject.transform.Find("BiomeHUDChip").gameObject.GetComponent<Text>().enabled.ToString());
            t = Time.time;
            SeraLogger.Message(Main.modName, "t: " + t.ToString());
        }

        public static void DisplayBiome(string message)
        {
            BiomeDisplay.main.ChangeText(message);
        }
    }
}
