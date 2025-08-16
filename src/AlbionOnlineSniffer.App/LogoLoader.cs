using System.Reflection;

namespace AlbionOnlineSniffer.App
{
    public class LogoLoader
    {
        public byte[]? LoadLogoAsBytes()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "AlbionOnlineSniffer.App.Assets.logo_sniffer.png";
                
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream != null)
                {
                    using var memoryStream = new MemoryStream();
                    stream.CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }
                
                return null;
            }
            catch (Exception ex)
            {
                // Log silencioso - não é crítico para o funcionamento
                return null;
            }
        }

        public bool IsLogoAvailable()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "AlbionOnlineSniffer.App.Assets.logo_sniffer.png";
                return assembly.GetManifestResourceNames().Contains(resourceName);
            }
            catch
            {
                return false;
            }
        }
    }
} 