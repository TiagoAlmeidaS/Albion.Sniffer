using AlbionOnlineSniffer.Core.Models.ResponseObj;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AlbionOnlineSniffer.Core.Services
{
    /// <summary>
    /// Provedor estático para PacketOffsets que permite acesso desacoplado
    /// Implementa padrão Service Locator para compatibilidade com framework Albion.Network
    /// </summary>
    public static class PacketOffsetsProvider
    {
        private static IServiceProvider _serviceProvider;
        private static PacketOffsets _cachedOffsets;
        
        /// <summary>
        /// Configura o service provider para resolver PacketOffsets
        /// </summary>
        /// <param name="serviceProvider">Service provider configurado</param>
        public static void Configure(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            // Reset cache to ensure subsequent GetOffsets resolves from the provided container
            _cachedOffsets = null;
        }
        
        /// <summary>
        /// Obtém a instância atual de PacketOffsets
        /// </summary>
        /// <returns>Instância de PacketOffsets configurada</returns>
        public static PacketOffsets GetOffsets()
        {
            if (_cachedOffsets == null)
            {
                if (_serviceProvider == null)
                {
                    throw new InvalidOperationException("PacketOffsetsProvider não foi configurado. Chame Configure() primeiro.");
                }
                try
                {
                    _cachedOffsets = _serviceProvider.GetRequiredService<PacketOffsets>();
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Erro ao obter PacketOffsets do service provider. Certifique-se de que foi registrado corretamente.", ex);
                }
            }
            
            return _cachedOffsets;
        }
        
        /// <summary>
        /// Força a recarga dos offsets do service provider
        /// Útil quando PacketOffsets foi alterado dinamicamente
        /// </summary>
        public static void RefreshOffsets()
        {
            if (_serviceProvider != null)
            {
                _cachedOffsets = _serviceProvider.GetRequiredService<PacketOffsets>();
            }
        }
        
        /// <summary>
        /// Verifica se o provider foi configurado
        /// </summary>
        public static bool IsConfigured => _serviceProvider != null;
    }
}
