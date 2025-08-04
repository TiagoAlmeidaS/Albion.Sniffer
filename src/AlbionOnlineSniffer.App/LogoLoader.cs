using System.Reflection;

namespace AlbionOnlineSniffer.App;

/// <summary>
/// Classe responsável por carregar a logo do aplicativo
/// </summary>
public static class LogoLoader
{
    private const string LogoResourceName = "AlbionOnlineSniffer.App.Assets.logo_sniffer.png";
    
    /// <summary>
    /// Carrega a logo do aplicativo como stream
    /// </summary>
    /// <returns>Stream da logo ou null se não encontrada</returns>
    public static Stream? LoadLogo()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            return assembly.GetManifestResourceStream(LogoResourceName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao carregar logo: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Carrega a logo como array de bytes
    /// </summary>
    /// <returns>Array de bytes da logo ou null se não encontrada</returns>
    public static byte[]? LoadLogoAsBytes()
    {
        try
        {
            using var stream = LoadLogo();
            if (stream == null) return null;
            
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao carregar logo como bytes: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Verifica se a logo está disponível no executável
    /// </summary>
    /// <returns>True se a logo está disponível</returns>
    public static bool IsLogoAvailable()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceNames = assembly.GetManifestResourceNames();
            return resourceNames.Contains(LogoResourceName);
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Lista todos os recursos embutidos no executável
    /// </summary>
    /// <returns>Lista de nomes dos recursos</returns>
    public static string[] GetEmbeddedResources()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            return assembly.GetManifestResourceNames();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }
} 