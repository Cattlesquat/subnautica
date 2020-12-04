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
        static GameObject textObject = null;
        static ContentSizeFitter textFitter = null;
        static Text textText = null;
        static uGUI_TextFade textFade = null;

        /**
         *  CenterMessage - hijacks the "intro" text item (the "press any key to begin" message) to display general "large text messages".
         */
        public static void CenterMessage(String s, float seconds)
        {
            if (textFade == null)
            {
                InitializeText();
            }

            textFade.SetText(s);
            textFade.SetState(true);
            textFade.FadeOut(seconds, null);

            //uGUI.main.intro.mainText.SetText(s);
            //uGUI.main.intro.mainText.transform.localPosition = new Vector3(0f, 250f, 0f);
            //uGUI.main.intro.mainText.SetState(true);
            //uGUI.main.intro.mainText.FadeOut(seconds, null);
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


        static private void InitializeText ()
        {
            // Make our own text object
            textObject = new GameObject("DeathRunText");
            textText = textObject.AddComponent<Text>();
            textFade = textObject.AddComponent<uGUI_TextFade>();

            textFitter = textObject.AddComponent<ContentSizeFitter>();
            textFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            textFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // This clones the in game "Press Any Button To Begin" message
            textText.font = uGUI.main.intro.mainText.text.font;
            textText.fontSize = uGUI.main.intro.mainText.text.fontSize;
            textText.alignment = uGUI.main.intro.mainText.text.alignment;
            textText.material = uGUI.main.intro.mainText.text.material;
            textText.color = uGUI.main.intro.mainText.text.color;

            // This puts the text OVER the "you are dead" screen, so it will still show for a death message
            var go = uGUI_PlayerDeath.main.blackOverlay;
            textObject.transform.SetParent(go.transform, false);
            textObject.layer++;

            // Location on screen
            textObject.transform.localPosition = new Vector3(0f, 250f, 0f);

            textObject.SetActive(true);
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

            SeraLogger.Message(DeathRun.modName, "Save DeathRun " + saveDirectory);

            try
            {
                SeraLogger.Message(DeathRun.modName, "settings");
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore, // Keeps our Vector3's etc from generating infinite references
                    //PreserveReferencesHandling = PreserveReferencesHandling.Objects
                };

                SeraLogger.Message(DeathRun.modName, "serialize");
                var saveDataJson = JsonConvert.SerializeObject(this, Formatting.Indented, settings);

                SeraLogger.Message(DeathRun.modName, "directory");
                if (!Directory.Exists(saveDirectory))
                {
                    Directory.CreateDirectory(saveDirectory);
                }

                SeraLogger.Message(DeathRun.modName, "Save DeathRun: " + Path.Combine(saveDirectory, DeathRun.SaveFile));
                SeraLogger.Message(DeathRun.modName, saveDataJson);

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

            SeraLogger.Message(DeathRun.modName, "Load DeathRun");

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

                // Special escape-pod re-adjustments
                EscapePod_FixedUpdate_Patch.JustLoadedGame();

                SeraLogger.Message(DeathRun.modName, "Last Depth = " + DeathRun.saveData.podSave.lastDepth);
                SeraLogger.Message(DeathRun.modName, "Pod Anchored = " + DeathRun.saveData.podSave.podAnchored);
                SeraLogger.Message(DeathRun.modName, "Pod Sinking = " + DeathRun.saveData.podSave.podSinking);
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
            SeraLogger.Message(DeathRun.modName, "Load Listener");
            DeathRun.saveData.Load();
        }

        public void OnProtoSerialize(ProtobufSerializer serializer)
        {
            SeraLogger.Message(DeathRun.modName, "Save Listener");
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