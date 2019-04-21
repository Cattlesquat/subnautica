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
        /*[Tooltip("The transform to use to scale/position the main text to the corner. Will be auto-positioned based on screen size")]
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
        public float centerDistFromTop;
        [Tooltip("The distance from the 'now entering' text from the top")]
        public float nowEnteringDistFromTop;
        [Tooltip("The time in seconds the main text should spend in the middle of the screen")]
        public float timeOnScreen = 5f;*/

        public GameObject BiomeHUDObject { private get; set; }
        public GameObject _BiomeHUDObject { private get; set; }

        private string _cachedBiome = "Unassigned";

        private bool instantiated = false;
        private bool cachedFlag = false;

        private void Awake()
        {
            BiomeHUDObject = Main.BiomeHUD;
        }

        private void Start()
        {
            _BiomeHUDObject = Instantiate<GameObject>(BiomeHUDObject);
            _BiomeHUDObject.transform.Find("BiomeHUDChip").gameObject.GetComponent<Text>().enabled = false;
            SeraLogger.Message(Main.modName, "BiomeDisplay.Awake() has run. BiomeDisplay is awake and running!");
            Hooks.Update += EnsureInstantiation;
            
            /*currentTarget = cornerTarget;*/
        }

        private void Update()
        {
            bool flag = this.IsVisible();
            if (cachedFlag != flag)
            {
                cachedFlag = flag;
                _BiomeHUDObject.transform.Find("BiomeHUDChip").gameObject.GetComponent<Text>().enabled = cachedFlag;
                /*try
                {
                    _BiomeHUDObject.transform.Find("BiomeBackground").gameObject.SetActive(cachedFlag);
                }
                catch (Exception ex)
                {
                    SeraLogger.GenericError(Main.modName, ex);
                }*/
            }
            if (cachedFlag)
                BiomeCheck();

            /*// Position the reference points
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
            nowEntering.color = textColor;*/
        }

        private bool IsVisible()
        {
            if (this == null)
                return false;
            if (!instantiated)
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
                Hooks.Update += EnsureInstantiation;
                instantiated = false;
                if (instantiated)
                    this.CancelInvoke("SceneCheck");
            }
        }

        /// <summary>
        /// This method ensures that after the game is loaded, the object is instantiated. Placing it in Awake/Start was failing
        /// </summary>
        private void EnsureInstantiation()
        {
            if (uGUI_SceneLoading.IsLoadingScreenFinished && !SceneManager.GetActiveScene().name.ToLower().Contains("menu"))
            {
                
                _BiomeHUDObject = Instantiate<GameObject>(BiomeHUDObject);
                _BiomeHUDObject.transform.Find("BiomeHUDChip").gameObject.GetComponent<Text>().enabled = cachedFlag;
                /*try
                {
                    _BiomeHUDObject.transform.Find("BiomeBackground").gameObject.SetActive(cachedFlag);
                }
                catch (Exception ex)
                {
                    SeraLogger.GenericError(Main.modName, ex);
                }*/
                InvokeRepeating("SceneCheck", 5f, 15f);
                instantiated = true;
                Hooks.Update -= EnsureInstantiation;
            }
        }

        private void BiomeCheck()
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
                    foreach (var biome in biomeList)
                    {
                        if (curBiome.Contains(biome.Key))
                        {
                            _BiomeHUDObject.transform.Find("BiomeHUDChip").gameObject.GetComponent<Text>().text = biome.Value;
                        }
                    }
                }
            }
        }

        private readonly Dictionary<string, string> biomeList = new Dictionary<string, string>()
        {
            { "safe",                    "Safe Shallows" },
            { "kelpforest",                "Kelp Forest" },
            { "grassy",                "Grassy Plateaus" },
            { "mushroomforest",        "Mushroom Forest" },
            { "jellyshroomcaves",    "Jellyshroom Caves" },
            { "sparse",                    "Sparse Reef" },
            { "underwaterislands" , "Underwater Islands" },
            { "bloodkelp" ,            "Blood Kelp Zone" },
            { "dunes" ,                     "Sand Dunes" },
            { "crashzone" ,                 "Crash Zone" },
            { "grandreef" ,                 "Grand Reef" },
            { "mountains" ,                  "Mountains" },
            { "lostriver" ,                 "Lost River" },
            { "ilz" ,               "Inactive Lava Zone" },
            { "lava" ,                       "Lava Lake" },
            { "floatingisland" ,       "Floating Island" },
            { "koosh" ,                      "Bulb Zone" },
            { "seatreader" ,        "Sea Treader's Path" },
            { "crag" ,                      "Crag Field" },
            { "unassigned" ,                "Unassigned" },
            { "void" ,            "Ecological Dead Zone" },
            // Special-case biomes, may remove
            { "precursor" ,         "Precursor Facility" },
            { "prison" ,  "Primary Containment Facility" },
            { "shipspecial" ,                   "Aurora" },
            { "shipinterior",                   "Aurora" },
            { "crashhome" ,                     "Aurora" },
            { "aurora" ,                        "Aurora" },
            { "mesas" ,                           "Mesa" },
            { "observatory" ,              "Observatory" },
            { "generatorroom" ,         "Generator Room" },
            { "crashedship" ,                   "Aurora" },
        };
    }
}
