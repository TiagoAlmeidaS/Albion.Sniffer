using System.Xml.Serialization;

namespace AlbionOnlineSniffer.Core.Models.Dependencies
{
    public class XmlTools
    {
        public static T Deserialize<T>(string filePath)
        {
            var serializer = new XmlSerializer(typeof(T));
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
            {
                return (T)serializer.Deserialize(fileStream);
            }
        }
    }
}