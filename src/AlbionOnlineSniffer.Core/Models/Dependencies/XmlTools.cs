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
                var result = serializer.Deserialize(fileStream);
                if (result == null)
                    throw new InvalidOperationException($"Falha ao deserializar arquivo {filePath}");
                return (T)result;
            }
        }
    }
}