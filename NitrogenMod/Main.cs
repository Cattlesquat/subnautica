namespace NitrogenMod
{
    using System;
    using System.Reflection;
    using Harmony;
    using SMLHelper.V2.Handlers;
    using SMLHelper.V2.Options;
    using System.IO;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using Patchers;

    public class Main
    {
        public static void Patch()
        {
            UnityEngine.Debug.Log("[NitrogenMod] Start patching. Version: 1.0.0.0");
            try
            {
                var harmony = HarmonyInstance.Create("seraphimrisen.nitrogenmod.mod");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                NitrogenOptions data = new NitrogenOptions();
                OptionsPanelHandler.RegisterModOptions(data);
                NitroDamagePatcher.adjustScaler(data.damageScaler);
                UnityEngine.Debug.Log("[NitrogenMod] Patching complete.");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log($"[NitrogenMod] ERROR: {e.ToString()}");
            }
        }
    }

    internal class NitrogenOptions : ModOptions
    {
        private const string Config = "./QMods/NitrogenMod/Config.xml";
        private const string toggleName = "nitrogenmodenabler";
        private const string sliderName = "damagescalerslider";
        public bool enabled = true;
        public float damageScaler = 1f;

        public NitrogenOptions() : base("Nitrogen Mod Options")
        {
            ToggleChanged += NitrogenEnabled;
            SliderChanged += DamageScalerSlider;
            ReadSettings();
        }

        internal void Initialize()
        {
            try
            {
                ReadSettings();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.Log("[NitrogenMod] Error loading " + Config + ": " + ex.ToString());
                SaveSettings();
            }
        }

        public override void BuildModOptions()
        {
            AddToggleOption(toggleName, "Enable Nitrogen", enabled);
            AddSliderOption(sliderName, "Damage Scaler", 0.25f, 10f, damageScaler);
        }

        private void NitrogenEnabled(object sender, ToggleChangedEventArgs args)
        {
            if (args.Id != toggleName)
                return;
            enabled = args.Value;
            SaveSettings();
            try
            {
                DevConsole.SendConsoleCommand("nitrogen");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.Log("[NitrogenMod] Error executing Nitrogen command:" + ex.ToString());
            }
        }

        private void DamageScalerSlider(object sender, SliderChangedEventArgs args)
        {
            if (args.Id != sliderName)
                return;
            damageScaler = args.Value;
            SaveSettings();
            
        }

        private void SaveSettings()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SaveData));
            TextWriter writer = new StreamWriter(Config);
            SaveData saveData = new SaveData(enabled, damageScaler);
            serializer.Serialize(writer, saveData);
            writer.Close();
            /*
             * File.WriteAllLines(Config, new[]
             * {
             *     "#   This is a value of either true or false. #",
             *     "{" + enabled.ToString() + "}",
             * }, Encoding.UTF8);
             * UnityEngine.Debug.Log("[NitrogenMod] File written successfully!");
             */
        }

        private void ReadSettings()
        {
            if (!File.Exists(Config))
            {
                UnityEngine.Debug.Log("[NitrogenMod] Config file not found. Creating default value.");
                SaveSettings();
            }
            XmlSerializer serializer = new XmlSerializer(typeof(SaveData));
            FileStream fs = new FileStream(Config, FileMode.Open);
            SaveData loadedData;
            loadedData = (SaveData)serializer.Deserialize(fs);
            try
            {
                enabled = Boolean.Parse(loadedData.isEnabled);
                damageScaler = float.Parse(loadedData.damageScaler);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.Log("[NitrogenMod] Error reading file. Setting defaults. Exception: " + ex.ToString());
                enabled = true;
                damageScaler = 1f;
                SaveSettings();
            }
        }
    }

    public class SaveData
    {
        public string isEnabled;
        public string damageScaler;

        public SaveData()
        {
            isEnabled = "true";
            damageScaler = "1";
        }

        public SaveData(bool enabled, float scaler)
        {
            isEnabled = enabled.ToString();
            damageScaler = scaler.ToString();
        }
    }
}
 
 
 
 