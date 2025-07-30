using SharpPcap;
using System;
using System.Linq;
using System.Net;

class Program
{
    static void Main()
    {
        Console.WriteLine("=== Teste de Adaptadores de Rede ===");
        
        try
        {
            var devices = CaptureDeviceList.Instance;
            Console.WriteLine($"Total de dispositivos encontrados: {devices.Count}");
            
            if (devices.Count == 0)
            {
                Console.WriteLine("ERRO: Nenhum dispositivo encontrado!");
                Console.WriteLine("Verifique se o Npcap está instalado e execute como Administrador.");
                return;
            }
            
            Console.WriteLine("\n=== Lista de Dispositivos ===");
            for (int i = 0; i < devices.Count; i++)
            {
                var dev = devices[i];
                Console.WriteLine($"\nDispositivo {i}:");
                Console.WriteLine($"  Nome: {dev.Name}");
                Console.WriteLine($"  Descrição: {dev.Description}");
                
                if (dev is SharpPcap.LibPcap.LibPcapLiveDevice liveDev)
                {
                    Console.WriteLine($"  Tipo: LibPcapLiveDevice");
                    Console.WriteLine($"  Endereços: {liveDev.Addresses?.Count ?? 0}");
                    
                    if (liveDev.Addresses != null)
                    {
                        foreach (var addr in liveDev.Addresses)
                        {
                            Console.WriteLine($"    - IP: {addr.Addr.ipAddress}");
                            Console.WriteLine($"    - Máscara: {addr.Netmask.ipAddress}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"  Tipo: {dev.GetType().Name}");
                }
            }
            
            Console.WriteLine("\n=== Teste de Seleção ===");
            
            // Testar estratégia 1: Dispositivo com endereços IP válidos
            var device1 = devices.FirstOrDefault(d => 
            {
                if (d.Name.Contains("Loopback") || d.Name.Contains("loopback"))
                    return false;
                
                if (d is SharpPcap.LibPcap.LibPcapLiveDevice liveDev)
                {
                    return liveDev.Addresses?.Any(addr => 
                        addr.Addr.ipAddress != null && 
                        !addr.Addr.ipAddress.Equals(IPAddress.Loopback) &&
                        !addr.Addr.ipAddress.Equals(IPAddress.IPv6Loopback)) ?? false;
                }
                return false;
            });
            
            if (device1 != null)
            {
                Console.WriteLine($"Estratégia 1 SUCESSO: {device1.Description}");
            }
            else
            {
                Console.WriteLine("Estratégia 1 FALHOU");
                
                // Testar estratégia 2: Qualquer dispositivo não-loopback
                var device2 = devices.FirstOrDefault(d => 
                    !d.Name.Contains("Loopback") && !d.Name.Contains("loopback"));
                
                if (device2 != null)
                {
                    Console.WriteLine($"Estratégia 2 SUCESSO: {device2.Description}");
                }
                else
                {
                    Console.WriteLine("Estratégia 2 FALHOU");
                    
                    // Testar estratégia 3: Primeiro dispositivo disponível
                    var device3 = devices.FirstOrDefault();
                    if (device3 != null)
                    {
                        Console.WriteLine($"Estratégia 3 SUCESSO: {device3.Description}");
                    }
                    else
                    {
                        Console.WriteLine("Estratégia 3 FALHOU - Nenhum dispositivo disponível");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERRO: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
        
        Console.WriteLine("\nPressione qualquer tecla para sair...");
        Console.ReadKey();
    }
} 