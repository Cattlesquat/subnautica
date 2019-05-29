namespace NoObservatoryMusic
{
    using System;
    using SMLHelper.V2.Options;
    using System.IO;
    using Patchers;
    using Common;

    internal class ObservatoryOptions : ModOptions
    {
        private const string Config = "./QMods/NoObservatoryMusic/Config.xml";

        private const string nomEnablerName = "noobservatorymusicenabler";

        public bool biomeDisabled = true;

        public ObservatoryOptions() : base("No Observatory Biome Options")
        {
            ToggleChanged += BiomeDisabled;
            InObservatoryPatcher.disabled = biomeDisabled;
            OnEnterPatcher.disabled = biomeDisabled;
            OnExitPatcher.disabled = biomeDisabled;
            ReadSettings();
        }

        internal void Initialize()
        {
            ReadSettings();
        }

        public override void BuildModOptions()
        {
            AddToggleOption(nomEnablerName, "Disable Biome", biomeDisabled);
        }

        private void BiomeDisabled(object sender, ToggleChangedEventArgs args)
        {
            if (args.Id != nomEnablerName)
                return;
            biomeDisabled = args.Value;
            InObservatoryPatcher.disabled = biomeDisabled;
            OnEnterPatcher.disabled = biomeDisabled;
            OnExitPatcher.disabled = biomeDisabled;
            SaveSettings();
        }

        private void SaveSettings()
        {
            ConfigMaker.WriteData(Config, biomeDisabled);
        }

        private void ReadSettings()
        {
            if (!File.Exists(Config))
            {
                SeraLogger.ConfigNotFound(Main.modName);
                SaveSettings();
            }
            else
            {
                try
                {
                    biomeDisabled = (bool)ConfigMaker.ReadData(Config, typeof(bool));
                }
                catch (Exception ex)
                {
                    SeraLogger.ConfigReadError(Main.modName, ex);
                    biomeDisabled = true;
                    SaveSettings();
                }
            }
        }
    }
}