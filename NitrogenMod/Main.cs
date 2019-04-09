namespace NitrogenMod
{
    using System;
    using System.Reflection;
    using Harmony;
    using SMLHelper.V2.Handlers;
    using SMLHelper.V2.Options;
    using System.IO;
    using System.Xml.Serialization;
    using Patchers;
    using Common;

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
        private const string enablerName = "nitrogenmodenabler";
        private const string sliderName = "damagescalerslider";
        private const string lethalName = "lethalmodeenabler";
        public bool enabled = true;
        public bool lethal = true;
        public float damageScaler = 1f;

        public NitrogenOptions() : base("Nitrogen Mod Options")
        {
            ToggleChanged += NitrogenEnabled;
            SliderChanged += DamageScalerSlider;
            ToggleChanged += NonLethalOption;
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
            AddToggleOption(enablerName, "Enable Nitrogen", enabled);
            AddSliderOption(sliderName, "Damage Scaler", 0.25f, 10f, damageScaler);
            AddToggleOption(lethalName, "Lethal Decompression", lethal);
        }

        private void NitrogenEnabled(object sender, ToggleChangedEventArgs args)
        {
            if (args.Id != enablerName)
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
            NitroDamagePatcher.AdjustScaler(damageScaler);
        }

        private void NonLethalOption(object sender, ToggleChangedEventArgs args)
        {
            if (args.Id != lethalName)
                return;
            lethal = args.Value;
            SaveSettings();
            NitroDamagePatcher.Lethality(lethal);
        }

        private void SaveSettings()
        {
            SaveData saveData = new SaveData(enabled, damageScaler, lethal);
            ConfigMaker.WriteData(Config, saveData);
        }

        private void ReadSettings()
        {
            if (!File.Exists(Config))
            {
                UnityEngine.Debug.Log("[NitrogenMod] Config file not found. Creating default value.");
                SaveSettings();
            }
            SaveData loadedData = (SaveData) ConfigMaker.ReadData(Config, typeof(SaveData));
            try
            {
                enabled = Boolean.Parse(loadedData.isEnabled);
                damageScaler = float.Parse(loadedData.damageScaler);
                lethal = Boolean.Parse(loadedData.isLethal);
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
        public string isLethal;

        public SaveData()
        {
            isEnabled = "true";
            damageScaler = "1";
            isLethal = "true";
        }

        public SaveData(bool enabled, float scaler, bool lethal)
        {
            isEnabled = enabled.ToString();
            damageScaler = scaler.ToString();
            isLethal = lethal.ToString();
        }
    }
}
 
 
 
 