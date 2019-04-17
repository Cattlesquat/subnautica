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

    class BiomeDisplay : MonoBehaviour
    {
        private static BiomeDisplay main;

        public Text biomeMessage;
        public RectTransform biomeCanvas;
        public Vector2 offset = new Vector2(140f, 140f);
        private float ySpacing = 10f;
        private float fadeTime = 1f;
        private float fadeDelay = 5f;

        [AssertNotNull] private List<BiomeDisplay._BiomeMessage> messages = new List<BiomeDisplay._BiomeMessage>();
        private const int poolChunkSize = 4;
        [AssertNotNull] private List<Text> pool = new List<Text>();

        private class _BiomeMessage
        {
            public Text biomeEntry;
            public string biomeText;
            public int num;
            public float timeToDelete;
        }

        private void Awake()
        {
            Utils.Assert(BiomeDisplay.main == null, "see log", null);
            BiomeDisplay.main = this;
            SeraLogger.Message(Main.modName, "BiomeDisplay.Awake() called.");
        }

        private void Update()
        {
            for (int i = 0; i < this.messages.Count; i++)
            {
                BiomeDisplay._BiomeMessage message = this.messages[i];
                if (message.timeToDelete <= Time.time)
                {
                    this.DeleteMessage(message);
                    break;
                }
            }
        }

        public static void DisplayBiome(string message)
        {
            if (BiomeDisplay.main != null)
            {
                BiomeDisplay.main._AddMessage(message);
            }
        }

        [SuppressMessage("Subnautica.Rules", "AvoidBoxingRule")]
        private void _AddMessage(string messageText)
        {
            if (string.IsNullOrEmpty(messageText))
            {
                return;
            }
            BiomeDisplay._BiomeMessage existingMessage = this.GetExistingMessage(messageText);
            if (existingMessage == null)
            {
                Rect rect = this.biomeCanvas.rect;
                Text entry = this.GetEntry();
                GameObject gameObject = entry.gameObject;
                gameObject.SetActive(true);
                RectTransform rectTransform = entry.rectTransform;
                entry.text = messageText;
                rectTransform.localPosition = new Vector3(rect.x + this.offset.x, -rect.y + this.GetTargetHeight(-1), 1f);
                iTween.MoveFrom(gameObject, iTween.Hash(new object[]
                {
                "x",
                -0.5f * Mathf.Min(rectTransform.rect.width, entry.preferredWidth),
                "y",
                0.5f * entry.preferredHeight,
                "time",
                0.3f,
                "islocal",
                true
                }));
                BiomeDisplay._BiomeMessage message = new BiomeDisplay._BiomeMessage();
                message.biomeEntry = entry;
                message.biomeText = messageText;
                message.num = 1;
                message.timeToDelete = Time.time + this.fadeTime + this.fadeDelay;
                this.messages.Add(message);
            }
            else
            {
                Text entry2 = existingMessage.biomeEntry;
                existingMessage.timeToDelete = Time.time + this.fadeTime + this.fadeDelay;
                existingMessage.num++;
                entry2.text = string.Format("{0} (x{1})", messageText, existingMessage.num);
            }
        }

        private BiomeDisplay._BiomeMessage GetExistingMessage(string messageText)
        {
            BiomeDisplay._BiomeMessage result = null;
            foreach (BiomeDisplay._BiomeMessage message in this.messages)
            {
                if (message.biomeText == messageText)
                {
                    result = message;
                    break;
                }
            }
            return result;
        }

        private void DeleteMessage(BiomeDisplay._BiomeMessage message)
        {
            this.messages.Remove(message);
            this.ReleaseEntry(message.biomeEntry);
            Rect rect = this.biomeCanvas.rect;
            int i = 0;
            int count = this.messages.Count;
            while (i < count)
            {
                BiomeDisplay._BiomeMessage message2 = this.messages[i];
                iTween.MoveTo(message2.biomeEntry.gameObject, iTween.Hash(new object[]
                {
                "x",
                rect.x + this.offset.x,
                "y",
                -rect.y + this.GetTargetHeight(i),
                "time",
                0.5f,
                "islocal",
                true
                }));
                i++;
            }
        }

        private float GetTargetHeight(int index = -1)
        {
            if (index == -1)
            {
                index = this.messages.Count;
            }
            float num = 0f;
            int i = 0;
            int num2 = Mathf.Min(this.messages.Count, index);
            while (i < num2)
            {
                BiomeDisplay._BiomeMessage message = this.messages[i];
                Text entry = message.biomeEntry;
                num += entry.preferredHeight;
                i++;
            }
            return -(this.offset.y + num + (float)index * this.ySpacing);
        }

        private Text GetEntry()
        {
            Text text;
            if (this.pool.Count == 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.biomeMessage.gameObject);
                    text = gameObject.GetComponent<Text>();
                    text.rectTransform.SetParent(this.biomeCanvas, false);
                    gameObject.SetActive(false);
                    this.pool.Add(text);
                }
            }
            int index = this.pool.Count - 1;
            text = this.pool[index];
            this.pool.RemoveAt(index);
            return text;
        }

        private void ReleaseEntry(Text entry)
        {
            if (entry == null)
            {
                return;
            }
            entry.gameObject.SetActive(false);
            this.pool.Add(entry);
        }
    }
}
