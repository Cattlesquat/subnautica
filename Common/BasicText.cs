/**
 * Basic Text object -- Cattlesquat
 * 
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Oculus.Newtonsoft.Json;
using SMLHelper.V2.Utility;
using Common;
using UnityEngine;
using UnityEngine.UI;

namespace Common
{
    public class BasicText
    {
        protected float x { get; set; } = 0;          // X position anchor
        protected float y { get; set; } = 210f;       // Y position anchor (defaults to a comfortable centered about 1/3 from top of screen)
        protected bool cloneAlign { get; set; }       // True if we're cloning Subnautica's "Press Any Button To Begin" alignment
        protected bool cloneColor { get; set; }       // True if we're cloning Subnautica's "Press Any Button To Begin" color
        protected bool cloneSize { get; set; }        // True if we're cloning Subnautica's "Press Any Button To Begin" fontsize
        protected TextAnchor align { get; set; }      // text alignment
        protected Color color { get; set; }           // text color
        protected int size { get; set; }              // text size
        protected GameObject textObject { get; set; } = null;          // Our game object
        protected uGUI_TextFade textFade { get; set; } = null;         // Our text fader
        protected Text textText { get; set; } = null;                  // Our text object
        protected ContentSizeFitter textFitter { get; set; } = null;   // Our content size fitter

        static int index = 0; // For giving unique names to the game objects

        public BasicText()
        {
            cloneAlign = true;
            cloneColor = true;
            cloneSize = true;
        }

        public BasicText(int set_x, int set_y) : this()
        {
            x = set_x;
            y = set_y;
        }

        public BasicText(TextAnchor useAlign) : this()
        {
            cloneAlign = false;
            align = useAlign;
        }

        public BasicText(Color useColor) : this()
        {
            cloneColor = false;
            color = useColor;
        }

        public BasicText(int useSize) : this()
        {
            cloneSize = false;
            size = useSize;
        }

        public BasicText(int useSize, Color useColor) : this()
        {
            cloneColor = false;
            color = useColor;
            cloneSize = false;
            size = useSize;
        }

        public BasicText(int useSize, TextAnchor useAlign) : this()
        {
            cloneAlign = false;
            align = useAlign;
            cloneSize = false;
            size = useSize;
        }

        public BasicText(int useSize, Color useColor, TextAnchor useAlign) : this()
        {
            cloneAlign = false;
            align = useAlign;
            cloneColor = false;
            color = useColor;
            cloneSize = false;
            size = useSize;
        }


        public BasicText(int set_x, int set_y, int useSize, Color useColor, TextAnchor useAlign) : this()
        {
            x = set_x;
            y = set_y;
            cloneAlign = false;
            align = useAlign;
            cloneColor = false;
            color = useColor;
            cloneSize = false;
            size = useSize;
        }

        public BasicText(int set_x, int set_y, int useSize, Color useColor) : this()
        {
            x = set_x;
            y = set_y;
            cloneColor = false;
            color = useColor;
            cloneSize = false;
            size = useSize;
        }

        public BasicText(int set_x, int set_y, int useSize) : this()
        {
            x = set_x;
            y = set_y;
            cloneSize = false;
            size = useSize;
        }

        /**
         * Returns our current text
         */
        public string getText()
        {
            if (textObject == null)
            {
                return "";
            }
            return textText.text;
        }

        /**
         * Sets text color
         */
        public void setColor(Color useColor)
        {
            cloneAlign = false;
            color = useColor;

            if (textObject != null)
            {
                textText.color = color;
            }
        }

        /**
         * Resets to using "cloned" color of Subnautica default
         */
        public void clearColor()
        {
            cloneColor = true;
            if (textObject != null)
            {
                textText.color = uGUI.main.intro.mainText.text.color;
            }
        }

        /**
         * Sets the font size
         */
        public void setSize(int useSize)
        {
            cloneSize = false;
            size = useSize;
            if (textObject != null)
            {
                textText.fontSize = size;
            }
        }

        /**
         * Resets to using "cloned" size of Subnautica default
         */
        public void clearSize()
        {
            cloneSize = true;
            if (textObject != null)
            {
                textText.fontSize = uGUI.main.intro.mainText.text.fontSize;
            }
        }


        /**
         * Sets screen display location (position relative to the actual text is determined by the alignment)
         */
        public void setLoc(float set_x, float set_y)
        {
            x = set_x;
            y = set_y;
            doAlignment();
        }

        /**
         * Computes proper transform position based on alignment & size of text.
         */
        private void doAlignment()
        {
            if (textObject == null)
            {
                return;
            }

            float width = textText.preferredWidth;
            float height = textText.preferredHeight;

            float displayX, displayY;

            switch (textText.alignment)
            {
                case TextAnchor.UpperLeft:
                case TextAnchor.MiddleLeft:
                case TextAnchor.LowerLeft:
                    displayX = x + width / 2;
                    break;

                case TextAnchor.UpperRight:
                case TextAnchor.MiddleRight:
                case TextAnchor.LowerRight:
                    displayX = x - width / 2;
                    break;

                default:
                    displayX = x;
                    break;
            }

            switch (textText.alignment)
            {
                case TextAnchor.UpperLeft:
                case TextAnchor.UpperCenter:
                case TextAnchor.UpperRight:
                    displayY = y - height / 2;
                    break;

                case TextAnchor.LowerLeft:
                case TextAnchor.LowerCenter:
                case TextAnchor.LowerRight:
                    displayY = y - height / 2;
                    break;

                default:
                    displayY = y;
                    break;
            }

            textObject.transform.localPosition = new Vector3(displayX, displayY, 0f);
        }

        /**
         * Hides our text item if it is displaying
         */
        public void Hide()
        {
            if (textObject == null)
            {
                return;
            }

            textFade.SetState(false);
            textObject.SetActive(false);
        }

        /**
         * Shows our text item, fading after a specified number of seconds (or stays on indefinitely if 0 seconds)
         */
        public void ShowMessage(String s, float seconds)
        {
            if (textObject == null)
            {
                InitializeText();
            }

            textFade.SetText(s);
            textFade.SetState(true);
            textObject.SetActive(true);
            if (seconds > 0) textFade.FadeOut(seconds, null);
        }

        /**
         * Shows our text item, with no schedule fade (i.e. indefinitely)
         */
        public void ShowMessage(String s)
        {
            ShowMessage(s, 0);
        }

        /**
         * Sets up all of our objects/components, when we are ready to actually display text for the first time.
         */
        private void InitializeText()
        {
            // Make our own text object
            textObject = new GameObject("BasicText" + (++index));
            textText = textObject.AddComponent<Text>();          // The text itself
            textFade = textObject.AddComponent<uGUI_TextFade>(); // The uGUI's helpful automatic fade component           

            // This makes the text box fit the text (rather than the other way around)
            textFitter = textObject.AddComponent<ContentSizeFitter>();
            textFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            textFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // This clones the in game "Press Any Button To Begin" message's font size, style, etc.
            textText.font = uGUI.main.intro.mainText.text.font;
            textText.fontSize = cloneSize ? uGUI.main.intro.mainText.text.fontSize : size;
            textText.alignment = cloneAlign ? uGUI.main.intro.mainText.text.alignment : align;
            textText.material = uGUI.main.intro.mainText.text.material;
            textText.color = cloneColor ? uGUI.main.intro.mainText.text.color : color;

            // This puts the text OVER the black "you are dead" screen, so it will still show for a death message
            var go = uGUI.main.overlays.overlays[0].graphic;
            textObject.transform.SetParent(go.transform, false); // Parents our text to the black overlay
            textText.canvas.overrideSorting = true;              // Turn on canvas sort override so the layers will work
            textObject.layer += 100;                             // Set to a higher layer than the black overlay

            // Sets our text's location on screen
            doAlignment();
            //textObject.transform.localPosition = new Vector3(x, y, 0f);

            // Turns our text item on
            textObject.SetActive(true);
        }
    }
}