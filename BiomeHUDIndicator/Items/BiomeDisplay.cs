namespace BiomeHUDIndicator.Items
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Gendarme;
    using System.Text;
    using UnityEngine;
    using UnityEngine.UI;
    using Common;
    using Items;
    using QModManager;
    using UnityEngine.SceneManagement;
    using SMLHelper.V2.Assets;
    using SMLHelper.V2.Handlers;
    using SMLHelper.V2.Utility;
    using SMLHelper.V2.Crafting;

    class BiomeDisplay : MonoBehaviour
    {
        public float timeOnScreen = 5f;

        public GameObject BiomeHUDObject { private get; set; }
        public GameObject _BiomeHUDObject { private get; set; }

        private string _cachedBiome = "Unassigned";

        private bool cachedFlag = false;
        private bool _started = false;
        private int _cachedIndex = 26;

        private void Awake()
        {
            BiomeHUDObject = Main.BiomeHUD;
            // DontDestroyOnLoad(BiomeHUDObject);
            // DontDestroyOnLoad(_BiomeHUDObject);
        }

        private void Start()
        {
            _BiomeHUDObject = Instantiate<GameObject>(BiomeHUDObject);
            _BiomeHUDObject.transform.GetChild(0).gameObject.GetComponent<Image>().enabled = cachedFlag;
            _BiomeHUDObject.transform.GetChild(1).gameObject.GetComponent<Text>().enabled = cachedFlag;
            _BiomeHUDObject.transform.GetChild(2).gameObject.GetComponent<Text>().enabled = cachedFlag;
            int i = 3;
            while (i <= 26)
            {
                _BiomeHUDObject.transform.GetChild(i).gameObject.GetComponent<Image>().enabled = cachedFlag;
                i++;
            }
            DontDestroyOnLoad(_BiomeHUDObject);
            SeraLogger.Message(Main.modName, "BiomeDisplay.Start() has run. BiomeDisplay is awake and running!");
            Hooks.Update += EnsureInstantiation;
        }

        private void Update()
        {
            if (_started)
            {
                bool flag = this.IsVisible();
                if (cachedFlag != flag)
                {
                    cachedFlag = flag;
                    _BiomeHUDObject.transform.GetChild(0).gameObject.GetComponent<Image>().enabled = cachedFlag;
                    _BiomeHUDObject.transform.GetChild(2).gameObject.GetComponent<Text>().enabled = cachedFlag;
                    SeraLogger.Message(Main.modName, " _cachedIndex = " + _cachedIndex.ToString());
                    _BiomeHUDObject.transform.GetChild(_cachedIndex).gameObject.GetComponent<Image>().enabled = cachedFlag;
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

        private void SceneCheck()
        {
            if (SceneManager.GetActiveScene().name.ToLower().Contains("menu"))
            {
                Destroy(_BiomeHUDObject);
                Hooks.Update += EnsureInstantiation;
                _started = false;
                if (_started)
                {
                    _started = false;
                    this.CancelInvoke("SceneCheck");
                }
            }
        }

        private void EnsureInstantiation()
        {
            if (uGUI_SceneLoading.IsLoadingScreenFinished && !SceneManager.GetActiveScene().name.ToLower().Contains("menu"))
            {
                try
                {
                    _BiomeHUDObject = Instantiate<GameObject>(BiomeHUDObject);
                    _BiomeHUDObject.transform.GetChild(0).gameObject.GetComponent<Image>().enabled = cachedFlag;
                    _BiomeHUDObject.transform.GetChild(1).gameObject.GetComponent<Text>().enabled = cachedFlag;
                    _BiomeHUDObject.transform.GetChild(2).gameObject.GetComponent<Text>().enabled = cachedFlag;
                    int i = 3;
                    while (i <= 26)
                    {
                        _BiomeHUDObject.transform.GetChild(i).gameObject.GetComponent<Image>().enabled = cachedFlag;
                        i++;
                    }
                }
                catch (Exception ex)
                {
                    SeraLogger.Message(Main.modName, "Instantiating failed.");
                    SeraLogger.GenericError(Main.modName, ex);
                }
                InvokeRepeating("SceneCheck", 5f, 5f);
                _started = true;
                Hooks.Update -= EnsureInstantiation;
            }
        }

        private void BiomeUpdate()
        {
            string curBiome = Player.main.GetBiomeString().ToLower();
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
                            _BiomeHUDObject.transform.GetChild(2).gameObject.GetComponent<Text>().text = biome.Value.FriendlyName;
                            _BiomeHUDObject.transform.GetChild(_cachedIndex).gameObject.GetComponent<Image>().enabled = false;
                            _cachedIndex = biome.Value.Index;
                            if(cachedFlag)
                                _BiomeHUDObject.transform.GetChild(_cachedIndex).gameObject.GetComponent<Image>().enabled = cachedFlag;
                            // currentTarget = centerTarget;
                            // timeEnteredBiome = Time.time;
                        }
                    }
                }
            }
        }

        private readonly Dictionary<string, BiomeIndex> biomeList = new Dictionary<string, BiomeIndex>()
        {
            { "safe", new BiomeIndex("Safe Shallows", 3) },
            { "kelpforest", new BiomeIndex("Kelp Forest", 4) },
            { "grassy", new BiomeIndex("Grassy Plateaus", 5) },
            { "mushroomforest", new BiomeIndex("Mushroom Forest", 6) },
            { "jellyshroomcaves", new BiomeIndex("Jellyshroom Caves", 7) },
            { "sparse", new BiomeIndex("Sparse Reef", 8) },
            { "underwaterislands" , new BiomeIndex("Underwater Islands", 9) },
            { "bloodkelp" , new BiomeIndex("Blood Kelp Zone", 10) },
            { "dunes" , new BiomeIndex("Sand Dunes", 11) },
            { "crashzone" , new BiomeIndex("Crash Zone", 12) },
            { "grandreef" , new BiomeIndex("Grand Reef", 13) },
            { "mountains" , new BiomeIndex("Mountains", 14) },
            { "lostriver" , new BiomeIndex("Lost River", 15) },
            { "ilz" , new BiomeIndex("Inactive Lava Zone", 16) },
            { "lava" , new BiomeIndex("Lava Lake", 17) },
            { "floatingisland" , new BiomeIndex("Floating Island", 18) },
            { "koosh" , new BiomeIndex("Bulb Zone", 19) },
            { "seatreader" , new BiomeIndex("Sea Treader's Path", 20) },
            { "crag" , new BiomeIndex("Crag Field", 21) },
            { "void" , new BiomeIndex("Ecological Dead Zone", 22) },
            // Special-case biomes, may remove
            { "precursor" , new BiomeIndex("Precursor Facility", 23) },
            { "prison" , new BiomeIndex("Primary Containment Facility", 24) },
            { "shipspecial" , new BiomeIndex("Aurora", 25) },
            { "shipinterior", new BiomeIndex("Aurora", 25) },
            { "crashhome" , new BiomeIndex("Aurora", 25) },
            { "aurora" , new BiomeIndex("Aurora", 25) },
            { "crashedship" , new BiomeIndex("Aurora", 25) },
            { "unassigned" , new BiomeIndex("Unassigned", 26) }, // Add to AB
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
