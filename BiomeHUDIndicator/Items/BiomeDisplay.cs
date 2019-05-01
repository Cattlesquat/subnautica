namespace BiomeHUDIndicator.Items
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using Common;

    class BiomeDisplay : MonoBehaviour
    {
        private static BiomeDisplay main;

        private const float timeOnScreen = 5f;

        public GameObject _BiomeHUDObject { private get; set; }
        public Transform hudTransform;

        private string _cachedBiome = "Unassigned";

        private byte imageAlpha = 255;
        
        private bool _started = false;
        private bool _cachedImageFlag = true;
        private bool _cachedFlag = false;
        private int _cachedIndex = 26;

        private void Awake()
        {
            BiomeDisplayOptions options = new BiomeDisplayOptions();
            imageAlpha = options.alphaValue;
            _cachedImageFlag = options.imageEnabled;
            _BiomeHUDObject = Instantiate<GameObject>(Main.BiomeHUD);
            hudTransform = GameObject.Find("ScreenCanvas").transform.Find("HUD");
            _BiomeHUDObject.transform.GetChild(0).gameObject.GetComponent<Image>().enabled = false;
            _BiomeHUDObject.transform.GetChild(1).gameObject.GetComponent<Image>().enabled = false;
            _BiomeHUDObject.transform.GetChild(2).gameObject.GetComponent<Text>().enabled = false;
            _BiomeHUDObject.transform.GetChild(3).gameObject.GetComponent<Text>().enabled = false;
            int i = 4;
            while (i <= 27)
            {
                _BiomeHUDObject.transform.GetChild(i).gameObject.GetComponent<Image>().enabled = false;
                _BiomeHUDObject.transform.GetChild(i).gameObject.GetComponent<Image>().color = new Color32(255, 255, 255, imageAlpha);
                i++;
            }
            _BiomeHUDObject.transform.SetParent(hudTransform, false);
            _BiomeHUDObject.transform.SetSiblingIndex(0);
            BiomeDisplay.main = this;
            SeraLogger.Message(Main.modName, "BiomeDisplay.Start() has run. BiomeDisplay is awake and running!");
        }

        private void Start()
        {
            _started = true;
        }

        private void Update()
        {
            if (_started)
            {
                bool flag = this.IsVisible();
                if (_cachedFlag != flag)
                {
                    _cachedFlag = flag;
                    _BiomeHUDObject.transform.GetChild(3).gameObject.GetComponent<Text>().enabled = _cachedFlag;
                    if (_cachedImageFlag)
                    {
                        _BiomeHUDObject.transform.GetChild(0).gameObject.GetComponent<Image>().enabled = _cachedFlag;
                        _BiomeHUDObject.transform.GetChild(_cachedIndex).gameObject.GetComponent<Image>().enabled = _cachedFlag;
                    }
                    else
                    {
                        _BiomeHUDObject.transform.GetChild(1).gameObject.GetComponent<Image>().enabled = _cachedFlag;
                    }
                }
                BiomeUpdate();
            }
        }

        private bool IsVisible()
        {
            if (this == null)
                return false;
            if (!uGUI_SceneLoading.IsLoadingScreenFinished)
                return false;
            if (!uGUI.isMainLevel)
                return false;
            if (LaunchRocket.isLaunching)
                return false;
            if (uGUI.isIntro)
                return false;
            Player main = Player.main;
            if (main == null)
                return false;
            PDA pda = main.GetPDA();
            if (pda != null && pda.isInUse)
                return false;
            Player.Mode mode = main.GetMode();
            if (mode == Player.Mode.Piloting)
                return false;
            Inventory main2 = Inventory.main;
            int biomeChip = main2.equipment.GetCount(CompassCore.BiomeChipID);
            if (main2 != null && main2.equipment != null && biomeChip > 0)
                return true;
            uGUI_CameraDrone main3 = uGUI_CameraDrone.main;
            return main3 != null && main3.GetCamera() != null;
        }
        
        private void BiomeUpdate()
        {
            string curBiome = null;
            if (Player.main != null)
                curBiome = Player.main.GetBiomeString().ToLower();
            if (curBiome != null)
            {
                int index = curBiome.IndexOf('_');
                if (index > 0)
                {
                    curBiome = curBiome.Substring(0, index);
                }
                if (curBiome != _cachedBiome && curBiome != "observatory")
                {
                    _cachedBiome = curBiome;
                    foreach (var biome in biomeList)
                    {
                        if (curBiome.Contains(biome.Key))
                        {
                            _BiomeHUDObject.transform.GetChild(3).gameObject.GetComponent<Text>().text = biome.Value.FriendlyName;
                            if (_cachedImageFlag)
                            {
                                _BiomeHUDObject.transform.GetChild(_cachedIndex).gameObject.GetComponent<Image>().enabled = false;
                                _cachedIndex = biome.Value.Index;
                                _BiomeHUDObject.transform.GetChild(_cachedIndex).gameObject.GetComponent<Image>().enabled = _cachedFlag;
                            }
                        }
                    }
                }
            }
        }

        private void UpdateImageVisibility()
        {
            if (_cachedImageFlag)
            {
                _BiomeHUDObject.transform.GetChild(0).gameObject.GetComponent<Image>().enabled = _cachedFlag;
                _BiomeHUDObject.transform.GetChild(1).gameObject.GetComponent<Image>().enabled = false;
                _BiomeHUDObject.transform.GetChild(_cachedIndex).gameObject.GetComponent<Image>().enabled = _cachedFlag;
            }
            else
            {
                _BiomeHUDObject.transform.GetChild(0).gameObject.GetComponent<Image>().enabled = false;
                _BiomeHUDObject.transform.GetChild(_cachedIndex).gameObject.GetComponent<Image>().enabled = false;
                _BiomeHUDObject.transform.GetChild(1).gameObject.GetComponent<Image>().enabled = _cachedFlag;
            }
        }

        public static void SetImageVisbility(bool imagesEnabled)
        {
            if (BiomeDisplay.main != null)
            {
                BiomeDisplay.main._cachedImageFlag = imagesEnabled;
                BiomeDisplay.main.UpdateImageVisibility();
            }
        }

        public static void SetImageTransparency(byte transparency)
        {
            if (BiomeDisplay.main != null)
            {
                BiomeDisplay.main.imageAlpha = transparency;
                int i = 4;
                while (i <= 27)
                {
                    BiomeDisplay.main._BiomeHUDObject.transform.GetChild(i).gameObject.GetComponent<Image>().color = new Color32(255, 255, 255, BiomeDisplay.main.imageAlpha);
                    i++;
                }
            }
        }

        private readonly Dictionary<string, BiomeIndex> biomeList = new Dictionary<string, BiomeIndex>()
        {
            { "safe", new BiomeIndex("Safe Shallows", 4) },
            { "kelpforest", new BiomeIndex("Kelp Forest", 5) },
            { "grassy", new BiomeIndex("Grassy Plateaus", 6) },
            { "mushroomforest", new BiomeIndex("Mushroom Forest", 7) },
            { "jellyshroomcaves", new BiomeIndex("Jellyshroom Caves", 8) },
            { "sparse", new BiomeIndex("Sparse Reef", 9) },
            { "underwaterislands" , new BiomeIndex("Underwater Islands", 10) },
            { "bloodkelp" , new BiomeIndex("Blood Kelp Zone", 11) },
            { "dunes" , new BiomeIndex("Sand Dunes", 12) },
            { "crashzone" , new BiomeIndex("Crash Zone", 13) },
            { "grandreef" , new BiomeIndex("Grand Reef", 14) },
            { "mountains" , new BiomeIndex("Mountains", 15) },
            { "lostriver" , new BiomeIndex("Lost River", 16) },
            { "ilz" , new BiomeIndex("Inactive Lava Zone", 17) },
            { "lava" , new BiomeIndex("Lava Lake", 18) },
            { "floatingisland" , new BiomeIndex("Floating Island", 19) },
            { "koosh" , new BiomeIndex("Bulb Zone", 20) },
            { "seatreader" , new BiomeIndex("Sea Treader's Path", 21) },
            { "crag" , new BiomeIndex("Crag Field", 22) },
            { "void" , new BiomeIndex("Ecological Dead Zone", 23) },
            { "precursor" , new BiomeIndex("Precursor Facility", 24) },
            { "prison" , new BiomeIndex("Primary Containment Facility", 25) },
            { "shipspecial" , new BiomeIndex("Aurora", 26) },
            { "shipinterior", new BiomeIndex("Aurora", 26) },
            { "crashhome" , new BiomeIndex("Aurora", 26) },
            { "aurora" , new BiomeIndex("Aurora", 26) },
            { "crashedship" , new BiomeIndex("Aurora", 26) },
            { "unassigned" , new BiomeIndex("Unassigned", 27) },
        };

        public struct BiomeIndex
        {
            public string FriendlyName { get; }
            public int Index { get; }

            public BiomeIndex(string name, int i)
            {
                FriendlyName = name;
                Index = i;
            }
        }
    }
}
