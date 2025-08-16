/*
 * AlbionOnlineSniffer - Fluxo Principal
 *
 * 1. Lê configurações de appsettings.json (RabbitMQ, Redis, etc).
 * 2. Instancia publishers de fila (RabbitMQ, Redis) via DependencyProvider do módulo Queue.
 * 3. Instancia todos os handlers e serviços de domínio via DependencyProvider do módulo Core.
 * 4. Conecta EventDispatcher ao Publisher para publicação automática de eventos.
 * 5. Instancia o parser central (Protocol16Deserializer) com PacketProcessor via DI.
 * 6. Instancia o capturador de pacotes via DependencyProvider do módulo Capture e conecta ao parser.
 * 7. Inicia a captura de pacotes. Ao receber pacotes, o fluxo é: Captura -> Parsing -> PacketProcessor -> EventDispatcher -> Publisher.
 * 8. Logging estruturado e tratamento de erros garantem rastreabilidade e robustez.
 *
 * Para adicionar novos eventos, handlers ou publishers, basta expandir os DependencyProviders e conectar os eventos desejados.
 */

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using AlbionOnlineSniffer.Queue;
using AlbionOnlineSniffer.Core;
using AlbionOnlineSniffer.Capture;
using System.Reflection;

namespace AlbionOnlineSniffer.App
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("🚀 Iniciando AlbionOnlineSniffer...");
                Console.WriteLine($"📁 Diretório atual: {Directory.GetCurrentDirectory()}");
                Console.WriteLine($"🔧 Versão do .NET: {Environment.Version}");
                Console.WriteLine($"💻 Arquitetura: {(Environment.Is64BitProcess ? "x64" : "x86")}");
                
                // Verificar assemblies carregados
                Console.WriteLine("📦 Assemblies carregados:");
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Console.WriteLine($"   - {assembly.GetName().Name} ({assembly.GetName().Version})");
                }

                using var loggerFactory = LoggerFactory.Create(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Information);
                });
                var logger = loggerFactory.CreateLogger<Program>();

                try
                {
                    logger.LogInformation("Iniciando AlbionOnlineSniffer...");

                    // 🎨 VALIDAR LOGO DO APLICATIVO
                    logger.LogInformation("🎨 Verificando logo do aplicativo...");

                    // Configuração
                    logger.LogInformation("📋 Carregando configurações...");
                    var configuration = new ConfigurationBuilder()
                        .SetBasePath(AppContext.BaseDirectory)
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .Build();

                    var binDumpsEnabled = configuration.GetValue<bool>("BinDumps:Enabled", true);
                    var binDumpsPath = configuration.GetValue<string>("BinDumps:BasePath", "ao-bin-dumps");
                    
                    logger.LogInformation("Configuração de bin-dumps: Habilitado={Enabled}, Caminho={Path}", 
                        binDumpsEnabled, binDumpsPath);

                    // Publishers
                    logger.LogInformation("📤 Configurando publishers...");
                    var rabbitMqConnectionString = configuration.GetConnectionString("RabbitMQ") ?? 
                        configuration.GetValue<string>("RabbitMQ:ConnectionString") ?? 
                        "amqp://localhost";
                    var exchange = configuration.GetValue<string>("RabbitMQ:Exchange", "albion.sniffer");
                    
                    var publisher = Queue.DependencyProvider.CreateRabbitMqPublisher(rabbitMqConnectionString, exchange);

                    // Configure dependency injection
                    logger.LogInformation("🔧 Configurando injeção de dependências...");
                    var services = new ServiceCollection();
                    
                    // Add logging
                    services.AddLogging(builder => builder.AddConsole());
                    
                    // Register Core services (o Core carrega offsets e indexes via DependencyProvider)
                    logger.LogInformation("🔧 Registrando serviços do Core...");
                    Core.DependencyProvider.RegisterServices(services);
                    
                    // Build service provider
                    logger.LogInformation("🔧 Construindo service provider...");
                    var serviceProvider = services.BuildServiceProvider();
                    
                    // Get services from DI container
                    logger.LogInformation("🔧 Obtendo serviços do container...");
                    var eventDispatcher = serviceProvider.GetRequiredService<Core.Services.EventDispatcher>();
                    var packetOffsets = serviceProvider.GetRequiredService<Core.Models.ResponseObj.PacketOffsets>();
                    var packetIndexes = serviceProvider.GetRequiredService<Core.Models.ResponseObj.PacketIndexes>();

                    // 🔧 VERIFICAR SE OS OFFSETS FORAM CARREGADOS CORRETAMENTE (via Core)
                    logger.LogInformation("🔍 VERIFICANDO OFFSETS CARREGADOS (via Core):");
                    logger.LogInformation("  - Leave: [{Offsets}]", string.Join(", ", packetOffsets.Leave));
                    logger.LogInformation("  - HealthUpdateEvent: [{Offsets}]", string.Join(", ", packetOffsets.HealthUpdateEvent));
                    logger.LogInformation("  - NewCharacter: [{Offsets}]", string.Join(", ", packetOffsets.NewCharacter));
                    logger.LogInformation("  - Move: [{Offsets}]", string.Join(", ", packetOffsets.Move));
                    
                    // 🔧 INTEGRAÇÃO COM MENSAGERIA - Conectar EventDispatcher ao Publisher
                    logger.LogInformation("🔧 Conectando EventDispatcher ao Publisher...");
                    eventDispatcher.RegisterGlobalHandler(async gameEvent =>
                    {
                        try
                        {
                            var eventType = gameEvent.GetType().Name;
                            var timestamp = DateTime.UtcNow;

                            logger.LogInformation("🎯 EVENTO RECEBIDO: {EventType} em {Timestamp}", eventType, timestamp);

                            var eventTypeFormatted = eventType.Replace("Event", "");

                            // Tentar extrair a posição já processada do evento (quando houver)
                            object? location = null;
                            try
                            {
                                // Preferir interface IHasPosition para reutilização entre módulos
                                if (gameEvent is Core.Models.Events.IHasPosition hasPosition)
                                {
                                    var pos = hasPosition.Position;
                                    location = new { X = pos.X, Y = pos.Y };
                                }
                                else
                                {
                                    var posProp = gameEvent.GetType().GetProperty("Position");
                                    if (posProp != null && posProp.PropertyType == typeof(System.Numerics.Vector2))
                                    {
                                        var pos = (System.Numerics.Vector2)posProp.GetValue(gameEvent);
                                        location = new { X = pos.X, Y = pos.Y };
                                    }
                                }
                            }
                            catch
                            {
                                // Ignorar extração de posição caso o evento não tenha ou ocorra falha
                            }

                            var topic = $"albion.event.{eventTypeFormatted.ToLowerInvariant()}";
                            var message = new
                            {
                                EventType = eventType,
                                Timestamp = timestamp,
                                Position = location, // incluir localização quando disponível
                                Data = gameEvent
                            };

                            logger.LogInformation("📤 PUBLICANDO: {EventType} -> {Topic}", eventType, topic);
                            await publisher.PublishAsync(topic, message);
                            logger.LogInformation("✅ Evento publicado na fila: {EventType} -> {Topic}", eventType, topic);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "❌ Erro ao publicar evento na fila: {EventType} - {Message}", gameEvent.GetType().Name, ex.Message);
                        }
                    });

                    logger.LogInformation("✅ EventDispatcher conectado ao sistema de mensageria!");
                    logger.LogInformation("🔧 Configuração de handlers: {HandlerCount} handlers registrados", 
                        eventDispatcher.GetHandlerCount("*"));

                    // Configurar serviços de parsing usando DI
                    logger.LogInformation("🔧 Configurando serviços de parsing...");
                    var definitionLoader = serviceProvider.GetRequiredService<Core.Services.PhotonDefinitionLoader>();
                    
                    // Configurar Albion.Network com handlers
                    logger.LogInformation("🔧 Configurando Albion.Network...");
                    var albionNetworkHandlerManager = serviceProvider.GetRequiredService<Core.Services.AlbionNetworkHandlerManager>();
                    var receiverBuilder = albionNetworkHandlerManager.ConfigureReceiverBuilder();
                    var photonReceiver = receiverBuilder.Build();
                    
                    // Criar Protocol16Deserializer com o receiver configurado
                    logger.LogInformation("🔧 Criando Protocol16Deserializer...");
                    var protocol16Deserializer = new Core.Services.Protocol16Deserializer(
                        photonReceiver, 
                        loggerFactory.CreateLogger<Core.Services.Protocol16Deserializer>()
                    );

                    // Carregar definições dos bin-dumps se habilitado
                    if (binDumpsEnabled)
                    {
                        try
                        {
                            logger.LogInformation("📂 Carregando definições dos bin-dumps...");
                            var fullBinDumpsPath = Path.Combine(Directory.GetCurrentDirectory(), binDumpsPath);
                            if (Directory.Exists(fullBinDumpsPath))
                            {
                                definitionLoader.Load(fullBinDumpsPath);
                                logger.LogInformation("Definições dos bin-dumps carregadas com sucesso");
                            }
                            else
                            {
                                logger.LogWarning("Diretório de bin-dumps não encontrado: {Path}", fullBinDumpsPath);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Erro ao carregar definições dos bin-dumps: {Message}", ex.Message);
                        }
                    }

                    // Configurar captura de pacotes
                    logger.LogInformation("🔧 Configurando captura de pacotes...");
                    var packetCaptureService = Capture.DependencyProvider.CreatePacketCaptureService(5050);

                    // Conectar o capturador ao parser
                    packetCaptureService.OnUdpPayloadCaptured += (packetData) =>
                    {
                        try
                        {
                            protocol16Deserializer.ReceivePacket(packetData);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Erro ao processar pacote: {Message}", ex.Message);
                        }
                    };

                    logger.LogInformation("✅ Captura de pacotes configurada!");

                    // Iniciar captura
                    logger.LogInformation("🚀 Iniciando captura de pacotes...");
                    packetCaptureService.Start();

                    logger.LogInformation("✅ AlbionOnlineSniffer iniciado com sucesso!");
                    logger.LogInformation("📡 Aguardando pacotes do Albion Online...");
                    logger.LogInformation("🛑 Pressione Ctrl+C para parar");

                    // Manter a aplicação rodando
                    Console.CancelKeyPress += (sender, e) =>
                    {
                        logger.LogInformation("🛑 Parando captura...");
                        packetCaptureService.Stop();
                        logger.LogInformation("✅ Captura parada. Saindo...");
                        Environment.Exit(0);
                    };

                    // Aguardar indefinidamente
                    while (true)
                    {
                        Thread.Sleep(1000);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "❌ Erro crítico na aplicação: {Message}", ex.Message);
                    Console.WriteLine($"❌ ERRO CRÍTICO: {ex.Message}");
                    Console.WriteLine($"📋 Stack Trace: {ex.StackTrace}");
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 ERRO FATAL: {ex.Message}");
                Console.WriteLine($"📋 Tipo: {ex.GetType().Name}");
                Console.WriteLine($"📋 Stack Trace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"📋 Inner Exception: {ex.InnerException.Message}");
                    Console.WriteLine($"📋 Inner Stack Trace: {ex.InnerException.StackTrace}");
                }
                
                Environment.Exit(1);
            }
        }
    }
}
