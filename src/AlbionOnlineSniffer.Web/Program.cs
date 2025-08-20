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

// Configuração básica de logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);

// Serviços básicos
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

// Registra serviços básicos
builder.Services.AddSingleton<MetricsService>();
builder.Services.AddSingleton<HealthCheckService>();
builder.Services.AddSingleton<SessionManager>();
builder.Services.AddSingleton<InboundMessageService>();
builder.Services.AddSingleton<EventStreamService>();

// Core services - com tratamento de erro
try
{
    AlbionOnlineSniffer.Core.DependencyProvider.RegisterServices(builder.Services);
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️ Erro ao registrar serviços Core: {ex.Message}");
    // Continua sem os serviços Core se necessário
}

// Options + Profiles
try
{
    builder.Services.AddSnifferOptions(builder.Configuration);
    builder.Services.ValidateOptionsOnStart<AlbionOnlineSniffer.Options.SnifferOptions>();
    builder.Services.AddProfileManagement();
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️ Erro ao configurar opções: {ex.Message}");
}

// Capture services
try
{
    AlbionOnlineSniffer.Capture.DependencyProvider.RegisterServices(builder.Services, builder.Configuration);
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️ Erro ao registrar serviços de captura: {ex.Message}");
}

// Web pipeline
builder.Services.AddSingleton<SnifferWebPipeline>();

// Queue services
try
{
    AlbionOnlineSniffer.Queue.DependencyProvider.AddQueueServices(builder.Services, builder.Configuration);
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️ Erro ao configurar serviços de fila: {ex.Message}");
}

var app = builder.Build();

// Configuração básica
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapHub<SnifferHub>("/hubs/sniffer");

// Health checks básicos
app.MapGet("/healthz", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

// API endpoints básicos
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

// Inicialização simplificada
app.Lifetime.ApplicationStarted.Register(() =>
{
    try
    {
        Console.WriteLine("🚀 AlbionOnlineSniffer.Web iniciado com sucesso!");
        Console.WriteLine($"📁 Diretório atual: {System.IO.Directory.GetCurrentDirectory()}");
        Console.WriteLine($"🔧 Versão do .NET: {Environment.Version}");
        Console.WriteLine($"💻 Arquitetura: {(Environment.Is64BitProcess ? "x64" : "x86")}");
        
        // Tenta inicializar o pipeline de forma segura
        try
        {
            var pipeline = app.Services.GetService<SnifferWebPipeline>();
            if (pipeline != null)
            {
                pipeline.Start();
                Console.WriteLine("✅ Pipeline iniciado com sucesso!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Erro ao iniciar pipeline: {ex.Message}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Erro durante inicialização: {ex.Message}");
    }
});

app.Lifetime.ApplicationStopping.Register(() =>
{
    try
    {
        var pipeline = app.Services.GetService<SnifferWebPipeline>();
        pipeline?.Stop();
        Console.WriteLine("🛑 Aplicação parando...");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Erro ao parar aplicação: {ex.Message}");
    }
});

app.Run();