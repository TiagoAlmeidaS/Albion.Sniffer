using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TestPacketSender
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            // Configurar logging
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });
            var logger = loggerFactory.CreateLogger<Program>();

            logger.LogInformation("=== TestPacketSender - Simulador de Pacotes UDP ===");
            logger.LogInformation("Este programa envia pacotes simulados para a porta 5050");
            logger.LogInformation("Certifique-se de que o AlbionOnlineSniffer está rodando primeiro!");
            logger.LogInformation("");

            try
            {
                Console.WriteLine("Escolha o tipo de teste:");
                Console.WriteLine("1. Enviar pacotes de teste simples");
                Console.WriteLine("2. Enviar eventos simulados do Albion Online");
                Console.WriteLine("3. Enviar pacote de texto customizado");
                Console.WriteLine("4. Modo interativo (enviar pacotes continuamente)");
                Console.Write("Digite sua escolha (1-4): ");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        await SendSimpleTestPackets(logger);
                        break;
                    case "2":
                        await SendAlbionEvents(logger);
                        break;
                    case "3":
                        await SendCustomTextPacket(logger);
                        break;
                    case "4":
                        await InteractiveMode(logger);
                        break;
                    default:
                        logger.LogWarning("Escolha inválida. Executando teste simples...");
                        await SendSimpleTestPackets(logger);
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro durante o teste: {Message}", ex.Message);
            }

            logger.LogInformation("Teste concluído. Pressione qualquer tecla para sair...");
            Console.ReadKey();
        }

        private static async Task SendSimpleTestPackets(ILogger logger)
        {
            logger.LogInformation("Enviando pacotes de teste simples...");
            
            Console.Write("Quantos pacotes enviar? (padrão: 5): ");
            var countInput = Console.ReadLine();
            var count = int.TryParse(countInput, out var parsedCount) ? parsedCount : 5;

            Console.Write("Delay entre pacotes em ms? (padrão: 1000): ");
            var delayInput = Console.ReadLine();
            var delay = int.TryParse(delayInput, out var parsedDelay) ? parsedDelay : 1000;

            await SendMultipleTestPacketsAsync(count, delay, logger);
        }

        private static async Task SendAlbionEvents(ILogger logger)
        {
            logger.LogInformation("Enviando eventos simulados do Albion Online...");

            // Simular evento de novo jogador
            await SendSimulatedEventAsync("NewCharacter", new
            {
                CharacterId = Guid.NewGuid().ToString(),
                Name = "TestPlayer",
                Position = new { X = 100, Y = 200, Z = 0 },
                Guild = "TestGuild"
            }, logger);

            await Task.Delay(500);

            // Simular evento de movimento
            await SendSimulatedEventAsync("Move", new
            {
                CharacterId = Guid.NewGuid().ToString(),
                Position = new { X = 150, Y = 250, Z = 0 },
                Speed = 5.5f
            }, logger);

            await Task.Delay(500);

            // Simular evento de combate
            await SendSimulatedEventAsync("Combat", new
            {
                AttackerId = Guid.NewGuid().ToString(),
                TargetId = Guid.NewGuid().ToString(),
                Damage = 150,
                Skill = "Fireball"
            }, logger);

            logger.LogInformation("Eventos simulados do Albion Online enviados");
        }

        private static async Task SendCustomTextPacket(ILogger logger)
        {
            logger.LogInformation("Enviando pacote de texto customizado...");
            
            Console.Write("Digite a mensagem para enviar: ");
            var message = Console.ReadLine() ?? "Teste customizado";
            
            await SendTextPacketAsync(message, logger);
            logger.LogInformation("Pacote de texto enviado: {Message}", message);
        }

        private static async Task InteractiveMode(ILogger logger)
        {
            logger.LogInformation("Modo interativo ativado. Digite 'quit' para sair.");
            logger.LogInformation("Digite mensagens para enviar como pacotes UDP:");
            
            while (true)
            {
                Console.Write("> ");
                var input = Console.ReadLine();
                
                if (string.IsNullOrEmpty(input) || input.ToLower() == "quit")
                {
                    break;
                }

                try
                {
                    await SendTextPacketAsync(input, logger);
                    logger.LogInformation("Pacote enviado: {Message}", input);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Erro ao enviar pacote: {Message}", ex.Message);
                }
            }
        }

        private static async Task SendPacketAsync(byte[] payload, ILogger logger)
        {
            try
            {
                using var udpClient = new UdpClient();
                var targetEndPoint = new IPEndPoint(IPAddress.Loopback, 5050);
                
                var bytesSent = await udpClient.SendAsync(payload, payload.Length, targetEndPoint);
                logger.LogInformation("Pacote enviado: {BytesSent} bytes para {EndPoint}", bytesSent, targetEndPoint);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao enviar pacote: {Message}", ex.Message);
            }
        }

        private static async Task SendTextPacketAsync(string message, ILogger logger)
        {
            var payload = Encoding.UTF8.GetBytes(message);
            await SendPacketAsync(payload, logger);
        }

        private static async Task SendSimulatedEventAsync(string eventType, object eventData, ILogger logger)
        {
            var eventPacket = new
            {
                EventType = eventType,
                Timestamp = DateTime.UtcNow,
                Data = eventData
            };

            var json = JsonSerializer.Serialize(eventPacket);
            await SendTextPacketAsync(json, logger);
        }

        private static async Task SendMultipleTestPacketsAsync(int count, int delayMs, ILogger logger)
        {
            logger.LogInformation("Enviando {Count} pacotes de teste com delay de {Delay}ms", count, delayMs);

            for (int i = 1; i <= count; i++)
            {
                var testData = new
                {
                    TestId = i,
                    Message = $"Pacote de teste #{i}",
                    Timestamp = DateTime.UtcNow
                };

                await SendSimulatedEventAsync("TestEvent", testData, logger);
                
                if (i < count) // Não aguardar após o último pacote
                {
                    await Task.Delay(delayMs);
                }
            }

            logger.LogInformation("Envio de pacotes de teste concluído");
        }
    }
} 