using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AlbionOnlineSniffer.Core;
using AlbionOnlineSniffer.Options.Extensions;
using AlbionOnlineSniffer.Capture;
using AlbionOnlineSniffer.Capture.Services;
using AlbionOnlineSniffer.Capture.Interfaces;
using AlbionOnlineSniffer.Core.Interfaces;
using AlbionOnlineSniffer.Web.Hubs;
using AlbionOnlineSniffer.Web.Services;
using AlbionOnlineSniffer.Web.Interfaces;
using AlbionOnlineSniffer.Web.Repositories;
using AlbionOnlineSniffer.Web.Models;
using WebLogEntry = AlbionOnlineSniffer.Web.Models.LogEntry;
using System.Numerics;
using AlbionOnlineSniffer.Queue;
using System.IO;
using System.Linq;
using AlbionOnlineSniffer.Queue.Publishers;
using AlbionOnlineSniffer.Core.Pipeline;
using AlbionOnlineSniffer.Core.Observability;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);

builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddRouting();

// Configuração dos repositórios em memória
var maxPackets = builder.Configuration.GetValue<int>("InMemoryRepositories:MaxPackets", 10000);
var maxEvents = builder.Configuration.GetValue<int>("InMemoryRepositories:MaxEvents", 10000);
var maxLogs = builder.Configuration.GetValue<int>("InMemoryRepositories:MaxLogs", 5000);
var maxSessions = builder.Configuration.GetValue<int>("InMemoryRepositories:MaxSessions", 1000);

// Registra repositórios em memória
builder.Services.AddSingleton<IInMemoryRepository<Packet>>(sp => new BoundedInMemoryRepository<Packet>(maxPackets));
builder.Services.AddSingleton<IInMemoryRepository<Event>>(sp => new BoundedInMemoryRepository<Event>(maxEvents));
builder.Services.AddSingleton<IInMemoryRepository<WebLogEntry>>(sp => new BoundedInMemoryRepository<WebLogEntry>(maxLogs));
builder.Services.AddSingleton<IInMemoryRepository<Session>>(sp => new BoundedInMemoryRepository<Session>(maxSessions));

// Registra serviços
builder.Services.AddSingleton<MetricsService>();
builder.Services.AddSingleton<HealthCheckService>();
builder.Services.AddSingleton<SessionManager>();
builder.Services.AddSingleton<InboundMessageService>();
builder.Services.AddSingleton<EventStreamService>();

// Core services
AlbionOnlineSniffer.Core.DependencyProvider.RegisterServices(builder.Services);

// Options + Profiles (igual ao App)
builder.Services.AddSnifferOptions(builder.Configuration);
builder.Services.ValidateOptionsOnStart<AlbionOnlineSniffer.Options.SnifferOptions>();
builder.Services.AddProfileManagement();

// Capture services usando DependencyProvider do módulo Capture (igual ao App)
builder.Services.AddCaptureServices(builder.Configuration);

// Web pipeline via DI
builder.Services.AddSingleton<SnifferWebPipeline>();

// Queue services + Event->Queue Bridge (mesma regra do App)
AlbionOnlineSniffer.Queue.DependencyProvider.AddQueueServices(builder.Services, builder.Configuration);

var app = builder.Build();

// Configura o PacketOffsetsProvider para permitir acesso estático em eventos
AlbionOnlineSniffer.Core.DependencyProvider.ConfigurePacketOffsetsProvider(app.Services);

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapHub<SnifferHub>("/hubs/sniffer");

// Health checks
app.MapGet("/healthz", (HealthCheckService health) => 
{
    var status = health.CheckOverallHealth();
    return Results.Json(status);
});

app.MapGet("/healthz/repositories", (HealthCheckService health) => 
{
    var status = health.CheckOverallHealth();
    var repoCheck = status.Checks.FirstOrDefault(c => c.Name == "Repositories");
    return Results.Json((object?)repoCheck ?? new { error = "Repositories check not found" });
});

app.MapGet("/healthz/memory", (HealthCheckService health) => 
{
    var status = health.CheckOverallHealth();
    var memoryCheck = status.Checks.FirstOrDefault(c => c.Name == "Memory");
    return Results.Json((object?)memoryCheck ?? new { error = "Memory check not found" });
});

app.MapGet("/healthz/performance", (HealthCheckService health) => 
{
    var status = health.CheckOverallHealth();
    var perfCheck = status.Checks.FirstOrDefault(c => c.Name == "Performance");
    return Results.Json((object?)perfCheck ?? new { error = "Performance check not found" });
});

// Métricas
app.MapGet("/metrics", (MetricsService metrics) => 
{
    var prometheusMetrics = metrics.GetPrometheusMetrics();
    return Results.Text(prometheusMetrics, "text/plain");
});

app.MapGet("/api/metrics", (MetricsService metrics) => 
{
    var metricsData = metrics.GetMetrics();
    return Results.Json(metricsData);
});

// Observability (Core) endpoints
app.MapGet("/obs/metrics", (IObservabilityService obs) => 
{
    var text = obs.GetPrometheusMetrics();
    return Results.Text(text, "text/plain");
});

app.MapGet("/obs/metrics/json", (IObservabilityService obs) => 
{
    return Results.Json(obs.GetJsonMetrics());
});

app.MapGet("/obs/healthz", async (IObservabilityService obs) => 
{
    var report = await obs.CheckHealthAsync();
    return Results.Json(report);
});

// API endpoints para dados
app.MapGet("/api/packets", (IInMemoryRepository<Packet> packets, int skip = 0, int take = 100) => 
{
    var data = packets.GetPaged(skip, take);
    var total = packets.Count;
    return Results.Json(new { data, total, skip, take, hasMore = skip + take < total });
});

app.MapGet("/api/events", (IInMemoryRepository<Event> events, int skip = 0, int take = 100) => 
{
    var data = events.GetPaged(skip, take);
    var total = events.Count;
    return Results.Json(new { data, total, skip, take, hasMore = skip + take < total });
});

app.MapGet("/api/logs", (IInMemoryRepository<WebLogEntry> logs, int skip = 0, int take = 100) => 
{
    var data = logs.GetPaged(skip, take);
    var total = logs.Count;
    return Results.Json(new { data, total, skip, take, hasMore = skip + take < total });
});

app.MapGet("/api/sessions", (IInMemoryRepository<Session> sessions, int skip = 0, int take = 100) => 
{
    var data = sessions.GetPaged(skip, take);
    var total = sessions.Count;
    return Results.Json(new { data, total, skip, take, hasMore = skip + take < total });
});

// Endpoints de controle
app.MapPost("/api/repositories/clear", (IInMemoryRepository<Packet> packets, IInMemoryRepository<Event> events, IInMemoryRepository<WebLogEntry> logs, IInMemoryRepository<Session> sessions) => 
{
    packets.Clear();
    events.Clear();
    logs.Clear();
    sessions.Clear();
    return Results.Ok(new { message = "Repositórios limpos com sucesso", timestamp = DateTime.UtcNow });
});

app.MapPost("/api/metrics/reset", (MetricsService metrics) => 
{
    metrics.ResetMetrics();
    return Results.Ok(new { message = "Métricas resetadas com sucesso", timestamp = DateTime.UtcNow });
});

// Endpoints legados para compatibilidade
app.MapGet("/api/events/recent", (EventStreamService stream) => Results.Json(stream.GetRecentEvents()));
app.MapGet("/api/packets/recent", (EventStreamService stream) => Results.Json(stream.GetRecentPackets()));
app.MapPost("/api/capture/start", (IPacketCaptureService capture) => { capture.Start(); return Results.Ok(new { started = true }); });
app.MapPost("/api/capture/stop", (IPacketCaptureService capture) => { capture.Stop(); return Results.Ok(new { started = false }); });

// Startup logs + inicialização do pipeline
using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;
	var loggerFactory = services.GetRequiredService<ILoggerFactory>();
	var logger = loggerFactory.CreateLogger("AlbionOnlineSniffer.Web");

	logger.LogInformation("🚀 Iniciando AlbionOnlineSniffer.Web...");
	logger.LogInformation("📁 Diretório atual: {Directory}", System.IO.Directory.GetCurrentDirectory());
	logger.LogInformation("🔧 Versão do .NET: {Version}", Environment.Version);
	logger.LogInformation("💻 Arquitetura: {Architecture}", (Environment.Is64BitProcess ? "x64" : "x86"));
	logger.LogInformation("📊 Configuração dos repositórios: Packets={MaxPackets}, Events={MaxEvents}, Logs={MaxLogs}, Sessions={MaxSessions}", 
		maxPackets, maxEvents, maxLogs, maxSessions);

	logger.LogInformation("📦 Assemblies carregados:");
	foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
	{
		logger.LogInformation("   - {Name} ({Version})", assembly.GetName().Name, assembly.GetName().Version);
	}



	var packetOffsets = services.GetRequiredService<AlbionOnlineSniffer.Core.Models.ResponseObj.PacketOffsets>();
	logger.LogInformation("🔍 VERIFICANDO OFFSETS CARREGADOS (via Core):");
	logger.LogInformation("  - Leave: [{Offsets}]", (packetOffsets.Leave != null && packetOffsets.Leave.Length > 0) ? string.Join(", ", packetOffsets.Leave) : "<nulo>");
	logger.LogInformation("  - HealthUpdateEvent: [{Offsets}]", (packetOffsets.HealthUpdateEvent != null && packetOffsets.HealthUpdateEvent.Length > 0) ? string.Join(", ", packetOffsets.HealthUpdateEvent) : "<nulo>");
	logger.LogInformation("  - NewCharacter: [{Offsets}]", (packetOffsets.NewCharacter != null && packetOffsets.NewCharacter.Length > 0) ? string.Join(", ", packetOffsets.NewCharacter) : "<nulo>");
	logger.LogInformation("  - Move: [{Offsets}]", (packetOffsets.Move != null && packetOffsets.Move.Length > 0) ? string.Join(", ", packetOffsets.Move) : "<nulo>");

	// 🔧 OBTER EVENTDISPATCHER (igual ao App)
	var eventDispatcher = services.GetRequiredService<AlbionOnlineSniffer.Core.Services.EventDispatcher>();

	// 🔧 INTEGRAÇÃO COM MENSAGERIA - Bridge via DI (igual ao App)
	logger.LogInformation("🔧 Conectando EventDispatcher ao Publisher via Bridge...");
	services.GetRequiredService<AlbionOnlineSniffer.Queue.Publishers.EventToQueueBridge>();
	logger.LogInformation("✅ Bridge Event->Queue registrada!");
	logger.LogInformation("🔧 Configuração de handlers: {HandlerCount} handlers registrados",
		eventDispatcher.GetHandlerCount("*"));

	// 🔧 CONFIGURAR SERVIÇOS DE PARSING (igual ao App)
	logger.LogInformation("🔧 Configurando serviços de parsing...");
	var protocol16Deserializer = services.GetRequiredService<AlbionOnlineSniffer.Core.Services.Protocol16Deserializer>();
	logger.LogInformation("✅ Protocol16Deserializer configurado!");

	// 🔧 CONFIGURAR CAPTURA DE PACOTES (igual ao App)
	logger.LogInformation("🔧 Configurando captura de pacotes...");
	var capturePipeline = services.GetRequiredService<SnifferWebPipeline>();
	logger.LogInformation("✅ Captura de pacotes configurada (pipeline)!");

	// Instancia o pipeline (DI faz o wire)
	services.GetRequiredService<SnifferWebPipeline>();

	// 🔧 INICIALIZAÇÃO DO PIPELINE CORE (igual ao App)
	var corePipeline = services.GetRequiredService<AlbionOnlineSniffer.Core.Pipeline.IEventPipeline>();
	logger.LogInformation("🚀 Pipeline Core obtido: {PipelineType}", corePipeline.GetType().Name);
	
	// Start the pipeline Core
	await corePipeline.StartAsync();
	logger.LogInformation("✅ Pipeline Core iniciado com sucesso!");

	// 🔧 REGISTRAR GLOBAL HANDLER NO EVENTDISPATCHER (igual ao App)
	eventDispatcher.RegisterGlobalHandler(async (eventData) =>
	{
		try
		{
			var eventTypeName = eventData.GetType().Name;
			await corePipeline.EnqueueAsync(eventTypeName, eventData);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Erro ao enfileirar evento no pipeline Core");
		}
	});
	logger.LogInformation("🔗 Pipeline Core conectado ao EventDispatcher");

	// Força a criação do bridge de publicação V1 para escutar eventos imediatamente
	services.GetService<V1ContractPublisherBridge>();

	// Inicializa Observabilidade (Core)
	var obs = services.GetService<IObservabilityService>();
	obs?.InitializeAsync().GetAwaiter().GetResult();
}

app.Lifetime.ApplicationStarted.Register(() =>
{
	try
	{
		var pipeline = app.Services.GetRequiredService<SnifferWebPipeline>();
		pipeline.Start();

		// Inicia o pipeline Core (assíncrono)
		var corePipeline = app.Services.GetService<IEventPipeline>();
		if (corePipeline != null)
		{
			Task.Run(() => corePipeline.StartAsync());
		}

		// Garante que o bridge V1 esteja instanciado
		app.Services.GetService<V1ContractPublisherBridge>();
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
		var pipeline = app.Services.GetRequiredService<SnifferWebPipeline>();
		pipeline.Stop();
	}
	catch (Exception ex)
	{
		var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
		logger.LogError(ex, "❌ Erro ao parar captura: {Message}", ex.Message);
	}
});



app.Run();