namespace BiomeHUDIndicator.BHIBehaviours
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using Common;
    using Items;

    class BiomeDisplay : MonoBehaviour
    {
        private static BiomeDisplay main;

        public GameObject _BiomeHUDObject { private get; set; }
        public Transform hudTransform; // Let's cache the HUD transform

        // We won't cache biome thumbnails due to the large number of elements.
        private Transform canvasTransform;
        private Text biomeText;
        private Text nowEntering;
        private Image backgroundThumbnails;
        private Image backgroundNoThumbnails;
        private Animator biomeTextAnimator;
        private Animator nowEnteringAnimator;

        private string _cachedBiome = "Unassigned";

        private byte imageAlpha = 255;
        
        private bool _started = false;
        private bool _thumbnailFlag = true;
        private bool _animationsEnabled = true;
        private bool _cachedFlag = false;
        private int _cachedIndex = 26;
        private float animationTimer = 0f;

        private void Awake()
        {
            BiomeDisplayOptions options = new BiomeDisplayOptions();
            _animationsEnabled = options.animationEnabled;
            _thumbnailFlag = options.imageEnabled;
            imageAlpha = options.alphaValue;

            _BiomeHUDObject = Instantiate<GameObject>(Main.BiomeHUD);

            // Cache objects we will call frequently
            canvasTransform = _BiomeHUDObject.transform;
            nowEntering = canvasTransform.GetChild(2).GetComponent<Text>();
            biomeText = canvasTransform.GetChild(3).GetComponent<Text>();
            backgroundThumbnails = canvasTransform.GetChild(0).gameObject.GetComponent<Image>();
            backgroundNoThumbnails = canvasTransform.GetChild(1).gameObject.GetComponent<Image>();
            biomeTextAnimator = biomeText.GetComponent<Animator>();
            nowEnteringAnimator = nowEntering.GetComponent<Animator>();

            backgroundThumbnails.enabled = false;
            backgroundNoThumbnails.enabled = false;
            nowEntering.enabled = true;
            biomeText.enabled = false;
            int i = 4;
            while (i <= 27)
            {
                canvasTransform.GetChild(i).gameObject.GetComponent<Image>().enabled = false;
                canvasTransform.GetChild(i).gameObject.GetComponent<Image>().color = new Color32(255, 255, 255, imageAlpha);
                i++;
            }
            biomeTextAnimator.enabled = true;
            biomeTextAnimator.SetBool("idle", true);
            nowEnteringAnimator.enabled = true;
            nowEnteringAnimator.SetBool("idle", true);

            hudTransform = GameObject.Find("ScreenCanvas").transform.Find("HUD");
            canvasTransform.SetParent(hudTransform, false);
            canvasTransform.SetSiblingIndex(0);

            BiomeDisplay.main = this;
            SeraLogger.Message(Main.modName, "BiomeDisplay is awake and running!");
        }

        private void Start()
        {
            _started = true;
        }

        private void Update()
        {
            if (Time.time >= animationTimer + 2.9f && !biomeTextAnimator.GetBool("idle"))
            {
                nowEnteringAnimator.SetBool("idle", true);
                biomeTextAnimator.SetBool("idle", true);
            }
            if (_started)
            {
                bool flag = this.IsVisible();
                if (_cachedFlag != flag)
                {
                    _cachedFlag = flag;
                    biomeText.enabled = _cachedFlag;
                    if (_thumbnailFlag)
                    {
                        backgroundThumbnails.enabled = _cachedFlag;
                        canvasTransform.GetChild(_cachedIndex).gameObject.GetComponent<Image>().enabled = _cachedFlag;
                    }
                    else
                    {
                        backgroundNoThumbnails.enabled = _cachedFlag;
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
            if (Player.main == null)
                return;
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
                            biomeText.text = biome.Value.FriendlyName;
                            if (_thumbnailFlag)
                            {
                                canvasTransform.GetChild(_cachedIndex).gameObject.GetComponent<Image>().enabled = false;
                                _cachedIndex = biome.Value.Index;
                                canvasTransform.GetChild(_cachedIndex).gameObject.GetComponent<Image>().enabled = _cachedFlag;
                                if (_animationsEnabled && _cachedFlag)
                                {
                                    nowEnteringAnimator.SetBool("idle", false);
                                    biomeTextAnimator.SetBool("idle", false);
                                    animationTimer = Time.time;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void UpdateImageVisibility()
        {
            if (_thumbnailFlag)
            {
                backgroundNoThumbnails.enabled = false;
                backgroundThumbnails.enabled = _cachedFlag;
                canvasTransform.GetChild(_cachedIndex).gameObject.GetComponent<Image>().enabled = _cachedFlag;
            }
            else
            {
                backgroundThumbnails.enabled = false;
                canvasTransform.GetChild(_cachedIndex).gameObject.GetComponent<Image>().enabled = false;
                backgroundNoThumbnails.enabled = _cachedFlag;
            }
        }

        private void UpdateAnimationVisibility (bool animation)
        {
            _animationsEnabled = animation;
        }

        public static void SetImageVisbility(bool imagesEnabled)
        {
            if (BiomeDisplay.main != null)
            {
                BiomeDisplay.main._thumbnailFlag = imagesEnabled;
                BiomeDisplay.main.UpdateImageVisibility();
            }
        }

        public static void SetAnimationEnabled(bool animations)
        {
            if (BiomeDisplay.main != null)
            {
                BiomeDisplay.main.UpdateAnimationVisibility(animations);
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
                    BiomeDisplay.main.canvasTransform.GetChild(i).gameObject.GetComponent<Image>().color = new Color32(255, 255, 255, BiomeDisplay.main.imageAlpha);
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
