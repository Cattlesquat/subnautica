namespace BiomeHUDIndicator
{
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using Fabricator;
using UnityEngine;
using UnityEngine.UI;

    internal class uGUI_BiomeIndicator : MonoBehaviour
    {
        // Setting up some cached values in case I need them
        public Text _cachedBiome;
        // This space reserved for fields the class needs to work with
        public Color32 textColor;
        [Header("Biome Indicator")] public Image shadow;
        private bool _initialized;
        private bool _showing;

        // Start method goes here
        private void Start()
        {
            this.shadow.material = new Material(this.shadow.material);
        }

        // OnDisable method goes here
        private void OnDisable()
        {

        }

        // LateUpdate goes here
        private void LateUpdate()
        {

        }

        // Initialize method
        private void Initialize()
        {
            if (this._initialized)
            {
                return;
            }
            Player main = Player.main;
            if (main == null)
            {
                return;
            }
            Language main2 = Language.main;
            if (main2 == null)
            {
                return;
            }
            this._cachedBiome.text = "None";
            this._initialized = true;
        }

        // Deinitialize method
        private void Deinitialize()
        {
            if(!this._initialized)
            {
                return;
            }
            this._initialized = false;
            Language main = Language.main;
            if (main != null)
            {

            }
        }

        // OnLanguageChanged method
        private void OnLanguageChanged()
        {

        }

        // UpdateBiome method
        private void Update()
        {
            if(!_initialized)
            {
                return;
            }
            Player main = Player.main;
            string curBiome = main.GetBiomeString();
            // This IF tree should get almost any biome
            if (curBiome != _cachedBiome.text)
            {
                if (curBiome.StartsWith("safe", true, null))
                {
                    _cachedBiome.text = "Safe Shallows";
                }
                else if (curBiome.StartsWith("kelp", true, null))
                {
                    _cachedBiome.text = "Kelp Forest";
                }
                else if (curBiome.StartsWith("Grassy", true, null))
                {
                    _cachedBiome.text = "Grassy Plateaus";
                }
                else if (curBiome.StartsWith("mushroom", true, null))
                {
                    _cachedBiome.text = "Mushroom Forest";
                }
                else if (curBiome.StartsWith("Jelly", true, null))
                {
                    _cachedBiome.text = "Jellyshroom Caves";
                }
                else if (curBiome.StartsWith("sparse", true, null))
                {
                    _cachedBiome.text = "sparseReef";
                }
                else if (curBiome.StartsWith("Floating", true, null))
                {
                    _cachedBiome.text = "Floating Island";
                }
                else if (curBiome.StartsWith("Ship", true, null) || curBiome.Equals("CrashHome") || curBiome.StartsWith("Aurora", true, null))
                {
                    _cachedBiome.text = "Aurora";
                }
                else if (curBiome.StartsWith("CrashZone", true, null))
                {
                    _cachedBiome.text = "Crash Zone";
                }
                else if (curBiome.StartsWith("UnderwaterIslands", true, null))
                {
                    _cachedBiome.text = "Underwater Islands";
                }
                else if (curBiome.StartsWith("seaTreader", true, null))
                {
                    _cachedBiome.text = "Sea Treader Path";
                }
                else if (curBiome.StartsWith("Grand", true, null))
                {
                    _cachedBiome.text = "Grand Reef";
                }
                else if (curBiome.StartsWith("DeepGrand"))
                {
                    _cachedBiome.text = "Deep Grand Reef";
                }
                else if (curBiome.StartsWith("Mountains", true, null) && !curBiome.Contains("Island"))
                {
                    _cachedBiome.text = "Mountains";
                }
                else if (curBiome.StartsWith("Mountains_Island", true, null))
                {
                    _cachedBiome.text = "Mountain Island";
                }
                else if (curBiome.StartsWith("Dunes", true, null))
                {
                    _cachedBiome.text = "Dunes";
                }
                else if (curBiome.StartsWith("LostRiver") || curBiome.StartsWith("Skeleton"))
                {
                    _cachedBiome.text = "Lost River";
                }
                else if (curBiome.StartsWith("TreeCove", true, null) || curBiome.StartsWith("GhostTree", true, null))
                {
                    _cachedBiome.text = "Ghost Tree Cove";
                }
                else if (curBiome.StartsWith("Crag", true, null))
                {
                    _cachedBiome.text = "Crag Field";
                }
                else if (curBiome.StartsWith("BonesField", true, null))
                {
                    _cachedBiome.text = "Bones Field";
                }
                else if (curBiome.StartsWith("Koosh", true, null))
                {
                    _cachedBiome.text = "Bulb Zone";
                }
                else if (curBiome.StartsWith("blood", true, null))
                {
                    _cachedBiome.text = "Blood Kelp Forest";
                }
                else if (curBiome.StartsWith("Inactive", true, null))
                {
                    _cachedBiome.text = "Inactive Lava Zone";
                }
                else if (curBiome.StartsWith("Active", true, null))
                {
                    _cachedBiome.text = "Active Lava Zone";
                }
                else if (curBiome.StartsWith("Mesas", true, null))
                {
                    _cachedBiome.text = "Crash Zone Mesas";
                }
                else if (curBiome.StartsWith("Prison", true, null))
                {
                    _cachedBiome.text = "Unknown";
                }
            }
        }
    }
}
