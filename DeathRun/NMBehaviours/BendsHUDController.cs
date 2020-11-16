namespace DeathRun.NMBehaviours
{
    using UnityEngine;
    using UnityEngine.UI;

    //
    // Much of Nitrogen/Bends and Crush-Depth code adapted from SeraphimRisen's NitrogenMod
    //

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
            _N2HUDWarning = Instantiate<GameObject>(Main.N2HUD);

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

            ErrorMessage.AddMessage("Added HUD Controller");

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
            }
            else
            {
                main.n2Depth.text = Mathf.RoundToInt(n2percent) + "%";
            }
        }
    }
}
