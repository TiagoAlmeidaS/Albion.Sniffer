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

// Capture services (igual ao App)
builder.Services.AddCaptureServices();

// Override opcional da porta via configuração (default 5050)
var udpPort = builder.Configuration.GetValue<int?>(
    "Capture:UdpPort")
    ?? builder.Configuration.GetValue<int?>("PacketCaptureSettings:UdpPort")
    ?? 5050;
builder.Services.AddSingleton<IPacketCaptureService>(sp =>
    new PacketCaptureService(udpPort, sp.GetService<IAlbionEventLogger>()));

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

	// Bin-dumps obrigatórios
	var binDumpsEnabled = builder.Configuration.GetValue<bool>("BinDumps:Enabled", true);
	var binDumpsPath = builder.Configuration.GetValue<string>("BinDumps:BasePath", "ao-bin-dumps");
	if (binDumpsEnabled)
	{
		var definitionLoader = services.GetRequiredService<PhotonDefinitionLoader>();
		var resolved = ResolveBinDumpsPath(binDumpsPath);
		definitionLoader.Load(resolved);
		logger.LogInformation("Definições dos bin-dumps carregadas com sucesso (Web) de: {Path}", resolved);
	}

	var packetOffsets = services.GetRequiredService<AlbionOnlineSniffer.Core.Models.ResponseObj.PacketOffsets>();
	logger.LogInformation("🔍 VERIFICANDO OFFSETS CARREGADOS (via Core):");
	logger.LogInformation("  - Leave: [{Offsets}]", (packetOffsets.Leave != null && packetOffsets.Leave.Length > 0) ? string.Join(", ", packetOffsets.Leave) : "<nulo>");
	logger.LogInformation("  - HealthUpdateEvent: [{Offsets}]", (packetOffsets.HealthUpdateEvent != null && packetOffsets.HealthUpdateEvent.Length > 0) ? string.Join(", ", packetOffsets.HealthUpdateEvent) : "<nulo>");
	logger.LogInformation("  - NewCharacter: [{Offsets}]", (packetOffsets.NewCharacter != null && packetOffsets.NewCharacter.Length > 0) ? string.Join(", ", packetOffsets.NewCharacter) : "<nulo>");
	logger.LogInformation("  - Move: [{Offsets}]", (packetOffsets.Move != null && packetOffsets.Move.Length > 0) ? string.Join(", ", packetOffsets.Move) : "<nulo>");

	// Instancia o pipeline (DI faz o wire)
	services.GetRequiredService<SnifferWebPipeline>();

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

// Helpers (resolver bin-dumps)
static string ResolveBinDumpsPath(string configuredPath)
{
    var envPath = Environment.GetEnvironmentVariable("ALBION_BIN_DUMPS_PATH");
    if (!string.IsNullOrWhiteSpace(envPath) && HasDefinitions(envPath))
        return Path.GetFullPath(envPath);

    if (!string.IsNullOrWhiteSpace(configuredPath) && Path.IsPathRooted(configuredPath) && HasDefinitions(configuredPath))
        return Path.GetFullPath(configuredPath);

    var candidates = new List<string>();
    var cwd = Directory.GetCurrentDirectory();
    var baseDir = AppContext.BaseDirectory;
    if (!string.IsNullOrWhiteSpace(configuredPath))
    {
        candidates.Add(Path.Combine(cwd, configuredPath));
        candidates.Add(Path.Combine(baseDir, configuredPath));
        var probe = cwd;
        for (int i = 0; i < 6; i++)
        {
            candidates.Add(Path.Combine(probe, configuredPath));
            probe = Path.GetFullPath(Path.Combine(probe, ".."));
        }
    }

    foreach (var dir in candidates.Distinct())
    {
        if (HasDefinitions(dir))
            return Path.GetFullPath(dir);
    }

    var checkedList = string.Join(Environment.NewLine + " - ", candidates.Distinct());
    throw new DirectoryNotFoundException($"Não foi possível localizar 'events.json' e 'enums.json'. Caminhos testados (BasePath='{configuredPath}'):\n - {checkedList}");
}

static bool HasDefinitions(string? dir)
{
    if (string.IsNullOrWhiteSpace(dir)) return false;
    try
    {
        var eventsPath = Path.Combine(dir, "events.json");
        var enumsPath = Path.Combine(dir, "enums.json");
        return File.Exists(eventsPath) && File.Exists(enumsPath);
    }
    catch { return false; }
}

app.Run();