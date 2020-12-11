/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 * Utilities
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Oculus.Newtonsoft.Json;
using SMLHelper.V2.Utility;
using Common;

namespace DeathRun
{
    using global::DeathRun.Patchers;
    using UnityEngine;
    using UnityEngine.UI;

    public class DeathRunUtils
    {
        public static CenterText[] centerMessages = new CenterText[] {
            new CenterText(0, 250f),
            new CenterText(1, 210f),
            new CenterText(2, -210f),
            new CenterText(3, -250f)
        };

        public class CenterText
        {
            int id { get; set; } = 0;
            float y { get; set; } = 250f;
            ContentSizeFitter textFitter { get; set; } = null;
            public GameObject textObject { get; set; } = null;
            public Text textText { get; set; } = null;
            public uGUI_TextFade textFade { get; set; } = null;

            public CenterText(int set_id, float set_y)
            {
                id = set_id;
                y = set_y;
            }

            public void ShowMessage(String s, float seconds)
            {
                if (textFade == null)
                {
                    InitializeText();
                }

                textFade.SetText(s);
                textFade.SetState(true);
                textFade.FadeOut(seconds, null);
            }

            private void InitializeText()
            {
                // Make our own text object
                textObject = new GameObject("DeathRunText" + id);
                textText = textObject.AddComponent<Text>();          // The text itself
                textFade = textObject.AddComponent<uGUI_TextFade>(); // The uGUI's helpful automatic fade component

                // This makes the text box fit the text (rather than the other way around)
                textFitter = textObject.AddComponent<ContentSizeFitter>();
                textFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                textFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                // This clones the in game "Press Any Button To Begin" message's font size, style, etc.
                textText.font = uGUI.main.intro.mainText.text.font;
                textText.fontSize = uGUI.main.intro.mainText.text.fontSize;
                textText.alignment = uGUI.main.intro.mainText.text.alignment;
                textText.material = uGUI.main.intro.mainText.text.material;
                textText.color = uGUI.main.intro.mainText.text.color;

                // This puts the text OVER the black "you are dead" screen, so it will still show for a death message
                var go = uGUI.main.overlays.overlays[0].graphic;
                textObject.transform.SetParent(go.transform, false); // Parents our text to the black overlay
                textText.canvas.overrideSorting = true;              // Turn on canvas sort override so the layers will work
                textObject.layer += 100;                             // Set to a higher layer than the black overlay

                // Sets our text's location on screen
                textObject.transform.localPosition = new Vector3(0f, y, 0f);

                // Turns our text item on
                textObject.SetActive(true);
            }
        }


        /**
         *  CenterMessage - hijacks the "intro" text item (the "press any key to begin" message) to display general "large text messages".
         */
        public static void CenterMessage(String s, float seconds)
        {
            CenterMessage(s, seconds, 0);
        }

        /**
         *  CenterMessage - hijacks the "intro" text item (the "press any key to begin" message) to display general "large text messages".
         */
        public static void CenterMessage(String s, float seconds, int index)
        {
            centerMessages[index].ShowMessage(s, seconds);
        }


        /**
         *  When just loaded a game, don't restore some previous mid-screen giant message
         */
        public static void JustLoadedGame()
        {
            foreach (CenterText ct in centerMessages)
            {
                if (ct.textText)
                {
                    ct.textText.text = "";
                }
            }
        }


        /**
         *  isIntroStillGoing() -- returns true if we're still on "press any key to continue" or during intro cinematic
         */
        public static bool isIntroStillGoing ()
        {
            // This checks if we're holding on the "press any key to continue" screen
            if (Player.main != null)
            {
                Survival surv = Player.main.GetComponent<Survival>();
                if ((surv != null) && surv.freezeStats)
                {
                    return true;
                }
            }

            // Checks if opening animation is running
            if ((EscapePod.main.IsPlayingIntroCinematic() && EscapePod.main.IsNewBorn()))
            {
                return true;
            }
            return false;
        }

        public static string sayTime(TimeSpan time)
        {
            string result = "";
            bool any = false;

            if (time.Days > 0)
            {
                result += time.Days + " Days";
                any = true;
            }

            if (any || (time.Hours > 0))
            {
                if (any) result += ", ";
                result += time.Hours + " Hours";
                any = true;
            }

            if (any || (time.Minutes > 0))
            {
                if (any) result += ", ";
                result += time.Minutes + " Minutes";
                any = true;
            }

            if ((time.Days == 0) && (time.Hours == 0))
            {
                if (any) result += ", ";
                result += time.Seconds + " Seconds";
            }

            return result;
        }
    }

    /**
     * DeathRunSaveData - saves and restores data we want saved with the saved game.
     */
    public class DeathRunSaveData
    {
        public PlayerSaveData playerSave { get; set; } // Player (lives, duration) save data
        public NitroSaveData nitroSave { get; set; } // Nitrogen/Bends save data
        public PodSaveData podSave { get; set; }     // Escape Pod save data
        public StartSpot startSave { get; set; }     // Escape Pod start spot save data
        public CountdownSaveData countSave { get; set; } //Countdown save data

        public DeathRunSaveData()
        {
            playerSave = new PlayerSaveData();
            nitroSave  = new NitroSaveData();
            podSave    = new PodSaveData();
            startSave  = new StartSpot();
            countSave  = new CountdownSaveData();

            setDefaults();
        }

        public void setDefaults()
        {
            playerSave.setDefaults();
            nitroSave.setDefaults();
            podSave.setDefaults();
            countSave.setDefaults();
        }

        public void Save()
        {
            string saveDirectory = SaveUtils.GetCurrentSaveDataDir();

            DeathRun.saveData.countSave.AboutToSaveGame();
            DeathRun.saveData.playerSave.AboutToSaveGame();

            try
            {
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore, // Keeps our Vector3's etc from generating infinite references
                    //PreserveReferencesHandling = PreserveReferencesHandling.Objects
                };

                var saveDataJson = JsonConvert.SerializeObject(this, Formatting.Indented, settings);

                if (!Directory.Exists(saveDirectory))
                {
                    Directory.CreateDirectory(saveDirectory);
                }

                File.WriteAllText(Path.Combine(saveDirectory, DeathRun.SaveFile), saveDataJson);
            }
            catch (Exception e)
            {
                SeraLogger.GenericError(DeathRun.modName, e);
                SeraLogger.Message(DeathRun.modName, "Failed");
            }
        }

        public void Load() 
        {
            var path = Path.Combine(SaveUtils.GetCurrentSaveDataDir(), DeathRun.SaveFile);

            if (!File.Exists(path))
            {
                SeraLogger.Message(DeathRun.modName, "Death Run data not found - using defaults");
                setDefaults();
                return;
            }

            try
            {
                var save = File.ReadAllText(path);

                var jsonSerializerSettings = new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore,
                    //PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                };
               
                //var json = JsonConvert.DeserializeObject<DeathRunSaveData>(save, jsonSerializerSettings);
                //this.exampleString = json.exampleString;
                //this.exampleData = json.exampleData;

                DeathRun.saveData = JsonConvert.DeserializeObject<DeathRunSaveData>(save, jsonSerializerSettings);

                DeathRun.saveData.countSave.JustLoadedGame();
                DeathRun.saveData.playerSave.JustLoadedGame();

                //SeraLogger.Message(DeathRun.modName, "FROM SAVE1 countcurr=" + DeathRun.saveData.countSave.currTime + "   countprev="+DeathRun.saveData.countSave.prevTime);
                //SeraLogger.Message(DeathRun.modName, "FROM SAVE2 countcurr=" + DeathRun.countdownMonitor.currValue + "   countprev=" + DeathRun.countdownMonitor.prevValue);

                //SeraLogger.Message(DeathRun.modName, "FROM SAVE3 playercurr=" + DeathRun.saveData.playerSave.currTime + "   playerprev=" + DeathRun.saveData.playerSave.prevTime);
                //SeraLogger.Message(DeathRun.modName, "FROM SAVE4 playercurr=" + DeathRun.playerMonitor.currValue + "   playerprev=" + DeathRun.playerMonitor.prevValue);

                //SeraLogger.Message(DeathRun.modName, " NumDeaths:" + DeathRun.saveData.playerSave.numDeaths);
                //SeraLogger.Message(DeathRun.modName, " StartedGame:" + DeathRun.saveData.playerSave.startedGame);
                //SeraLogger.Message(DeathRun.modName, " SpanAtDeath:" + DeathRun.saveData.playerSave.spanAtDeath);
                //SeraLogger.Message(DeathRun.modName, " TimeOfDeath:" + DeathRun.saveData.playerSave.timeOfDeath);


                // Special escape-pod re-adjustments
                EscapePod_FixedUpdate_Patch.JustLoadedGame();

                DeathRunUtils.JustLoadedGame();

                //string text = "Player: " + DeathRun.saveData.playerSave.timeMonitor.currValue + " / " + DeathRun.saveData.playerSave.timeMonitor.prevValue;
                //ErrorMessage.AddMessage(text);
                //SeraLogger.Message(DeathRun.modName, text);
                //       text = "Count: " + DeathRun.saveData.countSave.timeMonitor.currValue + " / " + DeathRun.saveData.countSave.timeMonitor.prevValue;
                //ErrorMessage.AddMessage(text);
                //SeraLogger.Message(DeathRun.modName, text);
            }
            catch (Exception e)
            {
                SeraLogger.GenericError(DeathRun.modName, e);
                SeraLogger.Message(DeathRun.modName, "Death Run data not found - using defaults");
                SeraLogger.Message(DeathRun.modName, e.StackTrace);
                setDefaults();
            }
        }
    }


    /**
     * DeathRunSaveListener - we add one of these to the Escape Pod (just to have a game object), and it listens for when the game is
     * saved and loaded so that we can do our save/load stuff too.
     */
    public class DeathRunSaveListener : MonoBehaviour, IProtoEventListener
    {
        public void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            DeathRun.saveData.Load();
        }

        public void OnProtoSerialize(ProtobufSerializer serializer)
        {
            DeathRun.saveData.Save();
        }
    }

    /**
     * Transformations that can be inexpensively copied (since the = operator in c# copies the reference not the object, and
     * copying a "real" Transform requires instantiating a game object: blarg)
     */
    public class Trans
    {
        public Vector3 position { get; set; }
        public Quaternion rotation { get; set; }
        public Vector3 localScale { get; set; }

        public Trans(Vector3 newPosition, Quaternion newRotation, Vector3 newLocalScale)
        {
            position = newPosition;
            rotation = newRotation;
            localScale = newLocalScale;
        }

        public Trans()
        {
            position = Vector3.zero;
            rotation = Quaternion.identity;
            localScale = Vector3.one;
        }

        public Trans(Transform transform)
        {
            copyFrom(transform);
        }

        public void copyFrom(Transform transform)
        {
            position = transform.position;
            rotation = transform.rotation;
            localScale = transform.localScale;
        }

        public void copyFrom(Trans trans)
        {
            position = trans.position;
            rotation = trans.rotation;
            localScale = trans.localScale;
        }

        public void copyTo(Transform transform)
        {
            transform.position = position;
            transform.rotation = rotation;
            transform.localScale = localScale;
        }

        public void copyTo(Trans trans)
        {
            trans.position = position;
            trans.rotation = rotation;
            trans.localScale = localScale;
        }
    }
}