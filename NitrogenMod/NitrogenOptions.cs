namespace NitrogenMod
{
    using System;
    using SMLHelper.V2.Options;
    using System.IO;
    using Patchers;
    using Common;

    class NitrogenOptions : ModOptions
    {
        private const string configFile = "./QMods/NitrogenMod/Config.xml";

        private const string nitroEnablerName = "nitrogenmodenabler";
        private const string lethalName = "lethalmodeenabler";
        private const string crushEnablerName = "crushmodenabler";
        private const string specialtyTanksEnablerName = "specialtytanksenabler";

        private const string nitroSliderName = "damagescalerslider";
        private const string crushSliderName = "crushdepthslider";

        public bool nitroEnabled = true;
        public bool nitroLethal = true;
        public bool crushEnabled = false;
        public bool specialtyTanksEnabled = true;

        public float damageScaler = 1f;

        public NitrogenOptions() : base("Nitrogen Mod Options")
        {
            ToggleChanged += SpecialtyTanksEnabled;
            ToggleChanged += NitrogenEnabled;
            ToggleChanged += NonLethalOption;
            SliderChanged += DamageScalerSlider;
            ToggleChanged += CrushEnabled;
            ReadSettings();
        }

        internal void Initialize()
        {
            ReadSettings();
        }

        public override void BuildModOptions()
        {
            AddToggleOption(specialtyTanksEnablerName, "Specialty Tanks (restart game)", specialtyTanksEnabled);
            AddToggleOption(nitroEnablerName, "Enable Nitrogen", nitroEnabled);
            AddToggleOption(lethalName, "Lethal Decompression", nitroLethal);
            AddSliderOption(nitroSliderName, "Damage Scaler", 0.25f, 10f, damageScaler);
            AddToggleOption(crushEnablerName, "Enable Crush Depth", crushEnabled);
        }

        private void SpecialtyTanksEnabled(object sender, ToggleChangedEventArgs args)
        {
            if (args.Id != specialtyTanksEnablerName)
                return;
            specialtyTanksEnabled = args.Value;
            Main.specialtyTanks = args.Value;
            SaveSettings();
        }

        private void NitrogenEnabled(object sender, ToggleChangedEventArgs args)
        {
            if (args.Id != nitroEnablerName)
                return;
            nitroEnabled = args.Value;
            try
            {
                DevConsole.SendConsoleCommand("nitrogen");
            }
            catch (Exception ex)
            {
                SeraLogger.GenericError(Main.modName, ex);
            }
            SaveSettings();
        }

        private void NonLethalOption(object sender, ToggleChangedEventArgs args)
        {
            if (args.Id != lethalName)
                return;
            nitroLethal = args.Value;
            NitroDamagePatcher.Lethality(nitroLethal);
            SaveSettings();
        }

        private void DamageScalerSlider(object sender, SliderChangedEventArgs args)
        {
            if (args.Id != nitroSliderName)
                return;
            damageScaler = args.Value;
            NitroDamagePatcher.AdjustScaler(damageScaler);
            SaveSettings();
        }

        private void CrushEnabled(object sender, ToggleChangedEventArgs args)
        {
            if (args.Id != crushEnablerName)
                return;
            crushEnabled = args.Value;
            BreathPatcher.EnableCrush(crushEnabled);
            SaveSettings();
        }

        private void SaveSettings()
        {
            ConfigMaker.WriteData(configFile, new SaveData(nitroEnabled, nitroLethal, damageScaler, crushEnabled, specialtyTanksEnabled));
        }

        private void ReadSettings()
        {
            if (!File.Exists(configFile))
            {
                SeraLogger.ConfigNotFound(Main.modName);
                SaveSettings();
            }
            else
            {
                try
                {
                    SaveData loadedData = (SaveData)ConfigMaker.ReadData(configFile, typeof(SaveData));
                    nitroEnabled = Boolean.Parse(loadedData.NitrogenEnabled);
                    nitroLethal = Boolean.Parse(loadedData.IsLethal);
                    damageScaler = float.Parse(loadedData.DamageScaler);
                    crushEnabled = Boolean.Parse(loadedData.CrushEnabled);
                    specialtyTanksEnabled = Boolean.Parse(loadedData.SpecialtyEnabled);
                }
                catch (Exception ex)
                {
                    SeraLogger.ConfigReadError(Main.modName, ex);
                    nitroEnabled = true;
                    nitroLethal = true;
                    damageScaler = 1f;
                    crushEnabled = false;
                    specialtyTanksEnabled = true;
                    SaveSettings();
                }
                Main.specialtyTanks = specialtyTanksEnabled;
            }
        }
    }

    public struct SaveData
    {
        public string NitrogenEnabled { get; set; }
        public string IsLethal { get; set; }
        public string CrushEnabled { get; set; }
        public string SpecialtyEnabled { get; set; }

        public string DamageScaler { get; set; }

        public SaveData(bool enabled, bool lethal, float scaler, bool crush, bool specialty)
        {
            NitrogenEnabled = enabled.ToString();
            IsLethal = lethal.ToString();
            DamageScaler = scaler.ToString();
            CrushEnabled = crush.ToString();
            SpecialtyEnabled = specialty.ToString();
        }
    }
}
