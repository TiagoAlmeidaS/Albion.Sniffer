using Microsoft.Extensions.Options;
using System.Text;

namespace AlbionOnlineSniffer.Options;

/// <summary>
/// Validator for SnifferOptions
/// </summary>
public class SnifferOptionsValidator : IValidateOptions<SnifferOptions>
{
    public ValidateOptionsResult Validate(string? name, SnifferOptions options)
    {
        var errors = new List<string>();

        // Validate profiles
        if (options.Profiles == null || options.Profiles.Count == 0)
        {
            errors.Add("At least one profile must be configured");
        }
        else
        {
            // Check for duplicate profile names
            var duplicates = options.Profiles
                .GroupBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            foreach (var duplicate in duplicates)
            {
                errors.Add($"Duplicate profile name found: {duplicate}");
            }

            // Validate active profile exists
            if (!string.IsNullOrEmpty(options.ActiveProfile))
            {
                var activeExists = options.Profiles.Any(p => 
                    p.Name.Equals(options.ActiveProfile, StringComparison.OrdinalIgnoreCase));
                
                if (!activeExists)
                {
                    errors.Add($"Active profile '{options.ActiveProfile}' not found in configured profiles");
                }
            }
        }

        // Validate PacketCapture settings
        if (options.PacketCapture != null)
        {
            if (string.IsNullOrWhiteSpace(options.PacketCapture.Filter))
            {
                errors.Add("PacketCapture.Filter cannot be empty");
            }

            if (!options.PacketCapture.AutoSelectDevice && 
                string.IsNullOrWhiteSpace(options.PacketCapture.DeviceName))
            {
                errors.Add("PacketCapture.DeviceName must be specified when AutoSelectDevice is false");
            }
        }

        // Validate Parsing settings
        if (options.Parsing != null)
        {
            var validProviders = new[] { "FileSystem", "Embedded", "Http" };
            
            if (!validProviders.Contains(options.Parsing.BinDumpProvider, StringComparer.OrdinalIgnoreCase))
            {
                errors.Add($"Invalid BinDumpProvider: {options.Parsing.BinDumpProvider}. Valid values: {string.Join(", ", validProviders)}");
            }

            if (!validProviders.Contains(options.Parsing.ItemMetadataProvider, StringComparer.OrdinalIgnoreCase))
            {
                errors.Add($"Invalid ItemMetadataProvider: {options.Parsing.ItemMetadataProvider}. Valid values: {string.Join(", ", validProviders)}");
            }

            // Validate Http provider settings
            if (options.Parsing.BinDumpProvider?.Equals("Http", StringComparison.OrdinalIgnoreCase) == true &&
                string.IsNullOrWhiteSpace(options.Parsing.RemoteDumpsUrl))
            {
                errors.Add("RemoteDumpsUrl must be specified when using Http BinDumpProvider");
            }

            if (options.Parsing.ItemMetadataProvider?.Equals("Http", StringComparison.OrdinalIgnoreCase) == true &&
                string.IsNullOrWhiteSpace(options.Parsing.RemoteItemsUrl))
            {
                errors.Add("RemoteItemsUrl must be specified when using Http ItemMetadataProvider");
            }
        }

        // Validate Publishing settings
        if (options.Publishing != null)
        {
            var validPublishers = new[] { "RabbitMQ", "Redis", "InMemory" };
            
            if (!validPublishers.Contains(options.Publishing.PublisherType, StringComparer.OrdinalIgnoreCase))
            {
                errors.Add($"Invalid PublisherType: {options.Publishing.PublisherType}. Valid values: {string.Join(", ", validPublishers)}");
            }

            if (string.IsNullOrWhiteSpace(options.Publishing.ConnectionString) &&
                !options.Publishing.PublisherType?.Equals("InMemory", StringComparison.OrdinalIgnoreCase) == true)
            {
                errors.Add("ConnectionString is required for non-InMemory publishers");
            }

            var validFormats = new[] { "MessagePack", "Json" };
            if (!validFormats.Contains(options.Publishing.SerializationFormat, StringComparer.OrdinalIgnoreCase))
            {
                errors.Add($"Invalid SerializationFormat: {options.Publishing.SerializationFormat}. Valid values: {string.Join(", ", validFormats)}");
            }
        }

        if (errors.Any())
        {
            return ValidateOptionsResult.Fail(string.Join("; ", errors));
        }

        return ValidateOptionsResult.Success;
    }
}