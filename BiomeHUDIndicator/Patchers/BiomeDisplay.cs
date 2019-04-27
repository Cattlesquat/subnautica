namespace BiomeHUDIndicator.Patchers
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

    class BiomeDisplay : MonoBehaviour
    {
        [Tooltip("The transform to use to scale/position the main text to the corner. Will be auto-positioned based on screen size")]
        public Transform cornerTarget;
        [Tooltip("The transform to use to scale/position the main text to the center. Will also be auto-positioned")]
        public Transform centerTarget;
        [Tooltip("The 'now entering' Text to display when a biome is entered")]
        public Text nowEntering;
        // The current target we are animating to
        Transform currentTarget;
        // The Text component to assign to
        Text UIText;
        // The time we last entered the biome
        float timeEnteredBiome = 0f;
        [Tooltip("The distance of the corner text from the edge")]
        public Vector2 cornerDistFromEdge;
        [Tooltip("The distance of the centre text from the top")]
        public float centerDistFromTop = 150f;
        [Tooltip("The distance from the 'now entering' text from the top")]
        public float nowEnteringDistFromTop = 125f;
        [Tooltip("The time in seconds the main text should spend in the middle of the screen")]
        public float timeOnScreen = 5f;

        public GameObject BiomeHUDObject { private get; set; }
        public GameObject _BiomeHUDObject { private get; set; }

        private string _cachedBiome = "Unassigned";

        private bool cachedFlag = false;
        private bool _started = false;

        private void Awake()
        {
            BiomeHUDObject = Main.BiomeHUD;
        }

        private void Start()
        {
            _BiomeHUDObject = Instantiate<GameObject>(BiomeHUDObject);
            _BiomeHUDObject.transform.GetChild(0).gameObject.GetComponent<Text>().enabled = cachedFlag;
            SeraLogger.Message(Main.modName, "BiomeDisplay.Start() has run. BiomeDisplay is awake and running!");
            Hooks.Update += EnsureInstantiation;
            cornerDistFromEdge = new Vector2(25f, 25f);

            currentTarget = cornerTarget;
        }

        private void Update()
        {
            if (_started)
            {
                bool flag = this.IsVisible();
                if (cachedFlag != flag)
                {
                    cachedFlag = flag;
                    //_BiomeHUDObject.transform.GetChild(0).gameObject.GetComponent<Image>().enabled = cachedFlag;
                    _BiomeHUDObject.transform.GetChild(2).gameObject.GetComponent<Text>().enabled = cachedFlag;
                }
                BiomeUpdate();
                //NowEntering();
            }
        }

        private void NowEntering()
        {
            // Position the reference points
            cornerTarget.localPosition = new Vector2(-Screen.width / 2 + cornerDistFromEdge.x, Screen.height / 2 - cornerDistFromEdge.y);
            centerTarget.localPosition = new Vector2(0, Screen.height / 2 - centerDistFromTop);
            nowEntering.transform.localPosition = new Vector2(0, Screen.height / 2 - nowEnteringDistFromTop);
            // Check if the main text has spent enough time on screen
            if (timeEnteredBiome + timeOnScreen <= Time.time)
            {
                currentTarget = cornerTarget;
            }
            // Total distance between start/finish
            float totaldist = Vector3.Distance(cornerTarget.position, centerTarget.position);
            // Normalized distance from current point to the corner position (for scale animation)
            float distanceNormalized = Vector3.Distance(transform.position, cornerTarget.position) / totaldist;
            // Animate the position/scale to the desired point on screen
            transform.position = Vector3.MoveTowards(transform.position, currentTarget.position, totaldist / 5f);
            transform.localScale = Vector3.Lerp(cornerTarget.localScale, centerTarget.localScale, distanceNormalized);
            // Animate the color of the now entering text
            Color textColor = Color.white;
            textColor.a = distanceNormalized;
            nowEntering.color = textColor;
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

        /// <summary>
        /// This method exists to make sure the player isn't in a menu or loading. Otherwise might have issues with losing the GameObject reference between game saves
        /// </summary>
        private void SceneCheck()
        {
            if (!uGUI_SceneLoading.IsLoadingScreenFinished || SceneManager.GetActiveScene().name.ToLower().Contains("menu"))
            {
                SeraLogger.Message(Main.modName, "SceneManager.GetActiveScene(): " + SceneManager.GetActiveScene());
                SeraLogger.Message(Main.modName, "IsLoadingScreenFinished: " + uGUI_SceneLoading.IsLoadingScreenFinished.ToString());
                Hooks.Update += EnsureInstantiation;
                _started = false;
                if (_started)
                {
                    _started = false;
                    this.CancelInvoke("SceneCheck");
                }
            }
        }

        /// <summary>
        /// This method ensures that after the game is loaded, the object is instantiated. Placing it in Awake/Start was failing
        /// </summary>
        private void EnsureInstantiation()
        {
            if (uGUI_SceneLoading.IsLoadingScreenFinished && !SceneManager.GetActiveScene().name.ToLower().Contains("menu"))
            {
                try
                {
                    _BiomeHUDObject = Instantiate<GameObject>(BiomeHUDObject);
                }
                catch (Exception ex)
                {
                    SeraLogger.Message(Main.modName, "Instantiating failed.");
                    SeraLogger.GenericError(Main.modName, ex);
                }
                try
                {
                    _BiomeHUDObject.transform.GetChild(2).gameObject.GetComponent<Text>().enabled = cachedFlag;
                }
                catch (Exception ex)
                {
                    SeraLogger.Message(Main.modName, "Setting Text.enabled failed.");
                    SeraLogger.GenericError(Main.modName, ex);
                }
                //_BiomeHUDObject.transform.GetChild(0).gameObject.GetComponent<Image>().enabled = cachedFlag;
                InvokeRepeating("SceneCheck", 5f, 15f);
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
                if (curBiome != _cachedBiome)
                {
                    _cachedBiome = curBiome;
                    if (cachedFlag)
                    {
                        foreach (var biome in biomeList)
                        {
                            if (curBiome.Contains(biome.Key))
                            {
                                _BiomeHUDObject.transform.GetChild(2).gameObject.GetComponent<Text>().text = biome.Value.FriendlyName;
                                currentTarget = centerTarget;
                                timeEnteredBiome = Time.time;
                            }
                        }
                    }
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
            { "sparse", new BiomeIndex("Sparse Reef", 9) }, // Image Needed
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
            // Special-case biomes, may remove
            { "precursor" , new BiomeIndex("Precursor Facility", 24) },
            { "prison" , new BiomeIndex("Primary Containment Facility", 25) },
            { "shipspecial" , new BiomeIndex("Aurora", 26) },
            { "shipinterior", new BiomeIndex("Aurora", 26) },
            { "crashhome" , new BiomeIndex("Aurora", 26) },
            { "aurora" , new BiomeIndex("Aurora", 26) },
            { "crashedship" , new BiomeIndex("Aurora", 26) },
            { "mesas" , new BiomeIndex("Mesa", 27) }, // Image Needed
            { "observatory" , new BiomeIndex("Observatory", 28) }, // Image Needed
            { "unassigned" , new BiomeIndex("Unassigned", 29) },
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
