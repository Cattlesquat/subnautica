﻿/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 * This section adapted from Seraphim Risen's NitrogenMod - here I revised the text displayed on the HUD to have some additional feedback modes (see the SetDepth method)
 */
namespace DeathRun.NMBehaviours
{
    using UnityEngine;
    using UnityEngine.UI;

    class BendsHUDController : MonoBehaviour
    {
        private static BendsHUDController main;

        public GameObject _N2HUDWarning { private get; set; }
        public Transform hudTransform;

        private Transform canvasTransform;
        private Text n2Warning;
        private Text n2Depth;
        private Animator flashRed;

        private void Awake()
        {
            _N2HUDWarning = Instantiate<GameObject>(DeathRun.N2HUD);

            canvasTransform = _N2HUDWarning.transform;
            n2Warning = canvasTransform.GetChild(0).GetComponent<Text>();
            n2Depth = canvasTransform.GetChild(1).GetComponent<Text>();
            flashRed = n2Warning.GetComponent<Animator>();

            n2Warning.enabled = false;
            n2Depth.enabled = false;
            flashRed.SetBool("unsafe", false);

            hudTransform = GameObject.Find("ScreenCanvas").transform.Find("HUD");
            canvasTransform.SetParent(hudTransform, false);
            canvasTransform.SetSiblingIndex(0);

            main = this;
        }

        private void Update()
        {

        }

        public static void SetActive(bool setActive, bool setWarning)
        {
            if (main == null)
                return;
            main.n2Warning.enabled = setActive && setWarning;
            main.n2Depth.enabled = setActive;
        }

        public static void SetFlashing(bool setFlashing)
        {
            if (main == null)
                return;
            main.flashRed.SetBool("unsafe", setFlashing);
        }

        public static void SetDepth(int safeDepth, float n2percent)
        {
            if (main == null)
                return;

            if ((n2percent >= 100) && (safeDepth >= 10))
            {
                main.n2Depth.text = safeDepth + "m";

                main.n2Depth.color = Color.white;
                if (!Player.main.IsSwimming())
                {
                    main.n2Depth.text += " *";
                }
                else if (DeathRun.saveData.nitroSave.atPipe)
                {
                    if (DeathRun.saveData.nitroSave.pipeTime > DeathRun.saveData.nitroSave.bubbleTime)
                    {
                        main.n2Depth.text += " P*";
                    } else
                    {
                        main.n2Depth.text += " B*";
                    }

                    main.n2Depth.color = Color.cyan;
                }

                int depth = (int)Ocean.main.GetDepthOf(Player.main.gameObject);
                if (depth < safeDepth)
                {
                    main.n2Depth.color = Color.red;
                } else if (depth < safeDepth + 3)
                {
                    main.n2Depth.color = Color.yellow;
                }

            }
            else
            {
                main.n2Depth.text = Mathf.RoundToInt(n2percent) + "%";

                main.n2Depth.color = Color.white;

                if (DeathRun.saveData.nitroSave.atPipe)
                {
                    main.n2Depth.color = Color.cyan;
                }
            }
        }
    }
}
