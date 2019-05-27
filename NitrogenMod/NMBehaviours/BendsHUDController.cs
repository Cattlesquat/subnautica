namespace NitrogenMod.NMBehaviours
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
        private Animator flashRed;

        private void Awake()
        {
            _N2HUDWarning = Instantiate<GameObject>(Main.N2HUD);

            canvasTransform = _N2HUDWarning.transform;
            n2Warning = canvasTransform.GetChild(0).GetComponent<Text>();
            flashRed = n2Warning.GetComponent<Animator>();

            n2Warning.enabled = false;
            flashRed.SetBool("unsafe", false);

            hudTransform = GameObject.Find("ScreenCanvas").transform.Find("HUD");
            canvasTransform.SetParent(hudTransform, false);
            canvasTransform.SetSiblingIndex(0);

            main = this;
        }

        private void Update()
        {

        }

        public static void SetActive(bool setActive)
        {
            if (main == null)
                return;
            main.n2Warning.enabled = setActive;
        }

        public static void SetFlashing(bool setFlashing)
        {
            if (main == null)
                return;
            main.flashRed.SetBool("unsafe", setFlashing);
        }
    }
}
