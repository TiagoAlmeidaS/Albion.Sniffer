using System.Runtime.CompilerServices;

namespace AlbionOnlineSniffer.Tests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        // Configura Verify para snapshots
        VerifyMessagePack.Initialize();
        
        // Configura diretório de snapshots
        Verifier.DerivePathInfo((sourceFile, projectDirectory, type, method) =>
        {
            var directory = Path.Combine(projectDirectory, "Snapshots", type.Name);
            return new PathInfo(directory, type.Name, method.Name);
        });

        // Configurações globais do Verify
        VerifierSettings.AddExtraSettings(settings =>
        {
            settings.DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Include;
            settings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Include;
        });

        // Ignora propriedades que mudam em cada execução
        VerifierSettings.IgnoreMember<Guid>(x => x == default);
        VerifierSettings.IgnoreMember<DateTime>(x => x == default);
        VerifierSettings.IgnoreMember<DateTimeOffset>(x => x == default);
        
        // Configurações para CI
        if (IsRunningInCI())
        {
            VerifierSettings.DisableDiff();
            VerifierSettings.DisableClipboard();
        }
    }

    private static bool IsRunningInCI()
    {
        return Environment.GetEnvironmentVariable("CI") == "true" ||
               Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true" ||
               Environment.GetEnvironmentVariable("TF_BUILD") == "True" ||
               Environment.GetEnvironmentVariable("JENKINS_URL") != null;
    }
}