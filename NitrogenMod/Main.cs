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
        private const string Config = "./QMods/NitrogenMod/Config.xml";

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
            NitroBreathPatcher.EnableCrush(crushEnabled);
            SaveSettings();
        }

        private void NewCrushDepth(object sender, SliderChangedEventArgs args)
        {
            if (args.Id != crushSliderName)
                return;
            crushDepth = args.Value;
            NitroBreathPatcher.AdjustCrush(crushDepth);
            SaveSettings();
        }

        private void SaveSettings()
        {
            ConfigMaker.WriteData(Config, new SaveData(nitroEnabled, nitroLethal, damageScaler, crushEnabled, crushDepth));
        }

        private void ReadSettings()
        {
            if (!File.Exists(Config))
            {
                SeraLogger.ConfigNotFound("[NitrogenMod]");
                SaveSettings();
            }
            try
            {
                SaveData loadedData = (SaveData)ConfigMaker.ReadData(Config, typeof(SaveData));
                nitroEnabled = Boolean.Parse(loadedData.nitrogenEnabled);
                nitroLethal = Boolean.Parse(loadedData.isLethal);
                damageScaler = float.Parse(loadedData.damageScaler);
                crushEnabled = Boolean.Parse(loadedData.crushEnabled);
                crushDepth = float.Parse(loadedData.crushDepth);
            }
            catch (Exception ex)
            {
                SeraLogger.ConfigReadError("[NitrogenMod]", ex);
                nitroEnabled = true;
                nitroLethal = true;
                damageScaler = 1f;
                crushEnabled = false;
                crushDepth = 500f;
                SaveSettings();
            }
        }
    }

    public class SaveData
    {
        public string nitrogenEnabled;
        public string isLethal;
        public string crushEnabled;

        public string damageScaler;
        public string crushDepth;
        
        public SaveData()
        {
            nitrogenEnabled = "true";
            damageScaler = "1";
            isLethal = "true";
            crushEnabled = "false";
            crushDepth = "500";
        }

        public SaveData(bool enabled, bool lethal, float scaler, bool crush, float depthDamage)
        {
            nitrogenEnabled = enabled.ToString();
            isLethal = lethal.ToString();
            damageScaler = scaler.ToString();
            crushEnabled = crush.ToString();
            crushDepth = depthDamage.ToString();
        }
    }
}
 
 
 
 