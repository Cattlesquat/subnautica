namespace NoObservatoryMusic
{
    using System;
    using System.Reflection;
    using Harmony;
    using SMLHelper.V2.Handlers;
    using SMLHelper.V2.Options;
    using System.IO;
    using Patchers;
    using Common;

    public static class Main
    {
        public static void Patch()
        {
            string modName = "[NoObservatoryMusic]";
            Logger.PatchStart(modName, "1.0.1");
            try
            {
                var harmony = HarmonyInstance.Create("seraphimrisen.noobservatorymusic.mod");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                OptionsPanelHandler.RegisterModOptions(new ObservatoryOptions());
                Logger.PatchComplete(modName);
            }
            catch (Exception ex)
            {
                Logger.PatchFailed(modName, ex);
            }
        }
    }

    internal class ObservatoryOptions : ModOptions
    {
        private const string modName = "[NoObservatoryMusic]";
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
                Logger.ConfigNotFound(modName);
                SaveSettings();
            }
            try
            {
                biomeDisabled = (bool) ConfigMaker.ReadData(Config, typeof(bool));
                Logger.ConfigReadSuccess(modName);
            }
            catch (Exception ex)
            {
                Logger.ConfigReadError(modName, ex);
                biomeDisabled = true;
                SaveSettings();
            }
        }
    }
}
