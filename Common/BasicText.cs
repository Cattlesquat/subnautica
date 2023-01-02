﻿/**
 * BasicText -- Cattlesquat
 * 
 * Places a simple text object on the screen and keeps it there until either hidden (or a designated fade-to-black timer has passed). 
 * By default uses the same font/size/color as the "Press Any Button To Begin" message at the beginning of the game, and appears 
 * centered about 1/3 down the screen, but all parameters can be reconfigured.
 * 
 * The idea of the defaults is that new modders don't have to bootstrap a bunch of irritating Unity stuff -- don't have to understand
 * what a "Material" is or how to make one, don't have to know to initialize a font, or even a color. Can just start adding text and
 * then can always custom & configure on further revision.
 * 
 * SIMPLE USAGE EXAMPLE:
 * BasicText message = new BasicText();
 * message.ShowMessage("This Message Will Fade In 10 Seconds", 10);
 * 
 * COMPLEX USAGE EXAMPLE:
 * BasicText message = new BasicText(TextAnchor.UpperLeft); // Note many other properties could also be set as constructor parameters
 * message.setColor(Color.red); // Set Color
 * message.setSize(20);         // Set Font Size
 * message.setLoc(200, 400);    // Set x/y position (0,0 is center of screen)
 * message.setFontStyle(FontStyle.Bold); // Bold 
 * message.ShowMessage("This message stays on screen until hidden"); // Display message; if fadeout seconds not specified, it just keeps showing
 * ... // other things happen, time goes by
 * message.Hide(); // Hides the message
 */
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Common
{
    public class BasicText
    {
        public BasicText()
        {
            cloneAlign = true;
            cloneColor = true;
            cloneSize = true;
            cloneFont = true;
            cloneStyle = true;
            cloneMaterial = true;
            overBlackScreen = false;
        }

        public BasicText(bool overBlack) : this()
        {            
            overBlackScreen = overBlack;
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
         * Shows our text item, with no schedule fade (i.e. indefinitely)
         */
        public void ShowMessage(string s)
        {
            ShowMessage(s, 0);
        }

        /**
         * Shows our text item, fading after a specified number of seconds (or stays on indefinitely if 0 seconds)
         */
        public void ShowMessage(string s, float seconds)
        {
            if (textObject == null)
            {
                // First time only, initialize the object and components
                InitializeText();
            }

            // Set our actual text
            textFade.SetText(s);

            // Sets our location on the screen
            doAlignment();

            // Turns our text item on
            textFade.SetState(true);
            textObject.SetActive(true);

            // If specified, sets the fade-out timer
            if (seconds > 0) textFade.FadeOut(seconds, null);
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
         * Sets screen display location (position relative to the actual text is determined by the alignment)
         */
        public void setLoc(float set_x, float set_y)
        {
            x = set_x;
            y = set_y;
            doAlignment();
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
                doAlignment();
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
                textText.fontSize = (int) uGUI.main.intro.mainText.text.fontSize;
                doAlignment();
            }
        }

        /**
         * Sets the font 
         */
        public void setFont(TMP_FontAsset useFont)
        {
            cloneFont = false;
            font = useFont;
            if (textObject != null)
            {
                textText.font = font;
                doAlignment();
            }
        }

        /**
         * Resets to using "cloned" font of Subnautica default
         */
        public void clearFont()
        {
            cloneFont = true;
            if (textObject != null)
            {
                textText.font = uGUI.main.intro.mainText.text.font;
                doAlignment();
            }
        }

        /**
         * Sets the font style
         */
        public void setFontStyle(FontStyle useStyle)
        {
            cloneStyle = false;
            style = useStyle;
            if (textObject != null)
            {
                textText.fontStyle = (FontStyles) style;
                doAlignment();
            }
        }

        /**
         * Resets to using "cloned" font style of Subnautica default
         */
        public void clearFontStyle()
        {
            cloneStyle = true;
            if (textObject != null)
            {
                textText.fontStyle = uGUI.main.intro.mainText.text.fontStyle;
            }
        }

        /**
         * Sets the font style
         */
        public void setAlign(TextAnchor useAlign)
        {
            cloneAlign = false;
            align = useAlign;
            if (textObject != null)
            {
                textText.alignment = (TextAlignmentOptions) align;
                doAlignment();
            }
        }

        /**
         * Resets to using "cloned" font style of Subnautica default
         */
        public void clearAlign()
        {
            cloneAlign = true;
            if (textObject != null)
            {
                textText.alignment = uGUI.main.intro.mainText.text.alignment;
                doAlignment();
            }
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
                case TextAlignmentOptions.TopLeft:
                case TextAlignmentOptions.Left:
                case TextAlignmentOptions.BottomLeft:
                    displayX = x + width / 2;
                    break;

                case TextAlignmentOptions.TopRight:
                case TextAlignmentOptions.Right:
                case TextAlignmentOptions.BottomRight:
                    displayX = x - width / 2;
                    break;

                default:
                    displayX = x;
                    break;
            }

            switch (textText.alignment)
            {
                case TextAlignmentOptions.TopLeft:
                case TextAlignmentOptions.Top:
                case TextAlignmentOptions.TopRight:
                    displayY = y - height / 2;
                    break;

                case TextAlignmentOptions.BottomLeft:
                case TextAlignmentOptions.Bottom:
                case TextAlignmentOptions.BottomRight:
                    displayY = y + height / 2;
                    break;

                default:
                    displayY = y;
                    break;
            }

            textObject.transform.localPosition = new Vector3(displayX, displayY, 0f);
        }

        /**
         * Sets up all of our objects/components, when we are ready to actually display text for the first time.
         */
        private void InitializeText()
        {
            // Make our own text object
            textObject = new GameObject("BasicText" + (++index));
            textText = textObject.AddComponent<TextMeshProUGUI>();          // The text itself
            textFade = textObject.AddComponent<uGUI_TextFade>(); // The uGUI's helpful automatic fade component           

            // This makes the text box fit the text (rather than the other way around)
            textFitter = textObject.AddComponent<ContentSizeFitter>();
            textFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            textFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // This clones the in game "Press Any Button To Begin" message's font size, style, etc.
            textText.font = cloneFont ? uGUI.main.intro.mainText.text.font : font;
            textText.fontSize = cloneSize ? (int) uGUI.main.intro.mainText.text.fontSize : size;
            textText.fontStyle = cloneStyle ? uGUI.main.intro.mainText.text.fontStyle : (FontStyles) style;
            textText.alignment = cloneAlign ? uGUI.main.intro.mainText.text.alignment : (TextAlignmentOptions) align;
            textText.color = cloneColor ? uGUI.main.intro.mainText.text.color : color;
            textText.material = cloneMaterial ? uGUI.main.intro.mainText.text.material : material;

            // This puts the text OVER the black "you are dead" screen, so it will still show for a death message
            // One could of course get into parenting this object to different things, in different layers, but for
            // basic purposes we're creating text that will "definitely show up".
            if (overBlackScreen)
            {
                var go = uGUI.main.overlays.overlays[0].graphic;
                textObject.transform.SetParent(go.transform, false); // Parents our text to the black overlay
                textText.canvas.overrideSorting = true;              // Turn on canvas sort override so the layers will work                    
                textObject.layer++;                                  // Set to a higher layer than the black overlay
            }
            else
            {
                textObject.transform.SetParent(uGUI.main.screenCanvas.transform, false); // Parents our text to the black overlay
                textText.canvas.overrideSorting = true;              // Turn on canvas sort override so the layers will work                    
                textObject.layer = 31;                               // Set to what seems to be Subnautica's highest layer
            }

            doAlignment();
        }

        protected float x { get; set; } = 0;          // X position anchor
        protected float y { get; set; } = 210f;       // Y position anchor (defaults to a comfortable centered about 1/3 from top of screen)
        protected bool overBlackScreen { get; set; }  // True if should forced be over the "death" screen
        protected bool cloneAlign { get; set; }       // True if we're cloning Subnautica's "Press Any Button To Begin" alignment
        protected bool cloneColor { get; set; }       // True if we're cloning Subnautica's "Press Any Button To Begin" color
        protected bool cloneSize { get; set; }        // True if we're cloning Subnautica's "Press Any Button To Begin" fontsize
        protected bool cloneFont { get; set; }        // True if we're cloning Subnautica's "Press Any Button To Begin" font
        protected bool cloneStyle { get; set; }       // True if we're cloning Subnautica's "Press Any Button To Begin" font style
        protected bool cloneMaterial { get; set; }    // True if we're cloning Subnautica's "Press Any Button To Begin" material
        protected TextAnchor align { get; set; }      // text alignment
        protected Color color { get; set; }           // text color
        protected int size { get; set; }              // text size
        protected TMP_FontAsset font { get; set; }             // text font
        protected FontStyle style { get; set; }       // text font style
        protected Material material { get; set; }     // text material
        protected GameObject textObject { get; set; } = null;          // Our game object
        protected uGUI_TextFade textFade { get; set; } = null;         // Our text fader
        protected TextMeshProUGUI textText { get; set; } = null;                  // Our text object
        protected ContentSizeFitter textFitter { get; set; } = null;   // Our content size fitter

        static int index = 0; // For giving unique names to the game objects
    }
}