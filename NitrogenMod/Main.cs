namespace NitrogenMod
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Linq;
    using Harmony;
    using UnityEngine;
    using SMLHelper.V2.Handlers;
    using SMLHelper.V2.Options;
    using SMLHelper.V2.Utility;
    using System.IO;
    using System.Text;

    public class Main
    {
        public static void Patch()
        {
            UnityEngine.Debug.Log("[NitrogenMod] Start patching. Version: 1.0.0.0");
            try
            {
                var harmony = HarmonyInstance.Create("seraphimrisen.nitrogenmod.mod");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                OptionsPanelHandler.RegisterModOptions(new NitrogenOptions());
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
        private const string Config = "./QMods/NitrogenMod/Config.txt";
        private const string toggleName = "nitrogenmodenabler";
        public bool enabled = true;

        public NitrogenOptions() : base("Nitrogen Mod Options")
        {
            base.ToggleChanged += NitrogenEnabled;
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
            base.AddToggleOption(toggleName, "Enable Nitrogen", enabled);
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

        private void SaveSettings()
        {
            File.WriteAllLines(Config, new[]
            {
                "#   This is a value of either true or false. #",
                "{" + enabled.ToString() + "}",
            }, Encoding.UTF8);
            UnityEngine.Debug.Log("[NitrogenMod] File written successfully!");
        }

        private void ReadSettings()
        {
            if (!File.Exists(Config))
            {
                UnityEngine.Debug.Log("[NitrogenMod] Config file not found. Creating default value.");
                SaveSettings();
            }

            string file = File.ReadAllText(Config, Encoding.UTF8);
            file = file.ToLower();
            if (file.Contains("{true}"))
            {
                enabled = true;
                UnityEngine.Debug.Log("[NitrogenMod] Config contained true");
            }
            else if (file.Contains("{false}"))
            {
                enabled = false;
                UnityEngine.Debug.Log("[NitrogenMod] Config contained false");
            }
        }
    } 
}
