namespace NitrogenMod
{
    using System;
    using System.Reflection;
    using Harmony;
    using SMLHelper.V2.Handlers;
    using SMLHelper.V2.Options;
    using System.IO;
    using Patchers;
    using Common;

    public class Main
    {
        public const string modName = "[NitrogenMod]";

        public static void Patch()
        {
            SeraLogger.PatchStart(modName, "1.0.0");
            try
            {
                var harmony = HarmonyInstance.Create("seraphimrisen.nitrogenmod.mod");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                OptionsPanelHandler.RegisterModOptions(new NitrogenOptions());
                SeraLogger.PatchComplete(modName);
            }
            catch (Exception ex)
            {
                SeraLogger.PatchFailed(modName, ex);
            }
        }
    }

    internal class NitrogenOptions : ModOptions
    {
        private const string configFile = "./QMods/NitrogenMod/Config.xml";

        private const string nitroEnablerName = "nitrogenmodenabler";
        private const string lethalName = "lethalmodeenabler";
        private const string crushEnablerName = "crushmodenabler";

        private const string nitroSliderName = "damagescalerslider";
        private const string crushSliderName = "crushdepthslider";

        public bool nitroEnabled = true;
        public bool nitroLethal = true;
        public bool crushEnabled = false;

        public float damageScaler = 1f;
        public float crushDepth = 500f;

        public NitrogenOptions() : base("Nitrogen Mod Options")
        {
            ToggleChanged += NitrogenEnabled;
            ToggleChanged += NonLethalOption;
            SliderChanged += DamageScalerSlider;
            ToggleChanged += CrushEnabled;
            SliderChanged += NewCrushDepth;
            ReadSettings();
        }

        internal void Initialize()
        {
            ReadSettings();
        }

        public override void BuildModOptions()
        {
            AddToggleOption(nitroEnablerName, "Enable Nitrogen", nitroEnabled);
            AddToggleOption(lethalName, "Lethal Decompression", nitroLethal);
            AddSliderOption(nitroSliderName, "Damage Scaler", 0.25f, 10f, damageScaler);
            AddToggleOption(crushEnablerName, "Enable Crush Depth", crushEnabled);
            AddSliderOption(crushSliderName, "Player Crush Depth", 250f, 1500f, crushDepth);
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

        private void NewCrushDepth(object sender, SliderChangedEventArgs args)
        {
            if (args.Id != crushSliderName)
                return;
            crushDepth = args.Value;
            BreathPatcher.AdjustCrush(crushDepth);
            SaveSettings();
        }

        private void SaveSettings()
        {
            ConfigMaker.WriteData(configFile, new SaveData(nitroEnabled, nitroLethal, damageScaler, crushEnabled, crushDepth));
        }

        private void ReadSettings()
        {
            if (!File.Exists(configFile))
            {
                SeraLogger.ConfigNotFound(Main.modName);
                SaveSettings();
            }
            try
            {
                SaveData loadedData = (SaveData)ConfigMaker.ReadData(configFile, typeof(SaveData));
                nitroEnabled = Boolean.Parse(loadedData.NitrogenEnabled);
                nitroLethal = Boolean.Parse(loadedData.IsLethal);
                damageScaler = float.Parse(loadedData.DamageScaler);
                crushEnabled = Boolean.Parse(loadedData.CrushEnabled);
                crushDepth = float.Parse(loadedData.CrushDepth);
            }
            catch (Exception ex)
            {
                SeraLogger.ConfigReadError(Main.modName, ex);
                nitroEnabled = true;
                nitroLethal = true;
                damageScaler = 1f;
                crushEnabled = false;
                crushDepth = 500f;
                SaveSettings();
            }
        }
    }

    public struct SaveData
    {
        public string NitrogenEnabled { get; set; }
        public string IsLethal { get; set; }
        public string CrushEnabled { get; set; }

        public string DamageScaler { get; set; }
        public string CrushDepth { get; set; }

        public SaveData(bool enabled, bool lethal, float scaler, bool crush, float depthDamage)
        {
            NitrogenEnabled = enabled.ToString();
            IsLethal = lethal.ToString();
            DamageScaler = scaler.ToString();
            CrushEnabled = crush.ToString();
            CrushDepth = depthDamage.ToString();
        }
    }
}
 
 
 
 