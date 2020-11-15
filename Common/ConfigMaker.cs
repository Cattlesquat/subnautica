namespace Common
{
    using System;
    using System.Xml.Serialization;
    using System.IO;

    /// <summary>
    /// SaveData gives you a common library to read and save XML files for configuration.
    /// </summary>
    public class ConfigMaker
    {
        public static void WriteData(string file, object dataToSave)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(dataToSave.GetType());
                TextWriter writer = new StreamWriter(file);
                serializer.Serialize(writer, dataToSave);
                writer.Close();
            }
            catch (Exception ex)
            {
                SeraLogger.SeralizerFailed(file, ex);
            }
        }

        public static object ReadData(string file, Type t)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(t);
                FileStream fs = new FileStream(file, FileMode.Open);
                return serializer.Deserialize(fs);
            }
            catch (Exception ex)
            {
                SeraLogger.SeralizerFailed(file, ex);
                return null;
            }
        }
    }
}
