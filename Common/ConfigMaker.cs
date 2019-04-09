namespace Common
{
    using System;
    using System.Xml;
    using System.Xml.Serialization;
    using System.IO;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// SaveData gives you a common library to read and save XML files for configuration.
    /// </summary>
    public class ConfigMaker
    {
        public static void WriteData(string fileLocation, object dataToSave)
        {
            XmlSerializer serializer = new XmlSerializer(dataToSave.GetType());
            TextWriter writer = new StreamWriter(fileLocation);
            serializer.Serialize(writer, dataToSave);
            writer.Close();
        }

        public static object ReadData(string fileLocation, Type t)
        {
            XmlSerializer serializer = new XmlSerializer(t);
            FileStream fs = new FileStream(fileLocation, FileMode.Open);
            return serializer.Deserialize(fs);
        }
    }
}
