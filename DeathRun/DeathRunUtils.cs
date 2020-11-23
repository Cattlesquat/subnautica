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
    using UnityEngine;

    public class DeathRunUtils
    {
        /**
         *  CenterMessage - hijacks the "intro" text item (the "press any key to begin" message) to display general "large text messages".
         */
        public static void CenterMessage(String s, float seconds)
        {
            uGUI.main.intro.mainText.SetText(s);
            uGUI.main.intro.mainText.transform.localPosition = new Vector3(0f, 250f, 0f);
            uGUI.main.intro.mainText.SetState(true);
            uGUI.main.intro.mainText.FadeOut(seconds, null);
        }
    }

    /**
     * DeathRunSaveData - saves and restores data we want saved with the saved game.
     */
    public class DeathRunSaveData
    {
        public int exampleData { get; set; }
        public string exampleString { get; set; }

        public DeathRunSaveData()
        {
            setDefaults();
        }

        public void setDefaults()
        {
            exampleData = 1;
            exampleString = "Default";
        }

        public void Save()
        {
            string saveDirectory = SaveUtils.GetCurrentSaveDataDir();

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
            };

            var saveDataJson = JsonConvert.SerializeObject(this, Formatting.Indented, settings);

            if (!Directory.Exists(saveDirectory))
            {
                Directory.CreateDirectory(saveDirectory);
            }

            File.WriteAllText(Path.Combine(saveDirectory, Main.SaveFile), saveDataJson);
        }

        public void Load() 
        {
            var path = Path.Combine(SaveUtils.GetCurrentSaveDataDir(), Main.SaveFile);

            if (!File.Exists(path))
            {
                SeraLogger.Message(Main.modName, "Death Run data not found - using defaults");
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
                };

                //var json = JsonConvert.DeserializeObject<DeathRunSaveData>(save, jsonSerializerSettings);
                //this.exampleString = json.exampleString;
                //this.exampleData = json.exampleData;

                Main.saveData = JsonConvert.DeserializeObject<DeathRunSaveData>(save, jsonSerializerSettings);
            }
            catch (Exception e)
            {
                SeraLogger.GenericError(Main.modName, e);
                SeraLogger.Message(Main.modName, "Death Run data not found - using defaults");
                SeraLogger.Message(Main.modName, e.StackTrace);
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
            Main.saveData.Load();
        }

        public void OnProtoSerialize(ProtobufSerializer serializer)
        {
            Main.saveData.Save();
        }
    }

    /**
     * Transformations that can be inexpensively copied (since the = operator in c# copies the reference not the object, and
     * copying a "real" Transform requires instantiating a game object: blarg)
     */
    public class Trans
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 localScale;

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