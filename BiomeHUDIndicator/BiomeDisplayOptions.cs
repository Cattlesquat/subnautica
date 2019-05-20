namespace BiomeHUDIndicator
{
    using System;
    using BHIBehaviours;
    using Common;
    using SMLHelper.V2.Options;
    using System.IO;

    internal class BiomeDisplayOptions : ModOptions
    {
        private const string configFile = Main.modFolder + "Config.xml";

        private const string imageEnablerName = "biomeimageenabler";
        private const string animationEnablerName = "biomeanimationenabler";

        private const string transparencySliderName = "biomeimagetransparencyslider";

        private const string imageEnablerPasser = "SetImageAlphaPassed";

        public bool imageEnabled = true;
        public bool animationEnabled = true;

        private float sliderFloat = 100f;
        public byte alphaValue = 255;

        public BiomeDisplayOptions() : base("Biome HUD Indicator Options")
        {
            ToggleChanged += AnimationsEnabled;
            ToggleChanged += ImagesEnabled;
            SliderChanged += SetImageAlpha;
            ReadSettings();
        }

        internal void Initialize()
        {
            ReadSettings();
        }

        public override void BuildModOptions()
        {
            AddToggleOption(animationEnablerName, "Enable Animations", animationEnabled);
            AddToggleOption(imageEnablerName, "Images Enabled", imageEnabled);
            AddSliderOption(transparencySliderName, "Image Transparency %", 0f, 100f, sliderFloat);
        }

        private void ImagesEnabled(object sender, ToggleChangedEventArgs args)
        {
            if (args.Id != imageEnablerName && args.Id != imageEnablerPasser)
                return;
            imageEnabled = args.Value;
            BiomeDisplay.SetImageVisbility(imageEnabled);
            if (args.Id != imageEnablerPasser)
                SaveSettings();
        }

        private void AnimationsEnabled(object sender, ToggleChangedEventArgs args)
        {
            if (args.Id != animationEnablerName)
                return;
            animationEnabled = args.Value;
            BiomeDisplay.SetAnimationEnabled(animationEnabled);
            SaveSettings();
        }

        private void SetImageAlpha(object sender, SliderChangedEventArgs args)
        {
            if (args.Id != transparencySliderName)
                return;
            sliderFloat = args.Value;
            decimal num = Decimal.Round(Convert.ToDecimal(sliderFloat) / 100, 2);
            alphaValue = (byte)Math.Round(num * 255);
            BiomeDisplay.SetImageTransparency(alphaValue);
            if (alphaValue == 0)
                ImagesEnabled(this, new ToggleChangedEventArgs(imageEnablerPasser, false));
            else if (!imageEnabled)
                ImagesEnabled(this, new ToggleChangedEventArgs(imageEnablerPasser, true));
            SaveSettings();
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
                    animationEnabled = Boolean.Parse(loadedData.AnimationsEnabled);
                    imageEnabled = Boolean.Parse(loadedData.ImagesEnabled);
                    alphaValue = byte.Parse(loadedData.ImageAlpha);
                    sliderFloat = float.Parse(loadedData.SliderValue);
                }
                catch (Exception ex)
                {
                    SeraLogger.ConfigReadError(Main.modName, ex);
                    animationEnabled = true;
                    imageEnabled = true;
                    alphaValue = 255;
                    sliderFloat = 100f;
                    SaveSettings();
                }
            }
        }

        private void SaveSettings()
        {
            ConfigMaker.WriteData(configFile, new SaveData(animationEnabled, imageEnabled, alphaValue, sliderFloat));
        }
    }

    public struct SaveData
    {
        public string AnimationsEnabled { get; set; }
        public string ImagesEnabled { get; set; }
        public string ImageAlpha { get; set; }
        public string SliderValue { get; set; }

        public SaveData(bool animations, bool enabled, byte alpha, float slider)
        {
            AnimationsEnabled = animations.ToString();
            ImagesEnabled = enabled.ToString();
            ImageAlpha = alpha.ToString();
            SliderValue = slider.ToString();
        }
    }

}
