using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Core;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Capture;
using AlbionOnlineSniffer.Capture.Services;
using AlbionOnlineSniffer.Capture.Interfaces;
using AlbionOnlineSniffer.Web.Hubs;
using AlbionOnlineSniffer.Web.Services;
using System.Numerics;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information); // Garantir que logs de informação sejam exibidos

builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddRouting();
builder.Services.AddSingleton<EventStreamService>();

// Core services
AlbionOnlineSniffer.Core.DependencyProvider.RegisterServices(builder.Services);

// Capture services (igual ao App)
builder.Services.AddCaptureServices();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapHub<SnifferHub>("/hubs/sniffer");

app.MapGet("/api/metrics", (IPacketCaptureService capture) => 
{
    if (capture is PacketCaptureService packetCapture)
        return Results.Json(packetCapture.Monitor.GetMetrics());
    return Results.NotFound("Monitor não disponível");
});
app.MapGet("/api/events/recent", (EventStreamService stream) => Results.Json(stream.GetRecentEvents()));
app.MapGet("/api/packets/recent", (EventStreamService stream) => Results.Json(stream.GetRecentPackets()));
app.MapPost("/api/capture/start", (IPacketCaptureService capture) => { capture.Start(); return Results.Ok(new { started = true }); });
app.MapPost("/api/capture/stop", (IPacketCaptureService capture) => { capture.Stop(); return Results.Ok(new { started = false }); });

// Wire up sniffer pipeline on startup
using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;
	var loggerFactory = services.GetRequiredService<ILoggerFactory>();
	var logger = loggerFactory.CreateLogger("AlbionOnlineSniffer.Web");
	var eventDispatcher = services.GetRequiredService<EventDispatcher>();
	var definitionLoader = services.GetRequiredService<PhotonDefinitionLoader>();
	var albionNetworkHandlerManager = services.GetRequiredService<AlbionNetworkHandlerManager>();
	var receiverBuilder = albionNetworkHandlerManager.ConfigureReceiverBuilder();
	var photonReceiver = receiverBuilder.Build();
	var protocol16Deserializer = new Protocol16Deserializer(
		photonReceiver,
		loggerFactory.CreateLogger<Protocol16Deserializer>()
	);

	var capture = services.GetRequiredService<IPacketCaptureService>();
	var hubContext = services.GetRequiredService<IHubContext<SnifferHub>>();
	var stream = services.GetRequiredService<EventStreamService>();

	// Log informações de inicialização
	logger.LogInformation("🚀 Iniciando AlbionOnlineSniffer.Web...");
	logger.LogInformation("📁 Diretório atual: {Directory}", System.IO.Directory.GetCurrentDirectory());
	logger.LogInformation("🔧 Versão do .NET: {Version}", Environment.Version);
	logger.LogInformation("💻 Arquitetura: {Architecture}", (Environment.Is64BitProcess ? "x64" : "x86"));

	// Verificar assemblies carregados
	logger.LogInformation("📦 Assemblies carregados:");
	foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
	{
		logger.LogInformation("   - {Name} ({Version})", assembly.GetName().Name, assembly.GetName().Version);
	}

	// Verificar offsets carregados
	var packetOffsets = services.GetRequiredService<AlbionOnlineSniffer.Core.Models.ResponseObj.PacketOffsets>();
	logger.LogInformation("🔍 VERIFICANDO OFFSETS CARREGADOS (via Core):");
	logger.LogInformation("  - Leave: [{Offsets}]", (packetOffsets.Leave != null && packetOffsets.Leave.Length > 0) ? string.Join(", ", packetOffsets.Leave) : "<nulo>");
	logger.LogInformation("  - HealthUpdateEvent: [{Offsets}]", (packetOffsets.HealthUpdateEvent != null && packetOffsets.HealthUpdateEvent.Length > 0) ? string.Join(", ", packetOffsets.HealthUpdateEvent) : "<nulo>");
	logger.LogInformation("  - NewCharacter: [{Offsets}]", (packetOffsets.NewCharacter != null && packetOffsets.NewCharacter.Length > 0) ? string.Join(", ", packetOffsets.NewCharacter) : "<nulo>");
	logger.LogInformation("  - Move: [{Offsets}]", (packetOffsets.Move != null && packetOffsets.Move.Length > 0) ? string.Join(", ", packetOffsets.Move) : "<nulo>");

	// Forward raw UDP payloads to UI and deserializer
	capture.OnUdpPayloadCaptured += payload =>
	{
		try
		{
			// Log detalhado do pacote capturado
			logger.LogInformation("📡 PACOTE UDP CAPTURADO: {Length} bytes", payload?.Length ?? 0);
			if (payload != null && payload.Length > 0)
			{
				logger.LogDebug("📊 PAYLOAD HEX: {Hex}", Convert.ToHexString(payload));
				
				stream.AddRawPacket(payload);
				hubContext.Clients.All.SendAsync("udpPayload", new
				{
					Length = payload.Length,
					Hex = Convert.ToHexString(payload)
				});
				protocol16Deserializer.ReceivePacket(payload);
			}
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "❌ Erro ao processar pacote: {Message}", ex.Message);
		}
	};

	// Broadcast metrics periodically
	if (capture is PacketCaptureService packetCapture)
	{
		packetCapture.Monitor.OnMetricsUpdated += metrics =>
		{
			stream.UpdateMetrics(metrics);
			hubContext.Clients.All.SendAsync("metrics", metrics);
			logger.LogDebug("📊 MÉTRICAS: {Packets} pacotes, {BytesPerSecond} B/s", 
				metrics.ValidPacketsCaptured, metrics.BytesPerSecond);
		};
	}

	// Broadcast parsed game events
	eventDispatcher.RegisterGlobalHandler(async gameEvent =>
	{
		object? location = null;
		try
		{
			if (gameEvent is AlbionOnlineSniffer.Core.Models.Events.IHasPosition hasPosition)
			{
				var pos = hasPosition.Position;
				location = new { X = pos.X, Y = pos.Y };
			}
			else
			{
				var posProp = gameEvent.GetType().GetProperty("Position");
				if (posProp != null && posProp.PropertyType == typeof(Vector2))
				{
					var posValue = posProp.GetValue(gameEvent);
					if (posValue != null)
					{
						var pos = (Vector2)posValue;
						location = new { X = pos.X, Y = pos.Y };
					}
				}
			}
		}
		catch { }

		var eventType = gameEvent.GetType().Name;
		var timestamp = DateTime.UtcNow;

		logger.LogInformation("🎯 EVENTO RECEBIDO: {EventType} em {Timestamp}", eventType, timestamp);
		if (location != null)
		{
			logger.LogInformation("📍 POSIÇÃO: {Position}", location);
		}

		var message = new
		{
			EventType = eventType,
			Timestamp = timestamp,
			Position = location,
			Data = gameEvent
		};

		stream.AddEvent(message);
		await hubContext.Clients.All.SendAsync("gameEvent", message);
	});

	// Optionally load bin-dumps definitions if directory exists (mirrors console app behavior)
	try
	{
		var binDumpsEnabled = builder.Configuration.GetValue<bool>("BinDumps:Enabled", true);
		var binDumpsPath = builder.Configuration.GetValue<string>("BinDumps:BasePath", "ao-bin-dumps");
		var fullBinDumpsPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), binDumpsPath);

		logger.LogInformation("Configuração de bin-dumps: Habilitado={Enabled}, Caminho={Path}", 
			binDumpsEnabled, binDumpsPath);

		if (binDumpsEnabled && System.IO.Directory.Exists(fullBinDumpsPath))
		{
			logger.LogInformation("📂 Carregando definições dos bin-dumps...");
			definitionLoader.Load(fullBinDumpsPath);
			logger.LogInformation("✅ Definições dos bin-dumps carregadas com sucesso");
		}
		else if (binDumpsEnabled)
		{
			logger.LogWarning("⚠️ Diretório de bin-dumps não encontrado: {Path}", fullBinDumpsPath);
		}
	}
	catch (Exception ex)
	{
		logger.LogError(ex, "❌ Erro ao carregar definições dos bin-dumps: {Message}", ex.Message);
	}
}

app.Lifetime.ApplicationStarted.Register(() =>
{
	try
	{
		var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
		var capture = app.Services.GetRequiredService<IPacketCaptureService>();
		logger.LogInformation("🚀 Iniciando captura de pacotes...");
		capture.Start();
		logger.LogInformation("✅ Captura iniciada com sucesso!");
		logger.LogInformation("📡 Aguardando pacotes do Albion Online...");
	}
	catch (Exception ex)
	{
		var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
		logger.LogError(ex, "❌ Falha ao iniciar captura automaticamente. Use /api/capture/start para tentar novamente.");
	}
});

app.Lifetime.ApplicationStopping.Register(() =>
{
	try
	{
		var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
		var capture = app.Services.GetRequiredService<IPacketCaptureService>();
		logger.LogInformation("🛑 Parando captura...");
		capture.Stop();
		logger.LogInformation("✅ Captura parada. Saindo...");
	}
	catch (Exception ex)
	{
		var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
		logger.LogError(ex, "❌ Erro ao parar captura: {Message}", ex.Message);
	}
});

app.Run();